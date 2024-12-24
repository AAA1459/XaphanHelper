using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class CS02_Start : CutsceneEntity
    {
        private readonly Player player;

        private BadelineDummy badeline;

        public CS02_Start(Player player)
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
            XaphanModule.ModSaveData.WatchedCutscenes.Add("Xaphan/0_Ch2_Start");
            level.Session.SetFlag("CS_Ch2_Start");
            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
        }

        public IEnumerator Cutscene(Level level)
        {
            player.StateMachine.State = 11;
            player.StateMachine.Locked = true;
            yield return Level.ZoomTo(new Vector2(160f, 110f), 1.5f, 1f);
            yield return 0.2f;
            yield return player.DummyWalkTo(player.Position.X - 15f, false, 0.5f);
            yield return 1f;
            yield return player.DummyWalkTo(player.Position.X + 25f, false, 0.5f);
            yield return 1.5f;
            yield return Textbox.Say("Xaphan_Ch2_A_Start");
            badeline = CutscenesHelper.BadelineSplit(Level, player);
            yield return CutscenesHelper.BadelineFloat(this, -30, -18, badeline, -1, true, false, true);
            yield return CutscenesHelper.BadelineFloat(this, 1, 0, badeline, null, true, false, false);
            yield return player.DummyWalkTo(player.Position.X - 5f, false, 0.5f);
            yield return 0.5f;
            yield return Textbox.Say("Xaphan_Ch2_A_Start_b");
            yield return player.DummyWalkTo(player.Position.X + 10f, false, 0.5f);
            yield return 1f;
            yield return Textbox.Say("Xaphan_Ch2_A_Start_c");
            yield return player.DummyWalkTo(player.Position.X - 5f, false, 0.5f);
            yield return 0.5f;
            yield return Textbox.Say("Xaphan_Ch2_A_Start_d");
            yield return CutscenesHelper.BadelineMerge(Level, player, badeline);
            yield return Level.ZoomBack(0.5f);
            EndCutscene(Level);
        }
    }
}
