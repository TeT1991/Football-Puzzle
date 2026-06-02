using System;

public class SkinService : ISkinService
{
    private SkinsCatalog _catalog;
    private int _curentSkinId;

    private ISaveService _saveService;

    public event Action<int> SkinChanged;

    public SkinService (SkinsCatalog catalog)
    {
        _saveService = ServiceLocator.Get<ISaveService> ();
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

    public void SetCurrent(int id)
    {
        _saveService.SaveCurrentSkin(id);
    }

    public void Unlock(int id)
    {
       _saveService.SaveUnlockedSkin(id);
    }
}
