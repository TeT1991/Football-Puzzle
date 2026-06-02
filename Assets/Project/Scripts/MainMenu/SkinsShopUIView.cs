using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkinsShopUIView : MonoBehaviour
{
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _closeBackgroundButton;
    [SerializeField] private RectTransform _itemsParent;
    [SerializeField] private ShopItemView _shopItemViewPrefab;

    private List<ShopItemView> _items;

    public event Action CloseButtonCliked;


    //Добавить события реакции на нажатия кнопок айтемов
    public RectTransform ItemsParent => _itemsParent;

    private void OnDestroy()
    {
        _closeButton.onClick.RemoveAllListeners();
    }

    public void Init()
    {
        _closeButton.onClick.AddListener(OnCloseButtonCliked);
        _closeBackgroundButton.onClick.AddListener(OnCloseButtonCliked);

        _items = new();
    }

    public void CreateItems(IReadOnlyList<ShopItemData> shopItemDatas)
    {
        foreach(ShopItemData shopItemData in shopItemDatas)
        {
            ShopItemView item = Instantiate(_shopItemViewPrefab);
            item.transform.parent = _itemsParent;

            item.Init(shopItemData);

            _items.Add(item);
        }
    }

    private void ClearItems()
    {
        foreach(var item in _items)
        {
            Destroy(item.gameObject);
        }

        _items.Clear();
    }

    private void OnCloseButtonCliked()
    {
        CloseButtonCliked?.Invoke();
    }
}