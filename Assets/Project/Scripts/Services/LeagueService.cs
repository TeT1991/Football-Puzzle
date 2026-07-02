using System.Collections.Generic;
using UnityEngine;

public class LeagueService : ILeagueService
{
    private readonly ISaveService _saveService;
    private readonly LeaguesCatalog _catalog;

    public LeagueService(ISaveService saveService, LeaguesCatalog catalog)
    {
        _saveService = saveService;
        _catalog = catalog;
    }

    public IReadOnlyList<LeagueMenuItemData> GetDatas()
    {
        List<LeagueMenuItemData> datas = new();
        LeagueMenuItemData data;

        foreach (LeagueDefinition leagueDefinition in _catalog.Catalog)
        {
            int id = leagueDefinition.ID;
            bool isUnlocked = leagueDefinition.IsUnlocked;

            data = new(id, isUnlocked);
            datas.Add(data);
        }

        return datas;
    }

    public bool IsUnlocked(int id)
    {
        throw new System.NotImplementedException();
    }

    public void Unlock(int id)
    {
        throw new System.NotImplementedException();
    }
}
