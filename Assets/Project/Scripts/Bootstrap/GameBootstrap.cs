using UnityEngine;
using UnityEngine.SceneManagement;
using YG;

public class GameBootstrap : MonoBehaviour
{
    [SerializeField] private SkinsCatalog _skinsCatalog;
    [SerializeField] private LeaguesCatalog _leaguesCatalog;
    [SerializeField] private GlobalStateProcessor _globalStateProcessor;
    [SerializeField] private int _initialSkinId = 0;

    private SaveService _saveService;
    private SkinService _skinService;
    private GlobalGameStateService _gloabalGameStateService;
    private WalletService _walletService;
    private LevelSelectionService _levelSelectionService;
    private LevelProgressionService _levelProgressionService;

    private void Awake()
    {
        YG2.onGetSDKData += OnPYG2Initialized;
    }

    private void OnPYG2Initialized()
    {
        if (YG2.isSDKEnabled)
        {
            YG2.onGetSDKData -= OnPYG2Initialized;
        }

        Debug.Log("PYG Initialized");

        RegisterServices();

        InitCurrentSkin();
        _globalStateProcessor.Init();

        string mainSceneName = "MainMenu";
        SceneManager.LoadScene(mainSceneName);
    }

    private void RegisterServices()
    {
        _saveService = new(_initialSkinId);
        ServiceLocator.Register<ISaveService>(_saveService);

        _skinService = new(_saveService,_skinsCatalog);
        ServiceLocator.Register<ISkinService>(_skinService);

        _gloabalGameStateService = new();
        ServiceLocator.Register<IGlobalGameStateService>(_gloabalGameStateService);

        _levelSelectionService = new();
        ServiceLocator.Register<ILevelSelectionService>(_levelSelectionService);

        _levelProgressionService = new(_saveService, _levelSelectionService, _leaguesCatalog);
        ServiceLocator.Register<ILevelProgressionService>(_levelProgressionService);

        _walletService = new(_saveService);
        ServiceLocator.Register<IWalletService>(_walletService);
        _walletService.IncreaseCurrency(CurrencyTypes.Soft, 123); // тестово, потом убрать
        _walletService.IncreaseCurrency(CurrencyTypes.Hard, 50); //тестово, потом убрать
    }

    private void InitCurrentSkin()
    {
        int currentSkinId = _saveService.CurrentSkinId;
        _skinService.TrySetCurrent(currentSkinId);
    }

    private void OnDestroy()
    {
        YG2.onGetSDKData -= OnPYG2Initialized;
    }
}
