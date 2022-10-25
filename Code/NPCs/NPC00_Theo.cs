﻿using Celeste;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections;

namespace Celeste.Mod.XaphanHelper.NPCs
{
    [CustomEntity("XaphanHelper/NPC00_Theo")]
    public class NPC00_Theo : NPC
    {
        public string mode;

        private bool StartMoving;

        public bool playerHasCollectedOneGem() { return XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch1_Gem_Collected") || XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch2_Gem_Collected") || XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch3_Gem_Collected") || XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch4_Gem_Collected"); }

        protected XaphanModuleSettings Settings => XaphanModule.Settings;

        public NPC00_Theo(EntityData data, Vector2 position) : base(data.Position + position)
        {
            mode = data.Attr("mode");
            Add(Sprite = GFX.SpriteBank.Create("theo"));
            Sprite.Scale.X = -1f;
            Sprite.Play("idle");
            IdleAnim = "idle";
            MoveAnim = "walk";
            Maxspeed = 30f;
            Position += new Vector2(0, 8);
            if (mode =="gemRoomB")
            {
                Sprite.Play("sleep");
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (!Settings.SpeedrunMode)
            {
                if (mode == "start" && XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Upgrade_DashBoots"))
                {
                    RemoveSelf();
                }
                else if (mode == "gemRoom" && (!XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Upgrade_DashBoots") || playerHasCollectedOneGem()))
                {
                    RemoveSelf();
                }
                else if (mode == "gemRoomB" && !playerHasCollectedOneGem())
                {
                    RemoveSelf();
                }
            }
            else
            {
                if (mode == "start" && SceneAs<Level>().Session.GetFlag("Upgrade_DashBoots"))
                {
                    RemoveSelf();
                }
                else if (mode == "gemRoom" && !SceneAs<Level>().Session.GetFlag("Upgrade_DashBoots"))
                {
                    RemoveSelf();
                }
                else if (mode == "gemRoomB")
                {
                    RemoveSelf();
                }
            }
        }

        public override void Update()
        {
            base.Update();
            if (mode =="gemRoom" && XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch0_Gem_Room_B") && !StartMoving)
            {
                StartMoving = true;
                Add(new Coroutine(Searching()));
            }
        }

        public IEnumerator Searching()
        {
            var rand = new Random();
            int result = rand.Next(2);
            if (result == 0)
            {
                Sprite.Play("idle");
            }
            else
            {
                Sprite.Play("think");
            }
            yield return rand.Next(4) + 2;
            yield return MoveTo(new Vector2(X - 40, Y));
            result = rand.Next(2);
            if (result == 0)
            {
                Sprite.Play("idle");
            }
            else
            {
                Sprite.Play("think");
            }
            yield return rand.Next(4) + 2;
            yield return MoveTo(new Vector2(X + 40, Y));
            Add(new Coroutine(Searching()));
        }
    }
}
