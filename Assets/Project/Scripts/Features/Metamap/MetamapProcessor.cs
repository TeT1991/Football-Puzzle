using System;
using System.Collections.Generic;
using UnityEngine;

public class MetamapProcessor : MonoBehaviour
{
    [SerializeField] private Transform _metamapRoot;
    [SerializeField] private Transform _metamapStartPoint;
    [SerializeField] private Transform _markersParent;
    [SerializeField] private MetamapLevelMarker _levelMarkerPrefab;
    [SerializeField] private MetamapChipView _metamapChipView;
    [SerializeField] private float _chipMoveDuration = 0.5f;
    [SerializeField] private float _dragThreshold;
    [SerializeField] private LayerMask _markersLayerMask;

    private LevelSelector _levelSelector;
    private LeaguesCatalog _leaguesCatalog;
    private MetamapBuilder _metamapBuilder;
    private MetamapScroller _metaMapScroller;

    private ISaveService _saveService;
    private ILevelSelectionService _levelSelectionService;
    private IGlobalGameStateService _globalGameStateService;

    private PointerGestureRecognizer _pointerGestureRecognizer;
    private List<IDisposable> _disposables;

    public void Init(PointerGestureRecognizer pointerGestureRecognizer, LeaguesCatalog leaguesCatalog)
    {
        _saveService = ServiceLocator.Get<ISaveService>();
        _levelSelectionService = ServiceLocator.Get<ILevelSelectionService>();
        _globalGameStateService = ServiceLocator.Get<IGlobalGameStateService>();

        _pointerGestureRecognizer = pointerGestureRecognizer;
        _leaguesCatalog = leaguesCatalog;

        _metamapBuilder = new(_levelMarkerPrefab, _metamapRoot, _metamapStartPoint, _markersParent);

        CreateMetamap();
        PlaceChip();

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

    private void PlaceChip()
    {
        if (_metamapChipView == null)
        {
            Debug.LogWarning("Metamap chip view is not assigned.");
            return;
        }

        _metamapChipView.transform.SetParent(_metamapRoot, true);

        if (TryGetCurrentProgressLevel(out MetamapLevelData targetData) == false)
        {
            return;
        }

        if (_metamapBuilder.TryGetMarker(targetData, out MetamapLevelMarker targetMarker) == false)
        {
            return;
        }

        if (TryGetSelectedLevelData(out MetamapLevelData selectedData) &&
            IsSameLevel(selectedData, targetData) == false &&
            _metamapBuilder.TryGetMarker(selectedData, out MetamapLevelMarker selectedMarker))
        {
            _metamapChipView.SetPosition(selectedMarker.transform.position);
            _metamapChipView.MoveTo(targetMarker.transform.position, _chipMoveDuration);
            return;
        }

        _metamapChipView.SetPosition(targetMarker.transform.position);
    }

    private bool TryGetCurrentProgressLevel(out MetamapLevelData data)
    {
        for (int leagueIndex = _leaguesCatalog.Catalog.Count - 1; leagueIndex >= 0; leagueIndex--)
        {
            LeagueDefinition league = _leaguesCatalog.Catalog[leagueIndex];

            if (league.Levels.Count == 0)
            {
                continue;
            }

            int unlockedLevelCount = _saveService.GetUnlockedLevelCount(league.ID);

            if (unlockedLevelCount <= 0)
            {
                continue;
            }

            int levelIndex = Mathf.Clamp(unlockedLevelCount - 1, 0, league.Levels.Count - 1);
            data = new MetamapLevelData(leagueIndex, levelIndex);
            return true;
        }

        data = default;
        return false;
    }

    private bool TryGetSelectedLevelData(out MetamapLevelData data)
    {
        LeagueDefinition selectedLeague = _levelSelectionService.SelectedLeague;
        int selectedLevelIndex = _levelSelectionService.SelectedLevelIndex;

        if (selectedLeague == null || selectedLevelIndex < 0)
        {
            data = default;
            return false;
        }

        for (int leagueIndex = 0; leagueIndex < _leaguesCatalog.Catalog.Count; leagueIndex++)
        {
            LeagueDefinition league = _leaguesCatalog.Catalog[leagueIndex];

            if (league == selectedLeague || league.ID == selectedLeague.ID)
            {
                data = new MetamapLevelData(leagueIndex, selectedLevelIndex);
                return true;
            }
        }

        data = default;
        return false;
    }

    private void OnLevelSelected(MetamapLevelData data)
    {
        LeagueDefinition league = _leaguesCatalog.Catalog[data.LeagueIndex];

        if (_saveService.IsLevelUnlocked(league.ID, data.LevelIndex) == false)
        {
            return;
        }

        if (_levelSelectionService.TrySelect(league, data.LevelIndex))
        {
            _globalGameStateService.SetState(GlobalGameState.Level);
        }
    }

    private MetamapLevelData GenerateLevelData(int leagueIndex, int levelIndex)
    {
        return new MetamapLevelData(leagueIndex, levelIndex);
    }

    private bool IsSameLevel(MetamapLevelData first, MetamapLevelData second)
    {
        return first.LeagueIndex == second.LeagueIndex &&
               first.LevelIndex == second.LevelIndex;
    }

    private void OnDestroy()
    {
        if (_levelSelector != null)
        {
            _levelSelector.LevelMarkerClicked -= OnLevelSelected;
        }

        if (_disposables == null)
        {
            return;
        }

        foreach (IDisposable disposable in _disposables)
        {
            disposable.Dispose();
        }
    }
}