using FFMpegCore;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using NeosModLoader;
using System;
using System.IO;

namespace FFMpegNeos
{
    public class FFMpegNeos : NeosMod
    {
        public override string Name => "FFMpegNeos";
        public override string Author => "dfgHiatus";
        public override string Version => "1.0.0";
        public override string Link => "https://github.com/dfgHiatus/FFMpegNeos/";

        private static ModConfiguration config;
        public static readonly string CachePath = Path.Combine(Engine.Current.CachePath, "Cache", "FFMPEG");

        public override void DefineConfiguration(ModConfigurationDefinitionBuilder builder)
        {
            builder
                .Version(new Version(1, 0, 0))
                .AutoSave(true);
        }

        public override void OnEngineInit()
        {
            config = GetConfiguration();
            Harmony harmony = new Harmony("net.dfgHiatus.FFMpegNeos");
            harmony.PatchAll();
            Initialize();
        }

        private static void Initialize()
        {
            Directory.CreateDirectory(CachePath);
            GlobalFFOptions.Configure(new FFOptions { 
                BinaryFolder = "nml_mods/ffmpeg", 
                TemporaryFilesFolder = Path.GetTempPath() });

        }
		
        [HarmonyPatch(typeof(VideoTextureProvider), "BuildInspectorUI", typeof(UIBuilder))]
        class VideoTextureProviderPatch
        {
            public static void Postfix(VideoTextureProvider __instance, UIBuilder ui)
            {
                //LocaleString text = "Extract snapshot from Video";
                //var snapShot = ui.Button(in text);
                //snapShot.LocalPressed += VideoManager.ExtractSnapshot(__instance);

                // Requires a custom editor
                // text = "Join video clips together";
                // ui.Button(in text, ExtractAudioFromVideo.Extract(__instance));

                // Requires a custom editor
                // text = "Join images into video";
                // ui.Button(in text, ExtractAudioFromVideo.Extract(__instance));

                // text = "Join images into a video:";
                // ui.Button(in text, ExtractAudioFromVideo.Extract(__instance));

                //text = "Create a sub-video";
                //var subVideo = ui.Button(in text);
                //subVideo.LocalPressed += VideoManager.ExtractSubVideo(__instance);

                //text = "Mute the audio of a video file:";
                //var mute = ui.Button(in text);
                //mute.LocalPressed += AudioManager.MuteAudio(__instance);

                LocaleString text = "Extract audio from video";
                var extract = ui.Button(in text);
                AudioManager.Instance = __instance;
                extract.LocalPressed += AudioManager.ExtractAudio;
            }
        }
    }
}