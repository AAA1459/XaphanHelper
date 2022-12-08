﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/CustomCrumbleBlock")]
    public class CustomCrumbleBlock : Solid
    {
        private List<Image> images;

        private List<Image> outline;

        private List<Coroutine> falls;

        private List<int> fallOrder;

        private ShakerList shaker;

        private LightOcclude occluder;

        private Coroutine outlineFader;

        private float respawnTime;

        private float respawnTimer;

        private float outlineColorStrength;

        private float outlineColorTimer;

        private float crumbleDelay;

        private bool oneUse;

        private bool triggerAdjacents;

        private string texture;

        private int rotation;

        private HashSet<CustomCrumbleBlock> groupedCustomCrumbleBlocks = new();

        public CustomCrumbleBlock(EntityData data, Vector2 offset) : base(data.Position + offset, data.Width, data.Height, safe: false)
        {
            EnableAssistModeChecks = false;
            respawnTime = data.Float("respawnTime", 2f);
            crumbleDelay = data.Float("crumbleDelay", 0.4f);
            oneUse = data.Bool("oneUse", false);
            triggerAdjacents = data.Bool("triggerAdjacents", false);
            texture = data.Attr("texture");
            if (string.IsNullOrEmpty(texture))
            {
                texture = "objects/crumbleBlock/default";
            }
            rotation = data.Int("rotation");
        }

        private void addRange(HashSet<CustomCrumbleBlock> set, IEnumerable<CustomCrumbleBlock> elements)
        {
            foreach (CustomCrumbleBlock element in elements)
            {
                set.Add(element);
            }
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            groupedCustomCrumbleBlocks.Add(this);
            if (triggerAdjacents)
            {
                addRange(groupedCustomCrumbleBlocks, CollideAll<CustomCrumbleBlock>(Position + Vector2.UnitX).OfType<CustomCrumbleBlock>().Where(p => p.triggerAdjacents));
                addRange(groupedCustomCrumbleBlocks, CollideAll<CustomCrumbleBlock>(Position - Vector2.UnitX).OfType<CustomCrumbleBlock>().Where(p => p.triggerAdjacents));
                addRange(groupedCustomCrumbleBlocks, CollideAll<CustomCrumbleBlock>(Position + Vector2.UnitY).OfType<CustomCrumbleBlock>().Where(p => p.triggerAdjacents));
                addRange(groupedCustomCrumbleBlocks, CollideAll<CustomCrumbleBlock>(Position - Vector2.UnitY).OfType<CustomCrumbleBlock>().Where(p => p.triggerAdjacents));
            }
            foreach (CustomCrumbleBlock block in new HashSet<CustomCrumbleBlock>(groupedCustomCrumbleBlocks))
            {
                addRange(groupedCustomCrumbleBlocks, block.groupedCustomCrumbleBlocks);
                block.groupedCustomCrumbleBlocks = groupedCustomCrumbleBlocks;
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            outline = new List<Image>();
            for (int i = (int)Width / 2; i < Width; i+= 4)
            {
                Image image = new Image(GFX.Game["objects/XaphanHelper/CustomCrumbleBlock/outlineTop"])
                {
                    Position = new Vector2(i, 0f),
                    Color = Color.White * 0f
                };
                Add(image);
                outline.Add(image);
            }
            for (int j = 0; j < Height; j += 4)
            {
                Image image = new Image(GFX.Game["objects/XaphanHelper/CustomCrumbleBlock/outlineRight"])
                {
                    Position = new Vector2(Width - 4f, j),
                    Color = Color.White * 0f
                };
                Add(image);
                outline.Add(image);
            }
            for (int i = (int)Width - 4; i >= 0; i-= 4)
            {
                Image image = new Image(GFX.Game["objects/XaphanHelper/CustomCrumbleBlock/outlineBottom"])
                {
                    Position = new Vector2(i, Height - 4f),
                    Color = Color.White * 0f
                };
                Add(image);
                outline.Add(image);
            }
            for (int j = (int)Height - 4; j >= 0; j -= 4)
            {
                Image image = new Image(GFX.Game["objects/XaphanHelper/CustomCrumbleBlock/outlineLeft"])
                {
                    Position = new Vector2(0, j),
                    Color = Color.White * 0f
                };
                Add(image);
                outline.Add(image);
            }
            for (int i = 0; i < Width / 2; i += 4)
            {
                Image image = new Image(GFX.Game["objects/XaphanHelper/CustomCrumbleBlock/outlineTop"])
                {
                    Position = new Vector2(i, 0f),
                    Color = Color.White * 0f,
                };
                Add(image);
                outline.Add(image);
            }
            outline.Reverse();
            outlineColorTimer = respawnTime / outline.Count;
            if (!oneUse)
            {
                Add(outlineFader = new Coroutine());
                outlineFader.RemoveOnComplete = false;
            }
            images = new List<Image>();
            falls = new List<Coroutine>();
            fallOrder = new List<int>();
            MTexture mTexture = GFX.Game[texture];
            for (int i = 0; i < Width; i += 8)
            {
                for (int j = 0; j < Height; j += 8)
                {
                    Image image = new Image(mTexture.GetSubtexture(Calc.Random.Next(mTexture.Width / 8) * 8, 0, 8, 8));
                    image.Position = new Vector2(4 + i, 4f + j);
                    image.CenterOrigin();
                    if (rotation != 0)
                    {
                        image.Rotation = rotation == 1 ? (float)Math.PI / 2 : (rotation == 2 ? (float)Math.PI : -(float)Math.PI / 2);
                    }              
                    Add(image);
                    images.Add(image);
                    Coroutine coroutine = new Coroutine();
                    coroutine.RemoveOnComplete = false;
                    falls.Add(coroutine);
                    Add(coroutine);
                    fallOrder.Add(i * j / 8);
                }
            }
            fallOrder.Shuffle();
            Add(new Coroutine(Sequence()));
            Add(shaker = new ShakerList(images.Count, on: false, delegate (Vector2[] v)
            {
                for (int k = 0; k < images.Count; k++)
                {
                    int imgPerLine = (int)Width / 8;
                    int PosY = Math.DivRem(k, imgPerLine, out int PosX);
                    images[k].Position = new Vector2(4 + PosX * 8, 4 + PosY * 8) + v[k];
                }
            }));
            Add(occluder = new LightOcclude(0.2f));
        }

        private IEnumerator Sequence()
        {
            while (true)
            {
                CustomCrumbleBlock triggered = getOneBlockWithPlayerOnTop();
                bool onTop;
                if (triggered != null)
                {
                    onTop = true;
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                }
                else
                {
                    triggered = getOneBlockWithPlayerClimbing();
                    if (triggered == null)
                    {
                        yield return null;
                        continue;
                    }
                    onTop = false;
                    Input.Rumble(RumbleStrength.Medium, RumbleLength.Medium);
                }
                if (triggered == this)
                {
                    Audio.Play("event:/game/general/platform_disintegrate", Center);
                }
                shaker.ShakeFor(onTop ? crumbleDelay + 0.2f : crumbleDelay + 0.6f, removeOnFinish: false);
                StartShaking(onTop ? crumbleDelay + 0.2f : crumbleDelay + 0.6f);
                foreach (Image image in images)
                {
                    SceneAs<Level>().Particles.Emit(CrumblePlatform.P_Crumble, 2, Position + image.Position + new Vector2(0f, 2f), Vector2.One * 3f);
                }
                for (int i = 0; i < (onTop ? 1 : 3); i++)
                {
                    yield return 0.2f;
                    foreach (Image image in images)
                    {
                        SceneAs<Level>().Particles.Emit(CrumblePlatform.P_Crumble, 2, Position + image.Position + new Vector2(0f, 2f), Vector2.One * 3f);
                    }
                }
                float timer = crumbleDelay;
                if (onTop)
                {
                    while (timer > 0f && getOneBlockWithPlayerOnTop() != null)
                    {
                        yield return null;
                        timer -= Engine.DeltaTime;
                    }
                }
                else
                {
                    while (timer > 0f)
                    {
                        yield return null;
                        timer -= Engine.DeltaTime;
                    }
                }
                if (!oneUse)
                {
                    outlineFader.Replace(OutlineFade(1f));
                }
                occluder.Visible = false;
                Collidable = false;
                DisableStaticMovers();
                for (int j = 0; j < Width / 8; j++)
                {
                    for (int k = 0; k < images.Count; k++)
                    {
                        if (k % 2 - j == 0)
                        {
                            falls[k].Replace(TileOut(images[k], 0.05f * j));
                        }
                    }
                }
                if (!oneUse)
                {
                    Depth = 1000;
                    respawnTimer = respawnTime;
                    while (respawnTimer > 0)
                    {
                        respawnTimer -= Engine.DeltaTime;
                        yield return null;
                    }
                    while (CollideCheck<Actor>() || CollideCheck<Solid>() || isGroupCollidingWithSomething())
                    {
                        yield return null;
                    }
                }
                else
                {
                    yield return 1f;
                    RemoveSelf();
                    yield break;
                }
                outlineFader.Replace(OutlineFade(0f));
                occluder.Visible = true;
                Collidable = true;
                EnableStaticMovers();
                for (int j = 0; j < Width / 8; j++)
                {
                    for (int k = 0; k < images.Count; k++)
                    {
                        if (k % 2 - j == 0)
                        {
                            falls[k].Replace(TileIn(k, images[k], 0.05f * j));
                        }
                    }
                }
                Depth = 0;
            }
        }

        private IEnumerator OutlineFade(float to)
        {
            float from = 1f - to;
            for (float t = 0f; t < 1f; t += Engine.DeltaTime * 2f)
            {
                outlineColorStrength = from + (to - from) * Ease.CubeInOut(t);
                yield return null;
            }
        }

        private IEnumerator TileOut(Image img, float delay)
        {
            img.Color = Color.Gray;
            yield return delay;
            float distance = (img.X * 7f % 3f + 1f) * 12f;
            Vector2 from = img.Position;
            for (float time = 0f; time < 1f; time += Engine.DeltaTime / 0.4f)
            {
                yield return null;
                img.Position = from + Vector2.UnitY * Ease.CubeIn(time) * distance;
                img.Color = Color.Gray * (1f - time);
                img.Scale = Vector2.One * (1f - time * 0.5f);
            }
            img.Visible = false;
        }

        private IEnumerator TileIn(int index, Image img, float delay)
        {
            yield return delay;
            Audio.Play("event:/game/general/platform_return", Center);
            img.Visible = true;
            img.Color = Color.White;
            int imgPerLine = (int)Width / 8;
            int PosY = Math.DivRem(index, imgPerLine, out int PosX);
            img.Position = new Vector2(PosX * 8 + 4, PosY * 8 + 4f);
            for (float time = 0f; time < 1f; time += Engine.DeltaTime / 0.25f)
            {
                yield return null;
                img.Scale = Vector2.One * (1f + Ease.BounceOut(1f - time) * 0.2f);
            }
            img.Scale = Vector2.One;
        }

        private CustomCrumbleBlock getOneBlockWithPlayerOnTop()
        {
            Player player = SceneAs<Level>().Tracker.GetEntity<Player>();
            if (player != null)
            {
                foreach (CustomCrumbleBlock Block in groupedCustomCrumbleBlocks)
                {
                    if (Block.GetPlayerOnTop() != null && player.Speed.Y >= 0)
                    {
                        return Block;
                    }
                }
            }
            return null;
        }

        private CustomCrumbleBlock getOneBlockWithPlayerClimbing()
        {
            foreach (CustomCrumbleBlock Block in groupedCustomCrumbleBlocks)
            {
                if (Block.GetPlayerClimbing() != null)
                {
                    return Block;
                }
            }
            return null;
        }

        private bool isGroupCollidingWithSomething()
        {
            foreach (CustomCrumbleBlock Block in groupedCustomCrumbleBlocks)
            {
                if (Block.CollideCheck<Actor>() || Block.CollideCheck<Solid>())
                {
                    return true;
                }
            }
            return false;
        }

        public override void Render()
        {
            base.Render();
            for (int i = 0; i < outline.Count; i ++)
            {
                if (respawnTimer < i * outlineColorTimer)
                {
                    outline[i].Color = Color.DimGray * outlineColorStrength;
                }
                else
                {
                    outline[i].Color = Color.White * outlineColorStrength;
                }
            }
        }
    }
}
