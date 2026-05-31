using UnityEngine;

public class SaveService : ISaveService
{
    private readonly string _skinIdName = "SkinID";

    public void SaveSkinId (int value)
    {
        PlayerPrefs.SetInt(_skinIdName, value);
    }

    public int LoadSkinId()
    {
        return PlayerPrefs.GetInt(_skinIdName);
    }
}
