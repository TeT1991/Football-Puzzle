
using System.Collections.Generic;

[System.Serializable]
public class SaveData
{
    public int CurrentSkinId;
    public List<int> UnlockedSkins;

    public static SaveData CreateNew()
    {
        return new SaveData
        {
            CurrentSkinId = 0,
            UnlockedSkins = new() { 0 }
        };
    }
}
