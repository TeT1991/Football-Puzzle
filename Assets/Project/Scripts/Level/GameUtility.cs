using System.Runtime.CompilerServices;
using UnityEngine;

public class GameUtility
{
    public static float CalculateGridOffset(int width, float cellSize)
    {
        return (width - 1) * cellSize / 2f;
    }

    public static Vector2Int ConvertPositionToCoordinates(Vector2 mouseWorldPosition, float gridPositionOffsestX, float gridPositionOffsestY)
    {
        float offsetedX = mouseWorldPosition.x + gridPositionOffsestX;
        float offsetedY = mouseWorldPosition.y + gridPositionOffsestY;
        Vector2 offsetedPosition = new(offsetedX, offsetedY);

        return Vector2Int.RoundToInt(offsetedPosition);
    }
}
