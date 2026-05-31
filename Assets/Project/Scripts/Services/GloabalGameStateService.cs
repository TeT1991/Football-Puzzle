using System;

public class GloabalGameStateService : IGloabalGameStateService
{
    private GlobalGameState _currentState;

    public GloabalGameStateService(GlobalGameState state)
    {
        SetState(state);
    }

    public event Action GlobalStateChanged;

    public GlobalGameState CurrentState => _currentState;

    public void SetState(GlobalGameState state)
    {
        _currentState = state;
        GlobalStateChanged?.Invoke();
    }
}
