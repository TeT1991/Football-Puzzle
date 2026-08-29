public interface ILevelSelectionService : IService
{
    LeagueDefinition SelectedLeague { get; }
    LevelDefinition SelectedLevel { get; }
    int SelectedLevelIndex { get; }

    bool TrySelect(LeagueDefinition leagueDefinition, int levelIndex);
}
