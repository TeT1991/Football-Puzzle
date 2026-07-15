using System;
using System.Diagnostics;

public class MainMenuPresenter : IDisposable
{
    private readonly MainMenuUIView _mainMenuUIView;
    private readonly SkinsShopUIView _skinsShopUIView;

    private readonly ISkinService _skinService;
    private readonly IWalletService _walletService;

    public MainMenuPresenter(MainMenuUIView mainMenuView, SkinsShopUIView skinsShopUIView,
        ISkinService skinService, IWalletService walletService)
    {
        _skinService = skinService;
        _walletService = walletService;

        _mainMenuUIView = mainMenuView;
        _mainMenuUIView.Init();
        _skinsShopUIView = skinsShopUIView;
        _skinsShopUIView.Init(_skinService.GetCurrent());

        _skinService.SkinChanged += _skinsShopUIView.MarkItemAsSelected;

        _mainMenuUIView.SkinsShopButtonClicked += OnSkinShopButtonClicked;

        _skinsShopUIView.SkinButonClicked += TryChangeSkin;
        _skinsShopUIView.BuyButtonClicked += TryBuySkin;
        _skinsShopUIView.CloseButtonClicked += CloseAllWindows;
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

    private void OnSkinShopButtonClicked()
    {
        CloseAllWindows();
        _skinsShopUIView.gameObject.SetActive(true);

        _skinsShopUIView.CreateItems(_skinService.GetDatas());
    }

    private void OnSkinShopButtonCloseClicked()
    {
        CloseAllWindows();
        _mainMenuUIView.gameObject.SetActive(true);
    }


    private void CloseAllWindows()
    {
        _mainMenuUIView.gameObject.SetActive(true);
        _skinsShopUIView.gameObject.SetActive(false);
    }

    public void Dispose()
    {
        _mainMenuUIView.SkinsShopButtonClicked -= OnSkinShopButtonClicked;
        _skinsShopUIView.CloseButtonClicked -= CloseAllWindows;
        _skinsShopUIView.BuyButtonClicked -= TryBuySkin;
        _skinsShopUIView.SkinButonClicked -= TryChangeSkin;
        _skinService.SkinChanged -= _skinsShopUIView.MarkItemAsSelected;
    }
}