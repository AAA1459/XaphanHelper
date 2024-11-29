using System.Collections;
using System.Reflection;
using Celeste.Mod.XaphanHelper.Entities;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.Managers
{
    [Tracked(true)]
    class BreathManager : Entity
    {
        private static FieldInfo playerFlash = typeof(Player).GetField("flash", BindingFlags.NonPublic | BindingFlags.Instance);

        public static bool Flashing;

        public float air;

        public Liquid currentLiquid;

        public float currentMaxAir;

        public bool isVisible;

        private Coroutine AirRoutine = new();

        public static bool forceRechargeAir;

        public BreathManager()
        {
            Tag = Tags.Persistent;
            air = -1f;
            Flashing = false;
            Depth = -20000;
        }

        public static void Load()
        {
            On.Celeste.Player.Render += modPlayerRender;
        }

        public static void Unload()
        {
            On.Celeste.Player.Render -= modPlayerRender;
        }

        private static void modPlayerRender(On.Celeste.Player.orig_Render orig, Player self)
        {
            orig(self);
            if (Flashing && !self.Dead && !forceRechargeAir)
            {
                self.Sprite.Color = Calc.HexToColor("0020BB");
            }
        }

        public static bool determineifBreathManager()
        {
            if (Engine.Scene is Level)
            {
                Level level = (Level)Engine.Scene;
                BreathManager manager = level.Tracker.GetEntity<BreathManager>();
                if (manager != null)
                {
                    if (Flashing)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override void Update()
        {
            Level level = SceneAs<Level>();
            Player player = Scene.Tracker.GetEntity<Player>();
            if (player != null)
            {
                if (player.CollideCheck<AirBubbles>())
                {
                    forceRechargeAir = true;
                }
                else
                {
                    forceRechargeAir = false;
                }
            }
            bool playerCurrentlyInLiquid = false;
            foreach (Liquid liquid in level.Tracker.GetEntities<Liquid>())
            {
                if (liquid.PlayerCompletelyInside() && liquid.canDrown && !liquid.visualOnly)
                {
                    playerCurrentlyInLiquid = true;
                    if (liquid != currentLiquid)
                    {
                        currentLiquid = liquid;
                        if (air == -1f)
                        {
                            air = liquid.airTimer;
                        }
                        else
                        {
                            air = GetAirPercent() * liquid.airTimer / 100;
                        }
                        currentMaxAir = liquid.airTimer;
                    }
                    break;
                }
            }
            if (playerCurrentlyInLiquid)
            {
                isVisible = true;
                if (!XaphanModule.UIOpened && currentLiquid != null)
                {
                    if (player != null && !player.Dead)
                    {
                        if (Scene.OnInterval(GetAirPercent() > ((100 / 15) * 4) ? 1.5f : 0.75f))
                        {
                            Scene.Add(new Liquid.AirBubble(player.Facing == Facings.Left ? player.TopLeft : player.TopRight - new Vector2(4f, 0f), currentLiquid));
                        }
                        if (GetAirPercent() < (4 * (100f / 15f)) && !XaphanModule.PlayerIsControllingRemoteDrone() && !forceRechargeAir)
                        {
                            if (Scene.OnRawInterval(0.06f))
                            {
                                Flashing = !Flashing;
                            }
                        }
                        else
                        {
                            Flashing = false;
                        }
                    }
                    if (!forceRechargeAir)
                    {
                        if (AirRoutine.Active)
                        {
                            AirRoutine.Cancel();
                        }
                        if (player != null && player.CanRetry && !XaphanModule.PlayerIsControllingRemoteDrone())
                        {
                            air -= Engine.DeltaTime;
                        }
                        if (air <= 0f)
                        {
                            air = 0f;
                            if (!player.Dead)
                            {
                                player.Die(Vector2.Zero);
                            }
                        }
                    }
                    else
                    {
                        if (!AirRoutine.Active)
                        {
                            Flashing = false;
                            Add(AirRoutine = new Coroutine(RechargeAir(false)));
                        }
                    }
                }
            }
            else if (currentLiquid != null && player != null)
            {
                currentLiquid = null;
                Flashing = false;
                if (!AirRoutine.Active)
                {
                    Add(AirRoutine = new Coroutine(RechargeAir()));
                }                
            }
            if (AirRoutine != null)
            {
                AirRoutine.Update();
            }
        }

        public float GetAirPercent()
        {
            return air * 100 / currentMaxAir;
        }

        private IEnumerator RechargeAir(bool HideAtEnd = true)
        {
            Flashing = false;
            while (air < currentMaxAir)
            {
                air += Engine.DeltaTime * currentMaxAir;
                yield return null;
            }
            air = currentMaxAir;
            if (HideAtEnd)
            {
                yield return 0.3f;
                isVisible = false;
            }
        }
    }
}
