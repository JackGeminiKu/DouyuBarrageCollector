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
        IDbConnection _connection = new SqlConnection(Properties.Settings.Default.ConnectionString);
        DouyuSocket _douyuSocket;

        public int RoomId { get; set; }

        public bool IsCollecting
        {
            get
            {
                var isCollecting = _connection.ExecuteScalar<bool>(
                    "select IsCollecting from RoomStatus where RoomId = @RoomId",
                    new { RoomId = RoomId }
                );
                return isCollecting;
            }
            private set
            {
                var rowCount = _connection.ExecuteScalar<int>(
                    "select count(*) from RoomStatus where RoomId = @RoomId",
                    new { RoomId = RoomId }
                );
                if (rowCount == 0) {
                    _connection.Execute(
                       @"insert into RoomStatus(RoomId, IsCollecting) values(@RoomId, @IsCollecting)",
                       new { RoomId = RoomId, IsCollecting = value }
                   );
                } else {
                    _connection.Execute(
                       "update RoomStatus set IsCollecting = @IsCollecting where RoomId = @RoomId",
                       new { IsCollecting = value, RoomId = RoomId }
                   );
                }
            }
        }

        public void StartCollect()
        {
            if (IsCollecting &&
                PasswordBox.ShowDialog("房间{0}正在收集中!\n如果要取消收集, 请输入密码!", RoomId) != "123456") {
                Environment.Exit(0);
            }

            ConnectBarrageServer();

            IsCollecting = true;
            _stopCollect = false;
            var messageText = "";
            while (!_stopCollect) {
                try {
                    TryKeepLive();

                    if (_douyuSocket.TryGetMessage(out messageText)) {
                        ProcessMessage(messageText);
                    } else {
                        MyThread.Wait(100);
                    }
                } catch (Exception ex) {
                    LogService.Error("获取&处理消息异常!", ex);
                    if (ex is SocketException || ex is ObjectDisposedException) {
                        try {
                            LogService.Error("开始重新连接!");
                            if (_douyuSocket != null) _douyuSocket.Disconnect();
                            ConnectBarrageServer();
                        } catch (Exception reconnectEx) {
                            LogService.Error("断线重连失败!", reconnectEx);
                            LogService.Error("等待3秒");
                            MyThread.Wait(3000);
                        }
                    }
                }
            }
            IsCollecting = false;
        }

        void ConnectBarrageServer()
        {
            // 获取弹幕服务器消息
            IPEndPoint[] barrageServers;
            int messageGroup;
            DouyuService.GetBarrageServerInfo(RoomId, out barrageServers, out messageGroup);

            // 连接弹幕服务器
            Exception exception = null;
            _douyuSocket = new DouyuSocket();
            _douyuSocket.ClientMessageSent += OnClientMessageSent;
            foreach (var server in barrageServers) {
                try {
                    exception = null;
                    LogService.InfoFormat("连接到弹幕服务器: {0}", server.ToString());
                    _douyuSocket.Connect(server);
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

        void LoginRoom()
        {
            LogService.Info("发送登录房间消息");
            _douyuSocket.SendMessage(new LoginreqMessage(RoomId));

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
            LogService.Info("发送加入房间分组消息");
            _douyuSocket.SendMessage(new JoinGroupMessage(RoomId, messageGroup));
        }

        void TryKeepLive()
        {
            if (_watch == null || _watch.ElapsedMilliseconds > KEEP_LIVE_INTERVAL) {
                LogService.Info("发送心跳消息");
                _douyuSocket.SendMessage(new MrklMessage());
                if (_watch == null)
                    _watch = Stopwatch.StartNew();
                else
                    _watch.Restart();
            }
        }

        Stopwatch _watch;
        const int KEEP_LIVE_INTERVAL = 30 * 1000;

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
            LogService.Info("发送登出消息");
            _douyuSocket.SendMessage(new LogoutMessage());
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

        protected void OnClientMessageSent(object sender, MessageEventArgs<ClientMessage> e)
        {
            if (ClientMessageSent != null)
                ClientMessageSent(this, e);
        }

        #endregion
    }
}
