using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelBootstrap : MonoBehaviour
{
    [SerializeField] private GameInput _gameInput;

    [SerializeField] private LevelDefinition _levelDefenition;
    [SerializeField] private CellView _cellViewPrefab;
    [SerializeField] private Transform _cellsParent;
    [SerializeField] private CellSelector _cellSelector;
    [SerializeField] private Transform _characterParent;
    [SerializeField] private Transform _enemiesParent;
    [SerializeField] private EntityView _entityViewPrefab;
    [SerializeField] private LevelProcessor _levelProcessor;
    [SerializeField] private RouteNodeView _routeNodeViewPrefab;
    [SerializeField] private Color _characterRouteColor;
    [SerializeField] private Color _enemiesRouteColor;
    [SerializeField] private GoalMarkerView _goalMarkerViewPrefab;
    [SerializeField] private LevelResultView _levelResultView;

    private GridBuilder _gridBuilder;
    private EntityCreator _entityCreator;
    private EntityRouteRegistry _entityRouteRegistry;
    private EntetiesMovementProcessor _entetiesMovementProcessor;
    private RoutesRenderer _routesRenderer;
    private SkinLoader _skinLoader; //Нужен ли?
    private LevelResultPresenter _levelResultPresenter;
    private PointerGestureRecognizer _pointerGestureRecognizer;

    private CellView[,] _cells;

    private List<IDisposable> _disposables;

    private void Awake()
    {
        LevelData levelData = GenerateLevelData();
        _pointerGestureRecognizer = new(_gameInput);


        _skinLoader = new();
        _skinLoader.Load();

        _cells = new CellView[levelData.Width, levelData.Height];
        _gridBuilder = new(_cellsParent, _cellViewPrefab);
        _cells = _gridBuilder.CreateTiles(levelData.Width, levelData.Height);

        _entityCreator = new(_characterParent, _entityViewPrefab);
        EntityView character = CreateEntityView( _entityCreator, _levelDefenition.CharacterPosition);

        InitEntities(character);

        _cellSelector.Init((CellView[,])_cells.Clone(), _pointerGestureRecognizer);

        _levelProcessor.Init(character, _cellSelector, _entityRouteRegistry, _entetiesMovementProcessor, _levelDefenition.GoalCoordinates);

        _routesRenderer = new(_routeNodeViewPrefab, _cells);
        DrawRoutes();
        SetGoalMarker();

        _levelResultPresenter = new(_levelResultView);
        _levelProcessor.LevelEnded += _levelResultPresenter.ApplyResultActions;

        _disposables = new()
        {
            _entetiesMovementProcessor,
            _levelResultPresenter
        };

        _levelProcessor.StartLevel(); //после всех инициализиаций
    }

    private void InitEntities(EntityView character)
    {
        //Установить скин персонажа наверное

        _entityRouteRegistry = new();
        _entityRouteRegistry.AddRoute(character, _levelDefenition.CharacterRoute);
        _entetiesMovementProcessor = new(character, _levelDefenition.Width, _levelDefenition.Height);
        _entetiesMovementProcessor.AddEntityRoutes(character, _levelDefenition.CharacterRoute);

        EntityCreator entityCreator = new(_enemiesParent, _entityViewPrefab);


        foreach (Route route in _levelDefenition.EnemyRoutes)
        {
            if (route == null || route.HasNodes == false)
            {
                continue;
            }

            EntityView view = CreateEntityView(entityCreator, route.StartCoordinates);
            _entityRouteRegistry.AddRoute(view, route);
            _entetiesMovementProcessor.AddEntityRoutes(view, route);
        }
    }

    private void DrawRoutes()
    {
        _routesRenderer.CreateRoutes(_levelDefenition.CharacterRoute.RouteNodes, _characterRouteColor);

        foreach (Route route in _levelDefenition.EnemyRoutes)
        {
            _routesRenderer.CreateRoutes(route.RouteNodes, _enemiesRouteColor);
        }
    }

    private LevelData GenerateLevelData()
    {
        int width = _levelDefenition.Width;
        int height = _levelDefenition.Height;
        Vector2Int characterStartPosition = _levelDefenition.CharacterPosition;

        return new(width, height, characterStartPosition);
    }

    private EntityView CreateEntityView(EntityCreator entityCreator, Vector2Int coordinates)
    {
        int width = _levelDefenition.Width;
        int height = _levelDefenition.Height;

        Vector2 offsetedPosition = GameUtility.ConvertCoordinatesToPosition(coordinates, width, height);

        return entityCreator.CreateEntity(offsetedPosition, coordinates);

    }

    private void SetGoalMarker()
    {
        Vector2Int goal = _levelDefenition.GoalCoordinates;
        int width = _levelDefenition.Width;
        int height = _levelDefenition.Height;
        _goalMarkerViewPrefab.transform.position = GameUtility.ConvertCoordinatesToPosition(goal, width, height);
    }

    private void OnDestroy()
    {
        foreach (IDisposable disposable in _disposables)
        {
            disposable.Dispose();
        }
    }
}

public class EntityCreator
{
    private readonly Transform _entitiesParent;
    private readonly EntityView _character;

    public EntityCreator(Transform entitiesPArent, EntityView entityPrefab)
    {
        _entitiesParent = entitiesPArent;
        _character = entityPrefab;
    }

    public EntityView CreateEntity(Vector2 position, Vector2Int coordinates)
    {
        EntityView entity = MonoBehaviour.Instantiate(_character);
        entity.transform.SetParent(_entitiesParent, false);
        entity.transform.position = position;
        entity.Init(coordinates);

        return entity;
    }
}
