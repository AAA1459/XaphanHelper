using System.Collections;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Cutscenes
{
    class CS05_Start : CutsceneEntity
    {
        private readonly Player player;

        private BadelineDummy badeline;

        private Vector2 badelinEndPosition;

        public CS05_Start(Player player)
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
            XaphanModule.ModSaveData.WatchedCutscenes.Add("Xaphan/0_Ch5_Start");
            level.Session.SetFlag("CS_Ch5_Start");
            player.StateMachine.Locked = false;
            player.StateMachine.State = 0;
        }

        public void badelineMerge(BadelineDummy badeline)
        {
            Audio.Play("event:/new_content/char/badeline/maddy_join_quick", badeline.Position);
            Level.Displacement.AddBurst(badeline.Center, 0.5f, 8f, 32f, 0.5f);
            badeline.RemoveSelf();
        }

        public void badelineFloat(int x, int y, BadelineDummy badeline, int? turnAtEndTo, bool faceDirection, bool fadeLight, bool quickEnd)
        {
            badelinEndPosition = new Vector2(badeline.Position.X + x, badeline.Position.Y + y);
            Add(new Coroutine(badeline.FloatTo(badelinEndPosition, turnAtEndTo, faceDirection, fadeLight, quickEnd)));
        }

        public void badelineFloatToPlayer(BadelineDummy badeline)
        {
            Add(new Coroutine(badeline.FloatTo(player.Position, null, true, false, true)));
        }

        public void badelineSplit(BadelineDummy badeline)
        {
            Audio.Play("event:/char/badeline/maddy_split", player.Position);
            Level.Add(badeline);
            Level.Displacement.AddBurst(badeline.Center, 0.5f, 8f, 32f, 0.5f);
        }

        public IEnumerator Cutscene(Level level)
        {
            player.StateMachine.State = 11;
            player.StateMachine.Locked = true;
            yield return Level.ZoomTo(new Vector2(160f, 110f), 1.5f, 1f);
            yield return 0.2f;
            yield return Textbox.Say("Xaphan_Ch5_A_Start");
            yield return player.DummyWalkTo(player.Position.X - 8f, false, 1f);
            player.Jump();
            player.AutoJump = true;
            player.AutoJumpTimer = 0.2f;
            yield return player.DummyWalkTo(player.Position.X - 76f, false, 1f);
            yield return 1f;
            yield return player.DummyWalkTo(player.Position.X + 40f, false, 0.75f);
            yield return 0.4f;
            yield return Textbox.Say("Xaphan_Ch5_A_Start_b");
            badeline = new BadelineDummy(player.Position);
            badelineSplit(badeline);
            badelineFloat(-30, -18, badeline, 1, true, false, true);
            while (badeline.Position != badelinEndPosition)
            {
                yield return 0.1f;
            }
            yield return Textbox.Say("Xaphan_Ch5_A_Start_c");
            player.Facing = Facings.Left;
            yield return Textbox.Say("Xaphan_Ch5_A_Start_d");
            yield return player.DummyWalkTo(player.Position.X - 8f, false, 1.25f);
            Coroutine BadelineRoutine = new();
            Add(BadelineRoutine = new Coroutine(BadelineStopMadeline(badeline)));
            yield return player.DummyWalkTo(player.Position.X - 32f, false, 1.25f);
            while (BadelineRoutine.Active)
            {
                yield return null;
            }
            yield return Textbox.Say("Xaphan_Ch5_A_Start_f");
            badelineFloatToPlayer(badeline);
            while (badeline.Position != player.Position)
            {
                yield return 0.1f;
            }
            badelineMerge(badeline);
            yield return Level.ZoomBack(0.5f);
            EndCutscene(Level);
        }

        public IEnumerator BadelineStopMadeline(BadelineDummy badeline)
        {
            badelineFloat(-33, 16, badeline, 1, false, true, true);
            yield return Textbox.Say("Xaphan_Ch5_A_Start_e");
        }
    }
}
