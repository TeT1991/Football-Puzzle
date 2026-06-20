using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RouteNode 
{
    [SerializeField] private Vector2Int _currentCoordinates;
    [SerializeField] private Vector2Int[] _chainedCoordinates;

    public RouteNode(Vector2Int currentCoordinates, Vector2Int[] chainedCoordinates)
    {
        _currentCoordinates = currentCoordinates;
        _chainedCoordinates = chainedCoordinates;
    }

    public Vector2Int CurrentCoordinates => _currentCoordinates;
    public IReadOnlyList<Vector2Int> ChainedCoordinates =>
        _chainedCoordinates ?? System.Array.Empty<Vector2Int>();

    public bool HasConnectionTo(Vector2Int target)
    {
        foreach (Vector2Int coordinates in ChainedCoordinates)
        {
            if (target == coordinates)
            {
                return true;
            }
        }

        return false;
    }
}
