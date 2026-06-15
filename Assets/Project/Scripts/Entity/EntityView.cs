using UnityEngine;

public class EntityView : MonoBehaviour
{
    private Vector2Int _currentCoordinates; //¬ременное поле дш€ тестов

    private EntityMover _entityMover;
    private Coroutine _coroutine;
    private float _animationDuration;

    public Vector2Int CurrentCoordinates => _currentCoordinates;

    public void Init(Vector2Int coordinates)
    {
        _entityMover = new(this);
        _animationDuration = 1f;
        SetCurrentCoordinates(coordinates);
    }

    public void StartMove(Vector3 targerPosition)
    {
        _coroutine = StartCoroutine(_entityMover.MoveTo(targerPosition, _animationDuration));
    }

    public void SetCurrentCoordinates(Vector2Int coordinates)
    {
        _currentCoordinates = coordinates;
    }
}
