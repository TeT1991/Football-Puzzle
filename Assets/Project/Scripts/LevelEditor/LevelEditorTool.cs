using System.Collections.Generic;
using UnityEngine;

public class LevelEditorTool : MonoBehaviour
{
    private const float CellSize = 1f;

    [Header("Level Data")]
    [SerializeField] private LevelDefinition _levelDefinition;
    [SerializeField, Min(1)] private int _gridWidth = 5;
    [SerializeField, Min(1)] private int _gridHeight = 5;

    [Header("Grid")]
    [SerializeField] private CellView _cellViewPrefab;
    [SerializeField] private Transform _cellsParent;

    [Header("EntityView")]
    [SerializeField] private GameObject _characterPrefab;
    [SerializeField] private Transform _characterParent;

    [Header("Enemies")]
    [SerializeField] private GameObject _enemyPrefab;
    [SerializeField] private Transform _enemiesParent;

    private LevelData _levelData;
    private GridBuilder _gridBuilder;
    private CellView[,] _cells;

    private LevelEditorMode _currentMode = LevelEditorMode.None;

    public LevelDefinition LevelDefinition => _levelDefinition;
    public LevelEditorMode CurrentMode => _currentMode;

    public float GridPlaneZ
    {
        get
        {
            if (_cellsParent != null)
            {
                return _cellsParent.position.z;
            }

            return transform.position.z;
        }
    }

    public void CreateLevel()
    {
        SetLevelData();

        if (CanBuildGrid() == false)
        {
            return;
        }

        EnsureLevelDefinitionExists(_levelData);

        ClearLevel();

        BuildGrid();
        SpawnObjectsFromDefinition();

        ApplySceneToDefinition();
    }

    public void ClearLevel()
    {
        HashSet<Transform> clearedParents = new HashSet<Transform>();

        ClearParent(_cellsParent, clearedParents);
        ClearParent(_characterParent, clearedParents);
        ClearParent(_enemiesParent, clearedParents);

        _cells = null;
        _gridBuilder = null;

        SetMode(LevelEditorMode.None);
    }

    public void StartPlacingCharacter()
    {
        if (EnsureGridReady() == false)
        {
            return;
        }

        SetMode(LevelEditorMode.PlacingCharacter);
    }

    public void StartPlacingEnemy()
    {
        if (EnsureGridReady() == false)
        {
            return;
        }

        SetMode(LevelEditorMode.PlacingEnemy);
    }

    public void StartPlacingStar()
    {
        if (EnsureGridReady() == false)
        {
            return;
        }

        SetLevelData();
        EnsureLevelDefinitionExists(_levelData);
        SetMode(LevelEditorMode.PlacingStar);
    }

    public void StopPlacement()
    {
        SetMode(LevelEditorMode.None);
    }

    public bool DeleteEnemy(Vector2Int coordinates)
    {
        if (_enemiesParent == null)
        {
            return false;
        }

        LevelEditorPlacedObject[] placedObjects =
            _enemiesParent.GetComponentsInChildren<LevelEditorPlacedObject>(true);

        foreach (LevelEditorPlacedObject placedObject in placedObjects)
        {
            if (placedObject.Type != LevelEditorObjectType.Enemy ||
                placedObject.Coordinates != coordinates)
            {
                continue;
            }

            DestroyObject(placedObject.gameObject);
            ApplySceneToDefinition();
            return true;
        }

        return false;
    }

    public void StartPlacingCharacters()
    {
        StartPlacingCharacter();
    }

    public void StopPlacingCharacters()
    {
        StopPlacement();
    }

    public bool HandleSceneClick(Vector2 worldPosition)
    {
        return HandleSceneClick(worldPosition, 0);
    }

    public bool HandleSceneClick(Vector2 worldPosition, int mouseButton)
    {
        if (_currentMode == LevelEditorMode.None)
        {
            return false;
        }

        if (TryGetCell(worldPosition, out CellView cell) == false)
        {
            return false;
        }

        switch (_currentMode)
        {
            case LevelEditorMode.PlacingCharacter:
                return TryPlaceCharacter(cell);

            case LevelEditorMode.PlacingEnemy:
                return TryPlaceEnemy(cell);

            case LevelEditorMode.PlacingStar:
                return mouseButton == 1
                    ? TryRemoveStar(cell)
                    : TryPlaceStar(cell);

            default:
                return false;
        }
    }

