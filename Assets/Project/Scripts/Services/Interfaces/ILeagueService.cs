using System;
using System.Collections.Generic;
public interface ILeagueService : IService
{
    IReadOnlyList<LeagueMenuItemData> GetDatas();
    bool IsUnlocked(int id);
    void Unlock(int id);
}
