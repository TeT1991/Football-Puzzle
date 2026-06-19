using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

[CustomEditor(typeof(LevelEditorTool))]
public class LevelEditorToolEditor : Editor
{
    private const float CellHalfSize = 0.5f;
    private const float PlayerPathLineWidth = 13f;
    private const float EnemyPathLineWidth = 13f;
    private const float PathLaneSpacing = 0.14f;
    private const int PlayerPathOwnerId = 0;
    private const float GoalMarkerLineWidth = 3f;
    private const float GoalMarkerOuterRadius = 0.32f;
    private const float GoalMarkerInnerRadius = 0.14f;

    private static readonly Color PlayerPathColor = new(0.02f, 0.18f, 0.42f, 1f);
    private static readonly Color GoalMarkerColor = new(1f, 0.75f, 0.05f, 0.9f);
    private static readonly Color GoalMarkerFillColor = new(1f, 0.75f, 0.05f, 0.35f);

    private LevelEditorTool _tool;
    private LevelEditorAssetUtility _assetUtility;

    private bool _isDrawingPlayerPath;
    private bool _isSelectingEnemyForPath;
    private bool _isDrawingEnemyPath;
    private bool _isDeletingEnemy;
    private bool _isSelectingLevelGoal;
    private bool _showPlayerPaths = true;
    private bool _isPathDragging;
    private int _pathDragButton = -1;
    private int _pathUndoGroup = -1;
    private CellView _lastPathCell;
    private bool _isEnemyPathDragging;
    private int _enemyPathDragButton = -1;
    private int _enemyPathUndoGroup = -1;
    private CellView _lastEnemyPathCell;
    private LevelEditorPlacedObject _selectedEnemy;

    private void OnEnable()
    {
        _tool = (LevelEditorTool)target;
        _assetUtility = new LevelEditorAssetUtility();

        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        EndPathDrag();
        StopEnemyPathDrawing();
        _isDeletingEnemy = false;
        _isSelectingLevelGoal = false;
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
            StopEnemyPathDrawing();
            StopEnemyDeletion();
            StopLevelGoalSelection();
            _tool.CreateLevel();
            MarkToolDirty();
        }

        if (GUILayout.Button("Clear Scene Level"))
        {
            StopPlayerPathDrawing();
            StopEnemyPathDrawing();
            StopEnemyDeletion();
            StopLevelGoalSelection();
            _tool.ClearLevel();
            MarkToolDirty();
        }

        EditorGUILayout.Space(8);

        if (GUILayout.Button("Place EntityView"))
        {
            StopPlayerPathDrawing();
            StopEnemyPathDrawing();
            StopEnemyDeletion();
            StopLevelGoalSelection();
            _tool.StartPlacingCharacter();
            SceneView.RepaintAll();
            MarkToolDirty();
        }

        if (GUILayout.Button("Place Enemy"))
        {
            StopPlayerPathDrawing();
            StopEnemyPathDrawing();
            StopEnemyDeletion();
            StopLevelGoalSelection();
            _tool.StartPlacingEnemy();
            SceneView.RepaintAll();
            MarkToolDirty();
        }

        if (GUILayout.Button("Delete Enemy"))
        {
            StartEnemyDeletion();
        }

        if (GUILayout.Button("Stop Placement"))
        {
            _tool.StopPlacement();
            StopEnemyDeletion();
            SceneView.RepaintAll();
            MarkToolDirty();
        }

        EditorGUILayout.Space(8);
        DrawLevelGoalControls();
        EditorGUILayout.Space(8);
        DrawPlayerPathControls();
        EditorGUILayout.Space(8);
        DrawEnemyPathControls();
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
        if (_isDeletingEnemy)
        {
            EditorGUILayout.HelpBox(
                "Mode: Delete Enemy. Click an enemy in Scene View. Esc - cancel.",
                MessageType.Info);
            return;
        }

        if (_isSelectingEnemyForPath)
        {
            EditorGUILayout.HelpBox(
                "Mode: Select Enemy. Click an enemy in Scene View. Esc - cancel.",
                MessageType.Info);
            return;
        }

        if (_isDrawingEnemyPath)
        {
            EditorGUILayout.HelpBox(
                "Mode: Draw Enemy Path. Start at the route end. LMB drag extends, RMB drag erases from the end, Esc stops drawing.",
                MessageType.Info);
            return;
        }

