using FrooxEngine;
using System;
using System.IO;
using System.Linq;

namespace FFMPEGResonite;

internal class FFMPEGInterface
{
    private static string _linuxFfMpegCache;
    private static string _linuxFfProbeCache;
    private static string _linuxFfPlayCache;

    private static string GetLinuxPath(string command, ref string cache)
    {
        // Most linux distros provide ffmpeg as a package, so it's better to look for it instead of
        // Embedding it
        if (string.IsNullOrWhiteSpace(cache))
        {
            var path = Environment.GetEnvironmentVariable("PATH") ?? "";
            var paths = path.Split(Path.PathSeparator).ToList();
            var test = paths.Select(i => Path.Combine(i, command)).FirstOrDefault(File.Exists);
            cache = string.IsNullOrWhiteSpace(test) ? "notfound" : test;
        }
        return cache;
    }

    public static string FFPMEG
    {
        get
        {
            return Engine.Current.Platform switch
            {
                Platform.Windows => Path.Combine(Engine.Current.AppPath, "rml_mods", "ffmpeg", "ffmpeg.exe"),
                Platform.Linux => GetLinuxPath("ffmpeg", ref _linuxFfMpegCache),
                _ => throw new NotImplementedException(),
            };
        }
    }

    public static string FFPROBE
    {
        get
        {
            return Engine.Current.Platform switch
            {
                Platform.Windows => Path.Combine(Engine.Current.AppPath, "rml_mods", "ffmpeg", "ffprobe.exe"),
                Platform.Linux => GetLinuxPath("ffprobe", ref _linuxFfProbeCache),
                _ => throw new NotImplementedException(),
            };
        }
    }

    public static string FFPLAY
    {
        get
        {
            return Engine.Current.Platform switch
            {
                Platform.Windows => Path.Combine(Engine.Current.AppPath, "rml_mods", "ffmpeg", "ffplay.exe"),
                Platform.Linux => GetLinuxPath("ffplay", ref _linuxFfPlayCache),
                _ => throw new NotImplementedException(),
            };
        }
    }
}