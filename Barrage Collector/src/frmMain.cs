using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Reflection;
using Douyu.Messsages;
using Jack4net;
using Jack4net.Log;
using System.Configuration;
using System.Diagnostics;

namespace Douyu.Client
{
    public partial class frmMain : Form
    {
        public frmMain()
        {
            InitializeComponent();
        }

        private void frmMain_Load(object sender, EventArgs e)
        {
            Application.ThreadException += Application_ThreadException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            SetFormLocation();
        }

        void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            LogService.Fatal("未处理异常: " + e.Exception.Message, e.Exception);
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            LogService.Fatal("未处理异常: " + ex.Message, ex);
        }

        void SetFormLocation()
        {
            Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - Size.Width, 0);
        }

        const string ROOM_ID = "122402";
        //const string ROOM_ID = "85894";
        //const string ROOM_ID = "67373"; // 陈一发
        //const string ROOM_ID = "71017"; // 冯提莫
        //const string ROOM_ID = "485503"; // 339
        //const string ROOM_ID = "468241"; // 魅力生活
        //const string ROOM_ID = "699689"; // 狐狸

        private void frmMain_Shown(object sender, EventArgs e)
        {
            Text += string.Format(" (房间号: {0})", ROOM_ID);
            barragePanel.StartCollect(ROOM_ID);
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            barragePanel.StopCollect();
        }
    }
}
