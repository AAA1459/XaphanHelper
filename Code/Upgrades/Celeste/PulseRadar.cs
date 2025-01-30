﻿using System.Collections;
using Celeste.Mod.XaphanHelper.Entities;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Upgrades
{
    class PulseRadar : Upgrade
    {
        Coroutine UsePulseRadarCoroutine = new();

        public static bool canUse = true;

        public override int GetDefaultValue()
        {
            return 0;
        }

        public override int GetValue()
        {
            return XaphanModule.ModSettings.PulseRadar ? 1 : 0;
        }

        public override void SetValue(int value)
        {
            XaphanModule.ModSettings.PulseRadar = (value != 0);
        }

        public override void Load()
        {
            On.Celeste.Level.Update += modLevelUpdate;
            On.Celeste.Player.Die += onPlayerDie;
        }

        private PlayerDeadBody onPlayerDie(On.Celeste.Player.orig_Die orig, Player self, Vector2 direction, bool evenIfInvincible, bool registerDeathInStats)
        {
            if (UsePulseRadarCoroutine.Active)
            {
                UsePulseRadarCoroutine.Cancel();
            }
            return orig(self, direction, evenIfInvincible, registerDeathInStats);
        }

        public override void Unload()
        {
            On.Celeste.Level.Update -= modLevelUpdate;
            On.Celeste.Player.Die -= onPlayerDie;
        }

        public static bool Active(Level level)
        {
            return XaphanModule.ModSettings.PulseRadar && !XaphanModule.ModSaveData.PulseRadarInactive.Contains(level.Session.Area.LevelSet);
        }

        public static bool isActive;

        private void modLevelUpdate(On.Celeste.Level.orig_Update orig, Level self)
        {
            orig(self);
            if (XaphanModule.useUpgrades)
            {
                if (Active(self))
                {
                    isActive = true;
                }
                else
                {
                    isActive = false;
                }
                if (isActive)
                {
                    Player player = self.Tracker.GetEntity<Player>();
                    if (player != null)
                    {
                        int radar = self.Tracker.GetEntities<Radar>().Count;
                        canUse = self.Tracker.GetEntities<RadarTile>().Count == 0 && radar == 0;
                    }
                    if (self.CanPause && !XaphanModule.PlayerIsControllingRemoteDrone() && player != null && player.StateMachine.State == Player.StNormal && !player.Ducking && !self.Session.GetFlag("In_bossfight") && XaphanModule.ModSettings.UseMiscItemSlot.Pressed && !XaphanModule.ModSettings.UseBagItemSlot.Pressed && !XaphanModule.ModSettings.OpenMap.Check && !XaphanModule.ModSettings.SelectItem.Check && !self.Session.GetFlag("Map_Opened") && player.Holding == null)
                    {
                        BagDisplay bagDisplay = GetDisplay(self, "misc");
                        if (bagDisplay != null)
                        {
                            if (bagDisplay.currentSelection == 3)
                            {
                                int radar = self.Tracker.GetEntities<Radar>().Count;
                                if (self.Tracker.GetEntities<RadarTile>().Count == 0 && radar == 0)
                                {
                                    UsePulseRadarCoroutine = new Coroutine(UsePulseRadar(player, self));
                                }
                            }
                        }
                    }
                    if (UsePulseRadarCoroutine != null)
                    {
                        UsePulseRadarCoroutine.Update();
                    }
                }
            }
        }

        private IEnumerator UsePulseRadar(Player player, Level level)
        {
            bool usedPulseRadar = false;
            float leniency = 0.5f;
            while (XaphanModule.ModSettings.UseMiscItemSlot.Check && !usedPulseRadar)
            {
                while (player.Dead && leniency > 0)
                {
                    leniency -= Engine.DeltaTime;
                    yield return null;
                }
                if (leniency <= 0)
                {
                    yield break;
                }
                if (player.Scene != null && !player.Dead && !player.DashAttacking && player.StateMachine.State != Player.StClimb)
                {
                    level.Add(new Radar(player.Position));
                    usedPulseRadar = true;
                }
                yield return null;
            }
        }
    }
}
