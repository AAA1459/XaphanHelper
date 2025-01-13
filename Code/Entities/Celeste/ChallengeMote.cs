﻿using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/ChallengeMote")]
    class ChallengeMote : Entity
    {
        public ParticleType P_Fire;

        private TalkComponent talk;

        private Level level;

        private TextMenu menu;

        private Sprite sprite = new(GFX.Game, "objects/XaphanHelper/ChallengeMote/");

        private Vector2 BerryPos;

        private Vector2 origBerryPos;

        private Strawberry strawberry;

        private CustomCollectable heart;

        private bool ShowHeart;

        private bool Started;

        private int ChapterIndex;

        private bool BerryAppeared;

        private Coroutine BerryRoutine = new();

        private bool AllowsSpaceJump;

        private bool AllowsBombs;

        private bool AllowsSpiderMagnet;

        private bool AllowsRemoteDrone;

        public bool SpaceJumpCollected()
        {
            return XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Upgrade_SpaceJump");
        }

        public bool BombsCollected()
        {
            return XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Upgrade_Bombs");
        }

        public bool SpiderMagnetCollected()
        {
            return XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Upgrade_SpiderMagnet");
        }

        public bool RemoteDroneCollected()
        {
            return XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Upgrade_RemoteDrone");
        }

        public bool LightningDashCollected()
        {
            return XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Upgrade_LightningDash");
        }

        public bool GravityJacketCollected()
        {
            return XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Upgrade_GravityJacket");
        }

        public ChallengeMote(EntityData data, Vector2 position, EntityID ID) : base(data.Position + position)
        {
            AllowsSpaceJump = data.Bool("allowsSpaceJump", false);
            AllowsBombs = data.Bool("allowsBombs", false);
            AllowsSpiderMagnet = data.Bool("allowsSpiderMagnet", false);
            AllowsRemoteDrone = data.Bool("allowsRemoteDrone", false);
            Tag = Tags.TransitionUpdate;
            BerryPos = Position + new Vector2(0, -48);
            P_Fire = new ParticleType
            {
                Source = GFX.Game["particles/fire"],
                Color = Calc.HexToColor("DDB935"),
                Color2 = Calc.HexToColor("E0372F"),
                ColorMode = ParticleType.ColorModes.Fade,
                FadeMode = ParticleType.FadeModes.Late,
                Acceleration = new Vector2(0f, -40f),
                LifeMin = 0.8f,
                LifeMax = 1.2f,
                Size = 0.5f,
                SizeRange = 0.4f,
                Direction = -(float)Math.PI / 2f,
                DirectionRange = (float)Math.PI / 6f,
                SpeedMin = 12f,
                SpeedMax = 10f,
                SpeedMultiplier = 0.2f,
                ScaleOut = true,
            };
            sprite.AddLoop("idle", "idle", 0.16f);
            sprite.Add("completed", "completed", 0.08f);
            sprite.Add("completedEnd", "completed", 0f, 9);
            sprite.CenterOrigin();
            sprite.Justify = new Vector2(0.5f, 0.5f);
            sprite.Play("idle");
            Add(sprite);
            Depth = 1000;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            level = SceneAs<Level>();
            string Prefix = SceneAs<Level>().Session.Area.LevelSet;
            ChapterIndex = SceneAs<Level>().Session.Area.ChapterIndex;
            Add(talk = new TalkComponent(new Rectangle(-12, 8, 24, 8), new Vector2(0f, -12f), Interact));
            talk.Enabled = false;
            Visible = false;
        }

        public override void Update()
        {
            base.Update();
            string Prefix = SceneAs<Level>().Session.Area.LevelSet;
            int chapterIndex = SceneAs<Level>().Session.Area.ChapterIndex == -1 ? 0 : SceneAs<Level>().Session.Area.ChapterIndex;
            if (XaphanModule.SoCMVersion >= new Version(3, 0, 0))
            {
                if (heart == null)
                {
                    heart = level.Entities.FindFirst<CustomCollectable>();
                }
                else
                {
                    BerryPos = heart.Center;
                    if (!ShowHeart)
                    {
                        heart.Visible = heart.Collidable = false;
                    }
                }
            }
            else
            {
                if (strawberry == null)
                {
                    strawberry = level.Entities.FindFirst<Strawberry>();
                    if (strawberry != null)
                    {
                        origBerryPos = strawberry.Position;
                    }
                }
            }
            if (XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + ChapterIndex + "_Boss_Defeated"))
            {
                if (!level.Session.GetFlag("boss_Normal_Mode") && !level.Session.GetFlag("boss_Challenge_Mode"))
                {
                    Visible = true;
                    talk.Enabled = !BerryRoutine.Active;
                    talk.PlayerMustBeFacing = false;
                    if (Scene.OnInterval(0.03f))
                    {
                        Vector2 position = Position + new Vector2(0f, 1f) + Calc.AngleToVector(Calc.Random.NextAngle(), 5f);
                        level.ParticlesBG.Emit(P_Fire, position + new Vector2(0, -3f));
                    }
                    if ((XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + ChapterIndex + "_Boss_Defeated_CM") || level.Session.GetFlag("Boss_Defeated_CM")) && (XaphanModule.SoCMVersion < new Version(3, 0, 0) ? strawberry != null : heart != null) && !BerryAppeared)
                    {
                        Visible = talk.Enabled = false;
                        BerryAppeared = true;
                        level.Displacement.AddBurst(Center, 0.5f, 8f, 32f, 0.5f);
                        sprite.Play("completed");
                        sprite.OnLastFrame = onLastFrame;
                    }
                    else if ((XaphanModule.SoCMVersion < new Version(3, 0, 0) && strawberry == null) || XaphanModule.SoCMVersion >= new Version(3, 0, 0) && heart == null)
                    {
                        sprite.Play("completedEnd");
                    }
                    if (Started)
                    {
                        level.Displacement.AddBurst(Center, 0.5f, 8f, 32f, 0.5f);
                        Started = false;
                    }
                }
                else
                {
                    if (XaphanModule.SoCMVersion >= new Version(3, 0, 0))
                    {
                        if (heart != null && heart.Visible)
                        {
                            level.Displacement.AddBurst(heart.Center, 0.5f, 8f, 32f, 0.5f);
                            heart.Visible = heart.Collidable = false;
                            BerryAppeared = false;
                        }
                    }
                    else
                    {
                        if (strawberry != null && strawberry.Position == BerryPos)
                        {
                            level.Displacement.AddBurst(strawberry.Center, 0.5f, 8f, 32f, 0.5f);
                            strawberry.Position = origBerryPos;
                            BerryAppeared = false;
                        }
                    }
                    Visible = talk.Enabled = false;
                    Started = true;
                    ManageUpgrades(level, false);
                }
            }
        }

        private void onLastFrame(string s)
        {
            Add(BerryRoutine = new Coroutine(BerryApear()));
        }

        private void Interact(Player player)
        {
            talk.Enabled = false;
            level.Session.SetFlag("Boss_Appeared", true);
            Add(new Coroutine(Routine(player)));
        }

        public IEnumerator BerryApear()
        {
            Audio.Play("event:/game/06_reflection/supersecret_heartappear");
            Entity dummy = new(BerryPos)
            {
                Depth = 1
            };
            Scene.Add(dummy);
            AreaKey area = level.Session.Area;
            MapData MapData = AreaData.Areas[area.ID].Mode[(int)area.Mode].MapData;
            Image white = null;
            if (XaphanModule.SoCMVersion >= new Version(3, 0, 0))
            {
                white = new Image(GFX.Game["collectables/heartGem/white00"]);
            }
            else
            {
                if (SaveData.Instance.CheckStrawberry(MapData.Area, strawberry.ID))
                {
                    white = new Image(GFX.Game["collectables/ghostberry/idle00"]);
                }
                else
                {
                    white = new Image(GFX.Game["collectables/strawberry/normal00"]);
                }
            }
            white.CenterOrigin();
            white.Scale = Vector2.Zero;
            dummy.Add(white);
            BloomPoint glow = new(0f, 16f);
            dummy.Add(glow);
            List<Entity> absorbs = new();
            for (int i = 0; i < 20; i++)
            {
                AbsorbOrb orb = new(Position, dummy);
                Scene.Add(orb);
                absorbs.Add(orb);
                yield return null;
            }
            yield return 0.8f;
            float duration = 0.6f;
            for (float p = 0f; p < 1f; p += Engine.DeltaTime / duration)
            {
                white.Scale = Vector2.One * p * 1.2f;
                glow.Alpha = p;
                (Scene as Level).Shake();
                yield return null;
            }
            foreach (Entity orb2 in absorbs)
            {
                orb2.RemoveSelf();
            }
            (Scene as Level).Flash(Color.White);
            if (XaphanModule.SoCMVersion < new Version(3, 0, 0))
            {
                strawberry.Position = dummy.Position;
            }
            else
            {
                ShowHeart = true;
                heart.Visible = heart.Collidable = true;
                level.Displacement.AddBurst(heart.Center, 0.5f, 8f, 32f, 0.5f);
            }
            Scene.Remove(dummy);
        }

        public IEnumerator Routine(Player player)
        {
            level.PauseLock = true;
            player.StateMachine.State = 11;
            yield return player.DummyWalkToExact((int)X, false, 1f, true);
            player.Facing = player.Center.X <= level.Bounds.Left + level.Bounds.Width / 2 ? Facings.Right : Facings.Left;
            Audio.Play("event:/ui/game/pause");
            level.FormationBackdrop.Display = true;
            level.FormationBackdrop.Alpha = 0.5f;
            menu = new TextMenu();
            menu.AutoScroll = false;
            menu.Position = new Vector2(Engine.Width / 2f, Engine.Height / 2f);
            menu.Add(new TextMenu.Header(Dialog.Clean("XaphanHelper_UI_ActiveCM_title")));
            menu.Add(new TextMenu.SubHeader(Dialog.Clean("XaphanHelper_UI_CM_note1")));
            menu.Add(new TextMenu.SubHeader(Dialog.Clean("XaphanHelper_UI_CM_note2")));
            menu.Add(new TextMenu.SubHeader(Dialog.Clean("XaphanHelper_UI_CM_note3")));
            menu.Add(new TextMenu.SubHeader(""));
            menu.Add(new TextMenu.Button(Dialog.Clean("XaphanHelper_UI_Replay_Normal_Mode")).Pressed(delegate
            {
                menu.RemoveSelf();
                ManageUpgrades(level, false);
                level.Session.Audio.Music.Event = SFX.EventnameByHandle("event:/music/xaphan/lvl_0_tension");
                level.Session.Audio.Apply();
                level.Session.SetFlag("boss_Normal_Mode", true);
                level.Session.SetFlag("Boss_Defeated", false);
                ResetBoss();
                level.Displacement.AddBurst(Center, 0.5f, 8f, 32f, 0.5f);
                level.Session.RespawnPoint = level.GetSpawnPoint(Position);
            }));
            string Prefix = level.Session.Area.LevelSet;
            menu.Add(new TextMenu.Button(Dialog.Clean(XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + ChapterIndex + "_Boss_Defeated_CM") ? "XaphanHelper_UI_Replay_Challenge_Mode" : "XaphanHelper_UI_Play_Challenge_Mode")).Pressed(delegate
            {
                menu.RemoveSelf();
                ManageUpgrades(level, false);
                Audio.Play("event:/game/05_mirror_temple/room_lightlevel_down");
                level.Session.Audio.Music.Event = SFX.EventnameByHandle("event:/music/xaphan/lvl_0_tension");
                level.Session.Audio.Apply();
                level.Session.SetFlag("boss_Challenge_Mode", true);
                level.Session.SetFlag("Boss_Defeated", false);
                ResetBoss();
                level.Displacement.AddBurst(Center, 0.5f, 8f, 32f, 0.5f);
                level.Session.RespawnPoint = level.GetSpawnPoint(Position);
            }));
            menu.Add(new TextMenu.Button(Dialog.Clean("menu_return_cancel")).Pressed(delegate
            {
                menu.OnCancel();
            }));
            menu.OnCancel = delegate
            {
                Audio.Play("event:/ui/main/button_back");
                menu.RemoveSelf();
            };
            level.Add(menu);
            while (!Input.MenuConfirm.Pressed && !Input.MenuCancel.Pressed)
            {
                yield return null;
            }
            level.FormationBackdrop.Display = false;
            level.PauseLock = false;
            talk.Enabled = true;
            yield return 0.15f;
            player.StateMachine.State = 0;
        }

        private void ResetBoss()
        {
            if (ChapterIndex == 1 && level.Session.Level == "D-07")
            {
                Torizo boss = level.Tracker.GetEntity<Torizo>();
                boss.playerHasMoved = false;
                boss.SetHealth(15);

            }
            else if (ChapterIndex == 2 && level.Session.Level == (XaphanModule.SoCMVersion >= new Version(3, 0, 0) ? "I-21" : "D-03"))
            {
                CustomFinalBoss boss = level.Tracker.GetEntity<CustomFinalBoss>();
                boss.playerHasMoved = false;
                boss.SetHits(0);
            }
            else if (ChapterIndex == 4 && level.Session.Level == "Q-21")
            {
                AncientGuardian boss = level.Tracker.GetEntity<AncientGuardian>();
                boss.playerHasMoved = false;
                boss.SetHealth(15);
            }
            else if (ChapterIndex == 5 && level.Session.Level == "Y-10")
            {
                Genesis boss = level.Tracker.GetEntity<Genesis>();
                boss.playerHasMoved = false;
                boss.SetHealth(15);
            }
        }

        public override void Render()
        {
            base.Render();
            if (Visible)
            {
                sprite.Render();
            }
        }

        public void ManageUpgrades(Level level, bool active)
        {
            if (active)
            {
                if (level.Session.GetFlag("Upgrade_HadSpaceJump"))
                {
                    level.Session.SetFlag("Upgrade_SpaceJump", true);
                    XaphanModule.ModSettings.SpaceJump = 2;
                    level.Session.SetFlag("Upgrade_HadSpaceJump", false);
                }
                if (level.Session.GetFlag("Upgrade_HadBombs"))
                {
                    level.Session.SetFlag("Upgrade_Bombs", true);
                    XaphanModule.ModSettings.Bombs = true;
                    level.Session.SetFlag("Upgrade_HadBombs", false);
                }
                if (level.Session.GetFlag("Upgrade_HadSpiderMagnet"))
                {
                    level.Session.SetFlag("Upgrade_SpiderMagnet", true);
                    XaphanModule.ModSettings.SpiderMagnet = true;
                    level.Session.SetFlag("Upgrade_HadSpiderMagnet", false);
                }
                if (level.Session.GetFlag("Upgrade_HadRemoteDrone"))
                {
                    level.Session.SetFlag("XaphanHelper_Prevent_Drone", false);
                    level.Session.SetFlag("Upgrade_HadRemoteDrone", false);
                }
                /*if (level.Session.GetFlag("Upgrade_HadLightningDash"))
                {
                    level.Session.SetFlag("Upgrade_LightningDash", true);
                    XaphanModule.ModSettings.LightningDash = true;
                    level.Session.SetFlag("Upgrade_HadLightningDash", false);
                }
                if (level.Session.GetFlag("Upgrade_HadGravityJacket"))
                {
                    level.Session.SetFlag("Upgrade_GravityJacket", true);
                    XaphanModule.ModSettings.GravityJacket = true;
                    level.Session.SetFlag("Upgrade_HadGravityJacket", false);
                }*/
            }
            else
            {
                if ((SpaceJumpCollected() || level.Session.GetFlag("Upgrade_SpaceJump")) && !AllowsSpaceJump)
                {
                    level.Session.SetFlag("Upgrade_SpaceJump", false);
                    XaphanModule.ModSettings.SpaceJump = 1;
                    level.Session.SetFlag("Upgrade_HadSpaceJump", true);
                }
                if ((BombsCollected() || level.Session.GetFlag("Upgrade_Bombs")) && !AllowsBombs)
                {
                    level.Session.SetFlag("Upgrade_Bombs", false);
                    XaphanModule.ModSettings.Bombs = false;
                    level.Session.SetFlag("Upgrade_HadBombs", true);
                }
                if ((SpiderMagnetCollected() || level.Session.GetFlag("Upgrade_SpiderMagnet")) && !AllowsSpiderMagnet)
                {
                    level.Session.SetFlag("Upgrade_SpiderMagnet", false);
                    XaphanModule.ModSettings.SpiderMagnet = false;
                    level.Session.SetFlag("Upgrade_HadSpiderMagnet", true);
                }
                if ((RemoteDroneCollected() || level.Session.GetFlag("Upgrade_RemoteDrone")) && !AllowsRemoteDrone)
                {
                    level.Session.SetFlag("XaphanHelper_Prevent_Drone", true);
                    level.Session.SetFlag("Upgrade_HadRemoteDrone", true);
                }
                /*if (LightningDashCollected() || level.Session.GetFlag("Upgrade_HadLightningDash"))
                {
                    level.Session.SetFlag("Upgrade_LightningDash", false);
                    XaphanModule.ModSettings.LightningDash = false;
                    level.Session.SetFlag("Upgrade_HadLightningDash", true);
                }
                if (GravityJacketCollected() || level.Session.GetFlag("Upgrade_HadGravityJacket"))
                {
                    level.Session.SetFlag("Upgrade_GravityJacket", false);
                    XaphanModule.ModSettings.GravityJacket = false;
                    level.Session.SetFlag("Upgrade_HadGravityJacket", true);
                }*/
            }
        }
    }
}
