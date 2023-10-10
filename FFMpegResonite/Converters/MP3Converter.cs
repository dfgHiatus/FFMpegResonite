using Elements.Core;
using Elements.Assets;
using FrooxEngine;
using System.Collections.Generic;
using System.IO;

namespace FFMPEGResonite.Converters
{
    internal static class MP3Converter
    {
        internal static void ConvertAndImport(List<string> mp3s, World world, float3 pos, floatQ rot)
        {
            // Convert the mp3's to oggs and then reimport them
            world.Coroutines.StartTask(async () =>
            {
                await default(ToBackground);
                List<string> oggs = new();

                foreach (var mp3 in mp3s)
                {
                    var convertedFilename = Path.Combine(
                        FFMPEGResonite.CachePath, 
                        Path.GetFileNameWithoutExtension(mp3) + "." + FFMPEGResonite.Config.GetValue(FFMPEGResonite.PreferredAudioFormat)); 
                    var command = $"-i {mp3} -c:a libvorbis {convertedFilename}"; // Defaults to "ogg"
                    if (await FFMPEGWrapper.RunFFScript(
                        FFMPEGInterface.FFPMEG,
                        command,
                        overwrite: FFMPEGResonite.Config.GetValue(FFMPEGResonite.Overwrite),
                        hidden: FFMPEGResonite.Config.GetValue(FFMPEGResonite.DontCreateConsole)))
                    {
                        oggs.Add(convertedFilename);
                    }
                }

                await default(ToWorld);
                UniversalImporter.Import(AssetClass.Audio, oggs, world, pos, rot, silent: FFMPEGResonite.Config.GetValue(FFMPEGResonite.ImportRawFiles));
            });
        }
    }
}
