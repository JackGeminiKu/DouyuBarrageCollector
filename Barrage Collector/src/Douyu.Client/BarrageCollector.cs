using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Douyu.Messsages;
using Jack4net.Log;
using Jack4net;
using System.IO;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.SqlClient;
using Dapper;
using System.Threading;
using System.Web;
using Newtonsoft.Json;

namespace Douyu.Client
{
    public class BarrageCollector
    {
        bool _stopCollect = false;
        Socket _socket;
        IDbConnection _connection;

        public int RoomId { get; set; }

        IDbConnection Connection
        {
            get
            {
                return _connection ?? (_connection = new SqlConnection(Properties.Settings.Default.ConnectionString));
            }
        }

        public bool IsCollecting
        {
            get
            {
                var isCollecting = Connection.ExecuteScalar<bool>(
                    "select IsCollecting from RoomStatus where RoomId = @RoomId",
                    new { RoomId = RoomId }
                );
                return isCollecting;
            }
            private set
            {
                var rowCount = Connection.ExecuteScalar<int>(
                    "select count(*) from RoomStatus where RoomId = @RoomId",
                    new { RoomId = RoomId }
                );
                if (rowCount == 0) {
                    Connection.Execute(
                       @"insert into RoomStatus(RoomId, IsCollecting) values(@RoomId, @IsCollecting)",
                       new { RoomId = RoomId, IsCollecting = value }
                   );
                } else {
                    Connection.Execute(
                       "update RoomStatus set IsCollecting = @IsCollecting where RoomId = @RoomId",
                       new { IsCollecting = value, RoomId = RoomId }
                   );
                }
            }
        }

        void StartKeepLiveTimer()
        {
            if (_timer == null) {
                TimerCallback keepLive = (state) => {
                    LogService.Debug("发送心跳消息");
                    SendMessage(new KeepLiveMessage());
                };
                _timer = new System.Threading.Timer(keepLive, null, KEEP_LIVE_INTERVAL, KEEP_LIVE_INTERVAL);
            }
        }

        System.Threading.Timer _timer;
        const int KEEP_LIVE_INTERVAL = 30 * 1000;

        public void StartCollect()
        {
            if (IsCollecting &&
                PasswordBox.ShowDialog("房间{0}正在收集中!\n如果要取消收集, 请输入密码!", RoomId) != "123456") {
                Environment.Exit(0);
            }

            ConnectBarrageServer();
            StartKeepLiveTimer();

            IsCollecting = true;
            _stopCollect = false;
            var messageText = "";
            while (!_stopCollect) {
                try {
                    if (TryGetMessage(out messageText)) {
                        ProcessMessage(messageText);
                    } else {
                        MyThread.Wait(100);
                    }
                } catch (Exception ex) {
                    LogService.Warn("获取&处理消息异常!", ex);
                    if (ex is SocketException || ex is ObjectDisposedException) {
                        try {
                            LogService.Warn("开始断线重连!");
                            ReConnect();
                        } catch (Exception ex2) {
                            LogService.Fatal("断线重连失败!", ex2);
                            LogService.Info("等待3秒");
                            MyThread.Wait(3000);
                        }
                    }
                }
            }
            IsCollecting = false;
        }

        void ProcessMessage(string messageText)
        {
            var type = messageText.Substring(0, messageText.IndexOf('/'));
            switch (type) {
                case "type@=chatmsg":
                    ChatMessage chatMessage = new ChatMessage(messageText);
                    ChatMessage.Save(chatMessage);
                    OnChatMessageRecieved(chatMessage);
                    break;
                case "type@=dgb":
                    GiftMessage giftMessage = new GiftMessage(messageText);
                    GiftMessage.Save(giftMessage);
                    OnGiftMessageRecieved(giftMessage);
                    break;
                case "type@=bc_buy_deserve":
                    ChouqinMessage chouqinMessage = new ChouqinMessage(messageText);
                    ChouqinMessage.Save(chouqinMessage);
                    OnChouqinMessageRecieved(chouqinMessage);
                    break;
                default:
                    Debug.Print(messageText);
                    break;
            }
        }

        public void StopCollect()
        {
            const long STOP_COLLECT_TIMEOUT = 3000;

            // 结束弹幕收集
            LogService.Info("结束弹幕收集");
            _stopCollect = true;
            var stopwatch = Stopwatch.StartNew();
            do {
                if (!IsCollecting)
                    break;
                MyThread.Wait(100);
            } while (stopwatch.ElapsedMilliseconds < STOP_COLLECT_TIMEOUT);
            if (IsCollecting)
                throw new DouyuException("结束弹幕收集失败: 关闭超时!");

            // 登出
            LogService.Info("登出");
            SendMessage(new LogoutMessage());
        }

