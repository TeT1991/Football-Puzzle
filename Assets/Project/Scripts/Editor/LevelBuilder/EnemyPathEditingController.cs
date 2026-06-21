using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

internal sealed class EnemyPathEditingController
{
    private readonly LevelEditorTool _tool;
    private readonly LevelEditorSceneQuery _sceneQuery;
    private readonly LevelEditorSceneRenderer _sceneRenderer;
    private readonly Action _repaint;

    private bool _isDragging;
    private int _dragButton = -1;
    private int _undoGroup = -1;
    private CellView _lastCell;
    private LevelEditorPlacedObject _selectedEnemy;

    public EnemyPathEditingController(
        LevelEditorTool tool,
        LevelEditorSceneQuery sceneQuery,
        LevelEditorSceneRenderer sceneRenderer,
        Action repaint)
    {
        _tool = tool;
        _sceneQuery = sceneQuery;
        _sceneRenderer = sceneRenderer;
        _repaint = repaint;
    }

    public bool IsSelectingEnemy { get; private set; }
    public bool IsDrawingPath { get; private set; }
    public bool IsActive => IsSelectingEnemy || IsDrawingPath;

    public bool StartSelection()
    {
        _tool.ApplySceneToDefinition();

        LevelDefinition definition = _tool.LevelDefinition;

        if (definition == null || definition.EnemyPositions.Count == 0)
        {
            Debug.LogWarning(
                "Place at least one enemy before drawing an enemy path.");
            return false;
        }

        IsSelectingEnemy = true;
        _repaint();
        return true;
    }

    public void Stop()
    {
        if (IsActive == false &&
            _selectedEnemy == null &&
            _isDragging == false)
        {
            return;
        }

        EndDrag();
        _selectedEnemy = null;
        IsSelectingEnemy = false;
        IsDrawingPath = false;
        _repaint();
    }

    public void HandleSceneGui(Event currentEvent)
    {
        if (IsSelectingEnemy)
        {
            HandleEnemySelection(currentEvent);
            return;
        }

        if (IsDrawingPath)
        {
            HandlePathEditing(currentEvent);
        }
    }

    private void HandleEnemySelection(Event currentEvent)
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
                out LevelEditorPlacedObject enemy))
        {
            SelectEnemy(enemy);
        }
        else
        {
            Debug.LogWarning("Click an enemy to draw its path.");
        }

        currentEvent.Use();
    }

    private void SelectEnemy(LevelEditorPlacedObject enemy)
    {
        if (enemy == null || enemy.Type != LevelEditorObjectType.Enemy)
        {
            return;
        }

        _selectedEnemy = enemy;
        IsSelectingEnemy = false;
        IsDrawingPath = true;

        EnemyPathEditorUtility.EnsurePath(
            _tool.LevelDefinition,
            enemy.Coordinates);

        _sceneRenderer.ApplySelectedEnemyColor(enemy);
        ResetDrag();
        _repaint();
    }

    private void HandlePathEditing(Event currentEvent)
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

        if (currentEvent.alt)
        {
            return;
        }

        if (_selectedEnemy == null)
        {
            Stop();
            return;
        }

        if (currentEvent.type == EventType.MouseDown &&
            (currentEvent.button == 0 || currentEvent.button == 1))
        {
            if (_sceneQuery.TryGetCellAtGuiPoint(
                    currentEvent.mousePosition,
                    out CellView cell) &&
                CanBeginDrag(cell))
            {
                BeginDrag(cell, currentEvent.button);
                currentEvent.Use();
            }

            return;
        }

        if (currentEvent.type == EventType.MouseDrag && _isDragging)
        {
            if (_sceneQuery.TryGetCellAtGuiPoint(
                    currentEvent.mousePosition,
                    out CellView cell))
            {
                HandleDrag(cell);
            }

            currentEvent.Use();
            return;
        }

        if (currentEvent.type == EventType.MouseUp && _isDragging)
        {
            EndDrag();
            currentEvent.Use();
        }
    }

    private bool CanBeginDrag(CellView cell)
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

    private void BeginDrag(CellView cell, int mouseButton)
    {
        Undo.IncrementCurrentGroup();
        _undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName(
            mouseButton == 0 ? "Draw Enemy Path" : "Erase Enemy Path");

        _isDragging = true;
        _dragButton = mouseButton;
        _lastCell = cell;
    }

    private void HandleDrag(CellView cell)
    {
        if (cell == null ||
            _lastCell == null ||
            cell == _lastCell ||
            _selectedEnemy == null)
        {
            return;
        }

        Vector2Int previousCoordinates = _lastCell.Coordinates;
        Vector2Int currentCoordinates = cell.Coordinates;

        bool changed = _dragButton == 0
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

        _lastCell = cell;
        _repaint();
    }

    private void EndDrag()
    {
        if (_undoGroup >= 0)
        {
            Undo.CollapseUndoOperations(_undoGroup);
        }

        ResetDrag();
    }

    private void ResetDrag()
    {
        _isDragging = false;
        _dragButton = -1;
        _undoGroup = -1;
        _lastCell = null;
    }
}
