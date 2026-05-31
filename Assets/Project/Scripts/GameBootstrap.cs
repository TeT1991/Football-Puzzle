using UnityEngine;
using UnityEngine.SceneManagement;

public class GameBootstrap : MonoBehaviour
{
    private SkinService _skinService;
    private SaveService _saveService;
    private GlobalGameStateService _gloabalGameStateService;

    private void Awake()
    {
        CreateServices();
        RegisterServices();

        string mainSceneName = "MainMenu";
        SceneManager.LoadScene(mainSceneName);
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