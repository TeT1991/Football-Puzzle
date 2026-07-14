using System;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuBootstrap : MonoBehaviour
{
    [SerializeField] private GameInput _gameInput;

    [SerializeField] private MainMenuUIView _mainMenuUIView;
    [SerializeField] private SkinsShopUIView _skinsShopUIView;
    [SerializeField] private MoneyPanelView _softCurrencyPanelView;
    [SerializeField] private MoneyPanelView _hardCurrencyPanelView;
    [SerializeField] private Transform _metamapParent;
    [SerializeField] private Transform _metamapStartPoint;
    [SerializeField] private LeaguesCatalog _leaguesCatalog;
    [SerializeField] private MetamapScroller _metaMapScroller;

    private ISkinService _skinService;
    private IGlobalGameStateService _globalGameStateService;
    private IWalletService _walletService;

    private List<IDisposable> _disposables;

    private MainMenuPresenter _mainMenuPresenter;
    private MoneyPanelPresenter _moneyPanelPresenter;

    private MetamapBuilder _metamapBuilder;

    private void Awake()
    {
        _globalGameStateService = ServiceLocator.Get<IGlobalGameStateService>();
        _skinService = ServiceLocator.Get<ISkinService>();
        _walletService = ServiceLocator.Get<IWalletService>();

        _disposables = new();
        _mainMenuPresenter = new(_mainMenuUIView, _skinsShopUIView, _skinService, _globalGameStateService, _walletService);
        _disposables.Add(_mainMenuPresenter);

        _moneyPanelPresenter = new(_walletService, _softCurrencyPanelView, _hardCurrencyPanelView);
        _disposables.Add(_moneyPanelPresenter);

        _metamapBuilder = new(_metamapParent, _metamapStartPoint);
        CreateMetamap();

        _metaMapScroller.Init(_gameInput,_metamapBuilder.Bounds);
    }

    private void CreateMetamap()
    {
        for (int i = 0; i < _leaguesCatalog.Catalog.Count; i++)
        {
            MetamapLocationView location = _leaguesCatalog.Catalog[i].MetamapLocationView;
            _metamapBuilder.CreateLocation(location);
        }
    }

    private void OnDestroy()
    {
        foreach (var item in _disposables)
        {
            //Debug.Log(item.GetType() + "is dispoased");
            item.Dispose();
        }
    }
}
