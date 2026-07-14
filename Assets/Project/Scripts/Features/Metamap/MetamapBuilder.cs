using System.Collections.Generic;
using UnityEngine;

public class MetamapBuilder
{
    private readonly Transform _locationsParent;
    private readonly Transform _metamapStartPoint;
    private readonly List<MetamapLocationView> _locations;
    private readonly List<Renderer> _renderers;

    public Bounds Bounds => GetBounds();

    public MetamapBuilder(Transform locationsParent, Transform metamapStartPoint)
    {
        _locationsParent = locationsParent;
        _metamapStartPoint = metamapStartPoint;
        _locations = new();
        _renderers = new();
    }

    public void CreateLocation(MetamapLocationView metamapLocationView)
    {
        MetamapLocationView location = MonoBehaviour.Instantiate(metamapLocationView, _locationsParent);

        location.transform.localPosition += CalculatePositionOffset(location.EntryPointPosition);
        _locations.Add(location);
        location.SortingGroup.sortingOrder = _locations.Count * (-1);

        if (location.Renderer == null)
        {
            throw new System.Exception("Location renderer = null");
        }

        _renderers.Add(location.Renderer);
    }

    public Bounds GetBounds()
    {
        Bounds bounds = new();

        foreach (Renderer renderer in _renderers)
        {
            bounds.Encapsulate(renderer.bounds);
        }

        return bounds;
    }

    private Vector3 CalculatePositionOffset(Transform entryPoint)
    {
        Vector3 targetPosition = _locations.Count == 0 ? _metamapStartPoint.position : _locations[^1].ExitPointPosition.position;
        return targetPosition - entryPoint.position;
    }
}
