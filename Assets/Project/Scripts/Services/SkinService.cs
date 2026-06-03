using System;
using System.Collections.Generic;
using System.Linq;

public class SkinService : ISkinService
{
    private readonly ISaveService _saveService;
    private readonly SkinsCatalog _catalog;

    private int _curentSkinId;

    public event Action<int> SkinChanged;
    public event Action<int> SkinUlocked;

    public SkinService(ISaveService saveService,SkinsCatalog catalog)
    {
        _saveService = saveService;
        _curentSkinId = _saveService.CurrentSkinId;
        _catalog = catalog;
    }
    public int GetCurrent()
    {
        return _saveService.CurrentSkinId;
    }

    public bool IsUnlocked(int id)
    {
        return _saveService.IsSkinUnlocked(id);
    }

    public void TrySetCurrent(int id)
    {
        if (id == _curentSkinId)
        {
            return;
        }
        _curentSkinId = id;
        _saveService.SaveCurrentSkin(_curentSkinId);
        SkinChanged?.Invoke(_curentSkinId);
    }

    public void Unlock(int id)
    {
        _saveService.SaveUnlockedSkin(id);
        SkinUlocked?.Invoke(id);
    }

    public SkinDefinition GetSkin(int id)
    {
        SkinDefinition skin = _catalog.GetById(id);

        if (skin == null)
        {
            throw new Exception($"Skin with id - {id} not found");
        }

        return skin;
    }

    public IReadOnlyList<SkinShopItemData> GetDatas()
    {
        List<SkinShopItemData> datas = new();
        SkinShopItemData data;

        foreach (SkinDefinition skin in _catalog.Catalog)
        {
            int id = skin.ID;
            int price = skin.Price;
            bool isUnlocked = _saveService.IsSkinUnlocked(id);
            data = new(id, price, isUnlocked);
            datas.Add(data);
        }

        return datas;

    }
}
