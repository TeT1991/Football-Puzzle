using UnityEngine;

[CreateAssetMenu(menuName = "Football Puzzle/Level")]

public class LevelDefinition : ScriptableObject
{
    [SerializeField] private int _width;
    [SerializeField] private int _height;

    public int Width => _width;
    public int Height => _height;

    public void UpdateData(LevelData levelData)
    {
        _width = levelData.Width;
        _height = levelData.Height;
    }
}
