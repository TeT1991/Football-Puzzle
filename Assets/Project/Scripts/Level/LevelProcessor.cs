using UnityEngine;

public class LevelProcessor : MonoBehaviour
{
    private LevelBuilder _levelBuilder;
    private CellSelector _cellSelector;

    private CellView[,] _cells;

    public void Init(LevelData levelData, LevelBuilder levelBuilder, CellSelector cellSelector)
    {
        int levelWidth = levelData.Width;
        int levelHeight = levelData.Height;
        _levelBuilder = levelBuilder;
        _levelBuilder.BuildLevel();
        _cells = _levelBuilder.GetCells();

        _cellSelector = cellSelector;
        _cellSelector.Init(_levelBuilder.GetCells(), _levelBuilder.GridPositionOffseX, _levelBuilder.GridPositionOffseY);

    }
}
