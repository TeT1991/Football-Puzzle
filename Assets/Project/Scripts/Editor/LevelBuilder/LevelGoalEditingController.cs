using System;
using UnityEditor;
using UnityEngine;

internal sealed class LevelGoalEditingController
{
    private readonly LevelEditorTool _tool;
    private readonly LevelEditorSceneQuery _sceneQuery;
    private readonly Action _markToolDirty;
    private readonly Action _repaint;

    public LevelGoalEditingController(
        LevelEditorTool tool,
        LevelEditorSceneQuery sceneQuery,
        Action markToolDirty,
        Action repaint)
    {
        _tool = tool;
        _sceneQuery = sceneQuery;
        _markToolDirty = markToolDirty;
        _repaint = repaint;
    }

    public bool IsActive { get; private set; }

    public bool Start()
    {
        _tool.ApplySceneToDefinition();

        if (_tool.LevelDefinition == null)
        {
            Debug.LogWarning(
                "Assign or create a LevelDefinition before selecting a level goal.");
            return false;
        }

        if (_sceneQuery.GetSceneCells().Length == 0)
        {
            _tool.CreateLevel();
        }

        if (_sceneQuery.GetSceneCells().Length == 0)
        {
            Debug.LogWarning(
                "The scene grid is not available for selecting a level goal.");
            return false;
        }

        IsActive = true;
        _repaint();
        return true;
    }

    public void Stop()
    {
        if (IsActive == false)
        {
            return;
        }

        IsActive = false;
        _repaint();
    }

    public void HandleSceneGui(Event currentEvent)
    {
        if (currentEvent.type == EventType.Layout)
        {
            HandleUtility.AddDefaultControl(
                GUIUtility.GetControlID(FocusType.Passive));
            return;
        }

        if (currentEvent.type == EventType.KeyDown &&
            currentEvent.keyCode == KeyCode.Escape)
        {
            Stop();
            currentEvent.Use();
            return;
        }

        if (currentEvent.alt ||
            currentEvent.type != EventType.MouseDown ||
            currentEvent.button != 0)
        {
            return;
        }

        if (_sceneQuery.TryGetCellAtGuiPoint(
                currentEvent.mousePosition,
                out CellView cell))
        {
            LevelDefinition definition = _tool.LevelDefinition;

            Undo.RecordObject(definition, "Select Level Goal");
            definition.SetGoal(cell.Coordinates);
            EditorUtility.SetDirty(definition);

            Stop();
            _markToolDirty();
            _repaint();
        }

        currentEvent.Use();
    }
}
