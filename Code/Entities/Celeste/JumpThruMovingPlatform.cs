﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked()]
    class JumpThruMovingPlatform : JumpThru
    {
        private static FieldInfo SpikesSpikeType = typeof(Spikes).GetField("overrideType", BindingFlags.Instance | BindingFlags.NonPublic);

        private Vector2[] nodes;

        private int amount;

        private int index;

        private float startOffset;

        private float spacingOffset;

        private float[] lengths;

        private float speed;

        private float percent;

        private string directory;

        private string lineColorA;

        private string lineColorB;

        private string particlesColorA;

        private string particlesColorB;

        public float alpha = 0f;

        private SoundSource trackSfx;

        private List<Sprite> sprites = new();

        private int direction;

        private bool swapped;

        private bool fromFirstLoad;

        private string mode;

        private int id;

        private float speedMult;

        private int length;

        private string stopFlag;

        private string swapFlag;

        private string moveFlag;

        private string forceInactiveFlag;

        private bool drawTrack;

        private bool particles;

        private bool AtStartOfTrack;

        private bool AtEndOfTrack;

        private bool Moving = true;

        private ParticleType P_Trail;

        public float noCollideDelay;

        private string Orientation;

        private bool AttachedEntity;

        public Spikes AttachedSpike;

        public Vector2 attachedEntityOffset;

        private string AttachedEntityPlatformsIndexes;

        public JumpThruMovingPlatform(int id, Vector2 position, Vector2[] nodes, string mode, string directory, int length, string lineColorA, string lineColorB, string particlesColorA, string particlesColorB, string orientation, int amount, int index, float speedMult, float startOffset, float spacingOffset, string attachedEntityPlatformsIndexes, string stopFlag, string swapFlag, string moveFlag, string forceInactiveFlag, bool drawTrack, bool particles, int direction, float startPercent = -1f, bool swapped = false) : base(position, 8, false)
        {
            Tag = Tags.TransitionUpdate;
            noCollideDelay = 0.01f;
            Add(new Coroutine(CollideDelayRoutine()));
            Add(new LedgeBlocker());
            Collider = new Hitbox(length * 8, 8, -length * 8 / 2, -4);
            this.id = id;
            this.nodes = nodes;
            this.mode = mode;
            this.directory = directory;
            this.length = length;
            this.lineColorA = lineColorA;
            this.lineColorB = lineColorB;
            this.particlesColorA = particlesColorA;
            this.particlesColorB = particlesColorB;
            Orientation = orientation;
            this.amount = amount;
            this.index = index;
            this.speedMult = speedMult;
            this.startOffset = startOffset;
            this.spacingOffset = spacingOffset;
            AttachedEntityPlatformsIndexes = attachedEntityPlatformsIndexes;
            this.stopFlag = stopFlag;
            this.swapFlag = swapFlag;
            this.moveFlag = moveFlag;
            this.forceInactiveFlag = forceInactiveFlag;
            this.drawTrack = drawTrack;
            this.particles = particles;
            this.direction = direction;
            this.swapped = swapped;
            this.fromFirstLoad = fromFirstLoad;
            if (string.IsNullOrEmpty(this.directory))
            {
                this.directory = "objects/XaphanHelper/JumpThruMovingPlatform";
            }
            lengths = new float[nodes.Length];
            for (int i = 1; i < lengths.Length; i++)
            {
                lengths[i] = lengths[i - 1] + Vector2.Distance(nodes[i - 1], nodes[i]);
            }
            speed = speedMult / lengths[lengths.Length - 1];
            if (startPercent == -1f && index != 0)
            {
                if (((float)index - 1) * spacingOffset < 1f)
                {
                    percent = ((float)index - 1) * spacingOffset;
                }
                else
                {
                    RemoveSelf();
                }
                percent += startOffset;
                if (percent > 1)
                {
                    RemoveSelf();
                }
            }
            else
            {
                percent = startPercent;
            }
            percent %= 1f;
            Position = GetPercentPosition(percent);
            sprites = BuildSprite();
            if (index == 0)
            {
                Add(trackSfx = new SoundSource());
                Collidable = false;
            }
            P_Trail = new ParticleType
            {
                Color = Calc.HexToColor(particlesColorA),
                Color2 = Calc.HexToColor(particlesColorB),
                ColorMode = ParticleType.ColorModes.Choose,
                FadeMode = ParticleType.FadeModes.Late,
                LifeMin = 0.3f,
                LifeMax = 0.6f,
                Size = 1f,
                DirectionRange = (float)Math.PI * 2f,
                SpeedMin = 4f,
                SpeedMax = 8f,
                SpeedMultiplier = 0.8f
            };
            Depth = 9999;
        }

        private List<Sprite> BuildSprite()
        {
            List<Sprite> list = new();
            for (int i = 0; i < length; i++)
            {
                Sprite sprite = new(GFX.Game, directory + "/");
                sprite.AddLoop("idle", "platform", 0f);
                sprite.CenterOrigin();
                sprite.Play("idle");
                sprite.FlipY = Orientation == "Bottom";
                sprite.Position.X = -length * 8 / 2 + i * 8 + 4;
                list.Add(sprite);
                Add(sprite);
            }
            return list;
        }

        private IEnumerator CollideDelayRoutine()
        {
            while (noCollideDelay > 0)
            {
                noCollideDelay -= Engine.DeltaTime;
                yield return null;
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (trackSfx != null)
            {
                PositionTrackSfx();
                //trackSfx.Play("event:/env/local/09_core/fireballs_idle");
            }

            if (index == 1)
            {
                AttachedSpike = CollideFirst<Spikes>(Position - Vector2.UnitY * 2);
                if (AttachedSpike != null)
                {
                    attachedEntityOffset = Position - AttachedSpike.Position;
                    foreach (Spikes spike in SceneAs<Level>().Tracker.GetEntities<Spikes>())
                    {
                        if (spike == AttachedSpike)
                        {
                            spike.RemoveSelf();
                        }
                    }
                }
            }
        }

        public override void Update()
        {
            alpha += Engine.DeltaTime * 4f;
            if ((Scene as Level).Transitioning)
            {
                PositionTrackSfx();
                return;
            }
            base.Update();

            if (index >= 1 && !AttachedEntity)
            {
                foreach (JumpThruMovingPlatform platform in SceneAs<Level>().Tracker.GetEntities<JumpThruMovingPlatform>())
                {
                    Logger.Log(LogLevel.Info, "XH", "Check platform id " + platform.id + " with index " + platform.index);
                    if (platform.id == id && platform.index == 1 && platform.AttachedSpike != null)
                    {
                        AttachedSpike = new Spikes(platform.Position - platform.attachedEntityOffset, length * 8, platform.AttachedSpike.Direction, (string)SpikesSpikeType.GetValue(platform.AttachedSpike));
                        attachedEntityOffset = platform.attachedEntityOffset;
                    }
                    if (platform.id == id && (!string.IsNullOrEmpty(AttachedEntityPlatformsIndexes) ? AttachedEntityPlatformsIndexes.Split(',').ToList().Contains(index.ToString()) : true) && AttachedSpike != null)
                    {
                        Logger.Log(LogLevel.Info, "XH", "Attached spike to platform index " + platform.index);
                        SceneAs<Level>().Add(AttachedSpike);
                    }
                }
                AttachedEntity = true;
            }

            if (mode == "Flag To Move" && !string.IsNullOrEmpty(moveFlag))
            {
                if (!SceneAs<Level>().Session.GetFlag(moveFlag))
                {
                    direction = -1;
                    if (AtEndOfTrack)
                    {
                        AtEndOfTrack = false;
                    }
                }
                else
                {
                    direction = 1;
                    if (AtStartOfTrack)
                    {
                        AtStartOfTrack = false;
                    }
                }
            }
            if ((!string.IsNullOrEmpty(forceInactiveFlag) && SceneAs<Level>().Session.GetFlag(forceInactiveFlag)) || (!string.IsNullOrEmpty(stopFlag) && SceneAs<Level>().Session.GetFlag(stopFlag)) || AtStartOfTrack || AtEndOfTrack || !Moving)
            {
                return;
            }
            if (index != 0)
            {
                if (mode == "Flag To Move")
                {
                    if (string.IsNullOrEmpty(moveFlag))
                    {
                        return;
                    }
                    if (direction == -1)
                    {
                        percent -= speed * Engine.DeltaTime;
                    }
                    else
                    {
                        percent += speed * Engine.DeltaTime;
                    }
                    if (percent <= 0)
                    {
                        foreach (JumpThruMovingPlatform platform in SceneAs<Level>().Tracker.GetEntities<JumpThruMovingPlatform>())
                        {
                            if (platform.id == id && platform.index != 0)
                            {
                                platform.AtStartOfTrack = true;
                                platform.percent = (platform.index - 1) * platform.spacingOffset;
                            }
                        }
                    }
                    if (percent >= 1)
                    {
                        foreach (JumpThruMovingPlatform platform in SceneAs<Level>().Tracker.GetEntities<JumpThruMovingPlatform>())
                        {
                            if (platform.id == id && platform.index != 0)
                            {
                                platform.AtEndOfTrack = true;
                                platform.percent = 1 - (platform.amount - platform.index) * platform.spacingOffset;
                            }
                        }
                    }
                }
                else
                {
                    if (direction == -1)
                    {
                        percent -= speed * Engine.DeltaTime;
                        if (percent <= 0)
                        {
                            if (mode == "Restart")
                            {
                                percent = percent + 1f;
                            }
                            else if (mode.Contains("Back And Forth"))
                            {
                                if (mode.Contains("All Sawblades"))
                                {
                                    foreach (JumpThruMovingPlatform platform in SceneAs<Level>().Tracker.GetEntities<JumpThruMovingPlatform>())
                                    {
                                        if (platform.id == id && platform.index != 0)
                                        {
                                            platform.direction = 1;
                                            if (platform != this)
                                            {
                                                platform.percent -= platform.speed * Engine.DeltaTime * 2;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    percent = Math.Abs(percent);
                                    direction = 1;
                                }
                            }
                        }
                    }
                    else
                    {
                        percent += speed * Engine.DeltaTime;
                        if (percent >= 1f)
                        {
                            if (mode == "Restart")
                            {
                                percent = percent - 1f;
                            }
                            else if (mode.Contains("Back And Forth"))
                            {
                                if (mode.Contains("All Sawblades"))
                                {
                                    foreach (JumpThruMovingPlatform platform in SceneAs<Level>().Tracker.GetEntities<JumpThruMovingPlatform>())
                                    {
                                        if (platform.id == id && platform.index != 0)
                                        {
                                            platform.direction = -1;
                                            platform.percent -= platform.speed * Engine.DeltaTime;
                                        }
                                    }
                                }
                                else
                                {
                                    percent = 1 - (percent - 1f);
                                    direction = -1;
                                }
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(swapFlag))
                    {
                        if (SceneAs<Level>().Session.GetFlag(swapFlag) && !swapped)
                        {
                            Scene.Add(new JumpThruMovingPlatform(id, Position, nodes, mode, directory, length, lineColorA, lineColorB, particlesColorA, particlesColorB, Orientation, amount, index, speedMult, startOffset, spacingOffset, AttachedEntityPlatformsIndexes, stopFlag, swapFlag, moveFlag, forceInactiveFlag, drawTrack, particles, direction == 1 ? -1 : 1, percent, true));
                            RemoveSelf();
                        }
                        else if (!SceneAs<Level>().Session.GetFlag(swapFlag) && swapped)
                        {
                            Scene.Add(new JumpThruMovingPlatform(id, Position, nodes, mode, directory, length, lineColorA, lineColorB, particlesColorA, particlesColorB, Orientation, amount, index, speedMult, startOffset, spacingOffset, AttachedEntityPlatformsIndexes, stopFlag, swapFlag, moveFlag, forceInactiveFlag, drawTrack, particles, direction == 1 ? -1 : 1, percent, false));
                            RemoveSelf();
                        }
                    }
                }
            }
            if (index >= 1 && AttachedSpike != null)
            {
                AttachedSpike.Position = GetPercentPosition(percent) - attachedEntityOffset;
            }
            MoveTo(GetPercentPosition(percent));
            PositionTrackSfx();
            if (Scene.OnInterval(0.05f) && index != 0 && particles)
            {
                SceneAs<Level>().ParticlesBG.Emit(P_Trail, 2, Center, Vector2.One * 3f);
            }
        }

        public void PositionTrackSfx()
        {
            if (trackSfx == null)
            {
                return;
            }
            Player entity = Scene.Tracker.GetEntity<Player>();
            if (entity == null)
            {
                return;
            }
            Vector2? vector = null;
            for (int i = 1; i < nodes.Length; i++)
            {
                Vector2 vector2 = Calc.ClosestPointOnLine(nodes[i - 1], nodes[i], entity.Center);
                if (!vector.HasValue || (vector2 - entity.Center).Length() < (vector.Value - entity.Center).Length())
                {
                    vector = vector2;
                }
            }
            if (vector.HasValue)
            {
                trackSfx.Position = vector.Value - Position;
                trackSfx.UpdateSfxPosition();
            }
        }

        public override void DebugRender(Camera camera)
        {
            if (index != 0)
            {
                base.DebugRender(camera);
            }
        }

        private Vector2 GetPercentPosition(float percent)
        {
            if (mode != "Flag To Move")
            {
                if (direction == -1)
                {
                    if (percent <= 0f)
                    {
                        return nodes[nodes.Length - 1];
                    }
                    if (percent >= 1f)
                    {
                        return nodes[0];
                    }
                }
                else
                {
                    if (percent <= 0f)
                    {
                        return nodes[0];
                    }
                    if (percent >= 1f)
                    {
                        return nodes[nodes.Length - 1];
                    }
                }
            }
            float num = lengths[lengths.Length - 1];
            float num2 = num * percent;
            int i;
            for (i = 0; i < lengths.Length - 1 && !(lengths[i + 1] > num2); i++)
            {
            }
            if (i == lengths.Length - 1)
            {
                if (mode != "Flag To Move")
                {
                    return nodes[0];
                }
                else
                {
                    return nodes[lengths.Length - 1];
                }
            }
            float min = lengths[i] / num;
            float max = lengths[i + 1] / num;
            float num3 = Calc.ClampedMap(percent, min, max);
            return Vector2.Lerp(nodes[i], nodes[i + 1], num3);
        }

        public override void Removed(Scene scene)
        {
            base.Removed(scene);
            if (AttachedSpike != null)
            {
                AttachedSpike.RemoveSelf();
            }
        }
    }
}
