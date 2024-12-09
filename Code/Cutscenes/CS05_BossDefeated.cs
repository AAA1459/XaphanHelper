using System.Collections;
using Celeste.Mod.XaphanHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class CS05_BossDefeated : CutsceneEntity
    {
        private readonly Player player;

        private readonly Genesis boss;

        private BadelineDummy badeline;

        public CS05_BossDefeated(Player player, Genesis boss)
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
            if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch5_Boss_Defeated" + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : "")))
            {
                XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch5_Boss_Defeated");
                if (XaphanModule.PlayerHasGolden)
                {
                    XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch5_Boss_Defeated_GoldenStrawberry");
                }
            }
            player.StateMachine.State = 0;
        }

        public IEnumerator Cutscene(Level level)
        {
            foreach (MagneticCeiling ceiling in Level.Tracker.GetEntities<MagneticCeiling>())
            {
                if (ceiling.playerWasAttached)
                {
                    ceiling.ForceDetachPlayer = true;
                }
            }
            while (!player.OnGround())
            {
                yield return null;
            }
            player.StateMachine.State = 11;
            yield return Level.ZoomTo(new Vector2(160f, 110f), 1.5f, 1f);
            yield return 0.2f;
            bool jump = false;
            while (boss.Center.X < player.Center.X)
            {
                player.Facing = Facings.Left;
                yield return null;
                if (boss.Center.X >= player.Center.X - 48f && !jump)
                {
                    jump = true;
                    player.Jump();
                    player.AutoJump = true;
                    player.AutoJumpTimer = 1f;
                }
            }
            player.Facing = Facings.Right;
            while (boss.Sprite.CurrentAnimationID != "walk")
            {
                yield return null;
            }
            yield return 0.5f;
            badeline = CutscenesHelper.BadelineSplit(Level, player);
            yield return CutscenesHelper.BadelineFloat(this, 30, -18, badeline, -1, true, false, true);
            yield return Textbox.Say("Xaphan_Ch5_A_Boss_Defeated");
            if (XaphanModule.ModSaveData.SavedFlags.Contains("Xaphan/0_Ch5_X-Lore-00_4190"))
            {
                yield return Textbox.Say("Xaphan_Ch5_A_Boss_Defeated_b");
            }
            else
            {
                yield return Textbox.Say("Xaphan_Ch5_A_Boss_Defeated_b_no_lore");
            }
            yield return Textbox.Say("Xaphan_Ch5_A_Boss_Defeated_c");
            yield return CutscenesHelper.BadelineMerge(Level, player, badeline);
            yield return Level.ZoomBack(0.5f);
            EndCutscene(Level);
        }
    }
}
