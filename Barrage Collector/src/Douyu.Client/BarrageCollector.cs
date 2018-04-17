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
using Douyu.Events;
using Jack4net.Log;
using Jack4net;
using System.IO;
using System.Text.RegularExpressions;

namespace Douyu.Client
{
    /// <summary>
    /// 弹幕客户端类
    /// </summary>
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

        public bool IsCollecting(string RoomId) { return DbService.IsCollecting(RoomId); }

        public void ClearCollectingStatus(string roomId)
        {
            DbService.SetCollecting(roomId, false);
        }

        public void StartCollect(string roomId)
        {
            if (IsPlaying) {
                MessageBox.Show("正在收集弹幕中, 请先停止!", "开始收集弹幕");
                return;
            }

            if (DbService.IsCollecting(roomId)) {
                MessageBox.Show(string.Format("收集房间弹幕失败: 房间{0}已经处于收集状态了!", roomId), "开始收集弹幕");
                return;
            }

            LogService.Default.Info("[Barrage] start collect");
            RoomId = roomId;
            ConnectServer();
            LoginRoom(roomId);
            JoinGroup(roomId);

            DbService.SetCollecting(roomId, true);
            IsPlaying = true;
            _stopListen = false;
            do {
                Stopwatch swAll = Stopwatch.StartNew();

                try {
                    // 心跳消息
                    KeepLive();

                    // 尝试获取服务器消息
                    var messageData = TryGetMessage();
                    if (messageData == "") {
                        Jack4net.Timer.Delay(100);
                        continue;
                    }

                    // 获取message items
                    var parsedValues = GetMessageItems(messageData);
                    if (parsedValues == null) continue;

                    // 发现有些服务器消息没有type项目, 如收到过: pingreq@=loginping/tick@=1516676439963/
                    if (parsedValues.ContainsKey("type") == false) {
                        LogService.GetLogger("Error").Error("服务器发送的消息没有type项: " + messageData);
                        continue;
                    }

                    // 处理消息
                    switch (parsedValues["type"].ToLower()) {
                        case "chatmsg": // 弹幕信息
                            ChatMessage chatMessage = new ChatMessage(messageData);
                            DbService.SaveChatMessage(chatMessage);
                            OnChatMessageRecieved(chatMessage);
                            break;
                        case "dgb": // 礼物
                            GiftMessage giftMessage = new GiftMessage(messageData);
                            DbService.SaveGiftMessage(giftMessage);
                            OnGiftMessageRecieved(giftMessage);
                            break;
                        case "bc_buy_deserve":  // 勤酬                           
                            ChouqinMessage chouqinMessage = new ChouqinMessage(messageData);
                            DbService.SaveChouqinMessage(chouqinMessage);
                            OnChouqinMessageRecieved(chouqinMessage);
                            break;
                        default:
                            LogService.GetLogger("Debug").Debug("未处理的消息: " + messageData);
                            OnServerMessageRecieved(new ServerMessage(messageData));
                            break;
                    }
                } catch (Exception ex) {
                    if (ex is SocketException || ex is ObjectDisposedException) {
                        try {
                            LogService.GetLogger("Error").ErrorFormatted("网络异常, 准备断线重连: " + ex.Message, ex);
                            ReConnect(roomId);  // 尝试断线重连: 有时候服务器会强制关闭连接!!!
                        } catch (Exception ex2) {
                            LogService.GetLogger("Error").Error("ObjectDisposedException, 断线重连失败: " + ex2.Message, ex2);
                        }
                        continue;
                    }
                    LogService.GetLogger("Error").Error("StartCollect Excpetion: " + ex.Message, ex);
                }

            } while (!_stopListen);
            IsPlaying = false;
        }

        public void StopCollect()
        {
            _stopListen = true;
            do {
                Jack4net.Timer.Delay(1);
            } while (IsPlaying);

            try {
                SendMessage(new LogoutMessage());
                DbService.SetCollecting(RoomId, false);
            } catch (Exception ex) {
                throw new DouyuException("登出失败: " + ex.Message);
            }
        }

