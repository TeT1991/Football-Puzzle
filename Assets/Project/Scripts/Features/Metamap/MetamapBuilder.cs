using System.Collections.Generic;
using UnityEngine;

public class MetamapBuilder
{
    private readonly LeaguesCatalog _catalog;
    private readonly Transform _locationsParent;
    private readonly Transform _metamapStartPoint;
    private readonly Transform _markersParent;
    private readonly List<MetamapLocationView> _locations;
    private readonly List<MetamapLevelMarker> _markers;
    private readonly List<Renderer> _renderers;
    private readonly MetamapLevelMarker _metamapLevelMarkerPrefab;

    public Bounds Bounds => GetBounds();

    public MetamapBuilder(MetamapLevelMarker metamapLevelMarker, Transform locationsParent, Transform metamapStartPoint, Transform markersParent)
    {
        _metamapLevelMarkerPrefab = metamapLevelMarker;
        _locationsParent = locationsParent;
        _metamapStartPoint = metamapStartPoint;
        _markersParent = markersParent;
        _locations = new();
        _renderers = new();
        _markers = new();
    }

    public void BuildMetamap()
    {
        foreach (LeagueDefinition league in _catalog.Catalog)
        {
            CreateLocation(league.MetamapLocationView);

            //CreateLevelMarkers()
        }
    }

    public MetamapLocationView CreateLocation(MetamapLocationView metamapLocationView)
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

        return location;
    }

    public void CreateLevelMarker(MetamapLevelData data, Transform position)
    {
        MetamapLevelMarker marker = MonoBehaviour.Instantiate(_metamapLevelMarkerPrefab, _markersParent);
        marker.Init(data);
        marker.transform.parent = position;
        marker.transform.localPosition = Vector3.zero;
        _markers.Add(marker);
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

    private void PlaceMarkers()
    {

    }

    private Vector3 CalculatePositionOffset(Transform entryPoint)
    {
        Vector3 targetPosition = _locations.Count == 0 ? _metamapStartPoint.position : _locations[^1].ExitPointPosition.position;
        return targetPosition - entryPoint.position;
    }
}
