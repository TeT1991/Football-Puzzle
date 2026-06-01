using System;
using UnityEngine;

public class MainMenuPresenter : IDisposable
{
    private readonly MainMenuUIView _mainMenuUIView;
    private readonly SkinsShopUIView _skinsShopUIView;
    private readonly IGlobalGameStateService _globalStateGameService;

    public MainMenuPresenter(MainMenuUIView mainMenuView, SkinsShopUIView skinsShopUIView)
    {
        _mainMenuUIView = mainMenuView;
        _mainMenuUIView.Init();
        _skinsShopUIView = skinsShopUIView;
        _skinsShopUIView.Init();


        _globalStateGameService = ServiceLocator.Get<IGlobalGameStateService>();

        _mainMenuUIView.PlayButtonClicked += OnPlayButtonCliked;
        _mainMenuUIView.SkinsShopButtonClicked += OnSkinShopButtonClicked;
        _skinsShopUIView.CloseButtonCliked += OnSkinShopButtonCloseClicked;
    }

    private void OnPlayButtonCliked()
    {
        _globalStateGameService.SetState(GlobalGameState.Level);
    }

    private void OnSkinShopButtonClicked()
    {
        _mainMenuUIView.gameObject.SetActive(false);
        _skinsShopUIView.gameObject.SetActive(true);
    }

    private void OnSkinShopButtonCloseClicked()
    {
        _skinsShopUIView.gameObject.SetActive(false);
        _mainMenuUIView.gameObject.SetActive(true);
    }

    public void Dispose()
    {
        _mainMenuUIView.PlayButtonClicked -= OnPlayButtonCliked;
        _mainMenuUIView.SkinsShopButtonClicked -= OnSkinShopButtonClicked;
        _skinsShopUIView.CloseButtonCliked -= OnSkinShopButtonCloseClicked;
    }
}