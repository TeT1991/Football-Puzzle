using UnityEngine;

public class MetamapLevelMarker : MonoBehaviour
{
    [SerializeField] MetamapLevelData _levelData;

    public MetamapLevelData LevelData => _levelData;

    public void Init(MetamapLevelData metamapLevel)
    {
        _levelData = metamapLevel;
    }
}
