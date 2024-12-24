﻿using System.Collections;
using Celeste.Mod.Entities;
using Celeste.Mod.XaphanHelper.Colliders;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Entities
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/BombSwitch")]
    public class BombSwitch : Entity
    {
        private bool bombInside;

        private bool triggered;

        public bool wasTriggered;

        public string flag;

        public Vector2? startSpawnPoint;

        public bool flagState;

        public bool registerInSaveData;

        public bool saveDataOnlyAfterCheckpoint;

        private string sprite;

        private Sprite switchSprite;

        private bool FlagRegiseredInSaveData()
        {
            Session session = SceneAs<Level>().Session;
            string Prefix = session.Area.LevelSet;
            int chapterIndex = session.Area.ChapterIndex;
            return XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flag);
        }

        public BombSwitch(EntityData data, Vector2 position) : base(data.Position + position)
        {
            Tag = Tags.TransitionUpdate;
            flag = data.Attr("flag");
            registerInSaveData = data.Bool("registerInSaveData");
            saveDataOnlyAfterCheckpoint = data.Bool("saveDataOnlyAfterCheckpoint");
            sprite = data.Attr("sprite", "objects/XaphanHelper/BombSwitch");
            Add(new BombCollider(OnBomb, new Circle(8f, 8f, 8f)));
            Add(switchSprite = new Sprite(GFX.Game, sprite + "/"));
            switchSprite.AddLoop("idle", "idle", 0);
            switchSprite.Play("idle");
            Depth = 8999;
        }

        public static void Load()
        {
            On.Celeste.ChangeRespawnTrigger.OnEnter += onChangeRespawnTriggerOnEnter;
        }

        public static void Unload()
        {
            On.Celeste.ChangeRespawnTrigger.OnEnter -= onChangeRespawnTriggerOnEnter;
        }

        private static void onChangeRespawnTriggerOnEnter(On.Celeste.ChangeRespawnTrigger.orig_OnEnter orig, ChangeRespawnTrigger self, Player player)
        {
            orig(self, player);
            bool onSolid = true;
            Vector2 point = self.Target + Vector2.UnitY * -4f;
            Session session = self.SceneAs<Level>().Session;
            if (self.Scene.CollideCheck<Solid>(point))
            {
                onSolid = self.Scene.CollideCheck<FloatySpaceBlock>(point);
            }
            if (onSolid && (!session.RespawnPoint.HasValue || session.RespawnPoint.Value != self.Target))
            {
                foreach (BombSwitch bombSwitch in self.SceneAs<Level>().Tracker.GetEntities<BombSwitch>())
                {
                    if (!string.IsNullOrEmpty(bombSwitch.flag))
                    {
                        bombSwitch.startSpawnPoint = session.RespawnPoint;
                        bombSwitch.flagState = session.GetFlag(bombSwitch.flag);
                        int chapterIndex = self.SceneAs<Level>().Session.Area.ChapterIndex;
                        self.SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + bombSwitch.flag + "_true", false);
                        self.SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + bombSwitch.flag + "_false", false);
                        if (bombSwitch.wasTriggered && bombSwitch.registerInSaveData && bombSwitch.saveDataOnlyAfterCheckpoint)
                        {
                            string Prefix = self.SceneAs<Level>().Session.Area.LevelSet;
                            if (self.SceneAs<Level>().Session.GetFlag(bombSwitch.flag) && !XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + bombSwitch.flag))
                            {
                                XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + bombSwitch.flag);
                            }
                            else if (self.SceneAs<Level>().Session.GetFlag(bombSwitch.flag) && XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + bombSwitch.flag))
                            {
                                XaphanModule.ModSaveData.SavedFlags.Remove(Prefix + "_Ch" + chapterIndex + "_" + bombSwitch.flag);
                            }
                        }
                    }
                }
            }
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            int chapterIndex = SceneAs<Level>().Session.Area.ChapterIndex;
            if (FlagRegiseredInSaveData())
            {
                flagState = true;
                SceneAs<Level>().Session.SetFlag(flag, true);
            }
            else
            {
                if (SceneAs<Level>().Session.GetFlag("Ch" + chapterIndex + "_" + flag + "_true"))
                {
                    flagState = true;
                    SceneAs<Level>().Session.SetFlag(flag, true);
                }
                else if (SceneAs<Level>().Session.GetFlag("Ch" + chapterIndex + "_" + flag + "_false"))
                {
                    flagState = false;
                    SceneAs<Level>().Session.SetFlag(flag, false);
                }
                else
                {
                    flagState = SceneAs<Level>().Session.GetFlag(flag);
                    SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + flag + "_true", false);
                    SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + flag + "_false", false);
                    SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + flag + "_" + (flagState ? "true" : "false"), true);
                }
            }
        }

        public override void Update()
        {
            base.Update();
            if (SceneAs<Level>().Transitioning && wasTriggered)
            {
                flagState = SceneAs<Level>().Session.GetFlag(flag);
                string Prefix = SceneAs<Level>().Session.Area.LevelSet;
                int chapterIndex = SceneAs<Level>().Session.Area.ChapterIndex;
                SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + flag + "_true", false);
                SceneAs<Level>().Session.SetFlag("Ch" + chapterIndex + "_" + flag + "_false", false);
                if (registerInSaveData && saveDataOnlyAfterCheckpoint)
                {
                    if (SceneAs<Level>().Session.GetFlag(flag) && !XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flag))
                    {
                        XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + flag);
                    }
                    else if (!SceneAs<Level>().Session.GetFlag(flag) && XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flag))
                    {
                        XaphanModule.ModSaveData.SavedFlags.Remove(Prefix + "_Ch" + chapterIndex + "_" + flag);
                    }
                }
            }
        }

        private void OnBomb(Bomb bomb)
        {
            if (!bomb.Hold.IsHeld && !triggered && !bombInside && bomb.AllowPushing)
            {
                triggered = true;
                Add(new Coroutine(MoveBomb(bomb)));
            }
        }

        private IEnumerator MoveBomb(Bomb bomb)
        {
            float timer = 0.15f;
            bomb.sloted = true;
            bomb.Depth = Depth - 1;
            while ((Vector2.Distance(Center + new Vector2(8, 12), bomb.Position) > 3f) && bomb != null && !bombInside && timer > 0f)
            {
                Vector2 vector = Calc.Approach(bomb.Position, Center + new Vector2(8, 12), 250f * Engine.DeltaTime);
                bomb.MoveToX(vector.X);
                bomb.MoveToY(vector.Y);
                timer -= Engine.DeltaTime;
                yield return null;
            }
            if (!bombInside)
            {
                Add(new Coroutine(SlotBomb(bomb)));
            }
        }

        private IEnumerator SlotBomb(Bomb bomb = null)
        {
            bombInside = true;
            if (bomb != null)
            {
                bomb.Position = Center + new Vector2(8, 12);
                while (!bomb.explode && !bomb.Hold.IsHeld)
                {
                    yield return null;
                }
                if (!bomb.Hold.IsHeld)
                {
                    wasTriggered = true;
                    startSpawnPoint = SceneAs<Level>().Session.RespawnPoint;
                    SceneAs<Level>().Session.SetFlag(flag, !SceneAs<Level>().Session.GetFlag(flag));
                    if (registerInSaveData && !saveDataOnlyAfterCheckpoint)
                    {
                        string Prefix = SceneAs<Level>().Session.Area.LevelSet;
                        int chapterIndex = SceneAs<Level>().Session.Area.ChapterIndex;
                        if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + flag))
                        {
                            XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch" + chapterIndex + "_" + flag);
                        }
                        else
                        {
                            XaphanModule.ModSaveData.SavedFlags.Remove(Prefix + "_Ch" + chapterIndex + "_" + flag);
                        }
                    }
                }
                else
                {
                    bomb.sloted = false;
                    bomb.Depth = 0;
                }
            }
            bombInside = triggered = false;
        }
    }
}
