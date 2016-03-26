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

		public static bool IsOnMac()
		{
			string output = GetProcessOutput ("uname", "");
			if(output.Contains("Darwin")) //codename for OS X
				return true;

			return false;
		}

		public static string GetUnixName()
		{
			if (IsOnMac ()) 
			{
				string macReturn = "OS X " + GetProcessOutput("sw_vers", "-productVersion");
				macReturn += " " + GetProcessOutput ("uname", "-m");
				macReturn.Trim ();

				return macReturn;
			}

			string returnValue = GetProcessOutput ("lsb_release", "-d");
			returnValue = returnValue.Substring (returnValue.LastIndexOf (':') + 1).Trim ();
			returnValue += " " + GetProcessOutput ("uname", "-m");

			return returnValue;
		}

		private static string GetProcessOutput(string process, string args)
		{
			try
			{
				Process p = new Process();
				p.StartInfo.UseShellExecute = false;
				p.StartInfo.RedirectStandardOutput = true;
				if(!string.IsNullOrEmpty(args))
					p.StartInfo.Arguments = $" {args}";
				p.StartInfo.FileName = process;
				p.Start();
				string output = p.StandardOutput.ReadToEnd();
				p.WaitForExit();
				if(output == null)
					output = "";
				output = output.Trim();
				return output;
			}
			catch(Exception)
			{
				return "";
			}
		}
	}
}

