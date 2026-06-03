public interface ISaveService : IService 
{
    int CurrentSkinId { get; }
    bool IsSkinUnlocked(int id);
    void SaveCurrentSkin(int id);
    void SaveUnlockedSkin(int id);

    void SaveCurrencyCount(CurrencyTypes type, int count);
    int GetCurrencyCount(CurrencyTypes type);
};