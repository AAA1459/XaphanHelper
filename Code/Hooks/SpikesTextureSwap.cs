﻿using System.Reflection;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Hooks
{
    internal class SpikesTextureSwap
    {
        private static FieldInfo SpikesOverrideType = typeof(Spikes).GetField("overrideType", BindingFlags.Instance | BindingFlags.NonPublic);

        private static FieldInfo SpikesSize = typeof(Spikes).GetField("size", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void Load()
        {
            On.Celeste.Spikes.Added += onSpikesAdded;
        }

        public static void Unload()
        {
            On.Celeste.Spikes.Added -= onSpikesAdded;
        }

        private static void onSpikesAdded(On.Celeste.Spikes.orig_Added orig, Spikes self, Scene scene)
        {
            if (SaveData.Instance.LevelSetStats.Name == "Xaphan/0" && XaphanModule.ModSaveData.GlobalFlags.Contains("Xaphan/0_Ch4_Escape_Complete") && SaveData.Instance.CurrentSession.Area.ChapterIndex == 5)
            {
                string oldType = (string)SpikesOverrideType.GetValue(self);
                if (oldType == "Xaphan/terminal")
                {
                    SpikesOverrideType.SetValue(self, oldType + "_damaged");
                }
            }
            orig(self, scene);
        }
    }
}
