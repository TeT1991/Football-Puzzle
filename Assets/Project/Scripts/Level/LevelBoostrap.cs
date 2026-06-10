using UnityEngine;

public class LevelBoostrap : MonoBehaviour
{
    [SerializeField] private CellView _cellVIewPrefab;
    [SerializeField] private Transform _cellsParent;
    [SerializeField] private LevelDefenition _levelDefenition;
    [SerializeField] private CellSelector _cellSelector;
    [SerializeField] private LevelProcessor _levelProcessor;

    private LevelBuilder _levelBuilder;
    private SkinLoader _skinLoader;

    private void Awake()
    {
        LevelData levelData = GenerateLevelData();
        Vector2Int levelSize = new(levelData.Width, levelData.Height);

        _levelBuilder = new(levelSize,_cellVIewPrefab, _cellsParent);

        _skinLoader = new();
        _skinLoader.Load();

        _levelProcessor.Init(levelData, _levelBuilder, _cellSelector);
    }

    private LevelData GenerateLevelData()
    {
        int width = _levelDefenition.Width;
        int height = _levelDefenition.Height;

        return new(width, height);
    }
}

public class LevelData
{
    private readonly int _width;
    private readonly int _height;

    public LevelData(int width, int height)
    {
        _width = width;
        _height = height;
    }

    public int Width => _width;
    public int Height => _height;
}
