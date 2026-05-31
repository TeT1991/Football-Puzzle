using System;

public class MainMenuModel : IDisposable
{
    private readonly MainMenuView _mainMenuView;
    private readonly IGlobalGameStateService _globalStateGameService;

    public MainMenuModel(MainMenuView mainMenuView)
    {
        _mainMenuView = mainMenuView;
        _globalStateGameService = ServiceLocator.Get<IGlobalGameStateService>();

        _mainMenuView.PlayButtonClicked += OnPlayButtonCliked;
    }

    private void OnPlayButtonCliked()
    {
        _globalStateGameService.SetState(GlobalGameState.Level);
    }

    public void Dispose()
    {
        _mainMenuView.PlayButtonClicked -= OnPlayButtonCliked;
    }
}