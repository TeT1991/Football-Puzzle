using System;
using System.Collections.Generic;

public interface ISkinService : IService
{
    event Action<int> SkinChanged;
    event Action<int> SkinUlocked;
    int GetCurrent();
    SkinDefinition GetSkin(int id);
    IReadOnlyList<SkinShopItemData> GetDatas();
    bool IsUnlocked(int id);
    void Unlock(int id);
    void TrySetCurrent(int id);
};
