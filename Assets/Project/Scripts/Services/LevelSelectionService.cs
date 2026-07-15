public class LevelSelectionService : ILevelSelectionService
{
    public LeagueDefinition SelectedLeague { get; private set; }
    public LevelDefinition SelectedLevel { get; private set; }
    public int SelectedLevelIndex { get; private set; } = -1;

    public bool TrySelect(LevelDefinition level)
    {
        SelectedLevel = level;
        return true;
    }
}
