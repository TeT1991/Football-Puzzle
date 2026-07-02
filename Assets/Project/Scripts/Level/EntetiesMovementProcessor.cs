using System;
using System.Collections.Generic;
using UnityEngine;

public class EntetiesMovementProcessor : IDisposable
{
    private readonly EntityView _character;
    private readonly Dictionary<EntityView, Route> _entities;
    private readonly Dictionary<EntityView, int> _enemyDirections = new();

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

        if (entityView != _character)
        {
            _enemyDirections[entityView] = 1;
        }

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

    public bool IsEnemyOnCoordinates(Vector2Int coordinates)
    {
        foreach (KeyValuePair<EntityView, Route> pair in _entities)
        {
            EntityView entity = pair.Key;

            if (entity == _character)
            {
                continue;
            }

            if (entity.CurrentCoordinates == coordinates)
            {
                return true;
            }
        }

        return false;
    }

    private Vector2Int GetNextCoordinates(EntityView entityView) //Слишком большой метод. Возможно надо раделить
    {
        Vector2Int currentCoordinates = entityView.CurrentCoordinates;
        Route route = _entities[entityView];
        IReadOnlyList<RouteNode> nodes = route.RouteNodes;

        if (nodes.Count == 0)
        {
            throw new InvalidOperationException("Enemy route is empty.");
        }

        if (nodes.Count == 1)
        {
            return currentCoordinates;
        }

        int currentIndex = -1;

        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].CurrentCoordinates == currentCoordinates)
            {
                currentIndex = i;
                break;
            }
        }

        if (currentIndex < 0)
        {
            throw new InvalidOperationException(
                $"Enemy coordinates {currentCoordinates} not exist in route.");
        }

        bool isCycled = nodes[0].CurrentCoordinates ==
                        nodes[nodes.Count - 1].CurrentCoordinates;

        if (isCycled)
        {
            int nextIndex = currentIndex + 1;

            if (nextIndex >= nodes.Count)
            {
                nextIndex = 1;
            }

            return nodes[nextIndex].CurrentCoordinates;
        }

        int direction = _enemyDirections[entityView];
        int nextPingPongIndex = currentIndex + direction;

        if (nextPingPongIndex >= nodes.Count)
        {
            direction = -1;
            nextPingPongIndex = currentIndex + direction;
        }
        else if (nextPingPongIndex < 0)
        {
            direction = 1;
            nextPingPongIndex = currentIndex + direction;
        }

        _enemyDirections[entityView] = direction;

        return nodes[nextPingPongIndex].CurrentCoordinates;
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
