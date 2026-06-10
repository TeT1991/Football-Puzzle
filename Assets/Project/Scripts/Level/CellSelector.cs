using System.Runtime.CompilerServices;
using UnityEngine;

public class CellSelector : MonoBehaviour
{
    private CellView[,] _cells;
    private float _gridPositionOffsestX;
    private float _gridPositionOffsestY;
    private Camera _camera;
    private GridCoordinatesConverter _coordinatesConverter;
    private Vector2 _mouseWorldPosition;

    private void Awake()
    {
        _camera = Camera.main;
    }

    public void Init(CellView[,] cells, float gridPositionOffsestX, float gridPositionOffsestY)
    {
        _cells = cells;
        _gridPositionOffsestX = gridPositionOffsestX;
        _gridPositionOffsestY = gridPositionOffsestY;
        _coordinatesConverter = new();
    }

    public void Update()
    {
        _mouseWorldPosition = _camera.ScreenToWorldPoint(Input.mousePosition);

        if (TryGetCell(_coordinatesConverter.ConvertMousePositionToCoordinates(_mouseWorldPosition, _gridPositionOffsestX, _gridPositionOffsestY),
            out CellView cell))
        {
            Debug.Log(cell.name);
        }
    }

    public bool TryGetCell(Vector2Int coodinates, out CellView cell)
    {
        int x = coodinates.x;
        int y = coodinates.y;
        int xSize = _cells.GetLength(0);
        int ySize = _cells.GetLength(1);

        if (x < 0 || y < 0 || x >= xSize || y >= ySize)
        {
            cell = null;
            return false;
        }

        cell = _cells[x, y];
        return true;
    }
}

public class GridCoordinatesConverter
{
    public Vector2Int ConvertMousePositionToCoordinates(Vector2 mouseWorldPosition, float gridPositionOffsestX, float gridPositionOffsestY)
    {
        float offsetedX = mouseWorldPosition.x + gridPositionOffsestX;
        float offsetedY = mouseWorldPosition.y + gridPositionOffsestY;
        Vector2 offsetedPosition = new(offsetedX, offsetedY);

        return Vector2Int.RoundToInt(offsetedPosition);
    }
}
