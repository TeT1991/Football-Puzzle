using UnityEngine;

public class MainMenuBootstrap : MonoBehaviour
{
    [SerializeField] private GlobalStateProcessor _globalStateProcessor;
    [SerializeField] private MainMenuView _mainMenuView;

    private MainMenuModel _mainMenuModel;

    private void Awake()
    {
        _globalStateProcessor.Init();
        _mainMenuView.Init();
        _mainMenuModel = new(_mainMenuView);
    }
}
