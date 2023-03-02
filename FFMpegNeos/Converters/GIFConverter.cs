using BaseX;
using CodeX;
using FrooxEngine;
using System.Collections.Generic;
using System.IO;

namespace FFMPEGNeos.Converters
{
    internal class GIFConverter
    {
        internal static void ConvertAndImport(List<string> gifs, World world, float3 pos, floatQ rot)
        {
            // Convert the gif's to mp4s and then reimport them
            world.Coroutines.StartTask(async () =>
            {
                await default(ToBackground);
                List<string> mp4s = new();

                foreach (var gif in gifs)
                {
                    var convertedFilename = Path.Combine(
                        FFMPEGNeos.CachePath,
                        Path.GetFileNameWithoutExtension(gif) + "." + FFMPEGNeos.Config.GetValue(FFMPEGNeos.PreferredVideoFormat)); 
                    var command = $"-i {gif} -pix_fmt yuv420p {convertedFilename}"; // Defaults to "mp4"
                    if (await FFMPEGWrapper.RunFFScript( 
                        FFMPEGInterface.FFPMEG, 
                        command, 
                        overwrite: FFMPEGNeos.Config.GetValue(FFMPEGNeos.Overwrite), 
                        hidden: FFMPEGNeos.Config.GetValue(FFMPEGNeos.DontCreateConsole)))
                    {
                        mp4s.Add(convertedFilename);
                    }
                }

                await default(ToWorld);
                UniversalImporter.Import(AssetClass.Video, mp4s, world, pos, rot, silent: FFMPEGNeos.Config.GetValue(FFMPEGNeos.ImportRawFiles));
            });
        }
    }
}
