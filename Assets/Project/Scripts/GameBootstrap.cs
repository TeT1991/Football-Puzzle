using UnityEngine;

public class GameBootstrap : MonoBehaviour
{
    private SkinService _skinService;
    private SaveService _saveService;
    private GloabalGameStateService _gloabalGameStateService;

    private GlobalGameState _initialGameState;

    private void Awake()
    {
        _initialGameState = GlobalGameState.Loading;

        CreateServices();
        RegisterServices();
    }

    private void CreateServices()
    {
        _gloabalGameStateService = new(_initialGameState);
        _skinService = new();
        _saveService = new();
    }

    private void RegisterServices()
    {
        ServiceLocator.Register<ISkinService>(_skinService);
        ServiceLocator.Register<ISaveService>(_saveService);
    }
}