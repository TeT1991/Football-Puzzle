using System;

public class MainMenuPresenter : IDisposable
{
    private readonly MainMenuUIView _mainMenuUIView;
    private readonly SkinsShopUIView _skinsShopUIView;
    private readonly IGlobalGameStateService _globalStateGameService;
    private readonly ISkinService _skinService;

    public MainMenuPresenter(MainMenuUIView mainMenuView, SkinsShopUIView skinsShopUIView, ISkinService skinService, IGlobalGameStateService globalStateGameService)
    {
        _skinService = skinService;
        _mainMenuUIView = mainMenuView;
        _mainMenuUIView.Init();
        _skinsShopUIView = skinsShopUIView;
        _skinsShopUIView.Init(_skinService.GetCurrent());

        _globalStateGameService = globalStateGameService;

        _skinService.SkinChanged += _skinsShopUIView.MarkItemAsSelected;

        _mainMenuUIView.PlayButtonClicked += OnPlayButtonCliked;
        _mainMenuUIView.SkinsShopButtonClicked += OnSkinShopButtonClicked;

        _skinsShopUIView.SkinButonClicked += TryChangeSkin;
        _skinsShopUIView.CloseButtonCliked += OnSkinShopButtonCloseClicked;
    }

    private void TryChangeSkin(int id)
    {
        if (_skinService.IsUnlocked(id))
        {
            _skinService.TrySetCurrent(id);
        }
        else
        {
            UnityEngine.Debug.Log("Скин заблокирован");
        }
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
        _skinsShopUIView.SkinButonClicked -= TryChangeSkin;
        _skinService.SkinChanged -= _skinsShopUIView.MarkItemAsSelected;
    }
}