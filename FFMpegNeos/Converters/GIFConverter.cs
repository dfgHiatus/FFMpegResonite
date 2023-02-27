//using BaseX;
//using FrooxEngine;
//using HarmonyLib;
//using System;
//using System.IO;
//using System.Threading.Tasks;
//using static FFMPEGNeos.MediaManager;

//namespace FFMPEGNeos
//{
//    /// <summary>
//    /// Converts GIFs to MP4s
//    /// </summary>
//    internal static class GIFConverter
//    {
//        [HarmonyPatch(typeof(ImageImporter), "ImportImage")]
//        class FileImporterPatch
//        {
//            public static bool Prefix(string path, ref Task __result, Slot targetSlot, float3? forward, StereoLayout stereoLayout, ImageProjection projection, bool setupScreenshotMetadata, bool addCollider)
//            {
//                Uri uri = new Uri(path);
//                System.Drawing.Image image = null;
//                bool validLocal = false;
//                bool validRemote = false;

//                // Local file import vs URL import
//                if (uri.Scheme == "file" && string.Equals(Path.GetExtension(path), ".gif", StringComparison.OrdinalIgnoreCase))
//                {
//                    image = System.Drawing.Image.FromStream(File.OpenRead(path));
//                    validLocal = true;
//                }
//                else if (uri.Scheme == "http" || uri.Scheme == "https")
//                {
//                    using var client = new System.Net.WebClient();
//                    var type = client.ResponseHeaders.Get("content-type");
//                    validRemote = type == "image/gif";
//                    image = System.Drawing.Image.FromStream(client.OpenRead(uri));
//                }

//                if (!(validLocal || validRemote))
//                {
//                    UniLog.Error($"Image is not a gif or the URI Scheme {uri.Scheme} is not supported");
//                    image?.Dispose();
//                    return true;
//                }

//                __result = targetSlot.StartTask(async delegate ()
//                {
//                    await default(ToBackground);
//                    string command;
//                    string importName;
//                    if (validRemote)
//                    {
//                        using var client = new System.Net.WebClient();
//                        var localPath = Path.Combine(FFMPEGInterface.FFPMEG, "temp.gif");
//                        client.DownloadFile(uri, localPath);
//                        command = $"-i {localPath} -pix_fmt yuv420p converted.mp4";
//                        importName = "converted.mp4";
//                    }
//                    else if (validLocal)
//                    {
//                        command = $"-i {path} -pix_fmt yuv420p {Path.GetFileNameWithoutExtension(path)}.mp4";
//                        importName = Path.GetFileNameWithoutExtension(path) + "mp4";
//                    }
//                    else
//                    {
//                        throw new ArgumentException();
//                    }

//                    await FFMPEGWrapper.RunFFScript(FFMPEGInterface.FFPMEG, command, hidden: false);
//                    MediaManager.ImportMedia(targetSlot, MediaType.VIDEO, importName);

//                });

//                return false;
//            }
//        }
//    }
//}
