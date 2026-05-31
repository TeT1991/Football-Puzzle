using System;
using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private GlobalStateProcessor _globalStateProcessor;
    [SerializeField] private MainMenuView _mainMenuView;

    private SkinService _skinService;
    private SaveService _saveService;
    private GlobalGameStateService _gloabalGameStateService;


    private MainMenuModel _mainMenuModel;

    private GlobalGameState _initialGameState;

    private void Awake()
    {
        _initialGameState = GlobalGameState.MainMenu;

        CreateServices();
        RegisterServices();

        _mainMenuModel = new(_mainMenuView);

        _globalStateProcessor.Init();
        _gloabalGameStateService.SetState(_initialGameState);
    }

    private void CreateServices()
    {
        _gloabalGameStateService = new();
        _skinService = new();
        _saveService = new();
    }

    private void RegisterServices()
    {
        ServiceLocator.Register<ISkinService>(_skinService);
        ServiceLocator.Register<ISaveService>(_saveService);
        ServiceLocator.Register<IGlobalGameStateService>(_gloabalGameStateService);
    }
}

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