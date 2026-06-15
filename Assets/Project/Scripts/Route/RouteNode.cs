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

    public bool HasConnectionTo(Vector2Int target)
    {
        foreach (Vector2Int coordinates in _chainedCoordinates)
        {
            if (target == coordinates)
            {
                return true;
            }
        }

        return false;
    }
}
