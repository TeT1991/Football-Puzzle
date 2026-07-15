using System;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUIView : MonoBehaviour
{
    [SerializeField] private Button _skinsShopButton;

    public event Action SkinsShopButtonClicked;

    private void OnDestroy()
    {
        _skinsShopButton.onClick.RemoveAllListeners();
    }

    public void Init()
    {
        _skinsShopButton.onClick.AddListener(OnSkinsShopButtonClicked);
    }

    private void OnSkinsShopButtonClicked()
    {
        SkinsShopButtonClicked?.Invoke();
    }
}
