using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectionItem : MonoBehaviour, IPointerEnterHandler, ISelectHandler
{
    [SerializeField] private Button button;

    private RectTransform _rectTransform;

    public RectTransform rectTransform => _rectTransform ??= (RectTransform)transform;
    public Action<SelectionItem> onSelected;
    public Action onPick;

    private void Awake()
    {
        button.onClick.AddListener(() => onPick?.Invoke());
    }


    public void OnPointerEnter(PointerEventData eventData)
    {
        button.Select();
        onSelected?.Invoke(this);
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (eventData == null)
        {
            button.Select();
            return;
        }

        onSelected?.Invoke(this);
    }
}
