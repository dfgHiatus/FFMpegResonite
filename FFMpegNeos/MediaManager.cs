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
        internal static async Task<(bool success, string inputName, string convertedName)> PrepareMedia(VideoTextureProvider videoTextureProvider, MediaType returnMediaType, IButton button = null)
        {
            UpdateButtonState(videoTextureProvider.Slot, enabled: false, "General.Processing", button);

            if (!TryGetVideoExportable(videoTextureProvider, out VideoExportable videoExportable, out string exportName))
            {
                UpdateButtonState(videoTextureProvider.Slot, enabled: true, "General.FAILED", button);

                return (false, string.Empty, string.Empty);
            }

            if (!await TryExportVideo(videoExportable, exportName))
            {
                UpdateButtonState(videoTextureProvider.Slot, enabled: true, "General.FAILED", button);

                return (false, string.Empty, string.Empty);
            }
            
            AcquireMediaOutput(returnMediaType, exportName, out var inputName, out var convertedName);

            return (true, inputName, convertedName);
        }
        
        internal static async void ImportMedia(Slot slot, MediaType returnMediaType, List<string> importedNames, IButton button = null)
        {
            await default(ToWorld);
            UniversalImporter.Import(
                DetectMediaType(returnMediaType),
                importedNames,
                slot.World,
                slot.GlobalPosition + (slot.Forward * -0.1f),
                slot.GlobalRotation);
            await default(ToBackground);

            UpdateButtonState(slot, enabled: true, "General.Done", button);
        }

        internal static async void ImportMedia(Slot slot, MediaType returnMediaType, string importName, IButton button = null)
        {
            await default(ToWorld);
            UniversalImporter.Import(
                DetectMediaType(returnMediaType),
                new List<string>() { importName },
                slot.World,
                slot.GlobalPosition + (slot.Forward * -0.1f),
                slot.GlobalRotation);
            await default(ToBackground);

            UpdateButtonState(slot, enabled: true, "General.Done", button);
        }
        
        private static void AcquireMediaOutput(MediaType returnMediaType, string exportName, out string inputName, out string convertedName)
        {
            inputName = Path.GetFullPath(Path.Combine(
                FFMPEGNeos.CachePath,
                exportName));
            convertedName = Path.GetFullPath(Path.Combine(
                FFMPEGNeos.CachePath,
                "converted_" +
                Path.GetFileNameWithoutExtension(exportName) +
                "." +
                GetMediaSuffix(returnMediaType)));
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

        private static async void UpdateButtonState(Slot slot, bool enabled, string message, IButton button = null)
        {
            if (button == null) return;

            await default(ToWorld);
            button.LabelText = slot.GetLocalized(message);
            button.Enabled = enabled;
            await default(ToBackground);
        }

        private static AssetClass DetectMediaType(MediaType mediaType)
        {
            switch (mediaType)
            {
                case MediaType.VIDEO:
                    return AssetClass.Video;
                case MediaType.AUDIO:
                    return AssetClass.Audio;
                case MediaType.IMAGE:
                    return AssetClass.Texture;
                default:
                    throw new ArgumentException();
            }
        }

        private static string GetMediaSuffix(MediaType mediaType)
        {
            switch (mediaType)
            {
                case MediaType.VIDEO:
                    return FFMPEGNeos.Config.GetValue(FFMPEGNeos.preferredVideoFormat);
                case MediaType.AUDIO:
                    return FFMPEGNeos.Config.GetValue(FFMPEGNeos.preferredAudioFormat);
                case MediaType.IMAGE:
                    return FFMPEGNeos.Config.GetValue(FFMPEGNeos.preferredImageFormat);
                default:
                    throw new ArgumentException();
            }
        }

        public enum MediaType
        {
            VIDEO,
            IMAGE,
            AUDIO
        }
    }
}
