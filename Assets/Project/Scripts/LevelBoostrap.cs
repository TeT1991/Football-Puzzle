using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelBoostrap : MonoBehaviour
{
    private SkinLoader _skinLoader;

    private void Awake()
    {
        _skinLoader = new();
        _skinLoader.Load();
    }
}
