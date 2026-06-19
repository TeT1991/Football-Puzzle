using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Football Puzzle/Level")]
public class LevelDefinition : ScriptableObject
{
    [SerializeField, Min(1)] private int _width = 5;
    [SerializeField, Min(1)] private int _height = 5;

    [SerializeField] private bool _hasCharacter;
    [SerializeField] private Vector2Int _characterPosition;

    [SerializeField, HideInInspector] private bool _hasGoal;
    [SerializeField] private Vector2Int _goalCoordinates;

    [SerializeField] private List<Vector2Int> _enemyPositions = new();
    [SerializeField] private List<EnemyRoute> _enemyRoutes = new();
    [FormerlySerializedAs("_testRoute")]
    [SerializeField] private Route _route = new();

    public int Width => _width;
    public int Height => _height;

    public bool HasCharacter => _hasCharacter;
    public Vector2Int CharacterPosition => _characterPosition;
    public bool HasGoal => _hasGoal;
    public Vector2Int GoalCoordinates => _goalCoordinates;
    public IReadOnlyList<Vector2Int> EnemyPositions => _enemyPositions;
    public IReadOnlyList<EnemyRoute> EnemyRoutes => _enemyRoutes;
    public Route TestRoute => _route;

    public void UpdateData(LevelData levelData)
    {
        SetSize(levelData.Width, levelData.Height);
        ValidateGoal();
    }

    public void SetData(
        int width,
        int height,
        bool hasCharacter,
        Vector2Int characterPosition,
        IReadOnlyList<Vector2Int> enemyPositions)
    {
        SetSize(width, height);

        _hasCharacter = hasCharacter;
        _characterPosition = characterPosition;

        ValidateGoal();

        _enemyPositions.Clear();

        HashSet<Vector2Int> usedPositions = new();

        if (enemyPositions == null)
        {
            RemoveRoutesWithoutEnemies();
            return;
        }

        foreach (Vector2Int enemyPosition in enemyPositions)
        {
            if (IsInsideGrid(enemyPosition) == false)
            {
                continue;
            }

            if (_hasCharacter && enemyPosition == _characterPosition)
            {
                continue;
            }

            if (usedPositions.Add(enemyPosition))
            {
                _enemyPositions.Add(enemyPosition);
            }
        }

        RemoveRoutesWithoutEnemies();
    }

    public void CopyFrom(LevelDefinition source)
    {
        if (source == null)
        {
            return;
        }

        SetData(
            source.Width,
            source.Height,
            source.HasCharacter,
            source.CharacterPosition,
            source.EnemyPositions);

        if (source.HasGoal)
        {
            SetGoal(source.GoalCoordinates);
        }
        else
        {
            ClearGoal();
        }

        CopyEnemyRoutesFrom(source);
    }

    public bool TryGetEnemyRoute(Vector2Int enemyCoordinates, out EnemyRoute enemyRoute)
    {
        enemyRoute = null;

        if (_enemyRoutes == null)
        {
            return false;
        }

        foreach (EnemyRoute route in _enemyRoutes)
        {
            if (route != null && route.EnemyStartCoordinates == enemyCoordinates)
            {
                enemyRoute = route;
                return true;
            }
        }

        return false;
    }

    public void SetEnemyRoute(
        Vector2Int enemyCoordinates,
        IReadOnlyList<Vector2Int> routeCoordinates)
    {
        _enemyRoutes ??= new List<EnemyRoute>();

        if (_enemyPositions.Contains(enemyCoordinates) == false)
        {
            return;
        }

        RemoveEnemyRoute(enemyCoordinates);
        _enemyRoutes.Add(new EnemyRoute(
            enemyCoordinates,
            BuildValidEnemyRoute(enemyCoordinates, routeCoordinates)));
    }

    public void RemoveEnemyRoute(Vector2Int enemyCoordinates)
    {
        if (_enemyRoutes == null)
        {
            return;
        }

        _enemyRoutes.RemoveAll(route =>
            route == null || route.EnemyStartCoordinates == enemyCoordinates);
    }

    public void SetGoal(Vector2Int coordinates)
    {
        if (IsInsideGrid(coordinates) == false)
        {
            return;
        }

        _goalCoordinates = coordinates;
        _hasGoal = true;
    }

    public void ClearGoal()
    {
        _hasGoal = false;
    }

    private void SetSize(int width, int height)
    {
        _width = Mathf.Max(1, width);
        _height = Mathf.Max(1, height);
    }

    private bool IsInsideGrid(Vector2Int position)
    {
        return position.x >= 0 &&
               position.y >= 0 &&
               position.x < _width &&
               position.y < _height;
    }

    private void ValidateGoal()
    {
        if (_hasGoal && IsInsideGrid(_goalCoordinates) == false)
        {
            _hasGoal = false;
        }
    }

    private void CopyEnemyRoutesFrom(LevelDefinition source)
    {
        _enemyRoutes ??= new List<EnemyRoute>();
        _enemyRoutes.Clear();

        if (source.EnemyRoutes == null)
        {
            return;
        }

        foreach (EnemyRoute route in source.EnemyRoutes)
        {
            if (route != null)
            {
                SetEnemyRoute(route.EnemyStartCoordinates, route.Coordinates);
            }
        }
    }

    private void RemoveRoutesWithoutEnemies()
    {
        _enemyRoutes ??= new List<EnemyRoute>();
        HashSet<Vector2Int> usedEnemies = new();

        for (int i = _enemyRoutes.Count - 1; i >= 0; i--)
        {
            EnemyRoute route = _enemyRoutes[i];

            if (route == null ||
                _enemyPositions.Contains(route.EnemyStartCoordinates) == false ||
                usedEnemies.Add(route.EnemyStartCoordinates) == false)
            {
                _enemyRoutes.RemoveAt(i);
                continue;
            }

            _enemyRoutes[i] = new EnemyRoute(
                route.EnemyStartCoordinates,
                BuildValidEnemyRoute(route.EnemyStartCoordinates, route.Coordinates));
        }
    }

    private List<Vector2Int> BuildValidEnemyRoute(
        Vector2Int enemyCoordinates,
        IReadOnlyList<Vector2Int> routeCoordinates)
    {
        List<Vector2Int> result = new() { enemyCoordinates };
        HashSet<Vector2Int> visited = new() { enemyCoordinates };

        if (routeCoordinates == null)
        {
            return result;
        }

        int startIndex = routeCoordinates.Count > 0 &&
                         routeCoordinates[0] == enemyCoordinates
            ? 1
            : 0;

        for (int i = startIndex; i < routeCoordinates.Count; i++)
        {
            Vector2Int coordinates = routeCoordinates[i];
            Vector2Int difference = coordinates - result[result.Count - 1];

            if (IsInsideGrid(coordinates) == false ||
                Mathf.Abs(difference.x) + Mathf.Abs(difference.y) != 1)
            {
                break;
            }

            if (coordinates == enemyCoordinates)
            {
                if (result.Count >= 3)
                {
                    result.Add(coordinates);
                }

                break;
            }

            if (visited.Add(coordinates) == false)
            {
                break;
            }

            result.Add(coordinates);
        }

        return result;
    }

    private void OnValidate()
    {
        SetSize(_width, _height);
        ValidateGoal();

        if (_enemyPositions == null)
        {
            _enemyPositions = new List<Vector2Int>();
        }

        RemoveRoutesWithoutEnemies();

        if (_route == null)
        {
            _route = new Route();
        }
    }
}
