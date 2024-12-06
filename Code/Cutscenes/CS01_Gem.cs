using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class CS01_Gem : CutsceneEntity
    {
        private readonly Player player;

        private BadelineDummy badeline;

        public CS01_Gem(Player player)
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
            XaphanModule.ModSaveData.WatchedCutscenes.Add("Xaphan/0_Ch1_Gem");
            level.Session.SetFlag("CS_Ch1_Gem");
            player.StateMachine.State = 0;
        }

        public IEnumerator Cutscene(Level level)
        {
            player.StateMachine.State = 11;
            yield return Level.ZoomTo(new Vector2(165f, 110f), 1.5f, 1f);
            yield return Textbox.Say("Xaphan_Ch1_A_Gem");
            badeline = CutscenesHelper.BadelineAppears(Level, new Vector2(player.Position.X - 60, player.Position.Y - 24));
            yield return CutscenesHelper.BadelineFloat(this, 1, 0, badeline, null, true, false, false);
            yield return 0.5;
            yield return player.DummyWalkTo(player.Position.X - 5f, false, 0.5f);
            yield return Textbox.Say("Xaphan_Ch1_A_Gem_b");
            yield return player.DummyRunTo(badeline.X + 30, false);
            yield return Textbox.Say("Xaphan_Ch1_A_Gem_c");
            yield return CutscenesHelper.BadelineFloat(this, 60, -15, badeline, null, true, false, false);
            yield return CutscenesHelper.BadelineFloat(this, 0, 0, badeline, null, true, false, false);
            yield return player.DummyWalkTo(player.Position.X + 5f, false, 0.5f);
            yield return Textbox.Say("Xaphan_Ch1_A_Gem_d");
            yield return CutscenesHelper.BadelineFloat(this, -1, 0, badeline, null, true, false, false);
            yield return Textbox.Say("Xaphan_Ch1_A_Gem_e");
            yield return CutscenesHelper.BadelineMerge(Level, player, badeline);
            yield return Level.ZoomBack(0.5f);
            EndCutscene(Level);
        }
    }
}