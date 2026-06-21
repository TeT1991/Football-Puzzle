using System;
using UnityEditor;
using UnityEngine;

internal sealed class EnemyDeletionController
{
    private readonly LevelEditorTool _tool;
    private readonly LevelEditorSceneQuery _sceneQuery;
    private readonly Action _markToolDirty;
    private readonly Action _repaint;

    public EnemyDeletionController(
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

        if (_tool.LevelDefinition == null ||
            _tool.LevelDefinition.EnemyPositions.Count == 0)
        {
            Debug.LogWarning("There are no enemies to delete.");
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

        if (_sceneQuery.TryGetEnemyAtGuiPoint(
                currentEvent.mousePosition,
                out LevelEditorPlacedObject enemy) &&
            _tool.DeleteEnemy(enemy.Coordinates))
        {
            EditorUtility.SetDirty(_tool.LevelDefinition);
            _markToolDirty();
            _repaint();

            if (_tool.LevelDefinition.EnemyPositions.Count == 0)
            {
                Stop();
            }
        }

        currentEvent.Use();
    }
}
