using System;
using UnityEditor;
using UnityEngine;

internal sealed class PlayerPathEditingController
{
    private readonly LevelEditorTool _tool;
    private readonly LevelEditorSceneQuery _sceneQuery;
    private readonly Action _repaint;

    private bool _isDragging;
    private int _dragButton = -1;
    private int _undoGroup = -1;
    private CellView _lastCell;

    public PlayerPathEditingController(
        LevelEditorTool tool,
        LevelEditorSceneQuery sceneQuery,
        Action repaint)
    {
        _tool = tool;
        _sceneQuery = sceneQuery;
        _repaint = repaint;
    }

    public bool IsActive { get; private set; }

    public bool Start()
    {
        _tool.ApplySceneToDefinition();

        LevelDefinition definition = _tool.LevelDefinition;

        if (definition == null)
        {
            Debug.LogWarning(
                "Assign or create a LevelDefinition before drawing a player path.");
            return false;
        }

        if (definition.HasCharacter == false)
        {
            Debug.LogWarning("Place the player before drawing a player path.");
            return false;
        }

        if (_sceneQuery.TryGetCellByCoordinates(
                definition.CharacterPosition,
                out _) == false)
        {
            _tool.CreateLevel();

            if (_sceneQuery.TryGetCellByCoordinates(
                    definition.CharacterPosition,
                    out _) == false)
            {
                Debug.LogWarning(
                    "The player start cell is not available in the scene grid.");
                return false;
            }
        }

        PlayerPathEditorUtility.EnsureNode(
            definition,
            definition.CharacterPosition);

        IsActive = true;
        ResetDrag();
        _repaint();
        return true;
    }

    public void Stop()
    {
        if (IsActive == false && _isDragging == false)
        {
            return;
        }

        EndDrag();
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

        if (currentEvent.alt)
        {
            return;
        }

        if (currentEvent.type == EventType.MouseDown &&
            (currentEvent.button == 0 || currentEvent.button == 1))
        {
            if (_sceneQuery.TryGetCellAtGuiPoint(
                    currentEvent.mousePosition,
                    out CellView cell) &&
                CanBeginDrag(cell, currentEvent.button))
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

    private bool CanBeginDrag(CellView cell, int mouseButton)
    {
        LevelDefinition definition = _tool.LevelDefinition;

        if (definition == null || cell == null)
        {
            return false;
        }

        bool containsNode = PlayerPathEditorUtility.ContainsNode(
            definition,
            cell.Coordinates);

        if (mouseButton == 1 || containsNode)
        {
            return containsNode;
        }

        Debug.LogWarning(
            "Start a new player path segment from the player start or an existing path node.");
        return false;
    }

    private void BeginDrag(CellView cell, int mouseButton)
    {
        Undo.IncrementCurrentGroup();
        _undoGroup = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName(
            mouseButton == 0 ? "Draw Player Path" : "Erase Player Path");

        _isDragging = true;
        _dragButton = mouseButton;
        _lastCell = cell;
    }

    private void HandleDrag(CellView cell)
    {
        if (cell == null || _lastCell == null || cell == _lastCell)
        {
            return;
        }

        Vector2Int previousCoordinates = _lastCell.Coordinates;
        Vector2Int currentCoordinates = cell.Coordinates;
        Vector2Int difference = currentCoordinates - previousCoordinates;

        if (Mathf.Abs(difference.x) + Mathf.Abs(difference.y) != 1)
        {
            return;
        }

        LevelDefinition definition = _tool.LevelDefinition;
        bool changed;

        if (_dragButton == 0)
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
                _lastCell = cell;
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
                _lastCell = cell;
            }
        }

        if (changed)
        {
            _repaint();
        }
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
