using System;

public interface ISkinService : IService
{
    event Action<int> SkinChanged;
    int GetCurrent();
    bool IsUnlocked(int id);
    void Unlock(int id);
    void SetCurrent(int id);
};
