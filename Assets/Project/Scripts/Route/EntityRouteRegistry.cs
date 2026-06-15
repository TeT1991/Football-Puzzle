using System;
using System.Collections.Generic;
using UnityEngine;

public class EntityRouteRegistry 
{
    private Dictionary<EntityView, Route> _routes;

    public EntityRouteRegistry()
    {
        _routes = new();
    }

    public void AddRoute(EntityView view, Route route)
    {
        _routes.Add(view, route);
    }

    public bool IsChainedCoordinates(EntityView view,  Vector2Int target)
    {
        if (_routes.ContainsKey(view) == false)
        {
            throw new Exception("No entity data");
        }

        Route route = _routes[view];

        foreach(RouteNode node in route.RouteNodes)
        {
            if (node.CurrentCoordinates != view.CurrentCoordinates)
            {
                continue;
            }

            if (node.HasConnectionTo(target))
            {
                return true;
            }
        }

        return false;
    }
}
