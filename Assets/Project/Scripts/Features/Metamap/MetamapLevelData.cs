using System;

[Serializable]
public readonly struct MetamapLevelData
{
    public readonly int _leagueIndex;
    public readonly int _levelIndex;

    public MetamapLevelData(int leagueIndex, int levelIndex)
    {
        _leagueIndex = leagueIndex;
        _levelIndex = levelIndex;
    }
}
