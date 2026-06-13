using System.Runtime.CompilerServices;
using UnityEngine;

public class GameUtility
{
    public static float CalculateGridOffset(int width)
    {
        float cellSize = 1;
        return (width - 1) * cellSize / 2f;
    }

    public static Vector2Int ConvertPositionToCoordinates(Vector2 position, float gridPositionOffsestX, float gridPositionOffsestY)
    {
        float offsetedX = position.x + gridPositionOffsestX;
        float offsetedY = position.y + gridPositionOffsestY;
        Vector2 offsetedPosition = new(offsetedX, offsetedY);

        return Vector2Int.RoundToInt(offsetedPosition);
    }
}
