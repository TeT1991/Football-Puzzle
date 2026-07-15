using System;

public class GlobalGameStateService : IGlobalGameStateService
{
    private GlobalGameState _currentState;

    public event Action<GlobalGameState> GlobalStateChanged;

    public GlobalGameState CurrentState => _currentState;

    public void SetState(GlobalGameState state)
    {
        _currentState = state;
        GlobalStateChanged?.Invoke(_currentState);
    }
}
