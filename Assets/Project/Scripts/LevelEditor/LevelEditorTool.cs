using UnityEditor;
using UnityEngine;

public class LevelEditorTool : MonoBehaviour
{
    [SerializeField] private string _path = "Assets/Project/Levels/NewLevelDefinition.asset";

    [SerializeField] private LevelDefinition _levelDefinition;
    [SerializeField] private CellView _cellViewPrefab;
    [SerializeField] private Transform _cellsParent;
    [SerializeField] private int _gridWidth;
    [SerializeField] private int _gridHeight;

    private LevelBuilder _levelBuilder;
    private CellView[,] _cells;
    private LevelEditorMode _currentMode = LevelEditorMode.None;

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
        _currentMode = LevelEditorMode.PlacingCharacters;
    }

    public void StopPlacingCharacters()
    {
        _currentMode = LevelEditorMode.None;
    }

    private LevelData GenerateLevelData()
    {
        LevelData levelData;

        if (_levelDefinition == null)
        {
            Debug.LogWarning("Level Definition not found. Creating new level data");
            levelData = new(_gridWidth, _gridHeight);
            CreateLevelDefinitonAsset(levelData);
        }
        else
        {
            Debug.LogWarning("Loading data from LevelDefinition");
            levelData = new(_levelDefinition.Width, _levelDefinition.Height);
        }

        return levelData;
    }

    private void CreateLevelDefinitonAsset(LevelData levelData)
    {
        LevelDefinition levelDefinition = ScriptableObject.CreateInstance<LevelDefinition>();
        levelDefinition.UpdateData(levelData);
        AssetDatabase.GenerateUniqueAssetPath(_path);
        AssetDatabase.CreateAsset(levelDefinition, _path);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}

public enum LevelEditorMode
{
    None,
    PlacingCharacters
}
