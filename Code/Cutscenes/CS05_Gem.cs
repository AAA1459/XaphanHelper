using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class CS05_Gem : CutsceneEntity
    {
        private readonly Player player;

        private BadelineDummy badeline;

        public CS05_Gem(Player player)
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
            XaphanModule.ModSaveData.WatchedCutscenes.Add("Xaphan/0_Ch5_Gem");
            level.Session.SetFlag("CS_Ch5_Gem");
            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
        }

        public IEnumerator Cutscene(Level level)
        {
            player.StateMachine.State = 11;
            player.StateMachine.Locked = true;
            yield return player.DummyWalkTo(Level.Bounds.Right - 44f, false, 1f);
            player.Jump();
            player.AutoJump = true;
            player.AutoJumpTimer = 1f;
            yield return player.DummyWalkTo(player.Position.X - 56f, false, 1f);
            yield return Level.ZoomTo(new Vector2(195f, 100f), 1.5f, 1f);
            badeline = CutscenesHelper.BadelineSplit(Level, player);
            yield return CutscenesHelper.BadelineFloat(this, 30, -18, badeline, -1, true, false, true);
            if (XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch5_W-Lore-00_46"))
            {
                yield return Textbox.Say("Xaphan_Ch5_A_Gem");
            }
            else
            {
                yield return Textbox.Say("Xaphan_Ch5_A_Gem_no_lore");
                player.Facing = Facings.Right;
                yield return Textbox.Say("Xaphan_Ch5_A_Gem_no_lore_b");
                player.Facing = Facings.Left;
                yield return Textbox.Say("Xaphan_Ch5_A_Gem_no_lore_c");
            }
            yield return Textbox.Say("Xaphan_Ch5_A_Gem_b");
            yield return CutscenesHelper.BadelineMerge(Level, player, badeline);
            yield return Level.ZoomBack(0.5f);
            EndCutscene(Level);
        }
    }
}