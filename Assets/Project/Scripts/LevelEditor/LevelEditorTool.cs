using System.Runtime.CompilerServices;
using UnityEngine;

public class LevelEditorTool : MonoBehaviour
{
    [SerializeField] private LevelDefinition _levelDefinition;
    [SerializeField] private CellView _cellViewPrefab;
    [SerializeField] private Transform _cellsParent;
    [SerializeField] private CellSelector _cellSelector;
    [SerializeField] private int _gridWidth;
    [SerializeField] private int _gridHeight;

    [SerializeField] private Transform _characterParent;
    [SerializeField] private GameObject _characterView;

    private LevelBuilder _levelBuilder;
    private CharacterCreator _characterCreator;

    private CellView[,] _cells;
    private float _gridPositionOffsetX;
    private float _gridPositionOffsetY;

    private LevelEditorMode _currentMode = LevelEditorMode.None;
    public LevelEditorMode CurrentMode => _currentMode;

    private CellView _selectedCell;

    private void Awake()
    {
        _characterCreator = new(_characterParent, _characterView);
    }

    public void CreateLevel()
    {
        ClearLevel();

        LevelData levelData = GenerateLevelData();
        Vector2Int gridSize = new(levelData.Width, levelData.Height);

        _levelBuilder = new(gridSize, _cellViewPrefab, _cellsParent);
        _levelBuilder.BuildLevel();
        _cells = new CellView[levelData.Width, levelData.Height];
        _cells = _levelBuilder.GetCells();

        float cellSize = 1;
        _gridPositionOffsetX = GameUtility.CalculateGridOffset(_levelDefinition.Width, cellSize);
        _gridPositionOffsetY = GameUtility.CalculateGridOffset(_levelDefinition.Height, cellSize);
        _cellSelector.Init((CellView[,])_cells.Clone(), _gridPositionOffsetX, _gridPositionOffsetY);
    }

    public void ClearLevel()
    {
        for (int i = _cellsParent.childCount - 1; i >= 0; i--)
        {
            Transform child = _cellsParent.GetChild(i);

            if (Application.isPlaying)
            {
                Destroy(child.gameObject);
            }
            else
            {
                DestroyImmediate(child.gameObject);
            }
        }

        _cells = null;
    }

    public void StartPlacingCharacters()
    {
        if (_cells == null)
        {
            TryRebuildCellsFromScene();
        }

        SetMode(LevelEditorMode.PlacingCharacters);
    }

    public void StopPlacingCharacters()
    {
        SetMode(LevelEditorMode.None);
        Debug.Log("Stop placing");
    }

    public void TrySelectCell(Vector2 worldPosition)
    {
        if (_cells == null)
        {
            bool rebuilt = TryRebuildCellsFromScene();

            if (rebuilt == false)
            {
                Debug.LogWarning("No cells found. Create level first.");
                return;
            }
        }

        if (_cellSelector.TryGetCell(worldPosition, out CellView cell))
        {
            Debug.Log(cell.name);
            _selectedCell = cell;
        }
        else
        {
            Debug.Log("Мимо");
        }
    }

    public void PlaceCharacter(Vector2Int position)
    {
        _characterCreator.CreateCharacter(position);
    }

    public void SetMode(LevelEditorMode mode)
    {
        _currentMode = mode;

    }

    public void ResetMode()
    {
        SetMode(LevelEditorMode.None);
    }

    public bool TryRebuildCellsFromScene()
    {
        LevelData levelData = GenerateLevelData();

        _cells = new CellView[levelData.Width, levelData.Height];

        CellView[] sceneCells = _cellsParent.GetComponentsInChildren<CellView>();

        foreach (CellView cell in sceneCells)
        {
            Vector2Int coordinates = cell.Coordinates;

            if (coordinates.x < 0 ||
                coordinates.y < 0 ||
                coordinates.x >= levelData.Width ||
                coordinates.y >= levelData.Height)
            {
                Debug.LogWarning($"Cell {cell.name} has invalid coordinates {coordinates}");
                continue;
            }

            _cells[coordinates.x, coordinates.y] = cell;
        }

        float cellSize = 1f;

        _gridPositionOffsetX = GameUtility.CalculateGridOffset(levelData.Width, cellSize);
        _gridPositionOffsetY = GameUtility.CalculateGridOffset(levelData.Height, cellSize);

        _cellSelector.Init(_cells, _gridPositionOffsetX, _gridPositionOffsetY);

        return sceneCells.Length > 0;
    }

    public LevelData GenerateLevelData()
    {
        LevelData levelData;

        if (_levelDefinition == null)
        {
            Debug.LogWarning("Level Definition not found. Creating new level data");
            levelData = new(_gridWidth, _gridHeight);
        }
        else
        {
            Debug.LogWarning("Loading data from LevelDefinition");
            levelData = new(_levelDefinition.Width, _levelDefinition.Height);
        }

        return levelData;
    }

    public void SetLevelDefinition(LevelDefinition levelDefinition)
    {
        _levelDefinition = levelDefinition;
    }
}

public enum LevelEditorMode
{
    None,
    PlacingCharacters
}

public class CharacterCreator
{
    private Transform _parent;
    private GameObject _character;

    public CharacterCreator(Transform parent, GameObject character)
    {
        _parent = parent;
        _character = character;
    }

    public GameObject CreateCharacter(Vector2Int position)
    {
        if (_character != null)
        {
            MonoBehaviour.Destroy(_character.gameObject);
            _character = null;
        }

        MonoBehaviour.Instantiate(_character);
        _character.transform.SetParent(_parent);
        _character.transform.position = (Vector2)position;

        return _character;
    }
}
