using Microsoft.Win32;
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

        public static string FindPython()
        {
            string[] locations = new string[] {@"SOFTWARE\Python\PythonCore",
                                               @"SOFTWARE\Wow6432\Python\PythonCore" };

            
            foreach( string loc in locations )
            {
                RegistryKey rk = Registry.LocalMachine.OpenSubKey(loc);
                if (rk != null)
                {
                    string found = getHighest(Registry.LocalMachine, loc);
                    if (String.IsNullOrEmpty(found) == false) return found;
                }
            }

            // now try Users
            foreach( string loc in locations )
            {
                RegistryKey rk = Registry.CurrentUser.OpenSubKey(loc);
                if (rk != null)
                {
                    string found = getHighest(Registry.CurrentUser, loc);
                    if (String.IsNullOrEmpty(found) == false) return found;
                }
            }

            return null;

        }

        private static string getHighest(RegistryKey parent, string key)
        {
            RegistryKey rk = parent.OpenSubKey(key);

            string[] versions = rk.GetSubKeyNames();

            double high = 0;
            foreach(string ver in versions)
            {
                Double temp = 0;
                if (Double.TryParse( ver, out temp))
                {
                    if (temp > high) high = temp;
                }
            }

            if (high>0)
            {
                RegistryKey install = parent.OpenSubKey(key + "\\" + high.ToString() + "\\InstallPath");
                if (install != null)
                {
                    string value = install.GetValue(null).ToString();

                    return value;
                }
            }

            return null;
        }
    }
}
