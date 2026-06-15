using System.Collections;
using UnityEngine;

public class EntityMover 
{
    private EntityView _view;

    public EntityMover(EntityView view)
    {
        _view = view;
    }

    public IEnumerator MoveTo(Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = _view.transform.position;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            _view.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            yield return null;
        }

        _view.transform.position = targetPosition;
    }
}