    public void ApplySceneToDefinition()
    {
        SetLevelData();

        EnsureLevelDefinitionExists(_levelData);

        bool hasCharacter = TryGetCharacterCoordinates(out Vector2Int characterPosition);
        List<Vector2Int> enemyPositions = GetEnemyCoordinates();

        _levelDefinition.SetData(
            _levelData.Width,
            _levelData.Height,
            hasCharacter,
            characterPosition,
            enemyPositions);
    }

    public void SetLevelData()
    {
        if (_levelDefinition != null)
        {
            _levelData = new LevelData(
                _levelDefinition.Width,
                _levelDefinition.Height,
                _levelDefinition.CharacterPosition);

            return;
        }

        _levelData = new LevelData(
            Mathf.Max(1, _gridWidth),
            Mathf.Max(1, _gridHeight),
            Vector2Int.zero);
    }

    public void SetLevelDefinition(LevelDefinition levelDefinition)
    {
        _levelDefinition = levelDefinition;
    }

    public void SetMode(LevelEditorMode mode)
    {
        _currentMode = mode;
    }

    public bool TryRebuildCellsFromScene()
    {
        if (_cellsParent == null)
        {
            Debug.LogWarning("Cells Parent is not assigned.");
            return false;
        }

        CellView[] sceneCells = _cellsParent.GetComponentsInChildren<CellView>(true);

        if (sceneCells.Length == 0)
        {
            return false;
        }

        SetLevelData();

        _cells = new CellView[_levelData.Width, _levelData.Height];

        foreach (CellView cell in sceneCells)
        {
            Vector2Int coordinates = cell.Coordinates;

            if (IsInsideGrid(coordinates, _levelData.Width, _levelData.Height) == false)
            {
                Debug.LogWarning($"Cell {cell.name} has invalid coordinates {coordinates}");
                continue;
            }

            _cells[coordinates.x, coordinates.y] = cell;
        }

        return true;
    }

    private void BuildGrid()
    {
        _gridBuilder = new GridBuilder(_cellsParent, _cellViewPrefab);

        _cells = _gridBuilder.CreateTiles(_levelData.Width, _levelData.Height);

    }

    private void SpawnObjectsFromDefinition()
    {
        if (_levelDefinition == null)
        {
            return;
        }

        if (_levelDefinition.HasCharacter)
        {
            if (TryGetCellByCoordinates(_levelDefinition.CharacterPosition, out CellView characterCell))
            {
                TryPlaceCharacter(characterCell, false);
            }
        }

        foreach (Vector2Int enemyPosition in _levelDefinition.EnemyPositions)
        {
            if (TryGetCellByCoordinates(enemyPosition, out CellView enemyCell))
            {
                TryPlaceEnemy(enemyCell, false);
            }
        }
    }

    private bool TryPlaceCharacter(CellView cell, bool updateDefinition = true)
    {
        if (cell == null)
        {
            return false;
        }

        if (_characterPrefab == null)
        {
            Debug.LogWarning("EntityView Prefab is not assigned.");
            return false;
        }

        if (_characterParent == null)
        {
            Debug.LogWarning("EntityView Parent is not assigned.");
            return false;
        }

        Vector2Int coordinates = cell.Coordinates;

        if (IsEnemyAt(coordinates))
        {
            Debug.LogWarning($"Can't place character at {coordinates}. Cell is occupied by enemy.");
            return false;
        }

        GameObject character = GetCharacterObject();

        if (character == null)
        {
            character = Instantiate(_characterPrefab, _characterParent);
        }

        SetupPlacedObject(
            character,
            LevelEditorObjectType.Character,
            coordinates,
            cell.transform.position,
            "EntityView");

        if (updateDefinition)
        {
            ApplySceneToDefinition();
        }

        return true;
    }

