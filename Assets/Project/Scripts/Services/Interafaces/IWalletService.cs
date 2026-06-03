using System;

public interface IWalletService : IService
{
    event Action<CurrencyTypes, int> CurrencyCountChanged;
    int GetCount(CurrencyTypes currencyType);

    bool IsEnough(CurrencyTypes currencyType, int value);

    void IncreaseCurrency(CurrencyTypes currencyType, int value);

    void DecreaseCurrency(CurrencyTypes currencyType, int value);
}