        void ConnectBarrageServer()
        {
            // 获取弹幕服务器消息
            IPEndPoint[] barrageServers;
            int messageGroup;
            GetBarrageServerInfo(out barrageServers, out messageGroup);

            // 连接弹幕服务器
            LogService.Info("连接弹幕服务器");
            Exception exception = null;
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            foreach (var server in barrageServers) {
                try {
                    LogService.InfoFormat("连接到弹幕服务器: {0}", server.ToString());
                    exception = null;
                    _socket.Connect(server);
                } catch (Exception ex) {
                    exception = ex;
                }
                if (exception == null)
                    break;
            }
            if (exception != null) {
                throw new DouyuException("连接弹幕服务器失败!", exception);
            }

            // 登录房间&入组
            LoginRoom();
            JoinGroup(messageGroup);
        }

        void GetBarrageServerInfo(out IPEndPoint[] barrageServers, out int groupId)
        {
            try {
                barrageServers = null;
                groupId = 0;

                // 连接斗鱼服务器
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var servers = RoomPage.GetServers(RoomId);
                Exception exception = null;
                foreach (var server in servers) {
                    try {
                        LogService.InfoFormat("连接到斗鱼服务器: {0}", server.ToString());
                        exception = null;
                        _socket.Connect(server);
                    } catch (Exception ex) {
                        exception = ex;
                    }
                    if (exception == null)
                        break;
                }
                if (exception != null)
                    throw new DouyuException("连接斗鱼服务器失败!", exception);

                // 返送登录消息, 获取弹幕服务器&弹幕群组
                SendMessage(new LoginreqMessage(RoomId));
                barrageServers = GetBarrageServers();
                groupId = GetMessageGroup();
            } finally {
                if (_socket != null && _socket.Connected)
                    _socket.Disconnect(false);
            }
        }

        IPEndPoint[] GetBarrageServers()
        {
            // 接收msgiplist消息
            var messageText = "";
            var stopwatch = Stopwatch.StartNew();
            do {
                if (TryGetMessage(out messageText) && messageText.Contains("type@=msgiplist"))
                    break;
                MyThread.Wait(100);
            } while (stopwatch.ElapsedMilliseconds < 20000);
            if (!messageText.Contains("type@=msgiplist"))
                throw new DouyuException("获取msgiplist消息失败!");

            // 解析弹幕服务器
            var regex = new Regex(@"ip@AA=(?<ip>\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3})@ASport@AA=(?<port>\d+)@AS@S");
            var matches = regex.Matches(messageText);
            var barrageServers = new IPEndPoint[matches.Count];
            for (var i = 0; i < matches.Count; ++i) {
                barrageServers[i] = new IPEndPoint(
                    IPAddress.Parse(matches[i].Groups["ip"].Value),
                    int.Parse(matches[i].Groups["port"].Value));
            }
            if (barrageServers.Length == 0)
                throw new DouyuException("没有找到弹幕服务器!");
            return barrageServers;
        }

        int GetMessageGroup()
        {
            // 接收setmsggroup消息
            var messageText = "";
            var stopwatch = Stopwatch.StartNew();
            do {
                if (TryGetMessage(out messageText) && messageText.Contains("type@=setmsggroup"))
                    break;
                MyThread.Wait(100);
            } while (stopwatch.ElapsedMilliseconds < 20000);
            if (!messageText.Contains("type@=setmsggroup"))
                throw new DouyuException("获取setmsggroup消息失败!");

            // 解析弹幕群组编号
            var regex = new Regex(@"gid@=(?<gid>\d+)");
            var match = regex.Match(messageText);
            if (!match.Success) {
                throw new DouyuException("没有找到弹幕群组编号");
            }
            return int.Parse(match.Groups["gid"].Value);
        }

        void LoginRoom()
        {
            LogService.Info("登录房间: " + RoomId);
            SendMessage(new LoginreqMessage());

            // 取消检查响应登录功能, 因为: 
            // 首次连接服务器, 可以收到登录响应. 但是断开之后再连接, 有可能收不到
            // 另外有时候服务器会不响应登录信息
            //const long LOGIN_TIMEOUT = 20000;
            //var loginres = "";
            //var watch = Stopwatch.StartNew();
            //do {
            //    if (_socket.Available > 0 && TryGetMessage(out loginres) && loginres.Contains("type@=loginres")) break;
            //    MyThread.Wait(100);
            //} while (watch.ElapsedMilliseconds < LOGIN_TIMEOUT);

            //if (loginres == null || !loginres.Contains("type@=loginres")) {
            //    LogService.Fatal("服务器没有响应登录信息!");
            //    throw new DouyuException("服务器没有响应登录信息!");
            //}
        }

        void JoinGroup(int messageGroup)
        {
            LogService.Info("加入房间分组");
            SendMessage(new JoinGroupMessage(RoomId, messageGroup));
        }


