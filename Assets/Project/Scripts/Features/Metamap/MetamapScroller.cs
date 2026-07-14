using UnityEngine;

public class MetamapScroller : MonoBehaviour
{
    [SerializeField] private Transform _mapRoot;
    [SerializeField] private float _dragTreshold;

    private GameInput _gameInput;

    private Camera _camera;
    private int _screenHeight;

    private Vector2 _startPointPosition;
    private float _startMapY;
    private bool _isDragging;
    private float _minMapY;
    private float _maxMapY;

    public void Init(GameInput gameInput, Bounds bounds)
    {
        _camera = Camera.main;
        _screenHeight = Screen.height;

        _gameInput = gameInput;

        float cameraBottom = _camera.transform.position.y - _camera.orthographicSize;
        float cameraTop = _camera.transform.position.y + _camera.orthographicSize;

        _minMapY = _mapRoot.position.y + cameraTop - bounds.max.y;
        _maxMapY = _mapRoot.position.y + cameraBottom - bounds.min.y;

        _gameInput.Pressed += OnInputPressed;
        _gameInput.Dragged += OnInputDragged;
        _gameInput.Released += OnInputReleased;
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

    public void OnInputReleased(Vector2 _)
    {
        
    }

    private void OnDestroy()
    {
        _gameInput.Pressed -= OnInputPressed;
        _gameInput.Dragged -= OnInputDragged;
        _gameInput.Released -= OnInputReleased;
    }
}
