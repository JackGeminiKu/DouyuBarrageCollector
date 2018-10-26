using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using My.Log;
using Douyu.Messsages;
using System.Net.Sockets;
using System.Windows.Forms;
using System.IO;
using System.Net;

namespace Douyu.Client
{
    public class DouyuSocket
    {
        Socket _socket;

        public void Connect(string host, int port)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Connect(host, port);
            Connected = true;
        }

        public void Connect(IPEndPoint server)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.Connect(server);
            Connected = true;
        }

        public void Disconnect()
        {
            _socket.Close();
            Connected = false;
        }

        public bool Connected { get; private set; }

        const int MAX_BUFFER_LENGTH = 65536;
        List<byte> _messageBuffer = new List<byte>();

        public bool TryGetMessage(out string messageText)
        {
            messageText = "";
            try {
                // 如果socket里面有数据, 先收了
                if (_socket.Available > 0) {
                    var buffer = new byte[MAX_BUFFER_LENGTH];
                    var len = _socket.Receive(buffer);
                    for (var i = 0; i < len; i++) {
                        _messageBuffer.Add(buffer[i]);
                    }
                }

                // 还不能拼出一个完整的消息?
                if (_messageBuffer.Count < 4)
                    return false;
                var msgTotalLen = 4 + _messageBuffer[0] + _messageBuffer[1] * 0x100 + _messageBuffer[2] * 0x10000
                    + _messageBuffer[3] * 0x1000000;

                // 有时候会无法解析消息, buff里面有很多数据!
                if (msgTotalLen > 100000) {
                    var dialogResult = MessageBox.Show(
                        string.Format("消息长度 = {0}, \n前12个字节 = {1}, 是否要缓存数据保存到桌面?",
                            msgTotalLen, _messageBuffer.Take(12).ToArray().ToHexString()),
                        "Try Get Message Error",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Error
                    );
                    if (dialogResult == DialogResult.Yes) {
                        File.WriteAllText(
                            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "MessageBuffer.txt"),
                            _messageBuffer.Take(10000).ToArray().ToHexString());
                    }
                }

                if (_messageBuffer.Count < msgTotalLen) {
                    return false;
                }

                // 获取字节消息
                var messageBytes = new byte[msgTotalLen];
                _messageBuffer.CopyTo(0, messageBytes, 0, msgTotalLen);
                _messageBuffer.RemoveRange(0, msgTotalLen);
                //Debug.Print("获得消息字节: " + messageBytes.ToHexString(" "));
                LogService.Debug("Raw data: " + messageBytes.ToHexString(" "));

                // 转换成字串消息
                messageText = UTF8Encoding.UTF8.GetString(messageBytes, 12, msgTotalLen - 12).Trim('\0');
                //Debug.Print("获得消息字串: " + messageText);
                LogService.Debug("获得消息字串: " + messageText + "\n");
                return true;
            } catch (Exception ex) {
                LogService.Error("尝试获取消息失败!", ex);
                return false;
            }
        }

        public void SendMessage(ClientMessage clientMessage)
        {
            LogService.Debug("发送消息: " + clientMessage.ToString());
            _socket.Send(clientMessage.MessgeBytes);
            OnClientMessageSent(clientMessage);
        }

        public event EventHandler<MessageEventArgs<ClientMessage>> ClientMessageSent;

        protected void OnClientMessageSent(ClientMessage clientMessage)
        {
            if (ClientMessageSent != null)
                ClientMessageSent(this, new MessageEventArgs<ClientMessage>(clientMessage));
        }
    }
}
