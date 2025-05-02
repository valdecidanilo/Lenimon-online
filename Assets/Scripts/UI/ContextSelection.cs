using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ContextSelection : MonoBehaviour
{
    [SerializeField] private RectTransform arrow;
    [SerializeField] private Vector2 arrowOffset;
    [Space, SerializeField] private List<SelectionItem> selectionItems;

    public int selectedId {  get; private set; }

    public SelectionItem currentSelected => selectionItems[selectedId];

    public int itemCount => selectionItems.Count;
    public SelectionItem this[int id] => selectionItems[id];

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
        currentSelected.OnSelect(null);
    }

    public void Select(int id)
    {
        selectedId = id;
        Focus();
    }

    public void ReleaseSelection()
    {
        EventSystem.current.SetSelectedGameObject(null);
    }

    private void OnItemPick()
    {
        onItemPick?.Invoke(selectedId);
    }

    private void OnSelect(SelectionItem item)
    {
        //Debug.Log($"{item.name} selected");
        Vector2 anchor = item.rectTransform.anchoredPosition + arrowOffset;

        arrow.anchorMax = item.rectTransform.anchorMax;
        arrow.anchorMin = item.rectTransform.anchorMin;
        arrow.anchoredPosition = anchor;

        selectedId = selectionItems.IndexOf(item);
        onSelect?.Invoke(selectedId);
    }
}