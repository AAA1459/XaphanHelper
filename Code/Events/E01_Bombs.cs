using System.Collections;
using Celeste.Mod.XaphanHelper.Cutscenes;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Events
{
    class E01_Bombs : CutsceneEntity
    {
        private Player player;

        private float left;

        public E01_Bombs(Player player, Level level, float left)
        {
            this.player = player;
            this.left = left;
        }

        public override void OnBegin(Level level)
        {
            level.InCutscene = false;
            level.CancelCutscene();
            Add(new Coroutine(Cutscene(level)));
        }

        public IEnumerator Cutscene(Level level)
        {
            if (!level.Session.GetFlag("Torizo_Defeated") && !level.Session.GetFlag("D-U0_Gate_1"))
            {
                while (!level.Session.GetFlag("Upgrade_Bombs") && player.Right > left - 16f)
                {
                    yield return null;
                }
                level.Session.SetFlag("D-U0_Gate_1");
                level.Session.Audio.Music.Event = SFX.EventnameByHandle("event:/music/xaphan/lvl_0_tension");
                level.Session.Audio.Apply();
                while (!player.OnSafeGround)
                {
                    yield return null;
                }
                if (!XaphanModule.ModSaveData.WatchedCutscenes.Contains("Xaphan/0_Ch1_Before_Boss"))
                {
                    Scene.Add(new CS01_BeforeBoss(player));
                }
            }
        }

        public override void OnEnd(Level level)
        {

        }
    }
}
