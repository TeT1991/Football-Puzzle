using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

[CustomEditor(typeof(LevelEditorTool))]
public class LevelEditorToolEditor : Editor
{
    private const float CellHalfSize = 0.5f;
    private const float PlayerPathLineWidth = 5f;

    private static readonly Color PlayerPathColor = new(0.05f, 0.45f, 1f, 0.5f);

    private LevelEditorTool _tool;
    private LevelEditorAssetUtility _assetUtility;

    private bool _isDrawingPlayerPath;
    private bool _showPlayerPaths = true;
    private bool _isPathDragging;
    private int _pathDragButton = -1;
    private int _pathUndoGroup = -1;
    private CellView _lastPathCell;

    private void OnEnable()
    {
        _tool = (LevelEditorTool)target;
        _assetUtility = new LevelEditorAssetUtility();

        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        EndPathDrag();
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        DrawDefaultInspector();
        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space(12);
        DrawModeInfo();
        EditorGUILayout.Space(6);

        if (GUILayout.Button("Create / Rebuild Level"))
        {
            StopPlayerPathDrawing();
            _tool.CreateLevel();
            MarkToolDirty();
        }

        if (GUILayout.Button("Clear Scene Level"))
        {
            StopPlayerPathDrawing();
            _tool.ClearLevel();
            MarkToolDirty();
        }

        EditorGUILayout.Space(8);

        if (GUILayout.Button("Place EntityView"))
        {
            StopPlayerPathDrawing();
            _tool.StartPlacingCharacter();
            SceneView.RepaintAll();
            MarkToolDirty();
        }

        if (GUILayout.Button("Place Enemy"))
        {
            StopPlayerPathDrawing();
            _tool.StartPlacingEnemy();
            SceneView.RepaintAll();
            MarkToolDirty();
        }

        if (GUILayout.Button("Stop Placement"))
        {
            _tool.StopPlacement();
            SceneView.RepaintAll();
            MarkToolDirty();
        }

        EditorGUILayout.Space(8);
        DrawPlayerPathControls();
        EditorGUILayout.Space(8);

        if (GUILayout.Button("Save"))
        {
            SaveCurrent();
        }

        if (GUILayout.Button("Save As New"))
        {
            SaveAsNew();
        }
    }

    private void DrawModeInfo()
    {
        if (_isDrawingPlayerPath)
        {
            EditorGUILayout.HelpBox(
                "Mode: Draw Player Path. LMB drag adds links, RMB drag removes links, Esc stops drawing.",
                MessageType.Info);
            return;
        }

        switch (_tool.CurrentMode)
        {
            case LevelEditorMode.None:
                EditorGUILayout.HelpBox("Mode: None", MessageType.Info);
                break;

            case LevelEditorMode.PlacingCharacter:
                EditorGUILayout.HelpBox(
                    "Mode: Place EntityView. Click on a cell in Scene View. Esc - cancel.",
                    MessageType.Info);
                break;

            case LevelEditorMode.PlacingEnemy:
                EditorGUILayout.HelpBox(
                    "Mode: Place Enemy. Click on a free cell in Scene View. Esc - cancel.",
                    MessageType.Info);
                break;
        }
    }

    private void DrawPlayerPathControls()
    {
        EditorGUILayout.LabelField("Player Path", EditorStyles.boldLabel);

        bool showPlayerPaths = EditorGUILayout.Toggle("Show Player Paths", _showPlayerPaths);

        if (showPlayerPaths != _showPlayerPaths)
        {
            _showPlayerPaths = showPlayerPaths;
            SceneView.RepaintAll();
        }

        using (new EditorGUI.DisabledScope(_isDrawingPlayerPath))
        {
            if (GUILayout.Button("Draw Player Path"))
            {
                StartPlayerPathDrawing();
            }
        }

        using (new EditorGUI.DisabledScope(_isDrawingPlayerPath == false))
        {
            if (GUILayout.Button("Stop Draw Player Path"))
            {
                StopPlayerPathDrawing();
            }
        }
    }

    private void StartPlayerPathDrawing()
    {
        _tool.StopPlacement();
        _tool.ApplySceneToDefinition();

        LevelDefinition definition = _tool.LevelDefinition;

        if (definition == null)
        {
            Debug.LogWarning("Assign or create a LevelDefinition before drawing a player path.");
            return;
        }

        if (definition.HasCharacter == false)
        {
            Debug.LogWarning("Place the player before drawing a player path.");
            return;
        }

        if (TryGetCellByCoordinates(definition.CharacterPosition, out _) == false)
        {
            _tool.CreateLevel();

            if (TryGetCellByCoordinates(definition.CharacterPosition, out _) == false)
            {
                Debug.LogWarning("The player start cell is not available in the scene grid.");
                return;
            }
        }

        PlayerPathEditorUtility.EnsureNode(definition, definition.CharacterPosition);

        _isDrawingPlayerPath = true;
        _showPlayerPaths = true;
        ResetPathDrag();

        SceneView.RepaintAll();
        Repaint();
    }

