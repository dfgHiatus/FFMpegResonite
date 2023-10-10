﻿using Elements.Core;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FFMPEGResonite
{
    internal static class FFMPEGWrapper
    { 
        public static async Task<bool> RunFFScript(string executable, string arguments, bool overwrite = true, bool hidden = true)
        {
            try
            {
                var process = new Process();
                process.StartInfo.FileName = executable;

                switch (Path.GetFileNameWithoutExtension(executable))
                {
                    case "ffmpeg":
                        process.StartInfo.Arguments = string.Join(" ", overwrite ? "-y" : "-n", arguments);
                        break;
                    case "ffprobe":
                    case "ffplay":
                        process.StartInfo.Arguments = arguments;
                        break;
                    default:
                        throw new ArgumentException();
                }
        
                process.StartInfo.UseShellExecute = true;
                process.StartInfo.CreateNoWindow = true; // Doesn't actually hide the window?
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
            CancellationToken cancellationToken = default)
        {
            if (process.HasExited) return Task.CompletedTask;

            var tcs = new TaskCompletionSource<object>();
            process.EnableRaisingEvents = true;
            process.Exited += (_, _) => tcs.TrySetResult(null);
            if (cancellationToken != default)
                cancellationToken.Register(() => tcs.SetCanceled());

            return process.HasExited ? Task.CompletedTask : tcs.Task;
        }
    }
}
