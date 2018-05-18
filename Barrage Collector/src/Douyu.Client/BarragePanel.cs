﻿using System;
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

        void barrageCollector_ChatMessageRecieved(object sender, MessageEventArgs<ChatMessage> e)
        {
            if (chkSimpleMode.Checked) {
                AppendText(txtServerMessage, e.Message.Text);
            } else {
                AppendText(txtServerMessage, "[{0}] [{1}] [弹幕] [{2}]: \t{3}",
                    DateTime.Now.ToString("[HH:mm:ss]"), e.Message.RoomId, e.Message.UserName, e.Message.Text);
            }
        }

        void barrageCollector_GiftMessageRecieved(object sender, MessageEventArgs<GiftMessage> e)
        {
            if (chkSimpleMode.Checked) {
                AppendText(txtServerMessage, e.Message.GiftName);
            } else {
                AppendText(txtServerMessage, "[{0}] [{1}] [礼物] [{2}]: \t{3}",
                    DateTime.Now.ToString("[HH:mm:ss]"), e.Message.RoomId, e.Message.UserName, e.Message.GiftName);
            }
        }

        void barrageCollector_ChouqinMessageRecieved(object sender, MessageEventArgs<ChouqinMessage> e)
        {
            if (chkSimpleMode.Checked) {
                AppendText(txtServerMessage, "酬勤{0}", e.Message.Level);
            } else {
                AppendText(txtServerMessage, "[{0}] [{1}] [酬勤] [{2}]: \t酬勤{3}",
                    DateTime.Now.ToString("[HH:mm:ss]"), e.Message.RoomId, e.Message.UserId, e.Message.Level);
            }
        }

        void barrageCollector_ClientMessageSent(object sender, MessageEventArgs<ClientMessage> e)
        {
            AppendText(txtClientMessage, "[Client Message]: \t{0}", e.Message.ToString());
        }

        void barrageCollector_ServerMessageRecieved(object sender, MessageEventArgs<ServerMessage> e)
        {
            if (chkShowAllServerMessage.Checked)
                AppendText(txtServerMessage, "[Server Message]: \t{0}", e.Message.ToString());
        }

        #endregion

        public void StartCollect(int room)
        {
            cboRoomId.Text = room.ToString();
            StartCollect();
        }

        void btnStartCollect_Click(object sender, EventArgs e)
        {
            StartCollect();
        }

        void StartCollect()
        {
            var RoomId = cboRoomId.Text;
            if (BarrageCollector.IsCollecting(RoomId)) {
                MessageBox.Show(string.Format("房间{0}已经处于收集状态了!", RoomId), "开始收集",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                var password = "";
                if (PasswordBox.ShowDialog("要强制清除收集状态, 开始收集? 请输入密码!", out password) == DialogResult.Cancel) {
                    return;
                }
                if (password != "52664638") {
                    MessageBox.Show("密码错误");
                    return;
                }
                BarrageCollector.ClearCollectingStatus(RoomId);
            }

            if (bwBarrageCollector.IsBusy) {
                MessageBox.Show("正在收集弹幕, 请先停止收集!", "开始收集弹幕", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            btnStartListen.Enabled = false;
            cboRoomId.Enabled = false;
            btnStopListen.Enabled = true;
            bwBarrageCollector.RunWorkerAsync();
        }

        void bwBarrageCollector_DoWork(object sender, DoWorkEventArgs e)
        {
            _barrageCollector.StartCollect(cboRoomId.GetTextCrossThread());
        }

        void btnStopCollect_Click(object sender, EventArgs e)
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

            if (message.Length > MAX_CHAR_COUNT)
                message = message.Substring(0, MAX_CHAR_COUNT) + "...";

            textBox.AppendLineCrossThread(message);
        }

        void AppendText(TextBox textBox, string format, params object[] args)
        {
            AppendText(textBox, string.Format(format, args));
        }

        void cboRoomId_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == '\r')
                btnStartListen.PerformClick();
        }

        void btnSaveRoom_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.SavedRoom = int.Parse(cboRoomId.Text);
            Properties.Settings.Default.Save();
            MessageBox.Show("房间" + cboRoomId.Text + "已经保存完成!", "保存房间", MessageBoxButtons.OK);
        }
    }
}