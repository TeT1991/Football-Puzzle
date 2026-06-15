using UnityEngine;

public class LevelProcessor : MonoBehaviour
{
    private LevelState _levelState;

    private EntityRouteRegistry _entityRouteRegistry;
    private EntityView _characterView;

    private CellSelector _cellSelector;

    private CellView[,] _cells;

    private void OnDestroy()
    {
        _cellSelector.CellSelected -= TryMoveCharacter;
    }

    public void Init(EntityView character, CellSelector cellSelector, EntityRouteRegistry entityRouteRegistry)
    {
        _cellSelector = cellSelector;
        _characterView = character;
        _entityRouteRegistry = entityRouteRegistry;

        _cellSelector.CellSelected += TryMoveCharacter;
    }

    public void StartLevel() //Дает возможность играть когда все инициализировалось. Нужо ли?
    {
        _levelState = LevelState.PlayerTurn;
        Debug.Log("Player turn");

        ApplyStateActions();
    }

    private void ApplyStateActions()
    {
        switch (_levelState)
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

    private void TryMoveCharacter(CellView cellView)
    {
        if (CanMove(cellView))
        {
            _cellSelector.StopSelecting();
            _cellSelector.ClearCurrentCell();
            _characterView.StartMove(cellView.transform.position);
            _characterView.SetCurrentCoordinates(cellView.Coordinates);
        }
    }

    private bool CanMove(CellView cellView)
    {
        if(cellView == null)
        {
            return false;
        }

        if( _entityRouteRegistry.IsChainedCoordinates(_characterView, cellView.Coordinates))
        {
            return true;
        }

        Debug.Log("Not chaind cell");
        return false;
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
