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
            ShowAppVersion();
        }

        void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            LogService.GetLogger("Error").Error("未处理异常: " + e.Exception.Message, e.Exception);
        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            LogService.GetLogger("Error").Error("未处理异常: " + ex.Message, ex);
        }

        void SetFormLocation()
        {
            Location = new Point(Screen.PrimaryScreen.WorkingArea.Width - Size.Width, 0);
        }

        void ShowAppVersion()
        {
            this.Text += " v" + Assembly.GetExecutingAssembly().GetName().Version;
        }

        private void frmMain_Shown(object sender, EventArgs e)
        {
            barragePanel.StartCollect(Properties.Settings.Default.SavedRoom);
        }
    }
}
