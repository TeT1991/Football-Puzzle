public interface ILevelSelectionService : IService
{
    LeagueDefinition SelectedLeague { get; }
    LevelDefinition SelectedLevel { get; }
    int SelectedLevelIndex { get; }

    bool TrySelect(LeagueDefinition league, int levelIndex);
    bool TrySelectNext();
}
