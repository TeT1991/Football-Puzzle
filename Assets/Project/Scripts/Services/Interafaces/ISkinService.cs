using System;
using System.Collections.Generic;

public interface ISkinService : IService
{
    event Action<int> SkinChanged;
    int GetCurrent();
    IReadOnlyList<ShopItemData> GetDatas();
    bool IsUnlocked(int id);
    void Unlock(int id);
    void TrySetCurrent(int id);
};
