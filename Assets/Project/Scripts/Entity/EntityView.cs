using System;
using UnityEngine;

public class EntityView : MonoBehaviour
{
    private Vector2Int _currentCoordinates; //¬ременное поле дш€ тестов

    private EntityMover _entityMover;
    private Coroutine _coroutine;
    private float _animationDuration;

    public event Action<EntityView> TargetPositionReached;

    public Vector2Int CurrentCoordinates => _currentCoordinates;

    private void OnDestroy()
    {
        _entityMover.TargetPositionReached -= OnTargetPositionReached;
    }

    public void Init(Vector2Int coordinates)
    {
        _entityMover = new(this);
        _animationDuration = 1f;
        SetCurrentCoordinates(coordinates);

        _entityMover.TargetPositionReached += OnTargetPositionReached;
    }

    public void StartMove(Vector3 targerPosition)
    {
        _coroutine = StartCoroutine(_entityMover.MoveTo(targerPosition, _animationDuration));
    }

    public void SetCurrentCoordinates(Vector2Int coordinates)
    {
        _currentCoordinates = coordinates;
    }

    private void OnTargetPositionReached()
    {
        TargetPositionReached?.Invoke(this);
    }
}
