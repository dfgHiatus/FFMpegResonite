using FrooxEngine;
using System;
using System.IO;

namespace FFMPEGNeos
{
    internal class FFMPEGInterface
    {
        public static string FFPMEG
        {
            get
            {
                switch (Engine.Current.Platform)
                {
                    case Platform.Windows:
                        return Path.Combine("nml_mods", "ffmpeg", "ffmpeg.exe");
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
                        return Path.Combine("nml_mods", "ffmpeg", "ffprobe.exe");
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
                        return Path.Combine("nml_mods", "ffmpeg", "ffplay.exe");
                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}