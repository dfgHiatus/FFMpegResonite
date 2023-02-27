using System.IO;

namespace FFMPEGNeos
{
    internal class FFMPEGInterface
    {
        public readonly static string FFPMEG = Path.Combine("nml_mods", "ffmpeg", "ffmpeg.exe");
        public readonly static string FFPROBE = Path.Combine("nml_mods", "ffmpeg", "ffprobe.exe");
        public readonly static string FFPLAY = Path.Combine("nml_mods", "ffmpeg", "ffplay.exe");
    }
}