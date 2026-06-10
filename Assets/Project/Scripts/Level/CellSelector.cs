using UnityEngine;

public class CellSelector : MonoBehaviour
{
    private CellView[,] _cells;
    private float _gridPositionOffsestX;
    private float _gridPositionOffsestY;
    private Camera _camera;

    public void Init(CellView[,] cells, float gridPositionOffsestX, float gridPositionOffsestY)
    {
        _cells = cells;
        _gridPositionOffsestX = gridPositionOffsestX;
        _gridPositionOffsestY = gridPositionOffsestY;
        _camera = Camera.main;
    }

    public void Update()
    {
        if (_camera != null)
        {
            if (TryGetCell(ConvertMousePositionToCoordinates(), out CellView cell))
            {
                Debug.Log(cell.name);
            }
        }
    }

    private Vector2Int ConvertMousePositionToCoordinates()
    {

        Vector2 mouseWorldPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
        float offsetedX = mouseWorldPosition.x + _gridPositionOffsestX;
        float offsetedY = mouseWorldPosition.y + _gridPositionOffsestY;
        Vector2 offsetedPosition = new(offsetedX, offsetedY);

        return Vector2Int.RoundToInt(offsetedPosition);
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
