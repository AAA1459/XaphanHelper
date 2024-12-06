using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class CS02_BeforeTemple : CutsceneEntity
    {
        private readonly Player player;

        private BadelineDummy badeline;

        public CS02_BeforeTemple(Player player)
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
            XaphanModule.ModSaveData.WatchedCutscenes.Add("Xaphan/0_Ch2_Before_Temple");
            level.Session.SetFlag("CS_Ch2_Before_Temple");
            player.StateMachine.State = 0;
        }

        public IEnumerator Cutscene(Level level)
        {
            player.StateMachine.State = 11;
            while (!player.OnGround())
            {
                yield return null;
            }
            player.Facing = Facings.Right;
            badeline = CutscenesHelper.BadelineSplit(Level, player);
            yield return CutscenesHelper.BadelineFloat(this, 30, -18, badeline, -1, true, false, true);
            yield return CutscenesHelper.BadelineFloat(this, -1, 0, badeline, null, true, false, false);
            yield return Textbox.Say("Xaphan_Ch2_A_Before_Temple");
            Add(new Coroutine(CameraTo(new Vector2(level.Bounds.Left + 640, level.Bounds.Top), 2f, Ease.SineInOut)));
            yield return 2f;
            yield return Textbox.Say("Xaphan_Ch2_A_Before_Temple_b");
            yield return new FadeWipe(Scene, wipeIn: false).Duration = 0.5f;
            level.Camera.Position = new Vector2(level.Bounds.Left, level.Bounds.Top);
            yield return new FadeWipe(Scene, wipeIn: true);
            yield return Textbox.Say("Xaphan_Ch2_A_Before_Temple_c");
            yield return CutscenesHelper.BadelineMerge(Level, player, badeline);
            EndCutscene(Level);
        }
    }
}
