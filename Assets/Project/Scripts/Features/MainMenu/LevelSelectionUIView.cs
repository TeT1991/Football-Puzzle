using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class LevelSelectionUIView : MonoBehaviour
{
    [SerializeField] private Button _closeButton;
    [SerializeField] private Button _closeBackgroundButton;
    [SerializeField] private RectTransform _itemsParent;
    [SerializeField] private LeagueItemView _leagueItemViewPrefab;

    private List<LeagueItemView> _items;

    public event Action CloseButtonClicked;

    public event Action LeagueButtonClicked;

    public void Init()
    {
        _closeButton.onClick.AddListener(OnCloseButtonClicked);
        _closeBackgroundButton.onClick.AddListener(OnCloseButtonClicked);

        _items = new();
    }

    public void CreateItems(IReadOnlyList<LeagueMenuItemData> leagueItemDatas)
    {
        ClearItems();

        foreach (LeagueMenuItemData data in leagueItemDatas)
        {
            LeagueItemView item = Instantiate(_leagueItemViewPrefab, _itemsParent, false);
            item.Init($"ID: {data.ID}");
            _items.Add(item);
        }
    }

    private void ClearItems()
    {
        foreach (LeagueItemView item in _items)
        {
            Destroy(item.gameObject);
        }

        _items.Clear();
    }

    private void OnCloseButtonClicked()
    {
        CloseButtonClicked?.Invoke();
    }
}