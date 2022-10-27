﻿using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [CustomEntity("XaphanHelper/CustomMoveBlock")]
    public class CustomMoveBlock : Solid
    {
        public enum Directions
        {
            Left,
            Right,
            Up,
            Down
        }

        private enum MovementState
        {
            Idling,
            Moving,
            Breaking
        }

        private class Border : Entity
        {
            public CustomMoveBlock Parent;

            public Border(CustomMoveBlock parent)
            {
                Parent = parent;
                Depth = parent.Depth + 1;
            }

            public override void Update()
            {
                if (Parent.Scene != Scene)
                {
                    RemoveSelf();
                }
                base.Update();
            }

            public override void Render()
            {
                Draw.Rect(Parent.X + Parent.Shake.X - 1f, Parent.Y + Parent.Shake.Y - 1f, Parent.Width + 2f, Parent.Height + 2f, Color.Black);
            }
        }

        [Pooled]
        private class Debris : Actor
        {
            private Image sprite;

            private Vector2 home;

            private Vector2 speed;

            private bool shaking;

            private bool firstHit;

            private float alpha;

            private Collision onCollideH;

            private Collision onCollideV;

            private float spin;

            private float lifeTimer;

            private float fadeLerp;

            public Debris() : base(Vector2.Zero)
            {
                Tag = Tags.TransitionUpdate;
                Collider = new Hitbox(4f, 4f, -2f, -2f);
                Add(sprite = new Image(Calc.Random.Choose(GFX.Game.GetAtlasSubtextures("objects/moveblock/debris"))));
                sprite.CenterOrigin();
                sprite.FlipX = Calc.Random.Chance(0.5f);
                onCollideH = delegate
                {
                    speed.X = (0f - speed.X) * 0.5f;
                };
                onCollideV = delegate
                {
                    if (firstHit || speed.Y > 50f)
                    {
                        Audio.Play("event:/game/general/debris_stone", Position, "debris_velocity", Calc.ClampedMap(speed.Y, 0f, 600f));
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

            public Debris Init(Vector2 position, Vector2 center, Vector2 returnTo)
            {
                Collidable = true;
                Position = position;
                speed = (position - center).SafeNormalize(60f + Calc.Random.NextFloat(60f));
                home = returnTo;
                sprite.Position = Vector2.Zero;
                sprite.Rotation = Calc.Random.NextAngle();
                shaking = false;
                sprite.Scale.X = 1f;
                sprite.Scale.Y = 1f;
                sprite.Color = Color.White;
                alpha = 1f;
                firstHit = false;
                spin = Calc.Random.Range(3.49065852f, 10.4719753f) * Calc.Random.Choose(1, -1);
                fadeLerp = 0f;
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
                if (fadeLerp < 1f)
                {
                    fadeLerp = Calc.Approach(fadeLerp, 1f, 2f * Engine.DeltaTime);
                }
                sprite.Color = Color.Lerp(Color.White, Color.Gray, fadeLerp) * alpha;
            }

            public void StopMoving()
            {
                Collidable = false;
            }

        }

        public static ParticleType P_Activate;

        public static ParticleType P_Break;

        public static ParticleType P_Move;

        private const float Accel = 300f;

        private const float MoveSpeed = 60f;

        private const float FastMoveSpeed = 75f;

        private const float SteerSpeed = (float)Math.PI * 16f;

        private const float MaxAngle = (float)Math.PI / 4f;

        private const float NoSteerTime = 0.2f;

        private const float CrashTime = 0.15f;

        private const float CrashResetTime = 0.1f;

        private const float RegenTime = 3f;

        private bool canSteer;

        private bool fast;

        private Directions direction;

        private float homeAngle;

        private int angleSteerSign;

        private Vector2 startPosition;

        private MovementState state;

        private bool leftPressed;

        private bool rightPressed;

        private bool topPressed;

        private float speed;

        private float targetSpeed;

        private float angle;

        private float targetAngle;

        private Player noSquish;

        private List<Image> body = new List<Image>();

        private List<Image> topButton = new List<Image>();

        private List<Image> leftButton = new List<Image>();

        private List<Image> rightButton = new List<Image>();

        private List<MTexture> arrows = new List<MTexture>();

        private Border border;

        private Color fillColor = idleBgFill;

        private float flash;

        private SoundSource moveSfx;

        private bool triggered;

        private static readonly Color idleBgFill = Calc.HexToColor("474070");

        private static readonly Color pressedBgFill = Calc.HexToColor("30b335");

        private static readonly Color breakingBgFill = Calc.HexToColor("cc2541");

        private float particleRemainder;

        public CustomMoveBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, false)
        {
            startPosition = Position;
            canSteer = data.Bool("canSteer", false);
            direction = data.Enum("direction", Directions.Right);
            fast = data.Bool("fast");
            switch (direction)
            {
                default:
                    homeAngle = (targetAngle = (angle = 0f));
                    angleSteerSign = 1;
                    break;
                case Directions.Left:
                    homeAngle = (targetAngle = (angle = (float)Math.PI));
                    angleSteerSign = -1;
                    break;
                case Directions.Up:
                    homeAngle = (targetAngle = (angle = -(float)Math.PI / 2f));
                    angleSteerSign = 1;
                    break;
                case Directions.Down:
                    homeAngle = (targetAngle = (angle = (float)Math.PI / 2f));
                    angleSteerSign = -1;
                    break;
            }
            int num = (int)Width / 8;
            int num2 = (int)Height / 8;
            MTexture mTexture = GFX.Game["objects/moveBlock/base"];
            MTexture mTexture2 = GFX.Game["objects/moveBlock/button"];
            if (canSteer && (direction == Directions.Left || direction == Directions.Right))
            {
                for (int i = 0; i < num; i++)
                {
                    int num3 = ((i != 0) ? ((i < num - 1) ? 1 : 2) : 0);
                    AddImage(mTexture2.GetSubtexture(num3 * 8, 0, 8, 8), new Vector2(i * 8, -4f), 0f, new Vector2(1f, 1f), topButton);
                }
                mTexture = GFX.Game["objects/moveBlock/base_h"];
            }
            else if (canSteer && (direction == Directions.Up || direction == Directions.Down))
            {
                for (int j = 0; j < num2; j++)
                {
                    int num4 = ((j != 0) ? ((j < num2 - 1) ? 1 : 2) : 0);
                    AddImage(mTexture2.GetSubtexture(num4 * 8, 0, 8, 8), new Vector2(-4f, j * 8), (float)Math.PI / 2f, new Vector2(1f, -1f), leftButton);
                    AddImage(mTexture2.GetSubtexture(num4 * 8, 0, 8, 8), new Vector2((num - 1) * 8 + 4, j * 8), (float)Math.PI / 2f, new Vector2(1f, 1f), rightButton);
                }
                mTexture = GFX.Game["objects/moveBlock/base_v"];
            }
            for (int k = 0; k < num; k++)
            {
                for (int l = 0; l < num2; l++)
                {
                    int num5 = ((k != 0) ? ((k < num - 1) ? 1 : 2) : 0);
                    int num6 = ((l != 0) ? ((l < num2 - 1) ? 1 : 2) : 0);
                    AddImage(mTexture.GetSubtexture(num5 * 8, num6 * 8, 8, 8), new Vector2(k, l) * 8f, 0f, new Vector2(1f, 1f), body);
                }
            }
            arrows = GFX.Game.GetAtlasSubtextures("objects/moveBlock/arrow");
            Add(moveSfx = new SoundSource());
            Add(new Coroutine(Controller()));
            UpdateColors();
            Add(new LightOcclude(0.5f));
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            scene.Add(border = new Border(this));
        }

        private IEnumerator Controller()
        {
            while (true)
            {
                triggered = false;
                state = MovementState.Idling;
                while (!triggered && !HasPlayerRider())
                {
                    yield return null;
                }
                Audio.Play("event:/game/04_cliffside/arrowblock_activate", Position);
                state = MovementState.Moving;
                StartShaking(0.2f);
                ActivateParticles();
                yield return 0.2f;
                targetSpeed = (fast ? 75f : 60f);
                moveSfx.Play("event:/game/04_cliffside/arrowblock_move");
                moveSfx.Param("arrow_stop", 0f);
                StopPlayerRunIntoAnimation = false;
                float crashTimer = 0.15f;
                float crashResetTimer = 0.1f;
                float noSteerTimer = 0.2f;
                while (true)
                {
                    if (canSteer)
                    {
                        targetAngle = homeAngle;
                        bool flag = ((direction != Directions.Right && direction != 0) ? HasPlayerClimbing() : HasPlayerOnTop());
                        if (flag && noSteerTimer > 0f)
                        {
                            noSteerTimer -= Engine.DeltaTime;
                        }
                        if (flag)
                        {
                            if (noSteerTimer <= 0f)
                            {
                                if (direction == Directions.Right || direction == Directions.Left)
                                {
                                    targetAngle = homeAngle + (float)Math.PI / 4f * angleSteerSign * Input.MoveY.Value;
                                }
                                else
                                {
                                    targetAngle = homeAngle + (float)Math.PI / 4f * angleSteerSign * Input.MoveX.Value;
                                }
                            }
                        }
                        else
                        {
                            noSteerTimer = 0.2f;
                        }
                    }
                    if (Scene.OnInterval(0.02f))
                    {
                        MoveParticles();
                    }
                    speed = Calc.Approach(speed, targetSpeed, 300f * Engine.DeltaTime);
                    angle = Calc.Approach(angle, targetAngle, (float)Math.PI * 16f * Engine.DeltaTime);
                    Vector2 vector = Calc.AngleToVector(angle, speed);
                    Vector2 vec = vector * Engine.DeltaTime;
                    bool flag2;
                    if (direction == Directions.Right || direction == Directions.Left)
                    {
                        flag2 = MoveCheck(vec.XComp());
                        noSquish = Scene.Tracker.GetEntity<Player>();
                        MoveVCollideSolids(vec.Y, thruDashBlocks: false);
                        noSquish = null;
                        LiftSpeed = vector;
                        if (Scene.OnInterval(0.03f))
                        {
                            if (vec.Y > 0f)
                            {
                                ScrapeParticles(Vector2.UnitY);
                            }
                            else if (vec.Y < 0f)
                            {
                                ScrapeParticles(-Vector2.UnitY);
                            }
                        }
                    }
                    else
                    {
                        flag2 = MoveCheck(vec.YComp());
                        noSquish = Scene.Tracker.GetEntity<Player>();
                        MoveHCollideSolids(vec.X, thruDashBlocks: false);
                        noSquish = null;
                        LiftSpeed = vector;
                        if (Scene.OnInterval(0.03f))
                        {
                            if (vec.X > 0f)
                            {
                                ScrapeParticles(Vector2.UnitX);
                            }
                            else if (vec.X < 0f)
                            {
                                ScrapeParticles(-Vector2.UnitX);
                            }
                        }
                        if (direction == Directions.Down && Top > (SceneAs<Level>().Bounds.Bottom + 32))
                        {
                            flag2 = true;
                        }
                    }
                    if (flag2)
                    {
                        moveSfx.Param("arrow_stop", 1f);
                        crashResetTimer = 0.1f;
                        if (!(crashTimer > 0f))
                        {
                            break;
                        }
                        crashTimer -= Engine.DeltaTime;
                    }
                    else
                    {
                        moveSfx.Param("arrow_stop", 0f);
                        if (crashResetTimer > 0f)
                        {
                            crashResetTimer -= Engine.DeltaTime;
                        }
                        else
                        {
                            crashTimer = 0.15f;
                        }
                    }
                    Level level = Scene as Level;
                    if (Left < level.Bounds.Left || Top < level.Bounds.Top || Right > level.Bounds.Right)
                    {
                        break;
                    }
                    yield return null;
                }
                Audio.Play("event:/game/04_cliffside/arrowblock_break", Position);
                moveSfx.Stop();
                state = MovementState.Breaking;
                speed = (targetSpeed = 0f);
                angle = (targetAngle = homeAngle);
                StartShaking(0.2f);
                StopPlayerRunIntoAnimation = true;
                yield return 0.2f;
                BreakParticles();
                List<Debris> debris = new List<Debris>();
                for (int i = 0; i < Width; i += 8)
                {
                    for (int j = 0; j < Height; j += 8)
                    {
                        Vector2 vector2 = new Vector2(i + 4f, j + 4f);
                        Debris debris2 = Engine.Pooler.Create<Debris>().Init(Position + vector2, Center, startPosition + vector2);
                        debris.Add(debris2);
                        Scene.Add(debris2);
                    }
                }
                CustomMoveBlock moveBlock = this;
                Vector2 amount = startPosition - Position;
                DisableStaticMovers();
                moveBlock.MoveStaticMovers(amount);
                Position = startPosition;
                Visible = (Collidable = false);
                yield return 2.2f;
                foreach (Debris item in debris)
                {
                    item.StopMoving();
                }
                while (CollideCheck<Actor>() || CollideCheck<Solid>())
                {
                    yield return null;
                }
                Collidable = true;
                EventInstance instance = Audio.Play("event:/game/04_cliffside/arrowblock_reform_begin", debris[0].Position);
                CustomMoveBlock moveBlock2 = this;
                Coroutine component;
                Coroutine routine = (component = new Coroutine(SoundFollowsDebrisCenter(instance, debris)));
                moveBlock2.Add(component);
                routine.RemoveSelf();
                Audio.Play("event:/game/04_cliffside/arrowblock_reappear", Position);
                Visible = true;
                EnableStaticMovers();
                speed = (targetSpeed = 0f);
                angle = (targetAngle = homeAngle);
                noSquish = null;
                fillColor = idleBgFill;
                UpdateColors();
                flash = 1f;
            }
        }

        private IEnumerator SoundFollowsDebrisCenter(EventInstance instance, List<Debris> debris)
        {
            while (true)
            {
                instance.getPlaybackState(out var pLAYBACK_STATE);
                if (pLAYBACK_STATE == PLAYBACK_STATE.STOPPED)
                {
                    break;
                }
                Vector2 zero = Vector2.Zero;
                foreach (Debris debri in debris)
                {
                    zero += debri.Position;
                }
                zero /= debris.Count;
                Audio.Position(instance, zero);
                yield return null;
            }
        }

        public override void Update()
        {
            base.Update();
            if (canSteer)
            {
                bool flag = (direction == Directions.Up || direction == Directions.Down) && CollideCheck<Player>(Position + new Vector2(-1f, 0f));
                bool flag2 = (direction == Directions.Up || direction == Directions.Down) && CollideCheck<Player>(Position + new Vector2(1f, 0f));
                bool flag3 = (direction == Directions.Left || direction == Directions.Right) && CollideCheck<Player>(Position + new Vector2(0f, -1f));
                foreach (Image item in topButton)
                {
                    item.Y = (flag3 ? 2 : 0);
                }
                foreach (Image item2 in leftButton)
                {
                    item2.X = (flag ? 2 : 0);
                }
                foreach (Image item3 in rightButton)
                {
                    item3.X = Width + (flag2 ? (-2) : 0);
                }
                if ((flag && !leftPressed) || (flag3 && !topPressed) || (flag2 && !rightPressed))
                {
                    Audio.Play("event:/game/04_cliffside/arrowblock_side_depress", Position);
                }
                if ((!flag && leftPressed) || (!flag3 && topPressed) || (!flag2 && rightPressed))
                {
                    Audio.Play("event:/game/04_cliffside/arrowblock_side_release", Position);
                }
                leftPressed = flag;
                rightPressed = flag2;
                topPressed = flag3;
            }
            if (moveSfx != null && moveSfx.Playing)
            {
                int num = (int)Math.Floor((0f - (Calc.AngleToVector(angle, 1f) * new Vector2(-1f, 1f)).Angle() + (float)Math.PI * 2f) % ((float)Math.PI * 2f) / ((float)Math.PI * 2f) * 8f + 0.5f);
                moveSfx.Param("arrow_influence", num + 1);
            }
            border.Visible = Visible;
            flash = Calc.Approach(flash, 0f, Engine.DeltaTime * 5f);
            UpdateColors();
        }

        public override void OnStaticMoverTrigger(StaticMover sm)
        {
            triggered = true;
        }

        public override void MoveHExact(int move)
        {
            if (noSquish != null && ((move < 0 && noSquish.X < X) || (move > 0 && noSquish.X > X)))
            {
                while (move != 0 && noSquish.CollideCheck<Solid>(noSquish.Position + Vector2.UnitX * move))
                {
                    move -= Math.Sign(move);
                }
            }
            base.MoveHExact(move);
        }

        public override void MoveVExact(int move)
        {
            if (noSquish != null && move < 0 && noSquish.Y <= Y)
            {
                while (move != 0 && noSquish.CollideCheck<Solid>(noSquish.Position + Vector2.UnitY * move))
                {
                    move -= Math.Sign(move);
                }
            }
            base.MoveVExact(move);
        }

        private bool MoveCheck(Vector2 speed)
        {
            if (speed.X != 0f)
            {
                if (MoveHCollideSolids(speed.X, thruDashBlocks: false))
                {
                    for (int i = 1; i <= 3; i++)
                    {
                        for (int num = 1; num >= -1; num -= 2)
                        {
                            Vector2 vector = new Vector2(Math.Sign(speed.X), i * num);
                            if (!CollideCheck<Solid>(Position + vector))
                            {
                                MoveVExact(i * num);
                                MoveHExact(Math.Sign(speed.X));
                                return false;
                            }
                        }
                    }
                    return true;
                }
                return false;
            }
            if (speed.Y != 0f)
            {
                if (MoveVCollideSolids(speed.Y, thruDashBlocks: false))
                {
                    for (int j = 1; j <= 3; j++)
                    {
                        for (int num2 = 1; num2 >= -1; num2 -= 2)
                        {
                            Vector2 vector2 = new Vector2(j * num2, Math.Sign(speed.Y));
                            if (!CollideCheck<Solid>(Position + vector2))
                            {
                                MoveHExact(j * num2);
                                MoveVExact(Math.Sign(speed.Y));
                                return false;
                            }
                        }
                    }
                    return true;
                }
                return false;
            }
            return false;
        }

        private void UpdateColors()
        {
            Color value = idleBgFill;
            if (state == MovementState.Moving)
            {
                value = pressedBgFill;
            }
            else if (state == MovementState.Breaking)
            {
                value = breakingBgFill;
            }
            fillColor = Color.Lerp(fillColor, value, 10f * Engine.DeltaTime);
            foreach (Image item in topButton)
            {
                item.Color = fillColor;
            }
            foreach (Image item2 in leftButton)
            {
                item2.Color = fillColor;
            }
            foreach (Image item3 in rightButton)
            {
                item3.Color = fillColor;
            }
        }

        private void AddImage(MTexture tex, Vector2 position, float rotation, Vector2 scale, List<Image> addTo)
        {
            Image image = new Image(tex);
            image.Position = position + new Vector2(4f, 4f);
            image.CenterOrigin();
            image.Rotation = rotation;
            image.Scale = scale;
            Add(image);
            addTo?.Add(image);
        }

        private void SetVisible(List<Image> images, bool visible)
        {
            foreach (Image image in images)
            {
                image.Visible = visible;
            }
        }

        public override void Render()
        {
            Vector2 position = Position;
            Position += Shake;
            foreach (Image item in leftButton)
            {
                item.Render();
            }
            foreach (Image item2 in rightButton)
            {
                item2.Render();
            }
            foreach (Image item3 in topButton)
            {
                item3.Render();
            }
            Draw.Rect(X + 3f, Y + 3f, Width - 6f, Height - 6f, fillColor);
            foreach (Image item4 in body)
            {
                item4.Render();
            }
            Draw.Rect(Center.X - 4f, Center.Y - 4f, 8f, 8f, fillColor);
            if (state != MovementState.Breaking)
            {
                int value = (int)Math.Floor((0f - angle + (float)Math.PI * 2f) % ((float)Math.PI * 2f) / ((float)Math.PI * 2f) * 8f + 0.5f);
                arrows[Calc.Clamp(value, 0, 7)].DrawCentered(Center);
            }
            else
            {
                GFX.Game["objects/moveBlock/x"].DrawCentered(Center);
            }
            float num = flash * 4f;
            Draw.Rect(X - num, Y - num, Width + num * 2f, Height + num * 2f, Color.White * flash);
            Position = position;
        }

        private void ActivateParticles()
        {
            bool flag = direction == Directions.Down || direction == Directions.Up;
            bool num = (!canSteer || !flag) && !CollideCheck<Player>(Position - Vector2.UnitX);
            bool flag2 = (!canSteer || !flag) && !CollideCheck<Player>(Position + Vector2.UnitX);
            bool flag3 = (!canSteer || flag) && !CollideCheck<Player>(Position - Vector2.UnitY);
            if (num)
            {
                SceneAs<Level>().ParticlesBG.Emit(MoveBlock.P_Activate, (int)(Height / 2f), CenterLeft, Vector2.UnitY * (Height - 4f) * 0.5f, (float)Math.PI);
            }
            if (flag2)
            {
                SceneAs<Level>().ParticlesBG.Emit(MoveBlock.P_Activate, (int)(Height / 2f), CenterRight, Vector2.UnitY * (Height - 4f) * 0.5f, 0f);
            }
            if (flag3)
            {
                SceneAs<Level>().ParticlesBG.Emit(MoveBlock.P_Activate, (int)(Width / 2f), TopCenter, Vector2.UnitX * (Width - 4f) * 0.5f, -(float)Math.PI / 2f);
            }
            SceneAs<Level>().ParticlesBG.Emit(MoveBlock.P_Activate, (int)(Width / 2f), BottomCenter, Vector2.UnitX * (Width - 4f) * 0.5f, (float)Math.PI / 2f);
        }

        private void BreakParticles()
        {
            Vector2 center = Center;
            for (int i = 0; i < Width; i += 4)
            {
                for (int j = 0; j < Height; j += 4)
                {
                    Vector2 vector = Position + new Vector2(2 + i, 2 + j);
                    SceneAs<Level>().Particles.Emit(MoveBlock.P_Break, 1, vector, Vector2.One * 2f, (vector - center).Angle());
                }
            }
        }

        private void MoveParticles()
        {
            Vector2 position;
            Vector2 positionRange;
            float num;
            float num2;
            if (direction == Directions.Right)
            {
                position = CenterLeft + Vector2.UnitX;
                positionRange = Vector2.UnitY * (Height - 4f);
                num = (float)Math.PI;
                num2 = Height / 32f;
            }
            else if (direction == Directions.Left)
            {
                position = CenterRight;
                positionRange = Vector2.UnitY * (Height - 4f);
                num = 0f;
                num2 = Height / 32f;
            }
            else if (direction == Directions.Down)
            {
                position = TopCenter + Vector2.UnitY;
                positionRange = Vector2.UnitX * (Width - 4f);
                num = -(float)Math.PI / 2f;
                num2 = Width / 32f;
            }
            else
            {
                position = BottomCenter;
                positionRange = Vector2.UnitX * (Width - 4f);
                num = (float)Math.PI / 2f;
                num2 = Width / 32f;
            }
            particleRemainder += num2;
            int num3 = (int)particleRemainder;
            particleRemainder -= num3;
            positionRange *= 0.5f;
            if (num3 > 0)
            {
                SceneAs<Level>().ParticlesBG.Emit(MoveBlock.P_Move, num3, position, positionRange, num);
            }
        }

        private void ScrapeParticles(Vector2 dir)
        {
            _ = Collidable;
            Collidable = false;
            if (dir.X != 0f)
            {
                float x = ((!(dir.X > 0f)) ? (Left - 1f) : Right);
                for (int i = 0; i < Height; i += 8)
                {
                    Vector2 vector = new Vector2(x, Top + 4f + i);
                    if (Scene.CollideCheck<Solid>(vector))
                    {
                        SceneAs<Level>().ParticlesFG.Emit(ZipMover.P_Scrape, vector);
                    }
                }
            }
            else
            {
                float y = ((!(dir.Y > 0f)) ? (Top - 1f) : Bottom);
                for (int j = 0; j < Width; j += 8)
                {
                    Vector2 vector2 = new Vector2(Left + 4f + j, y);
                    if (Scene.CollideCheck<Solid>(vector2))
                    {
                        SceneAs<Level>().ParticlesFG.Emit(ZipMover.P_Scrape, vector2);
                    }
                }
            }
            Collidable = true;
        }
    }
}
