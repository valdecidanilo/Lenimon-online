using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using LenixSO.Logger;
using Logger = LenixSO.Logger.Logger;

public class BagMenu : ContextMenu<Bag>
{
    [SerializeField] private TMP_Text screenName;
    [SerializeField] private Selectable[] screenIndicator;
    [SerializeField] private Image itemIcon;
    [SerializeField] private TMP_Text itemDescription;
    
    private const int screenCount = 4;

    private Bag bag;
    private BagItem[] bagItems;
    private List<ItemModel> itemList;
    private int itemOffset = 0;
    
    private int currentScreen = -1;
    private InputAction navigateAction;
    private InputAction confirmAction;

    protected override void Awake()
    {
        base.Awake();
        navigateAction = InputSystem.actions.FindAction("UI/Navigate");

        bagItems = new BagItem[contextSelection.itemCount];
        for (int i = 0; i < bagItems.Length; i++)
            bagItems[i] = contextSelection[i].GetComponent<BagItem>();

        contextSelection.onSelect += ShowItemDetails;
        contextSelection.onItemPick += OpenItemOptions;
    }

    public override void OpenMenu(Bag data)
    {
        bag = data;
        gameObject.SetActive(true);
        UpdateScreen();
    }

    public override void CloseMenu()
    {
        currentScreen = -1;
        gameObject.SetActive(false);
    }

    protected override void Update()
    {
        base.Update();
        if (navigateAction.WasPressedThisFrame()) ChangeScreen(navigateAction.ReadValue<Vector2>());
    }

    private void ChangeScreen(Vector2 direction)
    {
        int screenId = (currentScreen + screenCount + Mathf.FloorToInt(direction.x)) % screenCount;
        if(screenId == currentScreen) return;
        UpdateScreen(screenId);
    }

    private void UpdateScreen(int screenId = 0)
    {
        currentScreen = screenId;
        screenName.text = ScreenText(currentScreen);
        for (int i = 0; i < screenIndicator.Length; i++) screenIndicator[i].interactable = false;
        screenIndicator[currentScreen].interactable = true;
        LoadScreenData();
    }

    private void LoadScreenData()
    {
        itemList = currentScreen switch
        {
            0 => bag.items,
            1 => bag.pokeballs,
            2 => bag.battleItems,
            _ => bag.TMs,
        };
        if(itemList == null) return;

        for (int i = 0; i < contextSelection.itemCount; i++)
        {
            int id = i + itemOffset;
            bagItems[i].SetupItem(itemList.Count > id ? itemList[id] : null);
            if(id == itemList.Count) bagItems[i].SetAsCloseBag();
        }
        contextSelection.Select(0);
        ShowItemDetails(0);
    }

    private string ScreenText(int id)
    {
        return id switch
        {
            0 => "Items",
            1 => "Pokeballs",
            2 => "Battle",
            3 => "TMs",
        };
    }

    private void ShowItemDetails(int id)
    {
        int itemId = id + itemOffset;
        if (itemId >= itemList.Count)
        {
            itemIcon.sprite = PokeDatabase.emptySprite;
            itemDescription.text = string.Empty;
            return;
        }

        ItemModel item = itemList[itemId];
        itemIcon.sprite = item.sprite;
        itemDescription.text = item.effect;
    }

    private void OpenItemOptions(int id)
    {
        
    }
}
