﻿using SW9_Project.Logging;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace SW9_Project {
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        [DllImport("Kernel32")]
        public static extern void AllocConsole();

        [DllImport("Kernel32")]
        public static extern void FreeConsole();

        private bool isBuletinBoard = false;


        public MainWindow() {
            InitializeComponent();

            Task.Factory.StartNew(() => {
                AllocConsole();
                Connection.StartService();
            });

            //TODO: Implement at preprossor definition :D - JK
            if (System.Environment.GetCommandLineArgs().Length > 0)
            {
                string[] args = System.Environment.GetCommandLineArgs();
                // Get command line arguments
                foreach (string argument in args)
                {
                    switch (argument)
                    {
                        case "-BB":
                            isBuletinBoard = true;
                            break;

                    }
                }
            }

            if (isBuletinBoard)
            {
                StartBBWindow();
            }
            else
            {
                StartCanvasWindow();
            }
            
        }

        private void StartCanvasWindow() {
            CanvasWindow canvas = new CanvasWindow();
            if (Screen.AllScreens.Length > 1) {
                Screen s2 = Screen.AllScreens[1];
                System.Drawing.Rectangle r2 = s2.WorkingArea;
                canvas.Top = r2.Top;
                canvas.Left = r2.Left;
                canvas.WindowStyle = WindowStyle.None;
                canvas.WindowState = WindowState.Maximized;
                canvas.Topmost = true;
            } else {
                Screen s1 = Screen.AllScreens[0];
                System.Drawing.Rectangle r1 = s1.WorkingArea;
                canvas.Top = r1.Top;
                canvas.Left = r1.Left;
            }
            canvas.Show();
        }

        private void StartBBWindow()
        {
            BulletinBoard canvas = new BulletinBoard();
            if (Screen.AllScreens.Length > 1)
            {
                Screen s2 = Screen.AllScreens[1];
                System.Drawing.Rectangle r2 = s2.WorkingArea;
                canvas.Top = r2.Top;
                canvas.Left = r2.Left;
                canvas.WindowStyle = WindowStyle.None;
                canvas.WindowState = WindowState.Maximized;
                canvas.Topmost = true;
            }
            else {
                Screen s1 = Screen.AllScreens[0];
                System.Drawing.Rectangle r1 = s1.WorkingArea;
                canvas.Top = r1.Top;
                canvas.Left = r1.Left;
            }
            canvas.Show();
        }
    }
}
