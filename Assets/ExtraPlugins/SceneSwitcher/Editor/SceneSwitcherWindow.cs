using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcherWindow : EditorWindow
{
    private const string WindowTitle = "Scene Switcher";
    private const string MenuPath = "Tools/Extra Plugins/Scene Switcher";

    private readonly List<SceneEntry> _scenes = new();
    private Vector2 _scrollPosition;
    private string _search = string.Empty;

    [MenuItem(MenuPath)]
    public static void Open()
    {
        SceneSwitcherWindow window = GetWindow<SceneSwitcherWindow>();
        window.titleContent = new GUIContent(WindowTitle);
        window.minSize = new Vector2(420f, 280f);
        window.RefreshScenes();
        window.Show();
    }

    private void OnEnable()
    {
        RefreshScenes();
    }

    private void OnProjectChange()
    {
        RefreshScenes();
        Repaint();
    }

    private void OnGUI()
    {
        DrawToolbar();

        IReadOnlyList<SceneEntry> filteredScenes = GetFilteredScenes();

        if (filteredScenes.Count == 0)
        {
            EditorGUILayout.HelpBox("No scenes found.", MessageType.Info);
            return;
        }

        _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

        foreach (SceneEntry scene in filteredScenes)
        {
            DrawSceneRow(scene);
        }

        EditorGUILayout.EndScrollView();
    }

    private void DrawToolbar()
    {
        EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

        _search = GUILayout.TextField(_search, GUI.skin.FindStyle("ToolbarSearchTextField"), GUILayout.MinWidth(160f));

        if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(70f)))
        {
            RefreshScenes();
        }

        EditorGUILayout.EndHorizontal();
    }

    private void DrawSceneRow(SceneEntry scene)
    {
        Rect rowRect = EditorGUILayout.BeginVertical(EditorStyles.helpBox);
        bool isActive = SceneManager.GetActiveScene().path == scene.Path;

        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.BeginVertical();

        EditorGUILayout.LabelField(scene.Name, EditorStyles.boldLabel);
        EditorGUILayout.LabelField(scene.Path, EditorStyles.miniLabel);

        EditorGUILayout.EndVertical();

        GUILayout.FlexibleSpace();

        using (new EditorGUI.DisabledScope(isActive))
        {
            if (GUILayout.Button(isActive ? "Active" : "Open", GUILayout.Width(72f), GUILayout.Height(24f)))
            {
                if (OpenScene(scene.Path))
                {
                    RefreshScenes();
                    Repaint();
                }
            }
        }

        EditorGUILayout.EndHorizontal();

        if (scene.IsInBuildSettings)
        {
            Rect badgeRect = GUILayoutUtility.GetRect(90f, 18f, GUILayout.Width(90f));
            GUI.Label(badgeRect, "Build Settings", EditorStyles.miniLabel);
        }

        EditorGUILayout.EndVertical();

        if (Event.current.type == EventType.MouseDown && Event.current.clickCount == 2 && rowRect.Contains(Event.current.mousePosition))
        {
            if (OpenScene(scene.Path))
            {
                RefreshScenes();
                Repaint();
            }

            Event.current.Use();
        }
    }

    private IReadOnlyList<SceneEntry> GetFilteredScenes()
    {
        if (string.IsNullOrWhiteSpace(_search))
        {
            return _scenes;
        }

        string normalizedSearch = _search.Trim();

        return _scenes
            .Where(scene =>
                scene.Name.IndexOf(normalizedSearch, StringComparison.OrdinalIgnoreCase) >= 0 ||
                scene.Path.IndexOf(normalizedSearch, StringComparison.OrdinalIgnoreCase) >= 0)
            .ToList();
    }

    private void RefreshScenes()
    {
        HashSet<string> buildScenePaths = new();

        foreach (EditorBuildSettingsScene scene in EditorBuildSettings.scenes)
        {
            if (scene.enabled)
            {
                buildScenePaths.Add(scene.path);
            }
        }

        _scenes.Clear();

        foreach (string guid in AssetDatabase.FindAssets("t:Scene"))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);

            if (string.IsNullOrEmpty(path))
            {
                continue;
            }

            _scenes.Add(new SceneEntry(path, buildScenePaths.Contains(path)));
        }

        _scenes.Sort((left, right) => string.Compare(left.Path, right.Path, StringComparison.OrdinalIgnoreCase));
    }

    private static bool OpenScene(string scenePath)
    {
        if (EditorApplication.isPlaying)
        {
            EditorSceneManager.LoadSceneInPlayMode(scenePath, new LoadSceneParameters(LoadSceneMode.Single));
            return true;
        }

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
        {
            return false;
        }

        EditorSceneManager.OpenScene(scenePath);
        return true;
    }

    private readonly struct SceneEntry
    {
        public SceneEntry(string path, bool isInBuildSettings)
        {
            Path = path;
            Name = System.IO.Path.GetFileNameWithoutExtension(path);
            IsInBuildSettings = isInBuildSettings;
        }

        public string Path { get; }
        public string Name { get; }
        public bool IsInBuildSettings { get; }
    }
}
