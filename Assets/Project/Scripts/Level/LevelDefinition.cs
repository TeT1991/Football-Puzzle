using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(menuName = "Football Puzzle/Level")]
public class LevelDefinition : ScriptableObject
{
    public const int MaxStars = 3;

    [SerializeField, Min(1)] private int _width = 5;
    [SerializeField, Min(1)] private int _height = 5;

    [SerializeField] private bool _hasCharacter;
    [SerializeField] private Vector2Int _characterPosition;

    [SerializeField, HideInInspector] private bool _hasGoal;
    [SerializeField] private Vector2Int _goalCoordinates;

    [SerializeField] private List<Vector2Int> _enemyPositions = new();
    [SerializeField] private List<Route> _enemyRoutes = new();
    [SerializeField] private List<Vector2Int> _starCoordinates = new();
    [FormerlySerializedAs("_testRoute")]
    [FormerlySerializedAs("_route")]
    [SerializeField] private Route _characterRoot = new();

    public int Width => _width;
    public int Height => _height;

    public bool HasCharacter => _hasCharacter;
    public Vector2Int CharacterPosition => _characterPosition;
    public bool HasGoal => _hasGoal;
    public Vector2Int GoalCoordinates => _goalCoordinates;
    public IReadOnlyList<Vector2Int> EnemyPositions => _enemyPositions;
    public IReadOnlyList<Route> EnemyRoutes => _enemyRoutes;
    public IReadOnlyList<Vector2Int> StarCoordinates => _starCoordinates;
    public Route CharacterRoute => _characterRoot;
    public bool HasGoalStar => _hasGoal && HasStarAt(_goalCoordinates);

    public void UpdateData(LevelData levelData)
    {
        SetSize(levelData.Width, levelData.Height);
        ValidateGoal();
        ValidateStars();
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
        ValidateStars();

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
        SetStars(source.StarCoordinates);
    }

    public bool HasStarAt(Vector2Int coordinates)
    {
        return _starCoordinates != null &&
               _starCoordinates.Contains(coordinates);
    }

    public bool IsGoalStar(Vector2Int coordinates)
    {
        return _hasGoal && _goalCoordinates == coordinates;
    }

    public bool TryAddStar(Vector2Int coordinates)
    {
        _starCoordinates ??= new List<Vector2Int>();
        ValidateStars();

        if (IsInsideGrid(coordinates) == false)
        {
            return false;
        }

        if (_starCoordinates.Contains(coordinates))
        {
            return true;
        }

        if (_starCoordinates.Count >= MaxStars)
        {
            return false;
        }

        _starCoordinates.Add(coordinates);
        return true;
    }

    public bool RemoveStar(Vector2Int coordinates)
    {
        if (_starCoordinates == null)
        {
            return false;
        }

        if (IsGoalStar(coordinates))
        {
            return false;
        }

        return _starCoordinates.Remove(coordinates);
    }

    public void SetStars(IReadOnlyList<Vector2Int> starCoordinates)
    {
        _starCoordinates ??= new List<Vector2Int>();
        _starCoordinates.Clear();

        if (starCoordinates == null)
        {
            EnsureGoalStar();
            return;
        }

        foreach (Vector2Int coordinates in starCoordinates)
        {
            if (_starCoordinates.Count >= MaxStars)
            {
                break;
            }

            if (IsInsideGrid(coordinates) &&
                _starCoordinates.Contains(coordinates) == false)
            {
                _starCoordinates.Add(coordinates);
            }
        }

        EnsureGoalStar();
    }

    public bool TryGetEnemyRoute(Vector2Int enemyCoordinates, out Route enemyRoute)
    {
        enemyRoute = null;

        if (_enemyRoutes == null)
        {
            return false;
        }

        foreach (Route route in _enemyRoutes)
        {
            if (route != null &&
                route.HasNodes &&
                route.StartCoordinates == enemyCoordinates)
            {
                enemyRoute = route;
                return true;
            }
        }

        return false;
    }