        if (_isSelectingLevelGoal)
        {
            EditorGUILayout.HelpBox(
                "Mode: Select Level Goal. Click a cell in Scene View. Esc - cancel.",
                MessageType.Info);
            return;
        }

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

    private void DrawLevelGoalControls()
    {
        EditorGUILayout.LabelField("Level Goal", EditorStyles.boldLabel);

        LevelDefinition definition = _tool.LevelDefinition;
        string coordinates = definition != null && definition.HasGoal
            ? definition.GoalCoordinates.ToString()
            : "Not selected";

        EditorGUILayout.LabelField("Goal Coordinates", coordinates);

        using (new EditorGUI.DisabledScope(_isSelectingLevelGoal))
        {
            if (GUILayout.Button("Select Level Goal"))
            {
                StartLevelGoalSelection();
            }
        }
    }

    private void StartLevelGoalSelection()
    {
        StopPlayerPathDrawing();
        StopEnemyPathDrawing();
        StopEnemyDeletion();
        _tool.StopPlacement();
        _tool.ApplySceneToDefinition();

        if (_tool.LevelDefinition == null)
        {
            Debug.LogWarning("Assign or create a LevelDefinition before selecting a level goal.");
            return;
        }

        if (GetSceneCells().Length == 0)
        {
            _tool.CreateLevel();
        }

        if (GetSceneCells().Length == 0)
        {
            Debug.LogWarning("The scene grid is not available for selecting a level goal.");
            return;
        }

        _isSelectingLevelGoal = true;
        SceneView.RepaintAll();
        Repaint();
    }

