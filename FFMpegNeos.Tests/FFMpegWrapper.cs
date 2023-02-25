using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpegNeos.Tests
{
    
    internal static class FFMpegWrapper
    { 
        public static bool RunFFScript(string executable, string arguments, bool overwrite = true, bool hidden = true)
        {
            try
            {
                var process = new Process();
                process.StartInfo.FileName = executable;

                if (overwrite)
                    process.StartInfo.Arguments = string.Join(' ', "-y", arguments);
                else
                    process.StartInfo.Arguments = string.Join(' ', "-n", arguments);

                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.UseShellExecute = false;
                process.StartInfo.CreateNoWindow = hidden;
                process.StartInfo.WindowStyle = hidden ? ProcessWindowStyle.Hidden : ProcessWindowStyle.Normal;
                process.OutputDataReceived += OnOutput;
                process.Start();
                process.BeginOutputReadLine();
                process.WaitForExit();
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
        }

        private static void OnOutput(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }
    }
}
