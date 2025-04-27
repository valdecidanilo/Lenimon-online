using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContextSelection : MonoBehaviour
{

    [SerializeField] private RectTransform arrow;
    [SerializeField] private Vector2 arrowOffset;
    [Space, SerializeField] private List<SelectionItem> selectionItems;

    private int selectedId;

    public Action<int> onItemPick;
    public Action<int> onSelect;

    private void Awake()
    {
        for (int i = 0; i < selectionItems.Count; i++)
        {
            selectionItems[i].onSelected += OnSelect;
            selectionItems[i].onPick += OnItemPick;
        }

        selectedId = 0;
    }

    public void Focus()
    {
        selectionItems[selectedId].OnSelect(null);
    }

    private void OnItemPick()
    {
        onItemPick?.Invoke(selectedId);
    }

    private void OnSelect(SelectionItem item)
    {
        //Debug.Log($"{item.name} selected");

        Rect rect = item.rectTransform.rect;
        Vector2 anchor = item.rectTransform.anchoredPosition + arrowOffset;

        arrow.anchorMax = item.rectTransform.anchorMax;
        arrow.anchorMin = item.rectTransform.anchorMin;
        arrow.anchoredPosition = anchor;

        selectedId = selectionItems.IndexOf(item);
        onSelect?.Invoke(selectedId);
    }
}