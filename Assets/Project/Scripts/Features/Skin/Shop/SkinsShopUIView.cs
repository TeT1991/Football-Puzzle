using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SkinsShopUIView : MonoBehaviour
{
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _closeBackgroundButton;
    [SerializeField] private RectTransform _itemsParent;
    [SerializeField] private SkinShopItemView _shopItemViewPrefab;

    private int _selectedSkinId;

    private List<SkinShopItemView> _items;

    public event Action CloseButtonClicked;

    public event Action<int> SkinButonClicked;
    public event Action<int> BuyButtonClicked;

    public RectTransform ItemsParent => _itemsParent;

    private void OnDestroy()
    {
        _closeButton.onClick.RemoveAllListeners();
        ClearItems();
    }

    public void Init(int selectedId)
    {
        _closeButton.onClick.AddListener(OnCloseButtonCliked);
        _closeBackgroundButton.onClick.AddListener(OnCloseButtonCliked);

        _items = new();
        _selectedSkinId = selectedId;
    }

    public void CreateItems(IReadOnlyList<SkinShopItemData> shopItemDatas)
    {
        ClearItems();

        foreach (SkinShopItemData shopItemData in shopItemDatas)
        {
            SkinShopItemView item = Instantiate(_shopItemViewPrefab, _itemsParent, false);

            item.Init(shopItemData);
            item.SkinButtonClicked += OnSkinButtonClicked;
            item.BuyButtonClicked += OnBuyButtonClicked;

            _items.Add(item);
        }

        MarkItemAsSelected(_selectedSkinId);
    }

    private void ClearItems()
    {
        if (_items == null)
        {
            return;
        }

        foreach (var item in _items)
        {
            item.SkinButtonClicked -= OnSkinButtonClicked;
            item.BuyButtonClicked -= OnBuyButtonClicked;
            Destroy(item.gameObject);
        }

        _items.Clear();
    }

    public void Unlock(int id)
    {
        foreach (SkinShopItemView item in _items)
        {
            if (item.Id == id)
            {
                item.ShowUnlockGraphic();
                break;
            }
        }
    }

    public void MarkItemAsSelected(int id)
    {
        MarkItemsAsUnselected();

        foreach (SkinShopItemView item in _items)
        {
            if (item.Id == id)
            {
                item.ShowSelectedMark();
                _selectedSkinId = id;
                break;
            }
        }
    }

    private void MarkItemsAsUnselected()
    {
        foreach (SkinShopItemView item in _items)
        {
            item.HideSelectedMark();
        }
    }

    private void OnSkinButtonClicked(int id)
    {
        SkinButonClicked?.Invoke(id);
    }

    private void OnBuyButtonClicked(int id)
    {
        BuyButtonClicked?.Invoke(id);
    }


    private void OnCloseButtonCliked()
    {
        CloseButtonClicked?.Invoke();
    }
}
