using System.Collections.Generic;
using Celeste.Mod.XaphanHelper.Data;
using Microsoft.Xna.Framework;
using Monocle;

namespace Celeste.Mod.XaphanHelper.UI_Elements
{
    [Tracked(true)]
    public class MapProgressDisplay : Entity
    {
        public InGameMapControllerData InGameMapControllerData;

        public List<InGameMapSubAreaControllerData> SubAreaControllerData;

        public List<InGameMapRoomControllerData> RoomControllerData = new();

        public List<InGameMapTilesControllerData> TilesControllerData = new();

        public List<InGameMapEntitiesData> EntitiesData = new();

        public string MapPercent;

        public string SubAreaMapPercent;

        public int chapterIndex;

        public string currentRoom;

        public int subAreaIndex;

        public int mode;

        private bool NoMapTiles;

        public string Prefix;

        public Level Level;

        public MapScreen MapScreen;

        private bool PowerGripUnlocked = XaphanModule.ModSettings.PowerGrip;

        private bool RemoteDroneUnlocked = XaphanModule.ModSettings.RemoteDrone;

        private bool MissilesModuleUnlocked = XaphanModule.ModSettings.MissilesModule;

        private bool SuperMissilesModuleUnlocked = XaphanModule.ModSettings.SuperMissilesModule;

        public MapProgressDisplay(Vector2 position, Level level, InGameMapControllerData inGameMapControllerData, List<InGameMapSubAreaControllerData> subAreaControllerData, List<InGameMapRoomControllerData> roomControllerData, List<InGameMapTilesControllerData> tileControllerData, List<InGameMapEntitiesData> entitiesData, int chapterIndex, string currentRoom) : base(position)
        {
            Tag = Tags.HUD;
            Level = level;
            InGameMapControllerData = inGameMapControllerData;
            SubAreaControllerData = subAreaControllerData;
            RoomControllerData = roomControllerData;
            TilesControllerData = tileControllerData;
            EntitiesData = entitiesData;
            this.chapterIndex = chapterIndex;
            this.currentRoom = currentRoom;
            Depth = -10002;
        }

        public override void Added(Scene scene)
        {
            base.Added(scene);
            MapScreen = Level.Tracker.GetEntity<MapScreen>();
            Prefix = Level.Session.Area.LevelSet;
            if (!XaphanModule.ModSaveData.ProgressMode.ContainsKey(Prefix))
            {
                XaphanModule.ModSaveData.ProgressMode.Add(Prefix, 1);
            }
            else
            {
                mode = XaphanModule.ModSaveData.ProgressMode[Prefix];
            }
            if (mode == 1 && SubAreaControllerData.Count == 1)
            {
                mode = 2;
            }
            GetData(currentRoom);
        }

        public int CurrentMapTiles;

        public int TotalMapTiles;

        public int CurrentStrawberries;

        public int TotalStrawberries;

        public int CurrentMoonberries;

        public int TotalMoonberries;

        public int CurrentUpgrades;

        public int TotalUpgrades;

        public int CurrentStaminaUpgrades;

        public int TotalStaminaUpgrades;

        public int CurrentDroneMissilesUpgrades;

        public int TotalDroneMissilesUpgrades;

        public int CurrentDroneSuperMissilesUpgrades;

        public int TotalDroneSuperMissilesUpgrades;

        public int CurrentDroneFireRateUpgrades;

        public int TotalDroneFireRateUpgrades;

        public int CurrentHeart;

        public int TotalHeart;

        public int CurrentCassette;

        public int TotalCassette;

        public int SubAreaCurrentMapTiles;

        public int SubAreaTotalMapTiles;

        public int SubAreaCurrentStrawberries;

        public int SubAreaTotalStrawberries;

        public int SubAreaCurrentMoonberries;

        public int SubAreaTotalMoonberries;

        public int SubAreaCurrentUpgrades;

        public int SubAreaTotalUpgrades;

        public int SubAreaCurrentStaminaUpgrades;

        public int SubAreaTotalStaminaUpgrades;

        public int SubAreaCurrentDroneMissilesUpgrades;

        public int SubAreaTotalDroneMissilesUpgrades;

        public int SubAreaCurrentDroneSuperMissilesUpgrades;

        public int SubAreaTotalDroneSuperMissilesUpgrades;

        public int SubAreaCurrentDroneFireRateUpgrades;

        public int SubAreaTotalDroneFireRateUpgrades;

        public int SubAreaCurrentHeart;

        public int SubAreaTotalHeart;

        public int SubAreaCurrentCassette;

        public int SubAreaTotalCassette;

        public bool Initialized;

        public void GetData(string room)
        {
            Initialized = false;
            subAreaIndex = -1;
            if (chapterIndex != -1)
            {
                // Get data of the current area
                CurrentMapTiles = getCurrentMapTiles();
                TotalMapTiles = getTotalMapTiles();
                MapPercent = (CurrentMapTiles * 100 / TotalMapTiles).ToString();
                CurrentStrawberries = getCurrentStrawberries();
                TotalStrawberries = getTotalStrawberries();
                CurrentMoonberries = getCurrentMoonberries();
                TotalMoonberries = getTotalMoonberries();
                CurrentUpgrades = getCurrentUpgrades();
                TotalUpgrades = getTotalUpgrades();
                CurrentStaminaUpgrades = getCurrentStaminaUpgrades();
                TotalStaminaUpgrades = getTotalStaminaUpgrades();
                CurrentDroneMissilesUpgrades = getCurrentDroneMissilesUpgrades();
                TotalDroneMissilesUpgrades = getTotalDroneMissilesUpgrades();
                CurrentDroneSuperMissilesUpgrades = getCurrentDroneSuperMissilesUpgrades();
                TotalDroneSuperMissilesUpgrades = getTotalDroneSuperMissilesUpgrades();
                CurrentDroneFireRateUpgrades = getCurrentDroneFireRateUpgrades();
                TotalDroneFireRateUpgrades = getTotalDroneFireRateUpgrades();
                CurrentHeart = getCurrentHeart();
                TotalHeart = getTotalHeart();
                CurrentCassette = getCurrentCassette();
                TotalCassette = getTotalCassette();
                for (int i = 0; i < InGameMapControllerData.CustomCollectablesProgress.Split(',').Length; i++)
                {

                }

                // Get data of the curernt sub-area
                foreach (InGameMapRoomControllerData roomControllerData in RoomControllerData)
                {
                    if (roomControllerData.Room == room)
                    {
                        subAreaIndex = roomControllerData.SubAreaIndex;
                        break;
                    }
                }
                SubAreaCurrentMapTiles = getCurrentMapTiles(subAreaIndex);
                SubAreaTotalMapTiles = getTotalMapTiles(subAreaIndex);
                SubAreaMapPercent = (SubAreaCurrentMapTiles * 100 / SubAreaTotalMapTiles).ToString();
                SubAreaCurrentStrawberries = getCurrentStrawberries(subAreaIndex);
                SubAreaTotalStrawberries = getTotalStrawberries(subAreaIndex);
                SubAreaCurrentMoonberries = getCurrentMoonberries(subAreaIndex);
                SubAreaTotalMoonberries = getTotalMoonberries(subAreaIndex);
                SubAreaCurrentUpgrades = getCurrentUpgrades(subAreaIndex);
                SubAreaTotalUpgrades = getTotalUpgrades(subAreaIndex);
                SubAreaCurrentStaminaUpgrades = getCurrentStaminaUpgrades(subAreaIndex);
                SubAreaTotalStaminaUpgrades = getTotalStaminaUpgrades(subAreaIndex);
                SubAreaCurrentDroneMissilesUpgrades = getCurrentDroneMissilesUpgrades(subAreaIndex);
                SubAreaTotalDroneMissilesUpgrades = getTotalDroneMissilesUpgrades(subAreaIndex);
                SubAreaCurrentDroneSuperMissilesUpgrades = getCurrentDroneSuperMissilesUpgrades(subAreaIndex);
                SubAreaTotalDroneSuperMissilesUpgrades = getTotalDroneSuperMissilesUpgrades(subAreaIndex);
                SubAreaCurrentDroneFireRateUpgrades = getCurrentDroneFireRateUpgrades(subAreaIndex);
                SubAreaTotalDroneFireRateUpgrades = getTotalDroneFireRateUpgrades(subAreaIndex);
                SubAreaCurrentHeart = getCurrentHeart(subAreaIndex);
                SubAreaTotalHeart = getTotalHeart(subAreaIndex);
                SubAreaCurrentCassette = getCurrentCassette(subAreaIndex);
                SubAreaTotalCassette = getTotalCassette(subAreaIndex);
            }
            else
            {
                // get data of the whole campaign
                CurrentMapTiles = getCurrentMapTiles();
                TotalMapTiles = getTotalMapTiles();
                MapPercent = (getCurrentMapTiles() * 100 / getTotalMapTiles()).ToString();
                CurrentStrawberries = getCurrentStrawberries();
                TotalStrawberries = getTotalStrawberries();
                CurrentMoonberries = getCurrentMoonberries();
                TotalMoonberries = getTotalMoonberries();
                CurrentUpgrades = getCurrentUpgrades();
                TotalUpgrades = getTotalUpgrades();
                CurrentStaminaUpgrades = getCurrentStaminaUpgrades();
                TotalStaminaUpgrades = getTotalStaminaUpgrades();
                CurrentDroneMissilesUpgrades = getCurrentDroneMissilesUpgrades();
                TotalDroneMissilesUpgrades = getTotalDroneMissilesUpgrades();
                CurrentDroneSuperMissilesUpgrades = getCurrentDroneSuperMissilesUpgrades();
                TotalDroneSuperMissilesUpgrades = getTotalDroneSuperMissilesUpgrades();
                CurrentDroneFireRateUpgrades = getCurrentDroneFireRateUpgrades();
                TotalDroneFireRateUpgrades = getTotalDroneFireRateUpgrades();
                CurrentHeart = getCurrentHeart();
                TotalHeart = getTotalHeart();
                CurrentCassette = getCurrentCassette();
                TotalCassette = getTotalCassette();
            }
            Initialized = true;
        }

