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
            this.bwBarrageCollector = new System.ComponentModel.BackgroundWorker();
            this.gbControlBoard = new System.Windows.Forms.GroupBox();
            this.chkShowGift = new System.Windows.Forms.CheckBox();
            this.chkSimpleMode = new System.Windows.Forms.CheckBox();
            this.btnSaveRoom = new System.Windows.Forms.Button();
            this.cboRoomId = new System.Windows.Forms.ComboBox();
            this.btnStopListen = new System.Windows.Forms.Button();
            this.btnStartListen = new System.Windows.Forms.Button();
            this.lblRoomId = new System.Windows.Forms.Label();
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.txtMessage = new System.Windows.Forms.TextBox();
            this.gbControlBoard.SuspendLayout();
            this.tableLayoutPanel.SuspendLayout();
            this.SuspendLayout();
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
            this.tableLayoutPanel.Controls.Add(this.txtMessage, 0, 1);
            this.tableLayoutPanel.Controls.Add(this.gbControlBoard, 0, 0);
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 2;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 50F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(646, 279);
            this.tableLayoutPanel.TabIndex = 5;
            // 
            // txtMessage
            // 
            this.txtMessage.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtMessage.Location = new System.Drawing.Point(3, 54);
            this.txtMessage.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.txtMessage.Multiline = true;
            this.txtMessage.Name = "txtMessage";
            this.txtMessage.ReadOnly = true;
            this.txtMessage.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtMessage.Size = new System.Drawing.Size(640, 221);
            this.txtMessage.TabIndex = 5;
            this.txtMessage.WordWrap = false;
            // 
            // BarragePanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tableLayoutPanel);
            this.Name = "BarragePanel";
            this.Size = new System.Drawing.Size(646, 279);
            this.gbControlBoard.ResumeLayout(false);
            this.gbControlBoard.PerformLayout();
            this.tableLayoutPanel.ResumeLayout(false);
            this.tableLayoutPanel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.ComponentModel.BackgroundWorker bwBarrageCollector;
        private System.Windows.Forms.GroupBox gbControlBoard;
        private System.Windows.Forms.ComboBox cboRoomId;
        private System.Windows.Forms.Button btnStopListen;
        private System.Windows.Forms.Button btnStartListen;
        private System.Windows.Forms.Label lblRoomId;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.Button btnSaveRoom;
        private System.Windows.Forms.CheckBox chkSimpleMode;
        private System.Windows.Forms.CheckBox chkShowGift;
        private System.Windows.Forms.TextBox txtMessage;
    }
}
