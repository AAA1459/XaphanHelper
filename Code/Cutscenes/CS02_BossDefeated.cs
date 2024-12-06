using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class CS02_BossDefeated : CutsceneEntity
    {
        private readonly Player player;

        private BadelineDummy badeline;

        public CS02_BossDefeated(Player player)
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
            string Prefix = SceneAs<Level>().Session.Area.LevelSet;
            if (!XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch2_Boss_Defeated" + (XaphanModule.PlayerHasGolden ? "_GoldenStrawberry" : "")))
            {
                XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch2_Boss_Defeated");
                if (XaphanModule.PlayerHasGolden)
                {
                    XaphanModule.ModSaveData.SavedFlags.Add(Prefix + "_Ch2_Boss_Defeated_GoldenStrawberry");
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
            player.StateMachine.State = 11;
            if (player.Position.X <= level.Bounds.Left + level.Bounds.Width / 2 - 15)
            {
                yield return player.DummyWalkToExact(level.Bounds.Left + level.Bounds.Width / 2 - 15);
            }
            else
            {
                yield return player.DummyWalkToExact(level.Bounds.Left + level.Bounds.Width / 2 - 15, true, 1.5f);
            }
            player.Facing = Facings.Right;
            player.DummyAutoAnimate = false;
            player.Sprite.Play("tired");
            yield return Level.ZoomTo(new Vector2(160f, 110f), 1.5f, 1f);
            yield return Textbox.Say("Xaphan_Ch2_A_Boss_Defeated");
            badeline = CutscenesHelper.BadelineSplit(Level, player);
            yield return CutscenesHelper.BadelineFloat(this, 30, -18, badeline, -1, true, false, true);
            yield return CutscenesHelper.BadelineFloat(this, -1, 0, badeline, null, true, false, false);
            yield return Textbox.Say("Xaphan_Ch2_A_Boss_Defeated_b");
            player.Sprite.Play("idle");
            player.DummyAutoAnimate = true;
            yield return 0.75f;
            player.Facing = Facings.Left;
            yield return 1f;
            player.Facing = Facings.Right;
            yield return Textbox.Say("Xaphan_Ch2_A_Boss_Defeated_c");
            yield return CutscenesHelper.BadelineMerge(Level, player, badeline);
            yield return Level.ZoomBack(0.5f);
            EndCutscene(Level);
        }
    }
}
