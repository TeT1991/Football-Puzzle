using UnityEngine;
using UnityEngine.SceneManagement;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private SkinsCatalog _skinsCatalog;

    private SkinService _skinService;
    private SaveService _saveService;
    private GlobalGameStateService _gloabalGameStateService;

    private void Awake()
    {
        CreateServices();
        RegisterServices();

        SetInitialSkin();

        string mainSceneName = "MainMenu";
        SceneManager.LoadScene(mainSceneName);
    }

    private void CreateServices()
    {
        _gloabalGameStateService = new();
        _skinService = new(_skinsCatalog);
        _saveService = new();
    }

    private void RegisterServices()
    {
        ServiceLocator.Register<ISkinService>(_skinService);
        ServiceLocator.Register<ISaveService>(_saveService);
        ServiceLocator.Register<IGlobalGameStateService>(_gloabalGameStateService);
    }

    private void SetInitialSkin()
    {
        string skinIdname = "SkinId";
        if (PlayerPrefs.HasKey(skinIdname))
        {
            int id = PlayerPrefs.GetInt(skinIdname);
            _skinService.SetCurrent(id);
        }
        else
        {
            _skinService.SetCurrent(0);
        }
    }
}