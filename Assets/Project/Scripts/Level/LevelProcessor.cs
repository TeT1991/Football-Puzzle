using System;
using System.Collections;
using UnityEngine;

public class LevelProcessor : MonoBehaviour
{
    private LevelState _levelState;

    private EntityRouteRegistry _entityRouteRegistry;
    private EntetiesMovementProcessor _entetiesMovementProcessor;
    private EntityView _characterView;
    private Vector2Int _goalCoordinates;

    private CellSelector _cellSelector;

    private CellView[,] _cells;

    private void OnDestroy()
    {
        _cellSelector.CellSelected -= TryMoveCharacter;
        _entetiesMovementProcessor.EntitiesMovementStopped -= OnEntitiesMovementStopped;
    }

    public void Init(EntityView character, CellSelector cellSelector, EntityRouteRegistry entityRouteRegistry, EntetiesMovementProcessor entetiesMovementProcessor, Vector2Int goalCoordinates)
    {
        _cellSelector = cellSelector;
        _characterView = character;
        _entityRouteRegistry = entityRouteRegistry;
        _entetiesMovementProcessor = entetiesMovementProcessor;
        _goalCoordinates = goalCoordinates;

        _cellSelector.CellSelected += TryMoveCharacter;
        _entetiesMovementProcessor.EntitiesMovementStopped += OnEntitiesMovementStopped;
    }

    public void StartLevel() //Дает возможность играть когда все инициализировалось. Нужо ли?
    {
        SetLevelState(LevelState.EntitiesMoving);
        ApplyStateActions();
    }

    private void ApplyStateActions()
    {
        switch (_levelState)
        {
            case LevelState.EntitiesMoving:
                ApplyPlayerTurnState();
                break;

            case LevelState.EndLevelCheck:
                TryEndLevel();
                break;

            case LevelState.Win:
                ApplyWinState();
                break;

            case LevelState.Lose:
                ApplyLoseState();
                break;
        }

        Debug.Log(_levelState);
    }

    private void ApplyWinState()
    {
        Debug.Log("Win!!!");
    }
    private void ApplyLoseState()
    {
        Debug.Log("Lose");
    }

    private void ApplyPlayerTurnState()
    {
        _cellSelector.StartSelecting();
    }

    private void TryEndLevel()
    {

        if (_characterView.CurrentCoordinates == _goalCoordinates)
        {
            SetLevelState(LevelState.Win);
            return;
        }

        if (_entetiesMovementProcessor.IsEnemyOnCoordinates(_characterView.CurrentCoordinates))
        {
            SetLevelState(LevelState.Lose);
            return;
        }

        SetLevelState(LevelState.EntitiesMoving);
    }

    private void TryMoveCharacter(CellView cellView)
    {
        if (CanMove(cellView))
        {
            _cellSelector.StopSelecting();
            _entetiesMovementProcessor.StartEntetiesMovement(cellView.Coordinates);

        }
    }

    private bool CanMove(CellView cellView)
    {
        if (cellView == null)
        {
            return false;
        }

        if (_entityRouteRegistry.IsChainedCoordinates(_characterView, cellView.Coordinates))
        {
            return true;
        }

        Debug.Log("Not chaind cell");
        return false;
    }

    private void SetLevelState(LevelState state)
    {
        _levelState = state;
        ApplyStateActions();
    }

    private void OnEntitiesMovementStopped()
    {
        _cellSelector.ClearCurrentCell();
        SetLevelState(LevelState.EndLevelCheck);
    }
}

public enum LevelState
{
    Initialization,
    EntitiesMoving,
    EndLevelCheck,
    Win,
    Lose
}

public interface ICoroutineRunner
{
    Coroutine Run(IEnumerator routine);
    void Stop(Coroutine coroutine);
}
