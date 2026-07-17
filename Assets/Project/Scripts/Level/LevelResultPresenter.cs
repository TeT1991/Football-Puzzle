using System;

public class LevelResultPresenter : IDisposable
{
    private readonly LevelResultView _levelResultView;
    private readonly IGlobalGameStateService _globalGameStateService;

    LevelCompletionData _result;

    public LevelResultPresenter(LevelResultView levelResultView)
    {
        _levelResultView = levelResultView;
        _levelResultView.Init();
        _globalGameStateService = ServiceLocator.Get<IGlobalGameStateService>();
        _levelResultView.ResetLevelButtonPressed += ResetLevel;
        _levelResultView.NextLevelButtonPressed += GoToNextLevel;
    }

    public void ApplyResultActions(LevelCompletionData result)
    {
        SetResultData(result);
        ShowResultText(result.Result);
        ShowStars(result.Result == LevelResult.Win ? result.StarsCount : 0);
        ShowEndLevelPanel();
    }

    private void ShowResultText(LevelResult result)
    {
        _levelResultView.SetResultText(result);
    }

    private void ResetLevel()
    {
        UnityEngine.Debug.Log("!!!!");
        _globalGameStateService.SetState(GlobalGameState.Level);
    }

    private void GoToNextLevel()
    {
        _globalGameStateService.SetState(GlobalGameState.MainMenu); //Пока вернемся в главное меню, потом сделаем переход на уровень
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
    }
}