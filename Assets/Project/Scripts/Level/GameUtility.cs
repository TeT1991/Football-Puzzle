using UnityEngine;

public class GameUtility
{
    private static float CalculateGridOffset(int width)
    {
        float cellSize = 1;
        return (width - 1) * cellSize / 2f;
    }

    public static Vector2Int ConvertPositionToCoordinates(Vector2 position, int gridWidth, int gridHeight)
    {
        float offsetedX = position.x + CalculateGridOffset(gridWidth);
        float offsetedY = position.y + CalculateGridOffset(gridHeight);
        Vector2 offsetedPosition = new(offsetedX, offsetedY);

        return Vector2Int.RoundToInt(offsetedPosition);
    }

    public static Vector2 ConvertCoordinatesToPosition(Vector2Int coordinates, int gridWidth, int gridHeight)
    {
        float offsetedX = coordinates.x - CalculateGridOffset(gridWidth);
        float offsetedY = coordinates.y - CalculateGridOffset(gridHeight);

        return new Vector2(offsetedX, offsetedY);
    }
}
