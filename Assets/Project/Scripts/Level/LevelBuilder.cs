using UnityEngine;

public class LevelBuilder 
{
    private Transform _cellsParent;
    private Vector2Int _size;
    private CellView _cellViewPrefab;

    private int _width;
    private int _height;
    private float _gridPositionOffseX;
    private float _gridPositionOffseY;

    private CellView[,] _cells;

    public LevelBuilder(Vector2Int size, CellView cellViewPrefab, Transform cellsParent)
    {
        _size = size;
        _cellViewPrefab = cellViewPrefab;

        float cellSize = 1f;

        _width = size.x;
        _height = size.y;
        _cells = new CellView[_width, _height];
        _gridPositionOffseX = (_width - 1) * cellSize / 2f;
        _gridPositionOffseY = (_height - 1) * cellSize / 2f;

        _cellsParent = cellsParent; 
    }

    public float GridPositionOffseX => _gridPositionOffseX;
    public float GridPositionOffseY => _gridPositionOffseY;

    public void BuildLevel()
    {
        CreateTiles();
    }

    private void CreateTiles()
    {
        float cellSize = 1f;

        for (int y = 0; y < _height; y++)
        {
            for (int x = 0; x < _width; x++)
            {
                float xPosition = x * cellSize - _gridPositionOffseX;
                float yPosition = y * cellSize - _gridPositionOffseY;
                Vector2 position = new(xPosition, yPosition);
                Vector2Int coordinates = new(x, y);

                CellView cell = MonoBehaviour.Instantiate(_cellViewPrefab, _cellsParent);
                cell.transform.position = position;
                cell.Init(coordinates);
                _cells[x,y] = cell;
            }
        }
    }

    public CellView[,] GetCells()
    {
        return (CellView[,])_cells.Clone(); ;
    }
}
