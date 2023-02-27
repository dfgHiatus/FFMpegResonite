using BaseX;
using CodeX;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using Instances;
using NeosModLoader;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

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
                // Requires a custom editor
                // text = "Join video clips together";
                // ui.Button(in text, ExtractAudioFromVideo.Extract(__instance));

                // Requires a custom editor
                // text = "Join images into video";
                // ui.Button(in text, ExtractAudioFromVideo.Extract(__instance));

                // Requires a custom editor
                // text = "Join images into a video:";
                // ui.Button(in text, ExtractAudioFromVideo.Extract(__instance));

                //LocaleString text = "Extract snapshot from Video";
                //var snapShot = ui.Button(in text);
                //snapShot.LocalPressed += VideoManager.ExtractSnapshot();

                //text = "Create a sub-video";
                //var subVideo = ui.Button(in text);
                //subVideo.LocalPressed += VideoManager.ExtractSubVideo();
                
                LocaleString text = "Mute the audio of a video file";
                var mute = ui.Button(in text);
                mute.LocalPressed += (IButton button, ButtonEventData eventData) =>
                {
                    __instance.StartTask(async () =>
                    {
                        var result = await MediaManager.PrepareMedia(__instance, shouldReturnVideo: true);
                        if (!result.success) return;

                        var command = $"-i {result.inputName} -an {result.convertedName}";
                        if (await FFMPEGWrapper.RunFFScript(FFMPEGInterface.FFPMEG, command, Config.GetValue(overwrite), Config.GetValue(dontCreateConsole)))
                        {
                            MediaManager.ImportMedia(__instance, isVideo: true, result.convertedName);
                        }
                    });
                };

                text = "Extract audio from video";
                var extract = ui.Button(in text);
                extract.LocalPressed += (IButton button, ButtonEventData eventData) =>
                {
                    __instance.StartTask (async () =>
                    {
                        var result = await MediaManager.PrepareMedia(__instance, shouldReturnVideo: false);
                        if (!result.success) return;

                        var command = $"-i {result.inputName} -vn {result.convertedName}";
                        if (await FFMPEGWrapper.RunFFScript(FFMPEGInterface.FFPMEG, command, Config.GetValue(overwrite), Config.GetValue(dontCreateConsole)))
                        {
                            MediaManager.ImportMedia(__instance, isVideo: false, result.convertedName);
                        }
                    });
                };
            }
        }
    }
}