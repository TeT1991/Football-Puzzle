using System;

public class LevelResultPresenter : IDisposable
{
    private readonly LevelResultView _levelResultView;
    private readonly IGlobalGameStateService _globalGameStateService;
    private readonly ILevelProgressionService _levelProgressionService;

    LevelCompletionData _result;

    public LevelResultPresenter(LevelResultView levelResultView)
    {
        _levelResultView = levelResultView;
        _levelResultView.Init();
        _globalGameStateService = ServiceLocator.Get<IGlobalGameStateService>();
        _levelProgressionService = ServiceLocator.Get<ILevelProgressionService>();

        _levelResultView.ResetLevelButtonPressed += ResetLevel;
        _levelResultView.NextLevelButtonPressed += GoToNextLevel;
        _levelResultView.MainMenuButtonPressed += GoToMainMenu;
    }

    public void ApplyResultActions(LevelCompletionData result)
    {
        SetResultData(result);
        ShowResultText(result.Result);
        ShowStars(result.StarsCount);
        ShowEndLevelPanel();
        TrySaveProgress();
    }

    private void TrySaveProgress()
    {
        if (_result.Result != LevelResult.Win)
        {
            return;
        }

        _levelProgressionService.SaveProgress();
    }

    private void ShowResultText(LevelResult result)
    {
        _levelResultView.SetResultText(result);
    }

    private void ResetLevel()
    {
        _globalGameStateService.SetState(GlobalGameState.Level);
    }

    private void GoToNextLevel()
    {
        if (_result.Result != LevelResult.Win)
        {
            _globalGameStateService.SetState(GlobalGameState.Level);
            return;
        }

        if (_levelProgressionService.TrySelectNextLevel())
        {
            _globalGameStateService.SetState(GlobalGameState.Level);
            return;
        }

        _globalGameStateService.SetState(GlobalGameState.MainMenu);
    }

    private void GoToMainMenu()
    {
        _globalGameStateService.SetState(GlobalGameState.MainMenu);
    }

    private void ShowStars(int count)
    {
        _levelResultView.ShowStars(count);
    }

    private void ShowEndLevelPanel()
    {
        _levelResultView.gameObject.SetActive(true);
    }

    private void SetResultData(LevelCompletionData result)
    {
        _result = result;
    }

    public void Dispose()
    {
        _levelResultView.ResetLevelButtonPressed -= ResetLevel;
        _levelResultView.NextLevelButtonPressed -= GoToNextLevel;
        _levelResultView.MainMenuButtonPressed += GoToMainMenu;
    }
}
