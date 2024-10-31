using System.Collections.Generic;
using Celeste.Mod.XaphanHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using CelesteTags = Celeste.Tags;

namespace Celeste.Mod.XaphanHelper.Effects
{
    public class Heat : Backdrop
    {
        [Tracked(true)]
        public class BgHeat : Entity
        {
            public int ID;

            private bool[,] grid;

            public BgHeat(int ID)
            {
                Depth = 10100;
                Tag = (CelesteTags.TransitionUpdate);
                this.ID = ID;
            }

            public override void Added(Scene scene)
            {
                base.Added(scene);
                bool levelHasLava = false;
                foreach (Liquid liquid in SceneAs<Level>().Tracker.GetEntities<Liquid>())
                {
                    if (liquid.liquidType == "lava")
                    {
                        levelHasLava = true;
                        break;
                    }
                }
                if (!levelHasLava)
                {
                    //SceneAs<Level>().SnapColorGrade("none");
                    RemoveSelf();
                }
                grid = new bool[SceneAs<Level>().Bounds.Width / 8, SceneAs<Level>().Bounds.Height / 8];
                Add(new DisplacementRenderHook(RenderDisplacement));
                int i = 0;
                for (int length = grid.GetLength(0); i < length; i++)
                {
                    int j = 0;
                    for (int length2 = grid.GetLength(1); j < length2; j++)
                    {
                        grid[i, j] = !Scene.CollideCheck<SolidTiles>(new Rectangle(SceneAs<Level>().Bounds.X + i * 8, SceneAs<Level>().Bounds.Y + j * 8, 8, 8));
                    }
                }
            }

            public override void Update()
            {
                base.Update();
                Distort.WaterSineDirection = -1f;
                Distort.WaterAlpha = 0.5f;
                /*if (!SceneAs<Level>().Transitioning)
                {
                    SceneAs<Level>().NextColorGrade("hot", 0.85f);
                }
                else
                {
                    SceneAs<Level>().NextColorGrade("none", 5f);
                }*/
            }

            public void RenderDisplacement()
            {
                Color color = new(0.5f, 0.5f, 0.1f, 1f);
                int i = 0;
                int length = grid.GetLength(0);
                int length2 = grid.GetLength(1);
                for (; i < length; i++)
                {
                    if (length2 > 0 && grid[i, 0])
                    {
                        Draw.Rect(SceneAs<Level>().Bounds.X + (i * 8), SceneAs<Level>().Bounds.Y + 3f, 8f, 5f, color);
                    }
                    for (int j = 1; j < length2; j++)
                    {
                        if (grid[i, j])
                        {
                            int k;
                            for (k = 1; j + k < length2 && grid[i, j + k]; k++)
                            {
                            }
                            Draw.Rect(SceneAs<Level>().Bounds.X + (i * 8), SceneAs<Level>().Bounds.Y + (j * 8), 8f, k * 8, color);
                            j += k - 1;
                        }
                    }
                }
            }
        }

        public int ID = -1;

        public BgHeat heat;

        public Heat()
        {

        }

        public override void Update(Scene scene)
        {
            Level level = scene as Level;
            if (ID == -1)
            {
                HashSet<int> heats = new();
                foreach (BgHeat heat in scene.Tracker.GetEntities<BgHeat>())
                {
                    heats.Add(heat.ID);
                }
                ID = AffectID(heats);
            }
            if (IsVisible(level))
            {
                bool alreadyAdded = false;
                foreach (BgHeat heat in scene.Tracker.GetEntities<BgHeat>())
                {
                    if (heat.ID == ID)
                    {
                        alreadyAdded = true;
                        break;
                    }
                }
                if (!alreadyAdded)
                {
                    scene.Add(heat = new BgHeat(ID));
                }
            }
            if (!IsVisible(level))
            {
                foreach (BgHeat heat in scene.Tracker.GetEntities<BgHeat>())
                {
                    if (heat.ID == ID)
                    {
                        scene.Remove(heat);
                        break;
                    }
                }
            }
        }

        public int AffectID(HashSet<int> heats)
        {
            int SetID = Calc.Random.Next(0, 51);
            if (heats.Contains(SetID))
            {
                AffectID(heats);
            }
            else
            {
                return SetID;
            }
            return -1;
        }
    }
}
