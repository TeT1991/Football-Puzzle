using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelSelector
{
    private readonly Camera _camera;
    private readonly PointerGestureRecognizer _pointerGestureRecognizer;
    private readonly List<MetamapLevelMarker> _markers;

    private LayerMask _markersLayerMask;

    public event Action<MetamapLevelData> LevelMarkerClicked;
    public LevelSelector(PointerGestureRecognizer pointerGestureRecognizer, LayerMask markersLayerMask)
    {
        _camera = Camera.main;
        _markers = new();

        _pointerGestureRecognizer = pointerGestureRecognizer;
        _markersLayerMask = markersLayerMask;

        _pointerGestureRecognizer.Tapped += OnTapped;
    }

    private void OnTapped(Vector2 screenPosition)
    {
        Vector3 worldPosition = _camera.ScreenToWorldPoint(screenPosition);

        Collider2D hit = Physics2D.OverlapPoint(worldPosition, _markersLayerMask);

        if (hit == null)
        {
            Debug.Log("No marker here");
            return;
        }

        if (hit.TryGetComponent<MetamapLevelMarker>(out MetamapLevelMarker marker))
        {
            LevelMarkerClicked.Invoke(marker.LevelData);
        }
    }
}