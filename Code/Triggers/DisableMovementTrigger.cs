using System.Collections;
using Celeste.Mod.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Triggers
{
    [CustomEntity("XaphanHelper/DisableMovementTrigger")]
    class DisableMovementTrigger : Trigger
    {
        private string Flag;

        private float Time;

        private bool WalkCenter;

        private bool SwitchFacing;

        public DisableMovementTrigger(EntityData data, Vector2 offset) : base(data, offset)
        {
            Flag = data.Attr("flag");
            Time = data.Float("time");
            WalkCenter = data.Bool("walkCenter");
            SwitchFacing = data.Bool("switchFacing");
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            if (string.IsNullOrEmpty(Flag) || SceneAs<Level>().Session.GetFlag(Flag))
            {
                RemoveSelf();
            }
        }

        public override void OnStay(Player player)
        {
            base.OnStay(player);
            if (!Triggered && !string.IsNullOrEmpty(Flag) && SceneAs<Level>().Session.GetFlag(Flag))
            {
                Add(new Coroutine(WaitRoutine(player)));
            }
        }

        private IEnumerator WaitRoutine(Player player)
        {
            Triggered = true;
            player.StateMachine.State = Player.StDummy;
            SceneAs<Level>().CanRetry = false;
            if (WalkCenter)
            {
                yield return player.DummyWalkTo(Center.X - 4f);
            }
            if (SwitchFacing)
            {
                Facings currentFacing = player.Facing;
                if (currentFacing == Facings.Left)
                {
                    player.Facing = Facings.Right;
                }
                else
                {
                    player.Facing = Facings.Left;
                }
            }
            while (Time > 0)
            {
                Time -= Engine.DeltaTime;
                yield return null;
            }
            player.StateMachine.State = Player.StNormal;
            SceneAs<Level>().CanRetry = true;
        }
    }
}
