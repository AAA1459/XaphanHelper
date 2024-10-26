using System;
using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked()]
    [CustomEntity("XaphanHelper/RailBlock")]
    class RailBlock : Solid
    {
        private Vector2[] nodes;

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

        public Sprite saw;

        private Sprite node;

        private int direction;

        private bool fromFirstLoad;

        private int id;

        private float speedMult;

        private bool drawTrack;

        private bool particles;

        private ParticleType P_Trail;

        private bool rewind;

        private Coroutine WaitingRoutine = new();

        private bool canDash;

        private bool playerMomentum;

        private bool noReturn;

        public RailBlock(int id, Vector2[] nodes, string directory, string lineColorA, string lineColorB, string particlesColorA, string particlesColorB, int index, float speedMult, bool drawTrack, bool particles, bool canDash, bool playerMomentum, bool noReturn, bool fromFirstLoad = false) : base(nodes[0], 8, 8, false)
        {
            Tag = Tags.TransitionUpdate;
            Collider = new Hitbox(32f, 14f, -16f, -7f);
            this.id = id;
            this.nodes = nodes;
            this.directory = directory;
            this.lineColorA = lineColorA;
            this.lineColorB = lineColorB;
            this.particlesColorA = particlesColorA;
            this.particlesColorB = particlesColorB;
            this.index = index;
            this.speedMult = speedMult;
            this.drawTrack = drawTrack;
            this.particles = particles;
            if (canDash)
            {
                this.canDash = true;
                OnDashCollide += onDashCollide;
            }
            this.playerMomentum = playerMomentum;
            this.noReturn = noReturn;
            this.fromFirstLoad = fromFirstLoad;
            if (string.IsNullOrEmpty(this.directory))
            {
                this.directory = "objects/XaphanHelper/RailBlock";
            }
            lengths = new float[nodes.Length];
            for (int i = 1; i < lengths.Length; i++)
            {
                lengths[i] = lengths[i - 1] + Vector2.Distance(nodes[i - 1], nodes[i]);
            }
            speed = 0;
            percent = 0f;
            Position = GetPercentPosition(percent);
            Add(saw = new Sprite(GFX.Game, this.directory + "/"));
            saw.AddLoop("idle", "idle", 0.1f, 0, 1, 2, 3, 2, 1);
            saw.CenterOrigin();
            saw.Play("idle");
            Add(node = new Sprite(GFX.Game, this.directory + "/"));
            node.AddLoop("node", "node", 0.15f);
            node.CenterOrigin();
            node.Play("node");
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
            if (index == 0)
            {
                Depth = 9999;
            }
            Add(new Coroutine(DecaySpeedRoutine()));
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            if (index != 0)
            {
                foreach (Entity entity in SceneAs<Level>().Entities)
                {
                    if (CollideCheck(entity, Position + Vector2.UnitX * 2) || CollideCheck(entity, Position - Vector2.UnitX * 2) || CollideCheck(entity, Position - Vector2.UnitY * 2) || CollideCheck(entity, Position + Vector2.UnitY * 2))
                    {
                        foreach (Component component in entity.Components)
                        {
                            if (component.GetType() == typeof(StaticMover))
                            {
                                StaticMover staticMover = (StaticMover)component;
                                staticMover.Entity.Position.Y -= staticMover.Entity.Center.Y > Center.Y ? (staticMover.Entity.GetType() == typeof(MagneticCeiling) ? 2f : 1f) : -1f;
                                staticMover.Entity.Depth = Depth + 1;
                                staticMovers.Add(staticMover);
                            }
                        }
                    }
                }
            }
        }

        private IEnumerator DecaySpeedRoutine()
        {
            while (true)
            {
                if (playerMomentum && HasPlayerRider() && !rewind && percent < 1)
                {
                    if (speed < 150f * speedMult / lengths[lengths.Length - 1])
                    {
                        speed += Engine.DeltaTime / lengths[lengths.Length - 1] * 300 * speedMult;
                    }
                    else
                    {
                        speed = 150f * speedMult / lengths[lengths.Length - 1];
                    }
                    direction = 1;
                }
                else if (speed > 0 && !rewind)
                {
                    speed -= Engine.DeltaTime / lengths[lengths.Length - 1] * 300 * speedMult;
                }
                else if (!noReturn)
                {
                    if (percent == 0)
                    {
                        rewind = false;
                        yield return null;
                    }
                    else
                    {
                        if (speed <= 0 && !rewind && !WaitingRoutine.Active)
                        {
                            speed = 0;
                            Add(WaitingRoutine = new Coroutine(Waiting()));
                        }
                        else if (rewind)
                        {
                            if (speed < 75f * speedMult / lengths[lengths.Length - 1])
                            {
                                speed += Engine.DeltaTime / lengths[lengths.Length - 1] * 300 * speedMult;
                            }
                            else
                            {
                                speed = 75f * speedMult / lengths[lengths.Length - 1];
                            }
                        }
                    }
                }
                else if (noReturn)
                {
                    if (speed <= 0)
                    {
                        speed = 0;
                    }
                }
                yield return null;
            }
        }

        private IEnumerator Waiting()
        {
            Vector2 WaitPos = Position;
            yield return 1.5f;
            if (Position == WaitPos)
            {
                rewind = true;
            }
        }

        private DashCollisionResults onDashCollide(Player player, Vector2 direction)
        {
            if (!rewind)
            {
                float position = lengths[lengths.Length - 1] * percent;
                int previousNode = 0;
                int nextNode = 0;
                foreach (float length in lengths)
                {
                    if (position >= length)
                    {
                        bool isLastNode = Array.IndexOf(lengths, length) == lengths.GetUpperBound(0);
                        previousNode = !isLastNode ? Array.IndexOf(lengths, length) : Array.IndexOf(lengths, length) - 1;
                        nextNode = previousNode + 1;
                    }
                }

                if (direction.X == -1)
                {
                    if (nodes[previousNode].X < GetPercentPosition(percent).X)
                    {
                        this.direction = -1;
                    }
                    else if (nodes[previousNode].X > GetPercentPosition(percent).X)
                    {
                        this.direction = 1;
                    }
                    else if (nodes[nextNode].X < GetPercentPosition(percent).X)
                    {
                        this.direction = 1;
                    }
                    else if (nodes[nextNode].X > GetPercentPosition(percent).X)
                    {
                        this.direction = -1;
                    }
                    else
                    {
                        this.direction = 0;
                    }
                }
                else if (direction.X == 1)
                {
                    if (nodes[nextNode].X > GetPercentPosition(percent).X)
                    {
                        this.direction = 1;
                    }
                    else if ((nodes[nextNode].X < GetPercentPosition(percent).X))
                    {
                        this.direction = -1;
                    }
                    else if (nodes[previousNode].X > GetPercentPosition(percent).X)
                    {
                        this.direction = -1;
                    }
                    else if ((nodes[previousNode].X < GetPercentPosition(percent).X))
                    {
                        this.direction = 1;
                    }
                    else
                    {
                        this.direction = 0;
                    }
                }
                else if (direction.Y == -1)
                {
                    
                    if (nodes[previousNode].Y < GetPercentPosition(percent).Y)
                    {
                        this.direction = 1;
                    }
                    else if (nodes[previousNode].Y > GetPercentPosition(percent).Y)
                    {
                        this.direction = -1;
                    }
                    else if (nodes[nextNode].Y < GetPercentPosition(percent).Y)
                    {
                        this.direction = 1;
                    }
                    else if (nodes[nextNode].Y > GetPercentPosition(percent).Y)
                    {
                        this.direction = -1;
                    }
                    else
                    {
                        this.direction = 0;
                    }
                }
                else if (direction.Y == 1)
                {
                    if (nodes[nextNode].Y > GetPercentPosition(percent).Y)
                    {
                        this.direction = 1;
                    }
                    else if ((nodes[nextNode].Y < GetPercentPosition(percent).Y))
                    {
                        this.direction = -1;
                    }
                    else if (nodes[previousNode].Y > GetPercentPosition(percent).Y)
                    {
                        this.direction = 1;
                    }
                    else if ((nodes[previousNode].Y < GetPercentPosition(percent).Y))
                    {
                        this.direction = -1;
                    }
                    else
                    {
                        this.direction = 0;
                    }
                }

                speed = 240f * speedMult / lengths[lengths.Length - 1];
            }
            
            if (Input.GrabCheck)
            {
                player.StateMachine.State = Player.StClimb;
            }
            else
            {
                player.StateMachine.State = Player.StNormal;
            }

            return DashCollisionResults.NormalCollision;
        }

        public RailBlock(EntityData data, Vector2 offset, EntityID eid) : this(
            eid.ID,
            data.NodesWithPosition(offset),
            data.Attr("directory", "objects/XaphanHelper/RailBlock"),
            data.Attr("lineColorA", "2A251F"),
            data.Attr("lineColorB", "C97F35"),
            data.Attr("particlesColorA", "696A6A"),
            data.Attr("particlesColorB", "700808"),
            0,
            data.Float("speedMult", 1f),
            data.Bool("drawTrack", true),
            data.Bool("particles", true), 
            data.Bool("canDash", true),
            data.Bool("playerMomentum", false),
            data.Bool("noReturn", false),
            fromFirstLoad: true)
        {
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (fromFirstLoad)
            {
                Scene.Add(new RailBlock(id, nodes, directory, lineColorA, lineColorB, particlesColorA, particlesColorB, 1, speedMult, drawTrack, particles, canDash, playerMomentum, noReturn));
            }
            if (trackSfx != null)
            {
                PositionTrackSfx();
                //trackSfx.Play("event:/env/local/09_core/fireballs_idle");
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
            if (index != 0)
            {
                if (rewind)
                {
                    direction = -1;
                }
                if (speed == 0)
                {
                    return;
                }
                if (percent < 0)
                {
                    percent = 0;
                }
                else if (percent > 1)
                {
                    percent = 1;
                }
                if (direction == -1 && percent > 0)
                {
                    percent -= speed * Engine.DeltaTime;
                }
                else if (direction == 1 && percent <= 1)
                {
                    percent += speed * Engine.DeltaTime;
                }
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

        public override void Render()
        {
            if (index == 0)
            {
                if (drawTrack)
                {
                    for (int i = 0; i < nodes.Length; i++)
                    {
                        if (i + 1 < nodes.Length)
                        {
                            Draw.Line(nodes[i], nodes[i + 1], Calc.HexToColor(lineColorA), 4);
                            Draw.Line(nodes[i], nodes[i + 1], Calc.HexToColor(lineColorB) * (0.7f * (0.7f + ((float)Math.Sin(alpha) + 1f) * 0.125f)), 2);
                        }
                        if (i < nodes.Length)
                        {
                            node.RenderPosition = nodes[i];
                            node.Render();
                        }
                    }
                }
            }
            else
            {
                saw.Render();
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
            float num = lengths[lengths.Length - 1];
            float num2 = num * percent;
            int i;
            for (i = 0; i < lengths.Length - 1 && !(lengths[i + 1] > num2); i++)
            {
            }
            if (i == lengths.Length - 1)
            {
                return nodes[lengths.Length - 1];
            }
            float min = lengths[i] / num;
            float max = lengths[i + 1] / num;
            float num3 = Calc.ClampedMap(percent, min, max);
            return Vector2.Lerp(nodes[i], nodes[i + 1], num3);
        }
    }
}
