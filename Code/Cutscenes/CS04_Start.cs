using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class CS04_Start : CutsceneEntity
    {
        private readonly Player player;

        private BadelineDummy badeline;

        public CS04_Start(Player player)
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
            XaphanModule.ModSaveData.WatchedCutscenes.Add("Xaphan/0_Ch4_Start");
            level.Session.SetFlag("CS_Ch4_Start");
            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
        }

        public IEnumerator Cutscene(Level level)
        {
            player.StateMachine.State = 11;
            player.StateMachine.Locked = true;
            Coroutine BadelineRoutine = new();
            Add(BadelineRoutine = new Coroutine(BadelineStopMadeline()));
            yield return player.DummyWalkTo(Level.Bounds.Left + 12f, false, 1f);
            yield return 0.35f;
            player.Facing = Facings.Right;
            while (BadelineRoutine.Active)
            {
                yield return null;
            }
            yield return Level.ZoomTo(new Vector2(110f, 110f), 1.5f, 1f);
            yield return 0.2f;
            yield return Textbox.Say("Xaphan_Ch4_A_Start_b");
            yield return CutscenesHelper.BadelineMerge(Level, player, badeline);
            yield return Level.ZoomBack(0.5f);
            EndCutscene(Level);
        }

        private IEnumerator BadelineStopMadeline()
        {
            yield return 0.2f;
            badeline = CutscenesHelper.BadelineSplit(Level, player);
            yield return CutscenesHelper.BadelineFloat(this, 30, -18, badeline, -1, true, false, true);
            yield return Textbox.Say("Xaphan_Ch4_A_Start");
        }
    }
}
