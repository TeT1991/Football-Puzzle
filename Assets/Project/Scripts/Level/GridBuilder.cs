using UnityEngine;

public class GridBuilder
{
    private readonly Transform _cellsParent;
    private readonly CellView _cellViewPrefab;

    public GridBuilder(Transform cellsParent, CellView cellView)
    {
        _cellsParent = cellsParent;
        _cellViewPrefab = cellView;
    }

    public CellView[,] CreateTiles(int width, int height)
    {
        CellView[,] cells = new CellView[width, height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float xPosition = x - GameUtility.CalculateGridOffset(width);
                float yPosition = y - GameUtility.CalculateGridOffset(height);
                Vector2 position = new(xPosition, yPosition);
                Vector2Int coordinates = new(x, y);

                CellView cell = MonoBehaviour.Instantiate(_cellViewPrefab, _cellsParent);
                cell.transform.localPosition = position;
                cell.Init(coordinates);
                cells[x, y] = cell;
            }
        }

        return (CellView[,])cells.Clone(); ;
    }
}
