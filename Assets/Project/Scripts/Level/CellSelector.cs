using System.Runtime.CompilerServices;
using UnityEngine;

public class CellSelector : MonoBehaviour
{
    private CellView[,] _cells;
    private float _gridPositionOffsestX;
    private float _gridPositionOffsestY;
    private Camera _camera;

    private void Awake()
    {
        _camera = Camera.main;
    }

    public void Init(CellView[,] cells, float gridPositionOffsestX, float gridPositionOffsestY)
    {
        _cells = cells;
        _gridPositionOffsestX = gridPositionOffsestX;
        _gridPositionOffsestY = gridPositionOffsestY;
    }

    public void Update()
    {
        //_mouseWorldPosition = _camera.ScreenToWorldPoint(Input.mousePosition);

        //if (TryGetCell(GameUtility.ConvertMousePositionToCoordinates(_mouseWorldPosition, _gridPositionOffsestX, _gridPositionOffsestY),
        //    out CellView cell))
        //{
        //    Debug.Log(cell.name);
        //}
    }

    public bool TryGetCell(Vector2 position, out CellView cell)
    {
        Vector2Int coordinates = GameUtility.ConvertPositionToCoordinates(position, _gridPositionOffsestX, _gridPositionOffsestY);

        int x = coordinates.x;
        int y = coordinates.y;
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