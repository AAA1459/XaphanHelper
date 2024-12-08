using System.Collections;
using Celeste.Mod.XaphanHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class CS04_Warp : CutsceneEntity
    {
        private readonly Player player;

        private BadelineDummy badeline;

        public CS04_Warp(Player player)
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
            XaphanModule.ModSaveData.WatchedCutscenes.Add("Xaphan/0_Ch4_Warp");
            level.Session.SetFlag("CS_Ch4_Warp");
            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
        }

        public IEnumerator Cutscene(Level level)
        {
            player.StateMachine.State = 11;
            player.StateMachine.Locked = true;
            WarpStation station = Level.Tracker.GetEntity<WarpStation>();
            yield return player.DummyWalkTo(station.Center.X, false, 1f);
            yield return Level.ZoomTo(new Vector2(160f, 80f), 1.5f, 1f);
            yield return 0.2f;
            yield return Textbox.Say("Xaphan_Ch4_A_Warp");
            badeline = CutscenesHelper.BadelineSplit(Level, player);
            yield return CutscenesHelper.BadelineFloat(this, 30, -18, badeline, -1, true, false, true);
            yield return Textbox.Say("Xaphan_Ch4_A_Warp_b");
            if (XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch5_W-Lore-00_46"))
            {
                yield return Textbox.Say("Xaphan_Ch4_A_Warp_c");
            }
            else
            {
                yield return Textbox.Say("Xaphan_Ch4_A_Warp_c_no_lore");
            }
            yield return Textbox.Say("Xaphan_Ch4_A_Warp_d");
            yield return CutscenesHelper.BadelineMerge(Level, player, badeline);
            yield return Level.ZoomBack(0.5f);
            EndCutscene(Level);
        }
    }
}