        void ConnectServer()
        {
            LogService.Default.Info("[Barrage] connect server");
            const string BARRAGE_SERVER = "openbarrage.douyutv.com"; // 第三方弹幕协议服务器地址
            const int BARRAGE_PORT = 8601; // 第三方弹幕协议服务器端口 

            try {
                var ipEndPoint = new IPEndPoint(Dns.GetHostAddresses(BARRAGE_SERVER)[0], BARRAGE_PORT);
                _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _socket.Connect(ipEndPoint);
                if (_socket.Connected == false) {
                    throw new DouyuException("连接斗鱼服务器失败: " + ipEndPoint.ToString() + "\r\nNot Connected!");
                }
            } catch (Exception e) {
                throw new DouyuException("连接斗鱼服务器出错: " + e.Message);
            }
        }

        void LoginRoom(string RoomId)
        {
            LogService.Default.Info("[Barrage] login room " + RoomId);
            SendMessage(new LoginreqMessage(RoomId));

            // 取消检查响应登录功能, 因为: 
            // 首次连接服务器, 可以收到登录响应. 但是断开之后再连接, 有可能收不到
            var watch = Stopwatch.StartNew();
            do {

                if (_socket.Available > 0) break;
                Jack4net.Timer.Delay(100);
            } while (watch.ElapsedMilliseconds < 3 * 1000);

            var loginres = TryGetMessage();
            if (loginres.Contains("type@=loginres") == false) {
                LogService.GetLogger("Error").ErrorFormatted("服务器没有响应登录信息, 服务器返回信息为: {0}", loginres);
            }
        }

        void JoinGroup(string RoomId)
        {
            LogService.Default.Info("[Barrage] join group");
            SendMessage(new JoinGroupMessage(RoomId));
        }

        void KeepLive()
        {
            if (_watch.IsRunning == false) {
                _watch.Start();
                return;
            }

            if (_watch.ElapsedMilliseconds > MAX_TIME_KEEP_LIVE) {
                SendMessage(new KeepLiveMessage());
                _watch.Restart();
            }
        }

        Stopwatch _watch = new Stopwatch();
        const int MAX_TIME_KEEP_LIVE = 40 * 1000;

        string TryGetMessage()
        {
            try {
                // socket里面有数据
                if (_socket.Available > 0) {
                    var buffer = new byte[MAX_BUFFER_LENGTH];
                    var len = _socket.Receive(buffer);

                    for (var i = 0; i < len; i++) {
                        _messageBufer.Add(buffer[i]);
                    }
                }

                // 还不能解析出一个message
                if (_messageBufer.Count < 4)
                    return "";
                var msgLen = _messageBufer[0] + _messageBufer[1] * 0x100 + _messageBufer[2] * 0x10000
                    + _messageBufer[3] * 0x1000000 + 4;
                if (_messageBufer.Count < msgLen) {
                    return "";
                }

                // 获取message字节
                var messageBytes = new byte[msgLen];
                _messageBufer.CopyTo(0, messageBytes, 0, msgLen);

                _messageBufer.RemoveRange(0, msgLen);
                LogService.GetLogger("Message").Debug("[获得数据] " + messageBytes.ToHexString(" "));

                // 转换成字符串
                var message = UTF8Encoding.UTF8.GetString(messageBytes, 12, msgLen - 12).Trim('\0');
                LogService.GetLogger("Message").Debug("[获得消息] " + message);
                return message;
            } catch (Exception e) {
                LogService.GetLogger("Error").Error("TryGetMessage Error: " + e.Message, e);
                return "";
            }
        }

