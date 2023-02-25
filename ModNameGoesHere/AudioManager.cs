using BaseX;
using CodeX;
using FFMpegCore;
using FrooxEngine;
using FrooxEngine.LogiX.WorldModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FFMpegNeos
{
    public static class AudioManager
    {
        public static VideoTextureProvider Instance = null;

        internal static async void ExtractAudio(IButton button, ButtonEventData eventData)
        {
            await default(ToBackground);
            if (Instance is null) return;

            UniLog.Log("1");
            var result = await Instance.StartTask(() => Utils.SaveVideo(Instance)).ConfigureAwait(false);
            UniLog.Log("2");

            if (result.success)
            {
                UniLog.Log("4");
                var target = Path.Combine(
                    Engine.Current.CachePath,
                    "Cache",
                    Path.GetFileNameWithoutExtension(result.path) + ".wav");
                UniLog.Log(result.path);
                UniLog.Log(target);
                UniLog.Log(GlobalFFOptions.GetFFProbeBinaryPath());
                UniLog.Log(GlobalFFOptions.GetFFMpegBinaryPath());
                UniLog.Log(GlobalFFOptions.Current.WorkingDirectory);
                UniLog.Log(GlobalFFOptions.Current.BinaryFolder);
                UniLog.Log(GlobalFFOptions.Current.TemporaryFilesFolder);
                
                UniLog.Log("5");
                if (!FFMpeg.ExtractAudio(result.path, target))
                {
                    UniLog.Log("Failed to convert audio file!");
                    return;
                }

                UniLog.Log("6");
                await default(ToWorld);
                var slot = Engine.Current.WorldManager.FocusedWorld.AddSlot("temp");
                slot.PositionInFrontOfUser();

                UniLog.Log("7");
                UniversalImporter.Import(
                    AssetClass.Audio,
                    new List<string>() { target },
                    Instance.World,
                    slot.GlobalPosition,
                    slot.GlobalRotation);

                UniLog.Log("8");
                slot.Destroy();
            }
            else
            {
                await default(ToWorld);
                throw new Exception("Failed to save video");
            }

            Instance = null;
        }

        internal static ButtonEventHandler MuteAudio(VideoTextureProvider instance)
        {
            throw new NotImplementedException();
        }
    }
}
