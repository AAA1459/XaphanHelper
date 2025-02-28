﻿using Celeste.Mod.XaphanHelper.Managers;
using Celeste.Mod.XaphanHelper.Upgrades;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.UI_Elements
{
    [Tracked(true)]
    class SpaceJumpIndicator : Entity
    {
        public SpaceJumpIndicator()
        {
            Depth = -20000;
            AddTag(Tags.Persistent);
        }

        public override void Update()
        {
            base.Update();
            if (!isActive(SceneAs<Level>()) && Visible)
            {
                Visible = false;
            }
            else if (isActive(SceneAs<Level>()) && !Visible)
            {
                Visible = true;
            }
        }

        public bool isActive(Level level)
        {
            return XaphanModule.ModSettings.SpaceJump == 2 && !XaphanModule.ModSaveData.SpaceJumpInactive.Contains(level.Session.Area.LevelSet);
        }

        public override void Render()
        {
            base.Render();
            if (Visible && (SceneAs<Level>().Session.Area.LevelSet == "Xaphan/0" ? XaphanModule.ModSettings.SoCMSpaceJumpIndicator : XaphanModule.ModSettings.SpaceJumpIndicator) != 0)
            {
                Player player = Scene.Tracker.GetEntity<Player>();
                ScrewAttackManager manager = SceneAs<Level>().Tracker.GetEntity<ScrewAttackManager>();
                bool startedScrewAttack = false;
                if (manager != null)
                {
                    startedScrewAttack = manager.StartedScrewAttack;
                }
                if (player != null && (player.Sprite.Visible || !player.Sprite.Visible && (SceneAs<Level>().Session.GetFlag("Xaphan_Helper_Ceiling") || startedScrewAttack)) && !XaphanModule.PlayerIsControllingRemoteDrone())
                {
                    string size = "";
                    if ((SceneAs<Level>().Session.Area.LevelSet == "Xaphan/0" ? XaphanModule.ModSettings.SoCMSpaceJumpIndicator : XaphanModule.ModSettings.SpaceJumpIndicator) == 2)
                    {
                        size = "large";
                    }
                    else if ((SceneAs<Level>().Session.Area.LevelSet == "Xaphan/0" ? XaphanModule.ModSettings.SoCMSpaceJumpIndicator : XaphanModule.ModSettings.SpaceJumpIndicator) == 1)
                    {
                        size = "small";
                    }
                    MTexture jumpIndicator = GFX.Gui["upgrades/jumpindicator-" + size];
                    for (int i = 0; i < SpaceJump.GetJumpBuffer(); i++)
                    {
                        jumpIndicator.Draw(player.Center + new Vector2(-4, -20f));
                    }
                }
            }
        }
    }
}
