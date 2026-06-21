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
        _entetiesMovementProcessor.EntitiesMovementStopped -= OnEntitiesMovementStopped;
    }

    public void Init(EntityView character, CellSelector cellSelector, EntityRouteRegistry entityRouteRegistry, EntetiesMovementProcessor entetiesMovementProcessor)
    {
        _cellSelector = cellSelector;
        _characterView = character;
        _entityRouteRegistry = entityRouteRegistry;
        _entetiesMovementProcessor = entetiesMovementProcessor;

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
        }

        Debug.Log(_levelState);
    }

    private void ApplyPlayerTurnState()
    {
        _cellSelector.StartSelecting();
    }

    private void TryEndLevel()
    {
        Debug.Log("Try End level");

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

public class EntetiesMovementProcessor : IDisposable
{
    private readonly EntityView _character;
    private readonly Dictionary<EntityView, Route> _entities;

    private readonly int _width;
    private readonly int _height;

    private int _entitiesStopMovementCount;

    public event Action EntitiesMovementStopped;

    public EntetiesMovementProcessor(EntityView character, int width, int height)
    {
        _entitiesStopMovementCount = 0;
        _character = character;

        _width = width;
        _height = height;

        _entities = new();
    }

    public void AddEntityRoutes(EntityView entityView, Route route)
    {
        _entities.Add(entityView, route);

        entityView.TargetPositionReached += OnEntityMovementStopped;
    }

    public void StartEntetiesMovement(Vector2Int characterTargetPosition)
    {   
        foreach (KeyValuePair<EntityView, Route> pair in _entities)
        {
            EntityView entity = pair.Key;

            Vector2Int nextCoordinates = entity == _character? characterTargetPosition : GetNextCoordinates(entity);
            entity.SetCurrentCoordinates(nextCoordinates);

            Vector2 nextPosition = GameUtility.ConvertCoordinatesToPosition(nextCoordinates, _width, _height);

            entity.StartMove(nextPosition);
        }
    }

    private Vector2Int GetNextCoordinates(EntityView entityView)
    {
        Vector2Int currentCoordinates = entityView.CurrentCoordinates;
        Route route = _entities[entityView];
        IReadOnlyList<RouteNode> nodes = route.RouteNodes;

        for (int i = 0; i < nodes.Count; i++)
        {
            if(nodes[i].CurrentCoordinates != currentCoordinates)
            {
                continue;
            }

            int nextIndex = (i + 1) % nodes.Count;
            return nodes[nextIndex].CurrentCoordinates;
        }

        throw new InvalidOperationException($"Enemies coordinates {entityView.CurrentCoordinates} not exist in route.");
    }

    private void OnEntityMovementStopped(EntityView view)
    {
        _entitiesStopMovementCount++;

        if(_entitiesStopMovementCount == _entities.Count)
        {
            _entitiesStopMovementCount = 0;
            EntitiesMovementStopped?.Invoke();
        }
    }

    public void Dispose()
    {
        _character.TargetPositionReached -= OnEntityMovementStopped;
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
