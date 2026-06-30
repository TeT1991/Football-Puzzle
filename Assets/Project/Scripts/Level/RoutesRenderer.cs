using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class RoutesRenderer
{
    private RouteNodeView _routeNodeView;
    private Transform _routeNodeViewsParent;
    private CellView[,] _cells;

    public RoutesRenderer(RouteNodeView routeNodeView, CellView[,] cells)
    {
        _routeNodeView = routeNodeView;
        _cells = cells;

    }

    public void CreateRoutes(IReadOnlyList<RouteNode> RouteNodes, Color color)
    {
        foreach (RouteNode node in RouteNodes)
        {
            if (TryGetCell(node.CurrentCoordinates, out CellView cell) == false)
            {
                continue;
            }

            RouteNodeView routeNodeView = MonoBehaviour.Instantiate(_routeNodeView);
            routeNodeView.transform.parent = _routeNodeViewsParent;
            routeNodeView.transform.position = cell.transform.position;
            routeNodeView.SetGlowColor(color);
            routeNodeView.ShowRoutes(GetConnections(node));
        }
    }

    private bool TryGetCell(Vector2Int coordinates, out CellView cellView)
    {
        foreach (CellView cell in _cells)
        {
            if (cell.Coordinates == coordinates)
            {
                cellView = cell;
                return true;
            }
        }

        cellView = null;
        return false;
    }


    private RouteNodeConnections GetConnections(RouteNode routeNode)
    {
        RouteNodeConnections connections = 0;
        Vector2Int currentCoordinates = routeNode.CurrentCoordinates;

        foreach (Vector2Int chaindeCoordinates in routeNode.ChainedCoordinates)
        {
            Vector2Int direction = chaindeCoordinates - currentCoordinates;

            if(direction == Vector2Int.right)
            {
                connections |= RouteNodeConnections.Right;
            }

            if (direction == Vector2Int.down)
            {
                connections |= RouteNodeConnections.Down;
            }

            if (direction == Vector2Int.left)
            {
                connections |= RouteNodeConnections.Left;
            }

            if (direction == Vector2Int.up)
            {
                connections |= RouteNodeConnections.Up;
            }
        }

        return connections;
    }
}
