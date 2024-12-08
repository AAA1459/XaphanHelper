using System.Collections;
using Celeste.Mod.XaphanHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class CS01_BossDefeated : CutsceneEntity
    {
        private readonly Player player;

        private readonly Torizo boss;

        private BadelineDummy badeline;

        public CS01_BossDefeated(Player player, Torizo boss)
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
            if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch1_Boss_Defeated" + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : "")))
            {
                XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch1_Boss_Defeated");
                if (XaphanModule.PlayerHasGolden)
                {
                    XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch1_Boss_Defeated_GoldenStrawberry");
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
            if (player.Facing == Facings.Right)
            {
                yield return Textbox.Say("Xaphan_Ch1_A_Boss_Defeated");
                badeline = CutscenesHelper.BadelineSplit(Level, player);
                yield return CutscenesHelper.BadelineFloat(this, 30, -18, badeline, -1, true, false, true);
                yield return Textbox.Say("Xaphan_Ch1_A_Boss_Defeated_b");
            }
            else
            {
                yield return Textbox.Say("Xaphan_Ch1_A_Boss_Defeated_Face_Left");
                badeline = CutscenesHelper.BadelineSplit(Level, player);
                yield return CutscenesHelper.BadelineFloat(this, -30, -18, badeline, 1, true, false, true);
                yield return Textbox.Say("Xaphan_Ch1_A_Boss_Defeated_Face_Left_b");
            }
            yield return CutscenesHelper.BadelineMerge(Level, player, badeline);
            yield return Level.ZoomBack(0.5f);
            EndCutscene(Level);
        }
    }
}
