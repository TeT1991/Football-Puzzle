using YG;

public class SaveService : ISaveService
{
    private SaveData _saveData;

    public SaveService(int initialSkinId)
    {
        if(YG2.saves.Data == null)
        {
            _saveData = SaveData.CreateNew();
            Save();
            SaveCurrentSkin(initialSkinId);
            SaveUnlockedSkin(initialSkinId);
        }
        else
        {
            Load();
        }
    }

    public int CurrentSkinId => _saveData.CurrentSkinId;

    public int GetCurrencyCount(CurrencyTypes type)
    {
        if (type == CurrencyTypes.Soft)
        {
            return _saveData.SofCurrency;
        }

        else
        {
            return _saveData.HardCurrency;
        }
    }

    public bool IsSkinUnlocked(int id)
    {
        if (_saveData.UnlockedSkins.Contains(id))
        {
            return true;
        }
        
        return false;
    }

    public void Load()
    {
        _saveData = YG2.saves.Data;
    }

    public void Save()
    {
        YG2.saves.Data = _saveData;
        YG2.SaveProgress();
    }

    public void SaveCurrentSkin(int id)
    {
        _saveData.CurrentSkinId = id;
        Save();
    }

    public void SaveUnlockedSkin(int id)
    {
        if (_saveData.UnlockedSkins.Contains(id) == false)
        {
            _saveData.UnlockedSkins.Add(id);
            Save();
        }
        else
        {
           UnityEngine.Debug.Log($"skin {id} akready Exist");
        }
    }
}
