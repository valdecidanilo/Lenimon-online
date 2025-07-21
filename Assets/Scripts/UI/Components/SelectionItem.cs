using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SelectionItem : MonoBehaviour, IPointerEnterHandler, ISelectHandler
{
    [SerializeField] private Button button;

    private RectTransform _rectTransform;

    public Selectable selectable => button;

    private bool mouseSelection = true;

    public RectTransform rectTransform => _rectTransform ??= (RectTransform)transform;
    public Action<SelectionItem> onSelected;
    public Action onPick;

    private void OnEnable()
    {
        MouseSelection(mouseSelection);
    }

    private void Awake()
    {
        button.onClick.AddListener(() => onPick?.Invoke());
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if(!mouseSelection) return;
        button.Select();
        OnSelected();
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (eventData == null)
        {
            button.Select();
            return;
        }

        OnSelected();
    }

    public void MouseSelection(bool active)
    {
        mouseSelection = active;
        button.interactable = active;
    }

    private void OnSelected()
    {
        AudioManager.Instance.PlaySelectAudio();
        onSelected?.Invoke(this);
    }

    private void SelectionSubmit() => onPick?.Invoke();
}
