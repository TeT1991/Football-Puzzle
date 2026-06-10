using System;

public interface IGlobalGameStateService : IService
{
    event Action<GlobalGameState> GlobalStateChanged;
    GlobalGameState CurrentState { get; }
    void SetState(GlobalGameState state);
}
