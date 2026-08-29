public interface ILevelProgressionService : IService
{
    void SaveProgress();
    bool TrySelectNextLevel();
}