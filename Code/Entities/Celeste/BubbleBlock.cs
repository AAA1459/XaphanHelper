using System;
using System.Collections;
using System.Collections.Generic;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{

    [CustomEntity("XaphanHelper/BubbleBlock")]
    public class BubbleBlock : Solid
    {
        private class RespawnDebris : Entity
        {
            private Image sprite;

            private Vector2 from;

            private Vector2 to;

            private float percent;

            private float duration;

            private float timer;

            public Vector2 currentPos;

            public bool noMove;

            public float alpha;

            private SineWave sine;

            public RespawnDebris Init(Vector2 from, Vector2 to, float duration, Image image)
            {
                Add(sine = new SineWave(0.44f, 0f).Randomize());
                if (sprite == null)
                {
                    Add(sprite = new Image(image.Texture));
                    sprite.CenterOrigin();
                }
                else
                {
                    sprite.Texture = image.Texture;
                }
                Position = (this.from = from);
                percent = 0f;
                this.to = to;
                this.duration = duration;
                if (from == to)
                {
                    noMove = true;
                }
                return this;
            }

            public override void Update()
            {
                timer += Engine.DeltaTime;
                sine.Update();
                sprite.Position = new Vector2(sine.Value * (percent < 0.5f ? 2f : (percent < 0.75f ? 1f : 0f)), sine.ValueOverTwo * (percent < 0.5f ? 2f : (percent < 0.75f ? 1f : 0f)));
                if (percent < 0.3f)
                {
                    percent += Engine.DeltaTime / duration;
                    Position = Vector2.Lerp(from, to, Ease.ElasticOut(percent * 2));
                    alpha = noMove ? 1 - percent * 33 : 1 - percent;
                    sprite.Color = Color.White * alpha;
                }
                else if (percent <= 0.5f)
                {
                    percent += Engine.DeltaTime / duration;
                    currentPos = Position;
                }
                else
                {
                    percent += Engine.DeltaTime / duration;
                    Position = Vector2.Lerp(currentPos, from, Ease.QuintOut(percent - 0.5f));
                    alpha = noMove ? (percent - 0.5f) * 2 : percent;
                    sprite.Color = Color.White * alpha;
                    if (percent >= 1f)
                    {
                        RemoveSelf();
                    }
                }
            }
        }

        private int columns;

        private int lines;

        private float reappearFlash;

        private Vector2 anchor;

        private SineWave sine;

        private Coroutine breakRoutine = new();

        public BubbleBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, safe: false)
        {
            columns = data.Width / 8;
            lines = data.Height / 8;
            anchor = Position;
            Add(sine = new SineWave(0.44f, 0f).Randomize());
            Add(new DashListener(OnDashed));
            //OnDashCollide = OnDashed;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            MTexture mTexture = GFX.Game["objects/XaphanHelper/BubbleBlock/bubbles"];
            int textureColumns = mTexture.Width / 8;
            int textureLines = mTexture.Height / 8;
            for (int i = 0; i < columns; i++)
            {
                for (int j = 0; j < lines; j++)
                {
                    int selectedTextureColumn = Calc.Random.Next(0, textureColumns - 1);
                    int selectedTextureLine = Calc.Random.Next(0, textureLines);
                    Image image = new(mTexture.GetSubtexture(selectedTextureColumn * 8, selectedTextureLine * 8, 8, 8));
                    image.X = i * 8;
                    image.Y = j * 8;
                    Add(image);
                }
            }
        }

        public override void Update()
        {
            base.Update();
            if (Collidable)
            {
                sine.Rate = 1f;
                MoveTo(new Vector2((float)(anchor.X + (double)sine.Value * 2f), (float)(anchor.Y + (double)sine.ValueOverTwo * 2f)));
            }
            else
            {
                sine.Rate = 0f;
            }
            reappearFlash = Calc.Approach(reappearFlash, 0f, Engine.DeltaTime * 8f);
        }

        /*private DashCollisionResults OnDashed(Player player, Vector2 direction)
        {
            Add(new Coroutine(Sequence()));
            return DashCollisionResults.Ignore;
        }*/

        private void OnDashed(Vector2 vector)
        {
            if (!breakRoutine.Active)
            {
                Add(breakRoutine = new Coroutine(Sequence()));
            }
        }

        private IEnumerator Sequence()
        {
            float respawnTimer = 3.5f;
            for (int i = 0; i < Width; i += 8)
            {
                for (int j = 0; j < Height; j += 8)
                {
                    Vector2 vector = new Vector2(X + i + 4f, Y + j + 4f);
                    Vector2 mult = new Vector2(Math.Abs(vector.X - Center.X) / (Width / 2), Math.Abs(vector.Y - Center.Y) / (Height / 2)) + Vector2.One * 1.25f;
                    Image debrisImage = null;
                    foreach (Component component in Components)
                    {
                        if (component.GetType() == typeof(Image))
                        {
                            Image image = (Image)component;
                            if (image.X == i && image.Y == j)
                            {
                                debrisImage = image;
                                break;
                            }
                        }
                    }
                    Scene.Add(Engine.Pooler.Create<RespawnDebris>().Init(vector, vector + (vector - Center).SafeNormalize() * 16f * mult, respawnTimer, debrisImage));
                }
            }
            Collidable = Visible = false;
            DisableStaticMovers();
            Audio.Play("event:/game/xaphan/bubble_block_break", Position);
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            while (respawnTimer >= 0)
            {
                respawnTimer -= Engine.DeltaTime;
                yield return null;
            }
            reappearFlash = 1.6f;
            Collidable = Visible = true;
            EnableStaticMovers();
            if (CollideCheck(player))
            {
                player.Die(Vector2.Zero);
            }
            else
            {
                Audio.Play("event:/game/xaphan/bubble_block_reform", Position);
            }
            yield return 0.5f;
        }

        public override void Render()
        {
            base.Render();
            if (reappearFlash > 0f)
            {
                float num = Ease.SineInOut(reappearFlash) * 0.75f;
                float num2 = num * 2f;
                foreach (Component component in Components)
                {
                    if (component.GetType() == typeof(Image))
                    {
                        Image image = (Image)component;
                        Draw.Circle(image.Position, 4f, Color.White * num, 8);
                    }
                }
                for (int i = 0; i < columns; i++)
                {
                    for (int j = 0; j < lines; j++)
                    {
                        Draw.Circle(Position + new Vector2(i * 8 + 4, j * 8 + 4), 4f, Color.White * num, 8);
                        Draw.Circle(Position + new Vector2(i * 8 + 4, j * 8 + 4), 3f, Color.White * num, 8);
                        Draw.Circle(Position + new Vector2(i * 8 + 4, j * 8 + 4), 2f, Color.White * num, 8);
                        Draw.Circle(Position + new Vector2(i * 8 + 4, j * 8 + 4), 1f, Color.White * num, 8);
                    }
                }
            }
        }
    }
}
