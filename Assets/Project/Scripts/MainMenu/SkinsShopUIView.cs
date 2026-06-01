using System;
using UnityEngine;
using UnityEngine.UI;


public class SkinsShopUIView : MonoBehaviour
{
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _closeBackgroundButton;

    public event Action CloseButtonCliked;

    private void OnDestroy()
    {
        _closeButton.onClick.RemoveAllListeners();
    }

    public void Init()
    {
        _closeButton.onClick.AddListener(OnCloseButtonCliked);
        _closeBackgroundButton.onClick.AddListener(OnCloseButtonCliked);
    }

    private void OnCloseButtonCliked()
    {
        CloseButtonCliked?.Invoke();
    }
}
