﻿using System;
using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [CustomEntity("XaphanHelper/BouncyBubble")]
    class BouncyBubble : Entity
    {
        private Wiggler scaleWiggler;

        private Sprite Sprite;

        private SineWave sine;

        public float respawnTime;

        private CustomRefill refill;

        private ParticleType P_Break;

        public BouncyBubble(EntityData data, Vector2 position) : base(data.Position + position)
        {
            Collider = new Hitbox(16f, 16, -8f, -8f);
            respawnTime = data.Float("respawnTime", 0f);
            Add(new PlayerCollider(OnBounce, Collider));
            Add(Sprite = new Sprite(GFX.Game, "objects/XaphanHelper/BouncyBubble/"));
            Sprite.Add("idle", "idle", 0f);
            Sprite.Play("idle");
            Sprite.CenterOrigin();
            Add(scaleWiggler = Wiggler.Create(0.5f, 4f, delegate (float f)
            {
                Sprite.Scale = Vector2.One * (1f + f * 0.3f);
            }));
            P_Break = new ParticleType
            {
                Source = GFX.Game["particles/rect"],
                Color = Calc.HexToColor("47b5cc"),
                Color2 = Calc.HexToColor("c4f4ff"),
                ColorMode = ParticleType.ColorModes.Blink,
                FadeMode = ParticleType.FadeModes.Late,
                RotationMode = ParticleType.RotationModes.Random,
                Size = 0.5f,
                SizeRange = 0.2f,
                LifeMin = 0.6f,
                LifeMax = 0.9f,
                SpeedMin = 60f,
                SpeedMax = 70f,
                SpeedMultiplier = 0.1f,
                DirectionRange = (float)Math.PI * 2f,
                SpinFlippedChance = true,
                SpinMin = (float)Math.PI / 6f,
                SpinMax = 1.39626336f
            };
            Depth = -150;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (CollideCheck<CustomRefill>())
            {
                CustomRefill refill = CollideFirst<CustomRefill>();
                this.refill = refill;
                sine = refill.sine;
                respawnTime = refill.oneUse ? 0 : refill.respawnTime;
                UpdateY();
            }
            if (sine == null)
            {
                Add(sine = new SineWave(0.44f, 0f).Randomize());
                UpdateSpritePosition();
            }
        }

        public override void Update()
        {
            base.Update();
            if (refill != null)
            {
                refill.canCollect = !Collidable;
                UpdateY();
            }
            else if (sine != null)
            {
                UpdateSpritePosition();
            }
        }

        private void UpdateY()
        {
            Sprite.Y = sine.Value * 2f;
        }

        private void UpdateSpritePosition()
        {
            Sprite.Position = new Vector2((float)(double)sine.Value, (float)(double)sine.ValueOverTwo);
        }

        private void OnBounce(Player player)
        {
            if ((player.StateMachine.State == Player.StDash || player.DashAttacking) && Collidable)
            {
                Add(new Coroutine(Break(player)));
            }
            else
            {
                BouncePlayer(player, Center);
                scaleWiggler.Start();
                if (refill != null)
                {
                    refill.wiggler.Start();
                }
                Audio.Play("event:/game/06_reflection/feather_bubble_bounce", Position);
                Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
            }
        }

        public void BouncePlayer(Player player, Vector2 from)
        {
            if (player.StateMachine.State == 2)
            {
                player.StateMachine.State = 0;
            }

            if (player.StateMachine.State == 4 && player.CurrentBooster != null)
            {
                player.CurrentBooster.PlayerReleased();
            }

            Vector2 vector = (player.Center - from).SafeNormalize();

            player.Speed = vector * 220f;

            if (Math.Abs(player.Speed.X) < 100f)
            {
                if (player.Speed.X == 0f)
                {
                    player.Speed.X = (0 - player.Facing) * 100f;
                }
                else
                {
                    player.Speed.X = Math.Sign(player.Speed.X) * 100f;
                }
            }
        }

        private IEnumerator Break(Player player)
        {
            Collidable = Visible = false;
            float direction = ((!(player.Speed != Vector2.Zero)) ? (Position - player.Center).Angle() : player.Speed.Angle());
            if (refill == null)
            {
                SceneAs<Level>().ParticlesFG.Emit(P_Break, 15, Position, Vector2.One * 6f);
            }
            SlashFx.Burst(Position, direction);

            if (respawnTime > 0)
            {
                yield return respawnTime;

                Collidable = Visible = true;
                scaleWiggler.Start();
                if (refill != null)
                {
                    refill.wiggler.Start();
                }
            }
        }

    }
}