        public bool Hidden;

        public override void Update()
        {
            if (Visible)
            {
                base.Update();
                AreaKey area = Level.Session.Area;
                string Prefix = Level.Session.Area.LevelSet;
                if (chapterIndex != -1)
                {
                    if (InGameMapControllerData.ShowProgress == "AfterChapterComplete")
                    {
                        AreaModeStats areaModeStats = SaveData.Instance.Areas_Safe[area.ID - (area.ChapterIndex == -1 ? 0 : area.ChapterIndex) + chapterIndex].Modes[(int)area.Mode];
                        if (!areaModeStats.Completed)
                        {
                            Hidden = true;
                            mode = 0;
                            return;
                        }
                    }
                    else if (InGameMapControllerData.ShowProgress == "AfterCampaignComplete")
                    {
                        foreach (AreaStats areaStats in SaveData.Instance.Areas_Safe)
                        {
                            if (areaStats.LevelSet == Prefix && !SaveData.Instance.Areas_Safe[areaStats.ID].Modes[(int)area.Mode].Completed)
                            {
                                Hidden = true;
                                mode = 0;
                                return;
                            }
                        }
                    }
                }
                else
                {
                    if (InGameMapControllerData.ShowProgress == "AfterCampaignComplete")
                    {
                        foreach (AreaStats areaStats in SaveData.Instance.Areas_Safe)
                        {
                            if (areaStats.LevelSet == Prefix && !SaveData.Instance.Areas_Safe[areaStats.ID].Modes[(int)area.Mode].Completed)
                            {
                                Hidden = true;
                                mode = 0;
                                return;
                            }
                        }
                    }
                }
                if (chapterIndex != -1)
                {
                    if (XaphanModule.ModSaveData.ProgressMode[Prefix] > 0 && (subAreaIndex == -1 || SubAreaControllerData.Count == 1))
                    {
                        mode = 2;
                    }
                    else
                    {
                        mode = XaphanModule.ModSaveData.ProgressMode[Prefix];
                    }
                }
                else
                {
                    if (XaphanModule.ModSaveData.ProgressMode[Prefix] != 0)
                    {
                        mode = 2;
                    }
                    else
                    {
                        mode = XaphanModule.ModSaveData.ProgressMode[Prefix];
                    }
                }
            }
        }

        public List<string> getSubAreaRooms(int subAreaIndex)
        {
            List<string> subAreaRooms = new();
            foreach (InGameMapRoomControllerData roomControllerData in RoomControllerData)
            {
                if (roomControllerData.SubAreaIndex == subAreaIndex)
                {
                    subAreaRooms.Add(roomControllerData.Room);
                }
            }
            return subAreaRooms;
        }

