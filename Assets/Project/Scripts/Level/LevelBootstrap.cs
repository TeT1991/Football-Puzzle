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
    private SkinLoader _skinLoader; //Нужен ли?

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
        EntityView character = CreateEntityView(
            _entityCreator,
            _levelDefenition.CharacterPosition);

        InitEntities(character);

        _cellSelector.Init((CellView[,])_cells.Clone());

        _levelProcessor.Init(character, _cellSelector, _entityRouteRegistry, _entetiesMovementProcessor);


        _disposables = new();
        _disposables.Add(_entetiesMovementProcessor);

        _levelProcessor.StartLevel(); //после всех инициализиаций
    }

    private void InitEntities(EntityView character)
    {
        //Установить скин персонажа наверное

        _entityRouteRegistry = new();
        _entityRouteRegistry.AddRoute(character, _levelDefenition.CharacterRoute);
        _entetiesMovementProcessor = new(character, _levelDefenition.Width, _levelDefenition.Height);

        EntityCreator enemyCreator = new(_enemiesParent, _entityViewPrefab);

        foreach (Route route in _levelDefenition.EnemyRoutes)
        {
            if (route == null || route.HasNodes == false)
            {
                continue;
            }

            EntityView view = CreateEntityView(enemyCreator, route.StartCoordinates);
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

    private EntityView CreateEntityView(EntityCreator entityCreator, Vector2Int coordinates)
    {
        int width = _levelDefenition.Width;
        int height = _levelDefenition.Height;

        Vector2 offsetedPosition = GameUtility.ConvertCoordinatesToPosition(coordinates, width, height);

        return entityCreator.CreateEntity(offsetedPosition, coordinates);

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
