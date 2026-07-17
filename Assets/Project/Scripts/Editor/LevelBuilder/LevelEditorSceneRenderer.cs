using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

internal sealed class LevelEditorSceneRenderer
{
    private const float PlayerPathLineWidth = 13f;
    private const float EnemyPathLineWidth = 13f;
    private const float PathLaneSpacing = 0.14f;
    private const int PlayerPathOwnerId = 0;
    private const float GoalMarkerLineWidth = 3f;
    private const float GoalMarkerOuterRadius = 0.32f;
    private const float GoalMarkerInnerRadius = 0.14f;
    private const float CollectibleStarLineWidth = 2.5f;
    private const float CollectibleStarOuterRadius = 0.24f;
    private const float CollectibleStarInnerRadius = 0.1f;

    private static readonly Color PlayerPathColor =
        new(0.02f, 0.18f, 0.42f, 1f);
    private static readonly Color GoalMarkerColor =
        new(1f, 0.75f, 0.05f, 0.9f);
    private static readonly Color GoalMarkerFillColor =
        new(1f, 0.75f, 0.05f, 0.35f);
    private static readonly Color CollectibleStarColor =
        new(1f, 0.88f, 0.05f, 0.95f);
    private static readonly Color CollectibleStarFillColor =
        new(1f, 0.88f, 0.05f, 0.5f);

    private readonly LevelEditorTool _tool;
    private readonly LevelEditorSceneQuery _sceneQuery;

    public LevelEditorSceneRenderer(
        LevelEditorTool tool,
        LevelEditorSceneQuery sceneQuery)
    {
        _tool = tool;
        _sceneQuery = sceneQuery;
    }

    public void Draw(bool showPlayerPaths)
    {
        Dictionary<(Vector2Int, Vector2Int), List<int>> segmentOwners =
            BuildPathSegmentOwners();

        DrawPlayerPaths(segmentOwners, showPlayerPaths);
        DrawEnemyPaths(segmentOwners);
        DrawLevelGoalMarker();
        DrawCollectibleStarMarkers();
    }

    public void ApplySelectedEnemyColor(LevelEditorPlacedObject selectedEnemy)
    {
        if (selectedEnemy == null)
        {
            return;
        }

        ApplyEnemyColor(selectedEnemy, GetEnemyColor(selectedEnemy.Coordinates));
    }

    private Dictionary<(Vector2Int, Vector2Int), List<int>>
        BuildPathSegmentOwners()
    {
        Dictionary<(Vector2Int, Vector2Int), List<int>> owners = new();
        LevelDefinition definition = _tool.LevelDefinition;

        if (definition == null)
        {
            return owners;
        }

        Dictionary<Vector2Int, HashSet<Vector2Int>> playerGraph =
            PlayerPathEditorUtility.ReadGraph(definition);

        foreach (KeyValuePair<Vector2Int, HashSet<Vector2Int>> node in playerGraph)
        {
            foreach (Vector2Int connectedCoordinates in node.Value)
            {
                if (ShouldDrawConnection(node.Key, connectedCoordinates))
                {
                    AddPathSegmentOwner(
                        owners,
                        node.Key,
                        connectedCoordinates,
                        PlayerPathOwnerId);
                }
            }
        }

        if (definition.EnemyRoutes == null)
        {
            return owners;
        }

        foreach (Route route in definition.EnemyRoutes)
        {
            if (route == null || route.HasNodes == false)
            {
                continue;
            }

            int ownerId = GetEnemyPathOwnerId(
                definition,
                route.StartCoordinates);

            for (int i = 1; i < route.RouteNodes.Count; i++)
            {
                AddPathSegmentOwner(
                    owners,
                    route.RouteNodes[i - 1].CurrentCoordinates,
                    route.RouteNodes[i].CurrentCoordinates,
                    ownerId);
            }
        }

        foreach (List<int> segmentOwners in owners.Values)
        {
            segmentOwners.Sort();
        }

        return owners;
    }

