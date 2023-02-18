﻿using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.XaphanHelper.Upgrades;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    public class Bomb : Actor
    {
        private Sprite bombSprite;

        public Vector2 Speed;

        public float noGravityTimer;

        private Vector2 previousPosition;

        private Vector2 prevLiftSpeed;

        private bool explode;

        private Player player;

        private Collision onCollideH;

        private Collision onCollideV;

        public Holdable Hold;

        public static Holdable Hold2;

        private HoldableCollider hitSeeker;

        private float swatTimer;

        private bool shouldExplodeImmediately;

        public Bomb(Vector2 position, Player player) : base(position)
        {
            this.player = player;
            Collider = new Hitbox(8f, 7f, -4f, -7f);
            Add(bombSprite = new Sprite(GFX.Game, "upgrades/Bomb/"));
            bombSprite.AddLoop("idle", "idle", 1f, 0);
            bombSprite.AddLoop("countdown", "idle", 0.08f);
            bombSprite.AddLoop("explode", "explode", 0.08f);
            bombSprite.CenterOrigin();
            bombSprite.Justify = new Vector2(0.5f, 1f);
            bombSprite.Play("idle");
            Add(Hold = new Holdable(0.1f));
            Hold2 = Hold;
            Hold.PickupCollider = new Hitbox(12f, 14f, -6f, -14f);
            Hold.SlowFall = false;
            Hold.SlowRun = false;
            Hold.OnPickup = OnPickup;
            Hold.OnRelease = OnRelease;
            Hold.DangerousCheck = Dangerous;
            Hold.OnHitSeeker = HitSeeker;
            Hold.OnSwat = Swat;
            Hold.OnHitSpring = HitSpring;
            Hold.SpeedGetter = (() => Speed);
            onCollideH = OnCollideH;
            onCollideV = OnCollideV;
            Depth = 0;
        }

        private void OnPickup()
        {
            Speed = Vector2.Zero;
            AddTag(Tags.Persistent);
            AllowPushing = false;
        }

        private void OnRelease(Vector2 force)
        {
            RemoveTag(Tags.Persistent);
            if (force.X != 0f && force.Y == 0f)
            {
                force.Y = -0.4f;
            }
            Speed = force * 200f;
            if (Speed != Vector2.Zero)
            {
                noGravityTimer = 0.1f;
            }
            AllowPushing = true;
        }

        public void Swat(HoldableCollider hc, int dir)
        {
            if (Hold.IsHeld && hitSeeker == null)
            {
                swatTimer = 0.1f;
                hitSeeker = hc;
                Hold.Holder.Swat(dir);
            }
        }

        public bool Dangerous(HoldableCollider holdableCollider)
        {
            return !Hold.IsHeld && Speed != Vector2.Zero && hitSeeker != holdableCollider;
        }

        public void HitSeeker(Seeker seeker)
        {
            if (!Hold.IsHeld)
            {
                Speed = (Center - seeker.Center).SafeNormalize(120f);
            }
            Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_side", Position);
        }

        public bool HitSpring(Spring spring)
        {
            if (!Hold.IsHeld)
            {
                if (spring.Orientation == Spring.Orientations.Floor && Speed.Y >= 0f)
                {
                    Speed.X *= 0.5f;
                    Speed.Y = -160f;
                    noGravityTimer = 0.15f;
                    return true;
                }
                if (spring.Orientation == Spring.Orientations.WallLeft && Speed.X <= 0f)
                {
                    MoveTowardsY(spring.CenterY + 5f, 4f);
                    Speed.X = 220f;
                    Speed.Y = -80f;
                    noGravityTimer = 0.1f;
                    return true;
                }
                if (spring.Orientation == Spring.Orientations.WallRight && Speed.X >= 0f)
                {
                    MoveTowardsY(spring.CenterY + 5f, 4f);
                    Speed.X = -220f;
                    Speed.Y = -80f;
                    noGravityTimer = 0.1f;
                    return true;
                }
            }
            return false;
        }

        private void OnCollideH(CollisionData data)
        {
            Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_side", Position);
            if (Math.Abs(Speed.X) > 100f)
            {
                ImpactParticles(data.Direction);
            }
            Speed.X *= -0.2f;
        }

        private void OnCollideV(CollisionData data)
        {
            if (Speed.Y > 0f)
            {
                Audio.Play("event:/game/05_mirror_temple/crystaltheo_hit_ground", Position, "crystal_velocity", 0f);
            }
            if (Speed.Y > 160f)
            {
                ImpactParticles(data.Direction);
            }
            if (Speed.Y > 140f && !(data.Hit is SwapBlock) && !(data.Hit is DashSwitch))
            {
                Speed.Y *= -0.4f;
            }
            else
            {
                Speed.Y = 0f;
            }
        }

        private void ImpactParticles(Vector2 dir)
        {
            float direction;
            Vector2 position = default(Vector2);
            Vector2 positionRange;
            if (dir.X > 0f)
            {
                direction = (float)Math.PI;
                position = new Vector2(Right, Y - 4f);
                positionRange = Vector2.UnitY * 6f;
            }
            else if (dir.X < 0f)
            {
                direction = 0f;
                position = new Vector2(Left, Y - 4f);
                positionRange = Vector2.UnitY * 6f;
            }
            else if (dir.Y > 0f)
            {
                direction = -(float)Math.PI / 2f;
                position = new Vector2(X, Bottom);
                positionRange = Vector2.UnitX * 6f;
            }
            else
            {
                direction = (float)Math.PI / 2f;
                position = new Vector2(X, Top);
                positionRange = Vector2.UnitX * 6f;
            }
            SceneAs<Level>().Particles.Emit(TheoCrystal.P_Impact, 12, position, positionRange, direction);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (!XaphanModule.ModSettings.UseBagItemSlot.Check)
            {
                RemoveSelf();
            }
            else
            {
                Add(new Coroutine(Explode()));
            }
        }

        public override void Update()
        {
            Slope.SetCollisionBeforeUpdate(this);
            base.Update();
            if (Hold.IsHeld)
            {
                if (player.Facing == Facings.Right)
                {
                    bombSprite.FlipX = true;
                }
                else
                {
                    bombSprite.FlipX = false;
                }
            }
            else
            {
                foreach (Slope slope in SceneAs<Level>().Tracker.GetEntities<Slope>())
                {
                    if (slope.UpsideDown && CollideCheck(slope))
                    {
                        Position.Y += 1;
                    }
                }
            }
            if (swatTimer > 0f)
            {
                swatTimer -= Engine.DeltaTime;
            }
            if (hitSeeker != null && swatTimer <= 0f && !hitSeeker.Check(Hold))
            {
                hitSeeker = null;
            }
            if (!explode && !Hold.IsHeld)
            {
                if (OnGround())
                {
                    float target = (!OnGround(Position + Vector2.UnitX * 3f)) ? 20f : (OnGround(Position - Vector2.UnitX * 3f) ? 0f : (-20f));
                    Speed.X = Calc.Approach(Speed.X, target, 800f * Engine.DeltaTime);
                    Vector2 liftSpeed = LiftSpeed;
                    if (liftSpeed == Vector2.Zero && prevLiftSpeed != Vector2.Zero)
                    {
                        Speed = prevLiftSpeed;
                        prevLiftSpeed = Vector2.Zero;
                        Speed.Y = Math.Min(Speed.Y * 0.6f, 0f);
                        if (Speed.X != 0f && Speed.Y == 0f)
                        {
                            Speed.Y = -60f;
                        }
                        if (Speed.Y < 0f)
                        {
                            noGravityTimer = 0.15f;
                        }
                    }
                    else
                    {
                        prevLiftSpeed = liftSpeed;
                        if (liftSpeed.Y < 0f && Speed.Y < 0f)
                        {
                            Speed.Y = 0f;
                        }
                    }
                }
                else
                {
                    float num = 800f;
                    if (Math.Abs(Speed.Y) <= 30f)
                    {
                        num *= 0.5f;
                    }
                    float num2 = 350f;
                    if (Speed.Y < 0f)
                    {
                        num2 *= 0.5f;
                    }
                    Speed.X = Calc.Approach(Speed.X, 0f, num2 * Engine.DeltaTime);
                    if (noGravityTimer > 0f)
                    {
                        noGravityTimer -= Engine.DeltaTime;
                    }
                    else
                    {
                        Speed.Y = Calc.Approach(Speed.Y, 200f, num * Engine.DeltaTime);
                    }
                }
                previousPosition = ExactPosition;
                MoveH(Speed.X * Engine.DeltaTime, onCollideH);
                MoveV(Speed.Y * Engine.DeltaTime, onCollideV);
                foreach (KeyValuePair<Type, List<Entity>> entityList in Scene.Tracker.Entities)
                {
                    if (entityList.Key == typeof(Liquid))
                    {
                        foreach (Entity entity in entityList.Value)
                        {
                            Liquid liquid = (Liquid)entity;
                            if (CollideCheck(liquid) && (liquid.liquidType == "lava" || liquid.liquidType.Contains("acid")))
                            {
                                shouldExplodeImmediately = true;
                            }
                        }

                    }
                    else if (entityList.Key == typeof(LaserBeam))
                    {
                        foreach (Entity entity in entityList.Value)
                        {
                            LaserBeam beam = (LaserBeam)entity;
                            if (CollideCheck(beam) && (beam.Type == "Kill" || beam.Type == "Must Dash"))
                            {
                                shouldExplodeImmediately = true;
                            }
                        }
                    }
                }
                if (Left > SceneAs<Level>().Bounds.Right || Right < SceneAs<Level>().Bounds.Left || Top > SceneAs<Level>().Bounds.Bottom)
                {
                    RemoveSelf();
                }
            }
            if (!explode)
            {
                Hold.CheckAgainstColliders();
            }
            Slope.SetCollisionAfterUpdate(this);
        }

        public IEnumerator Explode()
        {
            while (Hold.IsHeld)
            {
                yield return null;
            }
            bombSprite.Play("countdown");
            float timer = 2f;
            while (timer >= 0 && !shouldExplodeImmediately)
            {
                yield return null;
                timer -= Engine.DeltaTime;
            }
            AllowPushing = false;
            Collider = new Circle(12f, 0f, -4f);
            Hold.PickupCollider = Collider;
            Speed = Vector2.Zero;
            noGravityTimer = 0.01f;
            yield return 0.01f;
            Hold.RemoveSelf();
            explode = true;
            bombSprite.Position += new Vector2(0, 12);
            Audio.Play("event:/game/xaphan/bomb_explode", Position);
            bombSprite.Play("explode", false);
            bombSprite.OnLastFrame = onLastFrame;
            while (bombSprite.CurrentAnimationFrame < 1)
            {
                yield return null;
            }
            Player player = CollideFirst<Player>();
            if (player != null && player.StateMachine.State != 11 && !Scene.CollideCheck<Solid>(Position + new Vector2(0, -10f), player.Center))
            {
                int dirX = 0;
                int dirY = 0;
                float dist = 0;
                if (player.Position.X < Position.X)
                {
                    dirX = -1;
                }
                else if (player.Position.X > Position.X)
                {
                    dirX = 1;
                }
                if (player.Position.Y <= Position.Y)
                {
                    dirY = -1;
                }
                else if (player.Position.Y > Position.Y)
                {
                    dirY = 1;
                }
                dist = (player.Position.X - Position.X) * 15;
                if (dist < 0)
                {
                    dist = -dist;
                }
                player.Speed.Y = ((180f - dist) * dirY) * (GravityJacket.determineIfInWater() && !XaphanModule.ModSettings.GravityJacket ? 0.8f : 1f);
                player.Speed.X = (180f * dirX) * (GravityJacket.determineIfInWater() && !XaphanModule.ModSettings.GravityJacket ? 0.8f : 1f);
            }

            foreach (Entity entity in Scene.Tracker.GetEntities<BreakBlockIndicator>())
            {
                BreakBlockIndicator breakBlockIndicator = (BreakBlockIndicator)entity;
                if (breakBlockIndicator.mode == "Bomb")
                {
                    if (CollideCheck(breakBlockIndicator))
                    {
                        breakBlockIndicator.BreakSequence();
                    }
                }
            }

            foreach (WorkRobot workRobot in Scene.Tracker.GetEntities<WorkRobot>())
            {
                int dir = 0;
                if (workRobot.Position.X < Position.X)
                {
                    dir = -1;
                }
                else if (workRobot.Position.X > Position.X)
                {
                    dir = 1;
                }
                if (CollideCheck(workRobot, Position + new Vector2(dir, 0)))
                {
                    workRobot.Push(new Vector2(75, 0), new Vector2(dir, 0));
                }
            }
        }

        private void onLastFrame(string s)
        {
            Visible = false;
            RemoveSelf();
        }
    }
}
