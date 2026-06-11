using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(LevelEditorTool))]
public class LevelEditorToolEditor : Editor
{
    private LevelEditorTool _tool;
    private LevelEditorAssetUtility _assetUtility;

    private void OnEnable()
    {
        _tool = (LevelEditorTool)target;
        _assetUtility = new();
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

        if (e.type == EventType.MouseDown && e.button == 0 && _tool.CurrentMode == LevelEditorMode.PlacingCharacters)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

            Plane plane = new Plane(Vector3.forward, Vector3.zero);

            if (plane.Raycast(ray, out float distance))
            {
                Vector3 worldPosition = ray.GetPoint(distance);
                _tool.TrySelectCell(worldPosition);
            }

            e.Use();
        }

        if (e.type == EventType.Layout && _tool.CurrentMode == LevelEditorMode.PlacingCharacters)
        {
            HandleUtility.AddDefaultControl(GUIUtility.GetControlID(FocusType.Passive));
        }

        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
        {
            Debug.Log("Escape pressed");

            _tool.StopPlacingCharacters();
            ActiveEditorTracker.sharedTracker.isLocked = false;
            ActiveEditorTracker.sharedTracker.ForceRebuild();

            e.Use();
        }

    }
}
