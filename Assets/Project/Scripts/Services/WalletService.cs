using System;

public class WalletService : IWalletService
{
    private readonly ISaveService _saveService;

    private int _softCurrency;
    private int _hardCurrency;

    public event Action<CurrencyTypes, int> CurrencyCountChanged;

    public WalletService(ISaveService saveService)
    {
        _saveService = saveService;
        _softCurrency = saveService.GetCurrencyCount(CurrencyTypes.Soft);
        _hardCurrency = saveService.GetCurrencyCount(CurrencyTypes.Hard);
    }

    public void DecreaseCurrency(CurrencyTypes currencyType, int value)
    {
        if (value < 0)
        {
            throw new System.Exception("Valu cant be < 0");
        }

        if (currencyType == CurrencyTypes.Soft)
        {
            _softCurrency -= value;
            CurrencyCountChanged?.Invoke(CurrencyTypes.Soft, _softCurrency);
        }
        else
        {
            _hardCurrency -= value;
            CurrencyCountChanged?.Invoke(CurrencyTypes.Hard, _hardCurrency);
        }
    }

    public void IncreaseCurrency(CurrencyTypes currencyType, int value)
    {
        if (value < 0)
        {
            throw new System.Exception("Valu cant be < 0");
        }

        if (currencyType == CurrencyTypes.Soft)
        {
            _softCurrency += value;
            CurrencyCountChanged?.Invoke(CurrencyTypes.Soft, _softCurrency);
        }
        else
        {
            _hardCurrency += value;
        }
    }
    public int GetCount(CurrencyTypes currencyType)
    {
        if (currencyType == CurrencyTypes.Soft)
        {
            return _softCurrency;
        }
        else
        {
            return _hardCurrency;
        }
    }

    public bool IsEnough(CurrencyTypes currencyType, int value)
    {
        if (currencyType == CurrencyTypes.Soft)
        {
            return _softCurrency - value > 0;
        }
        else
        {
            return _hardCurrency - value > 0;
        }
    }
}
