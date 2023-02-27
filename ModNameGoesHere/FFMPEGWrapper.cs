using BaseX;
using FrooxEngine;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FFMPEGNeos
{
    internal static class FFMPEGWrapper
    { 
        public static async Task<bool> RunFFScript(string executable, string arguments, bool overwrite = true, bool hidden = true)
        {
            try
            {
                var process = new Process();
                process.StartInfo.FileName = executable;
                process.StartInfo.Arguments = string.Join(" ", overwrite ? "-y" : "-n", arguments);
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.CreateNoWindow = true;
                process.Start();
                await process.WaitForExitAsync();
                return true;
            }
            catch (Exception e)
            {
                UniLog.Error(e.Message);
                return false;
            }
        }

        /// <summary>
        /// Waits asynchronously for the process to exit.
        /// https://stackoverflow.com/questions/470256/process-waitforexit-asynchronously
        /// </summary>
        /// <param name="process">The process to wait for cancellation.</param>
        /// <param name="cancellationToken">A cancellation token. If invoked, the task will return 
        /// immediately as canceled.</param>
        /// <returns>A Task representing waiting for the process to end.</returns>
        public static Task WaitForExitAsync(this Process process,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (process.HasExited) return Task.CompletedTask;

            var tcs = new TaskCompletionSource<object>();
            process.EnableRaisingEvents = true;
            process.Exited += (sender, args) => tcs.TrySetResult(null);
            if (cancellationToken != default(CancellationToken))
                cancellationToken.Register(() => tcs.SetCanceled());

            return process.HasExited ? Task.CompletedTask : tcs.Task;
        }
    }
}
