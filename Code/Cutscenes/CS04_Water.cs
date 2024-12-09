using System.Collections;
using Celeste.Mod.XaphanHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class CS04_Water : CutsceneEntity
    {
        private readonly Player player;

        private BadelineDummy badeline;

        public CS04_Water(Player player)
        {
            this.player = player;
        }

        public override void OnBegin(Level level)
        {
            if (XaphanModule.ModSettings.AutoSkipCutscenes)
            {
                EndCutscene(Level);
                WasSkipped = true;
            }
            else
            {
                Add(new Coroutine(Cutscene(level)));
            }
        }

        public override void OnEnd(Level level)
        {
            if (WasSkipped)
            {
                if (badeline != null)
                {
                    badeline.RemoveSelf();
                }
            }
            XaphanModule.ModSaveData.WatchedCutscenes.Add("Xaphan/0_Ch4_Water");
            level.Session.SetFlag("CS_Ch4_Water");
            level.Session.RespawnPoint = level.Session.GetSpawnPoint(player.Center);
            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
        }

        public IEnumerator Cutscene(Level level)
        {
            player.StateMachine.State = 11;
            player.StateMachine.Locked = true;
            MagneticCeiling ceiling = Level.Tracker.GetNearestEntity<MagneticCeiling>(player.Center);
            if (ceiling != null)
            {
                if (ceiling.playerWasAttached)
                {
                    ceiling.ForceDetachPlayer = true;
                }
            }
            yield return player.DummyWalkTo(player.Position.X + 12f, false, 1f);
            while (!player.OnSafeGround)
            {
                yield return null;
            }
            yield return Level.ZoomTo(new Vector2(210f, 74f), 1.5f, 1f);
            yield return 0.2f;
            badeline = CutscenesHelper.BadelineSplit(Level, player);
            yield return CutscenesHelper.BadelineFloat(this, 30, -18, badeline, -1, true, false, true);
            yield return Textbox.Say("Xaphan_Ch4_A_Water");
            yield return CutscenesHelper.BadelineFloat(this, 0, 0, badeline, 1, true, false, true);
            yield return Textbox.Say("Xaphan_Ch4_A_Water_b");
            Vector2 OrigCameraPosition = level.Camera.Position;
            Vector2 CameraPositionTo = OrigCameraPosition + new Vector2(0f, 30f);
            Add(new Coroutine(CameraTo(CameraPositionTo, 1f, Ease.SineInOut)));
            while (level.Camera.Position != CameraPositionTo)
            {
                yield return null;
            }
            yield return 1.5f;
            Add(new Coroutine(CameraTo(OrigCameraPosition, 1f, Ease.SineInOut)));
            while (level.Camera.Position != OrigCameraPosition)
            {
                yield return null;
            }
            yield return Textbox.Say("Xaphan_Ch4_A_Water_c");
            yield return CutscenesHelper.BadelineMerge(Level, player, badeline);
            yield return Level.ZoomBack(0.5f);
            EndCutscene(Level);
        }
    }
}
