namespace Celeste.Mod.XaphanHelper.Data
{
    public class PlayerStatData
    {
        public string Name;

        public int Medals;

        public int BlueCrystalHearts;

        public int YellowCrystalHearts;

        public int Strawberries;

        public int Deaths;

        public string Time;

        public string Version;

        public PlayerStatData(string name, int medals, int blueCrystalHearts, int yellowCrystalHearts, int strawberries, int deaths, string time, string version)
        {
            Name = name;
            Medals = medals;
            BlueCrystalHearts = blueCrystalHearts;
            YellowCrystalHearts = yellowCrystalHearts;
            Strawberries = strawberries;
            Deaths = deaths;
            Time = time;
            Version = version;
        }
    }
}
