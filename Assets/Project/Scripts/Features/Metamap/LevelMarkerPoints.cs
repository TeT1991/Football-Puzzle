using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class LevelMarkerPoints : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpriteShapeController spriteShape;
    [SerializeField] private Transform pointsParent;

    [Header("Markers")]
    [SerializeField, Min(1)] private int markersCount = 5;
    [SerializeField, Min(0f)] private float startOffset = 0f;
    [SerializeField, Min(0f)] private float endOffset = 0f;
    [SerializeField, Min(0f)] private float spacing = 1f;
    [SerializeField] private string pointNamePrefix = "LevelMarkerPoint_";

    [Header("Gizmos")]
    [SerializeField] private float gizmoRadius = 0.25f;
    [SerializeField] private Color gizmoColor = Color.yellow;

    private const int SamplesPerSegment = 100;

    public IReadOnlyList<Transform> Points => GetPoints();

    private void OnValidate()
    {
#if UNITY_EDITOR
        RecalculateSpacingFromOffsets();
        SceneView.RepaintAll();
#endif
    }

    private void OnDrawGizmos()
    {
        if (pointsParent == null)
            return;

        Gizmos.color = gizmoColor;

        for (int i = 0; i < pointsParent.childCount; i++)
        {
            Gizmos.DrawSphere(pointsParent.GetChild(i).position, gizmoRadius);
        }
    }

    public List<Transform> GetPoints()
    {
        List<Transform> points = new();

        if (pointsParent == null)
            return points;

        for (int i = 0; i < pointsParent.childCount; i++)
        {
            points.Add(pointsParent.GetChild(i));
        }

        return points;
    }

#if UNITY_EDITOR
    public void RebuildPointObjects()
    {
        if (spriteShape == null || pointsParent == null)
            return;

        List<Vector3> worldPositions = BuildWorldPositions();

        EnsurePointsCount(worldPositions.Count);

        for (int i = 0; i < worldPositions.Count; i++)
        {
            Transform point = pointsParent.GetChild(i);
            point.name = $"{pointNamePrefix}{i + 1:00}";
            point.position = worldPositions[i];
        }

        EditorUtility.SetDirty(pointsParent.gameObject);
        EditorUtility.SetDirty(gameObject);
    }

    private List<Vector3> BuildWorldPositions()
    {
        List<Vector3> worldPositions = new();

        Spline spline = spriteShape.spline;
        int pointCount = spline.GetPointCount();

        if (pointCount < 2)
            return worldPositions;

        float totalLength = GetPathLength(spline);

        if (totalLength <= 0f)
            return worldPositions;

        ClampOffsets(totalLength);

        if (markersCount <= 1)
        {
            Vector3 localPoint = GetPointOnPathByDistance(spline, startOffset);
            worldPositions.Add(spriteShape.transform.TransformPoint(localPoint));
            return worldPositions;
        }

        spacing = GetAvailableLength(totalLength) / (markersCount - 1);

        for (int i = 0; i < markersCount; i++)
        {
            float distance = startOffset + spacing * i;
            Vector3 localPoint = GetPointOnPathByDistance(spline, distance);
            Vector3 worldPoint = spriteShape.transform.TransformPoint(localPoint);

            worldPositions.Add(worldPoint);
        }

        return worldPositions;
    }

    private void EnsurePointsCount(int requiredCount)
    {
        while (pointsParent.childCount < requiredCount)
        {
            GameObject point = new GameObject($"{pointNamePrefix}{pointsParent.childCount + 1:00}");
            Undo.RegisterCreatedObjectUndo(point, "Create Level Marker Point");
            point.transform.SetParent(pointsParent, false);
        }

        while (pointsParent.childCount > requiredCount)
        {
            Transform child = pointsParent.GetChild(pointsParent.childCount - 1);
            Undo.DestroyObjectImmediate(child.gameObject);
        }
    }

    private void RecalculateSpacingFromOffsets()
    {
        if (spriteShape == null)
            return;

        Spline spline = spriteShape.spline;

        if (spline.GetPointCount() < 2)
            return;

        float totalLength = GetPathLength(spline);

        if (totalLength <= 0f)
            return;

        ClampOffsets(totalLength);

        if (markersCount <= 1)
        {
            spacing = 0f;
            return;
        }

        spacing = GetAvailableLength(totalLength) / (markersCount - 1);
    }

    private void ClampOffsets(float totalLength)
    {
        startOffset = Mathf.Clamp(startOffset, 0f, totalLength);
        endOffset = Mathf.Clamp(endOffset, 0f, totalLength - startOffset);
    }

    private float GetAvailableLength(float totalLength)
    {
        return Mathf.Max(0f, totalLength - startOffset - endOffset);
    }

    private float GetPathLength(Spline spline)
    {
        float length = 0f;
        int pointCount = spline.GetPointCount();

        for (int i = 0; i < pointCount - 1; i++)
        {
            Vector3 previous = GetBezierPoint(spline, i, 0f);

            for (int s = 1; s <= SamplesPerSegment; s++)
            {
                float t = s / (float)SamplesPerSegment;
                Vector3 current = GetBezierPoint(spline, i, t);

                length += Vector3.Distance(previous, current);
                previous = current;
            }
        }

        return length;
    }

    private Vector3 GetPointOnPathByDistance(Spline spline, float targetDistance)
    {
        int pointCount = spline.GetPointCount();

        if (pointCount < 2)
            return Vector3.zero;

        float currentDistance = 0f;

        for (int i = 0; i < pointCount - 1; i++)
        {
            Vector3 previous = GetBezierPoint(spline, i, 0f);
            float previousT = 0f;

            for (int s = 1; s <= SamplesPerSegment; s++)
            {
                float currentT = s / (float)SamplesPerSegment;
                Vector3 current = GetBezierPoint(spline, i, currentT);

                float stepLength = Vector3.Distance(previous, current);

                if (stepLength <= Mathf.Epsilon)
                {
                    previous = current;
                    previousT = currentT;
                    continue;
                }

                if (currentDistance + stepLength >= targetDistance)
                {
                    float stepT = (targetDistance - currentDistance) / stepLength;
                    float bezierT = Mathf.Lerp(previousT, currentT, stepT);

                    return GetBezierPoint(spline, i, bezierT);
                }

                currentDistance += stepLength;
                previous = current;
                previousT = currentT;
            }
        }

        return spline.GetPosition(pointCount - 1);
    }

    private Vector3 GetBezierPoint(Spline spline, int pointIndex, float t)
    {
        int nextIndex = pointIndex + 1;

        Vector3 p0 = spline.GetPosition(pointIndex);
        Vector3 p1 = p0 + spline.GetRightTangent(pointIndex);

        Vector3 p3 = spline.GetPosition(nextIndex);
        Vector3 p2 = p3 + spline.GetLeftTangent(nextIndex);

        float u = 1f - t;

        return
            u * u * u * p0 +
            3f * u * u * t * p1 +
            3f * u * t * t * p2 +
            t * t * t * p3;
    }
