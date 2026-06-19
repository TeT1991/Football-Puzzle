using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemyRoute
{
    [SerializeField] private Vector2Int _enemyStartCoordinates;
    [SerializeField] private List<Vector2Int> _coordinates = new();

    public EnemyRoute(Vector2Int enemyStartCoordinates, IEnumerable<Vector2Int> coordinates)
    {
        _enemyStartCoordinates = enemyStartCoordinates;
        _coordinates = coordinates != null
            ? new List<Vector2Int>(coordinates)
            : new List<Vector2Int>();
    }

    public Vector2Int EnemyStartCoordinates => _enemyStartCoordinates;
    public IReadOnlyList<Vector2Int> Coordinates => _coordinates;
}
