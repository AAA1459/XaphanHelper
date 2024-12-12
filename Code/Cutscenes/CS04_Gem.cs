using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class CS04_Gem : CutsceneEntity
    {
        private readonly Player player;

        private BadelineDummy badeline;

        public CS04_Gem(Player player)
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
            XaphanModule.ModSaveData.WatchedCutscenes.Add("Xaphan/0_Ch4_Gem");
            level.Session.SetFlag("CS_Ch4_Gem");
            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
        }

        public IEnumerator Cutscene(Level level)
        {
            level.InCutscene = false;
            level.CancelCutscene();
            while (!XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch4_Gem_Collected" + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : "")))
            {
                yield return null;
            }
            level.InCutscene = true;
            player.StateMachine.State = 11;
            player.StateMachine.Locked = true;
            if (player.Speed.X != 0)
            {
                player.Speed.X = Calc.Approach(player.Speed.X, 0f, Engine.DeltaTime * 200f);
            }
            while (!player.OnSafeGround)
            {
                yield return null;
            }
            player.Facing = Facings.Right;
            yield return Level.ZoomTo(new Vector2(160f, 110f), 1.5f, 1f);
            yield return 0.2f;
            badeline = CutscenesHelper.BadelineSplit(Level, player);
            yield return CutscenesHelper.BadelineFloat(this, 30, -18, badeline, -1, true, false, true);
            if (!XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch1_Gem2_Sloted" + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : "")))
            {
                yield return Textbox.Say("Xaphan_Ch4_A_Gem");
            }
            else
            {
                yield return Textbox.Say("Xaphan_Ch4_A_Gem_b");
            }
            yield return Textbox.Say("Xaphan_Ch4_A_Gem_c");
            yield return CutscenesHelper.BadelineMerge(Level, player, badeline);
            yield return Level.ZoomBack(0.5f);
            EndCutscene(Level);
        }
    }
}
