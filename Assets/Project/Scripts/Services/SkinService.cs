using System;
using UnityEngine;

public class SkinService : ISkinService
{
    private SkinsCatalog _catalog;
    private int _curentSkinId;

    public event Action<int> SkinChanged;

    public SkinService (SkinsCatalog catalog)
    {
        _catalog = catalog;
    }
    public string GetCurrent()
    {
        return _catalog.GetById(_curentSkinId).Name;
    }

    public bool IsUnlocked(int id)
    {
        return _catalog.GetById(_curentSkinId).IsLocked;
    }

    public void SetCurrent(int id)
    {
        _curentSkinId = id;
        string skinIdname = "SkinId";
        PlayerPrefs.SetInt(skinIdname, _curentSkinId);
    }

    public void Unlock(int id)
    {
        _catalog.GetById(_curentSkinId).Unlock();
    }
}
