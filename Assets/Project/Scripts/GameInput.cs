using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class GameInput : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    public event Action<Vector2> Pressed;
    public event Action<Vector2> Dragged;
    public event Action<Vector2> Released;

    public void OnPointerDown(PointerEventData eventData)
    {
        Pressed?.Invoke(eventData.position);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Dragged?.Invoke(eventData.position);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Released?.Invoke(eventData.position);
    }
}
