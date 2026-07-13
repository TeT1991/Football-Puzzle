public interface ISaveService : IService 
{
    int CurrentSkinId { get; }
    bool IsSkinUnlocked(int id);
    void SaveCurrentSkin(int id);
    void SaveUnlockedSkin(int id);

    void SaveCurrencyCount(CurrencyTypes type, int count);
    int GetCurrencyCount(CurrencyTypes type);

    int GetUnlockedLevelCount(int leagueId);
    bool IsLevelUnlocked(int leagueId, int levelIndex);
    void UnlockLevel(int leagueId, int levelIndex);
};
