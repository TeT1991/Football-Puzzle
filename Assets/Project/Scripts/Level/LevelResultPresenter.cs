using System;
using System.Diagnostics;
using UnityEngine;

public class LevelResultPresenter : IDisposable
{
    private readonly LevelResultView _levelResultView;
    private readonly IGlobalGameStateService _globalGameStateService;


    public LevelResultPresenter(LevelResultView levelResultView)
    {
        _levelResultView = levelResultView;
        _levelResultView.Init();
        _globalGameStateService = ServiceLocator.Get<IGlobalGameStateService>();
        _levelResultView.ResetLevelButtonPressed += ResetLevel;
        _levelResultView.NextLevelButtonPressed += GoToNextLevel;
    }

    public void ApplyResultActions(LevelResult result)
    {
        ShowEndLevelPanel();

        switch (result)
        {
            case LevelResult.Win:
                ApplyWinActions();
                break;

            case LevelResult.Lose:
                ApplyLoseActions();
                break;
        }
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

    private void ApplyWinActions()
    {
        _levelResultView.ShowStars(3); //тестово, потом поменять
    }

    private void ApplyLoseActions()
    {
        UnityEngine.Debug.Log("Lose");
    }

    private void ShowEndLevelPanel()
    {
        _levelResultView.gameObject.SetActive(true);
    }

    public void Dispose()
    {
        _levelResultView.ResetLevelButtonPressed -= ResetLevel;
        _levelResultView.NextLevelButtonPressed -= GoToNextLevel;
    }
}