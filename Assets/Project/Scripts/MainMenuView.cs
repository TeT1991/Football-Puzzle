using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuView : MonoBehaviour
{
    [SerializeField] private Button _playButton;

    public event Action PlayButtonClicked;

    private void OnDestroy()
    {
        _playButton.onClick.RemoveAllListeners();
    }

    public void Init()
    {
        _playButton.onClick.AddListener(OnPlayButtonClicked);
    }

    private void OnPlayButtonClicked()
    {
        PlayButtonClicked?.Invoke();
    }
}
