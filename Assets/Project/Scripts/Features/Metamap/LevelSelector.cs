using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelector : MonoBehaviour
{
    [SerializeField] private MetamapLevelMarker _testMarker;

    private Dictionary<int, MetamapLevelMarker> _markers;

    public void Init()
    {
        
    }
}

[Serializable]
public readonly struct MetamapLevelData
{
    public readonly int _leagueIndex;
    public readonly int _levelIndex;

    public MetamapLevelData(int leagueIndex, int levelIndex)
    {
        _leagueIndex = leagueIndex;
        _levelIndex = levelIndex;
    }
}
