public class LevelSelectionService : ILevelSelectionService
{
    public LeagueDefinition SelectedLeague { get; private set; }
    public LevelDefinition SelectedLevel { get; private set; }
    public int SelectedLevelIndex { get; private set; } = -1;

    public bool TrySelect(LeagueDefinition league, int levelIndex)
    {
        if (league == null)
        {
            return false;
        }

        if (levelIndex < 0 || levelIndex >= league.Levels.Count)
        {
            return false;
        }

        SelectedLeague = league;
        SelectedLevelIndex = levelIndex;
        SelectedLevel = league.Levels[levelIndex];

        return SelectedLevel != null;
    }
}
