using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirflowDesigner.Utilities
{
    public static class WindowsUtils
    {
        public static IntPtr GetMainWindowHandle()
        {

            System.Diagnostics.Process p = System.Diagnostics.Process.GetCurrentProcess();
            return p.MainWindowHandle;
        }
    }
}