    private bool TryPlaceEnemy(CellView cell, bool updateDefinition = true)
    {
        if (cell == null)
        {
            return false;
        }

        if (_enemyPrefab == null)
        {
            Debug.LogWarning("Enemy Prefab is not assigned.");
            return false;
        }

        if (_enemiesParent == null)
        {
            Debug.LogWarning("Enemies Parent is not assigned.");
            return false;
        }

        Vector2Int coordinates = cell.Coordinates;

        if (IsCharacterAt(coordinates))
        {
            Debug.LogWarning($"Can't place enemy at {coordinates}. Cell is occupied by character.");
            return false;
        }

        if (IsEnemyAt(coordinates))
        {
            Debug.LogWarning($"Can't place enemy at {coordinates}. Cell is already occupied by enemy.");
            return false;
        }

        GameObject enemy = Instantiate(_enemyPrefab, _enemiesParent);

        SetupPlacedObject(
            enemy,
            LevelEditorObjectType.Enemy,
            coordinates,
            cell.transform.position,
            "Enemy");

        if (updateDefinition)
        {
            ApplySceneToDefinition();
        }

        return true;
    }

    private bool TryPlaceStar(CellView cell)
    {
        if (cell == null)
        {
            return false;
        }

        SetLevelData();
        EnsureLevelDefinitionExists(_levelData);

        Vector2Int coordinates = cell.Coordinates;

        if (_levelDefinition.HasStarAt(coordinates))
        {
            return false;
        }

        if (_levelDefinition.TryAddStar(coordinates) == false)
        {
            Debug.LogWarning(
                $"Can't place star at {coordinates}. Maximum stars: {LevelDefinition.MaxStars}.");
            return false;
        }

        return true;
    }

    private bool TryRemoveStar(CellView cell)
    {
        if (cell == null || _levelDefinition == null)
        {
            return false;
        }

        return _levelDefinition.RemoveStar(cell.Coordinates);
    }

    private void SetupPlacedObject(
        GameObject target,
        LevelEditorObjectType objectType,
        Vector2Int coordinates,
        Vector3 worldPosition,
        string objectName)
    {
        target.transform.position = worldPosition;
        target.name = $"{objectName} {coordinates.x}:{coordinates.y}";

        LevelEditorPlacedObject placedObject = target.GetComponent<LevelEditorPlacedObject>();

        if (placedObject == null)
        {
            placedObject = target.AddComponent<LevelEditorPlacedObject>();
        }

        placedObject.Init(objectType, coordinates);
    }

    private bool TryGetCell(Vector2 worldPosition, out CellView cell)
    {
        cell = null;

        if (EnsureGridReady() == false)
        {
            return false;
        }

        Vector3 localPosition = _cellsParent.InverseTransformPoint(worldPosition);

        int gridWidth = _cells.GetLength(0);
        int gridHeight = _cells.GetLength(1);
        Vector2Int coordinates = GameUtility.ConvertPositionToCoordinates(
            localPosition,
            gridWidth,
            gridHeight);

        if (TryGetCellByCoordinates(coordinates, out CellView candidate) == false)
        {
            return false;
        }

        Vector3 cellLocalPosition = candidate.transform.localPosition;

        bool insideCell =
            Mathf.Abs(localPosition.x - cellLocalPosition.x) <= CellSize * 0.5f &&
            Mathf.Abs(localPosition.y - cellLocalPosition.y) <= CellSize * 0.5f;

        if (insideCell == false)
        {
            return false;
        }

        cell = candidate;
        return true;
    }

    private bool TryGetCellByCoordinates(Vector2Int coordinates, out CellView cell)
    {
        cell = null;

        if (_cells == null)
        {
            return false;
        }

        if (IsInsideGrid(coordinates, _cells.GetLength(0), _cells.GetLength(1)) == false)
        {
            return false;
        }

        cell = _cells[coordinates.x, coordinates.y];

        return cell != null;
    }

    private bool EnsureGridReady()
    {
        if (_cells != null)
        {
            return true;
        }

        if (TryRebuildCellsFromScene())
        {
            return true;
        }

        CreateLevel();

        return _cells != null;
    }

    private bool CanBuildGrid()
    {
        if (_cellViewPrefab == null)
        {
            Debug.LogWarning("Cell View Prefab is not assigned.");
            return false;
        }

        if (_cellsParent == null)
        {
            Debug.LogWarning("Cells Parent is not assigned.");
            return false;
        }

        return true;
    }

    private void EnsureLevelDefinitionExists(LevelData levelData)
    {
        if (_levelDefinition != null)
        {
            return;
        }

        _levelDefinition = ScriptableObject.CreateInstance<LevelDefinition>();
        _levelDefinition.UpdateData(levelData);
        _levelDefinition.name = "Unsaved Level Definition";
    }

