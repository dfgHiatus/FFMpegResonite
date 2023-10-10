using Elements.Core;
using Elements.Assets;
using FrooxEngine;
using System.Collections.Generic;
using System.IO;

namespace FFMPEGResonite.Converters
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
                        FFMPEGResonite.CachePath,
                        Path.GetFileNameWithoutExtension(gif) + "." + FFMPEGResonite.Config.GetValue(FFMPEGResonite.PreferredVideoFormat)); 
                    var command = $"-i {gif} -pix_fmt yuv420p {convertedFilename}"; // Defaults to "mp4"
                    if (await FFMPEGWrapper.RunFFScript( 
                        FFMPEGInterface.FFPMEG, 
                        command, 
                        overwrite: FFMPEGResonite.Config.GetValue(FFMPEGResonite.Overwrite), 
                        hidden: FFMPEGResonite.Config.GetValue(FFMPEGResonite.DontCreateConsole)))
                    {
                        mp4s.Add(convertedFilename);
                    }
                }

                await default(ToWorld);
                UniversalImporter.Import(AssetClass.Video, mp4s, world, pos, rot, silent: FFMPEGResonite.Config.GetValue(FFMPEGResonite.ImportRawFiles));
            });
        }
    }
}
