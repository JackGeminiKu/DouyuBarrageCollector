using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Douyu.Events;
using Douyu;

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
            _barrageCollector.ServerMessageRecieved += barrageCollector_ServerMessageRecieved;
        }

        ~BarragePanel()
        {
            _barrageCollector.ChatMessageRecieved -= barrageCollector_ChatMessageRecieved;
            _barrageCollector.GiftMessageRecieved -= barrageCollector_GiftMessageRecieved;
            _barrageCollector.ChouqinMessageRecieved -= barrageCollector_ChouqinMessageRecieved;

            _barrageCollector.ClientMessageSent -= barrageCollector_ClientMessageSent;
            _barrageCollector.ServerMessageRecieved -= barrageCollector_ServerMessageRecieved;
            _barrageCollector.StopCollect();
        }

        #region 弹幕消息

        void barrageCollector_ChatMessageRecieved(object sender, ChatMessageEventArgs e)
        {
            AppendText(txtServerMessage, "[{0}] [弹幕] [{1}]: \t{2}",
                e.ChatMessage.RoomId, e.ChatMessage.UserName, e.ChatMessage.Text);
        }

        void barrageCollector_GiftMessageRecieved(object sender, GiftMessageEventArgs e)
        {
            AppendText(txtServerMessage, "[{0}] [礼物] [{1}]: \t{2}",
                e.GiftMessage.RoomId, e.GiftMessage.UserName, e.GiftMessage.GiftName);
        }

        void barrageCollector_ChouqinMessageRecieved(object sender, ChouqinMessageEventArgs e)
        {
            AppendText(txtServerMessage, "[{0}] [酬勤] [{1}]: \t酬勤{2}",
                e.ChouqinMessage.RoomId, e.ChouqinMessage.UserId, e.ChouqinMessage.Level);
        }

        void barrageCollector_ClientMessageSent(object sender, ClientMessageEventArgs e)
        {
            AppendText(txtClientMessage, "[Client Message]: \t{0}", e.ClientMessage.MessageData);
        }

        void barrageCollector_ServerMessageRecieved(object sender, ServerMessageEventArgs e)
        {
            if (chkShowAllServerMessage.Checked)
                AppendText(txtServerMessage, "[Server Message]: \t{0}", e.ServerMessage.ToString());
        }

        #endregion

        public void StartCollect(int room)
        {
            cboRoomId.Text = room.ToString();
            StartCollect();
        }

        private void btnStartCollect_Click(object sender, EventArgs e)
        {
            StartCollect();
        }

        void StartCollect()
        {
            string RoomId = cboRoomId.Text;
            if (_barrageCollector.IsCollecting(RoomId)) {
                MessageBox.Show(string.Format("房间{0}已经处于收集状态了!", RoomId), "开始收集",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                string password;
                if (PasswordBox.ShowDialog("要强制清除收集状态, 开始收集? 请输入密码!", out password) == DialogResult.Cancel) {
                    return;
                }
                if (password != "52664638") {
                    MessageBox.Show("密码错误");
                    return;
                }
                _barrageCollector.ClearCollectingStatus(RoomId);

                //if (MessageBox.Show("要清除收集状态?", "清除收集状态", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
                //    MessageBoxDefaultButton.Button2) == DialogResult.No) {
                //    return;
                //}
                //_barrageCollector.ClearCollectingStatus(RoomId);
            }

            if (bwBarrageCollector.IsBusy)
                MessageBox.Show("正在收集弹幕, 请先停止收集", "开始收集弹幕");
            btnStartListen.Enabled = false;
            cboRoomId.Enabled = false;
            btnStopListen.Enabled = true;
            bwBarrageCollector.RunWorkerAsync();
        }

        private void bwBarrageCollector_DoWork(object sender, DoWorkEventArgs e)
        {
            _barrageCollector.StartCollect(cboRoomId.GetTextCrossThread());
        }

        private void btnStopCollect_Click(object sender, EventArgs e)
        {
            StopCollect();
        }

        void StopCollect()
        {
            btnStartListen.Enabled = true;
            cboRoomId.Enabled = true;
            btnStopListen.Enabled = false;
            _barrageCollector.StopCollect();
        }

        protected override void OnHandleDestroyed(EventArgs e)
        {
            if (_barrageCollector.IsPlaying) _barrageCollector.StopCollect();
            base.OnHandleDestroyed(e);
        }

        void AppendText(TextBox textBox, string message)
        {
            const int MAX_LINE_COUNT = 1000;
            const int MAX_CHAR_COUNT = 200;

            if (textBox.GetLineCount() > MAX_LINE_COUNT)
                textBox.ClearCrossThread();

            message = DateTime.Now.ToString("[HH:mm:ss] ") + message;
            if (message.Length > MAX_CHAR_COUNT)
                message = message.Substring(0, MAX_CHAR_COUNT) + "...";

            textBox.AppendLineCrossThread(message);
        }

        void AppendText(TextBox textBox, string format, params object[] args)
        {
            AppendText(textBox, string.Format(format, args));
        }

        private void cboRoomId_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
                btnStartListen.PerformClick();
        }

        private void btnSaveRoom_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.SavedRoom = int.Parse(cboRoomId.Text);
            Properties.Settings.Default.Save();
            MessageBox.Show("房间" + cboRoomId.Text + "已经保存完成!", "保存房间", MessageBoxButtons.OK);
        }
    }
}