        public int getCurrentMapTiles(int subAreaIndex = -1)
        {
            int currentTiles = 0;
            if (chapterIndex != -1)
            {
                if (subAreaIndex == -1)
                {
                    foreach (InGameMapTilesControllerData tilesControllerData in TilesControllerData)
                    {
                        for (int i = 0; i <= 9; i++)
                        {
                            string tile = tilesControllerData.GetTile(i);
                            if (tile != "None" && tile != "ElevatorShaft" && !tile.Contains("Arrow") && !tile.Contains("Connection"))
                            {
                                if (XaphanModule.ModSaveData.VisitedRoomsTiles.Contains(Prefix + "/Ch" + chapterIndex + "/" + tilesControllerData.Room + "-" + tilesControllerData.GetTileCords(i)))
                                {
                                    currentTiles++;
                                }
                            }
                        }
                    }
                }
                else
                {
                    List<string> subAreaRooms = getSubAreaRooms(subAreaIndex);
                    foreach (string room in subAreaRooms)
                    {
                        foreach (InGameMapTilesControllerData tilesControllerData in TilesControllerData)
                        {
                            if (room == tilesControllerData.Room)
                            {
                                for (int i = 0; i <= 9; i++)
                                {
                                    string tile = tilesControllerData.GetTile(i);
                                    if (tile != "None" && tile != "ElevatorShaft" && !tile.Contains("Arrow") && !tile.Contains("Connection"))
                                    {
                                        if (XaphanModule.ModSaveData.VisitedRoomsTiles.Contains(Prefix + "/Ch" + chapterIndex + "/" + tilesControllerData.Room + "-" + tilesControllerData.GetTileCords(i)))
                                        {
                                            currentTiles++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                for (int chapter = !MapScreen.hasInterlude ? 1 : 0; chapter < MapScreen.maxChapters; chapter++)
                {
                    foreach (InGameMapTilesControllerData tilesControllerData in TilesControllerData)
                    {
                        if (tilesControllerData.ChapterIndex == chapter)
                        {
                            for (int i = 0; i <= 9; i++)
                            {
                                string tile = tilesControllerData.GetTile(i);
                                if (tile != "None" && tile != "ElevatorShaft" && !tile.Contains("Arrow") && !tile.Contains("Connection"))
                                {
                                    if (XaphanModule.ModSaveData.VisitedRoomsTiles.Contains(Prefix + "/Ch" + chapter + "/" + tilesControllerData.Room + "-" + tilesControllerData.GetTileCords(i)))
                                    {
                                        currentTiles++;
                                    }
                                }
                            }

                        }
                    }
                }
            }
            return currentTiles;
        }

        public int getTotalMapTiles(int subAreaIndex = -1)
        {
            int totalTiles = 0;
            if (subAreaIndex == -1)
            {
                foreach (InGameMapTilesControllerData tilesControllerData in TilesControllerData)
                {
                    for (int i = 0; i <= 9; i++)
                    {
                        string tile = tilesControllerData.GetTile(i);
                        if (tile != "None" && tile != "ElevatorShaft" && !tile.Contains("Arrow") && !tile.Contains("Connection"))
                        {
                            totalTiles++;
                        }
                    }
                }
            }
            else
            {
                List<string> subAreaRooms = getSubAreaRooms(subAreaIndex);
                foreach (string room in subAreaRooms)
                {
                    foreach (InGameMapTilesControllerData tilesControllerData in TilesControllerData)
                    {
                        if (room == tilesControllerData.Room)
                        {
                            for (int i = 0; i <= 9; i++)
                            {
                                string tile = tilesControllerData.GetTile(i);
                                if (tile != "None" && tile != "ElevatorShaft" && !tile.Contains("Arrow") && !tile.Contains("Connection"))
                                {
                                    totalTiles++;
                                }
                            }
                        }
                    }
                }
            }
            if (totalTiles == 0)
            {
                totalTiles = 1;
                NoMapTiles = true;
            }
            return totalTiles;
        }

        public int getCurrentStrawberries(int subAreaIndex = -1)
        {
            int CurrentStrawberriesCount = 0;
            if (subAreaIndex == -1)
            {
                foreach (InGameMapEntitiesData entityData in EntitiesData)
                {
                    if (entityData.Type == "strawberry" && SaveData.Instance.CheckStrawberry(entityData.StrawberryArea, new EntityID(entityData.Room, entityData.ID)))
                    {
                        CurrentStrawberriesCount++;
                    }
                }
            }
            else
            {
                List<string> subAreaRooms = getSubAreaRooms(subAreaIndex);
                foreach (string room in subAreaRooms)
                {
                    foreach (InGameMapEntitiesData entityData in EntitiesData)
                    {
                        if (room == entityData.Room)
                        {
                            if (entityData.Type == "strawberry" && SaveData.Instance.CheckStrawberry(entityData.StrawberryArea, new EntityID(entityData.Room, entityData.ID)))
                            {
                                CurrentStrawberriesCount++;
                            }
                        }
                    }
                }
            }
            return CurrentStrawberriesCount;
        }

        public int getTotalStrawberries(int subAreaIndex = -1)
        {
            int TotalStrawberriesCount = 0;
            if (subAreaIndex == -1)
            {
                foreach (InGameMapEntitiesData entityData in EntitiesData)
                {
                    if (entityData.Type == "strawberry")
                    {
                        TotalStrawberriesCount++;
                    }
                }
            }
            else
            {
                List<string> subAreaRooms = getSubAreaRooms(subAreaIndex);
                foreach (string room in subAreaRooms)
                {
                    foreach (InGameMapEntitiesData entityData in EntitiesData)
                    {
                        if (room == entityData.Room)
                        {
                            if (entityData.Type == "strawberry")
                            {
                                TotalStrawberriesCount++;
                            }
                        }
                    }
                }
            }
            return TotalStrawberriesCount;
        }

        public int getCurrentMoonberries(int subAreaIndex = -1)
        {
            int CurrentMoonberriesCount = 0;
            if (subAreaIndex == -1)
            {
                foreach (InGameMapEntitiesData entityData in EntitiesData)
                {
                    if (entityData.Type == "moonberry" && SaveData.Instance.CheckStrawberry(entityData.StrawberryArea, new EntityID(entityData.Room, entityData.ID)))
                    {
                        CurrentMoonberriesCount++;
                    }
                }
            }
            else
            {
                List<string> subAreaRooms = getSubAreaRooms(subAreaIndex);
                foreach (string room in subAreaRooms)
                {
                    foreach (InGameMapEntitiesData entityData in EntitiesData)
                    {
                        if (room == entityData.Room)
                        {
                            if (entityData.Type == "moonberry" && SaveData.Instance.CheckStrawberry(entityData.StrawberryArea, new EntityID(entityData.Room, entityData.ID)))
                            {
                                CurrentMoonberriesCount++;
                            }
                        }
                    }
                }
            }
            return CurrentMoonberriesCount;
        }

        public int getTotalMoonberries(int subAreaIndex = -1)
        {
            int TotalMoonberriesCount = 0;
            if (subAreaIndex == -1)
            {
                foreach (InGameMapEntitiesData entityData in EntitiesData)
                {
                    if (entityData.Type == "moonberry")
                    {
                        TotalMoonberriesCount++;
                    }
                }
            }
            else
            {
                List<string> subAreaRooms = getSubAreaRooms(subAreaIndex);
                foreach (string room in subAreaRooms)
                {
                    foreach (InGameMapEntitiesData entityData in EntitiesData)
                    {
                        if (room == entityData.Room)
                        {
                            if (entityData.Type == "moonberry")
                            {
                                TotalMoonberriesCount++;
                            }
                        }
                    }
                }
            }
            return TotalMoonberriesCount;
        }

        public int getCurrentUpgrades(int subAreaIndex = -1)
        {
            int CurrentUpgradesCount = 0;
            if (subAreaIndex == -1)
            {
                foreach (InGameMapEntitiesData entityData in EntitiesData)
                {
                    if (entityData.Type == "upgrade" && XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Upgrade_" + entityData.UpgradeCollectableUpgrade))
                    {
                        CurrentUpgradesCount++;
                    }
                }
            }
            else
            {
                List<string> subAreaRooms = getSubAreaRooms(subAreaIndex);
                foreach (string room in subAreaRooms)
                {
                    foreach (InGameMapEntitiesData entityData in EntitiesData)
                    {
                        if (room == entityData.Room)
                        {
                            if (entityData.Type == "upgrade" && XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Upgrade_" + entityData.UpgradeCollectableUpgrade))
                            {
                                CurrentUpgradesCount++;
                            }
                        }
                    }
                }
            }
            return CurrentUpgradesCount;
        }

        public int getTotalUpgrades(int subAreaIndex = -1)
        {
            int TotalUpgradesCount = 0;
            if (subAreaIndex == -1)
            {
                foreach (InGameMapEntitiesData entityData in EntitiesData)
                {
                    if (entityData.Type == "upgrade")
                    {
                        TotalUpgradesCount++;
                    }
                }
            }
            else
            {
                List<string> subAreaRooms = getSubAreaRooms(subAreaIndex);
                foreach (string room in subAreaRooms)
                {
                    foreach (InGameMapEntitiesData entityData in EntitiesData)
                    {
                        if (room == entityData.Room)
                        {
                            if (entityData.Type == "upgrade")
                            {
                                TotalUpgradesCount++;
                            }
                        }
                    }
                }
            }
            return TotalUpgradesCount;
        }

        public int getCurrentStaminaUpgrades(int subAreaIndex = -1)
        {
            int CurrentStaminaUpgradesCount = 0;
            if (subAreaIndex == -1)
            {
                foreach (string staminaUpgrade in XaphanModule.ModSaveData.StaminaUpgrades)
                {
                    if (staminaUpgrade.Contains(chapterIndex >= 0 ? Prefix + "_Ch" + chapterIndex : Prefix))
                    {
                        CurrentStaminaUpgradesCount++;
                    }
                }
            }
            else
            {
                List<string> subAreaRooms = getSubAreaRooms(subAreaIndex);
                foreach (string room in subAreaRooms)
                {
                    foreach (InGameMapEntitiesData entityData in EntitiesData)
                    {
                        if (room == entityData.Room)
                        {
                            if (entityData.Type == "energyTank")
                            {
                                foreach (string staminaUpgrade in XaphanModule.ModSaveData.StaminaUpgrades)
                                {
                                    if (staminaUpgrade.Contains(Prefix + "_Ch" + chapterIndex + "_" + room))
                                    {
                                        CurrentStaminaUpgradesCount++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return CurrentStaminaUpgradesCount;
        }

        public int getTotalStaminaUpgrades(int subAreaIndex = -1)
        {
            int TotalStaminaUpgradesCount = 0;
            if (subAreaIndex == -1)
            {
                foreach (InGameMapEntitiesData entityData in EntitiesData)
                {
                    if (entityData.Type == "energyTank")
                    {
                        TotalStaminaUpgradesCount++;
                    }
                }
            }
            else
            {
                List<string> subAreaRooms = getSubAreaRooms(subAreaIndex);
                foreach (string room in subAreaRooms)
                {
                    foreach (InGameMapEntitiesData entityData in EntitiesData)
                    {
                        if (room == entityData.Room)
                        {
                            if (entityData.Type == "energyTank")
                            {
                                TotalStaminaUpgradesCount++;
                            }
                        }
                    }
                }
            }
            return TotalStaminaUpgradesCount;
        }

        public int getCurrentDroneMissilesUpgrades(int subAreaIndex = -1)
        {
            int CurrentDroneMissilesUpgradesCount = 0;
            if (subAreaIndex == -1)
            {
                foreach (string DroneMissilesUpgrade in XaphanModule.ModSaveData.DroneMissilesUpgrades)
                {
                    if (DroneMissilesUpgrade.Contains(chapterIndex >= 0 ? Prefix + "_Ch" + chapterIndex : Prefix))
                    {
                        CurrentDroneMissilesUpgradesCount++;
                    }
                }
            }
            else
            {
                List<string> subAreaRooms = getSubAreaRooms(subAreaIndex);
                foreach (string room in subAreaRooms)
                {
                    foreach (InGameMapEntitiesData entityData in EntitiesData)
                    {
                        if (room == entityData.Room)
                        {
                            if (entityData.Type == "missile")
                            {
                                foreach (string DroneMissilesUpgrade in XaphanModule.ModSaveData.DroneMissilesUpgrades)
                                {
                                    if (DroneMissilesUpgrade.Contains(Prefix + "_Ch" + chapterIndex + "_" + room))
                                    {
                                        CurrentDroneMissilesUpgradesCount++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return CurrentDroneMissilesUpgradesCount;
        }

        public int getTotalDroneMissilesUpgrades(int subAreaIndex = -1)
        {
            int TotalDroneMissilesUpgradesCount = 0;
            if (subAreaIndex == -1)
            {
                foreach (InGameMapEntitiesData entityData in EntitiesData)
                {
                    if (entityData.Type == "missile")
                    {
                        TotalDroneMissilesUpgradesCount++;
                    }
                }
            }
            else
            {
                List<string> subAreaRooms = getSubAreaRooms(subAreaIndex);
                foreach (string room in subAreaRooms)
                {
                    foreach (InGameMapEntitiesData entityData in EntitiesData)
                    {
                        if (room == entityData.Room)
                        {
                            if (entityData.Type == "missile")
                            {
                                TotalDroneMissilesUpgradesCount++;
                            }
                        }
                    }
                }
            }
            return TotalDroneMissilesUpgradesCount;
        }

        public int getCurrentDroneSuperMissilesUpgrades(int subAreaIndex = -1)
        {
            int CurrentDroneSuperMissilesUpgradesCount = 0;
            if (subAreaIndex == -1)
            {
                foreach (string DroneSuperMissilesUpgrade in XaphanModule.ModSaveData.DroneSuperMissilesUpgrades)
                {
                    if (DroneSuperMissilesUpgrade.Contains(chapterIndex >= 0 ? Prefix + "_Ch" + chapterIndex : Prefix))
                    {
                        CurrentDroneSuperMissilesUpgradesCount++;
                    }
                }
            }
            else
            {
                List<string> subAreaRooms = getSubAreaRooms(subAreaIndex);
                foreach (string room in subAreaRooms)
                {
                    foreach (InGameMapEntitiesData entityData in EntitiesData)
                    {
                        if (room == entityData.Room)
                        {
                            if (entityData.Type == "superMissile")
                            {
                                foreach (string DroneSuperMissilesUpgrade in XaphanModule.ModSaveData.DroneSuperMissilesUpgrades)
                                {
                                    if (DroneSuperMissilesUpgrade.Contains(Prefix + "_Ch" + chapterIndex + "_" + room))
                                    {
                                        CurrentDroneSuperMissilesUpgradesCount++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return CurrentDroneSuperMissilesUpgradesCount;
        }

        public int getTotalDroneSuperMissilesUpgrades(int subAreaIndex = -1)
        {
            int TotalDroneSuperMissilesUpgradesCount = 0;
            if (subAreaIndex == -1)
            {
                foreach (InGameMapEntitiesData entityData in EntitiesData)
                {
                    if (entityData.Type == "superMissile")
                    {
                        TotalDroneSuperMissilesUpgradesCount++;
                    }
                }
            }
            else
            {
                List<string> subAreaRooms = getSubAreaRooms(subAreaIndex);
                foreach (string room in subAreaRooms)
                {
                    foreach (InGameMapEntitiesData entityData in EntitiesData)
                    {
                        if (room == entityData.Room)
                        {
                            if (entityData.Type == "superMissile")
                            {
                                TotalDroneSuperMissilesUpgradesCount++;
                            }
                        }
                    }
                }
            }
            return TotalDroneSuperMissilesUpgradesCount;
        }

        public int getCurrentDroneFireRateUpgrades(int subAreaIndex = -1)
        {
            int CurrentFireRateUpgradesCount = 0;
            if (subAreaIndex == -1)
            {
                foreach (string droneFireRateUpgradeUpgrade in XaphanModule.ModSaveData.DroneFireRateUpgrades)
                {
                    if (droneFireRateUpgradeUpgrade.Contains(chapterIndex >= 0 ? Prefix + "_Ch" + chapterIndex : Prefix))
                    {
                        CurrentFireRateUpgradesCount++;
                    }
                }
            }
            else
            {
                List<string> subAreaRooms = getSubAreaRooms(subAreaIndex);
                foreach (string room in subAreaRooms)
                {
                    foreach (InGameMapEntitiesData entityData in EntitiesData)
                    {
                        if (room == entityData.Room)
                        {
                            if (entityData.Type == "fireRateModule")
                            {
                                foreach (string droneFireRateUpgradeUpgrade in XaphanModule.ModSaveData.DroneFireRateUpgrades)
                                {
                                    if (droneFireRateUpgradeUpgrade.Contains(Prefix + "_Ch" + chapterIndex + "_" + room))
                                    {
                                        CurrentFireRateUpgradesCount++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return CurrentFireRateUpgradesCount;
        }

        public int getTotalDroneFireRateUpgrades(int subAreaIndex = -1)
        {
            int TotalFireRateUpgradesCount = 0;
            if (subAreaIndex == -1)
            {
                foreach (InGameMapEntitiesData entityData in EntitiesData)
                {
                    if (entityData.Type == "fireRateModule")
                    {
                        TotalFireRateUpgradesCount++;
                    }
                }
            }
            else
            {
                List<string> subAreaRooms = getSubAreaRooms(subAreaIndex);
                foreach (string room in subAreaRooms)
                {
                    foreach (InGameMapEntitiesData entityData in EntitiesData)
                    {
                        if (room == entityData.Room)
                        {
                            if (entityData.Type == "fireRateModule")
                            {
                                TotalFireRateUpgradesCount++;
                            }
                        }
                    }
                }
            }
            return TotalFireRateUpgradesCount;
        }

        public int getCurrentHeart(int subAreaIndex = -1)
        {
            AreaKey area = Level.Session.Area;
            if (chapterIndex != -1)
            {
                if (subAreaIndex == -1)
                {
                    foreach (InGameMapEntitiesData entityData in EntitiesData)
                    {
                        if (entityData.Type == "heart" && SaveData.Instance.Areas_Safe[area.ID - (area.ChapterIndex == -1 ? 0 : area.ChapterIndex) + chapterIndex].Modes[(int)area.Mode].HeartGem)
                        {
                            return 1;
                        }
                    }
                }
                else
                {
                    List<string> subAreaRooms = getSubAreaRooms(subAreaIndex);
                    foreach (string room in subAreaRooms)
                    {
                        foreach (InGameMapEntitiesData entityData in EntitiesData)
                        {
                            if (room == entityData.Room)
                            {
                                if (entityData.Type == "heart" && SaveData.Instance.Areas_Safe[area.ID - (area.ChapterIndex == -1 ? 0 : area.ChapterIndex) + chapterIndex].Modes[(int)area.Mode].HeartGem)
                                {
                                    return 1;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                int curentHearts = 0;
                for (int chapter = !MapScreen.hasInterlude ? 1 : 0; chapter < MapScreen.maxChapters; chapter++)
                {
                    foreach (InGameMapEntitiesData entityData in EntitiesData)
                    {
                        if (entityData.ChapterIndex == chapter && entityData.Type == "heart" && SaveData.Instance.Areas_Safe[area.ID - (area.ChapterIndex == -1 ? 0 : area.ChapterIndex) + chapter].Modes[(int)area.Mode].HeartGem)
                        {
                            curentHearts++;
                        }
                    }
                }
                return curentHearts;
            }
            return 0;
        }

        public int getTotalHeart(int subAreaIndex = -1)
        {
            if (chapterIndex != -1)
            {
                if (subAreaIndex == -1)
                {
                    foreach (InGameMapEntitiesData entityData in EntitiesData)
                    {
                        if (entityData.Type == "heart")
                        {
                            return 1;
                        }
                    }
                }
                else
                {
                    List<string> subAreaRooms = getSubAreaRooms(subAreaIndex);
                    foreach (string room in subAreaRooms)
                    {
                        foreach (InGameMapEntitiesData entityData in EntitiesData)
                        {
                            if (room == entityData.Room)
                            {
                                if (entityData.Type == "heart")
                                {
                                    return 1;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                int totalHearts = 0;
                foreach (InGameMapEntitiesData entityData in EntitiesData)
                {
                    if (entityData.Type == "heart")
                    {
                        totalHearts++;
                    }
                }
                return totalHearts;
            }
            return 0;
        }

        public int getCurrentCassette(int subAreaIndex = -1)
        {
            AreaKey area = Level.Session.Area;
            if (chapterIndex != -1)
            {
                if (subAreaIndex == -1)
                {
                    foreach (InGameMapEntitiesData entityData in EntitiesData)
                    {
                        if (entityData.Type == "cassette" && SaveData.Instance.Areas_Safe[area.ID - (area.ChapterIndex == -1 ? 0 : area.ChapterIndex) + chapterIndex].Cassette)
                        {
                            return 1;
                        }
                    }
                }
                else
                {
                    List<string> subAreaRooms = getSubAreaRooms(subAreaIndex);
                    foreach (string room in subAreaRooms)
                    {
                        foreach (InGameMapEntitiesData entityData in EntitiesData)
                        {
                            if (room == entityData.Room)
                            {
                                if (entityData.Type == "cassette" && SaveData.Instance.Areas_Safe[area.ID - (area.ChapterIndex == -1 ? 0 : area.ChapterIndex) + chapterIndex].Cassette)
                                {
                                    return 1;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                int curentCassettes = 0;
                for (int chapter = !MapScreen.hasInterlude ? 1 : 0; chapter < MapScreen.maxChapters; chapter++)
                {
                    foreach (InGameMapEntitiesData entityData in EntitiesData)
                    {
                        if (entityData.ChapterIndex == chapter && entityData.Type == "cassette" && SaveData.Instance.Areas_Safe[area.ID - (area.ChapterIndex == -1 ? 0 : area.ChapterIndex) + chapter].Cassette)
                        {
                            curentCassettes++;
                        }
                    }
                }
                return curentCassettes;
            }
            return 0;
        }

        public int getTotalCassette(int subAreaIndex = -1)
        {
            if (chapterIndex != -1)
            {
                if (subAreaIndex == -1)
                {
                    foreach (InGameMapEntitiesData entityData in EntitiesData)
                    {
                        if (entityData.Type == "cassette")
                        {
                            return 1;
                        }
                    }
                }
                else
                {
                    List<string> subAreaRooms = getSubAreaRooms(subAreaIndex);
                    foreach (string room in subAreaRooms)
                    {
                        foreach (InGameMapEntitiesData entityData in EntitiesData)
                        {
                            if (room == entityData.Room)
                            {
                                if (entityData.Type == "cassette")
                                {
                                    return 1;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                int totalCassettes = 0;
                foreach (InGameMapEntitiesData entityData in EntitiesData)
                {
                    if (entityData.Type == "cassette")
                    {
                        totalCassettes++;
                    }
                }
                return totalCassettes;
            }
            return 0;
        }

        public int getCurrentCustomCollectable(string customCollectable, int subAreaIndex = -1)
        {
            int CurrentCustomCollectableCount = 0;
            if (chapterIndex != -1)
            {
                if (subAreaIndex == -1)
                {
                    foreach (InGameMapEntitiesData entityData in EntitiesData)
                    {
                        if (entityData.Type == "customCollectable" && entityData.CustomCollectableMapIcon == customCollectable && XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + entityData.CustomCollectableFlag))
                        {
                            CurrentCustomCollectableCount++;
                        }
                    }
                }
                else
                {
                    List<string> subAreaRooms = getSubAreaRooms(subAreaIndex);
                    foreach (string room in subAreaRooms)
                    {
                        foreach (InGameMapEntitiesData entityData in EntitiesData)
                        {
                            if (room == entityData.Room)
                            {
                                if (entityData.Type == "customCollectable" && entityData.CustomCollectableMapIcon == customCollectable && XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapterIndex + "_" + entityData.CustomCollectableFlag))
                                {
                                    CurrentCustomCollectableCount++;
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                for (int chapter = !MapScreen.hasInterlude ? 1 : 0; chapter < MapScreen.maxChapters; chapter++)
                {
                    foreach (InGameMapEntitiesData entityData in EntitiesData)
                    {
                        if (entityData.Type == "customCollectable" && entityData.CustomCollectableMapIcon == customCollectable && XaphanModule.ModSaveData.SavedFlags.Contains(Prefix + "_Ch" + chapter + "_" + entityData.CustomCollectableFlag))
                        {
                            CurrentCustomCollectableCount++;
                        }
                    }
                }
            }
            return CurrentCustomCollectableCount;
        }

        public int getTotalCustomCollectable(string customCollectable, int subAreaIndex = -1)
        {
            int TotalCustomCollectableCount = 0;
            if (subAreaIndex == -1)
            {
                foreach (InGameMapEntitiesData entityData in EntitiesData)
                {
                    if (entityData.Type == "customCollectable" && entityData.CustomCollectableMapIcon == customCollectable)
                    {
                        TotalCustomCollectableCount++;
                    }
                }
            }
            else
            {
                List<string> subAreaRooms = getSubAreaRooms(subAreaIndex);
                foreach (string room in subAreaRooms)
                {
                    foreach (InGameMapEntitiesData entityData in EntitiesData)
                    {
                        if (room == entityData.Room)
                        {
                            if (entityData.Type == "customCollectable" && entityData.CustomCollectableMapIcon == customCollectable)
                            {
                                TotalCustomCollectableCount++;
                            }
                        }
                    }
                }
            }
            return TotalCustomCollectableCount;
        }

        public override void Render()
        {
            if (!Initialized)
            {
                return;
            }
            base.Render();
            if (mode != 0)
            {
                int iconYPos = 0;
                int iconXPos = 209;
                int lineHeight = 60;
                int valueXPos = 70;
                int valueWidth = 0;
                int characterInline = 5;
                int characterImageHeight = (int)new Image(GFX.Gui["maps/keys/0"]).Height;
                List<int> linesYPos = new();

                if (!InGameMapControllerData.HideMapProgress && !NoMapTiles)
                {
                    Image mapIcon = new(GFX.Gui["maps/keys/map"]);
                    mapIcon.Position = new Vector2(Position.X, Position.Y + mapIcon.Height / 4);
                    mapIcon.Render();
                    linesYPos.Insert(iconYPos, (int)((Position.Y + lineHeight * iconYPos + mapIcon.Height / 4) + (mapIcon.Height - characterImageHeight) / 2));
                    iconYPos++;
                }
                if (chapterIndex != -1)
                {
                    if (!InGameMapControllerData.HideCassetteProgress && (subAreaIndex != -1 && mode == 1) ? SubAreaTotalCassette != 0 : TotalCassette != 0)
                    {
                        Image cassetteIcon = new(GFX.Gui["maps/keys/cassette"]);
                        cassetteIcon.Position = new Vector2(Position.X + iconXPos, Position.Y + cassetteIcon.Height / 4);
                        cassetteIcon.Render();
                        if (CurrentCassette == TotalCassette)
                        {
                            Image checkmark = new(GFX.Gui["maps/keys/checkmark"]);
                            checkmark.Position = cassetteIcon.Position + new Vector2(10f, 0f);
                            checkmark.Render();
                        }
                        else
                        {
                            Image crossmark = new(GFX.Gui["maps/keys/crossmark"]);
                            crossmark.Position = cassetteIcon.Position + new Vector2(10f, 0f);
                            crossmark.Render();
                        }
                        iconXPos += 81;
                    }
                    if (!InGameMapControllerData.HideHeartProgress && (subAreaIndex != -1 && mode == 1) ? SubAreaTotalHeart != 0 : TotalHeart != 0)
                    {
                        Image heartIcon = new(GFX.Gui["maps/keys/heart"]);
                        heartIcon.Position = new Vector2(Position.X + iconXPos, Position.Y + heartIcon.Height / 4);
                        heartIcon.Render();
                        if (CurrentHeart == TotalHeart)
                        {
                            Image checkmark = new(GFX.Gui["maps/keys/checkmark"]);
                            checkmark.Position = heartIcon.Position + new Vector2(10f, 0f);
                            checkmark.Render();
                        }
                        else
                        {
                            Image crossmark = new(GFX.Gui["maps/keys/crossmark"]);
                            crossmark.Position = heartIcon.Position + new Vector2(10f, 0f);
                            crossmark.Render();
                        }
                    }
                }
                if (!InGameMapControllerData.HideStrawberryProgress && (subAreaIndex != -1 && mode == 1) ? SubAreaTotalStrawberries != 0 : TotalStrawberries != 0)
                {
                    Image strawberryIcon = new(GFX.Gui["maps/keys/strawberry"]);
                    strawberryIcon.Position = new Vector2(Position.X, Position.Y + lineHeight * iconYPos + strawberryIcon.Height / 4);
                    strawberryIcon.Render();
                    linesYPos.Insert(iconYPos, (int)((Position.Y + lineHeight * iconYPos + strawberryIcon.Height / 4) + (strawberryIcon.Height - characterImageHeight) / 2));
                    iconYPos++;
                }
                if (!InGameMapControllerData.HideMoonberryProgress && (subAreaIndex != -1 && mode == 1) ? SubAreaTotalMoonberries != 0 && CurrentMoonberries != 0 : CurrentMoonberries != 0)
                {
                    Image moonberryIcon = new(GFX.Gui["maps/keys/moonberry"]);
                    moonberryIcon.Position = new Vector2(Position.X - 10f, Position.Y + lineHeight * iconYPos + moonberryIcon.Height / 4);
                    moonberryIcon.Render();
                    linesYPos.Insert(iconYPos, (int)((Position.Y + lineHeight * iconYPos + moonberryIcon.Height / 4) + (moonberryIcon.Height - characterImageHeight) / 2));
                    iconYPos++;
                }
                if (!InGameMapControllerData.HideUpgradeProgress && (subAreaIndex != -1 && mode == 1) ? SubAreaTotalStaminaUpgrades != 0 : TotalStaminaUpgrades != 0 && PowerGripUnlocked)
                {
                    Image staminaUpgradeIcon = new(GFX.Gui["maps/keys/energyTank"]);
                    staminaUpgradeIcon.Position = new Vector2(Position.X - 2f, Position.Y - 2f + lineHeight * iconYPos + staminaUpgradeIcon.Height / 4);
                    staminaUpgradeIcon.Render();
                    linesYPos.Insert(iconYPos, (int)((Position.Y + lineHeight * iconYPos + staminaUpgradeIcon.Height / 4) + (staminaUpgradeIcon.Height - characterImageHeight) / 2));
                    iconYPos++;
                }
                if (!InGameMapControllerData.HideUpgradeProgress && (subAreaIndex != -1 && mode == 1) ? SubAreaTotalDroneFireRateUpgrades != 0 : TotalDroneFireRateUpgrades != 0 && RemoteDroneUnlocked)
                {
                    Image fireRateUpgradeIcon = new(GFX.Gui["maps/keys/fireRateModule"]);
                    fireRateUpgradeIcon.Position = new Vector2(Position.X, Position.Y + lineHeight * iconYPos + fireRateUpgradeIcon.Height / 4);
                    fireRateUpgradeIcon.Render();
                    linesYPos.Insert(iconYPos, (int)((Position.Y + lineHeight * iconYPos + fireRateUpgradeIcon.Height / 4) + (fireRateUpgradeIcon.Height - characterImageHeight) / 2));
                    iconYPos++;
                }
                if (!InGameMapControllerData.HideUpgradeProgress && (subAreaIndex != -1 && mode == 1) ? SubAreaTotalDroneMissilesUpgrades != 0 : TotalDroneMissilesUpgrades != 0 && MissilesModuleUnlocked)
                {
                    Image DroneMissilesUpgradeIcon = new(GFX.Gui["maps/keys/missile"]);
                    DroneMissilesUpgradeIcon.Position = new Vector2(Position.X, Position.Y + lineHeight * iconYPos + DroneMissilesUpgradeIcon.Height / 4);
                    DroneMissilesUpgradeIcon.Render();
                    linesYPos.Insert(iconYPos, (int)((Position.Y + lineHeight * iconYPos + DroneMissilesUpgradeIcon.Height / 4) + (DroneMissilesUpgradeIcon.Height - characterImageHeight) / 2));
                    iconYPos++;
                }
                if (!InGameMapControllerData.HideUpgradeProgress && (subAreaIndex != -1 && mode == 1) ? SubAreaTotalDroneSuperMissilesUpgrades != 0 : TotalDroneSuperMissilesUpgrades != 0 && SuperMissilesModuleUnlocked)
                {
                    Image DroneSuperMissilesUpgradeIcon = new(GFX.Gui["maps/keys/superMissile"]);
                    DroneSuperMissilesUpgradeIcon.Position = new Vector2(Position.X - 2f, Position.Y - 2f + lineHeight * iconYPos + DroneSuperMissilesUpgradeIcon.Height / 4);
                    DroneSuperMissilesUpgradeIcon.Render();
                    linesYPos.Insert(iconYPos, (int)((Position.Y + lineHeight * iconYPos + DroneSuperMissilesUpgradeIcon.Height / 4) + (DroneSuperMissilesUpgradeIcon.Height - characterImageHeight) / 2));
                    iconYPos++;
                }
                for (int i = 0; i < InGameMapControllerData.CustomCollectablesProgress.Split(',').Length; i++)
                {
                    if (!InGameMapControllerData.SecretsCustomCollectablesProgress.Contains(InGameMapControllerData.CustomCollectablesProgress.Split(',')[i]))
                    {
                        if ((subAreaIndex != -1 && mode == 1) ? getTotalCustomCollectable(InGameMapControllerData.CustomCollectablesProgress.Split(',')[i], subAreaIndex) != 0 : getTotalCustomCollectable(InGameMapControllerData.CustomCollectablesProgress.Split(',')[i]) != 0)
                        {
                            Image customCollectableIcon = new(GFX.Gui["maps/" + Prefix + "/" + "keys/" + InGameMapControllerData.CustomCollectablesProgress.Split(',')[i]]);
                            customCollectableIcon.Position = new Vector2(Position.X, Position.Y + lineHeight * iconYPos + customCollectableIcon.Height / 4);
                            customCollectableIcon.Render();
                            linesYPos.Insert(iconYPos, (int)((Position.Y + lineHeight * iconYPos + customCollectableIcon.Height / 4) + (customCollectableIcon.Height - characterImageHeight) / 2));
                            iconYPos++;
                        }
                    }
                    else
                    {
                        if ((subAreaIndex != -1 && mode == 1) ? getTotalCustomCollectable(InGameMapControllerData.CustomCollectablesProgress.Split(',')[i], subAreaIndex) != 0 && getCurrentCustomCollectable(InGameMapControllerData.CustomCollectablesProgress.Split(',')[i]) != 0 : getCurrentCustomCollectable(InGameMapControllerData.CustomCollectablesProgress.Split(',')[i]) != 0)
                        {
                            Image customCollectableIcon = new(GFX.Gui["maps/" + Prefix + "/" + "keys/" + InGameMapControllerData.CustomCollectablesProgress.Split(',')[i]]);
                            customCollectableIcon.Position = new Vector2(Position.X, Position.Y + lineHeight * iconYPos + customCollectableIcon.Height / 4);
                            customCollectableIcon.Render();
                            linesYPos.Insert(iconYPos, (int)((Position.Y + lineHeight * iconYPos + customCollectableIcon.Height / 4) + (customCollectableIcon.Height - characterImageHeight) / 2));
                            iconYPos++;
                        }
                    }
                }
                if (chapterIndex == -1)
                {
                    if (!InGameMapControllerData.HideCassetteProgress && TotalCassette != 0)
                    {
                        Image cassetteIcon = new(GFX.Gui["maps/keys/cassette"]);
                        cassetteIcon.Position = new Vector2(Position.X, Position.Y + lineHeight * iconYPos + cassetteIcon.Height / 4);
                        cassetteIcon.Render();
                        linesYPos.Insert(iconYPos, (int)((Position.Y + lineHeight * iconYPos + cassetteIcon.Height / 4) + (cassetteIcon.Height - characterImageHeight) / 2));
                        iconYPos++;
                    }
                    if (!InGameMapControllerData.HideHeartProgress && TotalHeart != 0)
                    {
                        Image heartIcon = new(GFX.Gui["maps/keys/heart"]);
                        heartIcon.Position = new Vector2(Position.X, Position.Y + lineHeight * iconYPos + heartIcon.Height / 4);
                        heartIcon.Render();
                        linesYPos.Insert(iconYPos, (int)((Position.Y + lineHeight * iconYPos + heartIcon.Height / 4) + (heartIcon.Height - characterImageHeight) / 2));
                        iconYPos++;
                    }
                }
                if (!InGameMapControllerData.HideUpgradeProgress && (subAreaIndex != -1 && mode == 1) ? SubAreaTotalUpgrades != 0 : TotalUpgrades != 0)
                {
                    Image upgradeIcon = new(GFX.Gui["maps/keys/upgrade"]);
                    upgradeIcon.Position = new Vector2(Position.X, Position.Y + lineHeight * iconYPos + upgradeIcon.Height / 4);
                    upgradeIcon.Render();
                    linesYPos.Insert(iconYPos, (int)((Position.Y + lineHeight * iconYPos + upgradeIcon.Height / 4) + (upgradeIcon.Height - characterImageHeight) / 2));
                    iconYPos++;
                }
                iconYPos = 0;

                string character = "";
                if (mode == 2)
                {
                    if (!InGameMapControllerData.HideMapProgress && !NoMapTiles)
                    {
                        string MapPercentDisplay = MapPercent + "%";
                        for (int i = 0; i < MapPercentDisplay.Length; i++)
                        {
                            if (MapPercentDisplay[i] == '%')
                            {
                                character = "percent";
                            }
                            else
                            {
                                character = MapPercentDisplay[i].ToString();
                            }
                            Image characterImage = new(GFX.Gui["maps/keys/" + character]);
                            characterImage.Position = new Vector2(Position.X + valueXPos + valueWidth + characterInline, linesYPos[iconYPos]);
                            characterImage.Color = CurrentMapTiles == TotalMapTiles ? Color.Gold : Color.White;
                            characterImage.Render();
                            valueWidth += (int)characterImage.Width;
                        }
                        valueWidth = 0;
                        iconYPos++;
                    }

                    if (!InGameMapControllerData.HideStrawberryProgress && TotalStrawberries != 0)
                    {
                        string StrawberriesDisplay = (CurrentStrawberries + "/" + TotalStrawberries).ToString();
                        for (int i = 0; i < StrawberriesDisplay.Length; i++)
                        {
                            if (StrawberriesDisplay[i] == '/')
                            {
                                character = "slash";
                            }
                            else
                            {
                                character = StrawberriesDisplay[i].ToString();
                            }
                            Image characterImage = new(GFX.Gui["maps/keys/" + character]);
                            characterImage.Position = new Vector2(Position.X + valueXPos + valueWidth + characterInline, linesYPos[iconYPos]);
                            characterImage.Color = CurrentStrawberries == TotalStrawberries ? Color.Gold : Color.White;
                            characterImage.Render();
                            valueWidth += (int)characterImage.Width;
                        }
                        valueWidth = 0;
                        iconYPos++;
                    }

                    if (!InGameMapControllerData.HideMoonberryProgress && TotalMoonberries != 0 && CurrentMoonberries != 0)
                    {
                        string MoonberriesDisplay = (CurrentMoonberries + "/" + TotalMoonberries).ToString();
                        for (int i = 0; i < MoonberriesDisplay.Length; i++)
                        {
                            if (MoonberriesDisplay[i] == '/')
                            {
                                character = "slash";
                            }
                            else
                            {
                                character = MoonberriesDisplay[i].ToString();
                            }
                            Image characterImage = new(GFX.Gui["maps/keys/" + character]);
                            characterImage.Position = new Vector2(Position.X + valueXPos + valueWidth + characterInline, linesYPos[iconYPos]);
                            characterImage.Color = CurrentMoonberries == TotalMoonberries ? Color.Gold : Color.White;
                            characterImage.Render();
                            valueWidth += (int)characterImage.Width;
                        }
                        valueWidth = 0;
                        iconYPos++;
                    }

                    if (!InGameMapControllerData.HideUpgradeProgress && TotalStaminaUpgrades != 0 && (CurrentStaminaUpgrades != 0 || PowerGripUnlocked))
                    {
                        string StaminaUpgradeDisplay = (CurrentStaminaUpgrades + "/" + TotalStaminaUpgrades).ToString();
                        for (int i = 0; i < StaminaUpgradeDisplay.Length; i++)
                        {
                            if (StaminaUpgradeDisplay[i] == '/')
                            {
                                character = "slash";
                            }
                            else
                            {
                                character = StaminaUpgradeDisplay[i].ToString();
                            }
                            Image characterImage = new(GFX.Gui["maps/keys/" + character]);
                            characterImage.Position = new Vector2(Position.X + valueXPos + valueWidth + characterInline, linesYPos[iconYPos] - 3f);
                            characterImage.Color = CurrentStaminaUpgrades == TotalStaminaUpgrades ? Color.Gold : Color.White;
                            characterImage.Render();
                            valueWidth += (int)characterImage.Width;
                        }
                        valueWidth = 0;
                        iconYPos++;
                    }

                    if (!InGameMapControllerData.HideUpgradeProgress && TotalDroneFireRateUpgrades != 0 && (CurrentDroneFireRateUpgrades != 0 || RemoteDroneUnlocked))
                    {
                        string FireRateUpgradeDisplay = (CurrentDroneFireRateUpgrades + "/" + TotalDroneFireRateUpgrades).ToString();
                        for (int i = 0; i < FireRateUpgradeDisplay.Length; i++)
                        {
                            if (FireRateUpgradeDisplay[i] == '/')
                            {
                                character = "slash";
                            }
                            else
                            {
                                character = FireRateUpgradeDisplay[i].ToString();
                            }
                            Image characterImage = new(GFX.Gui["maps/keys/" + character]);
                            characterImage.Position = new Vector2(Position.X + valueXPos + valueWidth + characterInline, linesYPos[iconYPos]);
                            characterImage.Color = CurrentDroneFireRateUpgrades == TotalDroneFireRateUpgrades ? Color.Gold : Color.White;
                            characterImage.Render();
                            valueWidth += (int)characterImage.Width;
                        }
                        valueWidth = 0;
                        iconYPos++;
                    }

                    if (!InGameMapControllerData.HideUpgradeProgress && TotalDroneMissilesUpgrades != 0 && (CurrentDroneMissilesUpgrades != 0 || MissilesModuleUnlocked))
                    {
                        string DroneMissilesUpgradeDisplay = (CurrentDroneMissilesUpgrades + "/" + TotalDroneMissilesUpgrades).ToString();
                        for (int i = 0; i < DroneMissilesUpgradeDisplay.Length; i++)
                        {
                            if (DroneMissilesUpgradeDisplay[i] == '/')
                            {
                                character = "slash";
                            }
                            else
                            {
                                character = DroneMissilesUpgradeDisplay[i].ToString();
                            }
                            Image characterImage = new(GFX.Gui["maps/keys/" + character]);
                            characterImage.Position = new Vector2(Position.X + valueXPos + valueWidth + characterInline, linesYPos[iconYPos]);
                            characterImage.Color = CurrentDroneMissilesUpgrades == TotalDroneMissilesUpgrades ? Color.Gold : Color.White;
                            characterImage.Render();
                            valueWidth += (int)characterImage.Width;
                        }
                        valueWidth = 0;
                        iconYPos++;
                    }

                    if (!InGameMapControllerData.HideUpgradeProgress && TotalDroneSuperMissilesUpgrades != 0 && (CurrentDroneSuperMissilesUpgrades != 0 || SuperMissilesModuleUnlocked))
                    {
                        string DroneSuperMissilesUpgradeDisplay = (CurrentDroneSuperMissilesUpgrades + "/" + TotalDroneSuperMissilesUpgrades).ToString();
                        for (int i = 0; i < DroneSuperMissilesUpgradeDisplay.Length; i++)
                        {
                            if (DroneSuperMissilesUpgradeDisplay[i] == '/')
                            {
                                character = "slash";
                            }
                            else
                            {
                                character = DroneSuperMissilesUpgradeDisplay[i].ToString();
                            }
                            Image characterImage = new(GFX.Gui["maps/keys/" + character]);
                            characterImage.Position = new Vector2(Position.X + valueXPos + valueWidth + characterInline, linesYPos[iconYPos] - 3f);
                            characterImage.Color = CurrentDroneSuperMissilesUpgrades == TotalDroneSuperMissilesUpgrades ? Color.Gold : Color.White;
                            characterImage.Render();
                            valueWidth += (int)characterImage.Width;
                        }
                        valueWidth = 0;
                        iconYPos++;
                    }

                    for (int c = 0; c < InGameMapControllerData.CustomCollectablesProgress.Split(',').Length; c++)
                    {
                        if (!InGameMapControllerData.SecretsCustomCollectablesProgress.Contains(InGameMapControllerData.CustomCollectablesProgress.Split(',')[c]))
                        {
                            if (getTotalCustomCollectable(InGameMapControllerData.CustomCollectablesProgress.Split(',')[c]) != 0)
                            {
                                string CustomCollectableDisplay = (getCurrentCustomCollectable(InGameMapControllerData.CustomCollectablesProgress.Split(',')[c]) + "/" + getTotalCustomCollectable(InGameMapControllerData.CustomCollectablesProgress.Split(',')[c])).ToString();
                                for (int i = 0; i < CustomCollectableDisplay.Length; i++)
                                {
                                    if (CustomCollectableDisplay[i] == '/')
                                    {
                                        character = "slash";
                                    }
                                    else
                                    {
                                        character = CustomCollectableDisplay[i].ToString();
                                    }
                                    Image characterImage = new(GFX.Gui["maps/keys/" + character]);
                                    characterImage.Position = new Vector2(Position.X + valueXPos + valueWidth + characterInline, linesYPos[iconYPos]);
                                    characterImage.Color = getCurrentCustomCollectable(InGameMapControllerData.CustomCollectablesProgress.Split(',')[c]) == getTotalCustomCollectable(InGameMapControllerData.CustomCollectablesProgress.Split(',')[c]) ? Color.Gold : Color.White;
                                    characterImage.Render();
                                    valueWidth += (int)characterImage.Width;
                                }
                                valueWidth = 0;
                                iconYPos++;
                            }
                        }
                        else
                        {
                            if (getCurrentCustomCollectable(InGameMapControllerData.CustomCollectablesProgress.Split(',')[c]) != 0)
                            {
                                string CustomCollectableDisplay = (getCurrentCustomCollectable(InGameMapControllerData.CustomCollectablesProgress.Split(',')[c]) + "/" + getTotalCustomCollectable(InGameMapControllerData.CustomCollectablesProgress.Split(',')[c])).ToString();
                                for (int i = 0; i < CustomCollectableDisplay.Length; i++)
                                {
                                    if (CustomCollectableDisplay[i] == '/')
                                    {
                                        character = "slash";
                                    }
                                    else
                                    {
                                        character = CustomCollectableDisplay[i].ToString();
                                    }
                                    Image characterImage = new(GFX.Gui["maps/keys/" + character]);
                                    characterImage.Position = new Vector2(Position.X + valueXPos + valueWidth + characterInline, linesYPos[iconYPos]);
                                    characterImage.Color = getCurrentCustomCollectable(InGameMapControllerData.CustomCollectablesProgress.Split(',')[c]) == getTotalCustomCollectable(InGameMapControllerData.CustomCollectablesProgress.Split(',')[c]) ? Color.Gold : Color.White;
                                    characterImage.Render();
                                    valueWidth += (int)characterImage.Width;
                                }
                                valueWidth = 0;
                                iconYPos++;
                            }
                        }
                    }

                    if (chapterIndex == -1)
                    {
                        if (!InGameMapControllerData.HideCassetteProgress && TotalCassette != 0)
                        {
                            string CassettesDisplay = (CurrentCassette + "/" + TotalCassette).ToString();
                            for (int i = 0; i < CassettesDisplay.Length; i++)
                            {
                                if (CassettesDisplay[i] == '/')
                                {
                                    character = "slash";
                                }
                                else
                                {
                                    character = CassettesDisplay[i].ToString();
                                }
                                Image characterImage = new(GFX.Gui["maps/keys/" + character]);
                                characterImage.Position = new Vector2(Position.X + valueXPos + valueWidth + characterInline, linesYPos[iconYPos]);
                                characterImage.Color = CurrentCassette == TotalCassette ? Color.Gold : Color.White;
                                characterImage.Render();
                                valueWidth += (int)characterImage.Width;
                            }
                            valueWidth = 0;
                            iconYPos++;
                        }
                        if (!InGameMapControllerData.HideHeartProgress && TotalHeart != 0)
                        {
                            string HeartsDisplay = (CurrentHeart + "/" + TotalHeart).ToString();
                            for (int i = 0; i < HeartsDisplay.Length; i++)
                            {
                                if (HeartsDisplay[i] == '/')
                                {
                                    character = "slash";
                                }
                                else
                                {
                                    character = HeartsDisplay[i].ToString();
                                }
                                Image characterImage = new(GFX.Gui["maps/keys/" + character]);
                                characterImage.Position = new Vector2(Position.X + valueXPos + valueWidth + characterInline, linesYPos[iconYPos]);
                                characterImage.Color = CurrentHeart == TotalHeart ? Color.Gold : Color.White;
                                characterImage.Render();
                                valueWidth += (int)characterImage.Width;
                            }
                            valueWidth = 0;
                            iconYPos++;
                        }
                    }

                    if (!InGameMapControllerData.HideUpgradeProgress && TotalUpgrades != 0)
                    {
                        string UpgradeDisplay = (CurrentUpgrades + "/" + TotalUpgrades).ToString();
                        for (int i = 0; i < UpgradeDisplay.Length; i++)
                        {
                            if (UpgradeDisplay[i] == '/')
                            {
                                character = "slash";
                            }
                            else
                            {
                                character = UpgradeDisplay[i].ToString();
                            }
                            Image characterImage = new(GFX.Gui["maps/keys/" + character]);
                            characterImage.Position = new Vector2(Position.X + valueXPos + valueWidth + characterInline, linesYPos[iconYPos]);
                            characterImage.Color = CurrentUpgrades == TotalUpgrades ? Color.Gold : Color.White;
                            characterImage.Render();
                            valueWidth += (int)characterImage.Width;
                        }
                        valueWidth = 0;
                        iconYPos++;
                    }
                }
                else
                {
                    if (subAreaIndex != -1)
                    {
                        if (!InGameMapControllerData.HideMapProgress && !NoMapTiles)
                        {
                            string SubAreaMapPerecentDisplay = SubAreaMapPercent + "%";
                            for (int j = 0; j < SubAreaMapPerecentDisplay.Length; j++)
                            {
                                if (SubAreaMapPerecentDisplay[j] == '%')
                                {
                                    character = "percent";
                                }
                                else
                                {
                                    character = SubAreaMapPerecentDisplay[j].ToString();
                                }
                                Image characterImage = new(GFX.Gui["maps/keys/" + character]);
                                characterImage.Position = new Vector2(Position.X + valueXPos + valueWidth + characterInline, linesYPos[iconYPos]);
                                characterImage.Color = SubAreaCurrentMapTiles == SubAreaTotalMapTiles ? Color.Gold : Color.White;
                                characterImage.Render();
                                valueWidth += (int)characterImage.Width;
                            }
                            valueWidth = 0;
                            iconYPos++;
                        }

                        if (!InGameMapControllerData.HideStrawberryProgress && SubAreaTotalStrawberries != 0)
                        {
                            string StrawberriesDisplay = (SubAreaCurrentStrawberries + "/" + SubAreaTotalStrawberries).ToString();
                            for (int i = 0; i < StrawberriesDisplay.Length; i++)
                            {
                                if (StrawberriesDisplay[i] == '/')
                                {
                                    character = "slash";
                                }
                                else
                                {
                                    character = StrawberriesDisplay[i].ToString();
                                }
                                Image characterImage = new(GFX.Gui["maps/keys/" + character]);
                                characterImage.Position = new Vector2(Position.X + valueXPos + valueWidth + characterInline, linesYPos[iconYPos]);
                                characterImage.Color = SubAreaCurrentStrawberries == SubAreaTotalStrawberries ? Color.Gold : Color.White;
                                characterImage.Render();
                                valueWidth += (int)characterImage.Width;
                            }
                            valueWidth = 0;
                            iconYPos++;
                        }

                        if (!InGameMapControllerData.HideMoonberryProgress && SubAreaCurrentMoonberries != 0)
                        {
                            string MoonberriesDisplay = (SubAreaCurrentMoonberries + "/" + SubAreaTotalMoonberries).ToString();
                            for (int i = 0; i < MoonberriesDisplay.Length; i++)
                            {
                                if (MoonberriesDisplay[i] == '/')
                                {
                                    character = "slash";
                                }
                                else
                                {
                                    character = MoonberriesDisplay[i].ToString();
                                }
                                Image characterImage = new(GFX.Gui["maps/keys/" + character]);
                                characterImage.Position = new Vector2(Position.X + valueXPos + valueWidth + characterInline, linesYPos[iconYPos]);
                                characterImage.Color = SubAreaCurrentMoonberries == SubAreaTotalMoonberries ? Color.Gold : Color.White;
                                characterImage.Render();
                                valueWidth += (int)characterImage.Width;
                            }
                            valueWidth = 0;
                            iconYPos++;
                        }

                        if (!InGameMapControllerData.HideUpgradeProgress && SubAreaTotalStaminaUpgrades != 0 && (CurrentStaminaUpgrades != 0 || PowerGripUnlocked))
                        {
                            string StaminaUpgradeDisplay = (SubAreaCurrentStaminaUpgrades + "/" + SubAreaTotalStaminaUpgrades).ToString();
                            for (int i = 0; i < StaminaUpgradeDisplay.Length; i++)
                            {
                                if (StaminaUpgradeDisplay[i] == '/')
                                {
                                    character = "slash";
                                }
                                else
                                {
                                    character = StaminaUpgradeDisplay[i].ToString();
                                }
                                Image characterImage = new(GFX.Gui["maps/keys/" + character]);
                                characterImage.Position = new Vector2(Position.X + valueXPos + valueWidth + characterInline, linesYPos[iconYPos] - 3f);
                                characterImage.Color = SubAreaCurrentStaminaUpgrades == SubAreaTotalStaminaUpgrades ? Color.Gold : Color.White;
                                characterImage.Render();
                                valueWidth += (int)characterImage.Width;
                            }
                            valueWidth = 0;
                            iconYPos++;
                        }

                        if (!InGameMapControllerData.HideUpgradeProgress && SubAreaTotalDroneFireRateUpgrades != 0 && (CurrentDroneFireRateUpgrades != 0 || RemoteDroneUnlocked))
                        {
                            string FireRateUpgradeDisplay = (SubAreaCurrentDroneFireRateUpgrades + "/" + SubAreaTotalDroneFireRateUpgrades).ToString();
                            for (int i = 0; i < FireRateUpgradeDisplay.Length; i++)
                            {
                                if (FireRateUpgradeDisplay[i] == '/')
                                {
                                    character = "slash";
                                }
                                else
                                {
                                    character = FireRateUpgradeDisplay[i].ToString();
                                }
                                Image characterImage = new(GFX.Gui["maps/keys/" + character]);
                                characterImage.Position = new Vector2(Position.X + valueXPos + valueWidth + characterInline, linesYPos[iconYPos]);
                                characterImage.Color = SubAreaCurrentDroneFireRateUpgrades == SubAreaTotalDroneFireRateUpgrades ? Color.Gold : Color.White;
                                characterImage.Render();
                                valueWidth += (int)characterImage.Width;
                            }
                            valueWidth = 0;
                            iconYPos++;
                        }

                        if (!InGameMapControllerData.HideUpgradeProgress && SubAreaTotalDroneMissilesUpgrades != 0 && (CurrentDroneMissilesUpgrades != 0 || MissilesModuleUnlocked))
                        {
                            string DroneMissilesUpgradeDisplay = (SubAreaCurrentDroneMissilesUpgrades + "/" + SubAreaTotalDroneMissilesUpgrades).ToString();
                            for (int i = 0; i < DroneMissilesUpgradeDisplay.Length; i++)
                            {
                                if (DroneMissilesUpgradeDisplay[i] == '/')
                                {
                                    character = "slash";
                                }
                                else
                                {
                                    character = DroneMissilesUpgradeDisplay[i].ToString();
                                }
                                Image characterImage = new(GFX.Gui["maps/keys/" + character]);
                                characterImage.Position = new Vector2(Position.X + valueXPos + valueWidth + characterInline, linesYPos[iconYPos]);
                                characterImage.Color = SubAreaCurrentDroneMissilesUpgrades == SubAreaTotalDroneMissilesUpgrades ? Color.Gold : Color.White;
                                characterImage.Render();
                                valueWidth += (int)characterImage.Width;
                            }
                            valueWidth = 0;
                            iconYPos++;
                        }

                        if (!InGameMapControllerData.HideUpgradeProgress && SubAreaTotalDroneSuperMissilesUpgrades != 0 && (CurrentDroneSuperMissilesUpgrades != 0 || SuperMissilesModuleUnlocked))
                        {
                            string DroneSuperMissilesUpgradeDisplay = (SubAreaCurrentDroneSuperMissilesUpgrades + "/" + SubAreaTotalDroneSuperMissilesUpgrades).ToString();
                            for (int i = 0; i < DroneSuperMissilesUpgradeDisplay.Length; i++)
                            {
                                if (DroneSuperMissilesUpgradeDisplay[i] == '/')
                                {
                                    character = "slash";
                                }
                                else
                                {
                                    character = DroneSuperMissilesUpgradeDisplay[i].ToString();
                                }
                                Image characterImage = new(GFX.Gui["maps/keys/" + character]);
                                characterImage.Position = new Vector2(Position.X + valueXPos + valueWidth + characterInline, linesYPos[iconYPos]);
                                characterImage.Color = SubAreaCurrentDroneSuperMissilesUpgrades == SubAreaTotalDroneSuperMissilesUpgrades ? Color.Gold : Color.White;
                                characterImage.Render();
                                valueWidth += (int)characterImage.Width;
                            }
                            valueWidth = 0;
                            iconYPos++;
                        }

                        for (int c = 0; c < InGameMapControllerData.CustomCollectablesProgress.Split(',').Length; c++)
                        {
                            if (!InGameMapControllerData.SecretsCustomCollectablesProgress.Contains(InGameMapControllerData.CustomCollectablesProgress.Split(',')[c]))
                            {
                                if (getTotalCustomCollectable(InGameMapControllerData.CustomCollectablesProgress.Split(',')[c], subAreaIndex) != 0)
                                {
                                    string CustomCollectableDisplay = (getCurrentCustomCollectable(InGameMapControllerData.CustomCollectablesProgress.Split(',')[c], subAreaIndex) + "/" + getTotalCustomCollectable(InGameMapControllerData.CustomCollectablesProgress.Split(',')[c], subAreaIndex)).ToString();
                                    for (int i = 0; i < CustomCollectableDisplay.Length; i++)
                                    {
                                        if (CustomCollectableDisplay[i] == '/')
                                        {
                                            character = "slash";
                                        }
                                        else
                                        {
                                            character = CustomCollectableDisplay[i].ToString();
                                        }
                                        Image characterImage = new(GFX.Gui["maps/keys/" + character]);
                                        characterImage.Position = new Vector2(Position.X + valueXPos + valueWidth + characterInline, linesYPos[iconYPos]);
                                        characterImage.Color = getCurrentCustomCollectable(InGameMapControllerData.CustomCollectablesProgress.Split(',')[c], subAreaIndex) == getTotalCustomCollectable(InGameMapControllerData.CustomCollectablesProgress.Split(',')[c], subAreaIndex) ? Color.Gold : Color.White;
                                        characterImage.Render();
                                        valueWidth += (int)characterImage.Width;
                                    }
                                    valueWidth = 0;
                                    iconYPos++;
                                }
                            }
                            else
                            {
                                if (getTotalCustomCollectable(InGameMapControllerData.CustomCollectablesProgress.Split(',')[c], subAreaIndex) != 0 && getCurrentCustomCollectable(InGameMapControllerData.CustomCollectablesProgress.Split(',')[c]) != 0)
                                {
                                    string CustomCollectableDisplay = (getCurrentCustomCollectable(InGameMapControllerData.CustomCollectablesProgress.Split(',')[c], subAreaIndex) + "/" + getTotalCustomCollectable(InGameMapControllerData.CustomCollectablesProgress.Split(',')[c], subAreaIndex)).ToString();
                                    for (int i = 0; i < CustomCollectableDisplay.Length; i++)
                                    {
                                        if (CustomCollectableDisplay[i] == '/')
                                        {
                                            character = "slash";
                                        }
                                        else
                                        {
                                            character = CustomCollectableDisplay[i].ToString();
                                        }
                                        Image characterImage = new(GFX.Gui["maps/keys/" + character]);
                                        characterImage.Position = new Vector2(Position.X + valueXPos + valueWidth + characterInline, linesYPos[iconYPos]);
                                        characterImage.Color = getCurrentCustomCollectable(InGameMapControllerData.CustomCollectablesProgress.Split(',')[c], subAreaIndex) == getTotalCustomCollectable(InGameMapControllerData.CustomCollectablesProgress.Split(',')[c], subAreaIndex) ? Color.Gold : Color.White;
                                        characterImage.Render();
                                        valueWidth += (int)characterImage.Width;
                                    }
                                    valueWidth = 0;
                                    iconYPos++;
                                }
                            }
                        }

                        if (!InGameMapControllerData.HideUpgradeProgress && SubAreaTotalUpgrades != 0)
                        {
                            string UpgradeDisplay = (SubAreaCurrentUpgrades + "/" + SubAreaTotalUpgrades).ToString();
                            for (int i = 0; i < UpgradeDisplay.Length; i++)
                            {
                                if (UpgradeDisplay[i] == '/')
                                {
                                    character = "slash";
                                }
                                else
                                {
                                    character = UpgradeDisplay[i].ToString();
                                }
                                Image characterImage = new(GFX.Gui["maps/keys/" + character]);
                                characterImage.Position = new Vector2(Position.X + valueXPos + valueWidth + characterInline, linesYPos[iconYPos]);
                                characterImage.Color = SubAreaCurrentUpgrades == SubAreaTotalUpgrades ? Color.Gold : Color.White;
                                characterImage.Render();
                                valueWidth += (int)characterImage.Width;
                            }
                            valueWidth = 0;
                            iconYPos++;
                        }
                    }
                }
            }
        }
    }
}
