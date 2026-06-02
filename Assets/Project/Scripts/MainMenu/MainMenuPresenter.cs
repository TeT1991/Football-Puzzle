using System;

public class MainMenuPresenter : IDisposable
{
    private readonly MainMenuUIView _mainMenuUIView;
    private readonly SkinsShopUIView _skinsShopUIView;
    private readonly IGlobalGameStateService _globalStateGameService;
    private readonly ISkinService _skinService;

    public MainMenuPresenter(MainMenuUIView mainMenuView, SkinsShopUIView skinsShopUIView, ISkinService skinService)
    {
        _mainMenuUIView = mainMenuView;
        _mainMenuUIView.Init();
        _skinsShopUIView = skinsShopUIView;
        _skinsShopUIView.Init();
        _skinService = skinService;


        _globalStateGameService = ServiceLocator.Get<IGlobalGameStateService>();

        _mainMenuUIView.PlayButtonClicked += OnPlayButtonCliked;
        _mainMenuUIView.SkinsShopButtonClicked += OnSkinShopButtonClicked;
        _skinsShopUIView.CloseButtonCliked += OnSkinShopButtonCloseClicked;
    }

    private void OnPlayButtonCliked()
    {
        _globalStateGameService.SetState(GlobalGameState.Level);
    }

    private void OnSkinShopButtonClicked()
    {
        _mainMenuUIView.gameObject.SetActive(false);
        _skinsShopUIView.gameObject.SetActive(true);

        _skinsShopUIView.CreateItems(_skinService.GetDatas());
    }

    private void OnSkinShopButtonCloseClicked()
    {
        _skinsShopUIView.gameObject.SetActive(false);
        _mainMenuUIView.gameObject.SetActive(true);
    }

    public void Dispose()
    {
        _mainMenuUIView.PlayButtonClicked -= OnPlayButtonCliked;
        _mainMenuUIView.SkinsShopButtonClicked -= OnSkinShopButtonClicked;
        _skinsShopUIView.CloseButtonCliked -= OnSkinShopButtonCloseClicked;
    }
}