using System;

public class MoneyPanelPresenter : IDisposable
{
    private readonly IWalletService _walletService;
    private readonly MoneyPanelView _softCurrencyView;
    private readonly MoneyPanelView _hardCurrencyView;

    public MoneyPanelPresenter(IWalletService walletService,MoneyPanelView softCurrencyView, MoneyPanelView hardCurrencyView)
    {
        _walletService = walletService;
        _softCurrencyView = softCurrencyView;
        _hardCurrencyView = hardCurrencyView;

        _walletService.CurrencyCountChanged += SetCurrencyText;

        SetCurrencyText(CurrencyTypes.Soft, walletService.GetCount(CurrencyTypes.Soft));
        SetCurrencyText(CurrencyTypes.Hard, walletService.GetCount(CurrencyTypes.Hard));
    }

    public void Dispose()
    {
        _walletService.CurrencyCountChanged -= SetCurrencyText;
    }

    private void SetCurrencyText(CurrencyTypes currencyTypes, int value)
    {
        if(currencyTypes == CurrencyTypes.Soft)
        {
            _softCurrencyView.SetText(value);
        }
        else
        {
            _hardCurrencyView.SetText(value);
        }
    }
}
