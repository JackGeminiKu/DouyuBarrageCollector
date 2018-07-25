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

namespace Douyu.Client
{
    public class BarrageCollector
    {
        const int KEEP_LIVE_INTERVAL = 30 * 1000;
        bool _stopCollect = false;
        Socket _socket;
        IDbConnection _connection;
        System.Threading.Timer _timer;

        public BarrageCollector()
        {
            _connection = new SqlConnection(Properties.Settings.Default.ConnectionString);
            _timer = new System.Threading.Timer(KeepLive, null, KEEP_LIVE_INTERVAL, KEEP_LIVE_INTERVAL);
        }

        void KeepLive(object obj)
        {
            LogService.Debug("发送心跳消息");
            SendMessage(new KeepLiveMessage());
        }

        public string RoomId { get; set; }

        public bool IsCollecting
        {
            get
            {
                var rows = _connection.Query(
                    "select IsCollecting from RoomStatus where RoomId = @RoomId",
                    new { RoomId = RoomId }
                );
                return rows.Count() == 1 && rows.First().IsCollecting == true;
            }
            private set
            {
                var roomStatus = _connection.Query(
                    "select * from RoomStatus where RoomId = @RoomId",
                    new { RoomId = RoomId }
                );

                if (roomStatus.Count() != 0) {
                    _connection.Execute(
                       "update RoomStatus set IsCollecting = @IsCollecting where RoomId = @RoomId",
                       new { IsCollecting = value, RoomId = RoomId }
                   );
                } else {
                    _connection.Execute(
                       @"insert into RoomStatus(RoomId, IsCollecting) values(@RoomId, @IsCollecting)",
                       new { RoomId = RoomId, IsCollecting = value }
                   );
                }
            }
        }

        public void StartCollect()
        {
            if (IsCollecting && PasswordBox.ShowDialog("房间{0}正在收集中, 如果要取消收集, 请输入密码!", RoomId) != "52664638") {
                Application.Exit();
            }

            ConnectBarrageServer();
            LoginRoom();
            JoinGroup();

            IsCollecting = true;
            _stopCollect = false;
            var messageText = "";
            var messageItems = new Dictionary<string, string>();
            while (!_stopCollect) {
                try {
                    // 尝试获取服务器消息
                    if (!TryGetMessage(out messageText)) {
                        MyThread.Wait(100);
                        continue;
                    }

                    // 处理各种消息
                    var type = messageText.Substring(0, messageText.IndexOf('/'));
                    switch (type) {
                        case "type@=chatmsg":
                            ChatMessage chatMessage = new ChatMessage(messageText);
                            chatMessage.Text += " --- " + _messageBufer.Count();
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
                } catch (Exception ex) {
                    // 尝试断线重连: 有时候服务器会强制关闭连接!!!
                    if (ex is SocketException || ex is ObjectDisposedException) {
                        try {
                            LogService.Warn("网络异常, 准备断线重连: " + ex.Message, ex);
                            ReConnect();
                        } catch (Exception connectionEx) {
                            LogService.Fatal("断线重连失败: " + connectionEx.Message, connectionEx);
                        }
                        continue;
                    }

                    // 记录异常信息
                    LogService.Error("收集弹幕出现异常: " + ex.Message, ex);
                }
            }
            IsCollecting = false;
        }

        public void StopCollect()
        {
            // 结束弹幕收集
            LogService.Info("结束弹幕收集");
            const long STOP_COLLECT_TIMEOUT = 3000;
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
            try {
                LogService.Info("登出");
                SendMessage(new LogoutMessage());
            } catch (Exception ex) {
                throw new DouyuException("登出失败: " + ex.Message, ex);
            }
        }

        void ConnectBarrageServer()
        {
            const string BARRAGE_SERVER = "openbarrage.douyutv.com"; // 第三方弹幕协议服务器地址
            const int BARRAGE_PORT = 8601; // 第三方弹幕协议服务器端口 

            try {
                LogService.Info("连接弹幕服务器");
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket.Connect("222.187.0.83", BARRAGE_PORT);
                if (!_socket.Connected) {
                    LogService.Fatal("连接弹幕服务器失败: 连接不上啊!");
                }
            } catch (Exception ex) {
                LogService.Fatal("连接弹幕服务器失败: " + ex.Message, ex);
                throw new DouyuException("连接弹幕服务器失败: " + ex.Message, ex);
            }
        }

        void LoginRoom()
        {
            LogService.Info("登录房间: " + RoomId);
            SendMessage(new LoginreqMessage(RoomId));

            //// 取消检查响应登录功能, 因为: 
            //// 首次连接服务器, 可以收到登录响应. 但是断开之后再连接, 有可能收不到
            //// 另外有时候服务器会不响应登录信息
            //const long LOGIN_TIMEOUT = 3000;
            //var watch = Stopwatch.StartNew();
            //do {
            //    if (_socket.Available > 0) break;
            //    MyThread.Wait(100);
            //} while (watch.ElapsedMilliseconds < LOGIN_TIMEOUT);

            //var loginres = "";
            //if (!TryGetMessage(out loginres) || !loginres.Contains("type@=loginres")) {
            //    LogService.Fatal("服务器没有响应登录信息!");
            //    throw new DouyuException("服务器没有响应登录信息!");
            //}
        }

        void JoinGroup()
        {
            try {
                LogService.Info("加入房间分组");
                SendMessage(new JoinGroupMessage(RoomId));
            } catch (Exception ex) {
                LogService.Fatal("加入房间分组失败: " + ex.Message, ex);
                throw new DouyuException("加入房间分组失败: " + ex.Message, ex);
            }
        }

        const int MAX_TIME_KEEP_LIVE = 40 * 1000;

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
                Debug.Print("获得消息字节: " + messageBytes.ToHexString(" "));
                LogService.Debug("获得消息字节: " + messageBytes.ToHexString(" "));

                // 转换成字串消息
                messageText = UTF8Encoding.UTF8.GetString(messageBytes, 12, msgTotalLen - 12).Trim('\0');
                Debug.Print("获得消息字串: " + messageText);
                LogService.Info("获得消息字串: " + messageText);
                return true;
            } catch (Exception ex) {
                LogService.Error("尝试获取消息失败!", ex);
                return false;
            }
        }

        void SendMessage(ClientMessage clientMessage)
        {
            try {
                LogService.Info("发送消息: " + clientMessage.ToString());
                var messageBytes = clientMessage.MessgeBytes;
                var count = _socket.Send(messageBytes);
                if (count != messageBytes.Length)
                    LogService.Error("发送数据不全: " + clientMessage.ToString());
                OnClientMessageSent(clientMessage);
            } catch (Exception ex) {
                LogService.Info("send Message Throw Exception", ex);
                ReConnect();
            }
        }

        void ReConnect()
        {
            LogService.Info("重新连接!");
            if (_socket != null) _socket.Close();
            ConnectBarrageServer();
            LoginRoom();
            JoinGroup();
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
