using UnityEngine;

public class LevelProcessor : MonoBehaviour
{
    private EntityView _characterView;

    private CellSelector _cellSelector;

    private CellView[,] _cells;

    public void Init(EntityView character,CellSelector cellSelector)
    {

        _cellSelector = cellSelector;
        //_cellSelector.Init(_cells, _levelBuilder.GridPositionOffsetX, _levelBuilder.GridPositionOffsetY);

    }
}

public enum LevelState
{
    Initialization,
    PlayerTurn,
    EnemyTurn,
    Win,
    Lose
}
