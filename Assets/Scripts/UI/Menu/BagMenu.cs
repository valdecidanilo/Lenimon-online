using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using LenixSO.Logger;
using Logger = LenixSO.Logger.Logger;
using CallbackContext = UnityEngine.InputSystem.InputAction.CallbackContext;
using Battle;

public class BagMenu : ContextMenu<Bag>
{
    [SerializeField] private GameObject bagScene;
    [SerializeField] private TMP_Text screenName;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button prevButton;
    [SerializeField] private Button upButton;
    [SerializeField] private Button downButton;
    [SerializeField] private Selectable[] screenIndicator;
    [SerializeField] private Image itemIcon;
    [SerializeField] private Announcer itemDescription;
    [SerializeField] private ContextSelection optionsContext;
    
    private const int screenCount = 4;

    private int offsetRange => itemList.Count - contextSelection.itemCount + 1;

    private Bag bag;
    private BagItem[] bagItems;
    private List<ItemModel> itemList;
    private int itemOffset = 0;
    
    private int currentScreen = 0;
    private InputAction navigateAction;

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

        nextButton.onClick.AddListener(() => ShiftScreen(1));
        prevButton.onClick.AddListener(() => ShiftScreen(-1));

        upButton.onClick.AddListener(() => ItemMoveButton(1));
        downButton.onClick.AddListener(() => ItemMoveButton(-1));
        bagScene.SetActive(false);
    }

    public override void OpenMenu(Bag data)
    {
        Announcer.ChangeAnnouncer(itemDescription);
        contextSelection.MouseSelection(true);
        bag = data;
        bagScene.SetActive(true);
        optionsContext.gameObject.SetActive(false);
        UpdateScreen(currentScreen);
        base.OpenMenu(data);
        navigateAction.performed += ChangeScreen;
    }

    public override void CloseMenu()
    {
        //gameObject.SetActive(false);
        bagScene.SetActive(false);
        base.CloseMenu();
        navigateAction.performed -= ChangeScreen;
    }

    private void ChangeScreen(CallbackContext context)
    {
        Vector2 direction = context.ReadValue<Vector2>();
        ShiftScreen(Mathf.FloorToInt(direction.x));
        if (!context.action.WasPressedThisFrame()) return;
        ItemOffsetShift(Mathf.FloorToInt(direction.y));
    }

    private void ItemMoveButton(int delta)
    {
        int newItemId = contextSelection.selectedId - delta;
        if (newItemId >= 0 && newItemId < contextSelection.itemCount)
            contextSelection.Select(newItemId);
        else 
            ItemOffsetShift(delta);
    }

    private void ItemOffsetShift(int delta)
    {
        if (offsetRange < 1) return;
        int newItemId = contextSelection.selectedId - delta;
        if (newItemId >= 0 && newItemId < contextSelection.itemCount) return;
        int newOffset = itemOffset - delta;
        if (newOffset < 0 || newOffset > offsetRange) return;
        ShiftOffset(delta);
    }

    private void ShiftScreen(int direction)
    {
        int screenId = (currentScreen + screenCount + Mathf.FloorToInt(direction)) % screenCount;
        if(screenId == currentScreen) return;
        UpdateScreen(screenId);
        AudioManager.Instance.PlaySelectAudio();
    }

    private void ShiftOffset(int offset)
    {
        itemOffset -= offset;
        AudioManager.Instance.PlaySelectAudio();
        LoadScreenData();
    }

    private void UpdateScreen(int screenId = 0)
    {
        currentScreen = screenId;
        screenName.text = ScreenText(currentScreen);
        for (int i = 0; i < screenIndicator.Length; i++) screenIndicator[i].interactable = false;
        screenIndicator[currentScreen].interactable = true;
        itemOffset = 0;
        itemList = currentScreen switch
        {
            0 => bag.items,
            1 => bag.pokeballs,
            2 => bag.battleItems,
            _ => bag.TMs,
        };
        LoadScreenData();
        contextSelection.Select(0);
        ShowItemDetails(0);
    }

    private void LoadScreenData()
    {
        if(itemList == null) return;
        
        downButton.gameObject.SetActive(itemOffset < offsetRange);
        upButton.gameObject.SetActive(itemOffset > 0);

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
        StartCoroutine(Announcer.AnnounceCoroutine(item.effect));
    }

    private IEnumerator ScrollDelay(int offset)
    {
        //itemOffset += offset;
        yield return null;
        //contextSelection.Select(contextSelection.selectedId - offset);
        //LoadScreenData();
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
                CloseOptions();
                break;
        }
    }

    private IEnumerator NotUsableAnnouncement(int id)
    {
        yield return Announcer.AnnounceCoroutine("Can't use this item!", holdTime: .8f);
        ShowItemDetails(id);
    }

    private IEnumerator UseBattleItem(ItemModel item)
    {
        DisableNavigation();
        bool pokemonSelected = false;
        while (!pokemonSelected)
        {
            PickPokemonEvent evt = new(item.activePokemonOnly, tm: item as TMModel);
            yield return PartyMenu.PickPokemon(evt);
            
            if(evt.pickedPokemon == null)
            {
                ClosePartyMenu();
                Announcer.ChangeAnnouncer(itemDescription);
                yield return Announcer.AnnounceCoroutine(item.effect);
                OpenOptions();
                yield break;
            }

            if (evt.move != null)
            {
                if (evt.canLearnTM)
                {
                    //open move pick
                    MoveLearnEvent moveLearn = new(evt.move.data.moveData, evt.pickedPokemon);
                    yield return MoveHelper.LearnMoveSequence(moveLearn);
                    if (moveLearn.moveLearnt)
                    {
                        ClosePartyMenu();
                        Announcer.ChangeAnnouncer(itemDescription);
                        CloseOptions();
                        ShowItemDetails(contextSelection.selectedId);
                        pokemonSelected = true;
                    }else SummaryMenu.CloseSummaryMenu();
                }
                else
                {
                    yield return Announcer.AnnounceCoroutine($"{evt.pickedPokemon.name} can't learn this move!", true);
                    Announcer.CloseAnnouncement();
                }
            }
            else if (item.checkItemUse.Invoke(evt.pickedPokemon, out string failMessage))
            {
                MoveModel mockMove = evt.isCurrent ? ItemEffect.CreateMockMove(item) : MoveEffectCreator.EmptyMove();
                if (!evt.isCurrent)
                {
                    //animate on party then close
                    BattleEvent battleEvt = new();
                    battleEvt.target = battleEvt.origin = evt.pickedPokemon;
                    yield return item.battleEffect.EffectSequence(battleEvt);
                }
                UseItemSequence(mockMove);
            }
            else
            {
                yield return Announcer.AnnounceCoroutine(failMessage, true);
                Announcer.CloseAnnouncement();
            }
        }
        yield break;

        void UseItemSequence(MoveModel effect)
        {
            UseItem();
            ClosePartyMenu();
            CloseMenu();
            FightMenu.BeginBattle(effect);
            pokemonSelected = true;
        }
        void UseItem()
        {
            item.amount--;
            if (item.amount <= 0) itemList.Remove(item);
        }
        void ClosePartyMenu()
        {
            EnableNavigation();
            PartyMenu.ClosePartyMenu();
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

    protected override void OnDestroy()
    {
        base.OnDestroy();
        navigateAction.performed -= ChangeScreen;
    }
}