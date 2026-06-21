using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(LevelEditorTool))]
public class LevelEditorToolEditor : Editor
{
    private LevelEditorTool _tool;
    private LevelEditorAssetUtility _assetUtility;
    private LevelEditorSceneQuery _sceneQuery;
    private LevelEditorSceneRenderer _sceneRenderer;
    private PlayerPathEditingController _playerPathController;
    private EnemyPathEditingController _enemyPathController;
    private LevelGoalEditingController _goalController;
    private EnemyDeletionController _enemyDeletionController;

    private bool _showPlayerPaths = true;

    private void OnEnable()
    {
        _tool = (LevelEditorTool)target;
        _assetUtility = new LevelEditorAssetUtility();
        _sceneQuery = new LevelEditorSceneQuery(_tool);
        _sceneRenderer = new LevelEditorSceneRenderer(_tool, _sceneQuery);

        _playerPathController = new PlayerPathEditingController(
            _tool,
            _sceneQuery,
            RepaintEditor);
        _enemyPathController = new EnemyPathEditingController(
            _tool,
            _sceneQuery,
            _sceneRenderer,
            RepaintEditor);
        _goalController = new LevelGoalEditingController(
            _tool,
            _sceneQuery,
            MarkToolDirty,
            RepaintEditor);
        _enemyDeletionController = new EnemyDeletionController(
            _tool,
            _sceneQuery,
            MarkToolDirty,
            RepaintEditor);

        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        _playerPathController?.Stop();
        _enemyPathController?.Stop();
        _enemyDeletionController?.Stop();
        _goalController?.Stop();

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

        DrawLevelActions();
        EditorGUILayout.Space(8);
        DrawPlacementControls();
        EditorGUILayout.Space(8);
        DrawLevelGoalControls();
        EditorGUILayout.Space(8);
        DrawPlayerPathControls();
        EditorGUILayout.Space(8);
        DrawEnemyPathControls();
        EditorGUILayout.Space(8);
        DrawSaveControls();
    }

    private void DrawLevelActions()
    {
        if (GUILayout.Button("Create / Rebuild Level"))
        {
            StopAllModes();
            _tool.CreateLevel();
            MarkToolDirty();
        }

        if (GUILayout.Button("Clear Scene Level"))
        {
            StopAllModes();
            _tool.ClearLevel();
            MarkToolDirty();
        }
    }

