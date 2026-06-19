using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelBootstrap : MonoBehaviour
{
    [SerializeField] private LevelDefinition _levelDefenition;
    [SerializeField] private CellView _cellViewPrefab;
    [SerializeField] private Transform _cellsParent;
    [SerializeField] private CellSelector _cellSelector;
    [SerializeField] private Transform _characterParent;
    [SerializeField] private Transform _enemiesParent;
    [SerializeField] private EntityView _entityViewPrefab;
    [SerializeField] private LevelProcessor _levelProcessor;

    private GridBuilder _gridBuilder;
    private EntityCreator _entityCreator;
    private EntityRouteRegistry _entityRouteRegistry;
    private EntetiesMovementProcessor _entetiesMovementProcessor;
    private SkinLoader _skinLoader; //Ќужен ли?

    private CellView[,] _cells;

    private List<IDisposable> _disposables;

    private void Awake()
    {
        LevelData levelData = GenerateLevelData();

        _skinLoader = new();
        _skinLoader.Load();

        _cells = new CellView[levelData.Width, levelData.Height];
        _gridBuilder = new(_cellsParent, _cellViewPrefab);
        _cells = _gridBuilder.CreateTiles(levelData.Width, levelData.Height);

        _entityCreator = new(_characterParent, _entityViewPrefab);
        EntityView character = CreateEntityView();

        InitEntities(character);

        _cellSelector.Init((CellView[,])_cells.Clone());

        _levelProcessor.Init(character, _cellSelector, _entityRouteRegistry, _entetiesMovementProcessor);


        _disposables = new();
        _disposables.Add(_entetiesMovementProcessor);

        _levelProcessor.StartLevel(); //после всех инициализиаций
    }

    private void InitEntities(EntityView character)
    {
        //”становить скин персонажа наверное

        List<Route> routes = new List<Route>();

        _entityRouteRegistry = new();
        _entityRouteRegistry.AddRoute(character, _levelDefenition.CharacterRoute);
        _entetiesMovementProcessor = new(character);

        routes = (List<Route>)_levelDefenition.EnemyRoutes;

        foreach (Route route in routes)
        {
            EntityView view = CreateEntityView();
            _entityRouteRegistry.AddRoute(view, route);
            _entetiesMovementProcessor.AddEnemiesRoutes(view, route);
        }
    }

    private LevelData GenerateLevelData()
    {
        int width = _levelDefenition.Width;
        int height = _levelDefenition.Height;
        Vector2Int characterStartPosition = _levelDefenition.CharacterPosition;

        return new(width, height, characterStartPosition);
    }

    private EntityView CreateEntityView()
    {
        float gridOffsetX = GameUtility.CalculateGridOffset(_levelDefenition.Width);
        float gridOffsetY = GameUtility.CalculateGridOffset(_levelDefenition.Height);

        float xPosition = _levelDefenition.CharacterPosition.x - gridOffsetX;
        float yPosition = _levelDefenition.CharacterPosition.y - gridOffsetY;
        Vector2 offsetedPosition = new(xPosition, yPosition);

        return _entityCreator.CreateEntity(offsetedPosition, _levelDefenition.CharacterPosition);

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