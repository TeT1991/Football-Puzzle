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
    [SerializeField] private MetamapLocationView metamapLocationView;

    [Header("Markers")]
    [SerializeField, Min(1)] private int markersCount = 5;

    [SerializeField, Min(0f)] private float startOffset = 0f;
    [SerializeField, Min(0f)] private float endOffset = 0f;
    [SerializeField, Min(0.01f)] private float spacing = 1f;

    [Header("Gizmos")]
    [SerializeField] private float gizmoRadius = 0.25f;
    [SerializeField] private Color gizmoColor = Color.yellow;

    private const int SamplesPerSegment = 30;

    private readonly List<Vector3> markerWorldPositions = new();

    private void OnValidate()
    {
#if UNITY_EDITOR
        RecalculateSpacingFromOffsets();
        SceneView.RepaintAll();
#endif
    }

    private void OnDrawGizmos()
    {
#if UNITY_EDITOR
        DrawMarkerGizmos();
#endif
    }

#if UNITY_EDITOR
    public void SendPointsToMetamapLocationView()
    {
        RebuildMarkerPositions();

        if (metamapLocationView == null)
            return;

        // Тут вставь свой метод приема координат.
        // Например:
        // metamapLocationView.SetLevelMarkerPoints(markerWorldPositions);
    }

    private void DrawMarkerGizmos()
    {
        RebuildMarkerPositions();

        Gizmos.color = gizmoColor;

        foreach (Vector3 position in markerWorldPositions)
        {
            Gizmos.DrawSphere(position, gizmoRadius);
        }
    }

    private void RebuildMarkerPositions()
    {
        markerWorldPositions.Clear();

        if (spriteShape == null)
            return;

        Spline spline = spriteShape.spline;
        int pointCount = spline.GetPointCount();

        if (pointCount < 2)
            return;

        float totalLength = GetPathLength(spline);

        if (totalLength <= 0f)
            return;

        ClampOffsets(totalLength);

        if (markersCount <= 1)
        {
            Vector3 singlePoint = GetPointOnPathByDistance(spline, startOffset);
            markerWorldPositions.Add(spriteShape.transform.TransformPoint(singlePoint));
            return;
        }

        spacing = GetAvailableLength(totalLength) / (markersCount - 1);

        for (int i = 0; i < markersCount; i++)
        {
            float distance = startOffset + spacing * i;
            Vector3 localPosition = GetPointOnPathByDistance(spline, distance);
            Vector3 worldPosition = spriteShape.transform.TransformPoint(localPosition);

            markerWorldPositions.Add(worldPosition);
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

            for (int s = 1; s <= SamplesPerSegment; s++)
            {
                float t = s / (float)SamplesPerSegment;
                Vector3 current = GetBezierPoint(spline, i, t);

                float stepLength = Vector3.Distance(previous, current);

                if (currentDistance + stepLength >= targetDistance)
                {
                    float stepT = (targetDistance - currentDistance) / stepLength;
                    return Vector3.Lerp(previous, current, stepT);
                }

                currentDistance += stepLength;
                previous = current;
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
        SerializedProperty metamapLocationView = serializedObject.FindProperty("metamapLocationView");
        SerializedProperty markersCount = serializedObject.FindProperty("markersCount");
        SerializedProperty startOffset = serializedObject.FindProperty("startOffset");
        SerializedProperty endOffset = serializedObject.FindProperty("endOffset");
        SerializedProperty spacing = serializedObject.FindProperty("spacing");
        SerializedProperty gizmoRadius = serializedObject.FindProperty("gizmoRadius");
        SerializedProperty gizmoColor = serializedObject.FindProperty("gizmoColor");

        EditorGUILayout.PropertyField(spriteShape);
        EditorGUILayout.PropertyField(metamapLocationView);

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
            EditorGUILayout.HelpBox("Sprite Shape path is empty or not assigned.", MessageType.Info);
        }

        EditorGUILayout.Space();

        EditorGUILayout.PropertyField(gizmoRadius);
        EditorGUILayout.PropertyField(gizmoColor);

        EditorGUILayout.Space();

        if (GUILayout.Button("Send Points To MetamapLocationView"))
        {
            LevelMarkerPoints levelMarkerPoints = (LevelMarkerPoints)target;
            levelMarkerPoints.SendPointsToMetamapLocationView();
        }

        serializedObject.ApplyModifiedProperties();

        if (GUI.changed)
            SceneView.RepaintAll();
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