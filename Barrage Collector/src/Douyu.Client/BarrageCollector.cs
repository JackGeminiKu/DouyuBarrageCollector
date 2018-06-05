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
using System.Threading;
using System.Data;
using System.Data.SqlClient;
using Dapper;

namespace Douyu.Client
{
    public class BarrageCollector
    {
        bool _stopListen = false;
        Socket _socket;    // socket相关配置

        public BarrageCollector()
        {
            IsPlaying = false;
        }

        public string RoomId { get; private set; }

        public bool IsPlaying { get; private set; }

        static IDbConnection _connection;

        static BarrageCollector()
        {
            _connection = new SqlConnection(Properties.Settings.Default.ConnectionString);
        }

        public static bool IsCollecting(string roomId)
        {
            var rows = _connection.Query(
                @"select IsCollecting from RoomStatus where RoomId = @RoomId",
                new { RoomId = roomId }
            );
            return rows.Count() == 1 && rows.First().IsCollecting == true;
        }

        public static void SetCollecting(string roomId, bool isCollecting)
        {
            var roomStatus = _connection.Query(
                @"select * from RoomStatus where RoomId = @RoomId",
                new { RoomId = roomId }
            );

            if (roomStatus.Count() != 0) {
                var count = _connection.Execute(
                    @"update RoomStatus set IsCollecting = @IsCollecting where RoomId = @RoomId",
                    new { IsCollecting = isCollecting, RoomId = roomId }
                );
                if (count != 1) {
                    LogService.ErrorFormat("SetCollecting 失败: 影响的行数不为1!");
                    return;
                }
            } else {
                var count = _connection.Execute(
                    @"insert into RoomStatus(RoomId, IsCollecting) values(@RoomId, @IsCollecting)",
                    new { RoomId = roomId, IsCollecting = isCollecting }
                );
                if (count != 1) {
                    LogService.ErrorFormat("SetCollecting 失败: 影响的行数不为1!");
                    return;
                }
            }
        }

