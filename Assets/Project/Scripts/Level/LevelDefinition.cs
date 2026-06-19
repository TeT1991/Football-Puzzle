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
    [FormerlySerializedAs("_testRoute")]
    [SerializeField] private Route _route = new();

    public int Width => _width;
    public int Height => _height;

    public bool HasCharacter => _hasCharacter;
    public Vector2Int CharacterPosition => _characterPosition;
    public bool HasGoal => _hasGoal;
    public Vector2Int GoalCoordinates => _goalCoordinates;
    public IReadOnlyList<Vector2Int> EnemyPositions => _enemyPositions;
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

    private void OnValidate()
    {
        SetSize(_width, _height);
        ValidateGoal();

        if (_enemyPositions == null)
        {
            _enemyPositions = new List<Vector2Int>();
        }

        if (_route == null)
        {
            _route = new Route();
        }
    }
}
