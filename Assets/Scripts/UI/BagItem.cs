using TMPro;
using UnityEngine;

public class BagItem : MonoBehaviour
{
    [SerializeField] private TMP_Text itemName;
    [SerializeField] private TMP_Text itemAmount;

    public void SetupItem(ItemModel item)
    {
        itemName.text = item?.name ?? string.Empty;
        itemAmount.text = item != null ? $"x   {item.amount}" : string.Empty;
        gameObject.SetActive(item != null);
    }

    public void SetAsCloseBag()
    {
        gameObject.SetActive(true);
        itemName.text = $"Close bag";
        itemAmount.text = string.Empty;
    }
}