        public void StartCollect(string roomId)
        {
            if (IsPlaying) {
                MessageBox.Show("正在收集弹幕中, 请先停止!", "开始收集弹幕", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (BarrageCollector.IsCollecting(roomId)) {
                MessageBox.Show(
                    string.Format("收集房间弹幕失败: 房间{0}已经处于收集状态了!", roomId), "开始收集弹幕",
                    MessageBoxButtons.OK, MessageBoxIcon.Error
                );
                return;
            }

            RoomId = roomId;
            LogService.Info("Start collect");
            ConnectServer();
            LoginRoom(roomId);
            JoinGroup(roomId);

            BarrageCollector.SetCollecting(roomId, true);
            IsPlaying = true;
            _stopListen = false;
            var messageText = "";
            var messageItems = new Dictionary<string, string>();
            while (!_stopListen) {
                var swAll = Stopwatch.StartNew();
                try {
                    // 心跳消息
                    KeepLive();

                    // 尝试获取服务器消息
                    if (!TryGetMessage(out messageText)) {
                        MyThread.Wait(100);
                        continue;
                    }

                    // 获取message items
                    if (!TryParseMessage(messageText, out messageItems)) {
                        continue;
                    }

                    // 处理各种消息
                    switch (messageItems["type"].ToLower()) {
                        case "chatmsg":
                            ChatMessage chatMessage = new ChatMessage(messageText);
                            ChatMessage.Save(chatMessage);
                            OnChatMessageRecieved(chatMessage);
                            break;
                        case "dgb":
                            GiftMessage giftMessage = new GiftMessage(messageText);
                            GiftMessage.Save(giftMessage);
                            OnGiftMessageRecieved(giftMessage);
                            break;
                        case "bc_buy_deserve":
                            ChouqinMessage chouqinMessage = new ChouqinMessage(messageText);
                            ChouqinMessage.Save(chouqinMessage);
                            OnChouqinMessageRecieved(chouqinMessage);
                            break;
                        default:
                            OnServerMessageRecieved(new ServerMessage(messageText));
                            break;
                    }
                } catch (Exception ex) {
                    if (ex is SocketException || ex is ObjectDisposedException) {
                        try {
                            LogService.Warn("网络异常, 准备断线重连: " + ex.Message, ex);
                            ReConnect(roomId);  // 尝试断线重连: 有时候服务器会强制关闭连接!!!
                        } catch (Exception ex2) {
                            LogService.Fatal("断线重连失败: " + ex2.Message, ex2);
                        }
                        continue;
                    }
                    LogService.Error("StartCollect Excpetion: " + ex.Message, ex);
                }
            }
            IsPlaying = false;
        }

        public void StopCollect()
        {
            _stopListen = true;
            do {
                MyThread.Wait(1);
            } while (IsPlaying);

            try {
                SendMessage(new LogoutMessage());
                BarrageCollector.SetCollecting(RoomId, false);
            } catch (Exception ex) {
                throw new DouyuException("登出失败: " + ex.Message);
            }
        }

        void ConnectServer()
        {
            const string BARRAGE_SERVER = "openbarrage.douyutv.com"; // 第三方弹幕协议服务器地址
            const int BARRAGE_PORT = 8601; // 第三方弹幕协议服务器端口 

            LogService.InfoFormat("开始连接斗鱼服务器: {0}:{1}", BARRAGE_SERVER, BARRAGE_PORT);
            try {
                var ipEndPoint = new IPEndPoint(Dns.GetHostAddresses(BARRAGE_SERVER)[0], BARRAGE_PORT);
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket.Connect(ipEndPoint);
                if (!_socket.Connected) {
                    LogService.Fatal("连接斗鱼服务器失败: " + ipEndPoint.ToString() + "\r\nNot Connected!");
                    throw new DouyuException("连接斗鱼服务器失败!");
                }
            } catch (Exception e) {
                LogService.Fatal("连接斗鱼服务器失败: " + e.Message, e);
                throw new DouyuException("连接斗鱼服务器失败!");
            }
            LogService.InfoFormat("成功连接斗鱼服务器: {0}:{1}", BARRAGE_SERVER, BARRAGE_PORT);
        }

        void LoginRoom(string RoomId)
        {
            LogService.Info("登录房间: " + RoomId);
            SendMessage(new LoginreqMessage(RoomId));

            // 取消检查响应登录功能, 因为: 
            // 首次连接服务器, 可以收到登录响应. 但是断开之后再连接, 有可能收不到
            var watch = Stopwatch.StartNew();
            do {
                if (_socket.Available > 0) break;
                MyThread.Wait(100);
            } while (watch.ElapsedMilliseconds < 3000);

            var loginres = "";
            if (!TryGetMessage(out loginres) || !loginres.Contains("type@=loginres")) {
                LogService.FatalFormat("服务器没有响应登录信息, 服务器返回信息为: {0}", loginres);
                throw new DouyuException("登录房间失败!");
            }
        }

        void JoinGroup(string RoomId)
        {
            try {
                LogService.Info("加入房间分组");
                SendMessage(new JoinGroupMessage(RoomId));
            } catch (Exception ex) {
                throw new DouyuException("加入房间分组失败!", ex);
            }

        }

        void KeepLive()
        {
            if (_watch.IsRunning == false) {
                _watch.Start();
                return;
            }

            if (_watch.ElapsedMilliseconds > MAX_TIME_KEEP_LIVE) {
                LogService.Debug("发送心跳消息");
                SendMessage(new KeepLiveMessage());
                _watch.Restart();
            }
        }

        const int MAX_TIME_KEEP_LIVE = 40 * 1000;
        const int MAX_BUFFER_LENGTH = 65536;    // 设置字节获取buffer的最大值
        Stopwatch _watch = new Stopwatch();
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

                // 还不能拼出一个消息?
                if (_messageBufer.Count < 4)
                    return false;
                var msgTotalLen = 4 + _messageBufer[0] + _messageBufer[1] * 0x100 + _messageBufer[2] * 0x10000
                    + _messageBufer[3] * 0x1000000;
                if (_messageBufer.Count < msgTotalLen) {
                    return false;
                }

                // 获取字节消息
                var messageBytes = new byte[msgTotalLen];
                _messageBufer.CopyTo(0, messageBytes, 0, msgTotalLen);
                _messageBufer.RemoveRange(0, msgTotalLen);
                LogService.Debug("获得消息字节: " + messageBytes.ToHexString(" "));

                // 转换成字串消息
                messageText = UTF8Encoding.UTF8.GetString(messageBytes, 12, msgTotalLen - 12).Trim('\0');
                LogService.Info("获得消息: " + messageText);
                return true;
            } catch (Exception e) {
                LogService.Error("TryGetMessage Error: " + e.Message, e);
                return false;
            }
        }

