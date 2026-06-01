using UnityEngine;
using UnityEngine.SceneManagement;

public class GlobalStateProcessor : MonoBehaviour
{
    private IGlobalGameStateService _gloabalGameStateService;

    public void OnDestroy()
    {
        _gloabalGameStateService.GlobalStateChanged -= ProcessGlobalGameState;
    }

    public void Init()
    {
        DontDestroyOnLoad(gameObject);
        _gloabalGameStateService = ServiceLocator.Get<IGlobalGameStateService>();
        _gloabalGameStateService.GlobalStateChanged += ProcessGlobalGameState;
    }

    private void ProcessGlobalGameState(GlobalGameState state)
    {
        switch (state)
        {
            case GlobalGameState.Loading:
                //Show loading screen
                break;

            case GlobalGameState.MainMenu:
                ApplyMainMenuState();
                break;

            case GlobalGameState.Level:
                ApplyLevelState();
                break;
        }
    }

    private void ApplyMainMenuState()
    {
        string mainMenuSceneName = "MainMenu";
        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void ApplyLevelState()
    {
        string mainMenulevelSceneName = "Level";
        SceneManager.LoadScene(mainMenulevelSceneName);
    }
}
