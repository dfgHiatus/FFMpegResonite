using Elements.Assets;
using FrooxEngine;
using FrooxEngine.UIX;
using HarmonyLib;
using ResoniteModLoader;
using System;
using System.IO;
using System.Text.RegularExpressions;
using static FFMPEGResonite.MediaManager;

namespace FFMPEGResonite;

public sealed class FFMPEGResonite : ResoniteMod
{
    public override string Name => "FFMpegResonite";
    public override string Author => "dfgHiatus, Fro Zen";
    public override string Version => "2.0.0";
    public override string Link => "https://github.com/dfgHiatus/FFMpegResonite/";

    internal static readonly string CachePath = Engine.Current.Platform == Platform.Windows
        ? Path.GetFullPath(Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Temp", 
                "Yellow Dog Man Studios", 
                "Resonite", 
                "Cache", 
                "FFMPEG"))
        : Path.GetFullPath(
            Path.Combine(Path.GetTempPath(), 
                "Yellow Dog Man Studios", 
                "Resonite", 
                "Cache", 
                "FFMPEG"));
    
    internal static ModConfiguration Config;
    private const string InvalidTime = "Invalid time was provided!";
    private const string InvalidTimeRange = "Invalid time range was provided!";
    private const string InvalidFrameRate = "Invalid frame rate modifier was provided!";

    private static readonly Regex ValidTimes = new(@"[0-9]{2}:[0-9]{2}:[0-9]{2}(\.[0-9]{1,3})?");
    private static readonly Regex ValidFraction = new(@"[0-9]+/[0-9]+");
    
    [AutoRegisterConfigKey]
    public readonly static ModConfigurationKey<bool> Overwrite = 
        new("overwrite", "Overwrite files", () => true);

    [AutoRegisterConfigKey]
    public readonly static ModConfigurationKey<bool> ImportRawFiles =
        new("importRawFiles", "Import raw versions of converted files", () => true);

    [AutoRegisterConfigKey]
    public readonly static ModConfigurationKey<bool> DontCreateConsole =
        new("dontCreateConsole", "Hide FFmpeg output", () => true);

    [AutoRegisterConfigKey]
    public readonly static ModConfigurationKey<string> PreferredVideoFormat = 
        new("preferredVideoFormat", "Preferred video format", () => "mp4");

    [AutoRegisterConfigKey]
    public readonly static ModConfigurationKey<string> PreferredAudioFormat = 
        new("preferredAudioFormat", "Preferred audio format", () => "ogg");

    [AutoRegisterConfigKey]
    public readonly static ModConfigurationKey<string> PreferredImageFormat = 
        new("preferredImageFormat", "Preferred texture format", () => "jpg");

    [AutoRegisterConfigKey]
    public readonly static ModConfigurationKey<int> PreferredImageQuality = 
        new("preferredImageQuality", "Preferred image quality. Don't touch this unless you know what you're doing",
            () => 2);

    public override void OnEngineInit()
    {
        if (!Initialize()) return;
        Config = GetConfiguration();
        new Harmony("net.dfgHiatus.FFMPEGResonite").PatchAll();
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

        // For @IDEAs that need their own UI to be loaded
        //Engine.Current.RunPostInit(() => {
        //    DevCreateNewForm.AddAction("Editor", "Photo Manager", MediaManagerUI.Spawn);
        //});

        return true;
    }
    

    [HarmonyPatch(typeof(VideoTextureProvider), "BuildInspectorUI", typeof(UIBuilder))]
    public class VideoTextureProviderPatch
    {
        // We can't pass an instance into LocalPressed events, lambdas all the way!
        public static void Postfix(VideoTextureProvider __instance, UIBuilder ui)
        {
            ui.Spacer(12f);
            ui.Text("FFmpeg Utilities");
            ui.Text("Input times must be of the format HH:MM:SS");
            ui.Spacer(12f);

            // @IDEA
            // text = "Join video clips together";
            // ui.Button(in text, ExtractAudioFromVideo.Extract(__instance));

            // @IDEA
            // text = "Join images into video";
            // ui.Button(in text, ExtractAudioFromVideo.Extract(__instance));

            var snapshotTime = ui.HorizontalElementWithLabel("Time", 0.5f, () => ui.TextField("00:00:00"));
            var snapShot = ui.Button("Extract a snapshot from the video with the given time");
            ui.Spacer(12f);
            snapShot.LocalPressed += (button, _) =>
            {
                if (!IsValidTime(snapshotTime.Text.Content.Value))
                {
                    button.LabelText = InvalidTime;
                    button.Enabled = false;
                    return;
                }

                __instance.StartTask(async () =>
                {
                    var result = await PrepareVideo(__instance, returnAssetClass: AssetClass.Texture, button);
                    if (!result.success) return;

                    var command = $"-ss {snapshotTime.Text.Content.Value} -i {result.inputName} -vframes 1 -q:v {Config.GetValue(PreferredImageQuality)} {result.convertedName}";
                    if (await FFMPEGWrapper.RunFFScript(FFMPEGInterface.FFPMEG, command, Config.GetValue(Overwrite), Config.GetValue(DontCreateConsole)))
                    {
                        ImportMedia(__instance.Slot, AssetClass.Texture, result.convertedName, button);
                    }
                });
            };

            var startTimeFrames = ui.HorizontalElementWithLabel("Start", 0.5f, () => ui.TextField("00:00:00"));
            var endTimeFrames = ui.HorizontalElementWithLabel("End", 0.5f, () => ui.TextField("00:00:00"));
            var optionalFrameRate = ui.HorizontalElementWithLabel("Optional frame rate modifier", 0.5f, () => ui.TextField());
            ui.Text("Framerate modifer must be a fraction seperated by a slash, IE 1/10");
            var frames = ui.Button("Extract all images from time span");
            ui.Spacer(12f);
            frames.LocalPressed += (button, _) =>
            {
                if (!IsValidTimeRange(startTimeFrames.Text.Content.Value, endTimeFrames.Text.Content.Value))
                {
                    button.LabelText = InvalidTimeRange;
                    button.Enabled = false;
                    return;
                }

                if (!string.IsNullOrEmpty(optionalFrameRate.Text.Content.Value))
                {
                    if (!IsValidFrameRate(optionalFrameRate.Text.Content.Value))
                    {
                        button.LabelText = InvalidFrameRate;
                        button.Enabled = false;
                        return;
                    }
                }

                __instance.StartTask(async () =>
                {
                    var result = await PrepareVideo(__instance, returnAssetClass: AssetClass.Texture, button);
                    if (!result.success) return;

                    var dir = Path.Combine(CachePath, "range");
                    if (Directory.Exists(dir))
                        Directory.Delete(dir);

                    // TODO find a way to cache these?
                    Directory.CreateDirectory(dir);

                    var command = string.IsNullOrEmpty(optionalFrameRate.Text.Content.Value)
                        ? $"-ss {startTimeFrames.Text.Content.Value} -to {endTimeFrames.Text.Content.Value} -i {result.inputName} -q:v {Config.GetValue(PreferredImageQuality)} {Path.Combine(dir, "output_%03d.jpg")}"
                        : $"-ss {startTimeFrames.Text.Content.Value} -to {endTimeFrames.Text.Content.Value} -i {result.inputName} -q:v {Config.GetValue(PreferredImageQuality)} -vf fps={optionalFrameRate.Text.Content.Value} {Path.Combine(dir, "output_%03d.jpg")}";

                    if (await FFMPEGWrapper.RunFFScript(FFMPEGInterface.FFPMEG, command, Config.GetValue(Overwrite),
                            Config.GetValue(DontCreateConsole)))
                        ImportMedia(__instance.Slot, AssetClass.Texture, Directory.GetFiles(dir), button);
                });
            };

            var startTimeSubvideo = ui.HorizontalElementWithLabel("Start", 0.5f, () => ui.TextField("00:00:00"));
            var endTimeSubvideo = ui.HorizontalElementWithLabel("End", 0.5f, () => ui.TextField("00:00:00"));
            var subVideo = ui.Button("Create a sub-video with the following start and end times");
            ui.Spacer(12f);
            subVideo.LocalPressed += (button, _) =>
            {
                __instance.StartTask(async () =>
                {
                    if (!IsValidTimeRange(startTimeSubvideo.Text.Content.Value, endTimeSubvideo.Text.Content.Value))
                    {
                        button.LabelText = InvalidTimeRange;
                        button.Enabled = false;
                        return;
                    }
                    
                    var result = await PrepareVideo(__instance, returnAssetClass: AssetClass.Video, button);
                    if (!result.success) return;

                    var command = $"-ss {startTimeSubvideo.Text.Content.Value} -to {endTimeSubvideo.Text.Content.Value} -i {result.inputName} -c:v copy -c:a copy {result.convertedName}";
                    if (await FFMPEGWrapper.RunFFScript(FFMPEGInterface.FFPMEG, command, Config.GetValue(Overwrite), Config.GetValue(DontCreateConsole)))
                    {
                        ImportMedia(__instance.Slot, AssetClass.Video, result.convertedName, button);
                    }
                });
            };

            var mute = ui.Button("Mute audio in video");
            mute.LocalPressed += (button, _) =>
            {
                __instance.StartTask(async () =>
                {
                    var result = await PrepareVideo(__instance, AssetClass.Video, button);
                    if (!result.success) return;

                    var command = $"-i {result.inputName} -an {result.convertedName}";
                    if (await FFMPEGWrapper.RunFFScript(FFMPEGInterface.FFPMEG, command, Config.GetValue(Overwrite), Config.GetValue(DontCreateConsole)))
                    {
                        ImportMedia(__instance.Slot, AssetClass.Video, result.convertedName, button);
                    }
                });
            };

            var extract = ui.Button("Extract audio from video");
            extract.LocalPressed += (button, _) =>
            {
                __instance.StartTask(async () =>
                {
                    var result = await PrepareVideo(__instance, returnAssetClass: AssetClass.Audio, button);
                    if (!result.success) return;

                    var command = $"-i {result.inputName} -vn {result.convertedName}";
                    if (await FFMPEGWrapper.RunFFScript(FFMPEGInterface.FFPMEG, command, Config.GetValue(Overwrite),
                            Config.GetValue(DontCreateConsole)))
                        ImportMedia(__instance.Slot, AssetClass.Audio, result.convertedName, button);
                });
            };

            ui.Spacer(12f);
        }
    }

    private static bool IsValidTime(string candidate) => ValidTimes.IsMatch(candidate) && candidate.Length == 8;

    private static bool IsValidTimeRange(string start, string end)
    {
        if (!(IsValidTime(start) && IsValidTime(end)))
            return false;

        return DateTime.Parse(start) <= DateTime.Parse(end);
    }

    private static bool IsValidFrameRate(string candidate) => ValidFraction.IsMatch(candidate);
}