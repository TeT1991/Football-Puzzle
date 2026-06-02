using System;

public class SkinService : ISkinService
{
    private readonly ISaveService _saveService;

    private SkinsCatalog _catalog;
    private int _curentSkinId;

    public event Action<int> SkinChanged;

    public SkinService(SkinsCatalog catalog)
    {
        _saveService = ServiceLocator.Get<ISaveService>();
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
    }

    public void Unlock(int id)
    {
        _saveService.SaveUnlockedSkin(id);
    }
}
