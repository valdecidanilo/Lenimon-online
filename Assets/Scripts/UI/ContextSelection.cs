using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContextSelection : MonoBehaviour
{

    [SerializeField] private RectTransform arrow;
    [SerializeField] private Vector2 arrowOffset;
    [Space, SerializeField] private List<SelectionItem> selectionItems;

    private SelectionItem selected;

    public Action<int> onSubmit;
    public Action<int> onSelect;

    private void Awake()
    {
        selectionItems[0].OnSelect(null);
        for (int i = 0; i < selectionItems.Count; i++)
        {
            selectionItems[i].onSelected += OnSelect;
        }

        selected = selectionItems[0];
    }

    private void OnSelect(SelectionItem item)
    {
        //Debug.Log($"{item.name} selected");

        Rect rect = item.rectTransform.rect;
        Vector2 anchor = item.rectTransform.anchoredPosition + arrowOffset;

        arrow.anchorMax = item.rectTransform.anchorMax;
        arrow.anchorMin = item.rectTransform.anchorMin;
        arrow.anchoredPosition = anchor;

        selected = item;
        onSelect?.Invoke(selectionItems.IndexOf(selected));
    }
}
