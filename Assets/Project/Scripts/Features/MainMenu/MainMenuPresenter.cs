using System;

public class MainMenuPresenter : IDisposable
{
    private readonly MainMenuUIView _mainMenuUIView;
    private readonly SkinsShopUIView _skinsShopUIView;
    private readonly IGlobalGameStateService _globalStateGameService;
    private readonly ISkinService _skinService;
    private readonly IWalletService _walletService;

    public MainMenuPresenter(MainMenuUIView mainMenuView, SkinsShopUIView skinsShopUIView, 
        ISkinService skinService, IGlobalGameStateService globalStateGameService, IWalletService walletService)
    {
        _globalStateGameService = globalStateGameService;
        _skinService = skinService;
        _walletService = walletService;

        _mainMenuUIView = mainMenuView;
        _mainMenuUIView.Init();
        _skinsShopUIView = skinsShopUIView;
        _skinsShopUIView.Init(_skinService.GetCurrent());


        _skinService.SkinChanged += _skinsShopUIView.MarkItemAsSelected;

        _mainMenuUIView.PlayButtonClicked += OnPlayButtonCliked;
        _mainMenuUIView.SkinsShopButtonClicked += OnSkinShopButtonClicked;

        _skinsShopUIView.SkinButonClicked += TryChangeSkin;
        _skinsShopUIView.BuyButtonClicked += TryBuySkin;
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

    private void TryBuySkin(int id)
    {
        SkinDefinition skin = _skinService.GetSkin(id);

        if(skin == null || _skinService.IsUnlocked(id))
        {
            throw new Exception($"Cant buy skin {id}");
        }

        int price = skin.Price;
        CurrencyTypes currencyType = skin.CurrencyType;

        if(_walletService.TrySpend(currencyType, price))
        {
            _skinService.Unlock(id);
            _skinsShopUIView.Unlock(id);
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
        _skinsShopUIView.BuyButtonClicked -= TryBuySkin;
        _skinsShopUIView.SkinButonClicked -= TryChangeSkin;
        _skinService.SkinChanged -= _skinsShopUIView.MarkItemAsSelected;
    }
}