    private void StopPlayerPathDrawing()
    {
        if (_isDrawingPlayerPath == false && _isPathDragging == false)
        {
            return;
        }

        EndPathDrag();
        _isDrawingPlayerPath = false;

        SceneView.RepaintAll();
        Repaint();
    }

    private void SaveCurrent()
    {
        _tool.ApplySceneToDefinition();

        LevelDefinition savedLevelDefinition = _assetUtility.Save(_tool.LevelDefinition);

        if (savedLevelDefinition != null)
        {
            _tool.SetLevelDefinition(savedLevelDefinition);
        }

        MarkToolDirty();
    }

    private void SaveAsNew()
    {
        _tool.ApplySceneToDefinition();

        LevelDefinition sourceDefinition = _tool.LevelDefinition;
        LevelDefinition newLevelDefinition = _assetUtility.SaveAsNew(sourceDefinition);

        if (newLevelDefinition != null)
        {
            PlayerPathEditorUtility.CopyPlayerRoute(sourceDefinition, newLevelDefinition);
            _assetUtility.Save(newLevelDefinition);
            _tool.SetLevelDefinition(newLevelDefinition);
        }

        MarkToolDirty();
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (_tool == null)
        {
            return;
        }

        DrawPlayerPaths();

        Event currentEvent = Event.current;

        if (_isDrawingPlayerPath)
        {
            HandlePlayerPathInput(currentEvent);
            return;
        }

        if (_tool.CurrentMode == LevelEditorMode.None)
        {
            return;
        }

        if (currentEvent.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }

        if (currentEvent.type == EventType.KeyDown && currentEvent.keyCode == KeyCode.Escape)
        {
            _tool.StopPlacement();
            SceneView.RepaintAll();
            MarkToolDirty();

            currentEvent.Use();
            return;
        }

        if (currentEvent.type != EventType.MouseDown || currentEvent.button != 0)
        {
            return;
        }

        if (TryGetWorldPosition(currentEvent.mousePosition, out Vector3 worldPosition) &&
            _tool.HandleSceneClick(worldPosition))
        {
            MarkToolDirty();
            SceneView.RepaintAll();
        }

        currentEvent.Use();
    }

    private void HandlePlayerPathInput(Event currentEvent)
    {
        if (currentEvent.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            return;
        }

        if (currentEvent.type == EventType.KeyDown && currentEvent.keyCode == KeyCode.Escape)
        {
            StopPlayerPathDrawing();
            currentEvent.Use();
            return;
        }

        if (currentEvent.alt)
        {
            return;
        }

        if (currentEvent.type == EventType.MouseDown &&
            (currentEvent.button == 0 || currentEvent.button == 1))
        {
            if (TryGetCellAtGuiPoint(currentEvent.mousePosition, out CellView cell) &&
                CanBeginPathDrag(cell, currentEvent.button))
            {
                BeginPathDrag(cell, currentEvent.button);
                currentEvent.Use();
            }

            return;
        }

        if (currentEvent.type == EventType.MouseDrag && _isPathDragging)
        {
            if (TryGetCellAtGuiPoint(currentEvent.mousePosition, out CellView cell))
            {
                HandlePathDrag(cell);
            }

            currentEvent.Use();
            return;
        }

        if (currentEvent.type == EventType.MouseUp && _isPathDragging)
        {
            EndPathDrag();
            currentEvent.Use();
        }
    }

    private bool CanBeginPathDrag(CellView cell, int mouseButton)
    {
        LevelDefinition definition = _tool.LevelDefinition;

        if (definition == null || cell == null)
        {
            return false;
        }

        bool containsNode = PlayerPathEditorUtility.ContainsNode(
            definition,
            cell.Coordinates);

        if (mouseButton == 1)
        {
            return containsNode;
        }

        if (containsNode)
        {
            return true;
        }

        Debug.LogWarning("Start a new player path segment from the player start or an existing path node.");
        return false;
    }

    private void BeginPathDrag(CellView cell, int mouseButton)
    {
        Undo.IncrementCurrentGroup();
        _pathUndoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName(mouseButton == 0 ? "Draw Player Path" : "Erase Player Path");

        _isPathDragging = true;
        _pathDragButton = mouseButton;
        _lastPathCell = cell;
    }

