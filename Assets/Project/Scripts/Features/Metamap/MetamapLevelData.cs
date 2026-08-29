using System;

[Serializable]
public readonly struct MetamapLevelData
{
    public readonly int LeagueIndex;
    public readonly int LevelIndex;

    public MetamapLevelData(int leagueIndex, int levelIndex)
    {
        LeagueIndex = leagueIndex;
        LevelIndex = levelIndex;
    }
}
