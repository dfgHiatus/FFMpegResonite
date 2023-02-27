using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using NeosModLoader;
using System;
using System.IO;
using System.Text.RegularExpressions;
using static FFMPEGNeos.MediaManager;

namespace FFMPEGNeos
{
    public class FFMPEGNeos : NeosMod
    {
        public override string Name => "FFMpegNeos";
        public override string Author => "dfgHiatus";
        public override string Version => "1.0.0";
        public override string Link => "https://github.com/dfgHiatus/FFMpegNeos/";

        internal static readonly string CachePath = Path.GetFullPath(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
            "Temp",
            "Solirax",
            "NeosVR",
            "Cache", 
            "FFMPEG"));
        
        internal static ModConfiguration Config;

        private static Regex validTimes = new Regex(@"[0-9]{2}:[0-9]{2}:[0-9]{2}(\.[0-9]{1,3})?");
        
        [AutoRegisterConfigKey]
        public static ModConfigurationKey<bool> overwrite = new ModConfigurationKey<bool>("overwrite", "Overwrite files", () => true);

        [AutoRegisterConfigKey]
        public static ModConfigurationKey<bool> dontCreateConsole = new ModConfigurationKey<bool>("dontCreateConsole", "Hide FFMPEG output", () => true);

        [AutoRegisterConfigKey]
        public static ModConfigurationKey<bool> importFilesRaw = new ModConfigurationKey<bool>("importFilesRaw", "Import raw files", () => false);

        [AutoRegisterConfigKey]
        public static ModConfigurationKey<string> preferredVideoFormat = new ModConfigurationKey<string>("preferredVideoFormat", "Preferred video format", () => "mp4");

        [AutoRegisterConfigKey]
        public static ModConfigurationKey<string> preferredAudioFormat = new ModConfigurationKey<string>("preferredAudioFormat", "Preferred audio format", () => "ogg");

        [AutoRegisterConfigKey]
        public static ModConfigurationKey<string> preferredImageFormat = new ModConfigurationKey<string>("preferredImageFormat", "Preferred image format", () => "jpg");

        [AutoRegisterConfigKey]
        public static ModConfigurationKey<int> preferredImageQuality = new ModConfigurationKey<int>("preferredImageQuality", "Preferred image quality", () => 2);

        public override void DefineConfiguration(ModConfigurationDefinitionBuilder builder)
        {
            builder
                .Version(new Version(1, 0, 0))
                .AutoSave(true);
        }

        public override void OnEngineInit()
        {
            if (!Initialize()) return;
            Config = GetConfiguration();
            Harmony harmony = new Harmony("net.dfgHiatus.FFMPEGNeos");
            harmony.PatchAll();
        }

        private static bool Initialize()
        {
            Directory.CreateDirectory(CachePath);
            
            if (!File.Exists(FFMPEGInterface.FFPMEG))
            {
                Error(FFMPEGInterface.FFPMEG + " not found, aborting.");
                return false;
            }

            if (!File.Exists(FFMPEGInterface.FFPROBE))
            {
                Error(FFMPEGInterface.FFPROBE + " not found, aborting.");
                return false;
            }

            if (!File.Exists(FFMPEGInterface.FFPLAY))
            {
                Error(FFMPEGInterface.FFPLAY + " not found, aborting.");
                return false;
            }

            return true;
        }

