using UnityEngine;
using UnityEngine.EventSystems;

public class MetamapInput : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] private Transform _mapRoot;
    [SerializeField] private float _dragTreshold;

    private Camera _camera = Camera.main;
    private int _screenHeight;

    private Vector2 _startPointPosition;
    private float _startMapY;
    private bool _isDragging;
    private float _minMapY;
    private float _maxMapY;

    public void Init(Bounds bounds)
    {
        _camera = Camera.main;
        _screenHeight = Screen.height;

        float cameraBottom = _camera.transform.position.y - _camera.orthographicSize;
        float cameraTop = _camera.transform.position.y + _camera.orthographicSize;

        _minMapY = _mapRoot.position.y + cameraTop - bounds.max.y;
        _maxMapY = _mapRoot.position.y + cameraBottom - bounds.min.y;
    }

    public void OnDrag(PointerEventData eventData)
    {
       Vector2 deltaY = eventData.position - _startPointPosition;

        if(deltaY.magnitude < _dragTreshold)
        {
            return;
        }

        _isDragging = true;

        float worldPerPixel = (_camera.orthographicSize * 2f) / _screenHeight;

        Vector3 position = _mapRoot.position;

        position.y = Mathf.Clamp(_startMapY + deltaY.y * worldPerPixel, _minMapY, _maxMapY);
        _mapRoot.position = position;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        _startPointPosition = eventData.position;
        _startMapY = _mapRoot.position.y;
        _isDragging = false;
    }
}
