﻿using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Events;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/AncientGuardian")]
    public class AncientGuardian : Actor
    {
        [Tracked(true)]
        private class GuardianLaser : Entity
        {
            private Sprite TopSprite;

            private Sprite BlastSprite;

            bool FlipX;

            public GuardianLaser(Vector2 offset, bool flip = false) : base(offset)
            {
                FlipX = flip;
                Collider = new Hitbox(7f, 5f);
                Add(TopSprite = new Sprite(GFX.Game, "characters/Xaphan/Guardian/"));
                TopSprite.Add("laser", "laserTop", 0.06f);
                TopSprite.FlipX = FlipX;
                Add(BlastSprite = new Sprite(GFX.Game, "characters/Xaphan/Guardian/"));
                BlastSprite.Add("laser", "laserBlast", 0.06f);
                BlastSprite.FlipX = FlipX;
                Depth = -50000;
            }

            public override void Added(Scene scene)
            {
                base.Added(scene);
                Collider = new Hitbox(7f, SceneAs<Level>().Bounds.Bottom - Top);
                while (CollideCheck<Solid>() || CollideCheck<JumpThru>())
                {
                    Collider.Height -= 1;
                }
                Add(new PlayerCollider(onCollidePlayer));
                TopSprite.Position = new Vector2(-6f, 0f);
                TopSprite.Play("laser");
                BuildMidSprites();
                BlastSprite.Position = new Vector2(-6f, Collider.Height - 8);
                BlastSprite.Play("laser");
                BlastSprite.OnLastFrame += OnLastFrame;
            }

            private List<Sprite> BuildMidSprites()
            {
                List<Sprite> list = new();
                for (int i = 1; i < Collider.Height / 4 - 2; i++)
                {
                    Sprite sprite = new(GFX.Game, "characters/Xaphan/Guardian/");
                    sprite.Add("laser", "laserMid", 0.06f);
                    sprite.FlipX = FlipX;
                    sprite.Play("laser");
                    sprite.Position = new Vector2(-6f, i * 4);
                    list.Add(sprite);
                    Add(sprite);
                }
                return list;
            }

            private void OnLastFrame(string s)
            {
                RemoveSelf();
            }

            private void onCollidePlayer(Player player)
            {
                player.Die((player.Position - Position).SafeNormalize());
            }
        }

        [Tracked(true)]
        private class GuardianSprayFireBall : Actor
        {
            Sprite Sprite;

            public Vector2 Speed;

            private Collision onCollideH;

            private Collision onCollideV;

            private float cantKillTimer;

            private int rotation;

            private bool Collided;

            public GuardianSprayFireBall(Vector2 offset, Vector2 speed) : base(offset)
            {
                Add(new PlayerCollider(onCollidePlayer));
                Collider = new Circle(5f);
                Speed = speed;
                Add(Sprite = new Sprite(GFX.Game, "characters/Xaphan/Guardian/"));
                Sprite.Add("fireball", "sprayFireball", 0f);
                Sprite.Add("flame", "sprayFlame", 0.08f);
                Sprite.CenterOrigin();
                Sprite.Play("fireball");
                onCollideH = OnCollideH;
                onCollideV = OnCollideV;
                Add(new Coroutine(GravityRoutine()));
                Add(new Coroutine(SpriteRotationRoutine(speed.X)));
                Depth = -14999;
            }

            public override void Update()
            {
                base.Update();
                Sprite.FlipX = Speed.X > 0;
                if (cantKillTimer > 0f)
                {
                    cantKillTimer -= Engine.DeltaTime;
                }
                if (Sprite != null && Sprite.CurrentAnimationID != "explode")
                {
                    Sprite.Rotation = rotation;
                }
                MoveH(Speed.X * Engine.DeltaTime, onCollideH);
                MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
            }

            public IEnumerator GravityRoutine()
            {
                while (Sprite.CurrentAnimationID == "fireball")
                {
                    Speed.Y += 4f;
                    yield return null;
                }
            }

            public IEnumerator SpriteRotationRoutine(float dir)
            {
                while (!Collided)
                {
                    if (dir > 0)
                    {
                        if (rotation >= 360)
                        {
                            rotation = 0;
                        }
                        else
                        {
                            rotation++;
                            float timer = 0.04f;
                            while (timer > 0)
                            {
                                if (Collided)
                                {
                                    break;
                                }
                                timer -= Engine.DeltaTime;
                                yield return null;
                            }
                        }
                    }
                    else
                    {
                        if (rotation <= 0)
                        {
                            rotation = 360;
                        }
                        else
                        {
                            rotation--;
                            float timer = 0.04f;
                            while (timer > 0)
                            {
                                if (Collided)
                                {
                                    break;
                                }
                                timer -= Engine.DeltaTime;
                                yield return null;
                            }
                        }
                    }
                }
                rotation = 0;
            }

            private void OnCollideH(CollisionData data)
            {
                RemoveSelf();
            }

            private void OnCollideV(CollisionData data)
            {
                Speed = Vector2.Zero;
                if (!Collided)
                {
                    Collided = true;
                    Sprite.Play("flame");
                    Sprite.Position = new Vector2(1, -4);
                    Collider = new Circle(3f);
                    Sprite.OnLastFrame = delegate { RemoveSelf(); };
                }
            }

            private void onCollidePlayer(Player player)
            {
                if (cantKillTimer > 0)
                {
                    RemoveSelf();
                }
                else
                {
                    player.Die((player.Position - Position).SafeNormalize());
                }
            }

            public override void Render()
            {
                if (Sprite.CurrentAnimationID == "fireball")
                {
                    Sprite.DrawOutline();
                }
                base.Render();
            }
        }

        [Tracked(true)]
        private class GuardianFireBall : Actor
        {
            Sprite Sprite;

            List<Sprite> FlameSprites = new();

            public Vector2 Speed;

            private Collision onCollideH;

            private Collision onCollideV;

            private float cantKillTimer;

            private int rotation;

            private bool split;

            private bool WasSplitted;

            private bool Collided;

            private Vector2 Scale;

            private float Alpha;

            public GuardianFireBall(Vector2 offset, Vector2 speed, bool split = false) : base(offset)
            {
                Add(new PlayerCollider(onCollidePlayer));
                Collider = new Circle(5f);
                Speed = speed;
                this.split = split;
                Alpha = 1f;
                Scale = Vector2.One;
                Add(Sprite = new Sprite(GFX.Game, "characters/Xaphan/Guardian/"));
                Sprite.Add("fireball", "fireball", 0f);
                Sprite.Add("flame", "flame", 0.08f);
                Sprite.CenterOrigin();
                Sprite.Play("fireball");
                onCollideH = OnCollideH;
                onCollideV = OnCollideV;
                Add(new Coroutine(GravityRoutine()));
                Add(new Coroutine(SpriteRotationRoutine()));
                Depth = -14999;
            }

            public override void Update()
            {
                base.Update();
                Sprite.FlipX = Speed.X > 0;
                if (cantKillTimer > 0f)
                {
                    cantKillTimer -= Engine.DeltaTime;
                }
                if (Sprite != null && Sprite.CurrentAnimationID != "explode")
                {
                    Sprite.Rotation = rotation;
                }
                MoveH(Speed.X * Engine.DeltaTime, onCollideH);
                MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
            }

            public IEnumerator GravityRoutine()
            {
                while (Sprite.CurrentAnimationID == "fireball")
                {
                    Speed.Y += 4f;
                    yield return null;
                }
            }

            public IEnumerator SpriteRotationRoutine()
            {
                while (Sprite.CurrentAnimationID == "fireball")
                {
                    if (rotation >= 360)
                    {
                        rotation = 0;
                    }
                    else
                    {
                        rotation++;
                        yield return 0.04f;
                    }
                }
                rotation = 0;
            }

            private IEnumerator ExpandRoutine()
            {
                while (Collider.Width < 24f)
                {
                    Collider.Left -= 1f;
                    Collider.Width += 2f;
                    yield return 0.05f;
                }
                yield return 2f;
                float timer = 1.5f;
                while (timer > 0f)
                {
                    if (timer > 0.75f)
                    {
                        Scale = new Vector2(1f, timer / 1.5f);
                    }
                    if (timer <= 0.5f)
                    {
                        Alpha = timer * 2f;
                    }
                    if (timer <= 0.25f)
                    {
                        Collider = null;
                    }
                    timer -= Engine.DeltaTime;
                    yield return null;

                }
                RemoveSelf();
            }

            private void OnCollideH(CollisionData data)
            {
                RemoveSelf();
            }

            private void OnCollideV(CollisionData data)
            {
                Speed = Vector2.Zero;
                if (!WasSplitted && !Collided)
                {
                    Sprite.OnLastFrame = delegate { Sprite.Visible = false; };
                    Collided = true;
                    Position.Y = data.Hit.Top - 4f;
                    Collider = new Hitbox(12f, 4f, -6f, 0f);
                    Sprite.Position.Y -= 5;
                    Sprite.Play("flame");
                    for (int i = 0; i < 5; i++)
                    {
                        Sprite sprite = new(GFX.Game, "characters/Xaphan/Guardian/");
                        sprite.AddLoop("flame", "flame", 0.08f, 3, 4, 5, 4);
                        sprite.CenterOrigin();
                        sprite.Play("flame");
                        sprite.Position = new Vector2(i == 0 ? -9f : i * 4 - 8f, i % 2 != 0 ? -3f : -2f);
                        FlameSprites.Add(sprite);
                        Add(sprite);
                    }
                    Add(new Coroutine(ExpandRoutine()));
                }
                if (split && !WasSplitted)
                {
                    WasSplitted = true;
                    SceneAs<Level>().Add(new GuardianFireBall(Position, new Vector2(50f, -133f)));
                    SceneAs<Level>().Add(new GuardianFireBall(Position, new Vector2(-50f, -133f)));
                }
            }

            private void onCollidePlayer(Player player)
            {
                if (cantKillTimer > 0)
                {
                    RemoveSelf();
                }
                else
                {
                    player.Die((player.Position - Position).SafeNormalize());
                }
            }

            public override void Render()
            {
                if (Sprite.CurrentAnimationID == "fireball")
                {
                    Sprite.DrawOutline();
                }
                base.Render();
                foreach (Sprite sprite in FlameSprites)
                {
                    sprite.Scale = Scale;
                    sprite.Position.Y *= Scale.Y;
                    sprite.Color = Color.White * Alpha;
                    sprite.Render();
                }
            }
        }

        [Tracked(true)]
        private class GuardianSideFireBall : Actor
        {
            Sprite Sprite;

            public Vector2 Speed;

            private float cantKillTimer;

            private int rotation;

            public GuardianSideFireBall(Vector2 offset, Vector2 speed) : base(offset)
            {
                Add(new PlayerCollider(onCollidePlayer, new Circle(5f)));
                Speed = speed;
                Add(Sprite = new Sprite(GFX.Game, "characters/Xaphan/Guardian/"));
                Sprite.Add("fireball", "sideFireball", 0f);
                Sprite.CenterOrigin();
                Sprite.Play("fireball");
                Add(new Coroutine(GravityRoutine()));
                Add(new Coroutine(SpriteRotationRoutine()));
                Depth = -14999;
            }

            public override void Update()
            {
                base.Update();
                Sprite.FlipX = Speed.X > 0;
                if (cantKillTimer > 0f)
                {
                    cantKillTimer -= Engine.DeltaTime;
                }
                if (Sprite != null && Sprite.CurrentAnimationID != "explode")
                {
                    Sprite.Rotation = rotation;
                }
                MoveH(Speed.X * Engine.DeltaTime);
                MoveV(Speed.Y * Engine.DeltaTime);
                if (Bottom < SceneAs<Level>().Bounds.Top || Right < SceneAs<Level>().Bounds.Left || Left > SceneAs<Level>().Bounds.Right)
                {
                    RemoveSelf();
                }
            }

            public IEnumerator GravityRoutine()
            {
                while (true)
                {
                    Speed.Y -= 4f;
                    yield return null;
                }
            }

            public IEnumerator SpriteRotationRoutine()
            {
                while (true)
                {
                    if (rotation >= 360)
                    {
                        rotation = 0;
                    }
                    else
                    {
                        rotation++;
                        yield return 0.04f;
                    }
                }
            }

            private void onCollidePlayer(Player player)
            {
                if (cantKillTimer > 0)
                {
                    RemoveSelf();
                }
                else
                {
                    player.Die((player.Position - Position).SafeNormalize());
                }
            }

            public override void Render()
            {
                if (Sprite.CurrentAnimationID == "fireball")
                {
                    Sprite.DrawOutline();
                }
                base.Render();
            }
        }

        [Pooled]
        private class GuardianDebris : Actor
        {
            private Image sprite;

            private Vector2 speed;

            private bool shaking;

            private bool firstHit;

            private float alpha;

            private Collision onCollideH;

            private Collision onCollideV;

            private float spin;

            private float lifeTimer;

            private string directory;

            public GuardianDebris() : base(Vector2.Zero)
            {
                Tag = Tags.TransitionUpdate;
                Collider = new Hitbox(4f, 4f, -2f, -2f);

                onCollideH = delegate
                {
                    speed.X = (0f - speed.X) * 0.5f;
                };
                onCollideV = delegate
                {
                    if (firstHit || speed.Y > 50f)
                    {
                        Audio.Play("event:/game/06_reflection/fall_spike_smash", Position, "debris_velocity", Calc.ClampedMap(speed.Y, 0f, 600f));
                    }
                    if (speed.Y > 0f && speed.Y < 40f)
                    {
                        speed.Y = 0f;
                    }
                    else
                    {
                        speed.Y = (0f - speed.Y) * 0.25f;
                    }
                    firstHit = false;
                };
            }

            protected override void OnSquish(CollisionData data)
            {
            }

            public GuardianDebris Init(Vector2 position, Vector2 center)
            {
                Collidable = true;
                Position = position;
                speed = ((position - center).SafeNormalize(60f + Calc.Random.NextFloat(60f)) + Vector2.UnitY * -150f) * new Vector2(2f, 1f);
                directory = "characters/Xaphan/guardian/debris";
                Add(sprite = new Image(Calc.Random.Choose(GFX.Game.GetAtlasSubtextures(directory))));
                sprite.CenterOrigin();
                sprite.FlipX = Calc.Random.Chance(0.5f);
                sprite.Position = Vector2.Zero;
                sprite.Rotation = Calc.Random.NextAngle();
                shaking = false;
                sprite.Scale.X = 1f;
                sprite.Scale.Y = 1f;
                sprite.Color = Color.White;
                alpha = 1f;
                firstHit = false;
                spin = Calc.Random.Range(3.49065852f, 10.4719753f) * Calc.Random.Choose(1, -1);
                lifeTimer = Calc.Random.Range(0.6f, 2.6f);
                return this;
            }

            public override void Update()
            {
                base.Update();
                if (Collidable)
                {
                    speed.X = Calc.Approach(speed.X, 0f, Engine.DeltaTime * 100f);
                    if (!OnGround())
                    {
                        speed.Y += 400f * Engine.DeltaTime;
                    }
                    MoveH(speed.X * Engine.DeltaTime, onCollideH);
                    MoveV(speed.Y * Engine.DeltaTime, onCollideV);
                }
                if (shaking && Scene.OnInterval(0.05f))
                {
                    sprite.X = -1 + Calc.Random.Next(3);
                    sprite.Y = -1 + Calc.Random.Next(3);
                }
                if ((Scene as Level).Transitioning)
                {
                    alpha = Calc.Approach(alpha, 0f, Engine.DeltaTime * 4f);
                    sprite.Color = Color.White * alpha;
                }
                sprite.Rotation += spin * Calc.ClampedMap(Math.Abs(speed.Y), 50f, 150f) * Engine.DeltaTime;
                if (lifeTimer > 0f)
                {
                    lifeTimer -= Engine.DeltaTime;
                }
                else if (alpha > 0f)
                {
                    alpha -= 4f * Engine.DeltaTime;
                    if (alpha <= 0f)
                    {
                        RemoveSelf();
                    }
                }
                sprite.Color = Color.White * alpha;
            }
        }

        private Vector2 OrigPosition;

        private PlayerCollider pc;

        private Coroutine Routine = new();

        private Sprite Sprite;

        private Sprite EyesSprite;

        private Sprite LeftWheelSprite;

        private Sprite RightWheelSprite;

        public int Health;

        private float InvincibilityDelay;

        private bool Flashing;

        public bool playerHasMoved;

        private bool CannotHitPlayer;

        private float EyesAlpha;

        private Vector2 Speed;

        private int TrackPosition;

        private bool CanMove;

        private bool StopPattern;

        public bool ForcedDestroy;

        public bool HasGolden()
        {
            foreach (Strawberry item in Scene.Entities.FindAll<Strawberry>())
            {
                if (item.Golden && item.Follower.Leader != null)
                {
                    return true;
                }
            }
            return false;
        }

        public AncientGuardian(EntityData data, Vector2 offset) : base(data.Position + offset)
        {
            OrigPosition = Position;
            Collider = new Hitbox(30f, 23f, 18f, 23f);
            Add(pc = new PlayerCollider(OnPlayer, new Circle(15f, 33f, 18f)));
            Add(Sprite = new Sprite(GFX.Game, "characters/Xaphan/Guardian/"));
            Sprite.Add("idle", "idle", 0f);
            Sprite.Play("idle");
            Add(EyesSprite = new Sprite(GFX.Game, "characters/Xaphan/Guardian/"));
            EyesSprite.Add("laser", "eyes", 0, 0);
            EyesSprite.Add("sprayFireball", "eyes", 0, 1);
            EyesSprite.Add("fireball", "eyes", 0, 2);
            EyesSprite.Add("sideFireball", "eyes", 0, 3);
            EyesSprite.Add("off", "eyes", 0, 4);
            EyesSprite.Position += new Vector2(21f, 20f);
            Add(LeftWheelSprite = new Sprite(GFX.Game, "characters/Xaphan/Guardian/"));
            LeftWheelSprite.AddLoop("wheel", "wheel", 0.04f);
            LeftWheelSprite.Position += new Vector2(0f, 15f);
            Add(RightWheelSprite = new Sprite(GFX.Game, "characters/Xaphan/Guardian/"));
            RightWheelSprite.AddLoop("wheel", "wheel", 0.04f);
            RightWheelSprite.Position += new Vector2(51f, 15f);
            Health = 15;
            Depth = -15000;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            string Prefix = SceneAs<Level>().Session.Area.LevelSet;
            if (XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch4_Boss_Defeated"))
            {
                Visible = false;
            }
            if (!XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch4_BossStart"))
            {
                EyesAlpha = 1f;
                EyesSprite.Play("off");
            }
            foreach (GuardianLaser laser in SceneAs<Level>().Tracker.GetEntities<GuardianLaser>())
            {
                laser.RemoveSelf();
            }
            foreach (GuardianSprayFireBall fireball in SceneAs<Level>().Tracker.GetEntities<GuardianSprayFireBall>())
            {
                fireball.RemoveSelf();
            }
            foreach (GuardianFireBall fireball in SceneAs<Level>().Tracker.GetEntities<GuardianFireBall>())
            {
                fireball.RemoveSelf();
            }
            foreach (GuardianSideFireBall fireball in SceneAs<Level>().Tracker.GetEntities<GuardianSideFireBall>())
            {
                fireball.RemoveSelf();
            }
            Add(new Coroutine(SequenceRoutine()));
        }

        public void OnPlayer(Player player)
        {
            if (!CannotHitPlayer)
            {
                player.Speed = player.LiftSpeed = Vector2.Zero;
                if ((player.StateMachine.State == Player.StDash || player.DashAttacking) && InvincibilityDelay <= 0)
                {
                    CannotHitPlayer = true;
                    GetHit();
                    PushPlayer(player);
                }
                else
                {
                    player.Die((player.Position - Position).SafeNormalize());
                }
            }
        }

        private void PushPlayer(Player player)
        {
            if (player != null && !player.Dead)
            {
                if (player != null)
                {
                    Celeste.Freeze(0.1f);
                    Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                    player.StateMachine.State = 0;
                    player.Speed.Y = 150f;
                }
                SceneAs<Level>().Displacement.AddBurst(player.Position, 0.4f, 12f, 36f, 0.5f);
                SceneAs<Level>().Displacement.AddBurst(player.Position, 0.4f, 24f, 48f, 0.5f);
                SceneAs<Level>().Displacement.AddBurst(player.Position, 0.4f, 36f, 60f, 0.5f);
            }
        }

        public void Appear(bool visible)
        {
            SceneAs<Level>().Displacement.AddBurst(Center, 0.5f, 16f, 64f, 0.5f);
            Visible = visible;
        }

        public void GetHit()
        {
            if (Health > 0 && InvincibilityDelay <= 0)
            {
                Health -= 1;
                if (Health > 0)
                {
                    Audio.Play("event:/game/xaphan/guardian_hit", Position);
                }
                else
                {
                    Audio.Play("event:/game/xaphan/guardian_death", Position);
                }
                InvincibilityDelay = 0.75f;
            }
        }

        public override void Update()
        {
            base.Update();
            Collidable = Visible;
            EyesSprite.Color = Color.White * EyesAlpha;
            if (Flashing)
            {
                Sprite.Color = Color.Red;
            }
            else
            {
                Sprite.Color = Color.White;
            }
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (player == null && Routine.Active)
            {
                Routine.Cancel();
                return;
            }
            if (Health == 4 && !CanMove)
            {
                CanMove = StopPattern = true;
            }
            if (!playerHasMoved && player != null && player.Speed != Vector2.Zero)
            {
                playerHasMoved = true;
            }
            if (CannotHitPlayer && !CollideCheck(player))
            {
                CannotHitPlayer = false;
            }
            MoveH(Speed.X * Engine.DeltaTime);
            CustomSpinner spinner = CollideFirst<CustomSpinner>();
            if (spinner != null)
            {
                spinner.Destroy();
            }
        }

        public void SetHealth(int health)
        {
            Health = health;
        }

        public IEnumerator SequenceRoutine()
        {
            while (!SceneAs<Level>().Session.GetFlag("AncientGuardian_Start") || !playerHasMoved)
            {
                yield return null;
            }
            Add(new Coroutine(InvincibilityRoutine()));
            while (Health > 0)
            {
                float delay = SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode") ? 0.7f : 1f;
                Collidable = true;
                if (!Routine.Active)
                {
                    if (Health >= 13)
                    {
                        Add(Routine = new Coroutine(AttackPattern1(delay)));
                    }
                    else
                    {
                        if (CanMove)
                        {
                            if (StopPattern)
                            {
                                StopPattern = false;
                            }
                            if (TrackPosition == 0)
                            {
                                int moveRand = Calc.Random.Next(0, 2);
                                yield return MoveRoutine(moveRand == 0 ? -1 : 1);
                            }
                            else if (TrackPosition == -1)
                            {
                                yield return MoveRoutine(1);
                            }
                            else if (TrackPosition == 1)
                            {
                                yield return MoveRoutine(-1);
                            }
                        }
                        if (Health >= 5)
                        {
                            int pattern = Calc.Random.Next(1, 101);
                            if (pattern <= 33)
                            {
                                Add(Routine = new Coroutine(AttackPattern2(delay)));
                            }
                            else if (pattern <= 67)
                            {
                                Add(Routine = new Coroutine(AttackPattern3(delay)));
                            }
                            else
                            {
                                Add(Routine = new Coroutine(AttackPattern4(delay)));
                            }
                        }
                        else
                        {
                            int attack = Calc.Random.Next(1, 101);
                            if (attack <= 40)
                            {
                                Add(Routine = new Coroutine(LaserRoutine()));
                            }
                            else if (attack <= 80)
                            {
                                Add(Routine = new Coroutine(SprayFireBallsRoutine()));
                            }
                            else if (TrackPosition == 0)
                            {
                                Add(Routine = new Coroutine(SideFireballsRoutine()));
                            }
                        }
                    }
                }
                yield return null;
            }
            if (Routine.Active)
            {
                Routine.Cancel();
            }
            if (!ForcedDestroy)
            {
                Add(Routine = new Coroutine(DeathRoutine()));
            }
            ForcedDestroy = false;
            while (Health <= 0)
            {
                yield return null;
            }
            Visible = true;
            Add(new Coroutine(SequenceRoutine()));
        }

        public IEnumerator AttackPattern1(float delay)
        {
            yield return LaserRoutine();
            yield return delay;
            yield return SprayFireBallsRoutine();
            yield return delay;
            yield return FireballRoutine();
            yield return delay;
            yield return SideFireballsRoutine();
            yield return delay;
        }

        public IEnumerator AttackPattern2(float delay)
        {
            yield return SideFireballsRoutine();
            if (StopPattern)
            {
                yield break;
            }
            else
            {
                yield return delay;
            }
            yield return LaserRoutine();
            if (StopPattern)
            {
                yield break;
            }
            else
            {
                yield return delay;
            }
            yield return SprayFireBallsRoutine();
            if (StopPattern)
            {
                yield break;
            }
            else
            {
                yield return delay;
            }
            if (Health != 5)
            {
                yield return FireballRoutine();
                if (StopPattern)
                {
                    yield break;
                }
                else
                {
                    yield return delay;
                }
            }
            yield return SprayFireBallsRoutine();
            if (StopPattern)
            {
                yield break;
            }
            else
            {
                yield return delay;
            }
        }

        public IEnumerator AttackPattern3(float delay)
        {
            if (Health != 5)
            {
                yield return FireballRoutine();
                if (StopPattern)
                {
                    yield break;
                }
                else
                {
                    yield return delay;
                }
            }
            yield return SideFireballsRoutine();
            if (StopPattern)
            {
                yield break;
            }
            else
            {
                yield return delay;
            }
            yield return SprayFireBallsRoutine();
            if (StopPattern)
            {
                yield break;
            }
            else
            {
                yield return delay;
            }
            if (Health != 5)
            {
                yield return FireballRoutine();
                if (StopPattern)
                {
                    yield break;
                }
                else
                {
                    yield return delay;
                }
            }
            yield return LaserRoutine();
            if (StopPattern)
            {
                yield break;
            }
            else
            {
                yield return delay;
            }
        }

        public IEnumerator AttackPattern4(float delay)
        {
            yield return SprayFireBallsRoutine();
            if (StopPattern)
            {
                yield break;
            }
            else
            {
                yield return delay;
            }
            if (Health != 5)
            {
                yield return FireballRoutine();
                if (StopPattern)
                {
                    yield break;
                }
                else
                {
                    yield return delay;
                }
            }
            yield return SprayFireBallsRoutine();
            if (StopPattern)
            {
                yield break;
            }
            else
            {
                yield return delay;
            }
            yield return LaserRoutine();
            if (StopPattern)
            {
                yield break;
            }
            else
            {
                yield return delay;
            }
            yield return SideFireballsRoutine();
            if (StopPattern)
            {
                yield break;
            }
            else
            {
                yield return delay;
            }
        }

        public IEnumerator MoveRoutine(int dir)
        {
            float StartPosX = Position.X;
            LeftWheelSprite.Play("wheel");
            RightWheelSprite.Play("wheel");
            LeftWheelSprite.Rate = RightWheelSprite.Rate = dir > 0 ? -1 : 1;
            if (dir > 0 && TrackPosition <= 0)
            {
                Speed.X = 100f;
                while (Position.X < StartPosX + 52f)
                {
                    yield return null;
                }
                Speed.X = 0f;
                Position.X = StartPosX + 52f;
                TrackPosition++;
            }
            else if (dir < 0 && TrackPosition >= 0)
            {
                Speed.X = -100f;
                while (Position.X > StartPosX - 52f)
                {
                    yield return null;
                }
                Speed.X = 0f;
                Position.X = StartPosX - 52f;
                TrackPosition--;
            }
            LeftWheelSprite.Rate = RightWheelSprite.Rate = 0;
        }

        public IEnumerator LaserRoutine()
        {
            EyesSprite.Play("laser");
            Audio.Play("event:/game/xaphan/guardian_eyes_1", Position);
            Add(new Coroutine(EyesAlphaRoutine()));
            yield return 0.5f;
            if (SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode"))
            {
                Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
                if (player != null)
                {
                    int shoots = 0;
                    int moves = 0;
                    yield return 0.5f;
                    while (shoots < 3)
                    {
                        if (player.Center.X < Left && TrackPosition != -1)
                        {
                            yield return MoveRoutine(-1);
                            moves++;
                        }
                        else if (player.Center.X > Right && TrackPosition != 1)
                        {
                            yield return MoveRoutine(1);
                            moves++;
                        }
                        else
                        {

                            Audio.Play("event:/game/xaphan/guardian_laser", Position);
                            SceneAs<Level>().Add(new GuardianLaser(Position + new Vector2(25f, 23f)));
                            SceneAs<Level>().Add(new GuardianLaser(Position + new Vector2(34f, 23f), true));
                            shoots++;
                            moves = 0;
                            yield return 0.5f;
                        }
                        if (moves == 2)
                        {
                            Audio.Play("event:/game/xaphan/guardian_laser", Position);
                            SceneAs<Level>().Add(new GuardianLaser(Position + new Vector2(25f, 23f)));
                            SceneAs<Level>().Add(new GuardianLaser(Position + new Vector2(34f, 23f), true));
                            shoots++;
                            moves = 0;
                            yield return 0.5f;
                        }
                        yield return null;
                    }
                }
                if (TrackPosition != 0)
                {
                    yield return MoveRoutine(-TrackPosition);
                }
            }
            else
            {
                yield return 0.5f;
                Audio.Play("event:/game/xaphan/guardian_laser", Position);
                SceneAs<Level>().Add(new GuardianLaser(Position + new Vector2(25f, 23f)));
                SceneAs<Level>().Add(new GuardianLaser(Position + new Vector2(34f, 23f), true));
            }
            yield return 0.5f;
            Add(new Coroutine(EyesAlphaRoutine(true)));
            yield return 0.5f;
        }

        public IEnumerator SprayFireBallsRoutine()
        {
            EyesSprite.Play("sprayFireball");
            Audio.Play("event:/game/xaphan/guardian_eyes_2", Position);
            Add(new Coroutine(EyesAlphaRoutine()));
            yield return 1f;
            Audio.Play("event:/game/xaphan/guardian_fireball", Position);
            SceneAs<Level>().Add(new GuardianSprayFireBall(Position + new Vector2(33f), new Vector2(71f, 60f)));
            SceneAs<Level>().Add(new GuardianSprayFireBall(Position + new Vector2(33f), new Vector2(126f, 60f)));
            SceneAs<Level>().Add(new GuardianSprayFireBall(Position + new Vector2(33f), new Vector2(-71f, 60f)));
            SceneAs<Level>().Add(new GuardianSprayFireBall(Position + new Vector2(33f), new Vector2(-126f, 60f)));
            if (SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode"))
            {
                yield return 0.5f;
                Audio.Play("event:/game/xaphan/guardian_fireball", Position);
                SceneAs<Level>().Add(new GuardianSprayFireBall(Position + new Vector2(33f), Vector2.UnitY * 60f));
                SceneAs<Level>().Add(new GuardianSprayFireBall(Position + new Vector2(33f), new Vector2(71f, 60f)));
                SceneAs<Level>().Add(new GuardianSprayFireBall(Position + new Vector2(33f), new Vector2(-71f, 60f)));
                yield return 0.5f;
                Audio.Play("event:/game/xaphan/guardian_fireball", Position);
                SceneAs<Level>().Add(new GuardianSprayFireBall(Position + new Vector2(33f), Vector2.UnitY * 60f));
                SceneAs<Level>().Add(new GuardianSprayFireBall(Position + new Vector2(33f), new Vector2(71f, 60f)));
                SceneAs<Level>().Add(new GuardianSprayFireBall(Position + new Vector2(33f), new Vector2(126f, 60f)));
                SceneAs<Level>().Add(new GuardianSprayFireBall(Position + new Vector2(33f), new Vector2(-71f, 60f)));
                SceneAs<Level>().Add(new GuardianSprayFireBall(Position + new Vector2(33f), new Vector2(-126f, 60f)));
            }
            yield return 0.5f;
            Add(new Coroutine(EyesAlphaRoutine(true)));
            yield return 0.5f;
        }

        public IEnumerator FireballRoutine()
        {
            EyesSprite.Play("fireball");
            Audio.Play("event:/game/xaphan/guardian_eyes_3", Position);
            Add(new Coroutine(EyesAlphaRoutine()));
            yield return 1f;
            Audio.Play("event:/game/xaphan/guardian_fireball", Position);
            if (SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode"))
            {
                SceneAs<Level>().Add(new GuardianFireBall(Position + new Vector2(33f), Vector2.UnitY * 60f, false));
                SceneAs<Level>().Add(new GuardianFireBall(Position + new Vector2(33f), new Vector2(71f, 60f), false));
                SceneAs<Level>().Add(new GuardianFireBall(Position + new Vector2(33f), new Vector2(-71f, 60f), false));
            }
            else
            {
                SceneAs<Level>().Add(new GuardianFireBall(Position + new Vector2(33f), Vector2.UnitY * 60f, true));
            }
            yield return 0.5f;
            Add(new Coroutine(EyesAlphaRoutine(true)));
            yield return 0.5f;
        }

        public IEnumerator SideFireballsRoutine()
        {
            EyesSprite.Play("sideFireball");
            Audio.Play("event:/game/xaphan/guardian_eyes_4", Position);
            Add(new Coroutine(EyesAlphaRoutine()));
            yield return 1f;
            Audio.Play("event:/game/xaphan/guardian_fireball", Position);
            float horizontalSpeed = SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode") ? 100f : 125f;
            if (SceneAs<Level>().Session.GetFlag("boss_Challenge_Mode"))
            {
                SceneAs<Level>().Add(new GuardianSideFireBall(Position + new Vector2(33f), new Vector2(horizontalSpeed, 75f)));
                SceneAs<Level>().Add(new GuardianSideFireBall(Position + new Vector2(33f), new Vector2(horizontalSpeed, 150f)));
                SceneAs<Level>().Add(new GuardianSideFireBall(Position + new Vector2(33f), new Vector2(horizontalSpeed, 225f)));
                SceneAs<Level>().Add(new GuardianSideFireBall(Position + new Vector2(33f), new Vector2(-horizontalSpeed, 75f)));
                SceneAs<Level>().Add(new GuardianSideFireBall(Position + new Vector2(33f), new Vector2(-horizontalSpeed, 150f)));
                SceneAs<Level>().Add(new GuardianSideFireBall(Position + new Vector2(33f), new Vector2(-horizontalSpeed, 225f)));
            }
            else
            {
                SceneAs<Level>().Add(new GuardianSideFireBall(Position + new Vector2(33f), new Vector2(horizontalSpeed, 100f)));
                SceneAs<Level>().Add(new GuardianSideFireBall(Position + new Vector2(33f), new Vector2(horizontalSpeed, 200f)));
                SceneAs<Level>().Add(new GuardianSideFireBall(Position + new Vector2(33f), new Vector2(-horizontalSpeed, 100f)));
                SceneAs<Level>().Add(new GuardianSideFireBall(Position + new Vector2(33f), new Vector2(-horizontalSpeed, 200f)));
            }
            yield return 0.5f;
            Add(new Coroutine(EyesAlphaRoutine(true)));
            yield return 0.5f;
        }

        public IEnumerator DeathRoutine(bool skipAnim = false)
        {
            Health = 0;
            InvincibilityDelay = 0f;
            Speed = Vector2.Zero;
            Collidable = false;
            if (!skipAnim)
            {
                Audio.Play("event:/game/xaphan/guardian_death", Position);
                float musicFadeStart = 0f;
                while (musicFadeStart < 1)
                {
                    musicFadeStart += Engine.DeltaTime;
                    Audio.SetMusicParam("fade", 1f - musicFadeStart);
                    yield return null;
                }
                yield return 0.75f;
            }
            SceneAs<Level>().Displacement.AddBurst(Center, 0.5f, 16f, 64f, 0.5f);
            for (int i = 0; i < 28; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    Vector2 vector2 = new(i * 2 + 4f, j * 4 + 4f);
                    GuardianDebris debris = Engine.Pooler.Create<GuardianDebris>().Init(Position + Collider.Position - Vector2.UnitX * 16 + vector2, Position + Collider.Center);
                    Scene.Add(debris);
                }
            }
            Audio.Play("event:/game/xaphan/drone_destroy", Position);
            Audio.SetMusicParam("fade", 1f);
            SceneAs<Level>().Session.Audio.Music.Event = SFX.EventnameByHandle("event:/music/xaphan/lvl_0_item");
            SceneAs<Level>().Session.Audio.Apply();
            Visible = false;
            foreach (GuardianGate gate in SceneAs<Level>().Tracker.GetEntities<GuardianGate>())
            {
                gate.Break();
            }
            foreach (CustomRefill refill in SceneAs<Level>().Tracker.GetEntities<CustomRefill>())
            {
                SceneAs<Level>().Displacement.AddBurst(refill.Center, 0.5f, 8f, 32f, 0.5f);
                refill.RemoveSelf();
            }
            while (SceneAs<Level>().Session.GetFlag("In_bossfight"))
            {
                yield return null;
            }
            Position = OrigPosition;
            SceneAs<Level>().Session.SetFlag("AncientGuardian_Gates", false);
        }

        public IEnumerator EyesAlphaRoutine(bool fadeOut = false)
        {
            float timer = 0.5f;
            if (fadeOut)
            {
                while (timer > 0f)
                {
                    EyesAlpha -= Engine.DeltaTime * 3;
                    timer -= Engine.DeltaTime * 2;
                    yield return null;
                }
                EyesAlpha = 0f;
            }
            else
            {
                while (timer > 0f)
                {
                    EyesAlpha += Engine.DeltaTime * 3;
                    timer -= Engine.DeltaTime * 2;
                    yield return null;
                }
            }
        }

        public IEnumerator InvincibilityRoutine()
        {
            while (Health > 0)
            {
                while (InvincibilityDelay > 0)
                {
                    if (Scene.OnRawInterval(0.06f))
                    {
                        Flashing = !Flashing;
                    }
                    InvincibilityDelay -= Engine.DeltaTime;
                    yield return null;
                }
                InvincibilityDelay = 0;
                Flashing = false;
                yield return null;
            }
        }
    }
}
