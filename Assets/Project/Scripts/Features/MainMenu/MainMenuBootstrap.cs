using System;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuBootstrap : MonoBehaviour
{
    [SerializeField] private MainMenuUIView _mainMenuUIView;
    [SerializeField] private SkinsShopUIView _skinsShopUIView;
    [SerializeField] private LevelSelectionUIView _levelSelectionUIView;
    [SerializeField] private MoneyPanelView _softCurrencyPanelView;
    [SerializeField] private MoneyPanelView _hardCurrencyPanelView;

    private ISkinService _skinService;
    private IGlobalGameStateService _globalGameStateService;
    private IWalletService _walletService;
    private ILeagueService _leagueService;

    private List<IDisposable> _disposables;

    private MainMenuPresenter _mainMenuPresenter;
    private MoneyPanelPresenter _moneyPanelPresenter;

    private void Awake()
    {
        _globalGameStateService = ServiceLocator.Get<IGlobalGameStateService>();
        _skinService = ServiceLocator.Get<ISkinService>();
        _walletService = ServiceLocator.Get<IWalletService>();
        _leagueService = ServiceLocator.Get<ILeagueService>();

        _disposables = new();
        _mainMenuPresenter = new(_mainMenuUIView, _skinsShopUIView, _levelSelectionUIView, _skinService, _globalGameStateService, _walletService, _leagueService);
        _disposables.Add(_mainMenuPresenter);

        _moneyPanelPresenter = new(_walletService, _softCurrencyPanelView, _hardCurrencyPanelView);
        _disposables.Add(_moneyPanelPresenter);

        _leagueService = ServiceLocator.Get<ILeagueService>();

    }

    private void OnDestroy()
    {
        foreach (var item in _disposables)
        {
            Debug.Log(item.GetType() + "is dispoased");
            item.Dispose();
        }
    }
}
