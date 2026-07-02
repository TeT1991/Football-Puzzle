public class LeagueMenuItemData 
{
    private readonly int _id;
    private readonly bool _isUnlocked;

    public LeagueMenuItemData(int id, bool isUnlocked)
    {
        _id = id;
        _isUnlocked = isUnlocked;
    }

    public int ID => _id;
    public bool IsUnlocked => _isUnlocked;
}
