using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIView : MonoBehaviour
{
    [SerializeField] private Button _playButton;
    [SerializeField] private Button _skinsShopButton;
    [SerializeField] private Button _levelSelectionButton;

    public event Action PlayButtonClicked;
    public event Action SkinsShopButtonClicked;

    private void OnDestroy()
    {
        _playButton.onClick.RemoveAllListeners();
        _skinsShopButton.onClick.RemoveAllListeners();
        _levelSelectionButton.onClick.RemoveAllListeners();
    }

    public void Init()
    {
        _playButton.onClick.AddListener(OnPlayButtonClicked);
        _skinsShopButton.onClick.AddListener(OnSkinsShopButtonClicked);
    }

    private void OnPlayButtonClicked()
    {
        PlayButtonClicked?.Invoke();
    }

    private void OnSkinsShopButtonClicked()
    {
        SkinsShopButtonClicked?.Invoke();
    }
}
