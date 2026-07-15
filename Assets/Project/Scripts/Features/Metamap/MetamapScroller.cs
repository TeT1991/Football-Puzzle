using System;
using UnityEngine;

public class MetamapScroller : IDisposable
{
    private readonly Transform _mapRoot;
    private readonly float _dragTreshold;

    private readonly PointerGestureRecognizer _pointerGestureRecognizer;

    private readonly Camera _camera;
    private readonly int _screenHeight;
    private readonly float _minMapY;
    private readonly float _maxMapY;

    private Vector2 _startPointPosition;
    private float _startMapY;
    private bool _isDragging;

    public MetamapScroller(PointerGestureRecognizer pointerGestureRecognizer, Bounds bounds, Transform mapRoot, float dragTreshold)
    {
        _camera = Camera.main;
        _screenHeight = Screen.height;

        _pointerGestureRecognizer = pointerGestureRecognizer;

        float cameraBottom = _camera.transform.position.y - _camera.orthographicSize;
        float cameraTop = _camera.transform.position.y + _camera.orthographicSize;

        _mapRoot = mapRoot;
        _dragTreshold = dragTreshold;

        _minMapY = _mapRoot.position.y + cameraTop - bounds.max.y;
        _maxMapY = _mapRoot.position.y + cameraBottom - bounds.min.y;

        _pointerGestureRecognizer.DragStarted += OnInputPressed;
        _pointerGestureRecognizer.Dragged += OnInputDragged;
    }

    private void OnInputPressed(Vector2 postion)
    {
        _startPointPosition = postion;
        _startMapY = _mapRoot.position.y;
        _isDragging = false;
    }

    public void OnInputDragged(Vector2 position)
    {
        Vector2 deltaY = position - _startPointPosition;

        if (deltaY.magnitude < _dragTreshold)
        {
            return;
        }

        _isDragging = true;

        float worldPerPixel = (_camera.orthographicSize * 2f) / _screenHeight;

        Vector3 newMapPosition = _mapRoot.position;

        newMapPosition.y = Mathf.Clamp(_startMapY + deltaY.y * worldPerPixel, _minMapY, _maxMapY);
        _mapRoot.position = newMapPosition;
    }

    public void Dispose()
    {
        _pointerGestureRecognizer.DragStarted -= OnInputPressed;
        _pointerGestureRecognizer.Dragged -= OnInputDragged;
    }
}
