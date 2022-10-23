﻿using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/PowerLine")]
    class PowerLine : Entity
    {
        Sprite Sprite;

        Sprite LineSprite;

        public float alpha = 0f;

        private string flag;

        private bool inverted;

        private string directory;

        Dictionary<Vector2, string> tiles = new Dictionary<Vector2, string>();

        Dictionary<Vector2, Vector2> tilesSpritePos = new Dictionary<Vector2, Vector2>();

        public PowerLine(EntityData data, Vector2 position, EntityID ID) : base(data.Position + position)
        {
            Tag = Tags.TransitionUpdate;
            Collider = new Hitbox(data.Width, data.Height);
            flag = data.Attr("flag");
            inverted = data.Bool("inverted");
            directory = data.Attr("directory");
            if (string.IsNullOrEmpty(directory))
            {
                directory = "objects/XaphanHelper/PowerLine";
            }
            Sprite = new Sprite(GFX.Game, directory + "/");
            Sprite.AddLoop("frame", "frame", 0.08f);
            Sprite.Play("frame");
            LineSprite = new Sprite(GFX.Game, directory + "/");
            LineSprite.AddLoop("on", "on", 0.08f);
            LineSprite.AddLoop("off", "off", 0.08f);
            LineSprite.Play("off");
            Depth = -19999;
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            for (int i = 0; i < Width / 8; i++)
            {
                for (int j = 0; j < Height / 8; j++)
                {
                    bool N = false;
                    bool S = false;
                    bool E = false;
                    bool W = false;
                    bool None = false;
                    if (Scene.CollideCheck<PowerLine>(new Rectangle((int)X + i * 8, (int)Y + j * 8 - 8, 1, 1)))
                    {
                        N = true;
                    }
                    if (Scene.CollideCheck<PowerLine>(new Rectangle((int)X + i * 8, (int)Y + j * 8 + 8, 1, 1)))
                    {
                        S = true;
                    }
                    if (Scene.CollideCheck<PowerLine>(new Rectangle((int)X + i * 8 + 8, (int)Y + j * 8, 1, 1)))
                    {
                        E = true;
                    }
                    if (Scene.CollideCheck<PowerLine>(new Rectangle((int)X + i * 8 - 8, (int)Y + j * 8, 1, 1)))
                    {
                        W = true;
                    }
                    None = !N && !S && !E && !W;
                    tiles.Add(new Vector2(i, j), None ? "None" : (N ? "N" : "") + (S ? "S" : "") + (E ? "E" : "") + (W ? "W" : ""));
                }
            }
            GetSpritePos();
        }

        public override void Update()
        {
            base.Update();
            alpha += Engine.DeltaTime * 4f;
            if (!string.IsNullOrEmpty(flag))
            {
                if (SceneAs<Level>().Session.GetFlag(flag))
                {
                    LineSprite.Play(inverted ? "off" : "on");
                }
                else
                {
                    LineSprite.Play(inverted ? "on" : "off");
                }
            }
        }

        public void GetSpritePos()
        {
            foreach (KeyValuePair<Vector2, string> tile in tiles)
            {
                switch (tile.Value)
                {
                    case "None":
                        tilesSpritePos.Add(tile.Key, new Vector2(1, 3));
                        break;
                    case "N":
                        tilesSpritePos.Add(tile.Key, new Vector2(3, 2));
                        break;
                    case "S":
                        tilesSpritePos.Add(tile.Key, new Vector2(4, 2));
                        break;
                    case "E":
                        tilesSpritePos.Add(tile.Key, new Vector2(4, 3));
                        break;
                    case "W":
                        tilesSpritePos.Add(tile.Key, new Vector2(3, 3));
                        break;
                    case "SE":
                        tilesSpritePos.Add(tile.Key, new Vector2(0, 0));
                        break;
                    case "SW":
                        tilesSpritePos.Add(tile.Key, new Vector2(2, 0));
                        break;
                    case "NS":
                        tilesSpritePos.Add(tile.Key, new Vector2(0, 1));
                        break;
                    case "NE":
                        tilesSpritePos.Add(tile.Key, new Vector2(0, 2));
                        break;
                    case "EW":
                        tilesSpritePos.Add(tile.Key, new Vector2(1, 2));
                        break;
                    case "NW":
                        tilesSpritePos.Add(tile.Key, new Vector2(2, 2));
                        break;
                    case "NSE":
                        tilesSpritePos.Add(tile.Key, new Vector2(3, 0));
                        break;
                    case "SEW":
                        tilesSpritePos.Add(tile.Key, new Vector2(4, 0));
                        break;
                    case "NEW":
                        tilesSpritePos.Add(tile.Key, new Vector2(3, 1));
                        break;
                    case "NSW":
                        tilesSpritePos.Add(tile.Key, new Vector2(4, 1));
                        break;
                    case "NSEW":
                        tilesSpritePos.Add(tile.Key, new Vector2(0, 3));
                        break;
                }
            }
        }

        public override void Render()
        {
            base.Render();
            for (int i = 0; i < Width / 8; i++)
            {
                for (int j = 0; j < Height / 8; j++)
                {
                    Sprite.RenderPosition = LineSprite.RenderPosition = Position + new Vector2(i * 8, j * 8);
                    Sprite.DrawSubrect(Vector2.Zero, new Rectangle((int)tilesSpritePos[new Vector2(i, j)].X * 8, (int)tilesSpritePos[new Vector2(i, j)].Y * 8, 8, 8));
                    LineSprite.Color = LineSprite.CurrentAnimationID == "on" ? Color.White * (0.9f * (0.9f + ((float)Math.Sin(alpha) + 1f) * 0.125f)) : Color.White;
                    LineSprite.DrawSubrect(Vector2.Zero, new Rectangle((int)tilesSpritePos[new Vector2(i, j)].X * 8, (int)tilesSpritePos[new Vector2(i, j)].Y * 8, 8, 8));
                }
            }
        }
    }
}