        [HarmonyPatch(typeof(VideoTextureProvider), "BuildInspectorUI", typeof(UIBuilder))]
        public class VideoTextureProviderPatch
        {
            public static void Postfix(VideoTextureProvider __instance, UIBuilder ui)
            {
                ui.Spacer(12f);
                ui.Text("FFmpeg Utilities");
                ui.Spacer(12f);

                // Requires a custom editor
                // text = "Join video clips together";
                // ui.Button(in text, ExtractAudioFromVideo.Extract(__instance));

                // Requires a custom editor
                // text = "Join images into video";
                // ui.Button(in text, ExtractAudioFromVideo.Extract(__instance));

                // Requires a custom editor
                // text = "Join images into a video:";
                // ui.Button(in text, ExtractAudioFromVideo.Extract(__instance));

                var snapShot = ui.Button("Extract a snapshot from the video with the given time:");
                var snapshotTime = ui.HorizontalElementWithLabel("Time", 0.2f, () => ui.TextField());
                ui.Text("Time must be of the format HH:MM:SS");
                ui.Spacer(12f);
                snapShot.LocalPressed += (IButton button, ButtonEventData eventData) =>
                {
                    if (!IsValidTimeString(snapshotTime.Text.Content.Value))
                    {
                        button.LabelText = __instance.GetLocalized("General.FAILED");
                        button.Enabled = false;
                        return;
                    }

                    __instance.StartTask(async () =>
                    {
                        var result = await MediaManager.PrepareMedia(__instance, returnMediaType: MediaType.IMAGE, button);
                        if (!result.success) return;

                        var command = $"-ss {snapshotTime.Text.Content.Value} -i {result.inputName} -vframes 1 -q:v {Config.GetValue(preferredImageQuality)} {result.convertedName}";
                        if (await FFMPEGWrapper.RunFFScript(FFMPEGInterface.FFPMEG, command, Config.GetValue(overwrite), Config.GetValue(dontCreateConsole)))
                        {
                            MediaManager.ImportMedia(__instance.Slot, MediaType.IMAGE, result.convertedName, button);
                        }
                    });
                };

                var subVideo = ui.Button("Create a subvideo with the following start and end times:");
                var startTimeSubvideo = ui.HorizontalElementWithLabel("Start", 0.2f, () => ui.TextField());
                var endTimeSubvideo = ui.HorizontalElementWithLabel("End", 0.2f, () => ui.TextField());
                ui.Text("Times must be of the format HH:MM:SS");
                ui.Spacer(12f);
                subVideo.LocalPressed += (IButton button, ButtonEventData eventData) =>
                {
                    __instance.StartTask(async () =>
                    {
                        if (!(IsValidTimeString(startTimeSubvideo.Text.Content.Value) && IsValidTimeString(endTimeSubvideo.Text.Content.Value)))
                        {
                            button.LabelText = __instance.GetLocalized("General.FAILED");
                            button.Enabled = false;
                            return;
                        }
                        
                        var result = await MediaManager.PrepareMedia(__instance, returnMediaType: MediaType.VIDEO, button);
                        if (!result.success) return;

                        var command = $"-ss {startTimeSubvideo.Text.Content.Value} -to {endTimeSubvideo.Text.Content.Value} -i {result.inputName} -c:v copy -c:a copy {result.convertedName}";
                        if (await FFMPEGWrapper.RunFFScript(FFMPEGInterface.FFPMEG, command, Config.GetValue(overwrite), Config.GetValue(dontCreateConsole)))
                        {
                            MediaManager.ImportMedia(__instance.Slot, MediaType.VIDEO, result.convertedName, button);
                        }
                    });
                };

                var mute = ui.Button("Mute the audio of a video file");
                ui.Spacer(12f);
                mute.LocalPressed += (IButton button, ButtonEventData eventData) =>
                {
                    __instance.StartTask(async () =>
                    {
                        var result = await MediaManager.PrepareMedia(__instance, MediaType.VIDEO, button);
                        if (!result.success) return;

                        var command = $"-i {result.inputName} -an {result.convertedName}";
                        if (await FFMPEGWrapper.RunFFScript(FFMPEGInterface.FFPMEG, command, Config.GetValue(overwrite), Config.GetValue(dontCreateConsole)))
                        {
                            MediaManager.ImportMedia(__instance.Slot, MediaType.VIDEO, result.convertedName, button);
                        }
                    });
                };

                var extract = ui.Button("Extract audio from video");
                ui.Spacer(12f);
                extract.LocalPressed += (IButton button, ButtonEventData eventData) =>
                {
                    __instance.StartTask(async () =>
                    {
                        var result = await MediaManager.PrepareMedia(__instance, returnMediaType: MediaType.AUDIO, button);
                        if (!result.success) return;

                        var command = $"-i {result.inputName} -vn {result.convertedName}";
                        if (await FFMPEGWrapper.RunFFScript(FFMPEGInterface.FFPMEG, command, Config.GetValue(overwrite), Config.GetValue(dontCreateConsole)))
                        {
                            MediaManager.ImportMedia(__instance.Slot, MediaType.AUDIO, result.convertedName, button);
                        }
                    });
                };
            }
        }

        private static bool IsValidTimeString(string canidate)
        {
            return validTimes.IsMatch(canidate);
        }
    }
}