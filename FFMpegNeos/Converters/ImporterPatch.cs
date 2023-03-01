using BaseX;
using CodeX;
using FrooxEngine;
using HarmonyLib;
using System.Collections.Generic;
using System.IO;

namespace FFMPEGNeos.Converters;

[HarmonyPatch(typeof(UniversalImporter), "Import", typeof(AssetClass), typeof(IEnumerable<string>),
    typeof(World), typeof(float3), typeof(floatQ), typeof(bool))]
public class ImporterPatch
{
    const string MP3 = ".mp3";
    const string GIF = ".gif";

    static bool Prefix(ref IEnumerable<string> files, World world, float3 position, floatQ rotation)
    {
        List<string> gifs = new();
        List<string> mp3s = new();
        List<string> others = new();

        foreach (var file in files)
        {
            switch (Path.GetExtension(file).ToLower())
            {
                case MP3:
                    mp3s.Add(file);
                    break;
                case GIF:
                    gifs.Add(file);
                    break;
                default:
                    others.Add(file);
                    break;
            }
        }

        if (mp3s.Count > 0) MP3Converter.ConvertAndImport(mp3s, world, position, rotation);
        if (gifs.Count > 0) GIFConverter.ConvertAndImport(gifs, world, position, rotation);
        if (others.Count <= 0) return false;
        
        files = others;
        return true;
    }
}

