using NetCoreEx.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MWindows.Utils
{
    public static class Utils
    {
        public static WindowEntity[] GetWindows()
        {
            List<WindowEntity> ws = new List<WindowEntity>();
            WinApi.User32.User32Methods.EnumWindows((i, ip) =>
            {
                ws.Add(new WindowEntity(i));
                return true;
            }, IntPtr.Zero);

            return ws.ToArray();
        }
    }
}
