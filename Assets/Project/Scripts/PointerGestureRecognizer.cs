using System;
using UnityEngine;

public class PointerGestureRecognizer 
{
    private readonly GameInput _gameInput;
    private readonly float _dragThreshold;

    private Vector2 _pressPosition;
    private bool _isDragging;

    public event Action<Vector2> Tapped;
    public event Action<Vector2> DragStarted;
    public event Action<Vector2> Dragged;
    public event Action<Vector2> DragEnded;

    public PointerGestureRecognizer(GameInput gameInput)
    {
        _gameInput = gameInput;
        _dragThreshold = 0.06f;
        _isDragging = false;

        _gameInput.Pressed += OnPress;
        _gameInput.Dragged += OnDragged;
        _gameInput.Released += OnReleased;
    }

    private void OnPress(Vector2 position)
    {
        _pressPosition = position;
        _isDragging = false;
    }

    private void OnDragged(Vector2 position)
    {
        if (_isDragging == false)
        {
            Vector2 delta = position - _pressPosition;

            if(delta.sqrMagnitude < _dragThreshold)
            {
                return;
            }

            _isDragging = true;
            DragStarted?.Invoke(_pressPosition);
        }

        Dragged?.Invoke(_pressPosition);
    }

    private void OnReleased(Vector2 position)
    {
        if(_isDragging)
        {
            _isDragging = false;
            DragEnded?.Invoke(_pressPosition);
        }
        else
        {
            Tapped?.Invoke(_pressPosition);
        }
    }
}
