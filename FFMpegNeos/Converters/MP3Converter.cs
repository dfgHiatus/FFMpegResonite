//using BaseX;
//using CodeX;
//using FrooxEngine;
//using HarmonyLib;
//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace FFMPEGNeos
//{
//    /// <summary>
//    /// Converts MP3s to OGGs, or whatever the user's preferred audio type is
//    /// </summary>
//    internal static class MP3Converter
//    {
//        [HarmonyPatch(typeof(UniversalImporter), "Import", typeof(AssetClass), typeof(IEnumerable<string>),
//            typeof(World), typeof(float3), typeof(floatQ), typeof(bool))]
//        public class UniversalImporterPatch
//        {
//            static bool Prefix(ref IEnumerable<string> files, World world, float3 pos, floatQ rot, bool state)
//            {
//                List<string> mp3s = new();
//                List<string> notMp3s = new();
                
//                foreach (var file in files)
//                {
//                    if (Path.GetExtension(file).ToLower().Equals(".mp3"))
//                        mp3s.Add(file);
//                    else
//                        notMp3s.Add(file);
//                }
                
//                // Convert the mp3's to oggs and then reimport them recursively
//                string command = @"-i {inputMP3} -c:a libvorbis {ouputAudio}";
//                List<string> converted = new();
//                foreach (var mp3 in mp3s.ToArray())
//                {
//                    var convertedFilename = Path.GetFileNameWithoutExtension(mp3) + FFMPEGNeos.Config.GetValue(FFMPEGNeos.preferredAudioFormat);
//                    var formattedCommand = string.Format(command, mp3, convertedFilename);
//                    if (await FFMPEGWrapper.RunFFScript(FFMPEGInterface.FFPMEG, command, hidden: false))
//                    {
//                        converted.Add(convertedFilename);
//                    }
//                }

//                UniversalImporter.Import(AssetClass.Audio, converted, world, pos, rot, state);


//                if (notMp3s.Count <= 0) return false;
//                files = notMp3s.ToArray();
//                return true;
//            }
//        }
//    }
//}