    private void DrawPlayerPaths(
        Dictionary<(Vector2Int, Vector2Int), List<int>> segmentOwners,
        bool showPlayerPaths)
    {
        if (showPlayerPaths == false || _tool.LevelDefinition == null)
        {
            return;
        }

        Dictionary<Vector2Int, HashSet<Vector2Int>> graph =
            PlayerPathEditorUtility.ReadGraph(_tool.LevelDefinition);

        if (graph.Count == 0)
        {
            return;
        }

        Dictionary<Vector2Int, Vector3> positions = GetCellPositions();
        Color previousColor = Handles.color;
        CompareFunction previousZTest = Handles.zTest;

        Handles.color = PlayerPathColor;
        Handles.zTest = CompareFunction.Always;

        foreach (KeyValuePair<Vector2Int, HashSet<Vector2Int>> node in graph)
        {
            foreach (Vector2Int connectedCoordinates in node.Value)
            {
                if (ShouldDrawConnection(node.Key, connectedCoordinates) == false ||
                    positions.TryGetValue(node.Key, out Vector3 startPosition) == false ||
                    positions.TryGetValue(connectedCoordinates, out Vector3 endPosition) == false)
                {
                    continue;
                }

                Vector3 offset = GetPathLaneOffset(
                    node.Key,
                    connectedCoordinates,
                    startPosition,
                    endPosition,
                    PlayerPathOwnerId,
                    segmentOwners);

                Handles.DrawLine(
                    startPosition + offset,
                    endPosition + offset,
                    PlayerPathLineWidth);
            }
        }

        Handles.color = previousColor;
        Handles.zTest = previousZTest;
    }

    private void DrawEnemyPaths(
        Dictionary<(Vector2Int, Vector2Int), List<int>> segmentOwners)
    {
        LevelDefinition definition = _tool.LevelDefinition;

        if (definition == null || definition.EnemyRoutes == null)
        {
            return;
        }

        ApplyEnemyRouteColors(definition);

        Dictionary<Vector2Int, Vector3> positions = GetCellPositions();
        Color previousColor = Handles.color;
        CompareFunction previousZTest = Handles.zTest;
        Handles.zTest = CompareFunction.Always;

        foreach (Route route in definition.EnemyRoutes)
        {
            if (route == null || route.RouteNodes.Count < 2)
            {
                continue;
            }

            Handles.color = GetEnemyPathColor(route.StartCoordinates);

            for (int i = 1; i < route.RouteNodes.Count; i++)
            {
                Vector2Int previousCoordinates =
                    route.RouteNodes[i - 1].CurrentCoordinates;
                Vector2Int currentCoordinates =
                    route.RouteNodes[i].CurrentCoordinates;

                if (positions.TryGetValue(previousCoordinates, out Vector3 start) == false ||
                    positions.TryGetValue(currentCoordinates, out Vector3 end) == false)
                {
                    continue;
                }

                Vector3 offset = GetPathLaneOffset(
                    previousCoordinates,
                    currentCoordinates,
                    start,
                    end,
                    GetEnemyPathOwnerId(definition, route.StartCoordinates),
                    segmentOwners);

                Handles.DrawLine(
                    start + offset,
                    end + offset,
                    EnemyPathLineWidth);
            }
        }

        Handles.color = previousColor;
        Handles.zTest = previousZTest;
    }

    private void DrawLevelGoalMarker()
    {
        LevelDefinition definition = _tool.LevelDefinition;

        if (definition == null ||
            definition.HasGoal == false ||
            _sceneQuery.TryGetCellByCoordinates(
                definition.GoalCoordinates,
                out CellView cell) == false)
        {
            return;
        }

        DrawStarShape(
            cell.transform.position,
            GoalMarkerOuterRadius,
            GoalMarkerInnerRadius,
            GoalMarkerLineWidth,
            GoalMarkerColor,
            GoalMarkerFillColor);
    }

    private void DrawCollectibleStarMarkers()
    {
        LevelDefinition definition = _tool.LevelDefinition;

        if (definition == null || definition.StarCoordinates == null)
        {
            return;
        }

        foreach (Vector2Int coordinates in definition.StarCoordinates)
        {
            if (_sceneQuery.TryGetCellByCoordinates(
                    coordinates,
                    out CellView cell) == false)
            {
                continue;
            }

            DrawStarShape(
                cell.transform.position,
                CollectibleStarOuterRadius,
                CollectibleStarInnerRadius,
                CollectibleStarLineWidth,
                CollectibleStarColor,
                CollectibleStarFillColor);
        }
    }

    private static void DrawStarShape(
        Vector3 center,
        float outerRadius,
        float innerRadius,
        float lineWidth,
        Color lineColor,
        Color fillColor)
    {
        Vector3[] points = new Vector3[11];

        for (int i = 0; i < 10; i++)
        {
            float radius = i % 2 == 0
                ? outerRadius
                : innerRadius;
            float angle = Mathf.PI * 0.5f + i * Mathf.PI / 5f;

            points[i] = center + new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0f);
        }

        points[10] = points[0];

        Color previousColor = Handles.color;
        CompareFunction previousZTest = Handles.zTest;

