using System.Drawing;

namespace FFMpegNeos.Tests
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var inputPath = @"C:\Users\BenjaminEvans\AppData\Local\Temp\Solirax\NeosVR\Cache\FFMPEG\2023-02-21_08-06-15.mp4";
            var inputMP3 = "input.mp3";
            var inputGIF = "input.gif";

            var outputVideo = "output.mp4";
            
            var outputAudio = @"C:\Users\BenjaminEvans\AppData\Local\Temp\Solirax\NeosVR\Cache\FFMPEG\2023-02-21_08-06-15.ogg";

            var outputQuantity = "1";
            var outputTime1 = "00:00:05";
            var outputTime2 = "00:00:10";
            var outputQuality = "2";
            var outputFrame = "output.jpg";

            Console.WriteLine(DateTime.Now.ToString("hh:mm:ss"));

            /// Metadata
            var command = $"-i {inputPath} -show_format -v quiet";
            FFMpegWrapper.RunFFScript(FFMPEGInterface.FFPROBE, command, hidden: false);

            /// Extract audio from video
            // var command = $"-i {inputPath} -vn {outputAudio}";
            // FFMpegWrapper.RunFFScript(FFMPEGInterface.FFPMEG, command, hidden: false);

            /// Seperate video from audio
            //command = $"-i {inputPath} -an {outputVideo}";
            //FFMpegWrapper.RunFFScript(FFMPEGInterface.FFPMEG, command, hidden: false);

            /// Extract one snapshot from a video given a time
            //var command = $"-ss {outputTime1} -i {inputPath} -vframes {outputQuantity} -q:v {outputQuality} {outputFrame}";
            //FFMpegWrapper.RunFFScript(FFMPEGInterface.FFPMEG, command, hidden: false);

            /// Extract many snapshots from a video given a time range (optionally, skip frames with -vf fps=1/10!)
            //Directory.CreateDirectory("range");
            //var command = $"-ss {outputTime1} -to {outputTime2} -i {inputPath} -q:v {outputQuality} {Path.Combine("range", "output_%03d.jpg")}";
            //FFMpegWrapper.RunFFScript(FFMPEGInterface.FFPMEG, command, hidden: false);

            /// Extract a subvideo from a video
            //var command = $"-ss {outputTime1} -to {outputTime2} -i {inputPath} -c:v copy -c:a copy {outputVideo}";
            //FFMpegWrapper.RunFFScript(FFMPEGInterface.FFPMEG, command, hidden: false);

            /// Convert mp3s to an oggs
            //var command = $"-i {inputMP3} -c:a libvorbis {ouputAudio}";
            //FFMpegWrapper.RunFFScript(FFMPEGInterface.FFPMEG, command, hidden: false);

            /// Convert gifs to a mp4s
            //var command = $"-i {inputGIF} -pix_fmt yuv420p {outputVideo}";
            //FFMpegWrapper.RunFFScript(FFMPEGInterface.FFPMEG, command, hidden: false);

            /// TODO? Create a video from a list of images
        }
    }
}