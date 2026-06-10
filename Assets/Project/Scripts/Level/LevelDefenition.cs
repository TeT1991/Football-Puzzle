using UnityEngine;

[CreateAssetMenu(menuName = "Football Puzzle/Level")]

public class LevelDefenition : ScriptableObject
{
    [SerializeField] private int _width;
    [SerializeField] private int _height;

    public int Width => _width;
    public int Height => _height;
}