        void SendMessage(ClientMessage clientMessage)
        {
            //LogService.GetLogger("Message").Debug("[发送消息] " + clientMessage.MessageData);
            var messageBytes = clientMessage.MessgeBytes;
            //LogService.GetLogger("Message").Debug("[发送数据] " + messageBytes.ToHexString(" "));
            var len = _socket.Send(messageBytes);
            if (len != messageBytes.Length)
                LogService.GetLogger("Error").Error("发送数据不全: " + clientMessage.ToString());
            OnClientMessageSent(clientMessage);
        }

        void ReConnect(string RoomId)
        {
            LogService.Default.Info("[Barrage] reconnect server");
            if (_socket != null) _socket.Close();
            ConnectServer();
            LoginRoom(RoomId);
            JoinGroup(RoomId);
        }

        Dictionary<string, string> GetMessageItems(string messageString)
        {
            try {
                var messageItems = new Dictionary<string, string>();
                var items = messageString.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var value in items) {
                    var item = value.Split(new string[] { "@=" }, StringSplitOptions.None);
                    messageItems.Add(ReplaceKeyWord(item[0]), ReplaceKeyWord(item[1]));
                }

                return messageItems;
            } catch (Exception ex) {
                // rid@=122402/sc@=325600/sctn@=0/rid@=-1/type@=qausrespond/
                // roomset@=/catelv1@=/catelv2@=/type@=brafsn/rid@=122402/agc@=121/ftype@=0/rid@=122402/roomset@=/catelv1@=/catelv2@=/
                // 上面的不知道是什么消息, 有重复的key, 暂时忽略!
                if (messageString.Contains("type@=qausrespond") == false
                    && messageString.Contains("type@=brafsn") == false
                    && messageString.Contains("type@=rri") == false) {
                    LogService.GetLogger("Error").ErrorFormatted("解析服务器消息出错,  服务器消息 = {0}, 除错信息 = {1}",
                        messageString, ex.ToString());
                }
                return null;
            }
        }

        string ReplaceKeyWord(string value)
        {
            return value.Replace("@S", "/").Replace("@A", "@");
        }

        #region events

        public event EventHandler<ChatMessageEventArgs> ChatMessageRecieved;
        public event EventHandler<GiftMessageEventArgs> GiftMessageRecieved;
        public event EventHandler<ChouqinMessageEventArgs> ChouqinMessageRecieved;

        public event EventHandler<ClientMessageEventArgs> ClientMessageSent;
        public event EventHandler<ServerMessageEventArgs> ServerMessageRecieved;
        public event EventHandler<ScoreAddedEventArgs> ScoreAdded;

        protected void OnScoreAdded(ScoreAddedEventArgs args)
        {
            if (ScoreAdded != null) ScoreAdded(null, args);
        }

        protected void OnChatMessageRecieved(ChatMessage chatMessage)
        {
            if (ChatMessageRecieved != null) ChatMessageRecieved(this, new ChatMessageEventArgs(chatMessage));
        }

        protected void OnGiftMessageRecieved(GiftMessage giftMessage)
        {
            if (GiftMessageRecieved != null) GiftMessageRecieved(this, new GiftMessageEventArgs(giftMessage));
        }

        protected void OnChouqinMessageRecieved(ChouqinMessage message)
        {
            if (ChouqinMessageRecieved != null) ChouqinMessageRecieved(this, new ChouqinMessageEventArgs(message));
        }

        protected void OnClientMessageSent(ClientMessage clientMessage)
        {
            if (ClientMessageSent != null) ClientMessageSent(this, new ClientMessageEventArgs(clientMessage));
        }

        protected void OnServerMessageRecieved(ServerMessage serverMessage)
        {
            if (ServerMessageRecieved != null) ServerMessageRecieved(this, new ServerMessageEventArgs(serverMessage));
        }

        #endregion

        const int MAX_BUFFER_LENGTH = 4096;    // 设置字节获取buffer的最大值
        List<byte> _messageBufer = new List<byte>();
    }
}
