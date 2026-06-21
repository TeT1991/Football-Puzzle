using UnityEditor;
using UnityEngine;

internal sealed class LevelEditorSceneQuery
{
    private const float CellHalfSize = 0.5f;

    private readonly LevelEditorTool _tool;

    public LevelEditorSceneQuery(LevelEditorTool tool)
    {
        _tool = tool;
    }

    public bool TryGetCellAtGuiPoint(Vector2 guiPoint, out CellView cell)
    {
        cell = null;

        return TryGetWorldPosition(guiPoint, out Vector3 worldPosition) &&
               TryGetCellAtWorldPosition(worldPosition, out cell);
    }

    public bool TryGetEnemyAtGuiPoint(
        Vector2 guiPoint,
        out LevelEditorPlacedObject enemy)
    {
        enemy = null;
        GameObject pickedObject = HandleUtility.PickGameObject(guiPoint, false);

        if (pickedObject == null)
        {
            return false;
        }

        enemy = pickedObject.GetComponentInParent<LevelEditorPlacedObject>();

        return enemy != null && enemy.Type == LevelEditorObjectType.Enemy;
    }

    public bool TryGetCellByCoordinates(
        Vector2Int coordinates,
        out CellView cell)
    {
        foreach (CellView candidate in GetSceneCells())
        {
            if (candidate != null && candidate.Coordinates == coordinates)
            {
                cell = candidate;
                return true;
            }
        }

        cell = null;
        return false;
    }

    public CellView[] GetSceneCells()
    {
        Transform cellsParent = GetTransformReference("_cellsParent");

        return cellsParent != null
            ? cellsParent.GetComponentsInChildren<CellView>(true)
            : System.Array.Empty<CellView>();
    }

    public LevelEditorPlacedObject[] GetSceneEnemies()
    {
        Transform enemiesParent = GetTransformReference("_enemiesParent");

        return enemiesParent != null
            ? enemiesParent.GetComponentsInChildren<LevelEditorPlacedObject>(true)
            : System.Array.Empty<LevelEditorPlacedObject>();
    }

    private bool TryGetWorldPosition(Vector2 guiPoint, out Vector3 worldPosition)
    {
        Ray ray = HandleUtility.GUIPointToWorldRay(guiPoint);
        Plane plane = new(Vector3.forward, new Vector3(0f, 0f, _tool.GridPlaneZ));

        if (plane.Raycast(ray, out float distance))
        {
            worldPosition = ray.GetPoint(distance);
            return true;
        }

        worldPosition = default;
        return false;
    }

    private bool TryGetCellAtWorldPosition(
        Vector3 worldPosition,
        out CellView cell)
    {
        cell = null;
        float closestDistance = float.MaxValue;

        foreach (CellView candidate in GetSceneCells())
        {
            if (candidate == null)
            {
                continue;
            }

            Vector3 localPosition =
                candidate.transform.InverseTransformPoint(worldPosition);

            if (Mathf.Abs(localPosition.x) > CellHalfSize ||
                Mathf.Abs(localPosition.y) > CellHalfSize)
            {
                continue;
            }

            float distance =
                (candidate.transform.position - worldPosition).sqrMagnitude;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                cell = candidate;
            }
        }

        return cell != null;
    }

    private Transform GetTransformReference(string propertyName)
    {
        SerializedObject serializedTool = new(_tool);
        SerializedProperty property = serializedTool.FindProperty(propertyName);
        return property?.objectReferenceValue as Transform;
    }
}
