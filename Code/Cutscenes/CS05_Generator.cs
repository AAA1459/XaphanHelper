using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class CS05_Generator : CutsceneEntity
    {
        private readonly Player player;

        private BadelineDummy badeline;

        public CS05_Generator(Player player)
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
            XaphanModule.ModSaveData.WatchedCutscenes.Add("Xaphan/0_Ch5_Generator");
            level.Session.SetFlag("CS_Ch5_Generator");
            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
        }

        public IEnumerator Cutscene(Level level)
        {
            player.StateMachine.State = 11;
            player.StateMachine.Locked = true;
            if (!level.Session.GetFlag("CS_Ch5_Generator_P1"))
            {
                yield return Level.ZoomTo(new Vector2(110f, 110f), 1.5f, 1f);
                yield return 0.2f;
                badeline = CutscenesHelper.BadelineSplit(Level, player);
                yield return CutscenesHelper.BadelineFloat(this, 30, -18, badeline, -1, true, false, true);
                yield return Textbox.Say("Xaphan_Ch5_A_Generator");
                yield return CutscenesHelper.BadelineFloat(this, 0, 0, badeline, 1, true, false, true);
                Vector2 OrigCameraPosition = level.Camera.Position;
                Vector2 CameraPositionTo = OrigCameraPosition + new Vector2(100f, 0f);
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
                yield return CutscenesHelper.BadelineFloat(this, 0, 0, badeline, -1, true, false, true);
                if (XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch5_W-Lore-00_46"))
                {
                    yield return Textbox.Say("Xaphan_Ch5_A_Generator_b");
                }
                else
                {
                    yield return Textbox.Say("Xaphan_Ch5_A_Generator_b_no_lore");
                }
                yield return Textbox.Say("Xaphan_Ch5_A_Generator_c");
                yield return CutscenesHelper.BadelineMerge(Level, player, badeline);
                yield return Level.ZoomBack(0.5f);
            }
            level.InCutscene = false;
            level.CancelCutscene();
            level.Session.SetFlag("CS_Ch5_Generator_P1");
            player.StateMachine.State = 0;
            while (!XaphanModule.ModSaveData.GlobalFlags.Contains("Xaphan/0_Ch5_Auxiliary_Power"))
            {
                yield return null;
            }
            level.InCutscene = true;
            player.StateMachine.State = 11;
            yield return Level.ZoomTo(new Vector2(160f, 110f), 1.5f, 1f);
            yield return 0.2f;
            badeline = CutscenesHelper.BadelineSplit(Level, player);
            yield return CutscenesHelper.BadelineFloat(this, 30, -18, badeline, -1, true, false, true);
            yield return Textbox.Say("Xaphan_Ch5_A_Generator_d");
            yield return CutscenesHelper.BadelineMerge(Level, player, badeline);
            yield return Level.ZoomBack(0.5f);
            EndCutscene(Level);
        }

        public IEnumerator BadelineStopMadeline(BadelineDummy badeline)
        {
            yield return CutscenesHelper.BadelineFloat(this, -33, 16, badeline, 1, false, true, true);
            yield return Textbox.Say("Xaphan_Ch5_A_Start_e");
        }
    }
}
