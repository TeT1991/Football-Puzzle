using System;
using UnityEngine;

public class CellSelector : MonoBehaviour
{
    private CellView[,] _cells;
    private Camera _camera;
    private GameInput _gameInput;

    private Vector3 _mouseWorldPosition;
    private bool _isSelecting = false;
    private CellView _currentCell; //Потом надо передавать дату а не вью

    public event Action<CellView> CellSelected;

    public CellView CurrentCell => _currentCell;

    private void Awake()
    {
        _camera = Camera.main;
    }

    public void Init(CellView[,] cells, GameInput gameInput)
    {
        _cells = cells;
        _gameInput = gameInput;

        _gameInput.Released += OnInputReleased;
    }

    public bool TryGetCell(Vector2 position, out CellView cell)
    {
        int gridWidth = _cells.GetLength(0);
        int gridHeight = _cells.GetLength(1);

        Vector2Int coordinates = GameUtility.ConvertPositionToCoordinates(position, gridWidth, gridHeight);

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
        _currentCell = null;
        _isSelecting = true;
    }

    public void StopSelecting()
    {
        _isSelecting = false;
    }

    public void ClearCurrentCell()
    {
        _currentCell = null;
    }

    private void OnInputReleased(Vector2 screenPosition)
    {
        if (_isSelecting == false)
        {
            return;
        }

        _mouseWorldPosition = _camera.ScreenToWorldPoint(Input.mousePosition);

        if (TryGetCell(_mouseWorldPosition,
            out CellView cell))
        {
            _currentCell = cell;
            CellSelected?.Invoke(cell);
            StopSelecting();
        }
    }

    private void OnCellSelected()
    {
        CellSelected?.Invoke(_currentCell);
    }
}