        Handles.zTest = CompareFunction.Always;
        Handles.color = fillColor;

        for (int i = 0; i < 10; i++)
        {
            Handles.DrawAAConvexPolygon(center, points[i], points[i + 1]);
        }

        Handles.color = lineColor;
        Handles.DrawAAPolyLine(lineWidth, points);

        Handles.color = previousColor;
        Handles.zTest = previousZTest;
    }

    private Dictionary<Vector2Int, Vector3> GetCellPositions()
    {
        Dictionary<Vector2Int, Vector3> positions = new();

        foreach (CellView cell in _sceneQuery.GetSceneCells())
        {
            if (cell != null)
            {
                positions[cell.Coordinates] = cell.transform.position;
            }
        }

        return positions;
    }

    private void ApplyEnemyRouteColors(LevelDefinition definition)
    {
        foreach (LevelEditorPlacedObject enemy in _sceneQuery.GetSceneEnemies())
        {
            if (enemy == null ||
                enemy.Type != LevelEditorObjectType.Enemy ||
                definition.TryGetEnemyRoute(enemy.Coordinates, out _) == false)
            {
                continue;
            }

            ApplyEnemyColor(enemy, GetEnemyColor(enemy.Coordinates));
        }
    }

    private static void ApplyEnemyColor(
        LevelEditorPlacedObject enemy,
        Color color)
    {
        foreach (SpriteRenderer spriteRenderer in
                 enemy.GetComponentsInChildren<SpriteRenderer>(true))
        {
            spriteRenderer.color = color;
        }
    }

    private static void AddPathSegmentOwner(
        Dictionary<(Vector2Int, Vector2Int), List<int>> owners,
        Vector2Int first,
        Vector2Int second,
        int ownerId)
    {
        (Vector2Int, Vector2Int) key = GetPathSegmentKey(first, second);

        if (owners.TryGetValue(key, out List<int> segmentOwners) == false)
        {
            segmentOwners = new List<int>();
            owners.Add(key, segmentOwners);
        }

        if (segmentOwners.Contains(ownerId) == false)
        {
            segmentOwners.Add(ownerId);
        }
    }

    private static Vector3 GetPathLaneOffset(
        Vector2Int firstCoordinates,
        Vector2Int secondCoordinates,
        Vector3 firstPosition,
        Vector3 secondPosition,
        int ownerId,
        Dictionary<(Vector2Int, Vector2Int), List<int>> owners)
    {
        (Vector2Int, Vector2Int) key = GetPathSegmentKey(
            firstCoordinates,
            secondCoordinates);

        if (owners.TryGetValue(key, out List<int> segmentOwners) == false ||
            segmentOwners.Count < 2)
        {
            return Vector3.zero;
        }

        int laneIndex = segmentOwners.IndexOf(ownerId);

        if (laneIndex < 0)
        {
            return Vector3.zero;
        }

        Vector3 direction = ShouldDrawConnection(
            firstCoordinates,
            secondCoordinates)
            ? secondPosition - firstPosition
            : firstPosition - secondPosition;

        direction.Normalize();

        float centeredLane = laneIndex - (segmentOwners.Count - 1) * 0.5f;
        Vector3 perpendicular = new(-direction.y, direction.x, 0f);
        return perpendicular * centeredLane * PathLaneSpacing;
    }

    private static (Vector2Int, Vector2Int) GetPathSegmentKey(
        Vector2Int first,
        Vector2Int second)
    {
        return ShouldDrawConnection(first, second)
            ? (first, second)
            : (second, first);
    }

    private static int GetEnemyPathOwnerId(
        LevelDefinition definition,
        Vector2Int enemyCoordinates)
    {
        return 1 + enemyCoordinates.y * definition.Width + enemyCoordinates.x;
    }

    private static Color GetEnemyPathColor(Vector2Int coordinates)
    {
        Color color = Color.Lerp(GetEnemyColor(coordinates), Color.black, 0.35f);
        color.a = 1f;
        return color;
    }

    private static Color GetEnemyColor(Vector2Int coordinates)
    {
        int hash = coordinates.x * 73856093 ^ coordinates.y * 19349663;
        float hue = Mathf.Repeat(Mathf.Abs(hash) * 0.000013f, 1f);
        return Color.HSVToRGB(hue, 0.75f, 1f);
    }

    private static bool ShouldDrawConnection(
        Vector2Int first,
        Vector2Int second)
    {
        return first.x < second.x ||
               (first.x == second.x && first.y < second.y);
    }
}
