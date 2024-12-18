using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class CS05_Generator2 : CutsceneEntity
    {
        private readonly Player player;

        private BadelineDummy badeline;

        public CS05_Generator2(Player player)
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
            XaphanModule.ModSaveData.WatchedCutscenes.Add("Xaphan/0_Ch5_Generator2");
            level.Session.SetFlag("CS_Ch5_Generator2");
            player.StateMachine.State = 0;
        }

        public IEnumerator Cutscene(Level level)
        {
            level.InCutscene = false;
            level.CancelCutscene();
            while (!XaphanModule.ModSaveData.GlobalFlags.Contains("Xaphan/0_Ch5_Auxiliary_Power"))
            {
                yield return null;
            }
            level.InCutscene = true;
            player.StateMachine.State = 11;
            yield return 0.3f;
            yield return Level.ZoomTo(new Vector2(160f, 110f), 1.5f, 1f);
            yield return 0.2f;
            badeline = CutscenesHelper.BadelineSplit(Level, player);
            yield return CutscenesHelper.BadelineFloat(this, 30, -18, badeline, -1, true, false, true);
            yield return Textbox.Say("Xaphan_Ch5_A_Generator_d");
            yield return CutscenesHelper.BadelineMerge(Level, player, badeline);
            yield return Level.ZoomBack(0.5f);
            EndCutscene(Level);
        }
    }
}
