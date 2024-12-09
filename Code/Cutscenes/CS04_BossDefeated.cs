using System.Collections;
using Celeste.Mod.XaphanHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class CS04_BossDefeated : CutsceneEntity
    {
        private readonly Player player;

        private readonly AncientGuardian boss;

        private BadelineDummy badeline;

        public CS04_BossDefeated(Player player, AncientGuardian boss)
        {
            this.player = player;
            this.boss = boss;
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
            string Prefix = SceneAs<Level>().Session.Area.LevelSet;
            if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch4_Boss_Defeated" + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : "")))
            {
                XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch4_Boss_Defeated");
                if (XaphanModule.PlayerHasGolden)
                {
                    XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch4_Boss_Defeated_GoldenStrawberry");
                }
            }
            player.StateMachine.State = 0;
        }

        public IEnumerator Cutscene(Level level)
        {
            while (!player.OnGround())
            {
                yield return null;
            }
            if (player == null)
            {
                EndCutscene(Level);
                yield break;
            }
            player.StateMachine.State = 11;
            while (boss.Visible)
            {
                yield return null;
            }
            yield return 1f;
            yield return Level.ZoomTo(new Vector2(160f, 110f), 1.5f, 1f);
            player.Facing = Facings.Left;
            badeline = CutscenesHelper.BadelineSplit(Level, player);
            yield return CutscenesHelper.BadelineFloat(this, -30, -18, badeline, 1, true, false, true);
            yield return Textbox.Say("Xaphan_Ch4_A_Boss_Defeated");
            if (XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch1_Boss_Defeated"))
            {
                yield return Textbox.Say("Xaphan_Ch4_A_Boss_Defeated_b_Defeated_Alt_Boss");
                yield return 1f;
            }
            else
            {
                yield return Textbox.Say("Xaphan_Ch4_A_Boss_Defeated_b");
            }
            yield return CutscenesHelper.BadelineFloat(this, 0, 0, badeline, -1, true, false, true);
            yield return Textbox.Say("Xaphan_Ch4_A_Boss_Defeated_c");
            yield return CutscenesHelper.BadelineMerge(Level, player, badeline);
            yield return Level.ZoomBack(0.5f);
            EndCutscene(Level);
        }
    }
}