        void SendMessage(ClientMessage clientMessage)
        {
            LogService.Info("发送消息: " + clientMessage.ToString());
            var messageBytes = clientMessage.MessgeBytes;
            var count = _socket.Send(messageBytes);
            if (count != messageBytes.Length)
                LogService.Error("发送数据不全: " + clientMessage.ToString());
            OnClientMessageSent(clientMessage);
        }

        void ReConnect(string RoomId)
        {
            LogService.Info("重新连接");
            if (_socket != null) _socket.Close();
            ConnectServer();
            LoginRoom(RoomId);
            JoinGroup(RoomId);
        }

        bool TryParseMessage(string messageText, out Dictionary<string, string> messageItems)
        {
            LogService.Debug("开始解析服务器消息");
            messageItems = new Dictionary<string, string>();
            try {
                foreach (var value in messageText.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries)) {
                    var items = value.Split(new string[] { "@=" }, StringSplitOptions.None);
                    messageItems.Add(ReplaceKeyWord(items[0]), ReplaceKeyWord(items[1]));
                }

                // 发现有些服务器消息没有type项目, 如收到过: pingreq@=loginping/tick@=1516676439963/
                if (!messageItems.ContainsKey("type")) {
                    LogService.Warn("服务器发送的消息没有type项: " + messageText);
                    return false;
                }

                LogService.Debug("成功解析服务器消息!");
                return true;
            } catch (Exception ex) {
                // rid@=122402/sc@=325600/sctn@=0/rid@=-1/type@=qausrespond/
                // roomset@=/catelv1@=/catelv2@=/type@=brafsn/rid@=122402/agc@=121/ftype@=0/rid@=122402/roomset@=/catelv1@=/catelv2@=/
                // 上面的不知道是什么消息, 有重复的key, 暂时忽略!
                if (messageText.Contains("type@=qausrespond") ||
                    messageText.Contains("type@=brafsn") ||
                    messageText.Contains("type@=rri")) {
                    LogService.Warn("解析服务器消息失败!", ex);
                    return false;
                }
                LogService.ErrorFormat("解析服务器消息出错,  服务器消息 = {0}, 出错信息 = {1}", messageText, ex);
                return false;
            }
        }

        string ReplaceKeyWord(string value)
        {
            return value.Replace("@S", "/").Replace("@A", "@");
        }

        #region events

        public event EventHandler<MessageEventArgs<ChatMessage>> ChatMessageRecieved;
        public event EventHandler<MessageEventArgs<GiftMessage>> GiftMessageRecieved;
        public event EventHandler<MessageEventArgs<ChouqinMessage>> ChouqinMessageRecieved;
        public event EventHandler<MessageEventArgs<ClientMessage>> ClientMessageSent;
        public event EventHandler<MessageEventArgs<ServerMessage>> ServerMessageRecieved;
        public event EventHandler<ScoreAddedEventArgs> ScoreAdded;

        protected void OnScoreAdded(ScoreAddedEventArgs args)
        {
            if (ScoreAdded != null)
                ScoreAdded(this, args);
        }

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

        protected void OnServerMessageRecieved(ServerMessage serverMessage)
        {
            if (ServerMessageRecieved != null)
                ServerMessageRecieved(this, new MessageEventArgs<ServerMessage>(serverMessage));
        }

        #endregion
    }
}
