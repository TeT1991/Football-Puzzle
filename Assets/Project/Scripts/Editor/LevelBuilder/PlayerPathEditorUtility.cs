using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

internal static class PlayerPathEditorUtility
{
    private const string RouteProperty = "_characterRoot";
    private const string RouteNodesProperty = "_routeNodes";
    private const string CurrentCoordinatesProperty = "_currentCoordinates";
    private const string ChainedCoordinatesProperty = "_chainedCoordinates";

    public static Dictionary<Vector2Int, HashSet<Vector2Int>> ReadGraph(LevelDefinition definition)
    {
        Dictionary<Vector2Int, HashSet<Vector2Int>> graph = new();

        if (definition == null)
        {
            return graph;
        }

        SerializedObject serializedDefinition = new(definition);
        serializedDefinition.Update();

        if (TryGetNodesProperty(serializedDefinition, out SerializedProperty nodesProperty) == false)
        {
            return graph;
        }

        for (int i = 0; i < nodesProperty.arraySize; i++)
        {
            SerializedProperty nodeProperty = nodesProperty.GetArrayElementAtIndex(i);
            SerializedProperty coordinatesProperty =
                nodeProperty.FindPropertyRelative(CurrentCoordinatesProperty);
            SerializedProperty connectionsProperty =
                nodeProperty.FindPropertyRelative(ChainedCoordinatesProperty);

            if (coordinatesProperty == null)
            {
                continue;
            }

            Vector2Int coordinates = coordinatesProperty.vector2IntValue;

            if (graph.TryGetValue(coordinates, out HashSet<Vector2Int> connections) == false)
            {
                connections = new HashSet<Vector2Int>();
                graph.Add(coordinates, connections);
            }

            if (connectionsProperty == null)
            {
                continue;
            }

            for (int connectionIndex = 0;
                 connectionIndex < connectionsProperty.arraySize;
                 connectionIndex++)
            {
                Vector2Int connectedCoordinates =
                    connectionsProperty.GetArrayElementAtIndex(connectionIndex).vector2IntValue;

                if (connectedCoordinates != coordinates)
                {
                    connections.Add(connectedCoordinates);
                }
            }
        }

        return graph;
    }

    public static bool ContainsNode(LevelDefinition definition, Vector2Int coordinates)
    {
        return ReadGraph(definition).ContainsKey(coordinates);
    }

    public static bool HasConnection(
        LevelDefinition definition,
        Vector2Int from,
        Vector2Int to)
    {
        Dictionary<Vector2Int, HashSet<Vector2Int>> graph = ReadGraph(definition);

        return graph.TryGetValue(from, out HashSet<Vector2Int> connections) &&
               connections.Contains(to);
    }

    public static bool EnsureNode(LevelDefinition definition, Vector2Int coordinates)
    {
        Dictionary<Vector2Int, HashSet<Vector2Int>> graph = ReadGraph(definition);

        if (graph.ContainsKey(coordinates))
        {
            return false;
        }

        graph.Add(coordinates, new HashSet<Vector2Int>());
        WriteGraph(definition, graph, "Add Player Path Start");
        return true;
    }

    public static bool AddBidirectionalConnection(
        LevelDefinition definition,
        Vector2Int first,
        Vector2Int second)
    {
        if (definition == null || first == second)
        {
            return false;
        }

        Dictionary<Vector2Int, HashSet<Vector2Int>> graph = ReadGraph(definition);

        if (graph.TryGetValue(first, out HashSet<Vector2Int> firstConnections) == false)
        {
            firstConnections = new HashSet<Vector2Int>();
            graph.Add(first, firstConnections);
        }

        if (graph.TryGetValue(second, out HashSet<Vector2Int> secondConnections) == false)
        {
            secondConnections = new HashSet<Vector2Int>();
            graph.Add(second, secondConnections);
        }

        bool changed = firstConnections.Add(second);
        changed |= secondConnections.Add(first);

        if (changed)
        {
            WriteGraph(definition, graph, "Draw Player Path");
        }

        return changed;
    }

    public static bool RemoveBidirectionalConnection(
        LevelDefinition definition,
        Vector2Int first,
        Vector2Int second)
    {
        if (definition == null || first == second)
        {
            return false;
        }

        Dictionary<Vector2Int, HashSet<Vector2Int>> graph = ReadGraph(definition);
        bool changed = false;

        if (graph.TryGetValue(first, out HashSet<Vector2Int> firstConnections))
        {
            changed |= firstConnections.Remove(second);
        }

        if (graph.TryGetValue(second, out HashSet<Vector2Int> secondConnections))
        {
            changed |= secondConnections.Remove(first);
        }

        if (changed)
        {
            WriteGraph(definition, graph, "Erase Player Path");
        }

        return changed;
    }

    public static void CopyPlayerRoute(LevelDefinition source, LevelDefinition destination)
    {
        if (source == null || destination == null)
        {
            return;
        }

        WriteGraph(destination, ReadGraph(source), "Copy Player Path");
    }

    private static void WriteGraph(
        LevelDefinition definition,
        Dictionary<Vector2Int, HashSet<Vector2Int>> graph,
        string undoName)
    {
        if (definition == null)
        {
            return;
        }

        Undo.RecordObject(definition, undoName);

        SerializedObject serializedDefinition = new(definition);
        serializedDefinition.Update();

        if (TryGetNodesProperty(serializedDefinition, out SerializedProperty nodesProperty) == false)
        {
            Debug.LogError("Player route properties were not found in LevelDefinition.");
            return;
        }

        List<Vector2Int> nodeCoordinates = new(graph.Keys);
        nodeCoordinates.Sort(CompareCoordinates);
        nodesProperty.arraySize = nodeCoordinates.Count;

        for (int nodeIndex = 0; nodeIndex < nodeCoordinates.Count; nodeIndex++)
        {
            Vector2Int coordinates = nodeCoordinates[nodeIndex];
            SerializedProperty nodeProperty = nodesProperty.GetArrayElementAtIndex(nodeIndex);
            SerializedProperty coordinatesProperty =
                nodeProperty.FindPropertyRelative(CurrentCoordinatesProperty);
            SerializedProperty connectionsProperty =
                nodeProperty.FindPropertyRelative(ChainedCoordinatesProperty);

            coordinatesProperty.vector2IntValue = coordinates;

            List<Vector2Int> connections = new(graph[coordinates]);
            connections.Sort(CompareCoordinates);
            connectionsProperty.arraySize = connections.Count;

            for (int connectionIndex = 0;
                 connectionIndex < connections.Count;
                 connectionIndex++)
            {
                connectionsProperty.GetArrayElementAtIndex(connectionIndex).vector2IntValue =
                    connections[connectionIndex];
            }
        }

        serializedDefinition.ApplyModifiedProperties();
        EditorUtility.SetDirty(definition);
    }

    private static bool TryGetNodesProperty(
        SerializedObject serializedDefinition,
        out SerializedProperty nodesProperty)
    {
        nodesProperty = null;

        SerializedProperty routeProperty = serializedDefinition.FindProperty(RouteProperty);

        if (routeProperty == null)
        {
            return false;
        }

        nodesProperty = routeProperty.FindPropertyRelative(RouteNodesProperty);
        return nodesProperty != null;
    }

    private static int CompareCoordinates(Vector2Int first, Vector2Int second)
    {
        int xComparison = first.x.CompareTo(second.x);
        return xComparison != 0 ? xComparison : first.y.CompareTo(second.y);
    }
}
