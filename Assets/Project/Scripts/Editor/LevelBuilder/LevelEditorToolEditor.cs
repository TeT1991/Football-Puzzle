using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelEditorTool))]
public class LevelEditorToolEditor : Editor
{
    private LevelEditorTool _tool;
    private LevelEditorAssetUtility _assetUtility;

    private void Awake()
    {
        _tool = (LevelEditorTool)target;
        _assetUtility = new();
    }

    private void OnEnable()
    {
        Debug.Log("LevelEditorToolEditor enabled");
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        Debug.Log("LevelEditorToolEditor disabled");
        SceneView.duringSceneGui -= OnSceneGUI;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("Create Level"))
        {
            _tool.CreateLevel();

            Debug.Log(_assetUtility);

            LevelDefinition levelDefinition = _assetUtility.CreateLevelDefinitonAsset(_tool.GenerateLevelData());

            _tool.SetLevelDefinition(levelDefinition);
            EditorUtility.SetDirty(_tool);
        }

        if (GUILayout.Button("Clear Level"))
        {
            _tool.ClearLevel();
        }

        if (GUILayout.Button("Start placing character"))
        {

            ActiveEditorTracker.sharedTracker.isLocked = true;
            ActiveEditorTracker.sharedTracker.ForceRebuild();

            _tool.StartPlacingCharacters();
        }

        if (GUILayout.Button("Stop placing character"))
        {
            ActiveEditorTracker.sharedTracker.isLocked = false;
            ActiveEditorTracker.sharedTracker.ForceRebuild();

            _tool.StopPlacingCharacters();
        }
    }

    private void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        if (e.type == EventType.MouseDown && _tool.CurrentMode == LevelEditorMode.PlacingCharacters)
        {
            Debug.Log("Ћевый клик мыши в Scene View");
        }

        if (e.type == EventType.Layout && _tool.CurrentMode == LevelEditorMode.PlacingCharacters)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }

        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
        {
            Debug.Log("Escape pressed");

            _tool.StopPlacingCharacters();

            e.Use();
            return;
        }

    }
}
