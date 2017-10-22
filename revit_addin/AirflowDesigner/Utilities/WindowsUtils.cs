using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AirflowDesigner.Utilities
{
    public static class WindowsUtils
    {
        private static List<System.Windows.Forms.Form> _Forms = new List<System.Windows.Forms.Form>();

        public static IntPtr GetMainWindowHandle()
        {

            System.Diagnostics.Process p = System.Diagnostics.Process.GetCurrentProcess();
            return p.MainWindowHandle;
        }

        public static void RegisterModeless(System.Windows.Forms.Form f)
        {
            _Forms.Add(f);
        }

        public static void CloseAll()
        {
            foreach( var form in _Forms)
            {
                try
                {
                    if (form.IsDisposed) continue;
                    form.Close();
                }
                catch { }
            }

            _Forms.Clear();
        }
    }
}
