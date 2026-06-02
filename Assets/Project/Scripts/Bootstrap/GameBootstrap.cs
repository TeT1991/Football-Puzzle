using UnityEngine;
using UnityEngine.SceneManagement;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private SkinsCatalog _skinsCatalog;
    [SerializeField] private GlobalStateProcessor _globalStateProcessor;
    [SerializeField] private int _initialSkinId = 0;

    private SkinService _skinService;
    private SaveService _saveService;
    private GlobalGameStateService _gloabalGameStateService;

    private void Awake()
    {
        CreateServices();
        RegisterServices();

        InitCurrentSkin();
        _globalStateProcessor.Init();

        string mainSceneName = "MainMenu";
        SceneManager.LoadScene(mainSceneName);
    }

    private void CreateServices()
    {
        _gloabalGameStateService = new();
        _skinService = new(_skinsCatalog);
        _saveService = new(_initialSkinId);
    }

    private void RegisterServices()
    {
        ServiceLocator.Register<ISkinService>(_skinService);
        ServiceLocator.Register<ISaveService>(_saveService);
        ServiceLocator.Register<IGlobalGameStateService>(_gloabalGameStateService);
    }

    private void InitCurrentSkin()
    {
        int currentSkinId = _saveService.CurrentSkinId;
        _skinService.SetCurrent(currentSkinId);
    }
}