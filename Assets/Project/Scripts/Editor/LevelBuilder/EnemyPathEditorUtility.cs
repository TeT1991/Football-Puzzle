using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

internal static class EnemyPathEditorUtility
{
    public static List<Vector2Int> ReadPath(
        LevelDefinition definition,
        Vector2Int enemyStartCoordinates)
    {
        EnemyRoute route;

        if (definition != null &&
            definition.TryGetEnemyRoute(enemyStartCoordinates, out route))
        {
            return new List<Vector2Int>(route.Coordinates);
        }

        return new List<Vector2Int> { enemyStartCoordinates };
    }

    public static void EnsurePath(
        LevelDefinition definition,
        Vector2Int enemyStartCoordinates)
    {
        if (definition == null ||
            definition.TryGetEnemyRoute(enemyStartCoordinates, out _))
        {
            return;
        }

        WritePath(
            definition,
            enemyStartCoordinates,
            new List<Vector2Int> { enemyStartCoordinates },
            "Add Enemy Path Start");
    }

    public static bool CanAppend(
        IReadOnlyList<Vector2Int> path,
        Vector2Int coordinates)
    {
        if (path == null || path.Count == 0 || IsClosed(path))
        {
            return false;
        }

        Vector2Int last = path[path.Count - 1];

        if (IsAdjacent(last, coordinates) == false)
        {
            return false;
        }

        if (coordinates == path[0])
        {
            return path.Count >= 3;
        }

        for (int i = 0; i < path.Count; i++)
        {
            if (path[i] == coordinates)
            {
                return false;
            }
        }

        return true;
    }

    public static bool Append(
        LevelDefinition definition,
        Vector2Int enemyStartCoordinates,
        Vector2Int coordinates)
    {
        List<Vector2Int> path = ReadPath(definition, enemyStartCoordinates);

        if (CanAppend(path, coordinates) == false)
        {
            return false;
        }

        path.Add(coordinates);
        WritePath(definition, enemyStartCoordinates, path, "Draw Enemy Path");
        return true;
    }

    public static bool RemoveLastSegment(
        LevelDefinition definition,
        Vector2Int enemyStartCoordinates,
        Vector2Int from,
        Vector2Int to)
    {
        List<Vector2Int> path = ReadPath(definition, enemyStartCoordinates);

        if (path.Count < 2 ||
            path[path.Count - 1] != from ||
            path[path.Count - 2] != to)
        {
            return false;
        }

        path.RemoveAt(path.Count - 1);
        WritePath(definition, enemyStartCoordinates, path, "Erase Enemy Path");
        return true;
    }

    public static bool IsEndpoint(IReadOnlyList<Vector2Int> path, Vector2Int coordinates)
    {
        return path != null &&
               path.Count > 0 &&
               path[path.Count - 1] == coordinates;
    }

    private static bool IsClosed(IReadOnlyList<Vector2Int> path)
    {
        return path.Count > 2 && path[0] == path[path.Count - 1];
    }

    private static bool IsAdjacent(Vector2Int first, Vector2Int second)
    {
        Vector2Int difference = second - first;
        return Mathf.Abs(difference.x) + Mathf.Abs(difference.y) == 1;
    }

    private static void WritePath(
        LevelDefinition definition,
        Vector2Int enemyStartCoordinates,
        IReadOnlyList<Vector2Int> path,
        string undoName)
    {
        Undo.RecordObject(definition, undoName);
        definition.SetEnemyRoute(enemyStartCoordinates, path);
        EditorUtility.SetDirty(definition);
    }
}
