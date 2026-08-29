public class LevelProgressionService : ILevelProgressionService
{
    private readonly ISaveService _saveService;
    private readonly ILevelSelectionService _levelSelectionService;
    private readonly LeaguesCatalog _leaguesCatalog;

    public LevelProgressionService(
        ISaveService saveService,
        ILevelSelectionService levelSelectionService,
        LeaguesCatalog leaguesCatalog)
    {
        _saveService = saveService;
        _levelSelectionService = levelSelectionService;
        _leaguesCatalog = leaguesCatalog;
    }

    public void SaveProgress()
    {
        LeagueDefinition currentLeague = _levelSelectionService.SelectedLeague;

        if (currentLeague == null)
        {
            return;
        }

        int nextLevelIndex = _levelSelectionService.SelectedLevelIndex + 1;

        if (nextLevelIndex < currentLeague.Levels.Count)
        {
            _saveService.UnlockLevel(currentLeague.ID, nextLevelIndex);
            return;
        }

        if (_leaguesCatalog == null ||
            _leaguesCatalog.TryGetNextLeague(currentLeague, out LeagueDefinition nextLeague) == false)
        {
            return;
        }

        _saveService.UnlockLevel(nextLeague.ID, 0);
    }

    public bool TrySelectNextLevel()
    {
        LeagueDefinition currentLeague = _levelSelectionService.SelectedLeague;

        if (currentLeague == null)
        {
            return false;
        }

        int nextLevelIndex = _levelSelectionService.SelectedLevelIndex + 1;

        if (nextLevelIndex < currentLeague.Levels.Count)
        {
            return _levelSelectionService.TrySelect(currentLeague, nextLevelIndex);
        }

        if (_leaguesCatalog == null ||
            _leaguesCatalog.TryGetNextLeague(currentLeague, out LeagueDefinition nextLeague) == false)
        {
            return false;
        }

        return _levelSelectionService.TrySelect(nextLeague, 0);
    }


}
