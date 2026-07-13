
using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public int SoftCurrency;
    public int HardCurrency;

    public int CurrentSkinId;
    public List<int> UnlockedSkins;
    public List<LeagueProgressData> LeagueProgress;

    public static SaveData CreateNew()
    {
        return new SaveData
        {
            SoftCurrency = 0,
            HardCurrency = 0,
            CurrentSkinId = 0,
            UnlockedSkins = new() { 0 },
            LeagueProgress = new()
        };
    }
}

[System.Serializable]
public class LeagueProgressData
{
    public int LeagueId;
    public int UnlockedLevelCount = 1;
}