    private void StopLevelGoalSelection()
    {
        if (_isSelectingLevelGoal == false)
        {
            return;
        }

        _isSelectingLevelGoal = false;
        SceneView.RepaintAll();
        Repaint();
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
        StopLevelGoalSelection();
        StopEnemyPathDrawing();
        StopEnemyDeletion();
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

    private void DrawEnemyPathControls()
    {
        EditorGUILayout.LabelField("Enemy Path", EditorStyles.boldLabel);

        using (new EditorGUI.DisabledScope(
                   _isSelectingEnemyForPath || _isDrawingEnemyPath))
        {
            if (GUILayout.Button("Select Enemy To Draw Path"))
            {
                StartEnemyPathSelection();
            }
        }

        using (new EditorGUI.DisabledScope(
                   _isSelectingEnemyForPath == false && _isDrawingEnemyPath == false))
        {
            if (GUILayout.Button("Stop Draw Enemy Path"))
            {
                StopEnemyPathDrawing();
            }
        }
    }

    private void StartEnemyPathSelection()
    {
        StopPlayerPathDrawing();
        StopLevelGoalSelection();
        StopEnemyPathDrawing();
        StopEnemyDeletion();
        _tool.StopPlacement();
        _tool.ApplySceneToDefinition();

        LevelDefinition definition = _tool.LevelDefinition;

        if (definition == null || definition.EnemyPositions.Count == 0)
        {
            Debug.LogWarning("Place at least one enemy before drawing an enemy path.");
            return;
        }

        _isSelectingEnemyForPath = true;
        SceneView.RepaintAll();
        Repaint();
    }

    private void SelectEnemyForPath(LevelEditorPlacedObject enemy)
    {
        if (enemy == null || enemy.Type != LevelEditorObjectType.Enemy)
        {
            return;
        }

        _selectedEnemy = enemy;
        _isSelectingEnemyForPath = false;
        _isDrawingEnemyPath = true;

        EnemyPathEditorUtility.EnsurePath(
            _tool.LevelDefinition,
            enemy.Coordinates);

        ApplySelectedEnemyColor();
        ResetEnemyPathDrag();
        SceneView.RepaintAll();
        Repaint();
    }

    private void StopEnemyPathDrawing()
    {
        if (_isSelectingEnemyForPath == false &&
            _isDrawingEnemyPath == false &&
            _selectedEnemy == null &&
            _isEnemyPathDragging == false)
        {
            return;
        }

        EndEnemyPathDrag();
        _selectedEnemy = null;
        _isSelectingEnemyForPath = false;
        _isDrawingEnemyPath = false;

        SceneView.RepaintAll();
        Repaint();
    }

    private void StartEnemyDeletion()
    {
        StopPlayerPathDrawing();
        StopLevelGoalSelection();
        StopEnemyPathDrawing();
        _tool.StopPlacement();
        _tool.ApplySceneToDefinition();

        if (_tool.LevelDefinition == null ||
            _tool.LevelDefinition.EnemyPositions.Count == 0)
        {
            Debug.LogWarning("There are no enemies to delete.");
            return;
        }

        _isDeletingEnemy = true;
        SceneView.RepaintAll();
        Repaint();
    }

    private void StopEnemyDeletion()
    {
        if (_isDeletingEnemy == false)
        {
            return;
        }

        _isDeletingEnemy = false;
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

        Dictionary<(Vector2Int, Vector2Int), List<int>> pathSegmentOwners =
            BuildPathSegmentOwners();

        DrawPlayerPaths(pathSegmentOwners);
        DrawEnemyPaths(pathSegmentOwners);
        DrawLevelGoalMarker();

        Event currentEvent = Event.current;

        if (_isDeletingEnemy)
        {
            HandleEnemyDeletionInput(currentEvent);
            return;
        }

        if (_isSelectingEnemyForPath)
        {
            HandleEnemySelectionInput(currentEvent);
            return;
        }

        if (_isDrawingEnemyPath)
        {
            HandleEnemyPathInput(currentEvent);
            return;
        }

        if (_isSelectingLevelGoal)
        {
            HandleLevelGoalInput(currentEvent);
            return;
        }

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

    private void HandleEnemyDeletionInput(Event currentEvent)
    {
        if (currentEvent.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            return;
        }

        if (currentEvent.type == EventType.KeyDown && currentEvent.keyCode == KeyCode.Escape)
        {
            StopEnemyDeletion();
            currentEvent.Use();
            return;
        }

        if (currentEvent.alt ||
            currentEvent.type != EventType.MouseDown ||
            currentEvent.button != 0)
        {
            return;
        }

        if (TryGetEnemyAtGuiPoint(currentEvent.mousePosition, out LevelEditorPlacedObject enemy) &&
            _tool.DeleteEnemy(enemy.Coordinates))
        {
            EditorUtility.SetDirty(_tool.LevelDefinition);
            MarkToolDirty();
            SceneView.RepaintAll();

            if (_tool.LevelDefinition.EnemyPositions.Count == 0)
            {
                StopEnemyDeletion();
            }
        }

        currentEvent.Use();
    }

    private void HandleEnemySelectionInput(Event currentEvent)
    {
        if (currentEvent.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            return;
        }

        if (currentEvent.type == EventType.KeyDown && currentEvent.keyCode == KeyCode.Escape)
        {
            StopEnemyPathDrawing();
            currentEvent.Use();
            return;
        }

        if (currentEvent.alt ||
            currentEvent.type != EventType.MouseDown ||
            currentEvent.button != 0)
        {
            return;
        }

        if (TryGetEnemyAtGuiPoint(currentEvent.mousePosition, out LevelEditorPlacedObject enemy))
        {
            SelectEnemyForPath(enemy);
        }
        else
        {
            Debug.LogWarning("Click an enemy to draw its path.");
        }

        currentEvent.Use();
    }

    private void HandleEnemyPathInput(Event currentEvent)
    {
        if (currentEvent.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            return;
        }

        if (currentEvent.type == EventType.KeyDown && currentEvent.keyCode == KeyCode.Escape)
        {
            StopEnemyPathDrawing();
            currentEvent.Use();
            return;
        }

        if (currentEvent.alt)
        {
            return;
        }

        if (_selectedEnemy == null)
        {
            StopEnemyPathDrawing();
            return;
        }

        if (currentEvent.type == EventType.MouseDown &&
            (currentEvent.button == 0 || currentEvent.button == 1))
        {
            if (TryGetCellAtGuiPoint(currentEvent.mousePosition, out CellView cell) &&
                CanBeginEnemyPathDrag(cell))
            {
                BeginEnemyPathDrag(cell, currentEvent.button);
                currentEvent.Use();
            }

            return;
        }

        if (currentEvent.type == EventType.MouseDrag && _isEnemyPathDragging)
        {
            if (TryGetCellAtGuiPoint(currentEvent.mousePosition, out CellView cell))
            {
                HandleEnemyPathDrag(cell);
            }

            currentEvent.Use();
            return;
        }

        if (currentEvent.type == EventType.MouseUp && _isEnemyPathDragging)
        {
            EndEnemyPathDrag();
            currentEvent.Use();
        }
    }

    private bool CanBeginEnemyPathDrag(CellView cell)
    {
        if (cell == null || _selectedEnemy == null)
        {
            return false;
        }

        List<Vector2Int> path = EnemyPathEditorUtility.ReadPath(
            _tool.LevelDefinition,
            _selectedEnemy.Coordinates);

        if (EnemyPathEditorUtility.IsEndpoint(path, cell.Coordinates))
        {
            return true;
        }

        Debug.LogWarning("Continue or erase an enemy path from its end point.");
        return false;
    }

    private void BeginEnemyPathDrag(CellView cell, int mouseButton)
    {
        Undo.IncrementCurrentGroup();
        _enemyPathUndoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName(mouseButton == 0 ? "Draw Enemy Path" : "Erase Enemy Path");

        _isEnemyPathDragging = true;
        _enemyPathDragButton = mouseButton;
        _lastEnemyPathCell = cell;
    }

    private void HandleEnemyPathDrag(CellView cell)
    {
        if (cell == null ||
            _lastEnemyPathCell == null ||
            cell == _lastEnemyPathCell ||
            _selectedEnemy == null)
        {
            return;
        }

        Vector2Int previousCoordinates = _lastEnemyPathCell.Coordinates;
        Vector2Int currentCoordinates = cell.Coordinates;
        bool changed = _enemyPathDragButton == 0
            ? EnemyPathEditorUtility.Append(
                _tool.LevelDefinition,
                _selectedEnemy.Coordinates,
                currentCoordinates)
            : EnemyPathEditorUtility.RemoveLastSegment(
                _tool.LevelDefinition,
                _selectedEnemy.Coordinates,
                previousCoordinates,
                currentCoordinates);

        if (changed == false)
        {
            return;
        }

        _lastEnemyPathCell = cell;
        SceneView.RepaintAll();
        Repaint();
    }

    private void EndEnemyPathDrag()
    {
        if (_enemyPathUndoGroup >= 0)
        {
            Undo.CollapseUndoOperations(_enemyPathUndoGroup);
        }

        ResetEnemyPathDrag();
    }

    private void ResetEnemyPathDrag()
    {
        _isEnemyPathDragging = false;
        _enemyPathDragButton = -1;
        _enemyPathUndoGroup = -1;
        _lastEnemyPathCell = null;
    }

    private void HandleLevelGoalInput(Event currentEvent)
    {
        if (currentEvent.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
            return;
        }

        if (currentEvent.type == EventType.KeyDown && currentEvent.keyCode == KeyCode.Escape)
        {
            StopLevelGoalSelection();
            currentEvent.Use();
            return;
        }

        if (currentEvent.alt ||
            currentEvent.type != EventType.MouseDown ||
            currentEvent.button != 0)
        {
            return;
        }

        if (TryGetCellAtGuiPoint(currentEvent.mousePosition, out CellView cell))
        {
            LevelDefinition definition = _tool.LevelDefinition;

            Undo.RecordObject(definition, "Select Level Goal");
            definition.SetGoal(cell.Coordinates);
            EditorUtility.SetDirty(definition);

            StopLevelGoalSelection();
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

    private Dictionary<(Vector2Int, Vector2Int), List<int>> BuildPathSegmentOwners()
    {
        Dictionary<(Vector2Int, Vector2Int), List<int>> owners = new();
        LevelDefinition definition = _tool.LevelDefinition;

        if (definition == null)
        {
            return owners;
        }

        Dictionary<Vector2Int, HashSet<Vector2Int>> playerGraph =
            PlayerPathEditorUtility.ReadGraph(definition);

        foreach (KeyValuePair<Vector2Int, HashSet<Vector2Int>> node in playerGraph)
        {
            foreach (Vector2Int connectedCoordinates in node.Value)
            {
                if (ShouldDrawConnection(node.Key, connectedCoordinates))
                {
                    AddPathSegmentOwner(
                        owners,
                        node.Key,
                        connectedCoordinates,
                        PlayerPathOwnerId);
                }
            }
        }

        if (definition.EnemyRoutes == null)
        {
            return owners;
        }

        foreach (EnemyRoute route in definition.EnemyRoutes)
        {
            if (route == null)
            {
                continue;
            }

            int ownerId = GetEnemyPathOwnerId(
                definition,
                route.EnemyStartCoordinates);

            for (int i = 1; i < route.Coordinates.Count; i++)
            {
                AddPathSegmentOwner(
                    owners,
                    route.Coordinates[i - 1],
                    route.Coordinates[i],
                    ownerId);
            }
        }

        foreach (List<int> segmentOwners in owners.Values)
        {
            segmentOwners.Sort();
        }

        return owners;
    }

    private static void AddPathSegmentOwner(
        Dictionary<(Vector2Int, Vector2Int), List<int>> owners,
        Vector2Int first,
        Vector2Int second,
        int ownerId)
    {
        (Vector2Int, Vector2Int) key = GetPathSegmentKey(first, second);

        if (owners.TryGetValue(key, out List<int> segmentOwners) == false)
        {
            segmentOwners = new List<int>();
            owners.Add(key, segmentOwners);
        }

        if (segmentOwners.Contains(ownerId) == false)
        {
            segmentOwners.Add(ownerId);
        }
    }

    private static Vector3 GetPathLaneOffset(
        Vector2Int firstCoordinates,
        Vector2Int secondCoordinates,
        Vector3 firstPosition,
        Vector3 secondPosition,
        int ownerId,
        Dictionary<(Vector2Int, Vector2Int), List<int>> owners)
    {
        (Vector2Int, Vector2Int) key = GetPathSegmentKey(
            firstCoordinates,
            secondCoordinates);

        if (owners.TryGetValue(key, out List<int> segmentOwners) == false ||
            segmentOwners.Count < 2)
        {
            return Vector3.zero;
        }

        int laneIndex = segmentOwners.IndexOf(ownerId);

        if (laneIndex < 0)
        {
            return Vector3.zero;
        }

        Vector3 direction = ShouldDrawConnection(firstCoordinates, secondCoordinates)
            ? secondPosition - firstPosition
            : firstPosition - secondPosition;

        direction.Normalize();

        float centeredLane = laneIndex - (segmentOwners.Count - 1) * 0.5f;
        Vector3 perpendicular = new(-direction.y, direction.x, 0f);
        return perpendicular * centeredLane * PathLaneSpacing;
    }

    private static (Vector2Int, Vector2Int) GetPathSegmentKey(
        Vector2Int first,
        Vector2Int second)
    {
        return ShouldDrawConnection(first, second)
            ? (first, second)
            : (second, first);
    }

    private static int GetEnemyPathOwnerId(
        LevelDefinition definition,
        Vector2Int enemyCoordinates)
    {
        return 1 + enemyCoordinates.y * definition.Width + enemyCoordinates.x;
    }

    private void DrawPlayerPaths(
        Dictionary<(Vector2Int, Vector2Int), List<int>> pathSegmentOwners)
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

                Vector3 offset = GetPathLaneOffset(
                    node.Key,
                    connectedCoordinates,
                    startPosition,
                    endPosition,
                    PlayerPathOwnerId,
                    pathSegmentOwners);

                Handles.DrawAAPolyLine(
                    PlayerPathLineWidth,
                    startPosition + offset,
                    endPosition + offset);
            }
        }

        Handles.color = previousColor;
        Handles.zTest = previousZTest;
    }

    private void DrawEnemyPaths(
        Dictionary<(Vector2Int, Vector2Int), List<int>> pathSegmentOwners)
    {
        LevelDefinition definition = _tool.LevelDefinition;

        if (definition == null || definition.EnemyRoutes == null)
        {
            return;
        }

        ApplyEnemyRouteColors(definition);

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
        Handles.zTest = CompareFunction.Always;

        foreach (EnemyRoute route in definition.EnemyRoutes)
        {
            if (route == null || route.Coordinates.Count < 2)
            {
                continue;
            }

            Handles.color = GetEnemyPathColor(route.EnemyStartCoordinates);

            for (int i = 1; i < route.Coordinates.Count; i++)
            {
                if (positions.TryGetValue(route.Coordinates[i - 1], out Vector3 start) &&
                    positions.TryGetValue(route.Coordinates[i], out Vector3 end))
                {
                    Vector3 offset = GetPathLaneOffset(
                        route.Coordinates[i - 1],
                        route.Coordinates[i],
                        start,
                        end,
                        GetEnemyPathOwnerId(definition, route.EnemyStartCoordinates),
                        pathSegmentOwners);

                    Handles.DrawAAPolyLine(
                        EnemyPathLineWidth,
                        start + offset,
                        end + offset);
                }
            }
        }

        Handles.color = previousColor;
        Handles.zTest = previousZTest;
    }

    private void ApplySelectedEnemyColor()
    {
        if (_selectedEnemy == null)
        {
            return;
        }

        Color color = GetEnemyColor(_selectedEnemy.Coordinates);

        foreach (SpriteRenderer spriteRenderer in
                 _selectedEnemy.GetComponentsInChildren<SpriteRenderer>(true))
        {
            spriteRenderer.color = color;
        }
    }

    private void ApplyEnemyRouteColors(LevelDefinition definition)
    {
        foreach (LevelEditorPlacedObject enemy in GetSceneEnemies())
        {
            if (enemy == null ||
                enemy.Type != LevelEditorObjectType.Enemy ||
                definition.TryGetEnemyRoute(enemy.Coordinates, out _) == false)
            {
                continue;
            }

            Color color = GetEnemyColor(enemy.Coordinates);

            foreach (SpriteRenderer spriteRenderer in
                     enemy.GetComponentsInChildren<SpriteRenderer>(true))
            {
                spriteRenderer.color = color;
            }
        }
    }

    private static Color GetEnemyPathColor(Vector2Int coordinates)
    {
        Color color = Color.Lerp(GetEnemyColor(coordinates), Color.black, 0.35f);
        color.a = 1f;
        return color;
    }

    private static Color GetEnemyColor(Vector2Int coordinates)
    {
        int hash = coordinates.x * 73856093 ^ coordinates.y * 19349663;
        float hue = Mathf.Repeat(Mathf.Abs(hash) * 0.000013f, 1f);
        return Color.HSVToRGB(hue, 0.75f, 1f);
    }

    private void DrawLevelGoalMarker()
    {
        LevelDefinition definition = _tool.LevelDefinition;

        if (definition == null ||
            definition.HasGoal == false ||
            TryGetCellByCoordinates(definition.GoalCoordinates, out CellView cell) == false)
        {
            return;
        }

        Vector3 center = cell.transform.position;
        Vector3[] points = new Vector3[11];

        for (int i = 0; i < 10; i++)
        {
            float radius = i % 2 == 0 ? GoalMarkerOuterRadius : GoalMarkerInnerRadius;
            float angle = Mathf.PI * 0.5f + i * Mathf.PI / 5f;

            points[i] = center + new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0f);
        }

        points[10] = points[0];

        Color previousColor = Handles.color;
        CompareFunction previousZTest = Handles.zTest;

        Handles.zTest = CompareFunction.Always;
        Handles.color = GoalMarkerFillColor;

        for (int i = 0; i < 10; i++)
        {
            Handles.DrawAAConvexPolygon(center, points[i], points[i + 1]);
        }

        Handles.color = GoalMarkerColor;
        Handles.DrawAAPolyLine(GoalMarkerLineWidth, points);

        Handles.color = previousColor;
        Handles.zTest = previousZTest;
    }

    private bool TryGetCellAtGuiPoint(Vector2 guiPoint, out CellView cell)
    {
        cell = null;

        return TryGetWorldPosition(guiPoint, out Vector3 worldPosition) &&
               TryGetCellAtWorldPosition(worldPosition, out cell);
    }

    private bool TryGetEnemyAtGuiPoint(
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

    private LevelEditorPlacedObject[] GetSceneEnemies()
    {
        SerializedObject serializedTool = new(_tool);
        SerializedProperty enemiesParentProperty =
            serializedTool.FindProperty("_enemiesParent");
        Transform enemiesParent = enemiesParentProperty?.objectReferenceValue as Transform;

        return enemiesParent != null
            ? enemiesParent.GetComponentsInChildren<LevelEditorPlacedObject>(true)
            : new LevelEditorPlacedObject[0];
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
