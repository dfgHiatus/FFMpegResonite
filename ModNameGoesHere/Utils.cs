using FrooxEngine;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace FFMpegNeos
{
    public static class Utils
    {
        /// <summary>
        /// Saves a video to the cache folder if it does not already exist there. If it does, it will return the path to the cached version.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="overwrite"></param>
        public static async Task<(bool success, string path)> SaveVideo(VideoTextureProvider instance, bool overwrite = false)
        {
            var exporter = instance.Slot.GetComponent<VideoExportable>();
            var output = Path.Combine(FFMpegNeos.CachePath, exporter.ExportName);

            // TODO Do we overwrite?
            if (File.Exists(output) && !overwrite)
            {
                return (true, output);
            }

            // The last int parameter here seems to be unused
            var success = await exporter.Export(FFMpegNeos.CachePath, exporter.ExportName, 0);

            if (success)
            {
                return (true, output);
            }
            else
            {
                return (false, string.Empty);
            }
        }

        // Credit to delta for this method https://github.com/XDelta/
        public static string GenerateMD5(string filepath)
        {
            using var hasher = MD5.Create();
            using var stream = File.OpenRead(filepath);
            var hash = hasher.ComputeHash(stream);
            return BitConverter.ToString(hash).Replace("-", "");
        }
    }
}
