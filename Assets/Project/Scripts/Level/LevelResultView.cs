using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelResultView : MonoBehaviour
{
    [SerializeField] private List<StarView> _stars;
    [SerializeField] private Button _nextLevelButton;
    [SerializeField] private Button _resetLevelButton;
    [SerializeField] private Button _mainMenuButton;
    [SerializeField] private TextMeshProUGUI _resultText;
    [SerializeField] private string _winMessage;
    [SerializeField] private string _looseMessage;

    public event Action NextLevelButtonPressed;
    public event Action ResetLevelButtonPressed;
    public event Action MainMenuButtonPressed;

    private void OnDestroy()
    {
        _nextLevelButton.onClick.RemoveAllListeners();
        _resetLevelButton.onClick.RemoveAllListeners();
        _mainMenuButton.onClick.RemoveAllListeners();
    }

    public void Init()
    {
        foreach (StarView star in _stars)
        {
            star.HideStar();
        }

        _nextLevelButton.onClick.AddListener(OnNextLevelButtonPressed);
        _resetLevelButton.onClick.AddListener(OnResetLevelButtonPressed);
        _mainMenuButton.onClick.AddListener(OnMainMenuButtonPressed);
    }

    public void ShowStars(int count)
    {
        for (int i = 0; i < count; i++)
        {
            _stars[i].ShowStar();
        }
    }

    public void SetResultText(LevelResult result)
    {
        switch (result)
        {
            case LevelResult.Win:
                _resultText.text = _winMessage;
                break;

            case LevelResult.Lose:
                _resultText.text = _looseMessage; 
                break;
        }
    }

    private void OnNextLevelButtonPressed()
    {
        NextLevelButtonPressed?.Invoke();
    }

    private void OnResetLevelButtonPressed()
    {
        ResetLevelButtonPressed?.Invoke();
    }

    private void OnMainMenuButtonPressed()
    {
        MainMenuButtonPressed?.Invoke();
    }
}