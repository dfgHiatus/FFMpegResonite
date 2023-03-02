using FrooxEngine;
using System;
using System.IO;
using System.Linq;

namespace FFMPEGNeos
{
    internal class FFMPEGInterface
    {
        private static string _linuxFfMpegCache;
        private static string _linuxFfProbeCache;
        private static string _linuxFfPlayCache;

        private static string GetLinuxPath(string command, ref string cache)
        {
            //most linux distros provide ffmpeg as a package, so it's better to look for it instead of
            //embedding it
            if (string.IsNullOrWhiteSpace(cache))
            {
                var path = Environment.GetEnvironmentVariable("PATH") ?? "";
                var paths = path.Split(Path.PathSeparator).ToList();
                var test = paths.Select(i => Path.Combine(i, command)).FirstOrDefault(File.Exists);
                cache = string.IsNullOrWhiteSpace(test) ? "notfound" : test;
            }
            return cache;
        }
        public static string FFPMEG
        {
            get
            {
                switch (Engine.Current.Platform)
                {
                    case Platform.Windows:
                        return Path.Combine(Engine.Current.AppPath, "nml_mods", "ffmpeg", "ffmpeg.exe");
                    case Platform.Linux:
                        return GetLinuxPath("ffmpeg", ref _linuxFfMpegCache);
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public static string FFPROBE
        {
            get
            {
                switch (Engine.Current.Platform)
                {
                    case Platform.Windows:
                        return Path.Combine(Engine.Current.AppPath, "nml_mods", "ffmpeg", "ffprobe.exe");
                    case Platform.Linux:
                        return GetLinuxPath("ffprobe", ref _linuxFfProbeCache);
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        public static string FFPLAY
        {
            get
            {
                switch (Engine.Current.Platform)
                {
                    case Platform.Windows:
                        return Path.Combine(Engine.Current.AppPath, "nml_mods", "ffmpeg", "ffplay.exe");
                    case Platform.Linux:
                        return GetLinuxPath("ffplay", ref _linuxFfPlayCache);
                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}