    private void HandlePathDrag(CellView cell)
    {
        if (cell == null || _lastPathCell == null || cell == _lastPathCell)
        {
            return;
        }

        Vector2Int previousCoordinates = _lastPathCell.Coordinates;
        Vector2Int currentCoordinates = cell.Coordinates;
        Vector2Int difference = currentCoordinates - previousCoordinates;

        if (Mathf.Abs(difference.x) + Mathf.Abs(difference.y) != 1)
        {
            return;
        }

        LevelDefinition definition = _tool.LevelDefinition;
        bool changed;

        if (_pathDragButton == 0)
        {
            bool alreadyConnected = PlayerPathEditorUtility.HasConnection(
                definition,
                previousCoordinates,
                currentCoordinates);

            changed = PlayerPathEditorUtility.AddBidirectionalConnection(
                definition,
                previousCoordinates,
                currentCoordinates);

            if (changed || alreadyConnected)
            {
                _lastPathCell = cell;
            }
        }
        else
        {
            changed = PlayerPathEditorUtility.RemoveBidirectionalConnection(
                definition,
                previousCoordinates,
                currentCoordinates);

            if (changed)
            {
                _lastPathCell = cell;
            }
        }

        if (changed)
        {
            SceneView.RepaintAll();
            Repaint();
        }
    }

    private void EndPathDrag()
    {
        if (_pathUndoGroup >= 0)
        {
            Undo.CollapseUndoOperations(_pathUndoGroup);
        }

        ResetPathDrag();
    }

    private void ResetPathDrag()
    {
        _isPathDragging = false;
        _pathDragButton = -1;
        _pathUndoGroup = -1;
        _lastPathCell = null;
    }

    private void DrawPlayerPaths()
    {
        if (_showPlayerPaths == false || _tool.LevelDefinition == null)
        {
            return;
        }

        Dictionary<Vector2Int, HashSet<Vector2Int>> graph =
            PlayerPathEditorUtility.ReadGraph(_tool.LevelDefinition);

        if (graph.Count == 0)
        {
            return;
        }

        Dictionary<Vector2Int, Vector3> positions = new();

        foreach (CellView cell in GetSceneCells())
        {
            if (cell != null)
            {
                positions[cell.Coordinates] = cell.transform.position;
            }
        }

        Color previousColor = Handles.color;
        CompareFunction previousZTest = Handles.zTest;

        Handles.color = PlayerPathColor;
        Handles.zTest = CompareFunction.Always;

        foreach (KeyValuePair<Vector2Int, HashSet<Vector2Int>> node in graph)
        {
            foreach (Vector2Int connectedCoordinates in node.Value)
            {
                if (ShouldDrawConnection(node.Key, connectedCoordinates) == false ||
                    positions.TryGetValue(node.Key, out Vector3 startPosition) == false ||
                    positions.TryGetValue(connectedCoordinates, out Vector3 endPosition) == false)
                {
                    continue;
                }

                Handles.DrawAAPolyLine(
                    PlayerPathLineWidth,
                    startPosition,
                    endPosition);
            }
        }

        Handles.color = previousColor;
        Handles.zTest = previousZTest;
    }

    private bool TryGetCellAtGuiPoint(Vector2 guiPoint, out CellView cell)
    {
        cell = null;

        return TryGetWorldPosition(guiPoint, out Vector3 worldPosition) &&
               TryGetCellAtWorldPosition(worldPosition, out cell);
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

    private bool TryGetCellAtWorldPosition(Vector3 worldPosition, out CellView cell)
    {
        cell = null;
        float closestDistance = float.MaxValue;

        foreach (CellView candidate in GetSceneCells())
        {
            if (candidate == null)
            {
                continue;
            }

            Vector3 localPosition = candidate.transform.InverseTransformPoint(worldPosition);

            if (Mathf.Abs(localPosition.x) > CellHalfSize ||
                Mathf.Abs(localPosition.y) > CellHalfSize)
            {
                continue;
            }

            float distance = (candidate.transform.position - worldPosition).sqrMagnitude;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                cell = candidate;
            }
        }

        return cell != null;
    }

    private bool TryGetCellByCoordinates(Vector2Int coordinates, out CellView cell)
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

    private CellView[] GetSceneCells()
    {
        SerializedObject serializedTool = new(_tool);
        SerializedProperty cellsParentProperty = serializedTool.FindProperty("_cellsParent");
        Transform cellsParent = cellsParentProperty?.objectReferenceValue as Transform;

        return cellsParent != null
            ? cellsParent.GetComponentsInChildren<CellView>(true)
            : new CellView[0];
    }

    private static bool ShouldDrawConnection(Vector2Int first, Vector2Int second)
    {
        return first.x < second.x ||
               (first.x == second.x && first.y < second.y);
    }

    private void MarkToolDirty()
    {
        EditorUtility.SetDirty(_tool);

        if (_tool.gameObject.scene.IsValid())
        {
            EditorSceneManager.MarkSceneDirty(_tool.gameObject.scene);
        }
    }
}
