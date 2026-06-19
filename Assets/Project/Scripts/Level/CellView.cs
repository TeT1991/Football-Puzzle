using UnityEngine;

public class CellView : MonoBehaviour
{
    [SerializeField] private Vector2Int _coordinates;

    public Vector2Int Coordinates => _coordinates;

    public void Init(Vector2Int coordinates)
    {
        _coordinates = coordinates;
        gameObject.name = $"Cell {coordinates.x}:{coordinates.y}";
    }
}
