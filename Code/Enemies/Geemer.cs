﻿using System;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Enemies
{
    [CustomEntity("XaphanHelper/Geemer")]
    public class Geemer : Enemy
    {
        public Vector2 Speed;

        public float speedValue;

        public bool Clockwise;

        public Geemer(EntityData data, Vector2 offset) : base(data, offset)
        {
            Collider = new Hitbox(6f, 6f);
            Health = 400;
            Damage = 20;
            pc.Collider = new Hitbox(12f, 12f, -3f, -3f);
            bc.Collider = new Hitbox(16f, 16f, -5f, -5f);
            Clockwise = data.Bool("clockwise");
            speedValue = data.Float("speed", 20f);
            Add(sprite = new Sprite(GFX.Game, "enemies/Xaphan/Geemer/"));
            sprite.AddLoop("walk", "walk", 0.05f);
            sprite.Position += new Vector2(-3f, -5f);
            sprite.Play("walk");
        }

        public override void Update()
        {
            BeforeUpdate();
            base.Update();
            if (!Freezed)
            {
                bool noCollideX = false;
                bool noCollideY = false;
                if (Clockwise)
                {
                    if ((CollideCheck<Solid>(Position + new Vector2(0f, 2f)) || Bottom >= SceneAs<Level>().Bounds.Bottom) && !CollideCheck<Solid>(Position + new Vector2(-1f, 0f)))
                    {
                        if (Bottom > SceneAs<Level>().Bounds.Bottom)
                        {
                            Bottom = SceneAs<Level>().Bounds.Bottom;
                        }
                        Speed.X = -speedValue;
                        sprite.Rotation = 0;
                        sprite.Position = new Vector2(-4f, -7f);
                    }
                    else if ((CollideCheck<Solid>(Position + new Vector2(0f, -2f)) || Top <= SceneAs<Level>().Bounds.Top) && !CollideCheck<Solid>(Position + new Vector2(1f, 0f)))
                    {
                        if (Top < SceneAs<Level>().Bounds.Top)
                        {
                            Top = SceneAs<Level>().Bounds.Top;
                        }
                        Speed.X = speedValue;
                        sprite.Rotation = (float)Math.PI;
                        sprite.Position = new Vector2(12f, 13f);
                    }
                    else if (CollideCheck<Solid>(Position + new Vector2(2f, -2f)) && !CollideCheck<Solid>(Position + new Vector2(-1f, 1f)))
                    {
                        Speed.X = speedValue;
                    }
                    else if (CollideCheck<Solid>(Position + new Vector2(-2f, 2f)) && !CollideCheck<Solid>(Position + new Vector2(1f, -1f)))
                    {
                        Speed.X = -speedValue;
                    }
                    else
                    {
                        Speed.X = 0;
                        noCollideX = true;
                    }
                    if ((CollideCheck<Solid>(Position + new Vector2(2f, 0f)) || Right >= SceneAs<Level>().Bounds.Right) && !CollideCheck<Solid>(Position + new Vector2(0f, 1f)))
                    {
                        if (Right > SceneAs<Level>().Bounds.Right)
                        {
                            Right = SceneAs<Level>().Bounds.Right;
                        }
                        Speed.Y = speedValue;
                        sprite.Rotation = -(float)Math.PI / 2f;
                        sprite.Position = new Vector2(-6f, 11f);
                    }
                    else if ((CollideCheck<Solid>(Position + new Vector2(-2f, 0f)) || Left <= SceneAs<Level>().Bounds.Left) && !CollideCheck<Solid>(Position + new Vector2(0f, -1f)))
                    {
                        if (Left < SceneAs<Level>().Bounds.Left)
                        {
                            Left = SceneAs<Level>().Bounds.Left;
                        }
                        Speed.Y = -speedValue;
                        sprite.Rotation = (float)Math.PI / 2f;
                        sprite.Position = new Vector2(14f, -5f);
                    }
                    else if (CollideCheck<Solid>(Position + new Vector2(2f, 2f)) && !CollideCheck<Solid>(Position + new Vector2(-1f, -1f)))
                    {
                        Speed.Y = speedValue;
                    }
                    else if (CollideCheck<Solid>(Position + new Vector2(-2f, -2f)) && !CollideCheck<Solid>(Position + new Vector2(1f, 1f)))
                    {
                        Speed.Y = -speedValue;
                    }
                    else
                    {
                        Speed.Y = 0;
                        noCollideY = true;
                    }
                }
                else
                {
                    if ((CollideCheck<Solid>(Position + new Vector2(0f, 2f)) || Bottom >= SceneAs<Level>().Bounds.Bottom) && !CollideCheck<Solid>(Position + new Vector2(1f, 0f)))
                    {
                        if (Bottom > SceneAs<Level>().Bounds.Bottom)
                        {
                            Bottom = SceneAs<Level>().Bounds.Bottom;
                        }
                        Speed.X = speedValue;
                        sprite.Rotation = 0;
                        sprite.Position = new Vector2(-4f, -7f);
                    }
                    else if ((CollideCheck<Solid>(Position + new Vector2(0f, -2f)) || Top <= SceneAs<Level>().Bounds.Top) && !CollideCheck<Solid>(Position + new Vector2(-1f, 0f)))
                    {
                        if (Top < SceneAs<Level>().Bounds.Top)
                        {
                            Top = SceneAs<Level>().Bounds.Top;
                        }
                        Speed.X = -speedValue;
                        sprite.Rotation = (float)Math.PI;
                        sprite.Position = new Vector2(12f, 13f);
                    }
                    else if (CollideCheck<Solid>(Position + new Vector2(-2f, -2f)) && !CollideCheck<Solid>(Position + new Vector2(1f, 1f)))
                    {
                        Speed.X = -speedValue;
                    }
                    else if (CollideCheck<Solid>(Position + new Vector2(2f, 2f)) && !CollideCheck<Solid>(Position + new Vector2(-1f, -1f)))
                    {
                        Speed.X = speedValue;
                    }
                    else
                    {
                        Speed.X = 0;
                        noCollideX = true;
                    }
                    if ((CollideCheck<Solid>(Position + new Vector2(2f, 0f)) || Right >= SceneAs<Level>().Bounds.Right) && !CollideCheck<Solid>(Position + new Vector2(0f, -1f)))
                    {
                        if (Right > SceneAs<Level>().Bounds.Right)
                        {
                            Right = SceneAs<Level>().Bounds.Right;
                        }
                        Speed.Y = -speedValue;
                        sprite.Rotation = -(float)Math.PI / 2f;
                        sprite.Position = new Vector2(-6f, 11f);
                    }
                    else if ((CollideCheck<Solid>(Position + new Vector2(-2f, 0f)) || Left <= SceneAs<Level>().Bounds.Left) && !CollideCheck<Solid>(Position + new Vector2(0f, 1f)))
                    {
                        if (Left < SceneAs<Level>().Bounds.Left)
                        {
                            Left = SceneAs<Level>().Bounds.Left;
                        }
                        Speed.Y = speedValue;
                        sprite.Rotation = (float)Math.PI / 2f;
                        sprite.Position = new Vector2(14f, -5f);
                    }
                    else if (CollideCheck<Solid>(Position + new Vector2(-2f, 2f)) && !CollideCheck<Solid>(Position + new Vector2(1f, -1f)))
                    {
                        Speed.Y = speedValue;
                    }
                    else if (CollideCheck<Solid>(Position + new Vector2(2f, -2f)) && !CollideCheck<Solid>(Position + new Vector2(-1f, 1f)))
                    {
                        Speed.Y = -speedValue;
                    }
                    else
                    {
                        Speed.Y = 0;
                        noCollideY = true;
                    }
                }
                if (CollideCheck<Slope>(Position + new Vector2(2f, 3f)) || CollideCheck<Slope>(Position + new Vector2(-2f, 3f)))
                {
                    sprite.Rotation = 0;
                    sprite.Position = new Vector2(-4f, -7f);
                }
                else if (CollideCheck<Slope>(Position + new Vector2(2f, -3f)) || CollideCheck<Slope>(Position + new Vector2(-2f, -3f)))
                {
                    sprite.Rotation = (float)Math.PI;
                    sprite.Position = new Vector2(12f, 13f);
                }
                if (noCollideX && noCollideY)
                {
                    Speed.X = 0;
                    Speed.Y = 100f;
                    sprite.Rotation = 0;
                    sprite.Position = new Vector2(-4f, -7f);
                }
                MoveH(Speed.X * Engine.DeltaTime);
                MoveV(Speed.Y * Engine.DeltaTime);
            }
            AfterUpdate();
        }
    }
}
