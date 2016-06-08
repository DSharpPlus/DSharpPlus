using Microsoft.Win32;
using System;
using System.Diagnostics;

namespace Luigibot
{
	public static class OSDetermination
	{
        public static bool IsOnUnix()
        {
            if (System.IO.Path.DirectorySeparatorChar == '/')
                return true;

            return false;
        }

        public static bool IsOnWindows()
        {
            return System.IO.Path.DirectorySeparatorChar == '\\';
        }

        public static bool IsOnMac()
        {
            string output = GetProcessOutput("uname", "");
            if (output.Contains("Darwin")) //codename for OS X
                return true;

            return false;
        }

        private static string HKLM_GetString(string path, string key)
        {
            try
            {
                RegistryKey rk = Registry.LocalMachine.OpenSubKey(path);
                if (rk == null) return "";
                return (string)rk.GetValue(key);
            }
            catch { return ""; }
        }

        private static string FriendlyName()
        {
            string ProductName = HKLM_GetString(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "ProductName");
            string CSDVersion = HKLM_GetString(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", "CSDVersion");
            if (ProductName != "")
            {
                return (ProductName.StartsWith("Microsoft") ? "" : "Microsoft ") + ProductName +
                            (CSDVersion != "" ? " " + CSDVersion : "");
            }
            return "";
        }

        public static string GetUnixName()
        {
            if (IsOnMac())
            {
                Version macVersion = new Version(GetProcessOutput("sw_vers", "-productVersion"));
                string macReturn = (macVersion.Minor < 8 ? "Mac " : "") + "OS X " + GetProcessOutput("sw_vers", "-productVersion");
                macReturn += " " + GetProcessOutput("uname", "-m");
                macReturn.Trim();

                return macReturn;
            }

            if (IsOnWindows())
            {
                return FriendlyName() + (Environment.Is64BitOperatingSystem ? "amd64" : "i386");
            }

            string returnValue = GetProcessOutput("lsb_release", "-d");
            returnValue = returnValue.Substring(returnValue.LastIndexOf(':') + 1).Trim();
            returnValue += " " + GetProcessOutput("uname", "-m");

            return returnValue;
        }

        private static string GetProcessOutput(string process, string args)
        {
            try
            {
                Process p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                if (!string.IsNullOrEmpty(args))
                    p.StartInfo.Arguments = $" {args}";
                p.StartInfo.FileName = process;
                p.Start();
                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                if (output == null)
                    output = "";
                output = output.Trim();
                return output;
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}

