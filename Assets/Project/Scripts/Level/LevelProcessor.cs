using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelProcessor : MonoBehaviour
{
    private LevelState _levelState;

    private EntityRouteRegistry _entityRouteRegistry;
    private EntetiesMovementProcessor _entetiesMovementProcessor;
    private EntityView _characterView;

    private CellSelector _cellSelector;

    private CellView[,] _cells;

    private void OnDestroy()
    {
        _cellSelector.CellSelected -= TryMoveCharacter;
        _entetiesMovementProcessor.CharacterMovementStopped -= OnCharacterMovementStopped;
    }

    public void Init(EntityView character, CellSelector cellSelector, EntityRouteRegistry entityRouteRegistry, EntetiesMovementProcessor entetiesMovementProcessor)
    {
        _cellSelector = cellSelector;
        _characterView = character;
        _entityRouteRegistry = entityRouteRegistry;
        _entetiesMovementProcessor = entetiesMovementProcessor;

        _cellSelector.CellSelected += TryMoveCharacter;
        _entetiesMovementProcessor.CharacterMovementStopped += OnCharacterMovementStopped;
    }

    public void StartLevel() //Дает возможность играть когда все инициализировалось. Нужо ли?
    {
        SetLevelState(LevelState.PlayerTurn);
        ApplyStateActions();
    }

    private void ApplyStateActions()
    {
        switch (_levelState)
        {
            case LevelState.PlayerTurn:
                ApplyPlayerTurnState();
                break;

            case LevelState.EnemyTurn:
                ApplyEnemyTurnState();
                break;
        }

        Debug.Log(_levelState);
    }

    private void ApplyPlayerTurnState()
    {
        _cellSelector.StartSelecting();
    }

    private void ApplyEnemyTurnState()
    {
        Debug.Log("ENemy move");
    }

    private void TryMoveCharacter(CellView cellView)
    {
        if (CanMove(cellView))
        {
            _cellSelector.StopSelecting();
            _entetiesMovementProcessor.StartCharacterMovement(cellView.transform.position);
           

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

    private void OnCharacterMovementStopped()
    {
        Debug.Log(_cellSelector.CurrentCell);
        _characterView.SetCurrentCoordinates(_cellSelector.CurrentCell.Coordinates);
        _cellSelector.ClearCurrentCell();
        SetLevelState(LevelState.EnemyTurn);
        Debug.Log(_cellSelector);
    }

    private void OnEnemiesMovementStopped()
    {

    }
}

public class EntetiesMovementProcessor : IDisposable
{
    private readonly EntityView _character;

    public event Action CharacterMovementStopped;

    public EntetiesMovementProcessor(EntityView character)
    {
        _character = character;
        _character.TargetPositionReached += OnCharacterMovementStopped;
    }

    public void StartCharacterMovement(Vector3 targetPosition)
    {
        _character.StartMove(targetPosition);
    }

    private void OnCharacterMovementStopped()
    {
        CharacterMovementStopped?.Invoke();
    }

    public void Dispose()
    {
        _character.TargetPositionReached -= OnCharacterMovementStopped;
    }
}

public enum LevelState
{
    Initialization,
    PlayerTurn,
    EnemyTurn,
    EndLevelCheck,
    Win,
    Lose
}

public interface ICoroutineRunner
{
    Coroutine Run(IEnumerator routine);
    void Stop(Coroutine coroutine);
}
