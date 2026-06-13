using UnityEngine;

public class LevelData
{
    private readonly int _width;
    private readonly int _height;
    private Vector2Int _characterStartPosition;

    public LevelData(int width, int height, Vector2Int characterStartPosition)
    {
        _width = width;
        _height = height;
        _characterStartPosition = characterStartPosition;
    }

    public int Width => _width;
    public int Height => _height;
    public Vector2Int CharacterStartPosition => _characterStartPosition;    
}
