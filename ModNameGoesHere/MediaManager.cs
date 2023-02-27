using BaseX;
using CodeX;
using FrooxEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace FFMPEGNeos
{
    internal static class MediaManager
    {
        internal static async Task<(bool success, string inputName, string convertedName)> PrepareMedia(VideoTextureProvider videoTextureProvider, bool shouldReturnVideo)
        {
            if (!TryGetVideoExportable(videoTextureProvider, out VideoExportable videoExportable, out string exportName)) 
                return (false, null, string.Empty);

            if (!await TryExportVideo(videoExportable, exportName)) 
                return (false, null, string.Empty);
            
            AcquireMediaOutput(isVideo: shouldReturnVideo, exportName, out var inputName, out var convertedName);

            return (true, inputName, convertedName);
        }

        internal static async void ImportMedia(VideoTextureProvider videoTextureProvider, bool isVideo, string importName)
        {
            await default(ToWorld);
            UniversalImporter.Import(
                isVideo ? AssetClass.Video : AssetClass.Audio,
                new List<string>() { importName },
                videoTextureProvider.World,
                videoTextureProvider.Slot.GlobalPosition + (videoTextureProvider.Slot.Forward * -0.1f),
                videoTextureProvider.Slot.GlobalRotation);
            await default(ToBackground);
        }
        
        private static void AcquireMediaOutput(bool isVideo, string exportName, out string inputName, out string convertedName)
        {
            inputName = Path.GetFullPath(Path.Combine(
                FFMPEGNeos.CachePath,
                exportName));
            convertedName = Path.GetFullPath(Path.Combine(
                FFMPEGNeos.CachePath,
                "converted_" +
                Path.GetFileNameWithoutExtension(exportName) +
                "." +
                (isVideo ? FFMPEGNeos.Config.GetValue(FFMPEGNeos.preferredVideoFormat) : FFMPEGNeos.Config.GetValue(FFMPEGNeos.preferredAudioFormat))));
        }

        private static bool TryGetVideoExportable(VideoTextureProvider videoTextureProvider, out VideoExportable videoExportable, out string parsedName)
        {
            videoExportable = videoTextureProvider.Slot.GetComponent<VideoExportable>();
            if (videoExportable is null)
            {
                parsedName = string.Empty;
                return false;
            }

            parsedName = videoExportable.ExportName.Replace(' ', '_');
            return true;
        }

        private static async Task<bool> TryExportVideo(VideoExportable videoExport, string exportName)
        {
            if (videoExport == null)
            {
                UniLog.Error("VideoExportable not found on slot.");
                return false;
            }
            
            try
            {
                var exported = await videoExport.Export(FFMPEGNeos.CachePath, exportName, 0).ConfigureAwait(false);
                if (!exported)
                {
                    UniLog.Error("Video failed to export on 'Extract audio from video'");
                    return false;
                }
            }
            // File already exists, continue
            catch (AggregateException) { return true; }
            catch (IOException) { return true; }
            catch (Exception e)
            {
                UniLog.Error($"An unexpected error {e.GetType()} occured when exporting video on 'Extract audio from video': {e.Message}");
                return false;
            }

            return true;
        }
    }
}
