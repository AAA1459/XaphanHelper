﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Xml;
using Celeste.Mod.Meta;
using Celeste.Mod.UI;
using Celeste.Mod.XaphanHelper.Colliders;
using Celeste.Mod.XaphanHelper.Components;
using Celeste.Mod.XaphanHelper.Controllers;
using Celeste.Mod.XaphanHelper.Cutscenes;
using Celeste.Mod.XaphanHelper.Data;
using Celeste.Mod.XaphanHelper.Effects;
using Celeste.Mod.XaphanHelper.Enemies;
using Celeste.Mod.XaphanHelper.Entities;
using Celeste.Mod.XaphanHelper.Hooks;
using Celeste.Mod.XaphanHelper.Managers;
using Celeste.Mod.XaphanHelper.Triggers;
using Celeste.Mod.XaphanHelper.UI_Elements;
using Celeste.Mod.XaphanHelper.Upgrades;
using FMOD.Studio;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Monocle;
using MonoMod.Cil;
using MonoMod.Utils;
using static Celeste.Mod.XaphanHelper.XaphanModuleSession;

namespace Celeste.Mod.XaphanHelper
{
    public class XaphanModule : EverestModule
    {
        public static SpriteBank SpriteBank;

        public static XaphanModule Instance;

        // If you need to store ModSettings:
        public override Type SettingsType => typeof(XaphanModuleSettings);
        public static XaphanModuleSettings ModSettings => (XaphanModuleSettings)Instance._Settings;

        // If you need to store save data:
        public override Type SaveDataType => typeof(XaphanModuleSaveData);
        public static XaphanModuleSaveData ModSaveData => (XaphanModuleSaveData)Instance._SaveData;

        // If you need to store session data:
        public override Type SessionType => typeof(XaphanModuleSession);
        public static XaphanModuleSession ModSession => (XaphanModuleSession)Instance._Session;

        public static List<TeleportToOtherSideData> TeleportToOtherSideData = new();

        public static List<RoomMusicControllerData> RoomMusicControllerData = new();

        private FieldInfo OuiChapterSelect_icons = typeof(OuiChapterSelect).GetField("icons", BindingFlags.Instance | BindingFlags.NonPublic);

        private FieldInfo OuiChapterPanel_modes = typeof(OuiChapterPanel).GetField("modes", BindingFlags.Instance | BindingFlags.NonPublic);

        private FieldInfo OuiChapterPanel_instantClose = typeof(OuiChapterPanel).GetField("instantClose", BindingFlags.Instance | BindingFlags.NonPublic);

        private FieldInfo OuiChapterSelectIcon_tween = typeof(OuiChapterSelectIcon).GetField("tween", BindingFlags.Instance | BindingFlags.NonPublic);

        private FieldInfo OuiChapterSelectIcon_front = typeof(OuiChapterSelectIcon).GetField("front", BindingFlags.Instance | BindingFlags.NonPublic);

        private FieldInfo OuiChapterSelectIcon_back = typeof(OuiChapterSelectIcon).GetField("back", BindingFlags.Instance | BindingFlags.NonPublic);

        private FieldInfo OuiJournalProress_table = typeof(OuiJournalProgress).GetField("table", BindingFlags.Instance | BindingFlags.NonPublic);

        private FieldInfo OuiJournalSpeedrun_table = typeof(OuiJournalSpeedrun).GetField("table", BindingFlags.Instance | BindingFlags.NonPublic);

        private FieldInfo OuiFileSelectSlot_HighlightEase = typeof(OuiFileSelectSlot).GetField("highlightEase", BindingFlags.Instance | BindingFlags.NonPublic);

        private FieldInfo Overworld_transitioning = typeof(Overworld).GetField("transitioning", BindingFlags.Instance | BindingFlags.NonPublic);

        private FieldInfo OuiMapList_menu = typeof(OuiMapList).GetField("menu", BindingFlags.Instance | BindingFlags.NonPublic);

        private Type OuiChapterPanel_T_Option = typeof(OuiChapterPanel).GetNestedType("Option", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static);

        private bool hasOldExtendedVariants = false;

        private bool displayedOldExtVariantsPostcard = false;

        public static Version SoCMVersion;

        public static bool SaveUpdaterUpdateLorebook = false;

        public static bool startedAnyChapter = false;

        public static bool startedAnySoCMChapter = false;

        public static bool SkipSoCMIntro = false;

        public static bool SoCMTitleFromGame = false;

        public static bool onSlope;

        public static int onSlopeDir;

        public static bool onSlopeGentle;

        public static float onSlopeTop;

        public static bool onSlopeAffectPlayerSpeed;

        public static float MaxRunSpeed;

        public static bool ChangingSide;

        private Postcard oldExtVariantsPostcard;

        public static bool useMergeChaptersControllerCheck;

        public static bool useMergeChaptersController;

        public static string MergeChaptersControllerMode;

        public static bool MergeChaptersControllerKeepPrologue;

        private string MergeChaptersControllerLevelSet;

        private string lastLevelSet;

        public static bool isInLevel = false;

        public static bool CanOpenMap(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.LevelSet + "_Can_Open_Map");
        }

        public static bool UIOpened;

        public static bool ShowUI;

        public static bool TeleportBackFromDrone;

        public static bool NoDroneSpawnSound;

        public static bool refillJumps;

        public static bool SaveIconVisible = true;

        public enum Upgrades
        {
            // Celeste Upgrades

            PowerGrip,
            ClimbingKit,
            SpiderMagnet,
            DroneTeleport,
            JumpBoost,
            Bombs,
            MegaBombs,
            RemoteDrone,
            GoldenFeather,
            Binoculars,
            EtherealDash,
            PortableStation,
            PulseRadar,
            DashBoots,
            HoverJet,
            LightningDash,
            MissilesModule,
            SuperMissilesModule,

            // Metroid Upgrades

            Spazer,
            PlasmaBeam,
            MorphingBall,
            MorphBombs,
            SpringBall,
            HighJumpBoots,
            SpeedBooster,

            //Common Upgrades

            LongBeam,
            IceBeam,
            WaveBeam,
            VariaJacket,
            GravityJacket,
            ScrewAttack,
            SpaceJump
        }

        // Celeste Upgrades

