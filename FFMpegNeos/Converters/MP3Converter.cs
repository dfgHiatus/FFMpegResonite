using BaseX;
using CodeX;
using FrooxEngine;
using System.Collections.Generic;
using System.IO;

namespace FFMPEGNeos.Converters
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
                        FFMPEGNeos.CachePath, 
                        Path.GetFileNameWithoutExtension(mp3) + "." + FFMPEGNeos.Config.GetValue(FFMPEGNeos.preferredAudioFormat)); 
                    var command = $"-i {mp3} -c:a libvorbis {convertedFilename}"; // Defaults to "ogg"
                    if (await FFMPEGWrapper.RunFFScript(
                        FFMPEGInterface.FFPMEG,
                        command,
                        overwrite: FFMPEGNeos.Config.GetValue(FFMPEGNeos.overwrite),
                        hidden: FFMPEGNeos.Config.GetValue(FFMPEGNeos.dontCreateConsole)))
                    {
                        oggs.Add(convertedFilename);
                    }
                }

                await default(ToWorld);
                UniversalImporter.Import(AssetClass.Audio, oggs, world, pos, rot, silent: FFMPEGNeos.Config.GetValue(FFMPEGNeos.importRawFiles));
            });
        }
    }
}
