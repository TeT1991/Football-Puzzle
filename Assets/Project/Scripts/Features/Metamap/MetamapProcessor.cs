using System;
using System.Collections.Generic;
using UnityEngine;

public class MetamapProcessor : MonoBehaviour
{
    [SerializeField] private Transform _metamapRoot;
    [SerializeField] private Transform _metamapStartPoint;
    [SerializeField] private Transform _markersParent;
    [SerializeField] private MetamapLevelMarker _levelMarkerPrefab;
    [SerializeField] private float _dragThreshold;
    [SerializeField] private LayerMask _markersLayerMask;

    private LevelSelector _levelSelector;
    private LeaguesCatalog _leaguesCatalog;
    private MetamapBuilder _metamapBuilder;
    private MetamapScroller _metaMapScroller;
    private ILevelSelectionService _levelSelectionService;
    private IGlobalGameStateService _globalGameStateService;

    private PointerGestureRecognizer _pointerGestureRecognizer;
    private List<IDisposable> _disposables;

    public event Action<LevelDefinition> LevelSelected;


    public void Init(PointerGestureRecognizer pointerGestureRecognizer, LeaguesCatalog leaguesCatalog)
    {
        _levelSelectionService = ServiceLocator.Get<ILevelSelectionService>();
        _globalGameStateService = ServiceLocator.Get<IGlobalGameStateService>();

        _pointerGestureRecognizer = pointerGestureRecognizer;
        _leaguesCatalog = leaguesCatalog;

        _metamapBuilder = new(_levelMarkerPrefab, _metamapRoot, _metamapStartPoint, _markersParent);
        CreateMetamap();
        _levelSelector = new(_pointerGestureRecognizer, _markersLayerMask);
        _metaMapScroller = new(pointerGestureRecognizer, _metamapBuilder.Bounds, _metamapRoot, _dragThreshold);

        _levelSelector.LevelMarkerClicked += OnLevelSelected;
        _disposables = new()
        {
            _metaMapScroller
        };
    }

    private void CreateMetamap()
    {
        for (int i = 0; i < _leaguesCatalog.Catalog.Count; i++)
        {
            MetamapLocationView locationPrefab = _leaguesCatalog.Catalog[i].MetamapLocationView;
            MetamapLocationView location = _metamapBuilder.CreateLocation(locationPrefab);

            for (int j = 0; j < _leaguesCatalog.Catalog[i].Levels.Count; j++)
            {
                Transform parent = location.LevelMarkerPoints.GetPoints()[j].transform;
                MetamapLevelData data = GenerateLevelData(i, j);

                _metamapBuilder.CreateLevelMarker(data, parent);
            }
        }
    }

    private void OnLevelSelected(MetamapLevelData data)
    {
        LeagueDefinition league = _leaguesCatalog.Catalog[data._leagueIndex];
        LevelDefinition level = league.Levels[data._levelIndex];

        if (_levelSelectionService.TrySelect(level))
        {
            _globalGameStateService.SetState(GlobalGameState.Level);
        }
    }

    private MetamapLevelData GenerateLevelData(int leagueIndex, int levelIndex)
    {
        return new(leagueIndex, levelIndex);
    }

    private void OnDestroy()
    {
        foreach (IDisposable disposable in _disposables)
        {
            disposable.Dispose();
        }
    }
}
