using System;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuBootstrap : MonoBehaviour
{
    [SerializeField] private MainMenuUIView _mainMenuUIView;
    [SerializeField] private SkinsShopUIView _skinsShopUIView;

    private ISkinService _skinService;
    private IGlobalGameStateService _globalGameStateService;

    private List<IDisposable> _disposables;

    private MainMenuPresenter _mainMenuPresenter;

    private void Awake()
    {
        _skinService = ServiceLocator.Get<ISkinService>();
        _skinService.Unlock(1);

        _globalGameStateService = ServiceLocator.Get<IGlobalGameStateService>();
        _disposables = new();
        _mainMenuPresenter = new(_mainMenuUIView, _skinsShopUIView, _skinService, _globalGameStateService);
        _disposables.Add(_mainMenuPresenter);
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
