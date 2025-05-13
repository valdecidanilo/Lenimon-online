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
    private int currentScreen = -1;
    private InputAction navigateAction;
    private InputAction confirmAction;

    protected override void Awake()
    {
        base.Awake();
        navigateAction = InputSystem.actions.FindAction("UI/Navigate");

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
        contextSelection.Select(0);
        LoadScreenData();
    }

    private void LoadScreenData()
    {
        List<ItemModel> itemList = currentScreen switch
        {
            0 => bag.items,
            1 => bag.pokeballs,
            2 => bag.battleItems,
            _ => null,
        };

        if(itemList == null)
        {

            return;
        }

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
        
    }

    private void OpenItemOptions(int id)
    {
        
    }
}