        public static bool PowerGripCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.LevelSet + "_Upgrade_PowerGrip");
        }

        public static bool ClimbingKitCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.LevelSet + "_Upgrade_ClimbingKit");
        }

        public static bool SpiderMagnetCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.LevelSet + "_Upgrade_SpiderMagnet");
        }

        public static bool DroneTeleportCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.LevelSet + "_Upgrade_DroneTeleport");
        }

        public static bool JumpBoostCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.LevelSet + "_Upgrade_JumpBoost");
        }

        public static bool BombsCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.LevelSet + "_Upgrade_Bombs");
        }

        public static bool MegaBombsCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.LevelSet + "_Upgrade_MegaBombs");
        }

        public static bool RemoteDroneCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.LevelSet + "_Upgrade_RemoteDrone");
        }

        public static bool GoldenFeatherCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.LevelSet + "_Upgrade_GoldenFeather");
        }

        public static bool BinocularsCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.LevelSet + "_Upgrade_Binoculars");
        }

        public static bool EtherealDashCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.LevelSet + "_Upgrade_EtherealDash");
        }

        public static bool PortableStationCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.LevelSet + "_Upgrade_PortableStation");
        }

        public static bool PulseRadarCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.LevelSet + "_Upgrade_PulseRadar");
        }

        public static bool DashBootsCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.LevelSet + "_Upgrade_DashBoots");
        }

        public static bool HoverJetCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.LevelSet + "_Upgrade_HoverJet");
        }

        public static bool LightningDashCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.LevelSet + "_Upgrade_LightningDash");
        }

        public static bool MissilesModuleCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.LevelSet + "_Upgrade_MissilesModule");
        }

        public static bool SuperMissilesModuleCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.LevelSet + "_Upgrade_SuperMissilesModule");
        }

        // Metroid Upgrades

        public static bool SpazerCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.LevelSet + "_Upgrade_Spazer");
        }

        public static bool PlasmaBeamCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.LevelSet + "_Upgrade_PlasmaBeam");
        }

        public static bool MorphingBallCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.LevelSet + "_Upgrade_MorphingBall");
        }

        public static bool MorphBombsCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.LevelSet + "_Upgrade_MorphBombs");
        }

        public static bool SpringBallCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.LevelSet + "_Upgrade_SpringBall");
        }

        public static bool HighJumpBootsCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.LevelSet + "_Upgrade_HighJumpBoots");
        }

        public static bool SpeedBoosterCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.LevelSet + "_Upgrade_SpeedBooster");
        }

        // Common Upgrades

        public static bool LongBeamCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.LevelSet + "_Upgrade_LongBeam");
        }

        public static bool IceBeamCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.LevelSet + "_Upgrade_IceBeam");
        }

        public static bool WaveBeamCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.LevelSet + "_Upgrade_WaveBeam");
        }

        public static bool VariaJacketCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.LevelSet + "_Upgrade_VariaJacket");
        }

        public static bool GravityJacketCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.LevelSet + "_Upgrade_GravityJacket");
        }

        public static bool ScrewAttackCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.LevelSet + "_Upgrade_ScrewAttack");
        }

        public static bool SpaceJumpCollected(Level level)
        {
            return ModSaveData.SavedFlags.Contains(level.Session.Area.LevelSet + "_Upgrade_SpaceJump");
        }

        public static void useIngameMapCheck(Level level)
        {
            AreaKey area = level.Session.Area;
            MapData MapData = AreaData.Areas[area.ID].Mode[(int)area.Mode].MapData;
            foreach (LevelData levelData in MapData.Levels)
            {
                foreach (EntityData entity in levelData.Entities)
                {
                    if (entity.Name == "XaphanHelper/InGameMapController")
                    {
                        useIngameMap = true;
                        inGameMapProgressDisplayMode = entity.Attr("showProgress");
                        break;
                    }
                }
                if (useIngameMap)
                {
                    break;
                }
            }
        }

        public static void allRoomsUseTileControllerCheck(Level level)
        {
            AreaKey area = level.Session.Area;
            MapData MapData = AreaData.Areas[area.ID].Mode[(int)area.Mode].MapData;
            foreach (LevelData levelData in MapData.Levels)
            {
                if (levelData.Spawns.Count != 0)
                {
                    allRoomsUseTileController = false;
                    foreach (EntityData entity in levelData.Entities)
                    {
                        if (entity.Name == "XaphanHelper/InGameMapTilesController")
                        {
                            allRoomsUseTileController = true;
                            break;
                        }
                    }
                    if (!allRoomsUseTileController)
                    {
                        break;
                    }
                }
            }
        }

        public static void useUpgradesCheck(Level level)
        {
            if (useMetroidGameplay)
            {
                useUpgrades = true;
            }
            else
            {
                AreaKey area = level.Session.Area;
                MapData MapData = AreaData.Areas[area.ID].Mode[(int)area.Mode].MapData;
                foreach (LevelData levelData in MapData.Levels)
                {
                    foreach (EntityData entity in levelData.Entities)
                    {
                        if (entity.Name == "XaphanHelper/UpgradeController")
                        {
                            useUpgrades = true;
                            DisableStatusScreen = entity.Bool("disableStatusScreen", false);
                            break;
                        }
                    }
                    if (useUpgrades)
                    {
                        break;
                    }
                }
            }
        }

        public static void useMetroidGameplayCheck(Level level)
        {
            AreaKey area = level.Session.Area;
            MapData MapData = AreaData.Areas[area.ID].Mode[(int)area.Mode].MapData;
            foreach (LevelData levelData in MapData.Levels)
            {
                foreach (EntityData entity in levelData.Entities)
                {
                    if (entity.Name == "XaphanHelper/MetroidGameplayController")
                    {
                        useMetroidGameplay = true;
                        level.Tracker.GetEntity<Player>().ResetSprite(PlayerSpriteMode.Madeline);
                        break;
                    }
                }
                if (useMetroidGameplay)
                {
                    break;
                }
            }
        }

        public static bool useMetroidGameplaySessionCheck(Session session)
        {
            AreaKey area = session.Area;
            MapData MapData = AreaData.Areas[area.ID].Mode[(int)area.Mode].MapData;
            foreach (LevelData levelData in MapData.Levels)
            {
                foreach (EntityData entity in levelData.Entities)
                {
                    if (entity.Name == "XaphanHelper/MetroidGameplayController")
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public static bool PlayerIsControllingRemoteDrone()
        {
            if (Engine.Scene is Level)
            {
                Level level = (Level)Engine.Scene;
                Drone drone = level.Tracker.GetEntity<Drone>();
                if (drone != null && !Drone.Hold.IsHeld)
                {
                    return true;
                }
            }
            return false;
        }

        public static bool useIngameMap;

        public static string inGameMapProgressDisplayMode;

        public static bool allRoomsUseTileController;

        public static bool useUpgrades;

        public static bool DisableStatusScreen;

        public static bool forceStartingUpgrades;

        public static bool useMetroidGameplay;

        public Dictionary<Upgrades, Upgrade> UpgradeHandlers = new();

        public static bool PlayerHasGolden;

        public bool PlayerLostGolden;

        private bool CanLoadPlayer;

        public float cassetteAlpha;

        public bool cassetteWaitForKeyPress;

        public float cassetteTimer;

        public static bool TriggeredCountDown;

        public static bool minimapEnabled;

        public static bool onTitleScreen;

        public static bool startedGame;

        private bool onMapListOrSearch;

        public override void CreateModMenuSection(TextMenu menu, bool inGame, EventInstance pauseSnapshot)
        {
            bool isPlayingSoCM = false;
            if (Engine.Scene is Level)
            {
                isPlayingSoCM = ((Level)Engine.Scene).Session.Area.LevelSet == "Xaphan/0";
            }
            if (!isPlayingSoCM)
            {
                base.CreateModMenuSection(menu, inGame, pauseSnapshot);
            }
        }

        public XaphanModule()
        {
            Instance = this;

            // Celeste Upgrades

            UpgradeHandlers[Upgrades.PowerGrip] = new PowerGrip();
            UpgradeHandlers[Upgrades.ClimbingKit] = new ClimbingKit();
            UpgradeHandlers[Upgrades.SpiderMagnet] = new SpiderMagnet();
            UpgradeHandlers[Upgrades.DroneTeleport] = new DroneTeleport();
            UpgradeHandlers[Upgrades.JumpBoost] = new JumpBoost();
            UpgradeHandlers[Upgrades.Bombs] = new Bombs();
            UpgradeHandlers[Upgrades.MegaBombs] = new MegaBombs();
            UpgradeHandlers[Upgrades.RemoteDrone] = new RemoteDrone();
            UpgradeHandlers[Upgrades.GoldenFeather] = new GoldenFeather();
            UpgradeHandlers[Upgrades.Binoculars] = new Binoculars();
            UpgradeHandlers[Upgrades.EtherealDash] = new EtherealDash();
            UpgradeHandlers[Upgrades.PortableStation] = new PortableStation();
            UpgradeHandlers[Upgrades.PulseRadar] = new PulseRadar();
            UpgradeHandlers[Upgrades.DashBoots] = new DashBoots();
            UpgradeHandlers[Upgrades.HoverJet] = new HoverJet();
            UpgradeHandlers[Upgrades.LightningDash] = new LightningDash();
            UpgradeHandlers[Upgrades.MissilesModule] = new MissilesModule();
            UpgradeHandlers[Upgrades.SuperMissilesModule] = new SuperMissilesModule();

            //Metroid Upgrades

            UpgradeHandlers[Upgrades.Spazer] = new Spazer();
            UpgradeHandlers[Upgrades.PlasmaBeam] = new PlasmaBeam();
            UpgradeHandlers[Upgrades.MorphingBall] = new MorphingBall();
            UpgradeHandlers[Upgrades.MorphBombs] = new MorphBombs();
            UpgradeHandlers[Upgrades.SpringBall] = new SpringBall();
            UpgradeHandlers[Upgrades.HighJumpBoots] = new HighJumpBoots();
            UpgradeHandlers[Upgrades.SpeedBooster] = new SpeedBooster();

            // Common Upgrades

            UpgradeHandlers[Upgrades.LongBeam] = new LongBeam();
            UpgradeHandlers[Upgrades.IceBeam] = new IceBeam();
            UpgradeHandlers[Upgrades.WaveBeam] = new WaveBeam();
            UpgradeHandlers[Upgrades.VariaJacket] = new VariaJacket();
            UpgradeHandlers[Upgrades.GravityJacket] = new GravityJacket();
            UpgradeHandlers[Upgrades.ScrewAttack] = new ScrewAttack();
            UpgradeHandlers[Upgrades.SpaceJump] = new SpaceJump();
        }

        // Set up any hooks, event handlers and your mod in general here.
        // Load runs before Celeste itself has initialized properly.
        public override void Load()
        {
            DecalRegistry.AddPropertyHandler("XaphanHelper_BGdepth", delegate (Decal decal, XmlAttributeCollection attrs)
            {
                if (attrs["value"] != null && decal.Depth == 9000)
                {
                    decal.Depth = int.Parse(attrs["value"].Value);
                }
            });
            DecalRegistry.AddPropertyHandler("XaphanHelper_flagsHide", (Decal decal, XmlAttributeCollection attrs) =>
            {
                if (attrs["flags"] != null && (attrs["room"] == null || decal.SceneAs<Level>().Session.Level == attrs["room"].Value))
                {
                    decal.Add(new FlagDecalVisibilityToggle(attrs["flags"].Value.Split(','), attrs["inverted"] != null && bool.Parse(attrs["inverted"].Value)));
                }
            });
            DecalRegistry.AddPropertyHandler("XaphanHelper_flagSwap", delegate (Decal decal, XmlAttributeCollection attrs)
            {
                if (attrs["flag"] != null && attrs["offPath"] != null && attrs["onPath"] != null)
                {
                    if (attrs["room"] == null || decal.SceneAs<Level>().Session.Level == attrs["room"].Value)
                    {
                        decal.MakeFlagSwap(attrs["flag"].Value, attrs["offPath"].Value, attrs["onPath"].Value);
                        if (decal.SceneAs<Level>().Session.GetFlag(attrs["flag"].Value))
                        {
                            float X = attrs["offsetX"] != null ? float.Parse(attrs["offsetX"].Value) : 0f;
                            float Y = attrs["offsetY"] != null ? float.Parse(attrs["offsetY"].Value) : 0f;
                            decal.Position += new Vector2(decal.Scale.X == 1 ? X : -X, decal.Scale.Y == 1 ? Y : -Y);
                        }
                    }
                }
            });
            DecalRegistry.AddPropertyHandler("XaphanHelper_randomFlagSwap", (Decal decal, XmlAttributeCollection attrs) =>
            {
                if (attrs["flag"] != null && attrs["offPath"] != null && attrs["onPath"] != null)
                {
                    List<MTexture> offTextures = attrs["offPath"].Value.Split(',').Select(path => GFX.Game[path]).ToList();
                    List<MTexture> onTextures = attrs["onPath"].Value.Split(',').Select(path => GFX.Game[path]).ToList();
                    decal.Add(decal.Image = new RandomFlagSwapImage(attrs["flag"].Value, offTextures, onTextures));
                }
            });
            DecalRegistry.AddPropertyHandler("XaphanHelper_flagLight", delegate (Decal decal, XmlAttributeCollection attrs)
            {
                if (attrs["flag"] != null)
                {
                    bool inverted = attrs["inverted"] != null ? bool.Parse(attrs["inverted"].Value) : false;
                    float X = attrs["offsetX"] != null ? float.Parse(attrs["offsetX"].Value) : 0f;
                    float Y = attrs["offsetY"] != null ? float.Parse(attrs["offsetY"].Value) : 0f;
                    Color color = attrs["color"] != null ? Calc.HexToColor(attrs["color"].Value) : Color.White;
                    float alpha = attrs["alpha"] != null ? float.Parse(attrs["alpha"].Value) : 1f;
                    int startFade = attrs["startFade"] != null ? int.Parse(attrs["startFade"].Value) : 16;
                    int endFade = attrs["endFade"] != null ? int.Parse(attrs["endFade"].Value) : 24;
                    decal.Add(new FlagDecalLightToogle(attrs["flag"].Value, inverted, new Vector2(X, Y), color, alpha, startFade, endFade));
                }
            });
            DecalRegistry.AddPropertyHandler("XaphanHelper_flagLightOcclude", delegate (Decal decal, XmlAttributeCollection attrs)
            {
                if (attrs["flag"] != null)
                {
                    bool inverted = attrs["inverted"] != null ? bool.Parse(attrs["inverted"].Value) : false;
                    if (inverted ? !decal.SceneAs<Level>().Session.GetFlag(attrs["flag"].Value) : decal.SceneAs<Level>().Session.GetFlag(attrs["flag"].Value))
                    {
                        int X = attrs["x"] != null ? int.Parse(attrs["x"].Value) : 0;
                        int Y = attrs["y"] != null ? int.Parse(attrs["y"].Value) : 0;
                        int width = attrs["width"] != null ? int.Parse(attrs["width"].Value) : 16;
                        int height = attrs["height"] != null ? int.Parse(attrs["height"].Value) : 16;
                        float alpha = attrs["alpha"] != null ? float.Parse(attrs["alpha"].Value) : 1f;
                        decal.Add(new LightOcclude(new Rectangle(X, Y, width, height), alpha));
                    }
                }
            });
            DecalRegistry.AddPropertyHandler("XaphanHelper_flagRandomFlip", delegate (Decal decal, XmlAttributeCollection attrs)
            {
                if (attrs["flag"] != null)
                {
                    bool inverted = attrs["inverted"] != null ? bool.Parse(attrs["inverted"].Value) : false;
                    bool flipX = attrs["flipX"] != null ? bool.Parse(attrs["flipX"].Value) : false;
                    bool flipY = attrs["flipY"] != null ? bool.Parse(attrs["flipY"].Value) : false;
                    if (inverted ? !decal.SceneAs<Level>().Session.GetFlag(attrs["flag"].Value) : decal.SceneAs<Level>().Session.GetFlag(attrs["flag"].Value))
                    {
                        decal.Scale *= new Vector2(flipX ? (Calc.Random.Next(1, 3) == 2 ? -1 : 1) : 1, flipY ? (Calc.Random.Next(1, 3) == 2 ? -1 : 1) : 1);
                    }
                }
            });
            DecalRegistry.AddPropertyHandler("XaphanHelper_flagSparks", delegate (Decal decal, XmlAttributeCollection attrs)
            {
                if (attrs["flagTrue"] != null)
                {
                    float X = attrs["offsetX"] != null ? float.Parse(attrs["offsetX"].Value) : 0f;
                    float Y = attrs["offsetY"] != null ? float.Parse(attrs["offsetY"].Value) : 0f;
                    if (decal.SceneAs<Level>().Session.GetFlag(attrs["flagTrue"].Value) && (attrs["flagFalse"] != null ? !decal.SceneAs<Level>().Session.GetFlag(attrs["flagFalse"].Value) : true))
                    {
                        decal.SceneAs<Level>().Add(new SparkGenerator(decal.Position + new Vector2(decal.Scale.X < 0 ? X : -X, Y)));
                    }
                }
            });
            DecalRegistry.AddPropertyHandler("XaphanHelper_flame", delegate (Decal decal, XmlAttributeCollection attrs)
            {
                float X = attrs["offsetX"] != null ? float.Parse(attrs["offsetX"].Value) : 0f;
                float Y = attrs["offsetY"] != null ? float.Parse(attrs["offsetY"].Value) : 0f;
                decal.SceneAs<Level>().Add(new CustomTorch(decal.Position - new Vector2(8f), new Vector2(decal.Scale.X < 0 ? X : -X, Y), "FFA500", false, true, "objects/Xaphan/CustomTorch/flame", "", 1, 48, 64, "event:/game/05_mirror_temple/torch_activate", false));
            });
            foreach (Upgrades upgrade in UpgradeHandlers.Keys)
            {
                UpgradeHandlers[upgrade].Load();
            }
            Everest.Events.Level.OnLoadBackdrop += OnLoadBackdrop;
            Everest.Events.Level.OnLoadLevel += onLevelLoad;
            Everest.Events.Level.OnExit += onLevelExit;
            //Everest.Events.Level.OnCreatePauseMenuButtons += onCreatePauseMenuButtons;
            IL.Celeste.Player.Render += modILPlayerRender;
            On.Celeste.AreaData.HasMode += monAreaDataHasMode;
            On.Celeste.Cassette.UnlockedBSide.EaseIn += modCassetteUnlockedBSideEaseIn;
            On.Celeste.Cassette.UnlockedBSide.EaseOut += modCassetteUnlockedBSideEaseOut;
            On.Celeste.Cassette.UnlockedBSide.Render += modCassetteUnlockedBSideRender;
            On.Celeste.GameplayStats.Render += onGameplayStatsRender;
            On.Celeste.Holdable.Release += onHoldableRelease;
            On.Celeste.LevelEnter.Routine += modLevelEnterRoutine;
            On.Celeste.LevelEnter.BeforeRender += modLevelEnterBeforeRender;
            On.Celeste.LevelEnter.Go += onLevelEnterGo;
            On.Celeste.Level.EndPauseEffects += onLevelEndPauseEffects;
            On.Celeste.Level.Pause += onLevelPause;
            On.Celeste.Level.Update += onLevelUpdate;
            On.Celeste.OuiChapterSelect.GetMinMaxArea += modOuiChapterSelectGetMinMaxArea;
            On.Celeste.OuiChapterSelect.Update += modOuiChapterSelectUpdate;
            On.Celeste.OuiChapterSelectIcon.Update += modOuiChapterSelectIconUpdate;
            On.Celeste.OuiChapterPanel.Enter += modOuiChapterPanelEnter;
            On.Celeste.OuiChapterPanel.Leave += modOuiChapterPanelLeave;
            On.Celeste.OuiChapterPanel.GetModeHeight += modOuiChapterPanelGetModeHeight;
            On.Celeste.OuiChapterPanel.IncrementStats += modOuiChapterPanelIncrementStats;
            On.Celeste.OuiChapterPanel.IncrementStatsDisplay += modOuiChapterPanelIncrementStatsDisplay;
            On.Celeste.OuiChapterPanel.IsStart += modOuiChapterPanelIsStart;
            On.Celeste.OuiChapterPanel.UpdateStats += modOuiChapterPanelUpdateStats;
            On.Celeste.OuiChapterPanel.Reset += modOuiChapterPanelReset;
            On.Celeste.OuiChapterPanel.Start += modOuiChapterPanelStart;
            On.Celeste.OuiChapterPanel.StartRoutine += modOuiChapterPanelStartRoutine;
            On.Celeste.OuiFileSelectSlot.ctor_int_OuiFileSelect_SaveData += modOuiFileSelectSlotCtor_SaveData;
            On.Celeste.OuiFileSelectSlot.Show += modOuiFileSelectShow;
            On.Celeste.OuiFileSelectSlot.Render += modOuiFileSelectSlotRender;
            On.Celeste.OuiJournalProgress.ctor += modOuiJournalProressCtor;
            On.Celeste.OuiJournal.Enter += modOuiJournalEnter;
            On.Celeste.Overworld.SetNormalMusic += modOverworldSetNormalMusic;
            On.Celeste.Player.ctor += modPlayerCtor;
            On.Celeste.Player.CallDashEvents += modPlayerCallDashEvents;
            On.Celeste.Player.Render += onPlayerRender;
            On.Celeste.PlayerDeadBody.Render += onPlayerDeadBodyRender;
            On.Celeste.PlayerSprite.CreateFramesMetadata += PlayerSpriteMetadataHook;
            On.Celeste.ReturnMapHint.Render += onReturnMapHintRender;
            On.Celeste.SaveData.FoundAnyCheckpoints += modSaveDataFoundAnyCheckpoints;
            On.Celeste.SaveLoadIcon.Render += onSaveLoadIconRender;
            On.Celeste.SaveLoadIcon.Routine += onSaveLoadIconRoutine;
            On.Celeste.Session.Restart += onSessionRestart;
            On.Celeste.SpeedrunTimerDisplay.DrawTime += onSpeedrunTimerDisplayDrawTime;
            On.Celeste.Strawberry.OnPlayer += modStrawberryOnPlayer;
            On.Celeste.Strawberry.OnLoseLeader += modStrawberryOnLoseLeader;
            On.Celeste.Strawberry.CollectRoutine += onStrawberryCollectRoutine;
            On.Celeste.Mod.UI.OuiMapList.Enter += modOuiMapListEnter;
            On.Celeste.Mod.UI.OuiMapList.Inspect += modOuiMapListInspect;
            On.Celeste.Mod.UI.OuiMapList.Leave += modOuiMapListLeave;
            On.Celeste.Mod.UI.OuiMapSearch.Enter += modOuiMapSeatchEnter;
            On.Celeste.Mod.UI.OuiMapSearch.Inspect += modOuiMapSearchInspect;
            On.Celeste.Mod.UI.OuiMapSearch.Leave += modOuiMapSeatchLeave;
            SaveUpdater.Load();
            MetroidGameplayController.Load();
            ScrewAttackManager.Load();
            MapDisplay.Load();
            PlayerPlatform.Load();
            Slope.Load();
            CameraBlocker.Load();
            HeatController.Load();
            JumpBlock.Load();
            TimeManager.Load();
            TimedStrawberry.Load();
            CountdownDisplay.Load();
            Liquid.Load();
            MagneticCeiling.Load();
            Drone.Load();
            FlagDashSwitch.Load();
            TimedDashSwitch.Load();
            BagDisplay.Load();
            StatsFlags.Load();
            CustomEndScreenController.Load();
            Binocular.Load();
            EtherealBlock.Load();
            TilesetsSwap.Load();
            SpikesTextureSwap.Load();
            UpgradesDisplay.Load();
            LaserDetectorManager.Load();
            PushBlock.Load();
            FakePlayer.Load();
            PlayerDeadAction.Load();
            DroneSwitch.Load();
            TransitionBlackEffect.Load();
            WorkRobot.Load();
            BreakBlock.Load();
            CustomRefill.Load();
            MergedChaptersGoldenStrawberry.Load();
            Arrow.Load();
            BigScreen.Load();
            BombSwitch.Load();
            MergeChaptersBCSideHeartCompleteArea.Load();
            Conveyor.Load();
            SolidMovingPlatform.Load();
            AuxiliaryGenerator.Load();
            Detonator.Load();
            Lever.Load();
            LightManager.Load();
            ClimbableVine.Load();
            ExplosiveBoulder.Load();
            BreathDisplay.Load();
            BreathManager.Load();
            Skultera.Load();
            DebugBlocker.Load();
            CustomPufferSpringCollider.Load();
            Bomb.Load();
        }

        // Optional, do anything requiring either the Celeste or mod content here.
        public override void LoadContent(bool firstLoad)
        {
            SpriteBank = new SpriteBank(GFX.Game, "Graphics/Xaphan/CustomSprites.xml");
        }

        // Unload the entirety of your mod's content. Free up any native resources.
        public override void Unload()
        {
            foreach (Upgrades upgrade in UpgradeHandlers.Keys)
            {
                UpgradeHandlers[upgrade].Unload();
            }
            Everest.Events.Level.OnLoadBackdrop -= OnLoadBackdrop;
            Everest.Events.Level.OnLoadLevel -= onLevelLoad;
            Everest.Events.Level.OnExit -= onLevelExit;
            //Everest.Events.Level.OnCreatePauseMenuButtons -= onCreatePauseMenuButtons;
            IL.Celeste.Player.Render -= modILPlayerRender;
            On.Celeste.AreaData.HasMode -= monAreaDataHasMode;
            On.Celeste.Cassette.UnlockedBSide.EaseIn -= modCassetteUnlockedBSideEaseIn;
            On.Celeste.Cassette.UnlockedBSide.EaseOut -= modCassetteUnlockedBSideEaseOut;
            On.Celeste.Cassette.UnlockedBSide.Render -= modCassetteUnlockedBSideRender;
            On.Celeste.GameplayStats.Render -= onGameplayStatsRender;
            On.Celeste.Holdable.Release -= onHoldableRelease;
            On.Celeste.LevelEnter.Routine -= modLevelEnterRoutine;
            On.Celeste.LevelEnter.BeforeRender -= modLevelEnterBeforeRender;
            On.Celeste.LevelEnter.Go -= onLevelEnterGo;
            On.Celeste.Level.EndPauseEffects -= onLevelEndPauseEffects;
            On.Celeste.Level.Pause -= onLevelPause;
            On.Celeste.Level.Update -= onLevelUpdate;
            On.Celeste.OuiChapterSelect.GetMinMaxArea -= modOuiChapterSelectGetMinMaxArea;
            On.Celeste.OuiChapterSelect.Update -= modOuiChapterSelectUpdate;
            On.Celeste.OuiChapterSelectIcon.Update -= modOuiChapterSelectIconUpdate;
            On.Celeste.OuiChapterPanel.GetModeHeight -= modOuiChapterPanelGetModeHeight;
            On.Celeste.OuiChapterPanel.IncrementStats -= modOuiChapterPanelIncrementStats;
            On.Celeste.OuiChapterPanel.IncrementStatsDisplay -= modOuiChapterPanelIncrementStatsDisplay;
            On.Celeste.OuiChapterPanel.IsStart -= modOuiChapterPanelIsStart;
            On.Celeste.OuiChapterPanel.UpdateStats -= modOuiChapterPanelUpdateStats;
            On.Celeste.OuiChapterPanel.Reset -= modOuiChapterPanelReset;
            On.Celeste.OuiChapterPanel.Start -= modOuiChapterPanelStart;
            On.Celeste.OuiChapterPanel.StartRoutine -= modOuiChapterPanelStartRoutine;
            On.Celeste.OuiFileSelectSlot.ctor_int_OuiFileSelect_SaveData -= modOuiFileSelectSlotCtor_SaveData;
            On.Celeste.OuiFileSelectSlot.Show -= modOuiFileSelectShow;
            On.Celeste.OuiFileSelectSlot.Render -= modOuiFileSelectSlotRender;
            On.Celeste.OuiJournalProgress.ctor -= modOuiJournalProressCtor;
            On.Celeste.OuiJournal.Enter -= modOuiJournalEnter;
            On.Celeste.Overworld.SetNormalMusic -= modOverworldSetNormalMusic;
            On.Celeste.Player.ctor -= modPlayerCtor;
            On.Celeste.Player.CallDashEvents -= modPlayerCallDashEvents;
            On.Celeste.Player.Render -= onPlayerRender;
            On.Celeste.PlayerDeadBody.Render -= onPlayerDeadBodyRender;
            On.Celeste.PlayerSprite.CreateFramesMetadata -= PlayerSpriteMetadataHook;
            On.Celeste.ReturnMapHint.Render -= onReturnMapHintRender;
            On.Celeste.SaveData.FoundAnyCheckpoints -= modSaveDataFoundAnyCheckpoints;
            On.Celeste.SaveLoadIcon.Render -= onSaveLoadIconRender;
            On.Celeste.SaveLoadIcon.Routine -= onSaveLoadIconRoutine;
            On.Celeste.Session.Restart -= onSessionRestart;
            On.Celeste.SpeedrunTimerDisplay.DrawTime -= onSpeedrunTimerDisplayDrawTime;
            On.Celeste.Strawberry.OnPlayer -= modStrawberryOnPlayer;
            On.Celeste.Strawberry.OnLoseLeader -= modStrawberryOnLoseLeader;
            On.Celeste.Strawberry.CollectRoutine -= onStrawberryCollectRoutine;
            On.Celeste.Mod.UI.OuiMapList.Enter -= modOuiMapListEnter;
            On.Celeste.Mod.UI.OuiMapList.Inspect -= modOuiMapListInspect;
            On.Celeste.Mod.UI.OuiMapList.Leave -= modOuiMapListLeave;
            On.Celeste.Mod.UI.OuiMapSearch.Enter -= modOuiMapSeatchEnter;
            On.Celeste.Mod.UI.OuiMapSearch.Inspect -= modOuiMapSearchInspect;
            On.Celeste.Mod.UI.OuiMapSearch.Leave -= modOuiMapSeatchLeave;
            SaveUpdater.Unload();
            MetroidGameplayController.Unload();
            ScrewAttackManager.Unload();
            MapDisplay.Unload();
            PlayerPlatform.Unload();
            Slope.Unload();
            CameraBlocker.Unload();
            HeatController.Unload();
            JumpBlock.Unload();
            TimeManager.Unload();
            TimedStrawberry.Unload();
            CountdownDisplay.Unload();
            Liquid.Unload();
            MagneticCeiling.Unload();
            Drone.Unload();
            FlagDashSwitch.Unload();
            TimedDashSwitch.Unload();
            BagDisplay.Unload();
            StatsFlags.Unload();
            CustomEndScreenController.Unload();
            Binocular.Unload();
            EtherealBlock.Unload();
            TilesetsSwap.Unload();
            SpikesTextureSwap.Unload();
            UpgradesDisplay.Unload();
            LaserDetectorManager.Unload();
            PushBlock.Unload();
            FakePlayer.Unload();
            PlayerDeadAction.Unload();
            DroneSwitch.Unload();
            TransitionBlackEffect.Unload();
            WorkRobot.Unload();
            BreakBlock.Unload();
            CustomRefill.Unload();
            MergedChaptersGoldenStrawberry.Unload();
            Arrow.Unload();
            BigScreen.Unload();
            BombSwitch.Unload();
            MergeChaptersBCSideHeartCompleteArea.Unload();
            Conveyor.Unload();
            SolidMovingPlatform.Unload();
            AuxiliaryGenerator.Unload();
            Detonator.Unload();
            Lever.Unload();
            LightManager.Unload();
            ClimbableVine.Unload();
            ExplosiveBoulder.Unload();
            BreathDisplay.Unload();
            BreathManager.Unload();
            Skultera.Unload();
            DebugBlocker.Unload();
            CustomPufferSpringCollider.Unload();
            Bomb.Unload();
        }

        private void onHoldableRelease(On.Celeste.Holdable.orig_Release orig, Holdable self, Vector2 force)
        {
            if (self.Entity == null)
            {
                return;
            }
            orig(self, force);
        }


        // Custom States

        public static int StFastFall;

        private void modPlayerCtor(On.Celeste.Player.orig_ctor orig, Player self, Vector2 position, PlayerSpriteMode spriteMode)
        {
            orig.Invoke(self, position, spriteMode);
            StFastFall = StateMachineExt.AddState(self.StateMachine, FastFallUpdate, FastLabFallCoroutine);
        }

        private int FastFallUpdate()
        {
            if (Engine.Scene is Level)
            {
                Player player = ((Level)Engine.Scene).Tracker.GetEntity<Player>();
                player.Facing = Facings.Right;
                if (!player.OnGround() && player.DummyGravity)
                {
                    player.Speed.Y = 320f;
                }
            }
            return StFastFall;
        }

        private IEnumerator FastLabFallCoroutine()
        {
            if (Engine.Scene is Level)
            {
                Level level = (Level)Engine.Scene;
                Player player = level.Tracker.GetEntity<Player>();
                player.Sprite.Play("fallFast");
                while (!player.OnGround())
                {
                    yield return null;
                }
                player.Play("event:/char/madeline/mirrortemple_big_landing");
                if (player.Dashes <= 1)
                {
                    player.Sprite.Play("fallPose");
                }
                else
                {
                    player.Sprite.Play("idle");
                }
                player.Sprite.Scale.Y = 0.7f;
                Input.Rumble(RumbleStrength.Strong, RumbleLength.Medium);
                level.DirectionalShake(new Vector2(0f, 1f), 0.5f);
                player.Speed.X = 0f;
                level.Particles.Emit(Player.P_SummitLandA, 12, player.BottomCenter, Vector2.UnitX * 3f, -(float)Math.PI / 2f);
                level.Particles.Emit(Player.P_SummitLandB, 8, player.BottomCenter - Vector2.UnitX * 2f, Vector2.UnitX * 2f, 3.403392f);
                level.Particles.Emit(Player.P_SummitLandB, 8, player.BottomCenter + Vector2.UnitX * 2f, Vector2.UnitX * 2f, -(float)Math.PI / 12f);
                for (float p = 0f; p < 1f; p += Engine.DeltaTime)
                {
                    yield return null;
                }
                player.StateMachine.State = 0;
            }
        }

        private void onGameplayStatsRender(On.Celeste.GameplayStats.orig_Render orig, GameplayStats self)
        {
            AreaKey area = self.SceneAs<Level>().Session.Area;
            MapData MapData = AreaData.Areas[area.ID].Mode[(int)area.Mode].MapData;
            if (!useIngameMap || (useIngameMap && inGameMapProgressDisplayMode == "Never"))
            {
                orig(self);
            }
        }

        // Custom Chapter Pannel

        private void modOuiChapterSelectIconUpdate(On.Celeste.OuiChapterSelectIcon.orig_Update orig, OuiChapterSelectIcon self)
        {
            if (useMergeChaptersController && SaveData.Instance != null)
            {
                orig(self);
                if (!MergeChaptersControllerKeepPrologue || (MergeChaptersControllerKeepPrologue && SaveData.Instance.LastArea_Safe.ID != SaveData.Instance.LevelSetStats.AreaOffset))
                {
                    if (SaveData.Instance == null)
                    {
                        return;
                    }
                    self.sizeEase = Calc.Approach(self.sizeEase, 1f, Engine.DeltaTime * 4f);
                    if (SaveData.Instance.LastArea_Safe.ID == self.Area)
                    {
                        self.Depth = -50;
                    }
                    else
                    {
                        self.Depth = -45;
                    }
                    if (OuiChapterSelectIcon_tween.GetValue(self) == null)
                    {
                        if (self.IsSelected)
                        {
                            CustomOuiChapterPanel uI = (self.Scene as Overworld).GetUI<CustomOuiChapterPanel>();
                            if (uI != null)
                            {
                                self.Position = ((!uI.EnteringChapter) ? uI.OpenPosition : uI.Position) + uI.IconOffset;
                            }
                        }
                        else if (!self.IsHidden)
                        {
                            self.Position = Calc.Approach(self.Position, self.IdlePosition, 2400f * Engine.DeltaTime);
                        }
                    }
                    if (self.Area > SaveData.Instance.LevelSetStats.AreaOffset + (MergeChaptersControllerKeepPrologue ? 1 : 0) && self.Area <= (SaveData.Instance.LevelSetStats.AreaOffset + SaveData.Instance.LevelSetStats.MaxArea))
                    {
                        if (MergeChaptersControllerKeepPrologue && self.Area > SaveData.Instance.LevelSetStats.AreaOffset)
                        {
                            OuiChapterSelectIcon_front.SetValue(self, GFX.Gui[AreaData.Areas[SaveData.Instance.LevelSetStats.AreaOffset + 1].Icon]);
                            OuiChapterSelectIcon_back.SetValue(self, GFX.Gui.Has(AreaData.Areas[SaveData.Instance.LevelSetStats.AreaOffset + 1].Icon + "_back") ? GFX.Gui[AreaData.Areas[SaveData.Instance.LevelSetStats.AreaOffset + 1].Icon + "_back"] : GFX.Gui[AreaData.Areas[SaveData.Instance.LevelSetStats.AreaOffset + 1].Icon]);
                        }
                        else
                        {
                            OuiChapterSelectIcon_front.SetValue(self, GFX.Gui[AreaData.Areas[SaveData.Instance.LevelSetStats.AreaOffset].Icon]);
                            OuiChapterSelectIcon_back.SetValue(self, GFX.Gui.Has(AreaData.Areas[SaveData.Instance.LevelSetStats.AreaOffset].Icon + "_back") ? GFX.Gui[AreaData.Areas[SaveData.Instance.LevelSetStats.AreaOffset].Icon + "_back"] : GFX.Gui[AreaData.Areas[SaveData.Instance.LevelSetStats.AreaOffset].Icon]);
                        }
                    }
                }
            }
            else
            {
                orig(self);
            }
        }

        private void modOuiChapterSelectGetMinMaxArea(On.Celeste.OuiChapterSelect.orig_GetMinMaxArea orig, OuiChapterSelect self, out int areaOffs, out int areaMax)
        {
            foreach (OuiChapterSelectIcon icon in (List<OuiChapterSelectIcon>)OuiChapterSelect_icons.GetValue(self))
            {
                icon.Visible = true;
            }
            if (useMergeChaptersController)
            {
                if (MergeChaptersControllerKeepPrologue)
                {
                    areaOffs = SaveData.Instance.LevelSetStats.AreaOffset;
                    int areaOffsRaw = SaveData.Instance.LevelSetStats.AreaOffset;
                    int areaMaxRaw = Math.Max(areaOffsRaw, SaveData.Instance.UnlockedAreas_Safe);
                    do
                    {
                        areaMax = ((List<OuiChapterSelectIcon>)OuiChapterSelect_icons.GetValue(self)).FindLastIndex((OuiChapterSelectIcon i) => (i != null && i.Area == areaMaxRaw) || i.AssistModeUnlockable);
                    }
                    while (areaMax == -1 && --areaMaxRaw < areaOffsRaw);
                    if (areaMax == -1)
                    {
                        areaMax = areaMaxRaw;
                    }
                    if (areaMax > areaOffs + 1)
                    {
                        areaMax = areaOffs + 1;
                    }
                }
                else
                {
                    areaMax = areaOffs = SaveData.Instance.LevelSetStats.AreaOffset;
                }
                foreach (OuiChapterSelectIcon icon in (List<OuiChapterSelectIcon>)OuiChapterSelect_icons.GetValue(self))
                {
                    if (icon.Area > areaMax)
                    {
                        icon.Hide();
                        icon.Visible = false;
                    }
                }
            }
            else
            {
                orig(self, out areaOffs, out areaMax);
            }
        }

        private IEnumerator modOuiChapterPanelIncrementStats(On.Celeste.OuiChapterPanel.orig_IncrementStats orig, OuiChapterPanel self, bool shouldAdvance)
        {
            if (useMergeChaptersController && (MergeChaptersControllerKeepPrologue ? SaveData.Instance.LastArea_Safe.ID != SaveData.Instance.LevelSetStats.AreaOffset : true))
            {
                shouldAdvance = false;
            }
            yield return new SwapImmediately(orig(self, shouldAdvance));
        }

        private void modOuiMapSearchInspect(On.Celeste.Mod.UI.OuiMapSearch.orig_Inspect orig, UI.OuiMapSearch self, AreaData area, AreaMode mode)
        {
            MergeChaptersControllerCheck(area);
            if (useMergeChaptersController)
            {
                self.Focused = false;
                Audio.Play("event:/ui/world_map/icon/select");
                SaveData.Instance.LastArea_Safe = area.ToKey(mode);
                if (self.OuiIcons != null && area.ID < self.OuiIcons.Count)
                {
                    self.OuiIcons[area.ID > SaveData.Instance.LevelSetStats.AreaOffset + 1 ? SaveData.Instance.LevelSetStats.AreaOffset + 1 : area.ID].Select();
                }
                self.Overworld.Mountain.Model.EaseState(area.MountainState);
                MergeChaptersControllerCheck(area);
                if (MergeChaptersControllerKeepPrologue && area.ID == SaveData.Instance.LevelSetStats.AreaOffset)
                {
                    SaveData.Instance.LastArea_Safe.ID = SaveData.Instance.LevelSetStats.AreaOffset;
                    self.Overworld.Goto<OuiChapterPanel>();
                }
                else
                {
                    self.Overworld.Goto<CustomOuiChapterPanel>();
                }
            }
            else
            {
                orig(self, area, mode);
            }
        }

        private void modOuiMapListInspect(On.Celeste.Mod.UI.OuiMapList.orig_Inspect orig, UI.OuiMapList self, AreaData area, AreaMode mode)
        {
            MergeChaptersControllerCheck(area);
            if (useMergeChaptersController)
            {
                self.Focused = false;
                Audio.Play("event:/ui/world_map/icon/select");
                SaveData.Instance.LastArea_Safe = area.ToKey(mode);
                if (self.OuiIcons != null && area.ID < self.OuiIcons.Count)
                {
                    self.OuiIcons[area.ID > SaveData.Instance.LevelSetStats.AreaOffset + 1 ? SaveData.Instance.LevelSetStats.AreaOffset + 1 : area.ID].Select();
                }
                self.Overworld.Mountain.Model.EaseState(area.MountainState);
                MergeChaptersControllerCheck(area);
                if (MergeChaptersControllerKeepPrologue && area.ID == SaveData.Instance.LevelSetStats.AreaOffset)
                {
                    SaveData.Instance.LastArea_Safe.ID = SaveData.Instance.LevelSetStats.AreaOffset;
                    self.Overworld.Goto<OuiChapterPanel>();
                }
                else
                {
                    self.Overworld.Goto<CustomOuiChapterPanel>();
                }
            }
            else
            {
                orig(self, area, mode);
            }
        }

        private IEnumerator modOuiChapterPanelLeave(On.Celeste.OuiChapterPanel.orig_Leave orig, OuiChapterPanel self, Oui next)
        {
            if (!useMergeChaptersController || (useMergeChaptersController && MergeChaptersControllerKeepPrologue && SaveData.Instance.LastArea_Safe.ID == SaveData.Instance.LevelSetStats.AreaOffset))
            {
                yield return new SwapImmediately(orig(self, next));
            }
        }

        private IEnumerator modOuiChapterPanelEnter(On.Celeste.OuiChapterPanel.orig_Enter orig, OuiChapterPanel self, Oui from)
        {
            if (useMergeChaptersController)
            {
                if (MergeChaptersControllerKeepPrologue && SaveData.Instance.LastArea_Safe.ID == SaveData.Instance.LevelSetStats.AreaOffset)
                {
                    yield return new SwapImmediately(orig(self, from));
                }
                else
                {
                    self.Overworld.Goto<CustomOuiChapterPanel>();
                    self.Visible = false;
                    yield break;
                }
            }
            else
            {
                yield return new SwapImmediately(orig(self, from));
            }
        }

        private IEnumerator modOuiMapListEnter(On.Celeste.Mod.UI.OuiMapList.orig_Enter orig, OuiMapList self, Oui from)
        {
            onMapListOrSearch = true;
            return orig(self, from);
        }

        private IEnumerator modOuiMapListLeave(On.Celeste.Mod.UI.OuiMapList.orig_Leave orig, OuiMapList self, Oui next)
        {
            onMapListOrSearch = false;
            return orig(self, next);
        }

        private IEnumerator modOuiMapSeatchEnter(On.Celeste.Mod.UI.OuiMapSearch.orig_Enter orig, OuiMapSearch self, Oui from)
        {
            onMapListOrSearch = true;
            return orig(self, from);
        }

        private IEnumerator modOuiMapSeatchLeave(On.Celeste.Mod.UI.OuiMapSearch.orig_Leave orig, OuiMapSearch self, Oui next)
        {
            onMapListOrSearch = false;
            return orig(self, next);
        }

        private bool monAreaDataHasMode(On.Celeste.AreaData.orig_HasMode orig, AreaData self, AreaMode mode)
        {
            if (onMapListOrSearch && self.Name.Contains("Xaphan/0") && !self.Name.Contains("Prologue"))
            {
                return false;
            }
            return orig(self, mode);
        }

        private void modOverworldSetNormalMusic(On.Celeste.Overworld.orig_SetNormalMusic orig, Overworld self)
        {
            if (!useMergeChaptersController || SaveData.Instance == null || self.IsCurrent<OuiMainMenu>())
            {
                orig(self);
            }
            else
            {
                AreaData areaData = AreaData.Get(SaveData.Instance.LastArea_Safe);
                MapMetaMountain mapMetaMountain = areaData?.Meta?.Mountain;
                Audio.SetMusic(mapMetaMountain?.BackgroundMusic ?? "event:/music/menu/level_select");
                Audio.SetAmbience(mapMetaMountain?.BackgroundAmbience ?? "event:/env/amb/worldmap");
                foreach (KeyValuePair<string, float> item in mapMetaMountain?.BackgroundMusicParams ?? new Dictionary<string, float>())
                {
                    Audio.SetMusicParam(item.Key, item.Value);
                }
            }
        }

        private IEnumerator modCassetteUnlockedBSideEaseIn(On.Celeste.Cassette.UnlockedBSide.orig_EaseIn orig, Entity self)
        {
            if (startedAnySoCMChapter)
            {
                while ((cassetteAlpha += Engine.DeltaTime / 0.5f) < 1f)
                {
                    yield return null;
                }
                cassetteAlpha = 1f;
                yield return 1.5f;
                cassetteWaitForKeyPress = true;
            }
            else
            {
                yield return new SwapImmediately(orig(self));
            }
        }

        private IEnumerator modCassetteUnlockedBSideEaseOut(On.Celeste.Cassette.UnlockedBSide.orig_EaseOut orig, Entity self)
        {
            if (startedAnySoCMChapter)
            {
                cassetteWaitForKeyPress = false;
                while ((cassetteAlpha -= Engine.DeltaTime / 0.5f) > 0f)
                {
                    yield return null;
                }
                cassetteAlpha = 0f;
                cassetteTimer = 0f;
                self.RemoveSelf();
            }
            else
            {
                yield return new SwapImmediately(orig(self));
            }
        }

        private void modCassetteUnlockedBSideRender(On.Celeste.Cassette.UnlockedBSide.orig_Render orig, Entity self)
        {
            if (startedAnySoCMChapter)
            {
                cassetteTimer += Engine.DeltaTime;
                float num = Ease.CubeOut(cassetteAlpha);
                string text = Dialog.Clean("Xaphan_0_PortalAppeared");
                Vector2 position = Celeste.TargetCenter + new Vector2(0f, 64f);
                Vector2 adjust = Vector2.UnitY * 64f * (1f - num);
                Draw.Rect(-10f, -10f, 1940f, 1100f, Color.Black * num * 0.8f);
                GFX.Gui["collectables/cassette"].DrawJustified(position - adjust + new Vector2(0f, 32f), new Vector2(0.5f, 1f), Color.White * num);
                ActiveFont.Draw(text, position + adjust, new Vector2(0.5f, 0f), Vector2.One, Color.White * num);
                if (cassetteWaitForKeyPress)
                {
                    GFX.Gui["textboxbutton"].DrawCentered(new Vector2(1824f, 984 + ((cassetteTimer % 1f < 0.25f) ? 6 : 0)));
                }
            }
            else
            {
                orig(self);
            }
        }

        private void onSpeedrunTimerDisplayDrawTime(On.Celeste.SpeedrunTimerDisplay.orig_DrawTime orig, Vector2 position, string timeString, float scale, bool valid, bool finished, bool bestTime, float alpha)
        {
            if (useMergeChaptersController && MergeChaptersControllerMode != "Classic" && SaveData.Instance.CurrentSession.Area.Mode == AreaMode.Normal)
            {
                valid = true;
            }
            orig(position, timeString, scale, valid, finished, bestTime, alpha);
        }

        private Session onSessionRestart(On.Celeste.Session.orig_Restart orig, Session self, string intoLevel)
        {
            if (useMergeChaptersController && SaveData.Instance.CurrentSession.Area.Mode == AreaMode.Normal)
            {
                Session session = new(self.Area, self.StartCheckpoint, self.OldStats)
                {
                    Time = ModSaveData.SavedTime.ContainsKey(SaveData.Instance.CurrentSession.Area.LevelSet) ? ModSaveData.SavedTime[SaveData.Instance.CurrentSession.Area.LevelSet] : 0L,
                    UnlockedCSide = self.UnlockedCSide
                };
                if (intoLevel != null)
                {
                    session.Level = intoLevel;
                    if (intoLevel != self.MapData.StartLevel().Name)
                    {
                        session.StartedFromBeginning = false;
                    }
                }
                return session;
            }
            else
            {
                return orig(self, intoLevel);
            }
        }

        public override void Initialize()
        {
            base.Initialize();
            hasOldExtendedVariants = Everest.Modules.Any(module => module.Metadata.Name == "ExtendedVariantMode" && module.Metadata.Version < new Version(0, 15, 9));
            foreach (EverestModule module in Everest.Modules)
            {
                if (module.Metadata.Name == "TheSecretOfCelesteMountain")
                {
                    SoCMVersion = module.Metadata.Version;
                }
            }
        }

        private void onLevelEndPauseEffects(On.Celeste.Level.orig_EndPauseEffects orig, Level self)
        {
            if (!SoCMTitleFromGame)
            {
                orig(self);
            }
        }

        private void onLevelPause(On.Celeste.Level.orig_Pause orig, Level self, int startIndex, bool minimal, bool quickReset)
        {
            if (startedAnySoCMChapter && SoCMVersion >= new Version(3, 0, 0))
            {
                if (self.Session.Level == "A-00" && !startedGame)
                {
                    return;
                }
            }
            if (useMergeChaptersController && (MergeChaptersControllerKeepPrologue ? SaveData.Instance.LastArea_Safe.ID != SaveData.Instance.LevelSetStats.AreaOffset : true))
            {
                if (quickReset)
                {
                    return;
                }
            }
            orig(self, startIndex, minimal, quickReset);
            if (Enumerable.FirstOrDefault(self.Entities.ToAdd, (Entity e) => e is TextMenu) is TextMenu textMenu)
            {
                if (!quickReset)
                {
                    onCreatePauseMenuButtons(self, textMenu, minimal);
                }
            }            
        }

        private void onReturnMapHintRender(On.Celeste.ReturnMapHint.orig_Render orig, ReturnMapHint self)
        {
            if (useMergeChaptersController && MergeChaptersControllerMode != "Classic" && !((MergeChaptersControllerKeepPrologue && SaveData.Instance.CurrentSession.Area.ID == SaveData.Instance.LevelSetStats.AreaOffset)))
            {
                MTexture mTexture = GFX.Gui["checkpoint"];
                string text = "";
                if (MergeChaptersControllerMode == "Rooms")
                {
                    text = Dialog.Clean("XaphanHelper_UI_ReturnToMap_Rooms");
                }
                else if (MergeChaptersControllerMode == "Warps")
                {
                    text = Dialog.Clean("XaphanHelper_UI_ReturnToMap_Warps");
                }
                float width = ActiveFont.Measure(text).X * 0.75f;
                float textureWidth = mTexture.Width * 0.75f;
                Vector2 value2 = new((1920f - width - textureWidth - 64f) / 2f, 730f);
                ActiveFont.DrawOutline(text, value2 + new Vector2(width / 2f, 0f), new Vector2(0.5f, 0.5f), Vector2.One * 0.75f, Color.LightGray, 2f, Color.Black);
                value2.X += width + 64f;
                mTexture.DrawCentered(value2 + new Vector2(textureWidth * 0.5f, 0f), Color.White, 0.75f);
            }
            else
            {
                orig(self);
            }
        }

        private bool modSaveDataFoundAnyCheckpoints(On.Celeste.SaveData.orig_FoundAnyCheckpoints orig, SaveData self, AreaKey area)
        {
            if (area.LevelSet == MergeChaptersControllerLevelSet && MergeChaptersControllerMode != "Classic")
            {
                return false;
            }
            return orig(self, area);
        }

        private IEnumerator onSaveLoadIconRoutine(On.Celeste.SaveLoadIcon.orig_Routine orig, SaveLoadIcon self)
        {
            if (useMergeChaptersController && !SaveIconVisible)
            {
                self.Add(new Coroutine(SwitchBackSaveLoadIconVisibility(self)));
            }
            yield return new SwapImmediately(orig(self));
        }

        private IEnumerator SwitchBackSaveLoadIconVisibility(SaveLoadIcon icon)
        {
            DynData<SaveLoadIcon> SaveLoadIconData = new(icon);
            Sprite iconSprite = SaveLoadIconData.Get<Sprite>("icon");
            while (iconSprite.CurrentAnimationID != "end")
            {
                yield return null;
            }
            yield return 0.5f;
            SaveIconVisible = true;
        }

        private void onSaveLoadIconRender(On.Celeste.SaveLoadIcon.orig_Render orig, SaveLoadIcon self)
        {
            if (useMergeChaptersController && !SaveIconVisible)
            {
                return;
            }
            orig(self);
        }

        private int modOuiChapterPanelGetModeHeight(On.Celeste.OuiChapterPanel.orig_GetModeHeight orig, OuiChapterPanel self)
        {
            if (SaveData.Instance != null)
            {
                AreaStats areaStats = SaveData.Instance.Areas_Safe[self.Area.ID];
                if (self.Area.Mode == AreaMode.BSide && !areaStats.Cassette && ModSaveData.CSideUnlocked.Contains(SaveData.Instance.LevelSetStats.Name + ":" + self.Area.ChapterIndex) && SaveData.Instance.UnlockedModes < 3 && !SaveData.Instance.DebugMode)
                {
                    AreaModeStats areaModeStats = self.RealStats.Modes[2];
                    bool flag = areaModeStats.Strawberries.Count <= 0;
                    if (!self.Data.Interlude_Safe && ((areaModeStats.Deaths > 0 && self.Area.Mode != 0) || areaModeStats.Completed || areaModeStats.HeartGem))
                    {
                        flag = false;
                    }
                    if (!flag)
                    {
                        return 540;
                    }
                    return 300;
                }
            }
            return orig(self);
        }

        private bool modOuiChapterPanelIsStart(On.Celeste.OuiChapterPanel.orig_IsStart orig, OuiChapterPanel self, Overworld overworld, Overworld.StartMode start)
        {
            if ((useMergeChaptersController && !MergeChaptersControllerKeepPrologue) || (useMergeChaptersController && MergeChaptersControllerKeepPrologue && SaveData.Instance.CurrentSession.Area.ChapterIndex > 0))
            {
                self.Position += new Vector2(10000, 0);
                return false;
            }
            if (SaveData.Instance != null && !SaveData.Instance.DebugMode)
            {
                AreaStats areaStats = SaveData.Instance.Areas_Safe[SaveData.Instance.LastArea_Safe.ID];
                bool unlockedBSide = false;
                bool unlockedCSide = false;
                if (areaStats.Cassette)
                {
                    unlockedBSide = true;
                }
                if (ModSaveData.CSideUnlocked.Contains(SaveData.Instance.LevelSetStats.Name + ":" + SaveData.Instance.LastArea_Safe.ChapterIndex) && SaveData.Instance.UnlockedModes < 3)
                {
                    unlockedCSide = true;
                }
                if (!unlockedBSide && unlockedCSide)
                {
                    if (SaveData.Instance != null && SaveData.Instance.LastArea_Safe.ID == AreaKey.None.ID)
                    {
                        SaveData.Instance.LastArea_Safe = AreaKey.Default;
                        OuiChapterPanel_instantClose.SetValue(self, true);
                    }
                    if (start == Overworld.StartMode.AreaComplete || start == Overworld.StartMode.AreaQuit)
                    {
                        AreaData areaData = AreaData.Get(SaveData.Instance.LastArea_Safe.ID);
                        areaData = (AreaData.Get(areaData?.Meta?.Parent) ?? areaData);
                        if (areaData != null)
                        {
                            SaveData.Instance.LastArea_Safe.ID = areaData.ID;
                        }
                    }
                    bool num = self.orig_IsStart(overworld, start);
                    if (ModSaveData.LastPlayedSide == 0)
                    {
                        self.Area.Mode = AreaMode.Normal;
                    }
                    else
                    {
                        self.Area.Mode = AreaMode.BSide;
                    }
                    ModSaveData.LastPlayedSide = 0;
                    return num;
                }
                else
                {
                    return orig(self, overworld, start);
                }
            }
            return orig(self, overworld, start);
        }

        private void modOuiChapterPanelUpdateStats(On.Celeste.OuiChapterPanel.orig_UpdateStats orig, OuiChapterPanel self, bool wiggle, bool? overrideStrawberryWiggle, bool? overrideDeathWiggle, bool? overrideHeartWiggle)
        {
            DynData<OuiChapterPanel> OuiChapterPanelData = new(self);
            DeathsCounter deaths = OuiChapterPanelData.Get<DeathsCounter>("deaths");
            HeartGemDisplay heart = OuiChapterPanelData.Get<HeartGemDisplay>("heart");
            StrawberriesCounter strawberries = OuiChapterPanelData.Get<StrawberriesCounter>("strawberries");
            AreaStats areaStats = SaveData.Instance.Areas_Safe[self.Area.ID];
            if (self.Area.Mode == AreaMode.BSide && !areaStats.Cassette && ModSaveData.CSideUnlocked.Contains(SaveData.Instance.LevelSetStats.Name + ":" + self.Area.ChapterIndex) && SaveData.Instance.UnlockedModes < 3 && !SaveData.Instance.DebugMode)
            {
                AreaModeStats areaModeStats = self.DisplayedStats.Modes[2];
                AreaData areaData = AreaData.Get(self.Area);
                deaths.Visible = (areaModeStats.Deaths > 0 && (self.Area.Mode != 0 || self.RealStats.Modes[2].Completed) && !AreaData.Get(self.Area).Interlude_Safe);
                deaths.Amount = areaModeStats.Deaths;
                deaths.SetMode(AreaMode.CSide);
                heart.Visible = (areaModeStats.HeartGem && !areaData.Interlude_Safe && areaData.CanFullClear);
                heart.SetCurrentMode(self.Area.Mode, areaModeStats.HeartGem);
                strawberries.Visible = false;
                strawberries.Golden = true;
                if (wiggle)
                {
                    if (strawberries.Visible && (!overrideStrawberryWiggle.HasValue || overrideStrawberryWiggle.Value))
                    {
                        strawberries.Wiggle();
                    }
                    if (heart.Visible && (!overrideHeartWiggle.HasValue || overrideHeartWiggle.Value))
                    {
                        heart.Wiggle();
                    }
                    if (deaths.Visible && (!overrideDeathWiggle.HasValue || overrideDeathWiggle.Value))
                    {
                        deaths.Wiggle();
                    }
                }
            }
            else
            {
                orig(self, wiggle, overrideStrawberryWiggle, overrideDeathWiggle, overrideHeartWiggle);
            }

        }

        private void modOuiChapterPanelReset(On.Celeste.OuiChapterPanel.orig_Reset orig, OuiChapterPanel self)
        {
            orig(self);
            if (!SaveData.Instance.DebugMode)
            {
                if (ModSaveData.CSideUnlocked.Contains(SaveData.Instance.LevelSetStats.Name + ":" + self.Area.ChapterIndex) && SaveData.Instance.UnlockedModes < 3)
                {
                    object CSideOption;
                    ((IList)OuiChapterPanel_modes.GetValue(self)).Add(
                        CSideOption = DynamicData.New(OuiChapterPanel_T_Option)(new
                        {
                            Label = Dialog.Clean("overworld_remix2"),
                            Icon = GFX.Gui["menu/rmx2"],
                            ID = "C"
                        })
                    );
                }
            }
        }

        private int MergedChapterAreaOffset;

        private void MergeChaptersControllerCheck(AreaData data = null)
        {
            useMergeChaptersController = false;
            lastLevelSet = SaveData.Instance.LevelSetStats.Name;
            MergedChapterAreaOffset = SaveData.Instance.LevelSetStats.AreaOffset;
            if (data != null)
            {
                MergedChapterAreaOffset = AreaData.Areas.FindIndex((AreaData area) => area.LevelSet == data.LevelSet);
            }
            MapData MapData = AreaData.Areas[MergedChapterAreaOffset].Mode[0].MapData;
            foreach (LevelData levelData in MapData.Levels)
            {
                foreach (EntityData entity in levelData.Entities)
                {
                    if (entity.Name == "XaphanHelper/MergeChaptersController")
                    {
                        useMergeChaptersController = true;
                        MergeChaptersControllerLevelSet = SaveData.Instance.LevelSetStats.Name;
                        MergeChaptersControllerMode = entity.Attr("mode");
                        MergeChaptersControllerKeepPrologue = entity.Bool("keepPrologueSeparated") && AreaData.Areas[SaveData.Instance.LevelSetStats.AreaOffset].Interlude_Safe;
                        break;
                    }
                }
            }
        }

        private void modOuiChapterSelectUpdate(On.Celeste.OuiChapterSelect.orig_Update orig, OuiChapterSelect self)
        {
            if (ModSaveData != null)
            {
                ModSaveData.LoadedPlayer = false;
            }
            if (SaveData.Instance != null)
            {
                if (lastLevelSet != SaveData.Instance.LevelSetStats.Name)
                {
                    MergeChaptersControllerCheck();
                    self.Overworld.Maddy.Hide();
                }
            }
            orig(self);
        }

        private IEnumerator modOuiChapterPanelStartRoutine(On.Celeste.OuiChapterPanel.orig_StartRoutine orig, OuiChapterPanel self, string checkpoint)
        {
            if (useMergeChaptersController)
            {
                self.EnteringChapter = true;
                self.Overworld.Maddy.Hide(down: false);
                self.Overworld.Mountain.EaseCamera(self.Area.ID, self.Data.MountainZoom, 1f, true);
                self.Add(new Coroutine(self.EaseOut(removeChildren: false)));
                yield return 0.2f;
                ScreenWipe.WipeColor = Color.Black;
                AreaData.Get(self.Area).Wipe(self.Overworld, false, null);
                Audio.SetMusic(null);
                Audio.SetAmbience(null);
                yield return 0.5f;
                if (MergeChaptersControllerKeepPrologue && self.Area.ID == SaveData.Instance.LevelSetStats.AreaOffset)
                {
                    LevelEnter.Go(new Session(self.Area, checkpoint)
                    {
                        Time = 0L
                    }
                    , fromSaveData: false);
                }
                else
                {
                    LevelEnter.Go(new Session(self.Area, checkpoint)
                    {
                        Time = ModSaveData.SavedTime.ContainsKey(SaveData.Instance.LevelSetStats.Name) ? ModSaveData.SavedTime[SaveData.Instance.LevelSetStats.Name] : 0L,
                        DoNotLoad = ModSaveData.SavedNoLoadEntities.ContainsKey(SaveData.Instance.LevelSetStats.Name) ? ModSaveData.SavedNoLoadEntities[SaveData.Instance.LevelSetStats.Name] : new HashSet<EntityID>(),
                        Strawberries = ModSaveData.SavedSessionStrawberries.ContainsKey(SaveData.Instance.LevelSetStats.Name) ? ModSaveData.SavedSessionStrawberries[SaveData.Instance.LevelSetStats.Name] : new HashSet<EntityID>()
                    }
                    , fromSaveData: false);

                }
            }
            else
            {
                yield return new SwapImmediately(orig(self, checkpoint));
            }
        }

        private void modOuiJournalProressCtor(On.Celeste.OuiJournalProgress.orig_ctor orig, OuiJournalProgress self, OuiJournal journal)
        {
            if (useMergeChaptersController)
            {
                self.PageTexture = "page";
                OuiJournalPage.Table Table = new OuiJournalPage.Table()
                    .AddColumn(new OuiJournalPage.TextCell(Dialog.Clean("journal_progress"), new Vector2(0f, 0.5f), 1f, Color.Black * 0.7f, 400f, true))
                    .AddColumn(new OuiJournalPage.EmptyCell(64f))
                    .AddColumn(new OuiJournalPage.EmptyCell(64f))
                    .AddColumn(new OuiJournalPage.EmptyCell(100f))
                    .AddColumn(new OuiJournalPage.IconCell("strawberry", 150f))
                    .AddColumn(new OuiJournalPage.IconCell("skullblue", 100f));
                if (SaveData.Instance.UnlockedModes >= 2)
                {
                    Table.AddColumn(new OuiJournalPage.IconCell("skullred", 100f));
                }
                // Only display C-Side deaths when not in SoCM
                if (SaveData.Instance.UnlockedModes >= 3 && SaveData.Instance.LevelSetStats.Name != "Xaphan/0")
                {
                    Table.AddColumn(new OuiJournalPage.IconCell("skullgold", 100f));
                }
                Table.AddColumn(new OuiJournalPage.IconCell("time", 220f));

                Color TextColor = Color.Black * 0.6f;
                Vector2 TextJustify = new(0.5f, 0.5f);
                int TotalASidesDeaths = 0;
                int TotalBSidesDeaths = 0;
                int TotalCSidesDeaths = 0;
                long TotalTime = 0;
                for (int i = 0; i < SaveData.Instance.LevelSetStats.Areas.Count; i++)
                {
                    AreaData areaData = AreaData.Get(SaveData.Instance.LevelSetStats.AreaOffset + i);
                    AreaStats areaStats = SaveData.Instance.Areas_Safe[SaveData.Instance.LevelSetStats.AreaOffset + i + ((MergeChaptersControllerKeepPrologue && i == 0) ? 1 : 0)];

                    bool Visited = false;
                    foreach (string visitedChapter in ModSaveData.VisitedChapters)
                    {
                        string[] visitedChapterData = visitedChapter.Split('_');
                        if (visitedChapter.Contains(SaveData.Instance.LevelSet) && visitedChapterData[2] == "0" && i == int.Parse(visitedChapterData[1].Remove(0, 2)))
                        {
                            Visited = true;
                        }
                    }

                    // Change name of prologue in SoCM, else use normal area name
                    string areaName = areaData.Name == "Xaphan/0/0-Prologue" ? "Xaphan/0/0-Prologue_Journal" : areaData.Name;

                    OuiJournalPage.Row row = Table.AddRow().Add(new OuiJournalPage.TextCell((Visited || i == 0) ? Dialog.Clean(areaName) : "???", new Vector2(1f, 0.5f), 0.6f, TextColor, 400f, true)).Add(null);
                    if (!areaData.Interlude_Safe)
                    {
                        List<string> list = new();
                        for (int j = 0; j < areaStats.Modes.Length; j++)
                        {
                            if (areaStats.Modes[j].HeartGem)
                            {
                                list.Add("heartgem" + j);
                            }
                        }

                        // Add Yellow hearts for SoCM only
                        if (SoCMVersion >= new Version(3, 0, 0) && areaData.Name.Contains("Xaphan/0"))
                        {
                            int YellowHearts = 0;
                            foreach (string achievement in ModSaveData.Achievements)
                            {
                                if (achievement.Contains("boss" + i) && achievement.Contains("cm"))
                                {
                                    YellowHearts++;
                                }
                            }
                            if (YellowHearts > 0)
                            {
                                for (int j = 1; j <= YellowHearts; j++)
                                {
                                    list.Add("heartgem2");
                                }
                            }
                        }

                        if (list.Count <= 0)
                        {
                            list.Add("dot");
                        }
                        row.Add(new OuiJournalPage.IconsCell(areaStats.Cassette ? "cassette" : "dot"));
                        row.Add(new OuiJournalPage.IconsCell(-32f, list.ToArray()));
                        string text = "0";
                        if (Visited && (areaData.Mode[0].TotalStrawberries > 0 || areaStats.TotalStrawberries > 0))
                        {
                            text = $"{areaStats.TotalStrawberries.ToString()}/{areaData.Mode[0].TotalStrawberries}";
                        }
                        row.Add(new OuiJournalPage.TextCell(text, TextJustify, 0.5f, TextColor));
                    }
                    else
                    {
                        row.Add(null).Add(null).Add(null);
                    }
                    if (areaData.IsFinal_Safe)
                    {
                        row.Add(new OuiJournalPage.TextCell(Dialog.Deaths(areaStats.Modes[0].Deaths), TextJustify, 0.5f, TextColor)
                        {
                            SpreadOverColumns = SaveData.Instance.UnlockedModes
                        });
                        for (int j = 0; j < SaveData.Instance.UnlockedModes - 1; j++)
                        {
                            row.Add(null);
                        }
                        TotalASidesDeaths += areaStats.Modes[0].Deaths;
                    }
                    else
                    {
                        for (int k = 0; k < ((SoCMVersion >= new Version(3, 0, 0) && areaData.Name.Contains("Xaphan/0")) ? Math.Min(SaveData.Instance.UnlockedModes, 2) : SaveData.Instance.UnlockedModes); k++)
                        {
                            if (areaData.HasMode((AreaMode)k))
                            {
                                row.Add(new OuiJournalPage.TextCell(Dialog.Deaths(areaStats.Modes[k].Deaths), TextJustify, 0.5f, TextColor));
                                if (k == 0)
                                {
                                    TotalASidesDeaths += areaStats.Modes[0].Deaths;
                                }
                                else if (k == 1)
                                {
                                    TotalBSidesDeaths += areaStats.Modes[1].Deaths;
                                }
                                else if (k == 2)
                                {
                                    TotalCSidesDeaths += areaStats.Modes[2].Deaths;
                                }
                            }
                            else
                            {
                                row.Add(new OuiJournalPage.TextCell("-", TextJustify, 0.5f, TextColor));
                            }
                        }
                    }
                    if (areaStats.TotalTimePlayed > 0)
                    {
                        row.Add(new OuiJournalPage.TextCell(Dialog.Time(areaStats.TotalTimePlayed), TextJustify, 0.5f, TextColor));
                        TotalTime += areaStats.TotalTimePlayed;
                    }
                    else
                    {
                        row.Add(new OuiJournalPage.IconCell("dot"));
                    }
                }
                Table.AddRow();
                OuiJournalPage.Row total = Table.AddRow().Add(new OuiJournalPage.TextCell(Dialog.Clean("journal_totals"), new Vector2(1f, 0.5f), 0.7f, TextColor)).Add(null);

                AreaStats lastAreaStats = SaveData.Instance.Areas_Safe[SaveData.Instance.LevelSetStats.AreaOffset + SaveData.Instance.LevelSetStats.Areas.Count - 1];
                if (lastAreaStats.Modes[0].Completed && !ModSaveData.SavedFlags.Contains(SaveData.Instance.LevelSet + "_GoldenStrawberryGet"))
                {
                    total.Add(new OuiJournalPage.IconCell("clear")).Add(null);
                }
                else if (!lastAreaStats.Modes[0].Completed && ModSaveData.SavedFlags.Contains(SaveData.Instance.LevelSet + "_GoldenStrawberryGet"))
                {
                    total.Add(null).Add(new OuiJournalPage.IconCell("goldenStrawberry"));
                }
                else if (lastAreaStats.Modes[0].Completed && ModSaveData.SavedFlags.Contains(SaveData.Instance.LevelSet + "_GoldenStrawberryGet"))
                {
                    total.Add(new OuiJournalPage.IconCell("clear")).Add(new OuiJournalPage.IconCell("goldenStrawberry"));
                }
                else
                {
                    total.Add(null).Add(null);
                }
                total.Add(new OuiJournalPage.TextCell(SaveData.Instance.TotalStrawberries_Safe.ToString() + "/" + SaveData.Instance.LevelSetStats.MaxStrawberries.ToString(), TextJustify, 0.6f, TextColor));
                total.Add(new OuiJournalPage.TextCell(Dialog.Deaths(TotalASidesDeaths), TextJustify, 0.6f, TextColor));
                if (SaveData.Instance.UnlockedModes >= 2)
                {
                    total.Add(new OuiJournalPage.TextCell(Dialog.Deaths(TotalBSidesDeaths), TextJustify, 0.6f, TextColor));
                }
                if (SaveData.Instance.UnlockedModes >= 3 && SaveData.Instance.LevelSetStats.Name != "Xaphan/0")
                {
                    total.Add(new OuiJournalPage.TextCell(Dialog.Deaths(TotalCSidesDeaths), TextJustify, 0.6f, TextColor));
                }
                total.Add(new OuiJournalPage.TextCell(Dialog.Time(TotalTime), TextJustify, 0.6f, TextColor));
                Table.AddRow();
                OuiJournalProress_table.SetValue(self, Table);
            }
            else
            {
                orig(self, journal);
            }
        }

        private void modOuiFileSelectSlotRender(On.Celeste.OuiFileSelectSlot.orig_Render orig, OuiFileSelectSlot self)
        {
            orig(self);
            if (self.Exists && !self.Corrupted && self.FurthestArea == 99999999 && self.SaveData != null)
            {
                string name = self.SaveData.LevelSetStats.Name;
                Vector2 width = ActiveFont.Measure(name);
                ActiveFont.Draw(Dialog.Clean(name), self.Position - Vector2.UnitX * Ease.CubeInOut((float)OuiFileSelectSlot_HighlightEase.GetValue(self)) * 360f + new Vector2(110f, -10f), new Vector2(0.5f, 0.5f), Vector2.One * (width.X > 220f ? 0.55f : 0.8f), Color.Black * 0.6f);
            }
        }

        private void modOuiFileSelectShow(On.Celeste.OuiFileSelectSlot.orig_Show orig, OuiFileSelectSlot self)
        {
            orig(self);
            LevelSetStats levelSetStats = self.SaveData?.LevelSetStats;
            if (levelSetStats != null)
            {
                bool useMergeChaptersController = false;
                MapData MapData = AreaData.Areas[AreaData.Areas.FindIndex((AreaData area) => area.LevelSet == self.SaveData.LevelSet)].Mode[0].MapData;
                foreach (LevelData levelData in MapData.Levels)
                {
                    foreach (EntityData entity in levelData.Entities)
                    {
                        if (entity.Name == "XaphanHelper/MergeChaptersController")
                        {
                            useMergeChaptersController = true;
                            break;
                        }
                    }
                }
                if (useMergeChaptersController)
                {
                    self.FurthestArea = 99999999;
                }
            }
        }

        private void modOuiFileSelectSlotCtor_SaveData(On.Celeste.OuiFileSelectSlot.orig_ctor_int_OuiFileSelect_SaveData orig, OuiFileSelectSlot self, int index, OuiFileSelect fileSelect, SaveData data)
        {
            orig(self, index, fileSelect, data);
            bool useMergeChaptersController = false;
            MapData MapData = AreaData.Areas[AreaData.Areas.FindIndex((AreaData area) => area.LevelSet == data.LevelSet)].Mode[0].MapData;
            foreach (LevelData levelData in MapData.Levels)
            {
                foreach (EntityData entity in levelData.Entities)
                {
                    if (entity.Name == "XaphanHelper/MergeChaptersController")
                    {
                        useMergeChaptersController = true;
                        break;
                    }
                }
            }
            if (useMergeChaptersController)
            {
                self.FurthestArea = 99999999;
            }
        }



        private IEnumerator modOuiJournalEnter(On.Celeste.OuiJournal.orig_Enter orig, OuiJournal self, Oui from)
        {
            yield return new SwapImmediately(orig(self, from));
            if (useMergeChaptersController)
            {
                self.Pages.Clear();
                self.Pages.Add(new OuiJournalCover(self));
                self.Pages.Add(new OuiJournalProgress(self));
                self.Pages.Add(new OuiJournalPoem(self));
                if (Stats.Has())
                {
                    self.Pages.Add(new OuiJournalGlobal(self));
                }
                self.Pages[0].Redraw(self.CurrentPageBuffer);
            }
        }


        private void modOuiChapterPanelStart(On.Celeste.OuiChapterPanel.orig_Start orig, OuiChapterPanel self, string checkpoint)
        {
            AreaStats areaStats = SaveData.Instance.Areas_Safe[self.Area.ID];
            if (self.Area.Mode == AreaMode.BSide && !areaStats.Cassette && ModSaveData.CSideUnlocked.Contains(SaveData.Instance.LevelSetStats.Name + ":" + self.Area.ChapterIndex) && !SaveData.Instance.DebugMode)
            {
                self.Focused = false;
                Audio.Play("event:/ui/world_map/chapter/checkpoint_start");
                self.Add(new Coroutine(StartChapterCSideRoutine(self, checkpoint)));
            }
            else
            {
                orig(self, checkpoint);
                self.Overworld.ShowInputUI = false;
            }
        }

        private IEnumerator modOuiChapterPanelIncrementStatsDisplay(On.Celeste.OuiChapterPanel.orig_IncrementStatsDisplay orig, OuiChapterPanel self, AreaModeStats modeStats, AreaModeStats newModeStats, bool doHeartGem, bool doStrawberries, bool doDeaths, bool doRemixUnlock)
        {
            AreaStats areaStats = SaveData.Instance.Areas_Safe[self.Data.ID];
            if (self.Area.Mode == AreaMode.BSide && !areaStats.Cassette && ModSaveData.CSideUnlocked.Contains(SaveData.Instance.LevelSetStats.Name + ":" + self.Area.ChapterIndex) && SaveData.Instance.UnlockedModes < 3 && !SaveData.Instance.DebugMode)
            {
                modeStats = self.DisplayedStats.Modes[2];
                newModeStats = areaStats.Modes[2];
                doStrawberries = newModeStats.TotalStrawberries > modeStats.TotalStrawberries;
                doHeartGem = newModeStats.HeartGem && !modeStats.HeartGem;
                doDeaths = newModeStats.Deaths > modeStats.Deaths && (self.Area.Mode != 0 || newModeStats.Completed);
            }
            yield return new SwapImmediately(orig(self, modeStats, newModeStats, doHeartGem, doStrawberries, doDeaths, doRemixUnlock));
        }

        private IEnumerator StartChapterCSideRoutine(OuiChapterPanel chapterPanel, string checkpoint = null)
        {
            int num = checkpoint?.IndexOf('|') ?? (-1);
            if (num >= 0)
            {
                chapterPanel.Area = (AreaData.Get(checkpoint.Substring(0, num))?.ToKey(chapterPanel.Area.Mode) ?? chapterPanel.Area);
                checkpoint = checkpoint.Substring(num + 1);
            }
            chapterPanel.EnteringChapter = true;
            chapterPanel.Overworld.Maddy.Hide(down: false);
            chapterPanel.Overworld.Mountain.EaseCamera(chapterPanel.Area.ID, chapterPanel.Data.MountainZoom, 1f, false, true);
            chapterPanel.Add(new Coroutine(chapterPanel.EaseOut(removeChildren: false)));
            yield return 0.2f;
            ScreenWipe.WipeColor = Color.Black;
            AreaData.Get(chapterPanel.Area).Wipe(chapterPanel.Overworld, false, null);
            Audio.SetMusic(null);
            Audio.SetAmbience(null);
            yield return 0.5f;
            LevelEnter.Go(new Session(new AreaKey(chapterPanel.Area.ID, AreaMode.CSide), checkpoint), fromSaveData: false);
        }

        private void getTeleportToOtherSidePortalsData(Level level)
        {
            TeleportToOtherSideData.Clear();
            HashSet<int> Modes = new();
            Modes.Add(0);
            if (AreaData.Areas[level.Session.Area.ID].HasMode(AreaMode.BSide))
            {
                Modes.Add(1);
            }
            if (AreaData.Areas[level.Session.Area.ID].HasMode(AreaMode.CSide))
            {
                Modes.Add(2);
            }
            foreach (int mode in Modes)
            {
                MapData MapData = AreaData.Areas[level.Session.Area.ID].Mode[mode].MapData;
                if (MapData != null)
                {
                    foreach (LevelData levelData in MapData.Levels)
                    {
                        foreach (EntityData entity in levelData.Entities)
                        {
                            if (entity.Name == "XaphanHelper/TeleportToOtherSidePortal")
                            {
                                TeleportToOtherSideData.Add(new TeleportToOtherSideData(mode, levelData.Name, entity.Position, entity.Attr("side")));
                            }
                        }
                    }
                }
            }
        }

        private void getRoomMusicControllerData(Level level)
        {
            RoomMusicControllerData.Clear();
            MapData MapData = AreaData.Areas[level.Session.Area.ID].Mode[0].MapData;
            if (MapData != null)
            {
                foreach (LevelData levelData in MapData.Levels)
                {
                    foreach (EntityData entity in levelData.Entities)
                    {
                        if (entity.Name == "XaphanHelper/RoomMusicController")
                        {
                            RoomMusicControllerData.Add(new RoomMusicControllerData(entity.Attr("rooms"), entity.Attr("excludeRooms"), entity.Attr("flagInnactive"), entity.Attr("flagA"), entity.Attr("flagB"), entity.Attr("flagC"), entity.Attr("flagD"), entity.Attr("musicIfFlagA"), entity.Attr("musicIfFlagB"), entity.Attr("musicIfFlagC"), entity.Attr("musicIfFlagD"), entity.Attr("defaultMusic")));
                        }
                    }
                }
            }
        }

        private static void AutoSaveThread(object o)
        {
            Level level = Engine.Scene as Level;
            if (level != null)
            {
                level.AutoSave();
            }
        }

        private void onLevelLoad(Level level, Player.IntroTypes playerIntro, bool isFromLoader)
        {
            // Save the game on room entry when using a Merge Chapters Controller
            if (useMergeChaptersController && MergeChaptersControllerMode == "Rooms")
            {
                SaveIconVisible = false;
                //RunThread.Start(AutoSaveThread, "AutoSaveThread");
                ThreadPool.QueueUserWorkItem(AutoSaveThread);
            }

            isInLevel = true;
            string room = level.Session.Level;
            string Prefix = level.Session.Area.LevelSet;
            int chapterIndex = level.Session.Area.ChapterIndex == -1 ? 0 : level.Session.Area.ChapterIndex;
            AreaKey area = level.Session.Area;
            MapData MapData = AreaData.Areas[area.ID].Mode[(int)area.Mode].MapData;

            // Define current side played

            ModSaveData.LastPlayedSide = (int)level.Session.Area.Mode;

            // Reset all checks

            useIngameMap = false;
            allRoomsUseTileController = false;
            useMetroidGameplay = false;
            useUpgrades = false;
            onSlope = false;

            // Checks controllers

            useIngameMapCheck(level);
            allRoomsUseTileControllerCheck(level);
            useMetroidGameplayCheck(level);
            useUpgradesCheck(level);
            getTeleportToOtherSidePortalsData(level);
            getRoomMusicControllerData(level);

            // In-game Map stuff

            if (useIngameMap)
            {
                // Check for backward compatibility. Restaure explored tiles if needed.

                foreach (string visitedRoom in ModSaveData.VisitedRooms)
                {
                    if (visitedRoom.Contains(Prefix + "/Ch" + chapterIndex + "/"))
                    {
                        if (ModSaveData.VisitedRoomsTiles.Count == 0)
                        {
                            MapDisplay.RestaureExploredTiles(Prefix, chapterIndex, level);
                            break;
                        }
                        else
                        {
                            bool skipRestaure = false;
                            foreach (string tile in ModSaveData.VisitedRoomsTiles)
                            {
                                if (tile.Contains(Prefix + "/Ch" + chapterIndex + "/"))
                                {
                                    skipRestaure = true;
                                }
                            }
                            if (!skipRestaure)
                            {
                                MapDisplay.RestaureExploredTiles(Prefix, chapterIndex, level);
                                break;
                            }
                        }
                    }
                }

                // Add current room to the in-game map

                if (!ModSaveData.VisitedRooms.Contains(Prefix + "/Ch" + chapterIndex + "/" + room) && (!string.IsNullOrEmpty(ModSaveData.DestinationRoom) ? level.Session.Level == ModSaveData.DestinationRoom : true) && (useMergeChaptersController ? ModSaveData.LoadedPlayer : true))
                {
                    ModSaveData.VisitedRooms.Add(Prefix + "/Ch" + chapterIndex + "/" + room);
                }
            }

            if (level.Session.GetFlag("Map_Opened"))
            {
                level.Session.SetFlag("Map_Opened", false);
            }

            // Visited Chapters check

            if (useUpgrades || useMergeChaptersController)
            {
                if (!ModSaveData.VisitedChapters.Contains(Prefix + "_Ch" + chapterIndex + "_" + (int)level.Session.Area.Mode))
                {
                    ModSaveData.VisitedChapters.Add(Prefix + "_Ch" + chapterIndex + "_" + (int)level.Session.Area.Mode);
                    ModSaveData.VisitedChapters.Sort();
                }
            }

            // Upgrades stuff

            if (useUpgrades)
            {
                GiveUpgradesToPlayer(MapData, level);
            }
            else // Set upgrades to default values if the map is not using upgrades
            {
                ModSettings.PowerGrip = true;
                ModSettings.ClimbingKit = true;
                ModSettings.SpiderMagnet = false;
                ModSettings.DroneTeleport = false;
                ModSettings.JumpBoost = false;
                ModSettings.ScrewAttack = false;
                ModSettings.VariaJacket = false;
                ModSettings.GravityJacket = false;
                ModSettings.Bombs = false;
                ModSettings.MegaBombs = false;
                ModSettings.RemoteDrone = false;
                ModSettings.GoldenFeather = false;
                ModSettings.Binoculars = false;
                ModSettings.EtherealDash = false;
                ModSettings.PortableStation = false;
                ModSettings.PulseRadar = false;
                ModSettings.DashBoots = true;
                ModSettings.SpaceJump = 1;
                ModSettings.HoverJet = false;
                ModSettings.LightningDash = false;
                ModSettings.LongBeam = false;
                ModSettings.IceBeam = false;
                ModSettings.WaveBeam = false;
                ModSettings.Spazer = false;
                ModSettings.PlasmaBeam = false;
                ModSettings.MorphingBall = false;
                ModSettings.MorphBombs = false;
                ModSettings.SpringBall = false;
                ModSettings.HighJumpBoots = false;
                ModSettings.SpeedBooster = false;
                ModSettings.MissilesModule = false;
                ModSettings.SuperMissilesModule = false;
            }

            // Set flags based on previous player progress

            if (!ModSaveData.SavedFlags.Contains(Prefix + "_teleporting"))
            {
                foreach (string savedFlag in ModSaveData.SavedFlags)
                {
                    if (forceStartingUpgrades)
                    {
                        if (savedFlag.Contains("Upgrade_"))
                        {
                            continue;
                        }
                    }
                    string[] savedFlags = savedFlag.Split('_');
                    if (savedFlags[0] == Prefix && savedFlags[1] == "Ch" + chapterIndex)
                    {

                        string flagPrefix = savedFlags[0] + "_" + savedFlags[1] + "_";
                        string flag = string.Empty;
                        int num = savedFlag.IndexOf(flagPrefix);
                        if (num >= 0)
                        {
                            flag = savedFlag.Remove(num, flagPrefix.Length);
                        }
                        level.Session.SetFlag(flag);
                    }
                }
                foreach (string flag in ModSaveData.SavedFlags)
                {
                    if (forceStartingUpgrades)
                    {
                        if (flag.Contains("Upgrade_"))
                        {
                            continue;
                        }
                    }
                    if (flag.Contains(Prefix))
                    {
                        string toRemove = Prefix + "_";
                        string result = string.Empty;
                        int i = flag.IndexOf(toRemove);
                        if (i >= 0)
                        {
                            result = flag.Remove(i, toRemove.Length);
                        }
                        level.Session.SetFlag(result, true);
                    }
                }
                foreach (string flag in ModSaveData.GlobalFlags)
                {
                    if (flag.Contains(Prefix))
                    {
                        string toRemove = Prefix + "_";
                        string result = string.Empty;
                        int i = flag.IndexOf(toRemove);
                        if (i >= 0)
                        {
                            result = flag.Remove(i, toRemove.Length);
                        }
                        level.Session.SetFlag(result, true);
                    }
                }
            }

            // Room Music stuff

            if (string.IsNullOrEmpty(level.Session.LevelData.Music) && string.IsNullOrEmpty(ModSaveData.DestinationRoom) && (useMergeChaptersController ? ModSaveData.LoadedPlayer : true))
            {
                bool playerHasGolden = useMergeChaptersController && PlayerHasGolden;
                foreach (RoomMusicControllerData roomMusicControllerData in RoomMusicControllerData)
                {
                    if (!level.Session.GetFlag(roomMusicControllerData.FlagInnactive) || string.IsNullOrEmpty(roomMusicControllerData.FlagInnactive))
                    {
                        string[] excludeRooms = roomMusicControllerData.ExcludeRooms.Split(',');
                        bool canApplyMusic = true;
                        foreach (string excludeRoom in excludeRooms)
                        {
                            if (level.Session.Level == excludeRoom)
                            {
                                canApplyMusic = false;
                                break;
                            }
                        }
                        string[] allowedRooms = roomMusicControllerData.Rooms.Split(',');
                        foreach (string allowedRoom in allowedRooms)
                        {
                            if (level.Session.Level.Contains(allowedRoom) && canApplyMusic)
                            {
                                if (level.Session.GetFlag(roomMusicControllerData.FlagA) && !string.IsNullOrEmpty(roomMusicControllerData.FlagA))
                                {
                                    level.Session.Audio.Music.Event = SFX.EventnameByHandle(roomMusicControllerData.MusicIfFlagA);
                                }
                                else if (level.Session.GetFlag(roomMusicControllerData.FlagB) && !string.IsNullOrEmpty(roomMusicControllerData.FlagB))
                                {
                                    level.Session.Audio.Music.Event = SFX.EventnameByHandle(roomMusicControllerData.MusicIfFlagB);
                                }
                                else if (level.Session.GetFlag(roomMusicControllerData.FlagC) && !string.IsNullOrEmpty(roomMusicControllerData.FlagC))
                                {
                                    level.Session.Audio.Music.Event = SFX.EventnameByHandle(roomMusicControllerData.MusicIfFlagC);
                                }
                                else if (level.Session.GetFlag(roomMusicControllerData.FlagD) && !string.IsNullOrEmpty(roomMusicControllerData.FlagD))
                                {
                                    level.Session.Audio.Music.Event = SFX.EventnameByHandle(roomMusicControllerData.MusicIfFlagD);
                                }
                                else
                                {
                                    level.Session.Audio.Music.Event = SFX.EventnameByHandle(roomMusicControllerData.DefaultMusic);
                                }
                                break;
                            }
                        }
                        level.Session.Audio.Apply(forceSixteenthNoteHack: false);
                    }
                }
            }

            // SoCM only

            if (level.Session.Area.LevelSet == "Xaphan/0")
            {
                startedAnySoCMChapter = true;

                // Backward saves compatibility

                SaveUpdater.UpdateSave(level);
            }
            startedAnyChapter = true;
        }

        private static void GiveUpgradesToPlayer(MapData MapData, Level level)
        {
            // Remove all upgrades

            ModSettings.PowerGrip = false;
            ModSettings.ClimbingKit = false;
            ModSettings.SpiderMagnet = false;
            ModSettings.DroneTeleport = false;
            ModSettings.JumpBoost = false;
            ModSettings.ScrewAttack = false;
            ModSettings.VariaJacket = false;
            ModSettings.GravityJacket = false;
            ModSettings.Bombs = false;
            ModSettings.MegaBombs = false;
            ModSettings.RemoteDrone = false;
            ModSettings.GoldenFeather = false;
            ModSettings.Binoculars = false;
            ModSettings.EtherealDash = false;
            ModSettings.PortableStation = false;
            ModSettings.PulseRadar = false;
            ModSettings.DashBoots = false;
            ModSettings.SpaceJump = 1;
            ModSettings.HoverJet = false;
            ModSettings.LightningDash = false;
            ModSettings.LongBeam = false;
            ModSettings.IceBeam = false;
            ModSettings.WaveBeam = false;
            ModSettings.Spazer = false;
            ModSettings.PlasmaBeam = false;
            ModSettings.MorphingBall = false;
            ModSettings.MorphBombs = false;
            ModSettings.SpringBall = false;
            ModSettings.HighJumpBoots = false;
            ModSettings.SpeedBooster = false;
            ModSettings.MissilesModule = false;
            ModSettings.SuperMissilesModule = false;
            level.Session.SetFlag("Using_Elevator", false);

            EntityData UpgradeController = new();
            foreach (LevelData levelData in MapData.Levels)
            {
                foreach (EntityData entity in levelData.Entities)
                {
                    if (entity.Name == "XaphanHelper/UpgradeController")
                    {
                        UpgradeController = entity;
                        break;
                    }
                }
            }

            // Get upgrades info from the Upgrade Controller

            bool setPowerGrip = UpgradeController.Bool("onlyAllowPowerGrip") || UpgradeController.Bool("startWithPowerGrip");
            bool setClimbingKit = UpgradeController.Bool("onlyAllowClimbingKit") || UpgradeController.Bool("startWithClimbingKit");
            bool setSpiderMagnet = UpgradeController.Bool("onlyAllowSpiderMagnet") || UpgradeController.Bool("startWithSpiderMagnet");
            bool setDroneTeleport = UpgradeController.Bool("onlyAllowDroneTeleport") || UpgradeController.Bool("startWithDroneTeleport");
            bool setJumpBoost = UpgradeController.Bool("onlyAllowJumpBoost") || UpgradeController.Bool("startWithJumpBoost");
            bool setScrewAttack = UpgradeController.Bool("onlyAllowScrewAttack") || UpgradeController.Bool("startWithScrewAttack");
            bool setVariaJacket = UpgradeController.Bool("onlyAllowVariaJacket") || UpgradeController.Bool("startWithVariaJacket");
            bool setGravityJacket = UpgradeController.Bool("onlyAllowGravityJacket") || UpgradeController.Bool("startWithGravityJacket");
            bool setBombs = UpgradeController.Bool("onlyAllowBombs") || UpgradeController.Bool("startWithBombs");
            bool setMegaBombs = UpgradeController.Bool("onlyAllowMegaBombs") || UpgradeController.Bool("startWithMegaBombs");
            bool setRemoteDrone = UpgradeController.Bool("onlyAllowRemoteDrone") || UpgradeController.Bool("startWithRemoteDrone");
            bool setGoldenFeather = UpgradeController.Bool("onlyAllowGoldenFeather") || UpgradeController.Bool("startWithGoldenFeather");
            bool setBinoculars = UpgradeController.Bool("onlyAllowBinoculars") || UpgradeController.Bool("startWithBinoculars");
            bool setEtherealDash = UpgradeController.Bool("onlyAllowEtherealDash") || UpgradeController.Bool("startWithEtherealDash");
            bool setPortableStation = UpgradeController.Bool("onlyAllowPortableStation") || UpgradeController.Bool("startWithPortableStation");
            bool setPulseRadar = UpgradeController.Bool("onlyAllowPulseRadar") || UpgradeController.Bool("startWithPulseRadar");
            bool setDashBoots = UpgradeController.Bool("onlyAllowDashBoots") || UpgradeController.Bool("startWithDashBoots");
            bool setSpaceJump = UpgradeController.Bool("onlyAllowSpaceJump") || UpgradeController.Bool("startWithSpaceJump");
            bool setHoverJet = UpgradeController.Bool("onlyAllowHoverJet") || UpgradeController.Bool("startWithHoverJet");
            bool setLightningDash = UpgradeController.Bool("onlyAllowLightningDash") || UpgradeController.Bool("startWithLightningDash");
            bool setLongBeam = UpgradeController.Bool("onlyAllowLongBeam") || UpgradeController.Bool("startWithLongBeam");
            bool setIceBeam = UpgradeController.Bool("onlyAllowIceBeam") || UpgradeController.Bool("startWithIceBeam");
            bool setWaveBeam = UpgradeController.Bool("onlyAllowWaveBeam") || UpgradeController.Bool("startWithWaveBeam");
            bool setMissilesModule = UpgradeController.Bool("onlyAllowMissilesModule") || UpgradeController.Bool("startWithMissilesModule");
            bool setSuperMissilesModule = UpgradeController.Bool("onlyAllowSuperMissilesModule") || UpgradeController.Bool("startWithSuperMissilesModule");
            bool hasStartingUpgrades = setPowerGrip || setClimbingKit || setSpiderMagnet || setDroneTeleport || setJumpBoost || setScrewAttack || setVariaJacket || setGravityJacket || setBombs || setMegaBombs || setRemoteDrone || setGoldenFeather || setBinoculars || setEtherealDash || setPortableStation || setPulseRadar || setDashBoots || setSpaceJump || setHoverJet || setLightningDash || setLongBeam || setIceBeam || setWaveBeam || setMissilesModule || setSuperMissilesModule;
            forceStartingUpgrades = UpgradeController.Bool("onlyAllowStartingUpgrades", hasStartingUpgrades ? true : false);

            // Check specified upgrades for the golden berry

            bool goldenPowerGrip = UpgradeController.Bool("goldenStartWithPowerGrip");
            bool goldenClimbingKit = UpgradeController.Bool("goldenStartWithClimbingKit");
            bool goldenSpiderMagnet = UpgradeController.Bool("goldenStartWithSpiderMagnet");
            bool goldenDroneTeleport = UpgradeController.Bool("goldenStartWithDroneTeleport");
            bool goldenJumpBoost = UpgradeController.Bool("goldenStartWithJumpBoost");
            bool goldenScrewAttack = UpgradeController.Bool("goldenStartWithScrewAttack");
            bool goldenVariaJacket = UpgradeController.Bool("goldenStartWithVariaJacket");
            bool goldenGravityJacket = UpgradeController.Bool("goldenStartWithGravityJacket");
            bool goldenBombs = UpgradeController.Bool("goldenStartWithBombs");
            bool goldenMegaBombs = UpgradeController.Bool("goldenStartWithMegaBombs");
            bool goldenRemoteDrone = UpgradeController.Bool("goldenStartWithRemoteDrone");
            bool goldenGoldenFeather = UpgradeController.Bool("goldenStartWithGoldenFeather");
            bool goldenBinoculars = UpgradeController.Bool("goldenStartWithBinoculars");
            bool goldenEtherealDash = UpgradeController.Bool("goldenStartWithEtherealDash");
            bool goldenPortableStation = UpgradeController.Bool("goldenStartWithPortableStation");
            bool goldenPulseRadar = UpgradeController.Bool("goldenStartWithPulseRadar");
            bool goldenDashBoots = UpgradeController.Bool("goldenStartWithDashBoots");
            bool goldenSpaceJump = UpgradeController.Bool("goldenStartWithSpaceJump");
            bool goldenHoverJet = UpgradeController.Bool("goldenStartWithHoverJet");
            bool goldenLightningDash = UpgradeController.Bool("goldenStartWithLightningDash");
            bool goldenLongBeam = UpgradeController.Bool("goldenStartWithLongBeam");
            bool goldenIceBeam = UpgradeController.Bool("goldenStartWithIceBeam");
            bool goldenWaveBeam = UpgradeController.Bool("goldenStartWithWaveBeam");
            bool goldenMissilesModule = UpgradeController.Bool("goldenStartWithMissilesModule");
            bool goldenSuperMissilesModule = UpgradeController.Bool("goldenStartWithSuperMissilesModule");

            // Give specified upgrades

            if (hasStartingUpgrades)
            {
                if (setPowerGrip || level.Session.GetFlag("Upgrade_PowerGrip"))
                {
                    ModSettings.PowerGrip = true;
                    level.Session.SetFlag("Upgrade_PowerGrip", true);
                }
                if (setClimbingKit || level.Session.GetFlag("Upgrade_ClimbingKit"))
                {
                    ModSettings.ClimbingKit = true;
                    level.Session.SetFlag("Upgrade_ClimbingKit", true);
                }
                if (setSpiderMagnet || level.Session.GetFlag("Upgrade_SpiderMagnet"))
                {
                    ModSettings.SpiderMagnet = true;
                    level.Session.SetFlag("Upgrade_SpiderMagnet", true);
                }
                if (setDroneTeleport || level.Session.GetFlag("Upgrade_DroneTeleport"))
                {
                    ModSettings.DroneTeleport = true;
                    level.Session.SetFlag("Upgrade_DroneTeleport", true);
                }
                if (setJumpBoost || level.Session.GetFlag("Upgrade_JumpBoost"))
                {
                    ModSettings.JumpBoost = true;
                    level.Session.SetFlag("Upgrade_JumpBoost", true);
                }
                if (setScrewAttack || level.Session.GetFlag("Upgrade_ScrewAttack"))
                {
                    ModSettings.ScrewAttack = true;
                    level.Session.SetFlag("Upgrade_ScrewAttack", true);
                }
                if (setVariaJacket || level.Session.GetFlag("Upgrade_VariaJacket"))
                {
                    ModSettings.VariaJacket = true;
                    level.Session.SetFlag("Upgrade_VariaJacket", true);
                }
                if (setGravityJacket || level.Session.GetFlag("Upgrade_GravityJacket"))
                {
                    ModSettings.GravityJacket = true;
                    level.Session.SetFlag("Upgrade_GravityJacket", true);
                }
                if (setBombs || level.Session.GetFlag("Upgrade_Bombs"))
                {
                    ModSettings.Bombs = true;
                    level.Session.SetFlag("Upgrade_Bombs", true);
                }
                if (setMegaBombs || level.Session.GetFlag("Upgrade_MegaBombs"))
                {
                    ModSettings.MegaBombs = true;
                    level.Session.SetFlag("Upgrade_MegaBombs", true);
                }
                if (setRemoteDrone || level.Session.GetFlag("Upgrade_RemoteDrone"))
                {
                    ModSettings.RemoteDrone = true;
                    level.Session.SetFlag("Upgrade_RemoteDrone", true);
                }
                if (setGoldenFeather || level.Session.GetFlag("Upgrade_GoldenFeather"))
                {
                    ModSettings.GoldenFeather = true;
                    level.Session.SetFlag("Upgrade_GoldenFeather", true);
                }
                if (setBinoculars || level.Session.GetFlag("Upgrade_Binoculars"))
                {
                    ModSettings.Binoculars = true;
                    level.Session.SetFlag("Upgrade_Binoculars", true);
                }
                if (setEtherealDash || level.Session.GetFlag("Upgrade_EtherealDash"))
                {
                    ModSettings.EtherealDash = true;
                    level.Session.SetFlag("Upgrade_EtherealDash", true);
                }
                if (setPortableStation || level.Session.GetFlag("Upgrade_PortableStation"))
                {
                    ModSettings.PortableStation = true;
                    level.Session.SetFlag("Upgrade_PortableStation", true);
                }
                if (setPulseRadar || level.Session.GetFlag("Upgrade_PulseRadar"))
                {
                    ModSettings.PulseRadar = true;
                    level.Session.SetFlag("Upgrade_PulseRadar", true);
                }
                if (setDashBoots || level.Session.GetFlag("Upgrade_DashBoots"))
                {
                    ModSettings.DashBoots = true;
                    level.Session.SetFlag("Upgrade_DashBoots", true);
                }
                if (setSpaceJump || level.Session.GetFlag("Upgrade_SpaceJump"))
                {
                    ModSettings.SpaceJump = 2;
                    level.Session.SetFlag("Upgrade_SpaceJump", true);
                }
                if (setHoverJet || level.Session.GetFlag("Upgrade_HoverJet"))
                {
                    ModSettings.HoverJet = true;
                    level.Session.SetFlag("Upgrade_HoverJet", true);
                }
                if (setLightningDash || level.Session.GetFlag("Upgrade_LightningDash"))
                {
                    ModSettings.LightningDash = true;
                    level.Session.SetFlag("Upgrade_LightningDash", true);
                }
                if (setLongBeam || level.Session.GetFlag("Upgrade_LongBeam"))
                {
                    ModSettings.LongBeam = true;
                    level.Session.SetFlag("Upgrade_LongBeam", true);
                }
                if (setIceBeam || level.Session.GetFlag("Upgrade_IceBeam"))
                {
                    ModSettings.IceBeam = true;
                    level.Session.SetFlag("Upgrade_IceBeam", true);
                }
                if (setWaveBeam || level.Session.GetFlag("Upgrade_WaveBeam"))
                {
                    ModSettings.WaveBeam = true;
                    level.Session.SetFlag("Upgrade_WaveBeam", true);
                }
                if (setMissilesModule || level.Session.GetFlag("Upgrade_MissilesModule"))
                {
                    ModSettings.MissilesModule = true;
                    level.Session.SetFlag("Upgrade_MissilesModule", true);
                }
                if (setSuperMissilesModule || level.Session.GetFlag("Upgrade_SuperMissilesModule"))
                {
                    ModSettings.SuperMissilesModule = true;
                    level.Session.SetFlag("Upgrade_SuperMissilesModule", true);
                }
            }
            else
            {
                // Give back upgrades the player has unlocked

                if (!forceStartingUpgrades && (!PlayerHasGolden || (PlayerHasGolden && ModSaveData.GoldenStartChapter != -999)))
                {
                    // Celeste Upgrades

                    if (PowerGripCollected(level))
                    {
                        ModSettings.PowerGrip = true;
                        level.Session.SetFlag("Upgrade_PowerGrip", true);
                    }
                    if (ClimbingKitCollected(level))
                    {
                        ModSettings.ClimbingKit = true;
                        level.Session.SetFlag("Upgrade_ClimbingKit", true);
                    }
                    if (SpiderMagnetCollected(level))
                    {
                        ModSettings.SpiderMagnet = true;
                        level.Session.SetFlag("Upgrade_SpiderMagnet", true);
                    }
                    if (DroneTeleportCollected(level))
                    {
                        ModSettings.DroneTeleport = true;
                        level.Session.SetFlag("Upgrade_DroneTeleport", true);
                    }
                    if (JumpBoostCollected(level))
                    {
                        ModSettings.JumpBoost = true;
                        level.Session.SetFlag("Upgrade_JumpBoost", true);
                    }
                    if (BombsCollected(level))
                    {
                        ModSettings.Bombs = true;
                        level.Session.SetFlag("Upgrade_Bombs", true);
                    }
                    if (MegaBombsCollected(level))
                    {
                        ModSettings.MegaBombs = true;
                        level.Session.SetFlag("Upgrade_MegaBombs", true);
                    }
                    if (RemoteDroneCollected(level))
                    {
                        ModSettings.RemoteDrone = true;
                        level.Session.SetFlag("Upgrade_RemoteDrone", true);
                    }
                    if (GoldenFeatherCollected(level))
                    {
                        ModSettings.GoldenFeather = true;
                        level.Session.SetFlag("Upgrade_GoldenFeather", true);
                    }
                    if (BinocularsCollected(level))
                    {
                        ModSettings.Binoculars = true;
                        level.Session.SetFlag("Upgrade_Binoculars", true);
                    }
                    if (EtherealDashCollected(level))
                    {
                        ModSettings.EtherealDash = true;
                        level.Session.SetFlag("Upgrade_EtherealDash", true);
                    }
                    if (PortableStationCollected(level))
                    {
                        ModSettings.PortableStation = true;
                        level.Session.SetFlag("Upgrade_PortableStation", true);
                    }
                    if (PulseRadarCollected(level))
                    {
                        ModSettings.PulseRadar = true;
                        level.Session.SetFlag("Upgrade_PulseRadar", true);
                    }
                    if (DashBootsCollected(level))
                    {
                        ModSettings.DashBoots = true;
                        level.Session.SetFlag("Upgrade_DashBoots", true);
                    }
                    if (HoverJetCollected(level))
                    {
                        ModSettings.HoverJet = true;
                        level.Session.SetFlag("Upgrade_HoverJet", true);
                    }
                    if (LightningDashCollected(level))
                    {
                        ModSettings.LightningDash = true;
                        level.Session.SetFlag("Upgrade_LightningDash", true);
                    }
                    if (MissilesModuleCollected(level))
                    {
                        ModSettings.MissilesModule = true;
                        level.Session.SetFlag("Upgrade_MissilesModule", true);
                    }
                    if (SuperMissilesModuleCollected(level))
                    {
                        ModSettings.SuperMissilesModule = true;
                        level.Session.SetFlag("SuperUpgrade_MissilesModule", true);
                    }

                    //Metroid Upgrades

                    if (SpazerCollected(level))
                    {
                        ModSettings.Spazer = true;
                        level.Session.SetFlag("Upgrade_Spazer", true);
                    }
                    if (PlasmaBeamCollected(level))
                    {
                        ModSettings.PlasmaBeam = true;
                        level.Session.SetFlag("Upgrade_PlasmaBeam", true);
                    }
                    if (MorphingBallCollected(level))
                    {
                        ModSettings.MorphingBall = true;
                        level.Session.SetFlag("Upgrade_MorphingBall", true);
                    }
                    if (MorphBombsCollected(level))
                    {
                        ModSettings.MorphBombs = true;
                        level.Session.SetFlag("Upgrade_MorphBombs", true);
                    }
                    if (SpringBallCollected(level))
                    {
                        ModSettings.SpringBall = true;
                        level.Session.SetFlag("Upgrade_SpringBall", true);
                    }
                    if (HighJumpBootsCollected(level))
                    {
                        ModSettings.HighJumpBoots = true;
                        level.Session.SetFlag("Upgrade_HighJumpBoots", true);
                    }
                    if (SpeedBoosterCollected(level))
                    {
                        ModSettings.SpeedBooster = true;
                        level.Session.SetFlag("Upgrade_SpeedBooster", true);
                    }

                    // Common Upgrades

                    if (LongBeamCollected(level))
                    {
                        ModSettings.LongBeam = true;
                        level.Session.SetFlag("Upgrade_LongBeam", true);
                    }
                    if (IceBeamCollected(level))
                    {
                        ModSettings.IceBeam = true;
                        level.Session.SetFlag("Upgrade_IceBeam", true);
                    }
                    if (WaveBeamCollected(level))
                    {
                        ModSettings.WaveBeam = true;
                        level.Session.SetFlag("Upgrade_WaveBeam", true);
                    }
                    if (VariaJacketCollected(level))
                    {
                        ModSettings.VariaJacket = true;
                        level.Session.SetFlag("Upgrade_VariaJacket", true);
                    }
                    if (GravityJacketCollected(level))
                    {
                        ModSettings.GravityJacket = true;
                        level.Session.SetFlag("Upgrade_GravityJacket", true);
                    }
                    if (ScrewAttackCollected(level))
                    {
                        ModSettings.ScrewAttack = true;
                        level.Session.SetFlag("Upgrade_ScrewAttack", true);
                    }
                    if (SpaceJumpCollected(level))
                    {
                        ModSettings.SpaceJump = !useMetroidGameplay ? 2 : 6;
                        level.Session.SetFlag("Upgrade_SpaceJump", true);
                    }
                }
                else if (PlayerHasGolden) // If the player has the golden berry
                {
                    if (goldenPowerGrip || level.Session.GetFlag("Upgrade_PowerGrip"))
                    {
                        ModSettings.PowerGrip = true;
                    }
                    if (goldenClimbingKit || level.Session.GetFlag("Upgrade_ClimbingKit"))
                    {
                        ModSettings.ClimbingKit = true;
                    }
                    if (goldenSpiderMagnet || level.Session.GetFlag("Upgrade_SpiderMagnet"))
                    {
                        ModSettings.SpiderMagnet = true;
                    }
                    if (goldenDroneTeleport || level.Session.GetFlag("Upgrade_DroneTeleport"))
                    {
                        ModSettings.DroneTeleport = true;
                    }
                    if (goldenJumpBoost || level.Session.GetFlag("Upgrade_JumpBoost"))
                    {
                        ModSettings.JumpBoost = true;
                    }
                    if (goldenScrewAttack || level.Session.GetFlag("Upgrade_ScrewAttack"))
                    {
                        ModSettings.ScrewAttack = true;
                    }
                    if (goldenVariaJacket || level.Session.GetFlag("Upgrade_VariaJacket"))
                    {
                        ModSettings.VariaJacket = true;
                    }
                    if (goldenGravityJacket || level.Session.GetFlag("Upgrade_GravityJacket"))
                    {
                        ModSettings.GravityJacket = true;
                    }
                    if (goldenBombs || level.Session.GetFlag("Upgrade_Bombs"))
                    {
                        ModSettings.Bombs = true;
                    }
                    if (goldenMegaBombs || level.Session.GetFlag("Upgrade_MegaBombs"))
                    {
                        ModSettings.MegaBombs = true;
                    }
                    if (goldenRemoteDrone || level.Session.GetFlag("Upgrade_RemoteDrone"))
                    {
                        ModSettings.RemoteDrone = true;
                    }
                    if (goldenGoldenFeather || level.Session.GetFlag("Upgrade_GoldenFeather"))
                    {
                        ModSettings.GoldenFeather = true;
                    }
                    if (goldenBinoculars || level.Session.GetFlag("Upgrade_Binoculars"))
                    {
                        ModSettings.Binoculars = true;
                    }
                    if (goldenEtherealDash || level.Session.GetFlag("Upgrade_EtherealDash"))
                    {
                        ModSettings.EtherealDash = true;
                    }
                    if (goldenPortableStation || level.Session.GetFlag("Upgrade_PortableStation"))
                    {
                        ModSettings.PortableStation = true;
                    }
                    if (goldenPulseRadar || level.Session.GetFlag("Upgrade_PulseRadar"))
                    {
                        ModSettings.PulseRadar = true;
                    }
                    if (goldenDashBoots || level.Session.GetFlag("Upgrade_DashBoots"))
                    {
                        ModSettings.DashBoots = true;
                    }
                    if (goldenSpaceJump || level.Session.GetFlag("Upgrade_SpaceJump"))
                    {
                        ModSettings.SpaceJump = 2;
                    }
                    if (goldenHoverJet || level.Session.GetFlag("Upgrade_HoverJet"))
                    {
                        ModSettings.HoverJet = true;
                    }
                    if (goldenLightningDash || level.Session.GetFlag("Upgrade_LightningDash"))
                    {
                        ModSettings.LightningDash = true;
                    }
                    if (goldenLongBeam || level.Session.GetFlag("Upgrade_LongBeam"))
                    {
                        ModSettings.LongBeam = true;
                    }
                    if (goldenIceBeam || level.Session.GetFlag("Upgrade_IceBeam"))
                    {
                        ModSettings.IceBeam = true;
                    }
                    if (goldenWaveBeam || level.Session.GetFlag("Upgrade_WaveBeam"))
                    {
                        ModSettings.WaveBeam = true;
                    }
                    if (goldenMissilesModule || level.Session.GetFlag("Upgrade_MissilesModule"))
                    {
                        ModSettings.MissilesModule = true;
                    }
                    if (goldenSuperMissilesModule || level.Session.GetFlag("Upgrade_SuperMissilesModule"))
                    {
                        ModSettings.SuperMissilesModule = true;
                    }
                }
            }
        }

        private void onLevelExit(Level level, LevelExit exit, LevelExit.Mode mode, Session session, HiresSnow snow)
        {
            // Remove checks;

            useIngameMap = false;
            allRoomsUseTileController = false;
            useUpgrades = false;
            useMetroidGameplay = false;

            // Remove PickedGolden flag from save

            PlayerHasGolden = false;
            List<string> ToRemove = new();
            foreach (string flag in ModSaveData.SavedFlags)
            {
                if (flag.Contains("GoldenStrawberry") && flag != level.Session.Area.LevelSet + "_GoldenStrawberryGet")
                {
                    ToRemove.Add(flag);
                }
            }
            foreach (string flag in ToRemove)
            {
                ModSaveData.SavedFlags.Remove(flag);
            }

            // Reset Variables

            ModSaveData.DestinationRoom = "";
            ModSaveData.CountdownCurrentTime = -1;
            ModSaveData.CountdownShake = false;
            ModSaveData.CountdownExplode = false;
            if (!string.IsNullOrEmpty(ModSaveData.CountdownActiveFlag) && ModSaveData.SavedSesionFlags.ContainsKey(session.Area.LevelSet))
            {
                ModSaveData.SavedSesionFlags[session.Area.LevelSet] = ModSaveData.SavedSesionFlags[session.Area.LevelSet].Replace(ModSaveData.CountdownActiveFlag + ",", "");
                ModSaveData.SavedSesionFlags[session.Area.LevelSet] = ModSaveData.SavedSesionFlags[session.Area.LevelSet].Replace("," + ModSaveData.CountdownActiveFlag, "");
            }
            ModSaveData.CountdownActiveFlag = "";
            ModSaveData.CountdownStartChapter = -1;
            ModSaveData.CountdownStartRoom = "";
            ModSaveData.CountdownSpawn = new Vector2();
            ModSaveData.CountdownUseLevelWipe = false;
            ModSaveData.BagUIId1 = 0;
            ModSaveData.BagUIId2 = 0;
            ModSaveData.LoadedPlayer = false;
            startedAnySoCMChapter = false;
            SkipSoCMIntro = false;
            minimapEnabled = false;
            TriggeredCountDown = false;
            ModSaveData.CanDisplayPopups = false;
            if ((!useMergeChaptersController || (useMergeChaptersController && MergeChaptersControllerMode != "Rooms")) && mode != LevelExit.Mode.SaveAndQuit)
            {
                ModSaveData.startAsDrone.Remove(level.Session.Area.LevelSet);
                ModSaveData.droneStartChapter.Remove(level.Session.Area.LevelSet);
                ModSaveData.droneStartRoom.Remove(level.Session.Area.LevelSet);
                ModSaveData.droneStartSpawn.Remove(level.Session.Area.LevelSet);
                ModSaveData.droneCurrentSpawn.Remove(level.Session.Area.LevelSet);
                ModSaveData.fakePlayerFacing.Remove(level.Session.Area.LevelSet);
                ModSaveData.fakePlayerPosition.Remove(level.Session.Area.LevelSet);
            }
            NoDroneSpawnSound = false;
            SaveSettings();

            // Store Current Light Mode in SaveData if using a Merge Chapters Controller

            if (useMergeChaptersController && MergeChaptersControllerMode == "Rooms")
            {
                LightManager manager = level.Tracker.GetEntity<LightManager>();
                if (manager != null)
                {
                    if (manager.RespawnMode != LightModes.None)
                    {
                        ModSaveData.LightMode[level.Session.Area.LevelSet] = manager.RespawnMode;
                    }
                }
            }

            onSlope = false;
            isInLevel = false;
        }

        public static void SaveModSettings()
        {
            foreach (EverestModule module in Everest.Modules)
            {
                if (module.Metadata.Name == "XaphanHelper")
                {
                    module.SaveSettings();
                    break;
                }
            }
        }

        private static void onCreatePauseMenuButtons(Level level, TextMenu menu, bool minimal)
        {
            if (useMetroidGameplay)
            {
                // Find the "Retry" item and remove it from the menu if it exist
                int retryIndex = menu.Items.FindIndex(item => item.GetType() == typeof(TextMenu.Button) && ((TextMenu.Button)item).Label == Dialog.Clean("MENU_PAUSE_RETRY"));
                if (retryIndex != -1)
                {
                    menu.Remove(menu.Items[retryIndex]);
                }
            }
            if ((level.Session.GetFlag("boss_Normal_Mode") || level.Session.GetFlag("boss_Challenge_Mode")) && !level.Session.GetFlag("In_bossfight"))
            {
                // Find the position of "Retry"
                int retryIndex = menu.Items.FindIndex(item => item.GetType() == typeof(TextMenu.Button) && ((TextMenu.Button)item).Label == Dialog.Clean("MENU_PAUSE_RETRY"));

                if (retryIndex == -1)
                {
                    // Top of the menu if "Retry" is not found
                    retryIndex = 0;
                }

                // add the "Giveup Challenge Mode" button
                TextMenu.Button GiveUpCMButton = new(Dialog.Clean(level.Session.GetFlag("boss_Normal_Mode") ? "XaphanHelper_UI_GiveUpNM" : "XaphanHelper_UI_GiveUpCM"));
                GiveUpCMButton.Pressed(() =>
                {
                    level.PauseMainMenuOpen = false;
                    menu.RemoveSelf();
                    confirmGiveUpCMMenu(level, menu.Selection);
                });
                GiveUpCMButton.ConfirmSfx = "event:/ui/main/message_confirm";
                menu.Insert(retryIndex + 1, GiveUpCMButton);
            }
            if (useMergeChaptersController && (MergeChaptersControllerKeepPrologue ? SaveData.Instance.LastArea_Safe.ID != SaveData.Instance.LevelSetStats.AreaOffset : true))
            {
                // Find the "Restart chapter" button and remove it from the menu if it exist
                int restartAreaIndex = menu.Items.FindIndex(item => item.GetType() == typeof(TextMenu.Button) && ((TextMenu.Button)item).Label == Dialog.Clean("MENU_PAUSE_RESTARTAREA"));
                if (restartAreaIndex != -1)
                {
                    menu.Remove(menu.Items[restartAreaIndex]);
                }

                // add the "Restart campaign" button
                TextMenu.Button RestartCampaignButton = new(Dialog.Clean("XaphanHelper_UI_RestartCampaign"));
                RestartCampaignButton.Pressed(() =>
                {
                    level.PauseMainMenuOpen = false;
                    menu.RemoveSelf();
                    confirmRestartCampaign(level, menu.Selection);
                });
                RestartCampaignButton.ConfirmSfx = "event:/ui/main/message_confirm";
                menu.Insert(restartAreaIndex, RestartCampaignButton);
            }
            if (useMergeChaptersController && (level.Session.Area.Mode == AreaMode.BSide || level.Session.Area.Mode == AreaMode.CSide))
            {
                // Find the position of "Return to map"
                int returnToMapIndex = menu.Items.FindIndex(item => item.GetType() == typeof(TextMenu.Button) && ((TextMenu.Button)item).Label == Dialog.Clean("MENU_PAUSE_RETURN"));

                if (returnToMapIndex == -1)
                {
                    // Bottm of the menu if "Return to map" is not found
                    returnToMapIndex = menu.Items.Count - 1;
                }

                // remove "Return to map" from the menu
                if (returnToMapIndex != -1)
                {
                    menu.Remove(menu.Items[returnToMapIndex]);
                }

                // add the "Exit X-Side" button
                TextMenu.Button ExitSideButton = new(Dialog.Clean("XaphanHelper_UI_GiveUp" + (level.Session.Area.Mode == AreaMode.BSide ? "B" : "C") + "Side"));
                ExitSideButton.Pressed(() =>
                {
                    level.PauseMainMenuOpen = false;
                    menu.RemoveSelf();
                    confirmExitSideMenu(level, menu.Selection);
                });
                ExitSideButton.ConfirmSfx = "event:/ui/main/message_confirm";
                menu.Insert(returnToMapIndex, ExitSideButton);
            }

            // SoCM Only
            if (level.Session.Area.LevelSet == "Xaphan/0" && SoCMVersion >= new Version(3, 0, 0))
            {
                // Find the position of "Save and Quit"
                int saveAndQuitIndex = menu.Items.FindIndex(item => item.GetType() == typeof(TextMenu.Button) && ((TextMenu.Button)item).Label == Dialog.Clean("MENU_PAUSE_SAVEQUIT"));

                if (saveAndQuitIndex == -1)
                {
                    // Bottom of the menu if "Return to map" is not found
                    saveAndQuitIndex = menu.Items.Count - 1;
                }

                // remove "Save and Quit" from the menu
                if (saveAndQuitIndex != -1)
                {
                    menu.Remove(menu.Items[saveAndQuitIndex]);
                }

                // Find the position of "Restart Campaign"
                int restartCampaignIndex = menu.Items.FindIndex(item => item.GetType() == typeof(TextMenu.Button) && ((TextMenu.Button)item).Label == Dialog.Clean("XaphanHelper_UI_RestartCampaign"));

                if (restartCampaignIndex == -1)
                {
                    // Bottom of the menu if "Restart Campaign" is not found
                    restartCampaignIndex = menu.Items.Count - 1;
                }

                // remove "Restart Campaign" from the menu
                if (restartCampaignIndex != -1)
                {
                    menu.Remove(menu.Items[restartCampaignIndex]);
                }

                // Find the position of "Options"
                int OptionsIndex = menu.Items.FindIndex(item => item.GetType() == typeof(TextMenu.Button) && ((TextMenu.Button)item).Label == Dialog.Clean("MENU_PAUSE_OPTIONS"));

                if (OptionsIndex == -1)
                {
                    // Bottom of the menu if "Options" is not found
                    OptionsIndex = menu.Items.Count - 1;
                }

                // add the "Settings" button
                TextMenu.Button SettingsButton = new(Dialog.Clean("Xaphan_0_0_intro_vignette_Settings"));
                SettingsButton.Pressed(() =>
                {
                    level.PauseMainMenuOpen = false;
                    menu.RemoveSelf();
                    confirmSettingsMenu(level, menu.Selection);
                });
                SettingsButton.ConfirmSfx = "event:/ui/main/message_confirm";
                menu.Insert(OptionsIndex, SettingsButton);

                if (level.Session.Area.Mode == AreaMode.Normal)
                {
                    // Find the position of "Return to map"
                    int returnToMapIndex = menu.Items.FindIndex(item => item.GetType() == typeof(TextMenu.Button) && ((TextMenu.Button)item).Label == Dialog.Clean("MENU_PAUSE_RETURN"));

                    if (returnToMapIndex == -1)
                    {
                        // Bottom of the menu if "Return to map" is not found
                        returnToMapIndex = menu.Items.Count - 1;
                    }

                    // remove "Return to map" from the menu
                    if (returnToMapIndex != -1)
                    {
                        menu.Remove(menu.Items[returnToMapIndex]);
                    }

                    // add the "Return to Title Screen" button
                    TextMenu.Button ReturnToTitleButton = new(Dialog.Clean("Xaphan_0_Pause_Menu_ReturnTitle"));
                    ReturnToTitleButton.Pressed(() =>
                    {
                        level.PauseMainMenuOpen = false;
                        menu.RemoveSelf();
                        confirmReturnToTitleMenu(level, menu.Selection);
                    });
                    ReturnToTitleButton.ConfirmSfx = "event:/ui/main/message_confirm";
                    menu.Insert(returnToMapIndex, ReturnToTitleButton);
                }

                // Remove every other non-vanilla items from the menu
                List<int> IndexesToRemove = new();
                foreach (TextMenu.Item item in menu.Items)
                {
                    if (item.GetType() == typeof(TextMenu.Button) || !item.GetType().ToString().Contains("Celeste"))
                    {
                        TextMenu.Button button = (TextMenu.Button)item;
                        if(button.Label != Dialog.Clean("MENU_PAUSE_RESUME") &&
                            button.Label != Dialog.Clean("MENU_PAUSE_SKIP_CUTSCENE") &&
                            button.Label != Dialog.Clean("MENU_PAUSE_RETRY") &&
                            button.Label != Dialog.Clean("MENU_PAUSE_ASSIST") &&
                            button.Label != Dialog.Clean("MENU_PAUSE_VARIANT") &&
                            button.Label != Dialog.Clean("MENU_PAUSE_OPTIONS") &&
                            button.Label != Dialog.Clean("MENU_PAUSE_MODOPTIONS") &&
                            button.Label != Dialog.Clean("Xaphan_0_0_intro_vignette_Settings") &&
                            button.Label != Dialog.Clean("Xaphan_0_Pause_Menu_ReturnTitle") &&
                            button.Label != Dialog.Clean("XaphanHelper_UI_GiveUpNM") &&
                            button.Label != Dialog.Clean("XaphanHelper_UI_GiveUpCM") &&
                            button.Label != Dialog.Clean("XaphanHelper_UI_GiveUpBSide") &&
                            button.Label != Dialog.Clean("XaphanHelper_UI_GiveUpCSide"))
                        {
                            IndexesToRemove.Add(menu.Items.IndexOf(item));
                        }
                    }
                }
                IndexesToRemove.Sort((a, b) => b.CompareTo(a));
                foreach (int index in IndexesToRemove)
                {
                    menu.Remove(menu.Items[index]);
                }
            }
        }

        private static void confirmSettingsMenu(Level level, int returnIndex)
        {
            TextMenu SettingsMenu = new();
            SoCMIntro.CreateSettingsMenu(SettingsMenu);
            SettingsMenu.OnPause = (SettingsMenu.OnESC = delegate
            {
                SettingsMenu.RemoveSelf();
                level.Paused = false;
                Engine.FreezeTimer = 0.15f;
                Audio.Play("event:/ui/game/unpause");
            });
            SettingsMenu.OnCancel = delegate
            {
                Audio.Play("event:/ui/main/button_back");
                SettingsMenu.RemoveSelf();
                level.Pause(returnIndex, minimal: false);
            };
            level.Add(SettingsMenu);
        }

        private static void confirmGiveUpCMMenu(Level level, int returnIndex)
        {
            ChallengeMote CMote = level.Tracker.GetEntity<ChallengeMote>();
            if (CMote != null)
            {
                level.Paused = true;
                TextMenu menu = new();
                menu.AutoScroll = false;
                menu.Position = new Vector2(Engine.Width / 2f, Engine.Height / 2f - 100f);
                menu.Add(new TextMenu.Header(Dialog.Clean(level.Session.GetFlag("boss_Normal_Mode") ? "XaphanHelper_UI_GiveUpNM_title" : "XaphanHelper_UI_GiveUpCM_title")));
                menu.Add(new TextMenu.Button(Dialog.Clean("menu_return_continue")).Pressed(delegate
                {
                    menu.RemoveSelf();
                    CMote.ManageUpgrades(level, true);
                    if (level.Session.GetFlag("boss_Normal_Mode"))
                    {
                        level.Session.SetFlag("boss_Normal_Mode_Given_Up", true);
                    }
                    else
                    {
                        level.Session.SetFlag("boss_Challenge_Mode_Given_Up", true);
                    }
                    level.Session.SetFlag("Boss_Defeated", true);
                    level.Session.SetFlag("boss_Normal_Mode", false);
                    level.Session.SetFlag("boss_Challenge_Mode", false);
                    level.Session.SetFlag("XaphanHelper_Prevent_Drone", false);
                    level.Paused = false;
                    Engine.FreezeTimer = 0.15f;
                }));
                menu.Add(new TextMenu.Button(Dialog.Clean("menu_return_cancel")).Pressed(delegate
                {
                    menu.OnCancel();
                }));
                menu.OnPause = (menu.OnESC = delegate
                {
                    menu.RemoveSelf();
                    level.Paused = false;
                    Engine.FreezeTimer = 0.15f;
                    Audio.Play("event:/ui/game/unpause");
                });
                menu.OnCancel = delegate
                {
                    Audio.Play("event:/ui/main/button_back");
                    menu.RemoveSelf();
                    level.Pause(returnIndex, minimal: false);
                };
                level.Add(menu);
            }
        }

        private static void confirmExitSideMenu(Level level, int returnIndex)
        {
            level.Paused = true;
            ReturnToASideHint returnHint = null;
            level.Add(returnHint = new ReturnToASideHint());
            TextMenu menu = new();
            menu.AutoScroll = false;
            menu.Position = new Vector2(Engine.Width / 2f, Engine.Height / 2f - 100f);
            menu.Add(new TextMenu.Header(Dialog.Clean("XaphanHelper_UI_GiveUp" + (level.Session.Area.Mode == AreaMode.BSide ? "B" : "C") + "Side_title")));
            menu.Add(new TextMenu.Button(Dialog.Clean("menu_return_continue")).Pressed(delegate
            {
                menu.RemoveSelf();
                returnHint.RemoveSelf();
                PlayerHasGolden = false;
                ChangingSide = true;
                Audio.SetMusic(null);
                Audio.SetAmbience(null);
                ModSaveData.LoadedPlayer = false;
                level.DoScreenWipe(false, delegate
                {
                    LevelEnter.Go(new Session(new AreaKey(level.Session.Area.ID, AreaMode.Normal))
                    {
                        Time = ModSaveData.SavedTime.ContainsKey(level.Session.Area.LevelSet) ? ModSaveData.SavedTime[level.Session.Area.LevelSet] : 0L,
                        DoNotLoad = ModSaveData.SavedNoLoadEntities.ContainsKey(level.Session.Area.LevelSet) ? ModSaveData.SavedNoLoadEntities[level.Session.Area.LevelSet] : new HashSet<EntityID>(),
                        Strawberries = ModSaveData.SavedSessionStrawberries.ContainsKey(level.Session.Area.LevelSet) ? ModSaveData.SavedSessionStrawberries[level.Session.Area.LevelSet] : new HashSet<EntityID>()
                    }
                    , fromSaveData: false);
                });
            }));
            menu.Add(new TextMenu.Button(Dialog.Clean("menu_return_cancel")).Pressed(delegate
            {
                menu.OnCancel();
            }));
            menu.OnPause = (menu.OnESC = delegate
            {
                menu.RemoveSelf();
                returnHint.RemoveSelf();
                level.Paused = false;
                Engine.FreezeTimer = 0.15f;
                Audio.Play("event:/ui/game/unpause");
            });
            menu.OnCancel = delegate
            {
                Audio.Play("event:/ui/main/button_back");
                menu.RemoveSelf();
                returnHint.RemoveSelf();
                level.Pause(returnIndex, minimal: false);
            };
            level.Add(menu);
        }

        public static void confirmRestartCampaign(Level level, int returnIndex, bool fromTitleScreen = false, TextMenu previousTitleMenu = null, SaveProgressDisplay progressDisplay = null)
        {
            if (!fromTitleScreen)
            {
                level.Paused = true;
            }
            RestartCampaignHint returnHint = null;
            level.Add(returnHint = new RestartCampaignHint(fromTitleScreen ? true : false));
            TextMenu menu = new();
            menu.AutoScroll = false;
            menu.Position = new Vector2(Engine.Width / 2f, Engine.Height / 2f + (fromTitleScreen ? 100f : -100f));
            menu.Add(new TextMenu.Header(Dialog.Clean(fromTitleScreen ? "Xaphan_0_EraseProgress" : "XaphanHelper_UI_RestartCampaign_title")));
            menu.Add(new TextMenu.Button(Dialog.Clean("menu_restart_continue")).Pressed(delegate
            {
                returnHint.RemoveSelf();
                Engine.TimeRate = 1f;
                menu.Focused = false;
                level.Session.InArea = false;
                Audio.SetMusic(null);
                Audio.BusStopAll("bus:/gameplay_sfx", immediate: true);
                level.DoScreenWipe(false, delegate
                {
                    Commands.Cmd_Clear_InGameMap(true, true);
                    Commands.Cmd_Clear_Warps();
                    Commands.Cmd_Remove_Upgrades();
                    Commands.Cmd_Reset_Collectables_Upgrades();

                    ModSaveData.SavedRoom.Remove(level.Session.Area.LevelSet);
                    ModSaveData.SavedChapter.Remove(level.Session.Area.LevelSet);
                    ModSaveData.SavedSpawn.Remove(level.Session.Area.LevelSet);
                    ModSaveData.SavedLightingAlphaAdd.Remove(level.Session.Area.LevelSet);
                    ModSaveData.SavedBloomBaseAdd.Remove(level.Session.Area.LevelSet);
                    ModSaveData.SavedCoreMode.Remove(level.Session.Area.LevelSet);
                    ModSaveData.SavedMusic.Remove(level.Session.Area.LevelSet);
                    ModSaveData.SavedAmbience.Remove(level.Session.Area.LevelSet);
                    if (ModSaveData.SavedNoLoadEntities.ContainsKey(level.Session.Area.LevelSet))
                    {
                        ModSaveData.SavedNoLoadEntities[level.Session.Area.LevelSet].Clear();
                    }
                    ModSaveData.SavedTime.Remove(level.Session.Area.LevelSet);
                    ModSaveData.SavedFromBeginning.Remove(level.Session.Area.LevelSet);
                    ModSaveData.SavedSesionFlags.Remove(level.Session.Area.LevelSet);
                    if (ModSaveData.SavedSessionStrawberries.ContainsKey(level.Session.Area.LevelSet))
                    {
                        ModSaveData.SavedSessionStrawberries[level.Session.Area.LevelSet].Clear();
                    }
                    if (ModSaveData.LightMode.ContainsKey(level.Session.Area.LevelSet))
                    {
                        ModSaveData.LightMode.Remove(level.Session.Area.LevelSet);
                    }
                    List<string> FlagsToRemove = new();
                    List<string> CutscenesToRemove = new();
                    List<string> GlobalFlagsToRemove = new();
                    foreach (string savedFlag in ModSaveData.SavedFlags)
                    {
                        if (savedFlag.Contains(level.Session.Area.LevelSet) && savedFlag != "Xaphan/0_Skip_Vignette")
                        {
                            FlagsToRemove.Add(savedFlag);
                        }
                    }
                    foreach (string cutscene in ModSaveData.WatchedCutscenes)
                    {
                        if (cutscene.Contains(level.Session.Area.LevelSet))
                        {
                            CutscenesToRemove.Add(cutscene);
                        }
                    }
                    foreach (string globalFlag in ModSaveData.GlobalFlags)
                    {
                        if (globalFlag.Contains(level.Session.Area.LevelSet))
                        {
                            GlobalFlagsToRemove.Add(globalFlag);
                        }
                    }
                    foreach (string value in FlagsToRemove)
                    {
                        ModSaveData.SavedFlags.Remove(value);
                    }
                    foreach (string value in CutscenesToRemove)
                    {
                        ModSaveData.WatchedCutscenes.Remove(value);
                    }
                    foreach (string value in GlobalFlagsToRemove)
                    {
                        ModSaveData.GlobalFlags.Remove(value);
                    }

                    // SoCM only

                    if (fromTitleScreen)
                    {
                        // For each chapter...

                        for (int i = SaveData.Instance.LevelSetStats.AreaOffset; i < SaveData.Instance.LevelSetStats.AreaOffset + SaveData.Instance.LevelSetStats.Areas.Count; i++)
                        {
                            // Remove Strawberries

                            for (int j = 0; j <= 2; j++)
                            {
                                HashSet<EntityID> deletedStrawberries = new();
                                foreach (EntityID strawberry in SaveData.Instance.Areas_Safe[i].Modes[j].Strawberries)
                                {
                                    deletedStrawberries.Add(strawberry);
                                }
                                if (deletedStrawberries.Count > 0)
                                {
                                    foreach (EntityID strawberry in deletedStrawberries)
                                    {
                                        SaveData.Instance.Areas_Safe[i].Modes[j].Strawberries.Remove(strawberry);
                                        AreaModeStats areaModeStats = SaveData.Instance.Areas_Safe[i].Modes[j];
                                        areaModeStats.Strawberries.Remove(strawberry);
                                        areaModeStats.TotalStrawberries--;
                                        SaveData.Instance.TotalStrawberries_Safe--;
                                    }
                                }
                            }

                            // Remove Hearts for all sides

                            for (int j = 0; j <= 2; j++)
                            {
                                SaveData.Instance.Areas_Safe[i].Modes[j].HeartGem = false;
                            }

                            // Remove Cassette

                            SaveData.Instance.Areas_Safe[i].Cassette = false;
                        }

                        // Remove Lorebook entries

                        ModSaveData.LorebookEntries.Clear();
                        ModSaveData.LorebookEntriesRead.Clear();

                        // Reset achievements

                        List<string> chapters = new();
                        foreach (string chapter in ModSaveData.VisitedChapters)
                        {
                            if (chapter.Contains("Xaphan/0"))
                            {
                                chapters.Add(chapter);
                            }
                        }

                        if (chapters.Count > 0)
                        {
                            foreach (string chapter in chapters)
                            {
                                ModSaveData.VisitedChapters.Remove(chapter);
                            }
                        }

                        ModSaveData.Achievements.Clear();

                        // Reset NoLoad entities

                        ModSaveData.PreGoldenDoNotLoad.Clear();
                        SaveData.Instance.CurrentSession_Safe.DoNotLoad.Clear();
                    }
                    LevelEnter.Go(new Session(new AreaKey(SaveData.Instance.LevelSetStats.AreaOffset + (MergeChaptersControllerKeepPrologue ? 1 : 0), AreaMode.Normal)), fromSaveData: false);
                });
                foreach (LevelEndingHook component in level.Tracker.GetComponents<LevelEndingHook>())
                {
                    if (component.OnEnd != null)
                    {
                        component.OnEnd();
                    }
                }
            }));
            menu.Add(new TextMenu.Button(Dialog.Clean("menu_return_cancel")).Pressed(delegate
            {
                menu.OnCancel();
            }));
            if (level.Session.Area.LevelSet == "Xaphan/0")
            {
                menu.Selection = 2;
            }
            menu.OnPause = (menu.OnESC = delegate
            {
                menu.RemoveSelf();
                returnHint.RemoveSelf();
                level.Paused = false;
                Engine.FreezeTimer = 0.15f;
                Audio.Play("event:/ui/game/unpause");
            });
            menu.OnCancel = delegate
            {
                returnHint.RemoveSelf();
                Audio.Play("event:/ui/main/button_back");
                menu.RemoveSelf();
                if (!fromTitleScreen)
                {
                    level.Pause(returnIndex, minimal: false);
                }
                else
                {
                    if (previousTitleMenu != null)
                    {
                        previousTitleMenu.Focused = previousTitleMenu.Visible = true;
                    }
                    if (progressDisplay != null)
                    {
                        progressDisplay.Visible = true;
                    }
                    level.FormationBackdrop.Display = false;
                }
            };
            level.Add(menu);
        }

        private static void confirmReturnToTitleMenu(Level level, int returnIndex)
        {
            level.Paused = true;
            RestartToTitleHint returnHint = null;
            level.Add(returnHint = new RestartToTitleHint());
            TextMenu menu = new();
            menu.AutoScroll = false;
            menu.Position = new Vector2(Engine.Width / 2f, Engine.Height / 2f - 100f);
            menu.Add(new TextMenu.Header(Dialog.Clean("Xaphan_0_Pause_Menu_ReturnTitle_title")));
            menu.Add(new TextMenu.Button(Dialog.Clean("menu_restart_continue")).Pressed(delegate
            {
                level.AutoSave();
                Engine.TimeRate = 1f;
                menu.Focused = false;
                level.DoScreenWipe(false, delegate
                {
                    onTitleScreen = false;
                    ReturnToTitleScreen(level);
                });
            }));
            menu.Add(new TextMenu.Button(Dialog.Clean("menu_return_cancel")).Pressed(delegate
            {
                menu.OnCancel();
            }));
            menu.OnPause = (menu.OnESC = delegate
            {
                menu.RemoveSelf();
                returnHint.RemoveSelf();
                level.Paused = false;
                Engine.FreezeTimer = 0.15f;
                Audio.Play("event:/ui/game/unpause");
            });
            menu.OnCancel = delegate
            {
                returnHint.RemoveSelf();
                Audio.Play("event:/ui/main/button_back");
                menu.RemoveSelf();
                level.Pause(returnIndex, minimal: false);
            };
            level.Add(menu);
        }

        public static void ReturnToTitleScreen(Level level)
        {
            level.Paused = true;
            onTitleScreen = false;
            MergedChaptersGoldenStrawberry.ResetProgression(level);
            SoCMTitleFromGame = true;
            SkipSoCMIntro = false;
            level.Session.SetFlag("XaphanHelper_Loaded_Player", false);
            ModSaveData.LoadedPlayer = false;
            long currentTime = level.Session.Time;
            LightManager manager = level.Tracker.GetEntity<LightManager>();
            if (manager != null)
            {
                ModSaveData.LightMode[level.Session.Area.LevelSet] = manager.RespawnMode;
            }
            LevelEnter.Go(new Session(new AreaKey(AreaData.Get("Xaphan/0/0-Prologue").ToKey(AreaMode.Normal).ID))
            {
                Time = currentTime,
                DoNotLoad = ModSaveData.SavedNoLoadEntities.ContainsKey(level.Session.Area.LevelSet) ? ModSaveData.SavedNoLoadEntities[level.Session.Area.LevelSet] : new HashSet<EntityID>(),
                Strawberries = ModSaveData.SavedSessionStrawberries.ContainsKey(level.Session.Area.LevelSet) ? ModSaveData.SavedSessionStrawberries[level.Session.Area.LevelSet] : new HashSet<EntityID>()
            }
            , fromSaveData: false);
        }

        private static void modILPlayerRender(ILContext il)
        {
            ILCursor cursor = new(il);

            if (cursor.TryGotoNext(instr => instr.MatchCallvirt<StateMachine>("get_State"), instr => instr.MatchLdcI4(19)))
            {
                cursor.Index++;
                cursor.EmitDelegate<Func<int, int>>(orig =>
                {
                    if (HeatController.determineifHeatController() || BreathManager.determineifBreathManager())
                    {
                        return 19;
                    }
                    return orig;
                });
            }
        }

        private IEnumerator modLevelEnterRoutine(On.Celeste.LevelEnter.orig_Routine orig, LevelEnter self)
        {
            if (startedAnySoCMChapter)
            {
                if (hasOldExtendedVariants && !displayedOldExtVariantsPostcard)
                {
                    self.Add(oldExtVariantsPostcard = new Postcard(Dialog.Get("postcard_Xaphan_OldExtVariants")));
                    yield return oldExtVariantsPostcard.DisplayRoutine();
                    displayedOldExtVariantsPostcard = true;
                    oldExtVariantsPostcard = null;
                }
            }
            IEnumerator origEnum = orig(self);
            while (origEnum.MoveNext()) yield return origEnum.Current;
        }

        private void modLevelEnterBeforeRender(On.Celeste.LevelEnter.orig_BeforeRender orig, LevelEnter self)
        {
            orig(self);
            if (oldExtVariantsPostcard != null) oldExtVariantsPostcard.BeforeRender();
        }

        private void onLevelEnterGo(On.Celeste.LevelEnter.orig_Go orig, Session session, bool fromSaveData)
        {
            if (!fromSaveData && session.StartedFromBeginning && session.Area.Mode == AreaMode.Normal && session.Area.GetSID() == "Xaphan/0/0-Prologue" && !ModSaveData.SavedFlags.Contains("Xaphan/0_Skip_Vignette") && SoCMVersion < new Version(3, 0, 0))
            {
                ModSaveData.SavedFlags.Add("Xaphan/0_Skip_Vignette");
                Engine.Scene = new SoCMIntroVignette(session);
            }
            else
            {
                orig.Invoke(session, fromSaveData);
            }
        }

        private void modPlayerCallDashEvents(On.Celeste.Player.orig_CallDashEvents orig, Player self)
        {
            if (GravityJacket.determineIfInWater())
            {
                Audio.Play("event:/char/madeline/water_dash_gen");
            }
            orig(self);
        }

        private void onLevelUpdate(On.Celeste.Level.orig_Update orig, Level self)
        {
            // SoCm Only

            if (startedAnySoCMChapter && SoCMVersion >= new Version(3, 0, 0))
            {
                // Prevent Timer to start when teleporting to Title Screen

                if ((self.Session.Level == "A-00" || self.Session.Level == "Intro") && !SkipSoCMIntro && self.Session.Area.Mode == 0)
                {
                    MInput.Disabled = !onTitleScreen;
                    self.TimerStopped = true;
                    if (self.Wipe != null && self.Wipe.GetType() == typeof(SpotlightWipe))
                    {
                        self.Wipe.Cancel();
                    }
                }

                // Skip Title Screen when going back to prologue if player entered the a level without triggering the Title Screen first (ex: After a crash)

                if (self.Session.Level != "A-00" && self.Session.Level != "Intro")
                {
                    SkipSoCMIntro = startedGame = true;
                }

                // Set Flag to keep screen Black before Title Screen

                self.Session.SetFlag("SoCM_startedGame", startedGame);
            }

            orig(self);
            Player player = self.Tracker.GetEntity<Player>();
            AreaKey area = self.Session.Area;
            MapData MapData = AreaData.Areas[area.ID].Mode[(int)area.Mode].MapData;

            if (useMergeChaptersController && MergeChaptersControllerMode == "Classic")
            {
                if (self.Session.Level == MapData.StartLevel().Name)
                {
                    if (!ModSaveData.Checkpoints.Contains(self.Session.Area.LevelSet + "|" + self.Session.Area.ChapterIndex) && area.ID != SaveData.Instance.LevelSetStats.AreaOffset)
                    {
                        ModSaveData.Checkpoints.Add(self.Session.Area.LevelSet + "|" + self.Session.Area.ChapterIndex);
                        self.AutoSave();
                    }
                }
            }

            // Redo the MergeChaptersController check to prevent issues if the helper is hot re-loaded
            // This should never occur in normal circonstances

            if (!useMergeChaptersControllerCheck)
            {
                MergeChaptersControllerCheck();
                useMergeChaptersControllerCheck = true;
            }

            // Add Metroid UI if using Metroid gameplay

            if (useMetroidGameplay)
            {
                if (self.Tracker.GetEntity<HealthDisplay>() == null)
                {
                    self.Add(new HealthDisplay(new Vector2(22f, 22f)));
                }
                if (self.Tracker.GetEntity<AmmoDisplay>() == null)
                {
                    self.Add(new AmmoDisplay(new Vector2(22f, 31f)));
                }
            }

            // Resume the countdown started from an other chapter when entering a chapter from the start

            if (ModSaveData.CountdownCurrentTime != -1)
            {
                if (self.Tracker.GetEntity<CountdownDisplay>() == null)
                {
                    self.Add(new CountdownDisplay(ModSaveData.CountdownCurrentTime, ModSaveData.CountdownShake, ModSaveData.CountdownExplode, true, ModSaveData.CountdownStartChapter, ModSaveData.CountdownStartRoom, ModSaveData.CountdownSpawn, ModSaveData.CountdownActiveFlag, ModSaveData.CountdownHideFlag, ModSaveData.CountdownEventsFlags)
                    {
                        PauseTimer = true
                    });
                }
            }

            // Add or remove the minimap if the chapter use the in-game Map and conditions are meet

            if (useIngameMap)
            {
                if (self.Tracker.CountEntities<MiniMap>() == 0 && minimapEnabled)
                {
                    minimapEnabled = false;
                }
                if (allRoomsUseTileController && (self.Session.Area.LevelSet == "Xaphan/0" ? ModSettings.SoCMShowMiniMap : ModSettings.ShowMiniMap) && ModSaveData.SavedFlags.Contains(self.Session.Area.LevelSet + "_Can_Open_Map") && !minimapEnabled)
                {
                    if (self.Tracker.GetEntity<MiniMap>() == null)
                    {
                        self.Add(new MiniMap(self));
                        minimapEnabled = true;
                    }
                }
                else
                {
                    if (self.Session.Area.LevelSet == "Xaphan/0" ? !ModSettings.SoCMShowMiniMap : !ModSettings.ShowMiniMap)
                    {
                        MiniMap minimap = self.Tracker.GetEntity<MiniMap>();
                        if (minimap != null)
                        {
                            minimapEnabled = false;
                            minimap.RemoveSelf();
                        }
                    }
                }
            }

            // Add the Popup UI if current chapter is SoCM

            if (self.Session.Area.LevelSet == "Xaphan/0" && ModSaveData.CanDisplayPopups)
            {
                if (self.Tracker.CountEntities<Popup>() == 0)
                {
                    self.Add(new Popup());
                }
            }

            // Check if the chapter use the in-game Map or upgrades and allows to open the corresponding screen

            if (useIngameMap || useUpgrades)
            {
                string Prefix = self.Session.Area.LevelSet;
                int chapterIndex = self.Session.Area.ChapterIndex == -1 ? 0 : self.Session.Area.ChapterIndex;
                string room = self.Session.Level;
                if (self.CanPause && (self.CanRetry || PlayerIsControllingRemoteDrone()) && player != null && player.StateMachine.State == Player.StNormal && player.Speed == Vector2.Zero && !self.Session.GetFlag("In_bossfight") && player.OnSafeGround && ModSettings.OpenMap.Pressed && !self.Session.GetFlag("Map_Opened"))
                {
                    if (useIngameMap && ModSaveData.VisitedRooms.Contains(Prefix + "/Ch" + chapterIndex + "/" + room) && CanOpenMap(self))
                    {
                        player.StateMachine.State = Player.StDummy;
                        player.DummyAutoAnimate = false;
                        if (!player.Sprite.CurrentAnimationID.Contains("idle") && !player.Sprite.CurrentAnimationID.Contains("edge"))
                        {
                            player.Sprite.Play("idle");
                        }
                        self.Add(new MapScreen(self, false));
                    }
                    else if (useUpgrades && !DisableStatusScreen)
                    {
                        player.StateMachine.State = Player.StDummy;
                        player.DummyAutoAnimate = false;
                        if (!player.Sprite.CurrentAnimationID.Contains("idle") && !player.Sprite.CurrentAnimationID.Contains("edge"))
                        {
                            player.Sprite.Play("idle");
                        }
                        self.Add(new StatusScreen(self, false));
                    }
                }
            }

            // Change starting chapter and room if using a Merge Chapter Controller

            if (useMergeChaptersController && MergeChaptersControllerMode != "Classic" && self.Session.Area.Mode == AreaMode.Normal)
            {
                if ((ModSaveData.LoadedPlayer || self.Session.GetFlag("XaphanHelper_Loaded_Player")) && !CanLoadPlayer) // If for some reason thoses value are true when entering the level, reset them to false. May happen if the game is not exited correctly
                {
                    ModSaveData.LoadedPlayer = false;
                    self.Session.SetFlag("XaphanHelper_Loaded_Player", false);
                }

                // SoCM Only

                if (startedAnySoCMChapter && SoCMVersion >= new Version(3, 0, 0) && !SkipSoCMIntro && self.Session.Area.ChapterIndex == -1)
                {
                    if (self.Session.Level != "Intro")
                    {
                        NoDroneSpawnSound = true;
                        self.Add(new TeleportCutscene(player, "Intro", Vector2.Zero, 0, 0, false, 0f, "Fade", skipFirstWipe: true));
                    }
                }
                else
                {
                    CanLoadPlayer = true;
                    if (!self.Session.GetFlag("XaphanHelper_Loaded_Player") && !ModSaveData.LoadedPlayer && !self.Paused)
                    {
                        bool hasInterlude = false;
                        int maxChapters = SaveData.Instance.LevelSetStats.Areas.Count;
                        for (int i = 0; i < maxChapters; i++)
                        {
                            if (AreaData.Areas[(SaveData.Instance.LevelSetStats.AreaOffset + i)].Interlude)
                            {
                                hasInterlude = true;
                                break;
                            }
                        }
                        MapData destinationMapData;
                        bool loadAtStartOfCampaign = false;
                        if (ModSaveData.SavedChapter.ContainsKey(self.Session.Area.LevelSet) && ModSaveData.SavedRoom.ContainsKey(self.Session.Area.LevelSet))
                        {
                            int chapter = (ModSaveData.SavedChapter[self.Session.Area.LevelSet] == -1 ? 0 : ModSaveData.SavedChapter[self.Session.Area.LevelSet]) - (hasInterlude ? 0 : 1);
                            destinationMapData = AreaData.Areas[SaveData.Instance.LevelSetStats.AreaOffset + chapter].Mode[0].MapData;
                            if (destinationMapData.Get(ModSaveData.SavedRoom[self.Session.Area.LevelSet]) == null)
                            {
                                loadAtStartOfCampaign = true;
                            }
                        }
                        self.Session.SetFlag("XaphanHelper_Loaded_Player", true);
                        self.Session.SetFlag("XaphanHelper_Changed_Start_Room", true);
                        if (!ModSaveData.SavedRoom.ContainsKey(self.Session.Area.LevelSet) || !ModSaveData.SavedChapter.ContainsKey(self.Session.Area.LevelSet)
                            || !ModSaveData.SavedSpawn.ContainsKey(self.Session.Area.LevelSet) || !ModSaveData.SavedLightingAlphaAdd.ContainsKey(self.Session.Area.LevelSet)
                            || !ModSaveData.SavedBloomBaseAdd.ContainsKey(self.Session.Area.LevelSet) || !ModSaveData.SavedCoreMode.ContainsKey(self.Session.Area.LevelSet)
                            || !ModSaveData.SavedMusic.ContainsKey(self.Session.Area.LevelSet) || !ModSaveData.SavedAmbience.ContainsKey(self.Session.Area.LevelSet)
                            || !ModSaveData.SavedNoLoadEntities.ContainsKey(self.Session.Area.LevelSet) || !ModSaveData.SavedTime.ContainsKey(self.Session.Area.LevelSet)
                            || !ModSaveData.SavedFromBeginning.ContainsKey(self.Session.Area.LevelSet) || !ModSaveData.SavedSesionFlags.ContainsKey(self.Session.Area.LevelSet)
                            || !ModSaveData.SavedSessionStrawberries.ContainsKey(self.Session.Area.LevelSet) || (MergeChaptersControllerKeepPrologue && self.Session.Area.ID == SaveData.Instance.LevelSetStats.AreaOffset)
                            || loadAtStartOfCampaign)
                        {
                            ModSaveData.LoadedPlayer = true;
                            ModSaveData.CanDisplayPopups = true;

                            // SoCM Only

                            if (startedAnySoCMChapter && SoCMVersion >= new Version(3, 0, 0))
                            {
                                self.Add(new TeleportCutscene(player, "A-00", Vector2.Zero, 0, 0, false, 0f, "Spotlight", wipeDuration: 1.8f, skipFirstWipe: true));
                            }
                        }
                        else if (ModSaveData.SavedChapter[self.Session.Area.LevelSet] == self.Session.Area.ChapterIndex)
                        {
                            string[] sessionFlags = ModSaveData.SavedSesionFlags[self.Session.Area.LevelSet].Split(',');
                            self.Session.FirstLevel = false;
                            self.Session.LightingAlphaAdd = ModSaveData.SavedLightingAlphaAdd[self.Session.Area.LevelSet];
                            self.Session.BloomBaseAdd = ModSaveData.SavedBloomBaseAdd[self.Session.Area.LevelSet];
                            self.Session.CoreMode = ModSaveData.SavedCoreMode[self.Session.Area.LevelSet];
                            self.Session.DoNotLoad = ModSaveData.SavedNoLoadEntities[self.Session.Area.LevelSet];
                            self.Session.Time = ModSaveData.SavedTime[self.Session.Area.LevelSet];
                            self.Session.StartedFromBeginning = ModSaveData.SavedFromBeginning[self.Session.Area.LevelSet];
                            if (SaveData.Instance != null)
                            {
                                SaveData.Instance.CurrentSession.LightingAlphaAdd = ModSaveData.SavedLightingAlphaAdd[self.Session.Area.LevelSet];
                                SaveData.Instance.CurrentSession.BloomBaseAdd = ModSaveData.SavedBloomBaseAdd[self.Session.Area.LevelSet];
                                SaveData.Instance.CurrentSession.CoreMode = ModSaveData.SavedCoreMode[self.Session.Area.LevelSet];
                            }
                            self.Session.Audio.Music.Event = ModSaveData.SavedMusic[self.Session.Area.LevelSet];
                            self.Session.Audio.Ambience.Event = ModSaveData.SavedAmbience[self.Session.Area.LevelSet];
                            self.Session.Audio.Apply(forceSixteenthNoteHack: false);
                            foreach (string flag in sessionFlags)
                            {
                                if (!flag.Contains("XaphanHelper_StatFlag_") || (flag.Contains("XaphanHelper_StatFlag_") && flag.Contains("-Visited")))
                                {
                                    self.Session.SetFlag(flag, true);
                                }
                            }
                            self.Session.Strawberries = ModSaveData.SavedSessionStrawberries[self.Session.Area.LevelSet];
                            ModSaveData.LoadedPlayer = true;
                            self.Add(new TeleportCutscene(player, ModSaveData.SavedRoom[self.Session.Area.LevelSet], MergeChaptersControllerMode == "Warps" ? Vector2.Zero : ModSaveData.SavedSpawn[self.Session.Area.LevelSet], 0, 0, true, 0f, "Fade", skipFirstWipe: true, respawnAnim: true, useLevelWipe: true, spawnPositionX: MergeChaptersControllerMode == "Warps" ? ModSaveData.SavedSpawn[self.Session.Area.LevelSet].X : 0f, spawnPositionY: MergeChaptersControllerMode == "Warps" ? ModSaveData.SavedSpawn[self.Session.Area.LevelSet].Y : 0f));
                        }
                        else
                        {
                            LevelEnter.Go(new Session(new AreaKey(SaveData.Instance.LevelSetStats.AreaOffset + (ModSaveData.SavedChapter[self.Session.Area.LevelSet] == -1 ? 0 : ModSaveData.SavedChapter[self.Session.Area.LevelSet] - (hasInterlude ? 0 : 1))))
                            {
                                Time = ModSaveData.SavedTime.ContainsKey(self.Session.Area.LevelSet) ? ModSaveData.SavedTime[self.Session.Area.LevelSet] : 0L,
                                DoNotLoad = ModSaveData.SavedNoLoadEntities.ContainsKey(self.Session.Area.LevelSet) ? ModSaveData.SavedNoLoadEntities[self.Session.Area.LevelSet] : new HashSet<EntityID>(),
                                Strawberries = ModSaveData.SavedSessionStrawberries.ContainsKey(self.Session.Area.LevelSet) ? ModSaveData.SavedSessionStrawberries[self.Session.Area.LevelSet] : new HashSet<EntityID>()
                            }, fromSaveData: false);
                        }
                    }

                    // Save the room as the one that the player must load into when starting the campaign if using a MergeChaptersController with mode set to Rooms

                    else if (useMergeChaptersController && MergeChaptersControllerMode == "Rooms" && !PlayerHasGolden && !self.Frozen && self.Tracker.GetEntity<CountdownDisplay>() == null && !TriggeredCountDown && self.Tracker.GetEntity<Player>() != null && self.Tracker.GetEntity<Player>().StateMachine.State != Player.StDummy && !((MergeChaptersControllerKeepPrologue && self.Session.Area.ID == SaveData.Instance.LevelSetStats.AreaOffset)) && (startedAnySoCMChapter ? self.Session.Level != "Intro" : true))
                    {
                        //ModSaveData.LoadedPlayer = true;
                        if (!ModSaveData.SavedChapter.ContainsKey(self.Session.Area.LevelSet))
                        {
                            ModSaveData.SavedChapter.Add(self.Session.Area.LevelSet, self.Session.Area.ChapterIndex);
                        }
                        else
                        {
                            if (ModSaveData.SavedChapter[self.Session.Area.LevelSet] != self.Session.Area.ChapterIndex)
                            {
                                ModSaveData.SavedChapter[self.Session.Area.LevelSet] = self.Session.Area.ChapterIndex;
                            }
                        }
                        if (string.IsNullOrEmpty(ModSaveData.CountdownStartRoom))
                        {
                            if (!ModSaveData.SavedRoom.ContainsKey(self.Session.Area.LevelSet))
                            {
                                ModSaveData.SavedRoom.Add(self.Session.Area.LevelSet, self.Session.Level);
                            }
                            else
                            {
                                if (ModSaveData.SavedRoom[self.Session.Area.LevelSet] != self.Session.Level)
                                {
                                    ModSaveData.SavedRoom[self.Session.Area.LevelSet] = self.Session.Level;
                                }
                            }
                        }

                        if (self.Session.RespawnPoint == null)
                        {
                            self.Session.RespawnPoint = Vector2.Zero;
                        }
                        if (!ModSaveData.SavedSpawn.ContainsKey(self.Session.Area.LevelSet))
                        {
                            ModSaveData.SavedSpawn.Add(self.Session.Area.LevelSet, (Vector2)self.Session.RespawnPoint - new Vector2(self.Bounds.Left, self.Bounds.Top));
                        }
                        else
                        {
                            if (ModSaveData.SavedSpawn[self.Session.Area.LevelSet] != (Vector2)self.Session.RespawnPoint - new Vector2(self.Bounds.Left, self.Bounds.Top))
                            {
                                ModSaveData.SavedSpawn[self.Session.Area.LevelSet] = (Vector2)self.Session.RespawnPoint - new Vector2(self.Bounds.Left, self.Bounds.Top);
                            }
                        }
                        if (!ModSaveData.SavedLightingAlphaAdd.ContainsKey(self.Session.Area.LevelSet))
                        {
                            ModSaveData.SavedLightingAlphaAdd.Add(self.Session.Area.LevelSet, self.Lighting.Alpha - self.BaseLightingAlpha);
                        }
                        else
                        {
                            if (ModSaveData.SavedLightingAlphaAdd[self.Session.Area.LevelSet] != self.Lighting.Alpha - self.BaseLightingAlpha)
                            {
                                ModSaveData.SavedLightingAlphaAdd[self.Session.Area.LevelSet] = self.Lighting.Alpha - self.BaseLightingAlpha;
                            }
                        }
                        if (!ModSaveData.SavedBloomBaseAdd.ContainsKey(self.Session.Area.LevelSet))
                        {
                            ModSaveData.SavedBloomBaseAdd.Add(self.Session.Area.LevelSet, self.Bloom.Base - AreaData.Get(self).BloomBase);
                        }
                        else
                        {
                            if (ModSaveData.SavedBloomBaseAdd[self.Session.Area.LevelSet] != self.Bloom.Base - AreaData.Get(self).BloomBase)
                            {
                                ModSaveData.SavedBloomBaseAdd[self.Session.Area.LevelSet] = self.Bloom.Base - AreaData.Get(self).BloomBase;
                            }
                        }
                        if (!ModSaveData.SavedCoreMode.ContainsKey(self.Session.Area.LevelSet))
                        {
                            ModSaveData.SavedCoreMode.Add(self.Session.Area.LevelSet, self.Session.CoreMode);
                        }
                        else
                        {
                            if (ModSaveData.SavedCoreMode[self.Session.Area.LevelSet] != self.Session.CoreMode)
                            {
                                ModSaveData.SavedCoreMode[self.Session.Area.LevelSet] = self.Session.CoreMode;
                            }
                        }
                        if (!ModSaveData.SavedMusic.ContainsKey(self.Session.Area.LevelSet))
                        {
                            ModSaveData.SavedMusic.Add(self.Session.Area.LevelSet, self.Session.Audio.Music.Event);
                        }
                        else
                        {
                            if (ModSaveData.SavedMusic[self.Session.Area.LevelSet] != self.Session.Audio.Music.Event)
                            {
                                ModSaveData.SavedMusic[self.Session.Area.LevelSet] = self.Session.Audio.Music.Event;
                            }
                        }
                        if (!ModSaveData.SavedAmbience.ContainsKey(self.Session.Area.LevelSet))
                        {
                            ModSaveData.SavedAmbience.Add(self.Session.Area.LevelSet, self.Session.Audio.Ambience.Event);
                        }
                        else
                        {
                            if (ModSaveData.SavedAmbience[self.Session.Area.LevelSet] != self.Session.Audio.Ambience.Event)
                            {
                                ModSaveData.SavedAmbience[self.Session.Area.LevelSet] = self.Session.Audio.Ambience.Event;
                            }
                        }
                        if (!ModSaveData.SavedNoLoadEntities.ContainsKey(self.Session.Area.LevelSet))
                        {
                            ModSaveData.SavedNoLoadEntities.Add(self.Session.Area.LevelSet, self.Session.DoNotLoad);
                        }
                        else
                        {
                            if (ModSaveData.SavedNoLoadEntities[self.Session.Area.LevelSet] != self.Session.DoNotLoad)
                            {
                                ModSaveData.SavedNoLoadEntities[self.Session.Area.LevelSet] = self.Session.DoNotLoad;
                            }
                        }
                        if (!ModSaveData.SavedFromBeginning.ContainsKey(self.Session.Area.LevelSet))
                        {
                            ModSaveData.SavedFromBeginning.Add(self.Session.Area.LevelSet, self.Session.StartedFromBeginning);
                        }
                        else
                        {
                            if (ModSaveData.SavedFromBeginning[self.Session.Area.LevelSet] != self.Session.StartedFromBeginning)
                            {
                                ModSaveData.SavedFromBeginning[self.Session.Area.LevelSet] = self.Session.StartedFromBeginning;
                            }
                        }
                        string sessionFlags = "";
                        foreach (string flag in self.Session.Flags)
                        {
                            if (sessionFlags == "")
                            {
                                sessionFlags += flag;
                            }
                            else
                            {
                                sessionFlags += "," + flag;
                            }
                        }
                        if (!ModSaveData.SavedSesionFlags.ContainsKey(self.Session.Area.LevelSet))
                        {
                            ModSaveData.SavedSesionFlags.Add(self.Session.Area.LevelSet, sessionFlags);
                        }
                        else
                        {
                            if (ModSaveData.SavedSesionFlags[self.Session.Area.LevelSet] != sessionFlags)
                            {
                                ModSaveData.SavedSesionFlags[self.Session.Area.LevelSet] = sessionFlags;
                            }
                        }
                        if (!ModSaveData.SavedSessionStrawberries.ContainsKey(self.Session.Area.LevelSet))
                        {
                            ModSaveData.SavedSessionStrawberries.Add(self.Session.Area.LevelSet, self.Session.Strawberries);
                        }
                        else
                        {
                            if (ModSaveData.SavedSessionStrawberries[self.Session.Area.LevelSet] != self.Session.Strawberries)
                            {
                                ModSaveData.SavedSessionStrawberries[self.Session.Area.LevelSet] = self.Session.Strawberries;
                            }
                        }
                    }
                }

                if (useMergeChaptersController && !((MergeChaptersControllerKeepPrologue && self.Session.Area.ID == SaveData.Instance.LevelSetStats.AreaOffset)))
                {
                    if (!ModSaveData.SavedTime.ContainsKey(self.Session.Area.LevelSet))
                    {
                        ModSaveData.SavedTime.Add(self.Session.Area.LevelSet, self.Session.Time);
                    }
                    else
                    {
                        if (ModSaveData.SavedTime[self.Session.Area.LevelSet] != self.Session.Time)
                        {
                            ModSaveData.SavedTime[self.Session.Area.LevelSet] = self.Session.Time;
                        }
                    }
                }

                if (useMergeChaptersController && MergedChaptersGoldenStrawberry.Grabbed && !self.Session.GrabbedGolden)
                {
                    EntityData entityData = new();
                    entityData.Position = player.Position;
                    entityData.ID = MergedChaptersGoldenStrawberry.ID;
                    entityData.Name = "goldenBerry";
                    Strawberry strawberry = new(gid: new EntityID(MergedChaptersGoldenStrawberry.StartRoom, entityData.ID), data: entityData, offset: Vector2.Zero);
                    player.SceneAs<Level>().Add(strawberry);
                }
            }

            // Change chapter starting room if player used a warp or elevator

            if (!self.Session.GetFlag("XaphanHelper_Changed_Start_Room"))
            {
                self.Session.SetFlag("XaphanHelper_Changed_Start_Room", true);

                // Change level Wipe

                if (!string.IsNullOrEmpty(ModSaveData.Wipe))
                {
                    if (self.Wipe != null && self.Tracker.GetEntities<WarpScreen>().Count == 0)
                    {
                        self.Wipe.Cancel();
                        self.Add(GetWipe(self, true));
                    }
                }
                if (string.IsNullOrEmpty(ModSaveData.DestinationRoom))
                {
                    if (self.Session.StartedFromBeginning)
                    {
                        string currentRoom = self.Session.Level;
                        ScreenWipe Wipe = null;
                        foreach (LevelData level in MapData.Levels)
                        {
                            if (level.Name == currentRoom)
                            {
                                foreach (EntityData entity in level.Entities)
                                {
                                    if (entity.Name == "XaphanHelper/Elevator")
                                    {
                                        Wipe = new FadeWipe(self, true)
                                        {
                                            Duration = 1.35f
                                        };
                                        break;
                                    }
                                }
                                break;
                            }
                        }
                        if (Wipe != null)
                        {
                            self.Add(Wipe);
                        }
                        else
                        {
                            if (PlayerLostGolden)
                            {
                                self.DoScreenWipe(true);
                                PlayerLostGolden = false;
                            }
                        }
                    }
                }
                else
                {
                    self.Add(GetWipe(self, true));
                    if (!ModSaveData.ConsiderBeginning && !useMergeChaptersController)
                    {
                        self.Session.StartedFromBeginning = false;
                    }
                    ModSaveData.ConsiderBeginning = false;
                    string destinationRoom = ModSaveData.DestinationRoom;
                    Vector2 spawn = ModSaveData.Spawn;
                    string wipe = ModSaveData.Wipe;
                    float wipeDuration = ModSaveData.WipeDuration;
                    bool fromElevator = ModSaveData.TeleportFromElevator;
                    ModSaveData.DestinationRoom = "";
                    ModSaveData.Spawn = new Vector2();
                    ModSaveData.Wipe = "";
                    ModSaveData.WipeDuration = 0f;
                    ModSaveData.TeleportFromElevator = false;
                    if ((self = (Engine.Scene as Level)) != null)
                    {
                        if (string.IsNullOrEmpty(destinationRoom))
                        {
                            self.Add(new MiniTextbox("XaphanHelper_room_name_empty"));
                            return;
                        }
                        if (self.Session.MapData.Get(destinationRoom) == null)
                        {
                            self.Add(new MiniTextbox("XaphanHelper_room_not_exist"));
                            return;
                        }
                    }
                    if (TeleportBackFromDrone)
                    {
                        self.Add(new TransitionBlackEffect(true));
                        self.TimerHidden = true;
                        bool faceLeft = false;
                        if (ModSaveData.fakePlayerFacing[self.Session.Area.LevelSet] == Facings.Left)
                        {
                            faceLeft = true;
                        }
                        self.Add(new TeleportCutscene(player, ModSaveData.droneStartRoom[self.Session.Area.LevelSet], Vector2.Zero, 0, 0, true, 0f, "Fade", wipeDuration, fromElevator, wakeUpAnim: true, spawnPositionX: ModSaveData.fakePlayerPosition[self.Session.Area.LevelSet].X, spawnPositionY: ModSaveData.fakePlayerPosition[self.Session.Area.LevelSet].Y, faceLeft: faceLeft, drone: true));
                    }
                    else
                    {
                        self.Add(new TeleportCutscene(player, destinationRoom, spawn, 0, 0, true, 0f, wipe, wipeDuration, fromElevator, true, useLevelWipe: ModSaveData.CountdownUseLevelWipe));
                    }
                    ModSaveData.CountdownUseLevelWipe = false;
                }
            }

            // Check tiles player has visited

            if (useIngameMap)
            {
                string Prefix = self.Session.Area.LevelSet;
                int chapterIndex = self.Session.Area.ChapterIndex == -1 ? 0 : self.Session.Area.ChapterIndex;
                if (player != null && !self.Paused && !self.Transitioning)
                {
                    Vector2 playerPosition = new(Math.Min((float)Math.Floor((player.Center.X - self.Bounds.X) / 320f), (float)Math.Round(self.Bounds.Width / 320f, MidpointRounding.AwayFromZero) - 1), Math.Min((float)Math.Floor((player.Center.Y - self.Bounds.Y) / 184f), (float)Math.Round(self.Bounds.Height / 184f, MidpointRounding.AwayFromZero) + 1));
                    if (playerPosition.X < 0)
                    {
                        playerPosition.X = 0;
                    }
                    if (playerPosition.Y < 0)
                    {
                        playerPosition.Y = 0;
                    }
                    if (!ModSaveData.VisitedRoomsTiles.Contains(Prefix + "/Ch" + chapterIndex + "/" + self.Session.Level + "-" + playerPosition.X + "-" + playerPosition.Y) && (!string.IsNullOrEmpty(ModSaveData.DestinationRoom) ? self.Session.Level == ModSaveData.DestinationRoom : true) && (useMergeChaptersController ? ModSaveData.LoadedPlayer : true))
                    {
                        ModSaveData.VisitedRoomsTiles.Add(Prefix + "/Ch" + chapterIndex + "/" + self.Session.Level + "-" + playerPosition.X + "-" + playerPosition.Y);
                        InGameMapRoomController roomController = self.Tracker.GetEntity<InGameMapRoomController>();
                        List<Entity> tilesControllers = self.Tracker.GetEntities<InGameMapTilesController>();
                        if (CheckIfTileIsValid(self.Session.Level, playerPosition, tilesControllers))
                        {
                            if (StatsFlags.CurrentTiles != null)
                            {
                                StatsFlags.CurrentTiles[chapterIndex] += 1;
                            }
                            if (StatsFlags.CurrentSubAreaTiles != null)
                            {
                                if (StatsFlags.CurrentSubAreaTiles != null && roomController != null)
                                {
                                    int subAreaIndex = roomController.Data.Int("subAreaIndex");
                                    if (StatsFlags.CurrentSubAreaTiles[chapterIndex].ContainsKey(subAreaIndex))
                                    {
                                        StatsFlags.CurrentSubAreaTiles[chapterIndex][subAreaIndex]++;
                                    }
                                    else
                                    {
                                        StatsFlags.CurrentSubAreaTiles[chapterIndex].Add(subAreaIndex, 1);
                                    }
                                }
                            }
                        }
                        if (self.Session.Area.LevelSet == "Xaphan/0" ? ModSettings.SoCMShowMiniMap : ModSettings.ShowMiniMap)
                        {
                            MapDisplay mapDisplay = self.Tracker.GetEntity<MapDisplay>();
                            if (mapDisplay != null)
                            {
                                mapDisplay.GenerateTiles();
                                mapDisplay.GenerateIcons();
                            }
                        }
                    }
                }
            }

            // Detect if player is on top of a slope

            foreach (PlayerPlatform playerPlatform in self.Tracker.GetEntities<PlayerPlatform>())
            {
                if (player != null && playerPlatform.HasPlayerOnTop())
                {
                    onSlope = true;
                    onSlopeDir = playerPlatform.Side == "Right" ? 1 : -1;
                    onSlopeGentle = playerPlatform.Gentle;
                    onSlopeTop = playerPlatform.slopeTop;
                    onSlopeAffectPlayerSpeed = playerPlatform.AffectPlayerSpeed;
                    break;
                }
                else
                {
                    onSlope = false;
                    onSlopeDir = 0;
                    onSlopeGentle = false;
                    onSlopeTop = 0;
                    onSlopeAffectPlayerSpeed = false;
                }
            }

            // SoCM Only

            if (self.Session.Area.LevelSet == "Xaphan/0")
            {
                // Under liquids rooms

                foreach (Liquid liquid in self.Tracker.GetEntities<Liquid>())
                {
                    if (liquid.Position == new Vector2(self.Bounds.Left, self.Bounds.Top))
                    {
                        liquid.Collider = new Hitbox(self.Bounds.Width, self.Bounds.Height);
                        if (liquid.liquidType == "water")
                        {
                            liquid.Displacement.RemoveSelf();
                            liquid.grid = new bool[(int)(liquid.Collider.Width / 8f), (int)(liquid.Collider.Height / 8f)];
                            liquid.CheckSolidsForDisplacement();
                            liquid.Add(liquid.Displacement = new DisplacementRenderHook(liquid.RenderDisplacement));
                        }
                        break;
                    }
                }
                
                // Backward saves compatibility

                SaveUpdater.RemoveUpgrades();
                SaveUpdater.GiveUpgrades();
            }
        }

        private bool CheckIfTileIsValid(string room, Vector2 playerPosition, List<Entity> TilesControllers)
        {
            bool isValid = false;
            List<InGameMapTilesControllerData> TilesControllerData = new();
            foreach (Entity tileController in TilesControllers)
            {
                InGameMapTilesController controller = tileController as InGameMapTilesController;
                TilesControllerData.Add(new InGameMapTilesControllerData(0, room, controller.Data.Attr("tile0Cords"), controller.Data.Attr("tile0"), controller.Data.Attr("tile1Cords"), controller.Data.Attr("tile1"), controller.Data.Attr("tile2Cords"), controller.Data.Attr("tile2"),
                                            controller.Data.Attr("tile3Cords"), controller.Data.Attr("tile3"), controller.Data.Attr("tile4Cords"), controller.Data.Attr("tile4"), controller.Data.Attr("tile5Cords"), controller.Data.Attr("tile5"), controller.Data.Attr("tile6Cords"), controller.Data.Attr("tile6"),
                                            controller.Data.Attr("tile7Cords"), controller.Data.Attr("tile7"), controller.Data.Attr("tile8Cords"), controller.Data.Attr("tile8"), controller.Data.Attr("tile9Cords"), controller.Data.Attr("tile9"), controller.Data.Attr("display")));
            }
            foreach (InGameMapTilesControllerData tileControllerData in TilesControllerData)
            {
                for (int i = 0; i <= 9; i++)
                {
                    string tileCords = tileControllerData.GetTileCords(i);
                    if (tileCords == (playerPosition.X + "-" + playerPosition.Y))
                    {
                        string tile = tileControllerData.GetTile(i);
                        if (tile != "None" && tile != "ElevatorShaft" && !tile.Contains("Arrow") && !tile.Contains("Connection"))
                        {
                            isValid = true;
                            break;
                        }
                    }
                }
            }
            return isValid;
        }

        private void modStrawberryOnPlayer(On.Celeste.Strawberry.orig_OnPlayer orig, Strawberry self, Player player)
        {
            if (!PlayerHasGolden)
            {
                Level level = player.SceneAs<Level>();
                if (self.Golden)
                {
                    PlayerHasGolden = true;
                }
                if (useUpgrades)
                {
                    EntityData UpgradeController = new();
                    AreaKey area = level.Session.Area;
                    MapData MapData = AreaData.Areas[area.ID].Mode[(int)area.Mode].MapData;
                    foreach (LevelData levelData in MapData.Levels)
                    {
                        foreach (EntityData entity in levelData.Entities)
                        {
                            if (entity.Name == "XaphanHelper/UpgradeController")
                            {
                                UpgradeController = entity;
                                break;
                            }
                        }
                    }
                    if (PlayerHasGolden && (!useMergeChaptersController || (useMergeChaptersController && MergedChaptersGoldenStrawberry.ResetFlags))) // When the player grab a golden berry
                    {
                        if (useMergeChaptersController)
                        {
                            MergedChaptersGoldenStrawberry.ResetFlags = false;
                        }
                        else
                        {
                            foreach (string flag in ModSaveData.SavedFlags)
                            {
                                string Prefix = level.Session.Area.LevelSet;
                                int chapterIndex = level.Session.Area.ChapterIndex == -1 ? 0 : level.Session.Area.ChapterIndex;
                                string[] str = flag.Split('_');
                                if (str[0] == Prefix && str[1] == "Ch" + chapterIndex)
                                {
                                    string toRemove = str[0] + "_" + str[1] + "_";
                                    string result = string.Empty;
                                    int i = flag.IndexOf(toRemove);
                                    if (i >= 0)
                                    {
                                        result = flag.Remove(i, toRemove.Length);
                                    }
                                    level.Session.SetFlag(result, false);
                                }
                            }
                        }

                        if (!UpgradeController.Bool("noResetUpgrades"))
                        {
                            // Reset upgrades

                            level.Session.SetFlag("Upgrade_PowerGrip", false);
                            ModSettings.PowerGrip = false;
                            level.Session.SetFlag("Upgrade_ClimbingKit", false);
                            ModSettings.ClimbingKit = false;
                            level.Session.SetFlag("Upgrade_SpiderMagnet", false);
                            ModSettings.SpiderMagnet = false;
                            level.Session.SetFlag("Upgrade_DroneTeleport", false);
                            ModSettings.DroneTeleport = false;
                            level.Session.SetFlag("Upgrade_JumpBoost", false);
                            ModSettings.JumpBoost = false;
                            level.Session.SetFlag("Upgrade_ScrewAttack", false);
                            ModSettings.ScrewAttack = false;
                            level.Session.SetFlag("Upgrade_VariaJacket", false);
                            ModSettings.VariaJacket = false;
                            level.Session.SetFlag("Upgrade_GravityJacket", false);
                            ModSettings.GravityJacket = false;
                            level.Session.SetFlag("Upgrade_Bombs", false);
                            ModSettings.Bombs = false;
                            level.Session.SetFlag("Upgrade_MegaBombs", false);
                            ModSettings.MegaBombs = false;
                            level.Session.SetFlag("Upgrade_RemoteDrone", false);
                            ModSettings.RemoteDrone = false;
                            level.Session.SetFlag("Upgrade_GoldenFeather", false);
                            ModSettings.GoldenFeather = false;
                            level.Session.SetFlag("Upgrade_Binoculars", false);
                            ModSettings.Binoculars = false;
                            level.Session.SetFlag("Upgrade_EtherealDash", false);
                            ModSettings.EtherealDash = false;
                            level.Session.SetFlag("Upgrade_PortableStation", false);
                            ModSettings.PortableStation = false;
                            level.Session.SetFlag("Upgrade_PulseRadar", false);
                            ModSettings.PulseRadar = false;
                            level.Session.SetFlag("Upgrade_DashBoots", false);
                            ModSettings.DashBoots = false;
                            level.Session.SetFlag("Upgrade_SpaceJump", false);
                            ModSettings.SpaceJump = 1;
                            level.Session.SetFlag("Upgrade_HoverJet", false);
                            ModSettings.HoverJet = false;
                            level.Session.SetFlag("Upgrade_LightningDash", false);
                            ModSettings.LightningDash = false;
                            level.Session.SetFlag("Upgrade_LongBeam", false);
                            ModSettings.LongBeam = false;
                            level.Session.SetFlag("Upgrade_IceBeam", false);
                            ModSettings.IceBeam = false;
                            level.Session.SetFlag("Upgrade_WaveBeam", false);
                            ModSettings.WaveBeam = false;
                            level.Session.SetFlag("Upgrade_MissilesModule", false);
                            ModSettings.MissilesModule = false;
                            level.Session.SetFlag("Upgrade_SuperMissilesModule", false);
                            ModSettings.SuperMissilesModule = false;

                            // Give allowed starting upgrades

                            bool goldenPowerGrip = UpgradeController.Bool("goldenStartWithPowerGrip");
                            bool goldenClimbingKit = UpgradeController.Bool("goldenStartWithClimbingKit");
                            bool goldenSpiderMagnet = UpgradeController.Bool("goldenStartWithSpiderMagnet");
                            bool goldenDroneTeleport = UpgradeController.Bool("goldenStartWithDroneTeleport");
                            bool goldenJumpBoost = UpgradeController.Bool("goldenStartWithJumpBoost");
                            bool goldenScrewAttack = UpgradeController.Bool("goldenStartWithScrewAttack");
                            bool goldenVariaJacket = UpgradeController.Bool("goldenStartWithVariaJacket");
                            bool goldenGravityJacket = UpgradeController.Bool("goldenStartWithGravityJacket");
                            bool goldenBombs = UpgradeController.Bool("goldenStartWithBombs");
                            bool goldenMegaBombs = UpgradeController.Bool("goldenStartWithMegaBombs");
                            bool goldenRemoteDrone = UpgradeController.Bool("goldenStartWithRemoteDrone");
                            bool goldenGoldenFeather = UpgradeController.Bool("goldenStartWithGoldenFeather");
                            bool goldenBinoculars = UpgradeController.Bool("goldenStartWithBinoculars");
                            bool goldenEtherealDash = UpgradeController.Bool("goldenStartWithEtherealDash");
                            bool goldenPortableStation = UpgradeController.Bool("goldenStartWithPortableStation");
                            bool goldenPulseRadar = UpgradeController.Bool("goldenStartWithPulseRadar");
                            bool goldenDashBoots = UpgradeController.Bool("goldenStartWithDashBoots");
                            bool goldenSpaceJump = UpgradeController.Bool("goldenStartWithSpaceJump");
                            bool goldenHoverJet = UpgradeController.Bool("goldenStartWithHoverJet");
                            bool goldenLightningDash = UpgradeController.Bool("goldenStartWithLightningDash");
                            bool goldenLongBeam = UpgradeController.Bool("goldenStartWithLongBeam");
                            bool goldenIceBeam = UpgradeController.Bool("goldenStartWithIceBeam");
                            bool goldenWaveBeam = UpgradeController.Bool("goldenStartWithWaveBeam");
                            bool goldenMissilesModule = UpgradeController.Bool("goldenStartWithMissilesModule");
                            bool goldenSuperMissilesModule = UpgradeController.Bool("goldenStartWithSuperMissilesModule");

                            if (goldenPowerGrip)
                            {
                                ModSettings.PowerGrip = true;
                                level.Session.SetFlag("Upgrade_PowerGrip", true);
                                ModSaveData.SavedFlags.Add(level.Session.Area.LevelSet + "_Upgrade_PowerGrip");
                            }
                            if (goldenClimbingKit)
                            {
                                ModSettings.ClimbingKit = true;
                                level.Session.SetFlag("Upgrade_ClimbingKit", true);
                                ModSaveData.SavedFlags.Add(level.Session.Area.LevelSet + "_Upgrade_ClimbingKit");
                            }
                            if (goldenSpiderMagnet)
                            {
                                ModSettings.SpiderMagnet = true;
                                level.Session.SetFlag("Upgrade_SpiderMagnet", true);
                                ModSaveData.SavedFlags.Add(level.Session.Area.LevelSet + "_Upgrade_SpiderMagnet");
                            }
                            if (goldenDroneTeleport)
                            {
                                ModSettings.DroneTeleport = true;
                                level.Session.SetFlag("Upgrade_DroneTeleport", true);
                                ModSaveData.SavedFlags.Add(level.Session.Area.LevelSet + "_Upgrade_DroneTeleport");
                            }
                            if (goldenJumpBoost)
                            {
                                ModSettings.JumpBoost = true;
                                level.Session.SetFlag("Upgrade_JumpBoost", true);
                                ModSaveData.SavedFlags.Add(level.Session.Area.LevelSet + "_Upgrade_JumpBoost");
                            }
                            if (goldenScrewAttack)
                            {
                                ModSettings.ScrewAttack = true;
                                level.Session.SetFlag("Upgrade_ScrewAttack", true);
                                ModSaveData.SavedFlags.Add(level.Session.Area.LevelSet + "_Upgrade_ScrewAttack");
                            }
                            if (goldenVariaJacket)
                            {
                                ModSettings.VariaJacket = true;
                                level.Session.SetFlag("Upgrade_VariaJacket", true);
                                ModSaveData.SavedFlags.Add(level.Session.Area.LevelSet + "_Upgrade_VariaJacket");
                            }
                            if (goldenGravityJacket)
                            {
                                ModSettings.GravityJacket = true;
                                level.Session.SetFlag("Upgrade_GravityJacket", true);
                                ModSaveData.SavedFlags.Add(level.Session.Area.LevelSet + "_Upgrade_GravityJacket");
                            }
                            if (goldenBombs)
                            {
                                ModSettings.Bombs = true;
                                level.Session.SetFlag("Upgrade_Bombs", true);
                                ModSaveData.SavedFlags.Add(level.Session.Area.LevelSet + "_Upgrade_Bombs");
                            }
                            if (goldenMegaBombs)
                            {
                                ModSettings.MegaBombs = true;
                                level.Session.SetFlag("Upgrade_MegaBombs", true);
                                ModSaveData.SavedFlags.Add(level.Session.Area.LevelSet + "_Upgrade_MegaBombs");
                            }
                            if (goldenRemoteDrone)
                            {
                                ModSettings.RemoteDrone = true;
                                level.Session.SetFlag("Upgrade_RemoteDrone", true);
                                ModSaveData.SavedFlags.Add(level.Session.Area.LevelSet + "_Upgrade_RemoteDrone");
                            }
                            if (goldenGoldenFeather)
                            {
                                ModSettings.GoldenFeather = true;
                                level.Session.SetFlag("Upgrade_GoldenFeather", true);
                                ModSaveData.SavedFlags.Add(level.Session.Area.LevelSet + "_Upgrade_GoldenFeather");
                            }
                            if (goldenBinoculars)
                            {
                                ModSettings.Binoculars = true;
                                level.Session.SetFlag("Upgrade_Binoculars", true);
                                ModSaveData.SavedFlags.Add(level.Session.Area.LevelSet + "_Upgrade_Binoculars");
                            }
                            if (goldenEtherealDash)
                            {
                                ModSettings.EtherealDash = true;
                                level.Session.SetFlag("Upgrade_EtherealDash", true);
                                ModSaveData.SavedFlags.Add(level.Session.Area.LevelSet + "_Upgrade_EtherealDash");
                            }
                            if (goldenPortableStation)
                            {
                                ModSettings.PortableStation = true;
                                level.Session.SetFlag("Upgrade_PortableStation", true);
                                ModSaveData.SavedFlags.Add(level.Session.Area.LevelSet + "_Upgrade_PortableStation");
                            }
                            if (goldenPulseRadar)
                            {
                                ModSettings.PulseRadar = true;
                                level.Session.SetFlag("Upgrade_PulseRadar", true);
                                ModSaveData.SavedFlags.Add(level.Session.Area.LevelSet + "_Upgrade_PulseRadar");
                            }
                            if (goldenDashBoots)
                            {
                                ModSettings.DashBoots = true;
                                level.Session.SetFlag("Upgrade_DashBoots", true);
                                ModSaveData.SavedFlags.Add(level.Session.Area.LevelSet + "_Upgrade_DashBoots");
                            }
                            if (goldenSpaceJump)
                            {
                                ModSettings.SpaceJump = 2;
                                level.Session.SetFlag("Upgrade_SpaceJump", true);
                                ModSaveData.SavedFlags.Add(level.Session.Area.LevelSet + "_Upgrade_SpaceJump");
                            }
                            if (goldenHoverJet)
                            {
                                ModSettings.HoverJet = true;
                                level.Session.SetFlag("Upgrade_HoverJet", true);
                                ModSaveData.SavedFlags.Add(level.Session.Area.LevelSet + "_Upgrade_HoverJet");
                            }
                            if (goldenLightningDash)
                            {
                                ModSettings.LightningDash = true;
                                level.Session.SetFlag("Upgrade_LightningDash", true);
                                ModSaveData.SavedFlags.Add(level.Session.Area.LevelSet + "_Upgrade_LightningDash");
                            }
                            if (goldenLongBeam)
                            {
                                ModSettings.LongBeam = true;
                                level.Session.SetFlag("Upgrade_LongBeam", true);
                                ModSaveData.SavedFlags.Add(level.Session.Area.LevelSet + "_Upgrade_LongBeam");
                            }
                            if (goldenIceBeam)
                            {
                                ModSettings.IceBeam = true;
                                level.Session.SetFlag("Upgrade_IceBeam", true);
                                ModSaveData.SavedFlags.Add(level.Session.Area.LevelSet + "_Upgrade_IceBeam");
                            }
                            if (goldenWaveBeam)
                            {
                                ModSettings.WaveBeam = true;
                                level.Session.SetFlag("Upgrade_WaveBeam", true);
                                ModSaveData.SavedFlags.Add(level.Session.Area.LevelSet + "_Upgrade_WaveBeam");
                            }
                            if (goldenMissilesModule)
                            {
                                ModSettings.MissilesModule = true;
                                level.Session.SetFlag("Upgrade_MissilesModule", true);
                                ModSaveData.SavedFlags.Add(level.Session.Area.LevelSet + "_Upgrade_MissilesModule");
                            }
                            if (goldenSuperMissilesModule)
                            {
                                ModSettings.SuperMissilesModule = true;
                                level.Session.SetFlag("Upgrade_SuperMissilesModule", true);
                                ModSaveData.SavedFlags.Add(level.Session.Area.LevelSet + "_Upgrade_SuperMissilesModule");
                            }
                        }
                    }
                }
                if (!useMergeChaptersController)
                {
                    ModSaveData.GoldenStrawberryUnlockedWarps.Clear();
                    ModSaveData.GoldenStrawberryStaminaUpgrades.Clear();
                    ModSaveData.GoldenStrawberryDroneMissilesUpgrades.Clear();
                    ModSaveData.GoldenStrawberryDroneSuperMissilesUpgrades.Clear();
                    ModSaveData.GoldenStrawberryDroneFireRateUpgrades.Clear();
                }
            }
            orig(self, player);
        }

        private void modStrawberryOnLoseLeader(On.Celeste.Strawberry.orig_OnLoseLeader orig, Strawberry self)
        {
            if (self.Golden)
            {
                PlayerLostGolden = true;
            }
            if (useUpgrades && self.Golden)
            {
                PlayerHasGolden = ModSaveData.GoldenStartChapter == -999 ? false : true;
                orig(self);
            }
            else
            {
                orig(self);
            }
        }

        private static IEnumerator onStrawberryCollectRoutine(On.Celeste.Strawberry.orig_CollectRoutine orig, Strawberry self, int collectIndex)
        {
            yield return new SwapImmediately(orig(self, collectIndex));
            if (self.Golden)
            {
                PlayerHasGolden = false;
                if (useUpgrades)
                {
                    AreaKey area = self.SceneAs<Level>().Session.Area;
                    MapData MapData = AreaData.Areas[area.ID].Mode[(int)area.Mode].MapData;
                    GiveUpgradesToPlayer(MapData, self.SceneAs<Level>());
                }
                /*if (useMergeChaptersController)
                {
                    self.SceneAs<Level>().Session.Time += ModSaveData.PreGoldenTimer;
                    foreach (EntityID entity in self.SceneAs<Level>().Session.DoNotLoad)
                    {
                        ModSaveData.PreGoldenDoNotLoad.Add(entity);
                    }
                    foreach (EntityID entity in ModSaveData.PreGoldenDoNotLoad)
                    {
                        self.SceneAs<Level>().Session.DoNotLoad.Add(entity);
                    }
                    ModSaveData.PreGoldenTimer = 0;
                }*/
            }
        }

        private ScreenWipe GetWipe(Level level, bool wipeIn)
        {
            ScreenWipe Wipe = null;
            switch (ModSaveData.Wipe)
            {
                case "Spotlight":
                    Wipe = new SpotlightWipe(level, wipeIn)
                    {
                        Duration = ModSaveData.WipeDuration
                    };
                    break;
                case "Curtain":
                    Wipe = new CurtainWipe(level, wipeIn)
                    {
                        Duration = ModSaveData.WipeDuration
                    };
                    break;
                case "Mountain":
                    Wipe = new MountainWipe(level, wipeIn)
                    {
                        Duration = ModSaveData.WipeDuration
                    };
                    break;
                case "Dream":
                    Wipe = new DreamWipe(level, wipeIn)
                    {
                        Duration = ModSaveData.WipeDuration
                    };
                    break;
                case "Starfield":
                    Wipe = new StarfieldWipe(level, wipeIn)
                    {
                        Duration = ModSaveData.WipeDuration
                    };
                    break;
                case "Wind":
                    Wipe = new WindWipe(level, wipeIn)
                    {
                        Duration = ModSaveData.WipeDuration
                    };
                    break;
                case "Drop":
                    Wipe = new DropWipe(level, wipeIn)
                    {
                        Duration = ModSaveData.WipeDuration
                    };
                    break;
                case "Fall":
                    Wipe = new FallWipe(level, wipeIn)
                    {
                        Duration = ModSaveData.WipeDuration
                    };
                    break;
                case "KeyDoor":
                    Wipe = new KeyDoorWipe(level, wipeIn)
                    {
                        Duration = ModSaveData.WipeDuration
                    };
                    break;
                case "Angled":
                    Wipe = new AngledWipe(level, wipeIn)
                    {
                        Duration = ModSaveData.WipeDuration
                    };
                    break;
                case "Heart":
                    Wipe = new HeartWipe(level, wipeIn)
                    {
                        Duration = ModSaveData.WipeDuration
                    };
                    break;
                case "Fade":
                    Wipe = new FadeWipe(level, wipeIn)
                    {
                        Duration = ModSaveData.WipeDuration
                    };
                    break;
                case "Level":
                    level.DoScreenWipe(wipeIn);
                    break;
            }
            return Wipe;
        }

        private void onPlayerRender(On.Celeste.Player.orig_Render orig, Player self)
        {
            if (useUpgrades && (VariaJacket.Active(self.SceneAs<Level>()) || GravityJacket.Active(self.SceneAs<Level>())))
            {
                string id = "";
                if (GravityJacket.Active(self.SceneAs<Level>()))
                {
                    id = !useMetroidGameplay ? "gravity" : "samus_gravity";
                }
                else if (VariaJacket.Active(self.SceneAs<Level>()))
                {
                    id = !useMetroidGameplay ? "varia" : "samus_varia";
                }
                Effect fxColorGrading = GFX.FxColorGrading;
                fxColorGrading.CurrentTechnique = fxColorGrading.Techniques["ColorGradeSingle"];
                Engine.Graphics.GraphicsDevice.Textures[1] = GFX.ColorGrades[id].Texture.Texture_Safe;
                Draw.SpriteBatch.End();
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, fxColorGrading, (self.Scene as Level).GameplayRenderer.Camera.Matrix);
                orig(self);
                Draw.SpriteBatch.End();
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, (self.Scene as Level).GameplayRenderer.Camera.Matrix);
            }
            else
            {
                orig(self);
            }
        }

        private void onPlayerDeadBodyRender(On.Celeste.PlayerDeadBody.orig_Render orig, PlayerDeadBody self)
        {
            if (useUpgrades && (VariaJacket.Active(self.SceneAs<Level>()) || GravityJacket.Active(self.SceneAs<Level>())))
            {
                string id = "";
                if (GravityJacket.Active(self.SceneAs<Level>()))
                {
                    id = !useMetroidGameplay ? "gravity" : "samus_gravity";
                }
                else if (VariaJacket.Active(self.SceneAs<Level>()))
                {
                    id = !useMetroidGameplay ? "varia" : "samus_varia";
                }
                Effect fxColorGrading = GFX.FxColorGrading;
                fxColorGrading.CurrentTechnique = fxColorGrading.Techniques["ColorGradeSingle"];
                Engine.Graphics.GraphicsDevice.Textures[1] = GFX.ColorGrades[id].Texture.Texture_Safe;
                Draw.SpriteBatch.End();
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, fxColorGrading, (self.Scene as Level).GameplayRenderer.Camera.Matrix);
                orig(self);
                Draw.SpriteBatch.End();
                Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointWrap, DepthStencilState.None, RasterizerState.CullNone, null, (self.Scene as Level).GameplayRenderer.Camera.Matrix);
            }
            else
            {
                orig(self);
            }
        }

        private Backdrop OnLoadBackdrop(MapData map, BinaryPacker.Element child, BinaryPacker.Element super)
        {
            if (child.Name.Equals("XaphanHelper/Heat", StringComparison.OrdinalIgnoreCase))
            {
                return new Heat();
            }
            if (child.Name.Equals("XaphanHelper/HeatParticles", StringComparison.OrdinalIgnoreCase))
            {
                return new HeatParticles(child.Attr("particlesColors"), child.AttrInt("particlesAmount"), child.AttrBool("noMist"));
            }
            if (child.Name.Equals("XaphanHelper/Glow", StringComparison.OrdinalIgnoreCase))
            {
                return new Glow(child.Attr("color"));
            }
            if (child.Name.Equals("XaphanHelper/LightPetals", StringComparison.OrdinalIgnoreCase))
            {
                return new LightPetals(child.Attr("lightColor"), child.Attr("darkColor"));
            }
            return null;
        }

        public void PlayerSpriteMetadataHook(On.Celeste.PlayerSprite.orig_CreateFramesMetadata orig, string self)
        {
            orig(self);
            if (self == null || self.StartsWith("XaphanHelper_Extend_"))
            {
                return;
            }
            string extend_object = "XaphanHelper_Extend_" + self;
            if (!GFX.SpriteBank.Has(extend_object))
            {
                extend_object = "XaphanHelper_Extend_player";
            }
            PatchSprite(GFX.SpriteBank.SpriteData[extend_object].Sprite, GFX.SpriteBank.SpriteData[self].Sprite);
            if (extend_object == "XaphanHelper_Extend_" + self)
            {
                // Avoiding hooks confusion, don't call orig again instead call PlayerSprite.CreateFramesMetadata
                PlayerSprite.CreateFramesMetadata(extend_object);
            }
        }
        private void PatchSprite(Sprite origSprite, Sprite newSprite)
        {
            Dictionary<string, Sprite.Animation> newAnims = newSprite.Animations;

            // Shallow copy... sometimes new animations get added mid-update?
            Dictionary<string, Sprite.Animation> oldAnims = new(origSprite.Animations);
            foreach (KeyValuePair<string, Sprite.Animation> animEntry in oldAnims)
            {
                string origAnimId = animEntry.Key;

                Sprite.Animation origAnim = animEntry.Value;
                if (!newAnims.ContainsKey(origAnimId))
                {
                    newAnims[origAnimId] = origAnim;
                }
            }
        }
    }
}