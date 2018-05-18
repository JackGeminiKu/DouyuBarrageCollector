namespace Douyu.Client
{
    partial class BarragePanel
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPageServerMessage = new System.Windows.Forms.TabPage();
            this.txtServerMessage = new System.Windows.Forms.TextBox();
            this.tabPageClienMessage = new System.Windows.Forms.TabPage();
            this.txtClientMessage = new System.Windows.Forms.TextBox();
            this.tabPageDebug = new System.Windows.Forms.TabPage();
            this.bwBarrageCollector = new System.ComponentModel.BackgroundWorker();
            this.gbControlBoard = new System.Windows.Forms.GroupBox();
            this.chkSimpleMode = new System.Windows.Forms.CheckBox();
            this.btnSaveRoom = new System.Windows.Forms.Button();
            this.chkShowAllServerMessage = new System.Windows.Forms.CheckBox();
            this.cboRoomId = new System.Windows.Forms.ComboBox();
            this.btnStopListen = new System.Windows.Forms.Button();
            this.btnStartListen = new System.Windows.Forms.Button();
            this.lblRoomId = new System.Windows.Forms.Label();
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.chkShowGift = new System.Windows.Forms.CheckBox();
            this.tabControl.SuspendLayout();
            this.tabPageServerMessage.SuspendLayout();
            this.tabPageClienMessage.SuspendLayout();
            this.gbControlBoard.SuspendLayout();
            this.tableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl
            // 
            this.tabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl.Controls.Add(this.tabPageServerMessage);
            this.tabControl.Controls.Add(this.tabPageClienMessage);
            this.tabControl.Controls.Add(this.tabPageDebug);
            this.tabControl.Font = new System.Drawing.Font("YaHei Consolas Hybrid", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tabControl.Location = new System.Drawing.Point(3, 54);
            this.tabControl.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabControl.Multiline = true;
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(640, 221);
            this.tabControl.TabIndex = 3;
            // 
            // tabPageServerMessage
            // 
            this.tabPageServerMessage.Controls.Add(this.txtServerMessage);
            this.tabPageServerMessage.Location = new System.Drawing.Point(4, 26);
            this.tabPageServerMessage.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPageServerMessage.Name = "tabPageServerMessage";
            this.tabPageServerMessage.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPageServerMessage.Size = new System.Drawing.Size(632, 191);
            this.tabPageServerMessage.TabIndex = 1;
            this.tabPageServerMessage.Text = "服务器消息";
            this.tabPageServerMessage.UseVisualStyleBackColor = true;
            // 
            // txtServerMessage
            // 
            this.txtServerMessage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtServerMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtServerMessage.Location = new System.Drawing.Point(3, 4);
            this.txtServerMessage.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtServerMessage.Multiline = true;
            this.txtServerMessage.Name = "txtServerMessage";
            this.txtServerMessage.ReadOnly = true;
            this.txtServerMessage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtServerMessage.Size = new System.Drawing.Size(626, 183);
            this.txtServerMessage.TabIndex = 0;
            this.txtServerMessage.WordWrap = false;
            // 
            // tabPageClienMessage
            // 
            this.tabPageClienMessage.Controls.Add(this.txtClientMessage);
            this.tabPageClienMessage.Location = new System.Drawing.Point(4, 26);
            this.tabPageClienMessage.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPageClienMessage.Name = "tabPageClienMessage";
            this.tabPageClienMessage.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.tabPageClienMessage.Size = new System.Drawing.Size(591, 191);
            this.tabPageClienMessage.TabIndex = 3;
            this.tabPageClienMessage.Text = "客户端消息";
            this.tabPageClienMessage.UseVisualStyleBackColor = true;
            // 
            // txtClientMessage
            // 
            this.txtClientMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtClientMessage.Location = new System.Drawing.Point(3, 4);
            this.txtClientMessage.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtClientMessage.Multiline = true;
            this.txtClientMessage.Name = "txtClientMessage";
            this.txtClientMessage.ReadOnly = true;
            this.txtClientMessage.Size = new System.Drawing.Size(585, 183);
            this.txtClientMessage.TabIndex = 0;
            // 
            // tabPageDebug
            // 
            this.tabPageDebug.Location = new System.Drawing.Point(4, 26);
            this.tabPageDebug.Name = "tabPageDebug";
            this.tabPageDebug.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageDebug.Size = new System.Drawing.Size(591, 191);
            this.tabPageDebug.TabIndex = 6;
            this.tabPageDebug.Text = "调试";
            this.tabPageDebug.UseVisualStyleBackColor = true;
            // 
            // bwBarrageCollector
            // 
            this.bwBarrageCollector.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bwBarrageCollector_DoWork);
            // 
            // gbControlBoard
            // 
            this.gbControlBoard.Controls.Add(this.chkShowGift);
            this.gbControlBoard.Controls.Add(this.chkSimpleMode);
            this.gbControlBoard.Controls.Add(this.btnSaveRoom);
            this.gbControlBoard.Controls.Add(this.chkShowAllServerMessage);
            this.gbControlBoard.Controls.Add(this.cboRoomId);
            this.gbControlBoard.Controls.Add(this.btnStopListen);
            this.gbControlBoard.Controls.Add(this.btnStartListen);
            this.gbControlBoard.Controls.Add(this.lblRoomId);
            this.gbControlBoard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbControlBoard.Location = new System.Drawing.Point(3, 4);
            this.gbControlBoard.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.gbControlBoard.Name = "gbControlBoard";
            this.gbControlBoard.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.gbControlBoard.Size = new System.Drawing.Size(640, 42);
            this.gbControlBoard.TabIndex = 4;
            this.gbControlBoard.TabStop = false;
            this.gbControlBoard.Text = "选择房间";
            // 
            // chkSimpleMode
            // 
            this.chkSimpleMode.AutoSize = true;
            this.chkSimpleMode.Checked = true;
            this.chkSimpleMode.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkSimpleMode.Location = new System.Drawing.Point(362, 22);
            this.chkSimpleMode.Name = "chkSimpleMode";
            this.chkSimpleMode.Size = new System.Drawing.Size(74, 17);
            this.chkSimpleMode.TabIndex = 8;
            this.chkSimpleMode.Text = "简单模式";
            this.chkSimpleMode.UseVisualStyleBackColor = true;
            // 
            // btnSaveRoom
            // 
            this.btnSaveRoom.Location = new System.Drawing.Point(285, 17);
            this.btnSaveRoom.Name = "btnSaveRoom";
            this.btnSaveRoom.Size = new System.Drawing.Size(73, 21);
            this.btnSaveRoom.TabIndex = 7;
            this.btnSaveRoom.Text = "保存房间";
            this.btnSaveRoom.UseVisualStyleBackColor = true;
            this.btnSaveRoom.Click += new System.EventHandler(this.btnSaveRoom_Click);
            // 
            // chkShowAllServerMessage
            // 
            this.chkShowAllServerMessage.AutoSize = true;
            this.chkShowAllServerMessage.Location = new System.Drawing.Point(506, 22);
            this.chkShowAllServerMessage.Name = "chkShowAllServerMessage";
            this.chkShowAllServerMessage.Size = new System.Drawing.Size(134, 17);
            this.chkShowAllServerMessage.TabIndex = 6;
            this.chkShowAllServerMessage.Text = "显示所有服务器信息";
            this.chkShowAllServerMessage.UseVisualStyleBackColor = true;
            // 
            // cboRoomId
            // 
            this.cboRoomId.FormattingEnabled = true;
            this.cboRoomId.Items.AddRange(new object[] {
            "122402",
            "101217",
            "85894",
            "20415"});
            this.cboRoomId.Location = new System.Drawing.Point(60, 17);
            this.cboRoomId.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.cboRoomId.Name = "cboRoomId";
            this.cboRoomId.Size = new System.Drawing.Size(66, 21);
            this.cboRoomId.TabIndex = 4;
            this.cboRoomId.Text = "122402";
            this.cboRoomId.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.cboRoomId_KeyPress);
            // 
            // btnStopListen
            // 
            this.btnStopListen.Enabled = false;
            this.btnStopListen.Location = new System.Drawing.Point(207, 17);
            this.btnStopListen.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnStopListen.Name = "btnStopListen";
            this.btnStopListen.Size = new System.Drawing.Size(73, 21);
            this.btnStopListen.TabIndex = 3;
            this.btnStopListen.Text = "停止收集";
            this.btnStopListen.UseVisualStyleBackColor = true;
            this.btnStopListen.Click += new System.EventHandler(this.btnStopCollect_Click);
            // 
            // btnStartListen
            // 
            this.btnStartListen.Location = new System.Drawing.Point(130, 17);
            this.btnStartListen.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.btnStartListen.Name = "btnStartListen";
            this.btnStartListen.Size = new System.Drawing.Size(73, 21);
            this.btnStartListen.TabIndex = 2;
            this.btnStartListen.Text = "开始收集";
            this.btnStartListen.UseVisualStyleBackColor = true;
            this.btnStartListen.Click += new System.EventHandler(this.btnStartCollect_Click);
            // 
            // lblRoomId
            // 
            this.lblRoomId.AutoSize = true;
            this.lblRoomId.Location = new System.Drawing.Point(9, 22);
            this.lblRoomId.Name = "lblRoomId";
            this.lblRoomId.Size = new System.Drawing.Size(55, 13);
            this.lblRoomId.TabIndex = 0;
            this.lblRoomId.Text = "房间号：";
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.ColumnCount = 1;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.Controls.Add(this.gbControlBoard, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.tabControl, 0, 1);
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 2;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(646, 279);
            this.tableLayoutPanel.TabIndex = 5;
            // 
            // chkShowGift
            // 
            this.chkShowGift.AutoSize = true;
            this.chkShowGift.Location = new System.Drawing.Point(433, 22);
            this.chkShowGift.Name = "chkShowGift";
            this.chkShowGift.Size = new System.Drawing.Size(74, 17);
            this.chkShowGift.TabIndex = 9;
            this.chkShowGift.Text = "显示礼物";
            this.chkShowGift.UseVisualStyleBackColor = true;
            // 
            // BarragePanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel);
            this.Name = "BarragePanel";
            this.Size = new System.Drawing.Size(646, 279);
            this.tabControl.ResumeLayout(false);
            this.tabPageServerMessage.ResumeLayout(false);
            this.tabPageServerMessage.PerformLayout();
            this.tabPageClienMessage.ResumeLayout(false);
            this.tabPageClienMessage.PerformLayout();
            this.gbControlBoard.ResumeLayout(false);
            this.gbControlBoard.PerformLayout();
            this.tableLayoutPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPageClienMessage;
        private System.Windows.Forms.TextBox txtClientMessage;
        private System.Windows.Forms.TabPage tabPageServerMessage;
        private System.Windows.Forms.TextBox txtServerMessage;
        private System.Windows.Forms.TabPage tabPageDebug;
        private System.ComponentModel.BackgroundWorker bwBarrageCollector;
        private System.Windows.Forms.GroupBox gbControlBoard;
        private System.Windows.Forms.ComboBox cboRoomId;
        private System.Windows.Forms.Button btnStopListen;
        private System.Windows.Forms.Button btnStartListen;
        private System.Windows.Forms.Label lblRoomId;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.CheckBox chkShowAllServerMessage;
        private System.Windows.Forms.Button btnSaveRoom;
        private System.Windows.Forms.CheckBox chkSimpleMode;
        private System.Windows.Forms.CheckBox chkShowGift;
    }
}
