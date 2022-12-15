﻿using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [CustomEntity("XaphanHelper/CustomCollectable")]
    class CustomCollectable : Entity
    {
        public EntityID ID;

        private class BgFlash : Entity
        {
            private float alpha = 1f;

            public BgFlash()
            {
                Depth = 10100;
                Tag = Tags.Persistent;
            }

            public override void Update()
            {
                base.Update();
                alpha = Calc.Approach(alpha, 0f, Engine.DeltaTime * 0.5f);
                if (alpha <= 0f)
                {
                    RemoveSelf();
                }
            }

            public override void Render()
            {
                Vector2 position = (Scene as Level).Camera.Position;
                Draw.Rect(position.X - 10f, position.Y - 10f, 340f, 200f, Color.Black * alpha);
            }
        }

        private string sprite;

        private string collectSound;

        private string oldMusic;

        private string newMusic;

        public string flag;

        private bool changeMusic;

        private bool mustDash;

        private bool ignoreGolden;

        private Sprite collectable;

        private Wiggler scaleWiggler;

        private Vector2 moveWiggleDir;

        private Wiggler moveWiggler;

        private float bounceSfxDelay;

        private bool endChapter;

        private bool collectGoldenStrawberry;

        private bool completeSpeedrun;

        private bool registerInSaveData;

        private bool canRespawn;

        private bool shouldWaitBeforeRemoving;

        private bool FlagRegiseredInSaveData()
        {
            Session session = SceneAs<Level>().Session;
            string Prefix = session.Area.GetLevelSet();
            int chapterIndex = session.Area.ChapterIndex == -1 ? 0 : session.Area.ChapterIndex;
            if (!Settings.SpeedrunMode)
            {
                return XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flag);
            }
            else
            {
                return session.GetFlag(flag);
            }
        }

        protected XaphanModuleSettings Settings => XaphanModule.Settings;

        public CustomCollectable(EntityData data, Vector2 position, EntityID id) : base(data.Position + position)
        {
            Tag = Tags.TransitionUpdate;
            ID = id;
            sprite = data.Attr("sprite");
            changeMusic = data.Bool("changeMusic");
            collectSound = data.Attr("collectSound");
            newMusic = data.Attr("newMusic");
            flag = data.Attr("flag");
            mustDash = data.Bool("mustDash");
            endChapter = data.Bool("completeArea") || data.Bool("endChapter");
            collectGoldenStrawberry = data.Bool("collectGoldenStrawberry");
            completeSpeedrun = data.Bool("completeSpeedrun");
            registerInSaveData = data.Bool("registerInSaveData");
            ignoreGolden = data.Bool("ignoreGolden");
            canRespawn = data.Bool("canRespawn");
            if (sprite == "")
            {
                sprite = "collectables/XaphanHelper/CustomCollectable/collectable";
            }
            Collider = new Hitbox(12f, 12f, 2f, 2f);
            Add(collectable = new Sprite(GFX.Game, sprite));
            collectable.AddLoop("idle", "", 0.08f);
            collectable.AddLoop("static", "", 1f, 0);
            collectable.Play("idle");
            collectable.CenterOrigin();
            collectable.Position = collectable.Position + new Vector2(8, 8);
            Add(scaleWiggler = Wiggler.Create(0.5f, 4f, delegate (float f)
            {
                collectable.Scale = Vector2.One * (1f + f * 0.3f);
            }));
            moveWiggler = Wiggler.Create(0.8f, 2f);
            moveWiggler.StartZero = true;
            Add(moveWiggler);
            Add(new PlayerCollider(OnPlayer));
        }

        private void OnPlayer(Player player)
        {
            Level level = Scene as Level;
            if (mustDash && player.DashAttacking)
            {
                Add(new Coroutine(Collect(player, level)));
                return;
            }
            else if (!mustDash)
            {
                Add(new Coroutine(Collect(player, level)));
                return;
            }
            player.PointBounce(Center);
            moveWiggler.Start();
            scaleWiggler.Start();
            moveWiggleDir = (Center - player.Center).SafeNormalize(Vector2.UnitY);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            if (bounceSfxDelay <= 0f)
            {
                Audio.Play("event:/game/general/crystalheart_bounce", Position);
                bounceSfxDelay = 0.1f;
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            endChapter = (completeSpeedrun && Settings.SpeedrunMode) ? true : endChapter;
            bool haveGolden = false;
            foreach (Strawberry item in Scene.Entities.FindAll<Strawberry>())
            {
                if (item.Golden && item.Follower.Leader != null)
                {
                    haveGolden = true;
                    break;
                }
            }
            if (string.IsNullOrEmpty(flag))
            {
                RemoveSelf();
            }
            else
            {
                if (!canRespawn)
                {
                    if (!haveGolden || (haveGolden && ignoreGolden))
                    {
                        if (!Settings.SpeedrunMode && FlagRegiseredInSaveData() || SceneAs<Level>().Session.GetFlag(flag))
                        {
                            RemoveSelf();
                        }
                    }
                }
                else
                {
                    Session session = SceneAs<Level>().Session;
                    string Prefix = session.Area.GetLevelSet();
                    int chapterIndex = session.Area.ChapterIndex == -1 ? 0 : session.Area.ChapterIndex;
                    if (!registerInSaveData ? SceneAs<Level>().Session.GetFlag(flag) : XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flag))
                    {
                        shouldWaitBeforeRemoving = true;
                    }
                }
            }
        }

        public IEnumerator WaitBeforeRemoveRoutine(bool registerInSaveData)
        {
            float timer = 0.02f;
            Session session = SceneAs<Level>().Session;
            string Prefix = session.Area.GetLevelSet();
            int chapterIndex = session.Area.ChapterIndex == -1 ? 0 : session.Area.ChapterIndex;
            while (timer > 0)
            {
                timer -= Engine.DeltaTime;
                if (!registerInSaveData ? !SceneAs<Level>().Session.GetFlag(flag) : !XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flag))
                {
                    yield break;
                }
                yield return null;
            }
            RemoveSelf();
        }

        public override void Update()
        {
            base.Update();
            if (shouldWaitBeforeRemoving)
            {
                shouldWaitBeforeRemoving = false;
                Add(new Coroutine(WaitBeforeRemoveRoutine(registerInSaveData)));
            }
            bounceSfxDelay -= Engine.DeltaTime;
            collectable.Position = moveWiggleDir * moveWiggler.Value * -8f;
            collectable.Position = collectable.Position + new Vector2(8, 8);
        }

        private IEnumerator Collect(Player player, Level level)
        {
            Visible = false;
            Collidable = false;
            Session session = SceneAs<Level>().Session;
            if (!changeMusic)
            {
                SoundEmitter.Play(collectSound, this);
            }
            else
            {
                oldMusic = Audio.CurrentMusic;
                session.Audio.Music.Event = SFX.EventnameByHandle(collectSound);
                session.Audio.Apply(forceSixteenthNoteHack: false);
            }
            RegisterFlag();
            if (Settings.ShowMiniMap)
            {
                MapDisplay mapDisplay = SceneAs<Level>().Tracker.GetEntity<MapDisplay>();
                if (mapDisplay != null)
                {
                    mapDisplay.GenerateIcons();
                }
            }
            level.Shake();
            Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
            for (int i = 0; i < 10; i++)
            {
                Scene.Add(new AbsorbOrb(Position + new Vector2(8, 8), player));
            }
            level.Flash(Color.White, drawPlayerOver: true);
            Scene.Add(new BgFlash());
            List<Strawberry> strawbs = new();
            foreach (Follower follower in player.Leader.Followers)
            {
                if (follower.Entity is Strawberry)
                {
                    strawbs.Add(follower.Entity as Strawberry);
                }
            }
            if (collectGoldenStrawberry)
            {
                foreach (Strawberry strawb in strawbs)
                {
                    if (strawb.Golden)
                    {
                        strawb.OnCollect();
                    }
                }
                if (SceneAs<Level>().Session.GrabbedGolden)
                {
                    SceneAs<Level>().Session.GrabbedGolden = false;
                }
            }
            if (!endChapter)
            {
                Engine.TimeRate = 0.5f;
                while (Engine.TimeRate < 1f)
                {
                    Engine.TimeRate += Engine.RawDeltaTime * 0.5f;
                    yield return null;
                }
            }
            else
            {
                foreach (Strawberry strawb in strawbs)
                {
                    strawb.OnCollect();
                }
                Engine.TimeRate = 0.5f;
                player.Depth = -2000000;
                level.FormationBackdrop.Display = true;
                level.FormationBackdrop.Alpha = 1f;
                Visible = false;
                for (float i = 0f; i < 2f; i += Engine.RawDeltaTime)
                {
                    Engine.TimeRate = Calc.Approach(Engine.TimeRate, 0f, Engine.RawDeltaTime * 0.25f);
                    yield return null;
                }
                if (player.Dead)
                {
                    yield return 100f;
                }
                Engine.TimeRate = 1f;
                Tag = Tags.FrozenUpdate;
                level.Frozen = true;
                level.TimerStopped = true;
                level.RegisterAreaComplete();
                yield return new FadeWipe(level, wipeIn: false)
                {
                    Duration = 1.25f
                }.Duration;
                level.CompleteArea(spotlightWipe: false, skipScreenWipe: true, skipCompleteScreen: false);
            }
            if (changeMusic)
            {
                if (!string.IsNullOrEmpty(newMusic))
                {
                    session.Audio.Music.Event = SFX.EventnameByHandle(newMusic);
                }
                else
                {
                    session.Audio.Music.Event = SFX.EventnameByHandle(oldMusic);
                }
                session.Audio.Apply(forceSixteenthNoteHack: false);
            }
            if (!canRespawn)
            {
                session.DoNotLoad.Add(ID);
            }
            RemoveSelf();
        }

        private void RegisterFlag()
        {
            Session session = SceneAs<Level>().Session;
            int chapterIndex = session.Area.ChapterIndex == -1 ? 0 : session.Area.ChapterIndex;
            if (flag != "")
            {
                session.SetFlag(flag, true);
            }
            if (registerInSaveData)
            {
                string Prefix = session.Area.GetLevelSet();
                if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flag))
                {
                    XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + flag);
                }
            }
        }

        private IEnumerator CompleteArea(Player player, Level level)
        {
            level.FormationBackdrop.Alpha = 1f;
            Visible = false;
            if (player.Dead)
            {
                yield return 100f;
            }
            Engine.TimeRate = 1f;
            Tag = Tags.FrozenUpdate;
            level.Frozen = true;
            yield return new FadeWipe(level, wipeIn: false)
            {
                Duration = 3.25f
            }.Duration;
            level.CompleteArea(spotlightWipe: false, skipScreenWipe: true, skipCompleteScreen: false);
        }
    }
}
