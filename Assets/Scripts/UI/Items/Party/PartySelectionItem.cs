using UnityEngine;
using UnityEngine.UI;

public class PartySelectionItem : MonoBehaviour
{
    [SerializeField] protected ContextSelection context;
    [SerializeField] protected SelectionItem targetItem;
    [Space(8)] 
    [SerializeField] protected Image image;
    [SerializeField] protected Sprite normalSprite;
    [SerializeField] protected Sprite selectedSprite;

    private void Awake()
    {
        context.onSelect += OnSelection;
    }

    protected virtual void OnSelection(int id)
    {
        image.sprite = context.currentSelected == targetItem ? selectedSprite : normalSprite;
    }
}
