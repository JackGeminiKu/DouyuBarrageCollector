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
using My;
using My.Log;
using System.Configuration;
using System.Diagnostics;
using My.Windows.Forms;

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

        private void frmMain_Shown(object sender, EventArgs e)
        {
            Text += string.Format(" (房间号: {0})", AppSettings.RoomId);
            Text += " v" + MyApplication.Version;
            barragePanel.StartCollect(AppSettings.RoomId);
        }

        private void frmMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            barragePanel.StopCollect();
        }
    }
}
