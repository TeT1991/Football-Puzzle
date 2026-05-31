using System;

public class SkinService : ISkinService
{
    private int _skinId;

    public event Action SkinChanged;

    public void ChangeSkin(int id)
    {
        _skinId = id;
        SkinChanged?.Invoke();
    }
}
