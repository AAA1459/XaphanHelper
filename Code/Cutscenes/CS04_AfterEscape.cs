using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class CS04_AfterEscape : CutsceneEntity
    {
        private readonly Player player;

        private BadelineDummy badeline;

        public CS04_AfterEscape(Player player)
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
            XaphanModule.ModSaveData.WatchedCutscenes.Add("Xaphan/0_Ch4_After_Escape");
            level.Session.SetFlag("CS_Ch4_After_Escape");
            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
        }

        public IEnumerator Cutscene(Level level)
        {
            yield return 0.5f;
            player.StateMachine.State = 11;
            player.StateMachine.Locked = true;
            player.DummyAutoAnimate = false;
            player.Sprite.Play("tired");
            yield return Level.ZoomTo(new Vector2(160f, 110f), 1.5f, 1f);
            yield return 1f;
            yield return Textbox.Say("Xaphan_Ch4_A_After_Escape");
            badeline = CutscenesHelper.BadelineSplit(Level, player);
            yield return CutscenesHelper.BadelineFloat(this, -30, -18, badeline, 1, true, false, true);
            yield return Textbox.Say("Xaphan_Ch4_A_After_Escape_b");
            player.Sprite.Play("idle");
            player.DummyAutoAnimate = true;
            yield return 0.5f;
            player.Facing = Facings.Left;
            yield return Textbox.Say("Xaphan_Ch4_A_After_Escape_c");
            yield return player.DummyWalkTo(player.Position.X - 16f, false, 0.75f);
            yield return CutscenesHelper.BadelineFloat(this, 0, 0, badeline, -1, true, false, true);
            yield return Textbox.Say("Xaphan_Ch4_A_After_Escape_d");
            yield return CutscenesHelper.BadelineMerge(Level, player, badeline);
            yield return Level.ZoomBack(0.5f);
            EndCutscene(Level);
        }
    }
}