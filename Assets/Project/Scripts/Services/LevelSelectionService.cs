public class LevelSelectionService : ILevelSelectionService
{
    public LeagueDefinition SelectedLeague { get; private set; }
    public LevelDefinition SelectedLevel { get; private set; }
    public int SelectedLevelIndex { get; private set; } = -1;

    public bool TrySelect(LeagueDefinition league, int levelIndex)
    {
        if (league == null ||
            levelIndex < 0 ||
            levelIndex >= league.Levels.Count ||
            league.Levels[levelIndex] == null)
        {
            return false;
        }

        SelectedLeague = league;
        SelectedLevelIndex = levelIndex;
        SelectedLevel = league.Levels[levelIndex];
        return true;
    }

    public bool TrySelectNext()
    {
        if (SelectedLeague == null)
        {
            return false;
        }

        return TrySelect(SelectedLeague, SelectedLevelIndex + 1);
    }
}
