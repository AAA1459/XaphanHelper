using Celeste.Mod.XaphanHelper.Data;
using System.Collections.Generic;

namespace Celeste.Mod.XaphanHelper
{
    static class PlayerStat 
    {
        public static List<PlayerStatData> GeneratePlayersClearsList()
        {
            List<PlayerStatData> list = new();

            /*list.Add(new PlayerStatData(
                name: "Player 1",
                medals: 300,
                blueCrystalHearts: 1,
                yellowCrystalHearts: 3,
                strawberries: 79,
                deaths: 1893,
                time: "4:00:00",
                version: "3.0.0"
            ));*/

            return list;
        }

        public static List<PlayerStatData> GeneratePlayersFullClearsList()
        {
            List<PlayerStatData> list = new();

            list.Add(new PlayerStatData(
                name: "Gamation ",
                medals: 1290,
                blueCrystalHearts: 2,
                yellowCrystalHearts: 4,
                strawberries: 135,
                deaths: 9949,
                time: "26:44:52",
                version: "3.0.0"
            ));

            return list;
        }

        public static List<PlayerStatData> GeneratePlayersGoldenClearsList()
        {
            List<PlayerStatData> list = new();

            /*list.Add(new PlayerStatData(
                name: "Player 1",
                medals: 300,
                blueCrystalHearts: 2,
                redCrystalHearts: 0,
                yellowCrystalHearts: 1,
                strawberries: 79,
                normalDeaths: 1893,
                bSideDeaths: 274,
                time: "2:00:00"
            ));*/

            return list;
        }

        public static List<PlayerStatData> GeneratePlayersGoldenFullClearsList()
        {
            List<PlayerStatData> list = new();

            /*list.Add(new PlayerStatData(
                name: "Player 1",
                medals: 300,
                blueCrystalHearts: 2,
                redCrystalHearts: 0,
                yellowCrystalHearts: 1,
                strawberries: 79,
                normalDeaths: 1893,
                bSideDeaths: 274,
                time: "2:00:00"
            ));*/

            return list;
        }
    }
}