        const int MAX_BUFFER_LENGTH = 65536;    // 设置字节获取buffer的最大值
        List<byte> _messageBufer = new List<byte>();

        bool TryGetMessage(out string messageText)
        {
            messageText = "";
            try {
                // 如果socket里面有数据, 先收了
                if (_socket.Available > 0) {
                    var buffer = new byte[MAX_BUFFER_LENGTH];
                    var len = _socket.Receive(buffer);
                    for (var i = 0; i < len; i++) {
                        _messageBufer.Add(buffer[i]);
                    }
                }

                // 还不能拼出一个完整的消息?
                if (_messageBufer.Count < 4)
                    return false;
                var msgTotalLen = 4 + _messageBufer[0] + _messageBufer[1] * 0x100 + _messageBufer[2] * 0x10000
                    + _messageBufer[3] * 0x1000000;

                // 有时候会无法解析消息, buff里面有很多数据!
                if (msgTotalLen > 100000) {
                    var dialogResult = MessageBox.Show(
                        string.Format("消息长度 = {0}, \n前12个字节 = {1}, 是否要缓存数据保存到桌面?",
                            msgTotalLen, _messageBufer.Take(12).ToArray().ToHexString()),
                        "Try Get Message Error",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Error
                    );
                    if (dialogResult == DialogResult.Yes) {
                        File.WriteAllText(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
                            "MessageBuffer.txt"), _messageBufer.Take(10000).ToArray().ToHexString());
                    }
                }

                if (_messageBufer.Count < msgTotalLen) {
                    return false;
                }

                // 获取字节消息
                var messageBytes = new byte[msgTotalLen];
                _messageBufer.CopyTo(0, messageBytes, 0, msgTotalLen);
                _messageBufer.RemoveRange(0, msgTotalLen);
                //Debug.Print("获得消息字节: " + messageBytes.ToHexString(" "));
                LogService.Debug("获得消息字节: " + messageBytes.ToHexString(" "));

                // 转换成字串消息
                messageText = UTF8Encoding.UTF8.GetString(messageBytes, 12, msgTotalLen - 12).Trim('\0');
                //Debug.Print("获得消息字串: " + messageText);
                LogService.Info("获得消息字串: " + messageText);
                return true;
            } catch (Exception ex) {
                LogService.Error("尝试获取消息失败!", ex);
                return false;
            }
        }

        void SendMessage(ClientMessage clientMessage)
        {
            var sendOk = false;
            for (var i = 0; i < 3; ++i) {
                try {
                    LogService.Info("发送消息: " + clientMessage.ToString());
                    _socket.Send(clientMessage.MessgeBytes);
                    sendOk = true;
                    OnClientMessageSent(clientMessage);
                } catch (Exception ex) {
                    LogService.Error("发送消息出现异常!", ex);
                    if (ex is SocketException || ex is ObjectDisposedException) {
                        try {
                            LogService.Warn("开始重新连接弹幕服务器!");
                            ReConnect();
                        } catch (Exception ex2) {
                            LogService.Error("重新连接弹幕服务器失败!", ex2);
                        }
                    }
                }

                if (sendOk)
                    break;
                MyThread.Wait(3000);
            }

            if (!sendOk)
                throw new DouyuException("发送消息失败!");
        }

        void ReConnect()
        {
            LogService.Info("重新连接!");
            if (_socket != null) _socket.Close();
            ConnectBarrageServer();
        }

        #region events

        public event EventHandler<MessageEventArgs<ChatMessage>> ChatMessageRecieved;
        public event EventHandler<MessageEventArgs<GiftMessage>> GiftMessageRecieved;
        public event EventHandler<MessageEventArgs<ChouqinMessage>> ChouqinMessageRecieved;
        public event EventHandler<MessageEventArgs<ClientMessage>> ClientMessageSent;

        protected void OnChatMessageRecieved(ChatMessage chatMessage)
        {
            if (ChatMessageRecieved != null)
                ChatMessageRecieved(this, new MessageEventArgs<ChatMessage>(chatMessage));
        }

        protected void OnGiftMessageRecieved(GiftMessage giftMessage)
        {
            if (GiftMessageRecieved != null)
                GiftMessageRecieved(this, new MessageEventArgs<GiftMessage>(giftMessage));
        }

        protected void OnChouqinMessageRecieved(ChouqinMessage message)
        {
            if (ChouqinMessageRecieved != null)
                ChouqinMessageRecieved(this, new MessageEventArgs<ChouqinMessage>(message));
        }

        protected void OnClientMessageSent(ClientMessage clientMessage)
        {
            if (ClientMessageSent != null)
                ClientMessageSent(this, new MessageEventArgs<ClientMessage>(clientMessage));
        }

        #endregion
    }
}
