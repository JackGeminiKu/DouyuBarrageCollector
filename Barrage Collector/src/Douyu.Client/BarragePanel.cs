using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Douyu;
using Douyu.Messsages;

namespace Douyu.Client
{
    public partial class BarragePanel : UserControl
    {
        BarrageCollector _barrageCollector;

        public BarragePanel()
        {
            InitializeComponent();

            bwBarrageCollector.WorkerSupportsCancellation = true;

            _barrageCollector = new BarrageCollector();
            _barrageCollector.ChatMessageRecieved += barrageCollector_ChatMessageRecieved;
            _barrageCollector.GiftMessageRecieved += barrageCollector_GiftMessageRecieved;
            _barrageCollector.ChouqinMessageRecieved += barrageCollector_ChouqinMessageRecieved;
            _barrageCollector.ClientMessageSent += barrageCollector_ClientMessageSent;
        }

        ~BarragePanel()
        {
            Console.WriteLine("析构函数");
            _barrageCollector.ChatMessageRecieved -= barrageCollector_ChatMessageRecieved;
            _barrageCollector.GiftMessageRecieved -= barrageCollector_GiftMessageRecieved;
            _barrageCollector.ChouqinMessageRecieved -= barrageCollector_ChouqinMessageRecieved;
            _barrageCollector.ClientMessageSent -= barrageCollector_ClientMessageSent;
        }

        //protected override void OnHandleDestroyed(EventArgs e)
        //{
        //    Console.WriteLine("Destroyed");
        //    _barrageCollector.ChatMessageRecieved -= barrageCollector_ChatMessageRecieved;
        //    _barrageCollector.GiftMessageRecieved -= barrageCollector_GiftMessageRecieved;
        //    _barrageCollector.ChouqinMessageRecieved -= barrageCollector_ChouqinMessageRecieved;
        //    _barrageCollector.ClientMessageSent -= barrageCollector_ClientMessageSent;
        //    base.OnHandleDestroyed(e);
        //}

        public int RoomId { get; set; }

        public void StartCollect(int roomId)
        {
            RoomId = roomId;
            StartCollect();
        }

        public void StartCollect()
        {
            if (bwBarrageCollector.IsBusy) {
                MessageBox.Show("正在收集弹幕, 请先停止收集!", "开始收集弹幕", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            bwBarrageCollector.RunWorkerAsync();
        }

        public void StopCollect()
        {
            _barrageCollector.StopCollect();
        }

        void AppendText(TextBox textBox, string message)
        {
            const int MAX_LINE_COUNT = 100;
            const int MAX_CHAR_COUNT = 200;

            if (textBox.GetLineCount() > MAX_LINE_COUNT)
                textBox.ClearCrossThread();

            if (message.Length > MAX_CHAR_COUNT)
                message = message.Substring(0, MAX_CHAR_COUNT) + "...";

            textBox.AppendLineCrossThread(message);
        }

        void AppendText(TextBox textBox, string format, params object[] args)
        {
            AppendText(textBox, string.Format(format, args));
        }

        #region 事件处理

        void barrageCollector_ChatMessageRecieved(object sender, MessageEventArgs<ChatMessage> e)
        {
            AppendText(txtMessage, "[{0}] [{1}]: {2}",
                DateTime.Now.ToString("HH:mm:ss"), e.Message.UserName, e.Message.Text);
        }

        void barrageCollector_GiftMessageRecieved(object sender, MessageEventArgs<GiftMessage> e)
        {
            foreach (var arg in Environment.GetCommandLineArgs()) {
                if (arg == "-NoGiftShow")
                    return;
            }
            AppendText(txtMessage, "[{0}] [{1}]: {2}",
                DateTime.Now.ToString("HH:mm:ss"), e.Message.UserName, e.Message.Gift.Name);
        }

        void barrageCollector_ChouqinMessageRecieved(object sender, MessageEventArgs<ChouqinMessage> e)
        {
            foreach (var arg in Environment.GetCommandLineArgs()) {
                if (arg == "-NoGiftShow")
                    return;
            }
            AppendText(txtMessage, "[{0}] [{1}]: 酬勤{2}",
                DateTime.Now.ToString("HH:mm:ss"), e.Message.UserId, e.Message.Level);
        }

        void barrageCollector_ClientMessageSent(object sender, MessageEventArgs<ClientMessage> e)
        {
            AppendText(txtMessage, "[Client Message]: \t{0}", e.Message.ToString());
        }

        void bwBarrageCollector_DoWork(object sender, DoWorkEventArgs e)
        {
            _barrageCollector.RoomId = RoomId;
            _barrageCollector.StartCollect();
        }

        #endregion
    }
}
