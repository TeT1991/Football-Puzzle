using UnityEngine;

public class LevelProcessor : MonoBehaviour
{
    private LevelState _levelState;

    private EntityView _characterView;

    private CellSelector _cellSelector;

    private CellView[,] _cells;

    public void Init(EntityView character,CellSelector cellSelector)
    {
        _cellSelector = cellSelector;
    }

    public void StartLevel() //Дает возможность играть когда все инициализировалось. Нужо ли?
    {
        _levelState = LevelState.PlayerTurn;
        Debug.Log("Player turn");

        ApplyStateActions();
    }

    private void ApplyStateActions()
    {
        switch(_levelState)
        {
            case LevelState.PlayerTurn:
                ApplyPlayerTurnState();
                break;
        }
    }

    private void ApplyPlayerTurnState()
    {
        _cellSelector.StartSelecting();
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
