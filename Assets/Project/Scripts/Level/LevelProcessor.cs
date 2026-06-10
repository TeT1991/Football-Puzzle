using UnityEngine;

public class LevelProcessor : MonoBehaviour
{
    private LevelBuilder _levelBuilder;
    private CellSelector _cellSelector;

    private CellView[,] _cells;

    public void Init(LevelBuilder levelBuilder, CellSelector cellSelector)
    {
        _levelBuilder = levelBuilder;
        _levelBuilder.BuildLevel();
        _cells = _levelBuilder.GetCells();

        _cellSelector = cellSelector;
        _cellSelector.Init(_cells, _levelBuilder.GridPositionOffsetX, _levelBuilder.GridPositionOffsetY);

    }
}
