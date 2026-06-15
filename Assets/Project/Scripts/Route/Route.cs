using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Route
{
    [SerializeField] private List<RouteNode> _routeNodes = new();

    public Route()
    {
        _routeNodes ??= new();
    }

    public IReadOnlyList<RouteNode> RouteNodes => _routeNodes;

    public void AddRouteNode(RouteNode node)
    {
        _routeNodes ??= new();
        _routeNodes.Add(node);
    }

    public bool TryGetRouteNodeByCoordintates(Vector2Int coordinates, out RouteNode routeNode)
    {
        routeNode = null;

        if (_routeNodes == null)
        {
            return false;
        }

        foreach(RouteNode node in _routeNodes)
        {
            if(node.CurrentCoordinates == coordinates)
            {
                routeNode = node;
                return true;
            }
        }

        return false;
    }
}
