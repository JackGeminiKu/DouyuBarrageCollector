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
        bool _stopCollect = false;
        Socket _socket;

        static IDbConnection _connection;

        static BarrageCollector()
        {
            _connection = new SqlConnection(Properties.Settings.Default.ConnectionString);
        }

        public static bool IsCollectingRoom(string roomId)
        {
            var rows = _connection.Query(
                @"select IsCollecting from RoomStatus where RoomId = @RoomId",
                new { RoomId = roomId }
            );
            return rows.Count() == 1 && rows.First().IsCollecting == true;
        }

        public static void SetCollectingStatus(string roomId, bool isCollecting)
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
                    LogService.ErrorFormat("设置房间收集状态失败!");
                    return;
                }
            } else {
                var count = _connection.Execute(
                    @"insert into RoomStatus(RoomId, IsCollecting) values(@RoomId, @IsCollecting)",
                    new { RoomId = roomId, IsCollecting = isCollecting }
                );
                if (count != 1) {
                    LogService.ErrorFormat("设置房间收集状态失败!");
                    return;
                }
            }
        }

        public BarrageCollector()
        {
            IsCollectiing = false;
        }

        public string RoomId { get; private set; }

        public bool IsCollectiing { get; private set; }

        public void StartCollect(string roomId)
        {
            if (IsCollectiing) {
                MessageBox.Show("当前房间正在收集弹幕中, 请勿重复收集!", "开始收集弹幕", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (BarrageCollector.IsCollectingRoom(roomId)) {
                MessageBox.Show(
                    string.Format("收集房间弹幕失败: 房间{0}已经处于收集状态了!", roomId), "开始收集弹幕",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning
                );
                return;
            }

            RoomId = roomId;
            LogService.Info("开始收集弹幕");
            ConnectBarrageServer();
            LoginRoom(roomId);
            JoinGroup(roomId);

            BarrageCollector.SetCollectingStatus(roomId, true);
            IsCollectiing = true;
            _stopCollect = false;
            var messageText = "";
            var messageItems = new Dictionary<string, string>();
            while (!_stopCollect) {
                var stopwatch = Stopwatch.StartNew();
                try {
                    // 心跳消息
                    KeepLive();

                    // 尝试获取服务器消息
                    if (!TryGetMessage(out messageText)) {
                        MyThread.Wait(100);
                        continue;
                    }

                    //// 获取message items
                    //if (!TryParseMessage(messageText, out messageItems)) {
                    //    continue;
                    //}

                    // 处理各种消息
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
                    LogService.Error("收集弹幕出现异常: " + ex.Message, ex);
                }
            }
            IsCollectiing = false;
        }

        public void StopCollect()
        {
            LogService.Info("结束弹幕收集!");

            // 结束弹幕收集
            const long TIMEOUT = 3000;
            _stopCollect = true;
            var stopwatch = Stopwatch.StartNew();
            do {
                if (!IsCollectiing)
                    break;
                MyThread.Wait(1);
            } while (stopwatch.ElapsedMilliseconds < TIMEOUT);

            if (IsCollectiing)
                throw new DouyuException("结束弹幕收集失败: 关闭超时!");

            // 发送登出消息
            try {
                SendMessage(new LogoutMessage());
            } catch (Exception ex) {
                throw new DouyuException("登出失败: " + ex.Message);
            }

            // 取消收集状态
            BarrageCollector.SetCollectingStatus(RoomId, false);
        }

        void ConnectBarrageServer()
        {
            const string BARRAGE_SERVER = "openbarrage.douyutv.com"; // 第三方弹幕协议服务器地址
            const int BARRAGE_PORT = 8601; // 第三方弹幕协议服务器端口 

            LogService.Info("开始连接弹幕服务器...");
            try {
                var ipEndPoint = new IPEndPoint(Dns.GetHostAddresses(BARRAGE_SERVER)[0], BARRAGE_PORT);
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket.Connect(ipEndPoint);
                if (!_socket.Connected) {
                    LogService.Fatal("连接斗鱼服务器失败: 连接不上啊!");
                    throw new DouyuException("连接斗鱼服务器失败: 连接不上啊!");
                }
            } catch (Exception ex) {
                LogService.Fatal("连接斗鱼服务器失败: " + ex.Message, ex);
                throw new DouyuException("连接斗鱼服务器失败: " + ex.Message, ex);
            }
            LogService.Info("弹幕服务器连接成功!");
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
                LogService.Fatal("服务器没有响应登录信息!");
                throw new DouyuException("服务器没有响应登录信息!");
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

        const int MAX_TIME_KEEP_LIVE = 40 * 1000;

        void KeepLive()
        {
            if (_watch.IsRunning == false) {
                _watch.Start();
                return;
            }

            if (_watch.ElapsedMilliseconds < MAX_TIME_KEEP_LIVE) {
                return;
            }

            LogService.Debug("发送心跳消息");
            SendMessage(new KeepLiveMessage());
            _watch.Restart();
        }

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

                // 还不能拼出一个完整的消息?
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
            } catch (Exception ex) {
                LogService.Error("尝试获取消息失败!", ex);
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
            LogService.Info("重新连接!");
            if (_socket != null) _socket.Close();
            ConnectBarrageServer();
            LoginRoom(RoomId);
            JoinGroup(RoomId);
        }

        //bool TryParseMessage(string messageText, out Dictionary<string, string> messageItems)
        //{
        //    LogService.Debug("开始解析服务器消息");
        //    messageItems = new Dictionary<string, string>();
        //    try {
        //        foreach (var item in messageText.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries)) {
        //            int separatorStart = item.IndexOf("@=", StringComparison.Ordinal);
        //            string key = item.Substring(0, separatorStart);
        //            var value = item.Substring(separatorStart + 2);
        //            messageItems.Add(ReplaceKeyWord(key), ReplaceKeyWord(value));
        //        }

        //        // 发现有些服务器消息没有type项目, 如收到过: pingreq@=loginping/tick@=1516676439963/
        //        if (!messageItems.ContainsKey("type")) {
        //            LogService.Warn("服务器发送的消息没有type项: " + messageText);
        //            return false;
        //        }

        //        LogService.Debug("成功解析服务器消息!");
        //        return true;
        //    } catch (Exception ex) {
        //        // rid@=122402/sc@=325600/sctn@=0/rid@=-1/type@=qausrespond/
        //        // roomset@=/catelv1@=/catelv2@=/type@=brafsn/rid@=122402/agc@=121/ftype@=0/rid@=122402/roomset@=/catelv1@=/catelv2@=/
        //        // 上面的不知道是什么消息, 有重复的key, 暂时忽略!
        //        if (messageText.Contains("type@=qausrespond") ||
        //            messageText.Contains("type@=brafsn") ||
        //            messageText.Contains("type@=rri")) {
        //            LogService.Warn("解析服务器消息失败!", ex);
        //            return false;
        //        }
        //        LogService.ErrorFormat("解析服务器消息出错,  服务器消息 = {0}, 出错信息 = {1}", messageText, ex);
        //        return false;
        //    }
        //}

        string ReplaceKeyWord(string value)
        {
            return value.Replace("@S", "/").Replace("@A", "@");
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