    private GameObject GetCharacterObject()
    {
        if (_characterParent == null)
        {
            return null;
        }

        LevelEditorPlacedObject[] placedObjects =
            _characterParent.GetComponentsInChildren<LevelEditorPlacedObject>(true);

        GameObject foundCharacter = null;

        foreach (LevelEditorPlacedObject placedObject in placedObjects)
        {
            if (placedObject.Type != LevelEditorObjectType.Character)
            {
                continue;
            }

            if (foundCharacter == null)
            {
                foundCharacter = placedObject.gameObject;
            }
            else
            {
                DestroyObject(placedObject.gameObject);
            }
        }

        return foundCharacter;
    }

    private bool TryGetCharacterCoordinates(out Vector2Int coordinates)
    {
        coordinates = default;

        GameObject character = GetCharacterObject();

        if (character == null)
        {
            return false;
        }

        LevelEditorPlacedObject placedObject = character.GetComponent<LevelEditorPlacedObject>();

        if (placedObject == null)
        {
            return false;
        }

        coordinates = placedObject.Coordinates;
        return true;
    }

    private List<Vector2Int> GetEnemyCoordinates()
    {
        List<Vector2Int> coordinates = new List<Vector2Int>();

        if (_enemiesParent == null)
        {
            return coordinates;
        }

        LevelEditorPlacedObject[] placedObjects =
            _enemiesParent.GetComponentsInChildren<LevelEditorPlacedObject>(true);

        HashSet<Vector2Int> usedPositions = new HashSet<Vector2Int>();

        SetLevelData();

        foreach (LevelEditorPlacedObject placedObject in placedObjects)
        {
            if (placedObject.Type != LevelEditorObjectType.Enemy)
            {
                continue;
            }

            Vector2Int position = placedObject.Coordinates;

            if (IsInsideGrid(position, _levelData.Width, _levelData.Height) == false)
            {
                continue;
            }

            if (IsCharacterAt(position))
            {
                continue;
            }

            if (usedPositions.Add(position))
            {
                coordinates.Add(position);
            }
        }

        return coordinates;
    }

    private bool IsCharacterAt(Vector2Int coordinates)
    {
        return TryGetCharacterCoordinates(out Vector2Int characterCoordinates) &&
               characterCoordinates == coordinates;
    }

    private bool IsEnemyAt(Vector2Int coordinates)
    {
        List<Vector2Int> enemyCoordinates = GetEnemyCoordinates();

        foreach (Vector2Int enemyCoordinate in enemyCoordinates)
        {
            if (enemyCoordinate == coordinates)
            {
                return true;
            }
        }

        return false;
    }

    private bool IsInsideGrid(Vector2Int coordinates, int width, int height)
    {
        return coordinates.x >= 0 &&
               coordinates.y >= 0 &&
               coordinates.x < width &&
               coordinates.y < height;
    }

    private void ClearParent(Transform parent, HashSet<Transform> clearedParents)
    {
        if (parent == null)
        {
            return;
        }

        if (clearedParents.Add(parent) == false)
        {
            return;
        }

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            DestroyObject(parent.GetChild(i).gameObject);
        }
    }

    private void DestroyObject(GameObject target)
    {
        if (target == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Destroy(target);
        }
        else
        {
            DestroyImmediate(target);
        }
    }

    private void OnValidate()
    {
        _gridWidth = Mathf.Max(1, _gridWidth);
        _gridHeight = Mathf.Max(1, _gridHeight);
    }
}

public enum LevelEditorMode
{
    None,
    PlacingCharacter,
    PlacingEnemy,
    PlacingStar
}

public enum LevelEditorObjectType
{
    Character,
    Enemy
}

public class LevelEditorPlacedObject : MonoBehaviour
{
    [SerializeField] private LevelEditorObjectType _type;
    [SerializeField] private Vector2Int _coordinates;

    public LevelEditorObjectType Type => _type;
    public Vector2Int Coordinates => _coordinates;

    public void Init(LevelEditorObjectType type, Vector2Int coordinates)
    {
        _type = type;
        _coordinates = coordinates;
    }
}