    public void SetEnemyRoute(
        Vector2Int enemyCoordinates,
        Route route)
    {
        _enemyRoutes ??= new List<Route>();

        if (_enemyPositions.Contains(enemyCoordinates) == false ||
            route == null ||
            route.HasNodes == false ||
            route.StartCoordinates != enemyCoordinates)
        {
            return;
        }

        RemoveEnemyRoute(enemyCoordinates);
        _enemyRoutes.Add(new Route(route.RouteNodes));
    }

    public void RemoveEnemyRoute(Vector2Int enemyCoordinates)
    {
        if (_enemyRoutes == null)
        {
            return;
        }

        _enemyRoutes.RemoveAll(route =>
            route == null ||
            route.HasNodes == false ||
            route.StartCoordinates == enemyCoordinates);
    }

    public void SetGoal(Vector2Int coordinates)
    {
        if (IsInsideGrid(coordinates) == false)
        {
            return;
        }

        Vector2Int previousGoalCoordinates = _goalCoordinates;
        bool hadGoal = _hasGoal;

        _goalCoordinates = coordinates;
        _hasGoal = true;

        if (hadGoal && previousGoalCoordinates != _goalCoordinates)
        {
            _starCoordinates?.Remove(previousGoalCoordinates);
        }

        EnsureGoalStar();
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

    private void ValidateStars()
    {
        _starCoordinates ??= new List<Vector2Int>();

        HashSet<Vector2Int> usedPositions = new();

        for (int i = _starCoordinates.Count - 1; i >= 0; i--)
        {
            Vector2Int coordinates = _starCoordinates[i];

            if (IsInsideGrid(coordinates) == false ||
                usedPositions.Add(coordinates) == false)
            {
                _starCoordinates.RemoveAt(i);
            }
        }

        EnsureGoalStar();
        TrimStarsToLimit();
    }

    private void EnsureGoalStar()
    {
        _starCoordinates ??= new List<Vector2Int>();

        if (_hasGoal == false ||
            IsInsideGrid(_goalCoordinates) == false ||
            _starCoordinates.Contains(_goalCoordinates))
        {
            return;
        }

        while (_starCoordinates.Count >= MaxStars)
        {
            if (RemoveLastOptionalStar() == false)
            {
                break;
            }
        }

        if (_starCoordinates.Count < MaxStars)
        {
            _starCoordinates.Add(_goalCoordinates);
        }
    }

    private void TrimStarsToLimit()
    {
        while (_starCoordinates.Count > MaxStars)
        {
            if (RemoveLastOptionalStar() == false)
            {
                _starCoordinates.RemoveAt(_starCoordinates.Count - 1);
            }
        }
    }

    private bool RemoveLastOptionalStar()
    {
        for (int i = _starCoordinates.Count - 1; i >= 0; i--)
        {
            if (IsGoalStar(_starCoordinates[i]) == false)
            {
                _starCoordinates.RemoveAt(i);
                return true;
            }
        }

        return false;
    }

    private void CopyEnemyRoutesFrom(LevelDefinition source)
    {
        _enemyRoutes ??= new List<Route>();
        _enemyRoutes.Clear();

        if (source.EnemyRoutes == null)
        {
            return;
        }

        foreach (Route route in source.EnemyRoutes)
        {
            if (route != null && route.HasNodes)
            {
                SetEnemyRoute(route.StartCoordinates, route);
            }
        }
    }

    private void RemoveRoutesWithoutEnemies()
    {
        _enemyRoutes ??= new List<Route>();
        HashSet<Vector2Int> usedEnemies = new();

        for (int i = _enemyRoutes.Count - 1; i >= 0; i--)
        {
            Route route = _enemyRoutes[i];

            if (route == null ||
                route.HasNodes == false ||
                _enemyPositions.Contains(route.StartCoordinates) == false ||
                usedEnemies.Add(route.StartCoordinates) == false)
            {
                _enemyRoutes.RemoveAt(i);
            }
        }
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
        ValidateStars();

        if (_characterRoot == null)
        {
            _characterRoot = new Route();
        }
    }
}
