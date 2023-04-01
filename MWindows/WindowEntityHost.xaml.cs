using MWindows.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MWindows
{
    /// <summary>
    /// WindowEntityHost.xaml 的交互逻辑
    /// </summary>
    public partial class WindowEntityHost : UserControl
    {
        private Point LastMouseSelectPos = new Point();
        private WindowEntity SelectedWindow { get; set; } = null;
        private Task MouseTask = null;
        private Brush ButtonDefaultBackground = null;
        private Brush ButtonDefaultForeground = null;
        public WindowEntityHost()
        {
            InitializeComponent();
            ButtonDefaultBackground = BT_DropSelectWindow.Background;
            ButtonDefaultForeground = BT_DropSelectWindow.Foreground;
        }

        private void StartSelectWidnow()
        {
            if (MouseTask != null) return;
            OnDropWindowStart();
            MouseTask = Task.Run(() =>
            {
                while (true)
                {
                    bool isBreak = false;
                    Dispatcher.Invoke(() =>
                    {
                        isBreak = Mouse.LeftButton == MouseButtonState.Released;
                    });
                    if (WinApi.User32.User32Methods.GetCursorPos(out var pt))
                    {
                        if (LastMouseSelectPos.X != pt.X || LastMouseSelectPos.Y != pt.Y)
                        {
                            LastMouseSelectPos = new Point(pt.X, pt.Y);
                            var wptr = WinApi.User32.User32Methods.WindowFromPoint(pt);
                            if (wptr != IntPtr.Zero && WinApi.User32.User32Methods.IsWindow(wptr))
                            {
                                if (SelectedWindow != null && wptr == SelectedWindow.Hwnd)
                                {
                                    //如果所选窗体与现在的句柄相同
                                    Dispatcher.Invoke(() =>
                                    {
                                        OnDropWindowUpdate();
                                    });
                                }
                                else
                                {
                                    Dispatcher.Invoke(() =>
                                    {
                                        Success(wptr);
                                    });
                                }
                            }
                            else
                            {
                                Fail();
                            }
                        }
                    }
                    if (isBreak)
                    {
                        MouseTask = null;
                        Dispatcher.Invoke(() =>
                        {
                            OnDropWindowEnd();
                        });
                        break;
                    }
                }
                Thread.Sleep(400);
            });
            void Success(IntPtr ptr)
            {
                Selected(new WindowEntity(ptr));
            }
            void Fail()
            {
                MessageBox.Show("选择失败");
            }
        }
        private void Selected(WindowEntity window)
        {
            this.SelectedWindow = window;
            OnDropWindowUpdate();
        }
        //开始选择窗体
        private void OnDropWindowStart()
        {
            Cursor = Cursors.Cross;
            BT_DropSelectWindow.Background = Brushes.DodgerBlue;
            BT_DropSelectWindow.Foreground = Brushes.White;
        }
        //正在选择窗体
        private void OnDropWindowUpdate()
        {
            TEXT_DropSelectPos.Text = LastMouseSelectPos.X + "," + LastMouseSelectPos.Y;
            TEXT_DropSelectWindowTitle.Text = SelectedWindow.Title + "(" + SelectedWindow.ClassName + ")";
            TEXT_DropSelectHandle.Text = SelectedWindow.Hwnd.ToString();
        }
        //窗体选择完成
        private void OnDropWindowEnd()
        {
            Cursor = Cursors.Arrow;
            BT_DropSelectWindow.Background = ButtonDefaultBackground;
            BT_DropSelectWindow.Foreground = ButtonDefaultForeground;
            if (SelectedWindow != null)
            {
                LIST_SelectedWindowControls.Items.Clear();
                var w = SelectedWindow;
                foreach (var p in w.GetType().GetProperties())
                {
                    if (p.SetMethod != null)
                    {
                        if (p.PropertyType == typeof(bool))
                        {
                            AddControlSwitch(p.Name, (bool)p.GetValue(w), (b) => p.SetValue(w, b));
                        }
                        else if (p.PropertyType == typeof(string))
                        {
                            AddControlText(p.Name, (string)p.GetValue(w), (b) => p.SetValue(w, b));
                        }
                    }
                }
            }
        }
        private void BT_DropSelectWindow_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            StartSelectWidnow();
        }
        private void AddControlSwitch(String Title, bool Default, Action<bool> ChangeHandle)
        {
            ToggleButton bt = new ToggleButton()
            {
                IsChecked = Default,
                MinWidth=150,
                Height=25,
                Content = Title,
            };
            bt.Checked += (ss, ee) => ChangeHandle.Invoke(true);
            bt.Unchecked += (ss, ee) => ChangeHandle.Invoke(false);
            this.LIST_SelectedWindowControls.Items.Add(bt);
        }
        private void AddControlText(String Title, String Default, Action<String> ChangeHandle)
        {
            TextBox text = new TextBox();
            text.Height = 25;
            text.MinWidth = 150;
            text.ToolTip = Title;
            text.Text = Default;
            text.TextChanged += (ss, ee) => ChangeHandle.Invoke(text.Text);
            this.LIST_SelectedWindowControls.Items.Add(text);
        }
    }
}
