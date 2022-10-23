﻿using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using MonoMod.Utils;
using System;
using System.Reflection;

namespace Celeste.Mod.XaphanHelper.Triggers
{
    [Tracked(true)]
    [CustomEntity("XaphanHelper/CameraBlocker")]
    class CameraBlocker : Trigger
    {
        private EntityData Data;

        private Vector2 Offset;

        private readonly bool topLeft, topCenter, topRight, centerLeft, centerRight, bottomLeft, bottomCenter, bottomRight, inside;

        private readonly string flag;

        private static ILHook hookOrigUpdate;

        private static ILHook hookLoadLevel;

        private static ILHook hookTransitionRoutine;

        public CameraBlocker(EntityData data, Vector2 offset) : base(data, offset)
        {
            Data = data;
            Offset = offset;
            topLeft = data.Bool("topLeft");
            topCenter = data.Bool("topCenter");
            topRight = data.Bool("topRight");
            centerLeft = data.Bool("centerLeft");
            centerRight = data.Bool("centerRight");
            bottomLeft = data.Bool("bottomLeft");
            bottomCenter = data.Bool("bottomCenter");
            bottomRight = data.Bool("bottomRight");
            inside = data.Bool("inside");
            flag = data.Attr("flag");
            AddTag(Tags.TransitionUpdate);
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            SceneAs<Level>().Add(new LookoutBlocker(Data, Offset));
        }

        public override void Awake(Scene scene)
        {
            base.Awake(scene);
            Update();
        }

        public override void Update()
        {
            base.Update();
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null)
            {
                if (!string.IsNullOrEmpty(flag) && !(Scene as Level).Session.GetFlag(flag))
                {
                    Collidable = false;
                }
                else
                {
                    bool top = player.Bottom <= Top;
                    bool centerY = player.Bottom > Top && player.Top < Bottom;
                    bool bottom = player.Top >= Bottom;
                    bool left = player.Right <= Left;
                    bool centerX = player.Right > Left && player.Left < Right;
                    bool right = player.Left >= Right;

                    Collidable =
                        (topLeft && top && left) ||
                        (topCenter && top && centerX) ||
                        (topRight && top && right) ||
                        (centerLeft && centerY && left) ||
                        (centerRight && centerY && right) ||
                        (bottomLeft && bottom && left) ||
                        (bottomCenter && bottom && centerX) ||
                        (bottomRight && bottom && right) ||
                        (inside && centerX && centerY);
                }
            }
        }

        public static void Load()
        {
            hookOrigUpdate = new ILHook(typeof(Player).GetMethod("orig_Update"), ilHookCameraTarget);
            hookLoadLevel = new ILHook(typeof(Level).GetMethod("orig_LoadLevel"), ilHookGetFullCameraTarget);
            hookTransitionRoutine = new ILHook(typeof(Level).GetMethod("orig_TransitionRoutine", BindingFlags.NonPublic | BindingFlags.Instance).GetStateMachineTarget(), ilHookGetFullCameraTarget);
        }

        public static void Unload()
        {
            hookOrigUpdate?.Dispose();
            hookLoadLevel?.Dispose();
            hookTransitionRoutine?.Dispose();

            hookOrigUpdate = null;
            hookLoadLevel = null;
            hookTransitionRoutine = null;
        }

        private static void ilHookCameraTarget(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Player>("get_CameraTarget")))
            {
                cursor.EmitDelegate<Func<Vector2, Vector2>>(modCameraTarget);
            }
        }

        private static void ilHookGetFullCameraTarget(ILContext il)
        {
            ILCursor cursor = new ILCursor(il);
            while (cursor.TryGotoNext(MoveType.After, instr => instr.MatchCallvirt<Level>("GetFullCameraTargetAt")))
            {
                cursor.EmitDelegate<Func<Vector2, Vector2>>(modCameraTarget);
            }
        }

        private static Vector2 modCameraTarget(Vector2 orig)
        {
            Player p = Engine.Scene.Tracker.GetEntity<Player>();
            if (p == null) return orig;

            Rectangle viewpoint = new Rectangle((int)orig.X, (int)orig.Y, 320, 180);
            foreach (CameraBlocker border in Engine.Scene.Tracker.GetEntities<CameraBlocker>())
            {
                while (border.Collidable && border.CollideRect(viewpoint))
                {
                    // the border is enabled and on-screen, unacceptable!
                    if (p.Left <= border.Right && viewpoint.Right >= border.Left && (border.topLeft || border.centerLeft || border.bottomLeft))
                    {
                        // player is on the left, camera is too far right => push camera to the left.
                        orig.X--;
                        viewpoint.X--;
                    }
                    else if (p.Right >= border.Left && viewpoint.Left <= border.Right && (border.topRight || border.centerRight || border.bottomRight))
                    {
                        // player is on the right, camera is too far left => push camera to the right.
                        orig.X++;
                        viewpoint.X++;
                    }
                    else if (p.Bottom >= border.Top && viewpoint.Top <= border.Bottom && (border.bottomLeft || border.bottomCenter || border.bottomRight))
                    {
                        // player is on the bottom, camera is too far up => push camera to the bottom.
                        orig.Y++;
                        viewpoint.Y++;
                    }
                    else if (p.Top <= border.Bottom && viewpoint.Bottom >= border.Top && (border.topLeft || border.topCenter || border.topRight))
                    {
                        // player is on the top, camera is too far down => push camera to the top;
                        orig.Y--;
                        viewpoint.Y--;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return orig;
        }
    }
}
