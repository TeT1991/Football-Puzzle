using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemyRoute
{
    [SerializeField] private List<Vector2Int> _coordinates = new();

    public EnemyRoute(IEnumerable<Vector2Int> coordinates)
    {
        _coordinates = coordinates != null
            ? new List<Vector2Int>(coordinates)
            : new List<Vector2Int>();
    }

    public bool HasCoordinates => _coordinates != null && _coordinates.Count > 0;
    public Vector2Int EnemyStartCoordinates => HasCoordinates
        ? _coordinates[0]
        : default;
    public IReadOnlyList<Vector2Int> Coordinates => _coordinates;
}
