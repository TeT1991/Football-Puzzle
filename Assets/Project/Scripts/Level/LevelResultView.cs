using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelResultView : MonoBehaviour
{
    [SerializeField] private  List<StarView> _stars;
    [SerializeField] private Button _nextLevelButton;
    [SerializeField] private Button _resetLevelButton;

    public event Action NextLevelButtonPressed;
    public event Action ResetLevelButtonPressed;

    private void OnDestroy()
    {
        _nextLevelButton.onClick.RemoveAllListeners();
        _resetLevelButton.onClick.RemoveAllListeners();
    }

    public void Init()
    {
        foreach(StarView star in _stars)
        {
            star.HideStar();
        }

        _nextLevelButton.onClick.AddListener(OnNextLevelButtonPressed);
        _resetLevelButton.onClick.AddListener(OnResetLevelButtonPressed);
        
    }

    public void ShowStars(int count)
    {
        for (int i = 0; i < count; i++)
        {
            _stars[i].ShowStar();
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
}