#endif
}

#if UNITY_EDITOR
[CustomEditor(typeof(LevelMarkerPoints))]
public class LevelMarkerPointsEditor : Editor
{
    private const int SamplesPerSegment = 30;

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        SerializedProperty spriteShape = serializedObject.FindProperty("spriteShape");
        SerializedProperty pointsParent = serializedObject.FindProperty("pointsParent");
        SerializedProperty markersCount = serializedObject.FindProperty("markersCount");
        SerializedProperty startOffset = serializedObject.FindProperty("startOffset");
        SerializedProperty endOffset = serializedObject.FindProperty("endOffset");
        SerializedProperty spacing = serializedObject.FindProperty("spacing");
        SerializedProperty pointNamePrefix = serializedObject.FindProperty("pointNamePrefix");
        SerializedProperty gizmoRadius = serializedObject.FindProperty("gizmoRadius");
        SerializedProperty gizmoColor = serializedObject.FindProperty("gizmoColor");

        EditorGUI.BeginChangeCheck();

        EditorGUILayout.PropertyField(spriteShape);
        EditorGUILayout.PropertyField(pointsParent);

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(markersCount);

        float pathLength = GetPathLength(spriteShape.objectReferenceValue as SpriteShapeController);

        if (pathLength > 0f)
        {
            float startMax = Mathf.Max(0f, pathLength - endOffset.floatValue);
            startOffset.floatValue = EditorGUILayout.Slider("Start Offset", startOffset.floatValue, 0f, startMax);

            float endMax = Mathf.Max(0f, pathLength - startOffset.floatValue);
            endOffset.floatValue = EditorGUILayout.Slider("End Offset", endOffset.floatValue, 0f, endMax);

            float availableLength = Mathf.Max(0f, pathLength - startOffset.floatValue - endOffset.floatValue);

            if (markersCount.intValue > 1)
                spacing.floatValue = availableLength / (markersCount.intValue - 1);
            else
                spacing.floatValue = 0f;

            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.Slider("Spacing", spacing.floatValue, 0f, pathLength);
            EditorGUI.EndDisabledGroup();
        }
        else
        {
            spacing.floatValue = 0f;
            EditorGUILayout.HelpBox("Sprite Shape path is empty or not assigned.", MessageType.Info);
        }

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(pointNamePrefix);
        EditorGUILayout.PropertyField(gizmoRadius);
        EditorGUILayout.PropertyField(gizmoColor);

        bool changed = EditorGUI.EndChangeCheck();

        serializedObject.ApplyModifiedProperties();

        LevelMarkerPoints levelMarkerPoints = (LevelMarkerPoints)target;

        if (changed)
        {
            levelMarkerPoints.RebuildPointObjects();
            SceneView.RepaintAll();
        }

        EditorGUILayout.Space();

        if (GUILayout.Button("Rebuild Level Marker Points"))
        {
            levelMarkerPoints.RebuildPointObjects();
            SceneView.RepaintAll();
        }
    }

    private float GetPathLength(SpriteShapeController spriteShape)
    {
        if (spriteShape == null)
            return 0f;

        Spline spline = spriteShape.spline;
        int pointCount = spline.GetPointCount();

        if (pointCount < 2)
            return 0f;

        float length = 0f;

        for (int i = 0; i < pointCount - 1; i++)
        {
            Vector3 previous = GetBezierPoint(spline, i, 0f);

            for (int s = 1; s <= SamplesPerSegment; s++)
            {
                float t = s / (float)SamplesPerSegment;
                Vector3 current = GetBezierPoint(spline, i, t);

                length += Vector3.Distance(previous, current);
                previous = current;
            }
        }

        return length;
    }

    private Vector3 GetBezierPoint(Spline spline, int pointIndex, float t)
    {
        int nextIndex = pointIndex + 1;

        Vector3 p0 = spline.GetPosition(pointIndex);
        Vector3 p1 = p0 + spline.GetRightTangent(pointIndex);

        Vector3 p3 = spline.GetPosition(nextIndex);
        Vector3 p2 = p3 + spline.GetLeftTangent(nextIndex);

        float u = 1f - t;

        return
            u * u * u * p0 +
            3f * u * u * t * p1 +
            3f * u * t * t * p2 +
            t * t * t * p3;
    }
}
#endif