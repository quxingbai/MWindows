using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NetCoreEx.Geometry;
using api = WinApi.User32.User32Methods;

namespace MWindows.Utils
{
    public class WindowEntity
    {
        private IntPtr Ptr = IntPtr.Zero;
        public Rectangle Rectangle { get => GetRec(); set => SetRec(value); }
        public bool IsTopMost { get => GetTopMost(); set => SetTopMost(value); }
        public String Title { get => GetText(); set => SetText(value); }
        public String ClassName { get => GetClassName(); }
        public IntPtr Hwnd { get => Ptr; }
        public bool IsVisible { get => api.IsWindowVisible(Ptr); }
        public WindowEntity(IntPtr ptr)
        {
            Ptr = ptr;
        }
        private String GetClassName()
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                api.GetClassName(Ptr, sb, 512);
                return sb.ToString();
            }
            catch(AccessViolationException error)
            {
                throw new Exception("ASD");
            }
        }
        private Rectangle GetRec()
        {
            api.GetWindowRect(Ptr, out Rectangle rec);
            return rec;
        }
        private bool SetRec(Rectangle rec)
        {
            return api.SetWindowPos(Ptr, new IntPtr(IsTopMost ? -1 : -2), rec.Left, rec.Top, rec.Width, rec.Height, WinApi.User32.WindowPositionFlags.SWP_NOZORDER);
        }
        private bool GetTopMost()
        {
            var code = (int)api.GetWindowLongPtr(Ptr, -20);
            return (code & 0x00000008) == 0x00000008;
        }
        private bool SetTopMost(bool IsTopMost)
        {
            if (IsTopMost == this.IsTopMost) return true;
            var rec = GetRec();
            return api.SetWindowPos(Ptr, (IntPtr)(IsTopMost ? -1:-2), rec.Left, rec.Top, rec.Width, rec.Height, 0);
        }
        private String GetText()
        {
            StringBuilder sb = new StringBuilder();
            api.GetWindowText(Ptr, sb, 512);
            return sb.ToString();
        }
        private bool SetText(String Text)
        {
            return api.SetWindowText(Ptr, Text);
        }
    }
}
