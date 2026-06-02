using System;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuBootstrap : MonoBehaviour
{
    [SerializeField] private MainMenuUIView _mainMenuUIView;
    [SerializeField] private SkinsShopUIView _skinsShopUIView;

    private ISkinService _skinService;

    private List<IDisposable> _disposables;

    private MainMenuPresenter _mainMenuPresenter;

    private void Awake()
    {
        _skinService = ServiceLocator.Get<ISkinService>();
        _disposables = new();
        _mainMenuPresenter = new(_mainMenuUIView, _skinsShopUIView, _skinService);
        _disposables.Add(_mainMenuPresenter);

        _skinsShopUIView.Init();
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
