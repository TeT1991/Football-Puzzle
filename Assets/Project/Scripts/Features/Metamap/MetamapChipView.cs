using System.Collections;
using UnityEngine;

public class MetamapChipView : MonoBehaviour
{
    private Coroutine _movementRoutine;

    public void SetPosition(Vector3 position)
    {
        StopMovement();

        position.z = transform.position.z;
        transform.position = position;
    }

    public void MoveTo(Vector3 targetPosition, float duration)
    {
        StopMovement();

        targetPosition.z = transform.position.z;
        _movementRoutine = StartCoroutine(MoveRoutine(targetPosition, duration));
    }

    private IEnumerator MoveRoutine(Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;

        if (duration <= 0f)
        {
            transform.position = targetPosition;
            _movementRoutine = null;
            yield break;
        }

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float progress = Mathf.Clamp01(elapsedTime / duration);

            transform.position = Vector3.Lerp(startPosition, targetPosition, progress);

            yield return null;
        }

        transform.position = targetPosition;
        _movementRoutine = null;
    }

    private void StopMovement()
    {
        if (_movementRoutine == null)
        {
            return;
        }

        StopCoroutine(_movementRoutine);
        _movementRoutine = null;
    }
}