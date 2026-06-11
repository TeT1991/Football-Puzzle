using UnityEngine;

public class LevelEditorTool : MonoBehaviour
{
    [SerializeField] private string _path = "Assets/Project/Levels/NewLevelDefinition.asset";

    [SerializeField] private LevelDefinition _levelDefinition;
    [SerializeField] private CellView _cellViewPrefab;
    [SerializeField] private Transform _cellsParent;
    [SerializeField] private int _gridWidth;
    [SerializeField] private int _gridHeight;

    [SerializeField] private Transform _characterParent;
    [SerializeField] private GameObject _characterView;

    private LevelBuilder _levelBuilder;
    private CharacterCreator _characterCreator;
    private CellView[,] _cells;
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
        SetMode(LevelEditorMode.PlacingCharacters);
    }

    public void StopPlacingCharacters()
    {
        SetMode(LevelEditorMode.None);
    }

    public void SetMode(LevelEditorMode mode)
    {
        _currentMode = mode;

    }

    public void ResetMode()
    {
        SetMode(LevelEditorMode.None);
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
        MonoBehaviour.Instantiate(_character);
        _character.transform.SetParent(_parent);
        _character.transform.position = (Vector2)position;

        return _character;
    }
}
