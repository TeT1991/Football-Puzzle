using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemView : MonoBehaviour
{
    [SerializeField] private Button _skinButton;
    [SerializeField] private Button _buyButton;
    [SerializeField] private TextMeshProUGUI _priceText;
    [SerializeField] private TextMeshProUGUI _skinSprite; //потом поменять на спрайт
    [SerializeField] private RectTransform _markAsSelectedObject;

    private int _id;
    private float _price;

    public event Action<int> SkinButtonClicked;
    public event Action<int> BuyButtonClicked;

    public int Id => _id;
    public float Price => _price;

    public void Init( ShopItemData data)
    {
        _id = data.Id;
        _price = data.Price;

        _skinButton.interactable = data.IsUnlocked;
        _buyButton.gameObject.SetActive(data.IsUnlocked == false);

        _skinSprite.text = _id.ToString();
        _priceText.text = _price.ToString();

        _skinButton.onClick.AddListener(OnSkinButtonClicked);
        _buyButton.onClick.AddListener(OnBuyButtonClicked);

    }

    public void ShowSelectedMark()
    {
        _markAsSelectedObject.gameObject.SetActive(true);
    }

    public void HideSelectedMark()
    {
        _markAsSelectedObject.gameObject.SetActive(false);
    }

    private void OnBuyButtonClicked()
    {
        BuyButtonClicked?.Invoke(_id);
    }

    private void OnSkinButtonClicked()
    {
        SkinButtonClicked?.Invoke(_id);
    }

    private void OnDestroy()
    {
        _skinButton.onClick.RemoveListener(OnSkinButtonClicked);
        _buyButton.onClick.RemoveListener(OnBuyButtonClicked);
    }
}
