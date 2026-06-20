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

    public Route(IEnumerable<RouteNode> routeNodes)
    {
        _routeNodes = new List<RouteNode>();

        if (routeNodes == null)
        {
            return;
        }

        foreach (RouteNode node in routeNodes)
        {
            if (node == null)
            {
                continue;
            }

            Vector2Int[] connections = new Vector2Int[node.ChainedCoordinates.Count];

            for (int i = 0; i < connections.Length; i++)
            {
                connections[i] = node.ChainedCoordinates[i];
            }

            _routeNodes.Add(new RouteNode(node.CurrentCoordinates, connections));
        }
    }

    public IReadOnlyList<RouteNode> RouteNodes => _routeNodes ??= new List<RouteNode>();
    public bool HasNodes => _routeNodes != null && _routeNodes.Count > 0;
    public Vector2Int StartCoordinates => HasNodes
        ? _routeNodes[0].CurrentCoordinates
        : default;

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
