using UnityEngine;
using YG;

public class SavesResete : MonoBehaviour
{
    private ISaveService _saveService;

    public void Init(ISaveService saveService)
    {
        _saveService = saveService;
    }

    public void ResetSaves()
    {

            YG2.SetDefaultSaves();
            YG2.saves.Data = SaveData.CreateNew();
            YG2.SaveProgress();
            Debug.Log("Сохранения сброшены");
        
    }
}
