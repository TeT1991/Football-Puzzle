using UnityEngine;

public class LevelBootstrap : MonoBehaviour
{
    [SerializeField] private LevelDefinition _levelDefenition;
    [SerializeField] private CellView _cellViewPrefab;
    [SerializeField] private Transform _cellsParent;
    [SerializeField] private CellSelector _cellSelector;
    [SerializeField] private Transform _characterParent;
    [SerializeField] private EntityView _entityViewPrefab;
    [SerializeField] private LevelProcessor _levelProcessor;

    private GridBuilder _gridBuilder;
    private EntityCreator _entityCreator;
    private SkinLoader _skinLoader; //Нужен ли?

    private CellView[,] _cells;

    private void Awake()
    {
        LevelData levelData = GenerateLevelData();

        _skinLoader = new();
        _skinLoader.Load();

        _cells = new CellView[levelData.Width, levelData.Height];
        _gridBuilder = new(_cellsParent, _cellViewPrefab);
        _cells = _gridBuilder.CreateTiles(levelData.Width, levelData.Height);

        _entityCreator = new(_characterParent, _entityViewPrefab);
        CreateCharacter(out EntityView character);

        _levelProcessor.Init(character ,_cellSelector);
    }

    private LevelData GenerateLevelData()
    {
        int width = _levelDefenition.Width;
        int height = _levelDefenition.Height;
        Vector2Int characterStartPosition = _levelDefenition.CharacterPosition;

        return new(width, height, characterStartPosition);
    }

    private void CreateCharacter(out EntityView character)
    {
        float gridOffsetX = GameUtility.CalculateGridOffset(_levelDefenition.Width);
        float gridOffsetY = GameUtility.CalculateGridOffset(_levelDefenition.Height);

        float xPosition = _levelDefenition.CharacterPosition.x - gridOffsetX;
        float yPosition = _levelDefenition.CharacterPosition.y - gridOffsetY;
        Vector2 offsetedPosition = new Vector2(xPosition, yPosition);

        character = _entityCreator.CreateEntity(offsetedPosition);
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

    public EntityView CreateEntity(Vector2 position)
    {
        EntityView entity = MonoBehaviour.Instantiate(_character);
        entity.transform.SetParent(_entitiesParent, false);
        entity.transform.position = position;
        entity.Init();

        return entity;
    }
}