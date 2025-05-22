using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using LenixSO.Logger;
using Logger = LenixSO.Logger.Logger;
using CallbackContext = UnityEngine.InputSystem.InputAction.CallbackContext;

public class BagMenu : ContextMenu<Bag>
{
    [SerializeField] private TMP_Text screenName;
    [SerializeField] private Selectable[] screenIndicator;
    [SerializeField] private Image itemIcon;
    [SerializeField] private Announcer itemDescription;
    [SerializeField] private ContextSelection optionsContext;
    [SerializeField] private PartyMenu partyMenu;
    
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
        optionsContext.onItemPick += OnPickOption;
    }

    public override void OpenMenu(Bag data)
    {
        Announcer.ChangeAnnouncer(itemDescription);
        contextSelection.MouseSelection(true);
        bag = data;
        gameObject.SetActive(true);
        optionsContext.gameObject.SetActive(false);
        UpdateScreen();
        base.OpenMenu(data);
        navigateAction.performed += ChangeScreen;
    }

    public override void CloseMenu()
    {
        currentScreen = -1;
        gameObject.SetActive(false);
        base.CloseMenu();
        navigateAction.performed -= ChangeScreen;
    }

    private void ChangeScreen(CallbackContext context)
    {
        Vector2 direction = context.ReadValue<Vector2>();
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
        itemOffset = 0;
        LoadScreenData();
        contextSelection.Select(0);
        ShowItemDetails(0);
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
            Announcer.CloseAnnouncement();
            return;
        }
        
        if (id >= bagItems.Length - 1)
        {
            StartCoroutine(ScrollDelay(1));
            return;
        }
        else if(id == 0 && itemOffset > 0)
        {
            StartCoroutine(ScrollDelay(-1));
            return;
        }

        ItemModel item = itemList[itemId];
        itemIcon.sprite = item.sprite;
        StartCoroutine(Announcer.Announce(item.effect));
    }

    private IEnumerator ScrollDelay(int offset)
    {
        itemOffset += offset;
        yield return null;
        contextSelection.Select(contextSelection.selectedId - offset);
        LoadScreenData();
    }

    private void OpenItemOptions(int id)
    {
        if (id + itemOffset >= itemList.Count)
        {
            base.ReturnCall(new());
            return;
        }
        
        OpenOptions();
    }

    protected override void ReturnCall(CallbackContext context)
    {
        if (optionsContext.gameObject.activeSelf)
        {
            CloseOptions();
            return;
        }
        base.ReturnCall(context);
    }

    private void OpenOptions()
    {
        navigateAction.performed -= ChangeScreen;
        contextSelection.MouseSelection(false);
        optionsContext.gameObject.SetActive(true);
        optionsContext.Select(0);
    }

    private void OnPickOption(int id)
    {
        switch (id)
        {
            case 0:
                ItemModel item = itemList[itemOffset + contextSelection.selectedId];
                IEnumerator coroutine = item.battleEffect == null ? NotUsableAnnouncement(id) : UseBattleItem(item);
                StartCoroutine(coroutine);
                break;
            case 1:
                break;
            case 2:
                CloseOptions();
                break;
        }
    }

    private IEnumerator NotUsableAnnouncement(int id)
    {
        yield return Announcer.Announce("Can't use this item!", holdTime: .8f);
        ShowItemDetails(id);
    }

    private IEnumerator UseBattleItem(ItemModel item)
    {
        DisableNavigation();
        bool pokemonSelected = false;
        while (!pokemonSelected)
        {
            PickPokemonEvent evt = new();
            yield return partyMenu.PickPokemon(evt);
        
            if(evt.pickedPokemon == null)
            {
                EnableNavigation();
                partyMenu.CloseMenu();
                Announcer.ChangeAnnouncer(itemDescription);
                yield return Announcer.Announce(item.effect);
                OpenOptions();
                Logger.Log("Canceled");
                yield break;
            }
            
            if (evt.isCurrent)
            {
                //close bag and mockup a move
                pokemonSelected = true;
            }
            else
            {
                //animate on party then close
                //may also need to be a move
                if (item.activePokemonOnly)
                {
                    yield return Announcer.Announce("This item can only be used on the pokemon in battle!", true);
                    Announcer.CloseAnnouncement();
                }
                else
                {
                    
                    pokemonSelected = true;
                }
            }
        }
    }

    private void DisableNavigation()
    {
        cancelAction.performed -= ReturnCall;
        navigateAction.performed -= ChangeScreen;
    }

    private void EnableNavigation()
    {
        cancelAction.performed += ReturnCall;
        navigateAction.performed += ChangeScreen;
    }

    private void CloseOptions()
    {
        navigateAction.performed += ChangeScreen;
        contextSelection.MouseSelection(true);
        optionsContext.gameObject.SetActive(false);
        contextSelection.Focus();
    }
}
