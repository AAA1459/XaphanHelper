﻿using System;
using System.Collections;
using Celeste.Mod.XaphanHelper.Controllers;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class CS00_GemRoomC : CutsceneEntity
    {
        private readonly Player player;

        private GemController gemController;

        public CS00_GemRoomC(Player player)
        {
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            if (XaphanModule.ModSettings.AutoSkipCutscenes)
            {
                EndCutscene(Level);
                WasSkipped = true;
            }
            else
            {
                gemController = Scene.Entities.FindFirst<GemController>();
                Add(new Coroutine(Cutscene(level)));
            }
        }

        public override void OnEnd(Level level)
        {
            if (WasSkipped)
            {
                level.Session.SetFlag("CS_Ch0_Gem_Room_Activeate_Gems");
            }
            if (XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_End_Area_Open"))
            {
                XaphanModule.ModSaveData.WatchedCutscenes.Add("Xaphan/0_Ch0_Gem_Room_C");
                level.Session.SetFlag("CS_Ch0_Gem_Room_C");
            }
            player.StateMachine.State = 0;
        }

        public IEnumerator Cutscene(Level level)
        {
            player.StateMachine.State = 11;
            if (!XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch0_Gem_Room_First_Time"))
            {
                player.Facing = Facings.Right;
                XaphanModule.ModSaveData.SavedFlags.Add("Xaphan/0_Ch0_Gem_Room_First_Time");
                yield return Textbox.Say("Xaphan_Ch0_A_Gem_Room_C");
            }
            yield return player.DummyWalkToExact((int)gemController.X);
            yield return gemController.ActivateGems();
            int missingGems = 0;
            for (int i = 1; i <= (XaphanModule.SoCMVersion >= new Version(3, 0, 0) ? 5 : 4); i++)
            {
                if (!XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch" + i + "_Gem_Sloted"))
                {
                    missingGems++;
                }
            }
            if (!XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch1_Gem2_Sloted"))
            {
                missingGems++;
            }
            if (missingGems > 0)
            {
                yield return Textbox.Say("Xaphan_Ch0_A_Gem_Room_C_b_" + missingGems);
            }
            EndCutscene(Level);
        }
    }
}
