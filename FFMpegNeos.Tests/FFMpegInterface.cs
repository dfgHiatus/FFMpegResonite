using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpegNeos.Tests
{
    internal class FFMPEGInterface
    {
        public readonly static string FFPMEG = Path.Combine("nml_mods", "ffmpeg", "ffmpeg.exe");
        public readonly static string FFPROBE = Path.Combine("nml_mods", "ffmpeg", "ffprobe.exe");
    }
}
