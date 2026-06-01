using UnityEngine;

public class SkinLoader 
{
    public void Load()
    {
        Debug.Log("Загружен скин - " + ServiceLocator.Get<ISkinService>().GetCurrent());
    }
}