    private void DrawPlacementControls()
    {
        if (GUILayout.Button("Place EntityView"))
        {
            StopAllModes();
            _tool.StartPlacingCharacter();
            RepaintEditor();
            MarkToolDirty();
        }

        if (GUILayout.Button("Place Enemy"))
        {
            StopAllModes();
            _tool.StartPlacingEnemy();
            RepaintEditor();
            MarkToolDirty();
        }

        if (GUILayout.Button("Delete Enemy"))
        {
            StopAllModes();
            _enemyDeletionController.Start();
        }

        if (GUILayout.Button("Stop Placement"))
        {
            _tool.StopPlacement();
            _enemyDeletionController.Stop();
            RepaintEditor();
            MarkToolDirty();
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

        using (new EditorGUI.DisabledScope(_goalController.IsActive))
        {
            if (GUILayout.Button("Select Level Goal"))
            {
                StopAllModes();
                _goalController.Start();
            }
        }
    }

    private void DrawPlayerPathControls()
    {
        EditorGUILayout.LabelField("Player Path", EditorStyles.boldLabel);

        bool showPlayerPaths = EditorGUILayout.Toggle(
            "Show Player Paths",
            _showPlayerPaths);

        if (showPlayerPaths != _showPlayerPaths)
        {
            _showPlayerPaths = showPlayerPaths;
            SceneView.RepaintAll();
        }

        using (new EditorGUI.DisabledScope(_playerPathController.IsActive))
        {
            if (GUILayout.Button("Draw Player Path"))
            {
                StopAllModes();

                if (_playerPathController.Start())
                {
                    _showPlayerPaths = true;
                }
            }
        }

        using (new EditorGUI.DisabledScope(
                   _playerPathController.IsActive == false))
        {
            if (GUILayout.Button("Stop Draw Player Path"))
            {
                _playerPathController.Stop();
            }
        }
    }

    private void DrawEnemyPathControls()
    {
        EditorGUILayout.LabelField("Enemy Path", EditorStyles.boldLabel);

        using (new EditorGUI.DisabledScope(_enemyPathController.IsActive))
        {
            if (GUILayout.Button("Select Enemy To Draw Path"))
            {
                StopAllModes();
                _enemyPathController.StartSelection();
            }
        }

        using (new EditorGUI.DisabledScope(
                   _enemyPathController.IsActive == false))
        {
            if (GUILayout.Button("Stop Draw Enemy Path"))
            {
                _enemyPathController.Stop();
            }
        }
    }

    private void DrawSaveControls()
    {
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
        if (_enemyDeletionController.IsActive)
        {
            DrawModeMessage(
                "Mode: Delete Enemy. Click an enemy in Scene View. Esc - cancel.");
            return;
        }

        if (_enemyPathController.IsSelectingEnemy)
        {
            DrawModeMessage(
                "Mode: Select Enemy. Click an enemy in Scene View. Esc - cancel.");
            return;
        }

        if (_enemyPathController.IsDrawingPath)
        {
            DrawModeMessage(
                "Mode: Draw Enemy Path. Start at the route end. LMB drag extends, RMB drag erases from the end, Esc stops drawing.");
            return;
        }

        if (_goalController.IsActive)
        {
            DrawModeMessage(
                "Mode: Select Level Goal. Click a cell in Scene View. Esc - cancel.");
            return;
        }

        if (_playerPathController.IsActive)
        {
            DrawModeMessage(
                "Mode: Draw Player Path. LMB drag adds links, RMB drag removes links, Esc stops drawing.");
            return;
        }

        switch (_tool.CurrentMode)
        {
            case LevelEditorMode.None:
                DrawModeMessage("Mode: None");
                break;

            case LevelEditorMode.PlacingCharacter:
                DrawModeMessage(
                    "Mode: Place EntityView. Click on a cell in Scene View. Esc - cancel.");
                break;

            case LevelEditorMode.PlacingEnemy:
                DrawModeMessage(
                    "Mode: Place Enemy. Click on a free cell in Scene View. Esc - cancel.");
                break;
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        if (_tool == null)
        {
            return;
        }

        _sceneRenderer.Draw(_showPlayerPaths);

        Event currentEvent = Event.current;

        if (_enemyDeletionController.IsActive)
        {
            _enemyDeletionController.HandleSceneGui(currentEvent);
            return;
        }

        if (_enemyPathController.IsActive)
        {
            _enemyPathController.HandleSceneGui(currentEvent);
            return;
        }

        if (_goalController.IsActive)
        {
            _goalController.HandleSceneGui(currentEvent);
            return;
        }

        if (_playerPathController.IsActive)
        {
            _playerPathController.HandleSceneGui(currentEvent);
            return;
        }

        HandlePlacementInput(currentEvent);
    }

    private void HandlePlacementInput(Event currentEvent)
    {
        if (_tool.CurrentMode == LevelEditorMode.None)
        {
            return;
        }

        if (currentEvent.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(
                GUIUtility.GetControlID(FocusType.Passive));
            return;
        }

        if (currentEvent.type == EventType.KeyDown &&
            currentEvent.keyCode == KeyCode.Escape)
        {
            _tool.StopPlacement();
            RepaintEditor();
            MarkToolDirty();
            currentEvent.Use();
            return;
        }

        if (currentEvent.type != EventType.MouseDown ||
            currentEvent.button != 0)
        {
            return;
        }

        if (_sceneQuery.TryGetCellAtGuiPoint(
                currentEvent.mousePosition,
                out CellView cell) &&
            _tool.HandleSceneClick(cell.transform.position))
        {
            MarkToolDirty();
            SceneView.RepaintAll();
        }

        currentEvent.Use();
    }

    private void SaveCurrent()
    {
        _tool.ApplySceneToDefinition();

        LevelDefinition savedLevelDefinition =
            _assetUtility.Save(_tool.LevelDefinition);

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
        LevelDefinition newLevelDefinition =
            _assetUtility.SaveAsNew(sourceDefinition);

        if (newLevelDefinition != null)
        {
            PlayerPathEditorUtility.CopyPlayerRoute(
                sourceDefinition,
                newLevelDefinition);
            _assetUtility.Save(newLevelDefinition);
            _tool.SetLevelDefinition(newLevelDefinition);
        }

        MarkToolDirty();
    }

    private void StopAllModes()
    {
        _playerPathController.Stop();
        _enemyPathController.Stop();
        _enemyDeletionController.Stop();
        _goalController.Stop();
        _tool.StopPlacement();
    }

    private void DrawModeMessage(string message)
    {
        EditorGUILayout.HelpBox(message, MessageType.Info);
    }

    private void RepaintEditor()
    {
        SceneView.RepaintAll();
        Repaint();
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
