using YG;
using System.Collections.Generic;
using UnityEngine;

public class SaveService : ISaveService
{
    private SaveData _saveData;

    public SaveService(int initialSkinId)
    {
        if (YG2.saves.Data == null)
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
            return _saveData.SoftCurrency;
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

    public void SaveCurrencyCount(CurrencyTypes type, int count)
    {
        if (type == CurrencyTypes.Soft)
        {
            _saveData.SoftCurrency = count;
        }
        else
        {
            _saveData.HardCurrency = count;
        }

        Save();
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

    public int GetUnlockedLevelCount(int leagueId)
    {
        LeagueProgressData progress = GetLeagueProgress(leagueId, false);
        return progress == null ? 1 : Mathf.Max(1, progress.UnlockedLevelCount);
    }

    public bool IsLevelUnlocked(int leagueId, int levelIndex)
    {
        return levelIndex >= 0 && levelIndex < GetUnlockedLevelCount(leagueId);
    }

    public void UnlockLevel(int leagueId, int levelIndex)
    {
        if (levelIndex < 0)
        {
            return;
        }

        LeagueProgressData progress = GetLeagueProgress(leagueId, true);
        int requiredUnlockedCount = levelIndex + 1;

        if (requiredUnlockedCount <= progress.UnlockedLevelCount)
        {
            return;
        }

        progress.UnlockedLevelCount = requiredUnlockedCount;
        Save();
    }

    private LeagueProgressData GetLeagueProgress(int leagueId, bool createIfMissing)
    {
        _saveData.LeagueProgress ??= new List<LeagueProgressData>();

        foreach (LeagueProgressData progress in _saveData.LeagueProgress)
        {
            if (progress != null && progress.LeagueId == leagueId)
            {
                return progress;
            }
        }

        if (createIfMissing == false)
        {
            return null;
        }

        LeagueProgressData newProgress = new()
        {
            LeagueId = leagueId,
            UnlockedLevelCount = 1
        };

        _saveData.LeagueProgress.Add(newProgress);
        return newProgress;
    }
}
