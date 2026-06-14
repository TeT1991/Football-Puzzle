using System;
using UnityEngine;

public class CellSelector : MonoBehaviour
{
    private CellView[,] _cells;
    private float _gridPositionOffsestX;
    private float _gridPositionOffsestY;
    private Camera _camera;

    private Vector3 _mouseWorldPosition;
    private bool _isSelecting = false;
    private CellView _selectedCell; //Потом надо передавать дату а не вью

    public Action CellSelected;

    private void Awake()
    {
        _camera = Camera.main;
    }

    public void Init(CellView[,] cells)
    {
        _cells = cells;
        _gridPositionOffsestX = GameUtility.CalculateGridOffset(_cells.GetLength(0));
        _gridPositionOffsestY = GameUtility.CalculateGridOffset(_cells.GetLength(1));
    }

    public void Update()
    {
        if (_isSelecting)
        {
            _mouseWorldPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
            // подсветить клетки прри наведении

            if (Input.GetMouseButtonUp(0))
            {
                if (TryGetCell(_mouseWorldPosition,
                    out CellView cell))
                {
                    _selectedCell = cell;
                    // StopSelecting();
                    Debug.Log(cell.Coordinates);
                }
            }

        }
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

    public void StartSelecting()
    {
        _selectedCell = null;
        _isSelecting = true;
    }

    public void StopSelecting()
    {
        _isSelecting = false;
    }

    private void OnCellSelected()
    {
        CellSelected?.Invoke();
    }
}