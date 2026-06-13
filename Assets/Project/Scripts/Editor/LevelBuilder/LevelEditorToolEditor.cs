using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

[CustomEditor(typeof(LevelEditorTool))]
public class LevelEditorToolEditor : Editor
{
    private LevelEditorTool _tool;
    private LevelEditorAssetUtility _assetUtility;

    private void OnEnable()
    {
        _tool = (LevelEditorTool)target;
        _assetUtility = new LevelEditorAssetUtility();

        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
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
            _tool.CreateLevel();
            MarkToolDirty();
        }

        if (GUILayout.Button("Clear Scene Level"))
        {
            _tool.ClearLevel();
            MarkToolDirty();
        }

        EditorGUILayout.Space(8);

        if (GUILayout.Button("Place EntityView"))
        {
            _tool.StartPlacingCharacter();
            SceneView.RepaintAll();
            MarkToolDirty();
        }

        if (GUILayout.Button("Place Enemy"))
        {
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
        switch (_tool.CurrentMode)
        {
            case LevelEditorMode.None:
                EditorGUILayout.HelpBox("Mode: None", MessageType.Info);
                break;

            case LevelEditorMode.PlacingCharacter:
                EditorGUILayout.HelpBox("Mode: Place EntityView. Click on a cell in Scene View. Esc — cancel.", MessageType.Info);
                break;

            case LevelEditorMode.PlacingEnemy:
                EditorGUILayout.HelpBox("Mode: Place Enemy. Click on a free cell in Scene View. Esc — cancel.", MessageType.Info);
                break;
        }
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

        LevelDefinition newLevelDefinition = _assetUtility.SaveAsNew(_tool.LevelDefinition);

        if (newLevelDefinition != null)
        {
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

        Event currentEvent = Event.current;

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

        Ray ray = HandleUtility.GUIPointToWorldRay(currentEvent.mousePosition);
        Plane plane = new Plane(Vector3.forward, new Vector3(0f, 0f, _tool.GridPlaneZ));

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 worldPosition = ray.GetPoint(distance);

            if (_tool.HandleSceneClick(worldPosition))
            {
                MarkToolDirty();
                SceneView.RepaintAll();
            }
        }

        currentEvent.Use();
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