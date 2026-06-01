using System;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuBootstrap : MonoBehaviour
{
    [SerializeField] private GlobalStateProcessor _globalStateProcessor;
    [SerializeField] private MainMenuUIView _mainMenuUIView;
    [SerializeField] private SkinsShopUIView _skinsShopUIView;

    private List<IDisposable> _disposables;

    private MainMenuPresenter _mainMenuPresenter;

    private void Awake()
    {
        _disposables = new();
        _globalStateProcessor.Init();
        _mainMenuPresenter = new(_mainMenuUIView, _skinsShopUIView);
        _disposables.Add(_mainMenuPresenter);
    }

    private void OnDestroy()
    {
        foreach(var item in _disposables)
        {
            Debug.Log(item.GetType() + "is dispoased");
            item.Dispose();
        }
    }
}
