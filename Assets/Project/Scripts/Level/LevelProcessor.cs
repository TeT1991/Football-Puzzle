using System;
using System.Collections;
using UnityEngine;

public class LevelProcessor : MonoBehaviour
{
    private LevelState _levelState;

    private EntityRouteRegistry _entityRouteRegistry;
    private EntetiesMovementProcessor _entetiesMovementProcessor;
    private StarsCollector _starsCollector;
    private EntityView _characterView;
    private Vector2Int _goalCoordinates;

    private CellSelector _cellSelector;

    private CellView[,] _cells;

    public event Action<Vector2Int> CharacterMovementStopped;
    public event Action<LevelCompletionData> LevelCompleted;

    private void OnDestroy()
    {
        _cellSelector.CellSelected -= TryMoveCharacter;
        _entetiesMovementProcessor.EntitiesMovementStopped -= OnEntitiesMovementStopped;
    }

    public void Init(EntityView character, CellSelector cellSelector, EntityRouteRegistry entityRouteRegistry, EntetiesMovementProcessor entetiesMovementProcessor,
        StarsCollector starsCollector, Vector2Int goalCoordinates)
    {
        _cellSelector = cellSelector;
        _characterView = character;
        _entityRouteRegistry = entityRouteRegistry;
        _entetiesMovementProcessor = entetiesMovementProcessor;
        _starsCollector = starsCollector;
        _goalCoordinates = goalCoordinates;

        _cellSelector.CellSelected += TryMoveCharacter;
        _entetiesMovementProcessor.EntitiesMovementStopped += OnEntitiesMovementStopped;
    }

    public void StartLevel() //Дает возможность играть когда все инициализировалось. Нужо ли?
    {
        SetLevelState(LevelState.EntitiesMoving);
    }

    private void ApplyStateActions()
    {
        switch (_levelState)
        {
            case LevelState.EntitiesMoving:
                ApplyPlayerTurnState();
                break;

            case LevelState.ResultCheck:
                GetTurnResult();
                break;

            case LevelState.Finished:
                ApplyFinishedStateResult();
                break;
        }
    }

    private void ApplyFinishedStateResult()
    {
        Debug.Log("Win!!!");
    }
    private void ApplyLoseState()
    {
        Debug.Log("Lose");
    }

    private void ApplyPlayerTurnState()
    {
        _cellSelector.StartSelecting();
    }

    private void GetTurnResult()
    {
        _starsCollector.TryCollectStar(_characterView.CurrentCoordinates);

        if (TryCompleteLevel() == false)
        {
            return;
        }

        SetLevelState(LevelState.EntitiesMoving);
    }

    private bool TryCompleteLevel()
    {
        if (_characterView.CurrentCoordinates == _goalCoordinates)
        {
            SetLevelState(LevelState.Finished);
            LevelCompletionData data = new(LevelResult.Win, _starsCollector.CollectedStarsCount);
            LevelCompleted?.Invoke(data);
            return false;
        }

        if (_entetiesMovementProcessor.IsEnemyOnCoordinates(_characterView.CurrentCoordinates))
        {
            LevelCompletionData data = new(LevelResult.Lose, 0);
            LevelCompleted?.Invoke(data);
            return false;
        }

        return true; //Дублирование. Поумать как убрать.
    }

    private void TryCollectStar()
    {

    }

    private void TryMoveCharacter(CellView cellView)
    {
        if (CanMove(cellView))
        {
            _cellSelector.StopSelecting();
            _entetiesMovementProcessor.StartEntetiesMovement(cellView.Coordinates);
        }
    }

    private bool CanMove(CellView cellView)
    {
        if (cellView == null)
        {
            return false;
        }

        if (_entityRouteRegistry.IsChainedCoordinates(_characterView, cellView.Coordinates))
        {
            return true;
        }

        Debug.Log("Not chaind cell");
        return false;
    }

    private void SetLevelState(LevelState state)
    {
        _levelState = state;
        ApplyStateActions();
    }

    private void OnEntitiesMovementStopped()
    {
        _cellSelector.ClearCurrentCell();
        SetLevelState(LevelState.ResultCheck);
    }
}

public enum LevelState
{
    Initialization,
    EntitiesMoving,
    ResultCheck,
    Finished
}

public interface ICoroutineRunner
{
    Coroutine Run(IEnumerator routine);
    void Stop(Coroutine coroutine);
}

public readonly struct LevelCompletionData
{
    public LevelResult Result { get; }
    public int StarsCount { get; }

    public LevelCompletionData(LevelResult result, int starsCount)
    {
        Result = result;
        StarsCount = starsCount;
    }
}
