﻿using System;
using System.Collections;
using System.Linq;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [CustomEntity("XaphanHelper/CustomFollower")]
    class CustomFollower : Entity
    {
        [Tracked(true)]
        private class CustomFollowerCollectText : Entity
        {
            private Sprite[] sprites;

            private int color;

            private VertexLight light;

            private BloomPoint bloom;

            private int index;

            private DisplacementRenderer.Burst burst;

            private string text;

            private float textWidth;

            public CustomFollowerCollectText(Vector2 position, int index, int color, string text) : base(position)
            {
                this.text = text;
                sprites = new Sprite[text.Length];
                for (int i = 0; i < text.Length; i++)
                {
                    Sprite sprite = new Sprite(GFX.Game, "collectables/XaphanHelper/CustomFollower/collectText/");
                    sprite.Add("start", "start", 0.08f);
                    sprite.Add("char", text[i] == ' ' ? "_" : text[i].ToString(), 1f);
                    sprite.Add("end", "end", 0.08f);
                    sprite.Play("start");
                    sprites[i] = sprite;
                    
                }
                textWidth = sprites.Sum((Sprite sprite) => sprite.Width -1) + 1;
                Add(light = new VertexLight(Color.White, 1f, 16, 24));
                Add(bloom = new BloomPoint(1f, 12f));
                Depth = -2000100;
                Tag = (int)Tags.Persistent | (int)Tags.TransitionUpdate | (int)Tags.FrozenUpdate;
                this.color = color;
                this.index = index;
            }

            public override void Added(Scene scene)
            {
                index = Math.Min(5, index);
                float lastX = (float)Math.Round(-textWidth / 2f, MidpointRounding.AwayFromZero);
                for (int i = 0; i < text.Length; i++)
                {
                    sprites[i].X = lastX;
                    lastX = sprites[i].X + sprites[i].Width - 1;
                    Add(sprites[i]);
                }
                Sprite[] array = sprites;
                foreach (Sprite sprite in array)
                {
                    sprite.OnLastFrame = delegate
                    {
                        if (sprite.CurrentAnimationID == "start")
                        {
                            sprite.Play("char");
                        }
                        else if (sprite.CurrentAnimationID == "char")
                        {
                            sprite.Play("end");
                        }
                        else if (sprite.CurrentAnimationID == "end")
                        {
                            RemoveSelf();
                        }
                    };
                }
                base.Added(scene);
                foreach (Entity entity in Scene.Tracker.GetEntities<CustomFollowerCollectText>())
                {
                    if (entity != this && Vector2.DistanceSquared(entity.Position, Position) <= 256f)
                    {
                        entity.RemoveSelf();
                    }
                }
                burst = (scene as Level).Displacement.AddBurst(Position, 0.3f, 16f, 24f, 0.3f);
            }

            public override void Update()
            {

                Level level = Scene as Level;
                if (level.Frozen)
                {
                    if (burst != null)
                    {
                        burst.AlphaFrom = (burst.AlphaTo = 0f);
                        burst.Percent = burst.Duration;
                    }
                    return;
                }
                base.Update();
                Camera camera = level.Camera;
                Y -= 8f * Engine.DeltaTime;
                X = Calc.Clamp(X, camera.Left + 8f, camera.Right - 8f);
                Y = Calc.Clamp(Y, camera.Top + 8f, camera.Bottom - 8f);
                light.Alpha = Calc.Approach(light.Alpha, 0f, Engine.DeltaTime * 4f);
                bloom.Alpha = light.Alpha;
                ParticleType particleType = (color == 0 ? Strawberry.P_GhostGlow : ( color == 1 ? Strawberry.P_Glow : Strawberry.P_MoonGlow));
                Sprite[] array = sprites;
                foreach (Sprite sprite in array)
                {
                    if (Scene.OnInterval(0.05f))
                    {
                        if (sprite.Color == particleType.Color2)
                        {
                            sprite.Color = particleType.Color;
                        }
                        else
                        {
                            sprite.Color = particleType.Color2;
                        }
                    }
                    if (Scene.OnInterval(0.06f) && sprite.CurrentAnimationFrame > 11)
                    {
                        level.ParticlesFG.Emit(particleType, 1, Position + Vector2.UnitY * -2f, new Vector2(8f, 4f));
                    }
                }
            }
        }

        public EntityID ID;

        private Vector2 start;

        public Follower Follower;

        private Sprite sprite;

        private bool collected;

        public bool ReturnHomeWhenLost = true;

        private Wiggler wiggler;

        private Wiggler rotateWiggler;

        private BloomPoint bloom;

        private VertexLight light;

        private Tween lightTween;

        private float wobble;

        private float collectTimer;

        private string type;

        private string Prefix;

        private int chapterIndex;

        public CustomFollower(EntityData data, Vector2 offset, EntityID gid)
        {
            ID = gid;
            string str = data.Attr("type").Replace(" ", "");
            type = (char.ToLower(str[0]) + str.Substring(1));
            Position = (start = data.Position + offset);
            Depth = -100;
            Collider = new Hitbox(14f, 14f, -7f, -7f);
            Add(new PlayerCollider(OnPlayer));
            Add(new MirrorReflection());
            Add(Follower = new Follower(ID, null, OnLoseLeader));
            Follower.FollowDelay = 0.3f;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            Prefix = SceneAs<Level>().Session.Area.GetLevelSet();
            chapterIndex = SceneAs<Level>().Session.Area.ChapterIndex;
            switch (type)
            {
                case "energyTank":
                    {
                        if ((!XaphanModule.PlayerHasGolden && !XaphanModule.Settings.SpeedrunMode && XaphanModule.ModSaveData.StaminaUpgrades.Contains(Prefix + "_Ch" + chapterIndex + "_" + ID)) || ((XaphanModule.PlayerHasGolden || XaphanModule.Settings.SpeedrunMode) && XaphanModule.ModSaveData.SpeedrunModeStaminaUpgrades.Contains(Prefix + "_Ch" + chapterIndex + "_" + ID)))
                        {
                            RemoveSelf();
                        }
                        break;
                    }
                case "missile":
                    {
                        if ((!XaphanModule.PlayerHasGolden && !XaphanModule.Settings.SpeedrunMode && XaphanModule.ModSaveData.DroneMissilesUpgrades.Contains(Prefix + "_Ch" + chapterIndex + "_" + ID)) || ((XaphanModule.PlayerHasGolden || XaphanModule.Settings.SpeedrunMode) && XaphanModule.ModSaveData.SpeedrunModeDroneMissilesUpgrades.Contains(Prefix + "_Ch" + chapterIndex + "_" + ID)))
                        {
                            RemoveSelf();
                        }
                        break;
                    }
                case "superMissile":
                    {
                        if ((!XaphanModule.PlayerHasGolden && !XaphanModule.Settings.SpeedrunMode && XaphanModule.ModSaveData.DroneSuperMissilesUpgrades.Contains(Prefix + "_Ch" + chapterIndex + "_" + ID)) || ((XaphanModule.PlayerHasGolden || XaphanModule.Settings.SpeedrunMode) && XaphanModule.ModSaveData.SpeedrunModeDroneSuperMissilesUpgrades.Contains(Prefix + "_Ch" + chapterIndex + "_" + ID)))
                        {
                            RemoveSelf();
                        }
                        break;
                    }
                case "fireRateModule":
                    {
                        if ((!XaphanModule.PlayerHasGolden && !XaphanModule.Settings.SpeedrunMode && XaphanModule.ModSaveData.DroneFireRateUpgrades.Contains(Prefix + "_Ch" + chapterIndex + "_" + ID)) || ((XaphanModule.PlayerHasGolden || XaphanModule.Settings.SpeedrunMode) && XaphanModule.ModSaveData.SpeedrunModeDroneFireRateUpgrades.Contains(Prefix + "_Ch" + chapterIndex + "_" + ID)))
                        {
                            RemoveSelf();
                        }
                        break;
                    }
            }
            Add(sprite = new Sprite(GFX.Game, "collectables/XaphanHelper/CustomFollower/" + type + "/"));
            sprite.AddLoop("idle", "idle", 0.1f, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3, 0, 1, 2, 3);
            sprite.Add("collect", "collect", 0.05f);
            sprite.CenterOrigin();
            sprite.Play("idle");
            sprite.OnFrameChange = OnAnimate;
            Add(wiggler = Wiggler.Create(0.4f, 4f, delegate (float v)
            {
                sprite.Scale = Vector2.One * (1f + v * 0.35f);
            }));
            Add(rotateWiggler = Wiggler.Create(0.5f, 4f, delegate (float v)
            {
                sprite.Rotation = v * 30f * ((float)Math.PI / 180f);
            }));
            Add(bloom = new BloomPoint(1f, 12f));
            Add(light = new VertexLight(Color.White, 1f, 16, 24));
            Add(lightTween = light.CreatePulseTween());
            if ((scene as Level).Session.BloomBaseAdd > 0.1f)
            {
                bloom.Alpha *= 0.5f;
            }
        }

        public override void Update()
        {
            if (!collected)
            {
                wobble += Engine.DeltaTime * 4f;
                Sprite obj = sprite;
                BloomPoint bloomPoint = bloom;
                float num2 = (light.Y = (float)Math.Sin(wobble) * 2f);
                float num5 = (obj.Y = (bloomPoint.Y = num2));
                int followIndex = Follower.FollowIndex;
                if (Follower.Leader != null && Follower.DelayTimer <= 0f && StrawberryRegistry.IsFirstStrawberry(this))
                {
                    Player player = Follower.Leader.Entity as Player;
                    bool flag = false;
                    if (player != null && player.Scene != null && !player.StrawberriesBlocked)
                    {
                        if (player.OnSafeGround && player.StateMachine.State != 13)
                        {
                            flag = true;
                        }
                    }
                    if (flag)
                    {
                        collectTimer += Engine.DeltaTime;
                        if (collectTimer > 0.15f)
                        {
                            OnCollect();
                        }
                    }
                    else
                    {
                        collectTimer = Math.Min(collectTimer, 0f);
                    }
                }
                else
                {
                    if (followIndex > 0)
                    {
                        collectTimer = -0.15f;
                    }
                }
            }
            base.Update();
            if (Follower.Leader != null && Scene.OnInterval(0.08f))
            {
                int color = type == "energyTank" ? 0 : (type == "missile" ? 1 : 2);
                ParticleType particleType = (color == 0 ? Strawberry.P_GhostGlow : (color == 1 ? Strawberry.P_Glow : Strawberry.P_MoonGlow));
                SceneAs<Level>().ParticlesFG.Emit(particleType, Position + Calc.Random.Range(-Vector2.One * 6f, Vector2.One * 6f));
            }
        }

        public void OnPlayer(Player player)
        {
            if (Follower.Leader != null || collected)
            {
                return;
            }
            ReturnHomeWhenLost = true;
            Audio.Play("event:/game/general/strawberry_touch", Position);
            player.Leader.GainFollower(Follower);
            wiggler.Start();
            Depth = -1000000;
        }

        private void OnAnimate(string id)
        {
            if (sprite.CurrentAnimationFrame == 27)
            {
                lightTween.Start();
                if (!collected && (CollideCheck<FakeWall>() || CollideCheck<Solid>()))
                {
                    Audio.Play("event:/game/general/strawberry_pulse", Position);
                    SceneAs<Level>().Displacement.AddBurst(Position, 0.6f, 4f, 28f, 0.1f);
                }
                else
                {
                    Audio.Play("event:/game/general/strawberry_pulse", Position);
                    SceneAs<Level>().Displacement.AddBurst(Position, 0.6f, 4f, 28f, 0.2f);
                }
            }
        }
        public void OnCollect()
        {
            if (!collected)
            {
                int collectIndex = 0;
                collected = true;
                if (Follower.Leader != null)
                {
                    Player obj = Follower.Leader.Entity as Player;
                    collectIndex = obj.StrawberryCollectIndex;
                    obj.StrawberryCollectIndex++;
                    obj.StrawberryCollectResetTimer = 2.5f;
                    Follower.Leader.LoseFollower(Follower);
                }
                Session session = (Scene as Level).Session;
                //session.DoNotLoad.Add(ID);
                session.UpdateLevelStartDashes();
                Add(new Coroutine(CollectRoutine(collectIndex)));
            }
        }

        private IEnumerator CollectRoutine(int collectIndex)
        {
            Tag = Tags.TransitionUpdate;
            Depth = -2000010;
            Audio.Play("event:/game/general/strawberry_get", Position, "colour", 0, "count", collectIndex);
            Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            sprite.Play("collect");
            while (sprite.Animating)
            {
                yield return null;
            }
            string text = type == "energyTank" ? "+5 " + Dialog.Clean("XaphanHelper_Collect_EnergyTank") : (type == "missile" ? "+2 " + Dialog.Clean("XaphanHelper_Collect_Missiles") : "+1 " + Dialog.Clean("XaphanHelper_Collect_SuperMissiles"));
            Scene.Add(new CustomFollowerCollectText(Position, collectIndex, type == "energyTank" ? 0 : (type == "missile" ? 1 : 2), text));
            switch (type)
            {
                case "energyTank":
                    {

                        XaphanModule.ModSaveData.StaminaUpgrades.Add(Prefix + "_Ch" + chapterIndex + "_" + ID);
                        if (XaphanModule.PlayerHasGolden || XaphanModule.Settings.SpeedrunMode)
                        {
                            XaphanModule.ModSaveData.SpeedrunModeStaminaUpgrades.Add(Prefix + "_Ch" + chapterIndex + "_" + ID);
                        }
                        break;
                    }
                case "missile":
                    {

                        XaphanModule.ModSaveData.DroneMissilesUpgrades.Add(Prefix + "_Ch" + chapterIndex + "_" + ID);
                        if (XaphanModule.PlayerHasGolden || XaphanModule.Settings.SpeedrunMode)
                        {
                            XaphanModule.ModSaveData.SpeedrunModeDroneMissilesUpgrades.Add(Prefix + "_Ch" + chapterIndex + "_" + ID);
                        }
                        break;
                    }
                case "superMissile":
                    {

                        XaphanModule.ModSaveData.DroneSuperMissilesUpgrades.Add(Prefix + "_Ch" + chapterIndex + "_" + ID);
                        if (XaphanModule.PlayerHasGolden || XaphanModule.Settings.SpeedrunMode)
                        {
                            XaphanModule.ModSaveData.SpeedrunModeDroneSuperMissilesUpgrades.Add(Prefix + "_Ch" + chapterIndex + "_" + ID);
                        }
                        break;
                    }
                case "fireRateModule":
                    {

                        XaphanModule.ModSaveData.DroneFireRateUpgrades.Add(Prefix + "_Ch" + chapterIndex + "_" + ID);
                        if (XaphanModule.PlayerHasGolden || XaphanModule.Settings.SpeedrunMode)
                        {
                            XaphanModule.ModSaveData.SpeedrunModeDroneFireRateUpgrades.Add(Prefix + "_Ch" + chapterIndex + "_" + ID);
                        }
                        break;
                    }
            }
            RemoveSelf();
        }

        private void OnLoseLeader()
        {
            if (collected || !ReturnHomeWhenLost)
            {
                return;
            }
            Alarm.Set(this, 0.15f, delegate
            {
                Vector2 vector = (start - Position).SafeNormalize();
                float num = Vector2.Distance(Position, start);
                float num2 = Calc.ClampedMap(num, 16f, 120f, 16f, 96f);
                Vector2 control = start + vector * 16f + vector.Perpendicular() * num2 * Calc.Random.Choose(1, -1);
                SimpleCurve curve = new SimpleCurve(Position, start, control);
                Tween tween = Tween.Create(Tween.TweenMode.Oneshot, Ease.SineOut, MathHelper.Max(num / 100f, 0.4f), start: true);
                tween.OnUpdate = delegate (Tween f)
                {
                    Position = curve.GetPoint(f.Eased);
                };
                tween.OnComplete = delegate
                {
                    Depth = 0;
                };
                Add(tween);
            });
        }
    }
}
