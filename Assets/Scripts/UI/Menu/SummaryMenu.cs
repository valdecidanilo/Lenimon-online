using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using CallbackContext = UnityEngine.InputSystem.InputAction.CallbackContext;

public class SummaryMenu : ContextMenu<(bool showHp, Pokemon pokemon)>
{
    [Header("Screens")]
    [SerializeField] private GameObject[] screens;
    [SerializeField] private Button backButton;
    [SerializeField] private Announcer announcer;
    //All sceens: front sprite, nickname/name, lv, gender, pokeball?
    #region General
    [Header("General")]
    [SerializeField] private Image pokemonImage;
    [SerializeField] private Image gender;
    [SerializeField] private Image pokeball;
    [SerializeField] private TMP_Text nickname;
    [SerializeField] private TMP_Text species;
    [SerializeField] private TMP_Text level;
    #endregion
    //Screen 1: type, ability/description, nature/met lv/ route
    #region Screen 1
    [Header("Screen 1")]
    [SerializeField] private Image[] types;
    [SerializeField] private TMP_Text abilityName;
    [SerializeField] private TMP_Text abilityDesc;
    [SerializeField] private TMP_Text memo;
    #endregion
    //Screen 2: stats, item, xp
    #region Screen 2
    [Header("Screen 2")]
    [SerializeField] private Image itemImage;
    [SerializeField] private TMP_Text item;
    [SerializeField] private TMP_Text hp;
    [SerializeField] private TMP_Text atk;
    [SerializeField] private TMP_Text def;
    [SerializeField] private TMP_Text sAtk;
    [SerializeField] private TMP_Text sDef;
    [SerializeField] private TMP_Text spd;
    [SerializeField] private TMP_Text xp;
    [SerializeField] private TMP_Text nextLv;
    [SerializeField] private Image xpBar;
    #endregion
    //Screen 3: moves/descr
    #region Screen 3
    [Header("Screen 3")]
    [SerializeField] private GameObject selectMove;
    [SerializeField] private TMP_Text power;
    [SerializeField] private TMP_Text accuracy;
    [SerializeField] private TMP_Text description;
    [SerializeField] private ContextSelection moveSelection;
    private SummaryMove[] moves;
    #endregion

    public event Action<int> onShiftPokemon;

    private const int screenCount = 3;
    private const int moveScreen = 2;
    private Sprite emptyIcon;

    private Pokemon pokemon;
    private int currentScreen = 0;

    private static SummaryMenu instance;

    protected override void Awake()
    {
        instance = this;
        base.Awake();

        contextSelection.onSelect += UpdateScreen;
        contextSelection.onItemPick += OpenMoveDetails;
        moveSelection.onSelect += OnMoveSelect;
        backButton.onClick.AddListener(BaseReturnCall);
    }

    public override void OpenMenu((bool showHp, Pokemon pokemon) data)
    {
        Announcer.ChangeAnnouncer(announcer);
        emptyIcon ??= itemImage.sprite;
        if(moves == null)
        {
            moves = new SummaryMove[4];
            for (int i = 0; i < moves.Length; i++)
            {
                moves[i] = moveSelection[i].GetComponent<SummaryMove>();
            }
        }

        pokemon = data.pokemon;
        //set general
        pokemonImage.sprite = pokemon.frontSprite;
        PokeDatabase.SetGenderSprite(gender, pokemon.gender);
        nickname.text = pokemon.name;
        species.text = $"/{pokemon.name}";
        level.text = $"{pokemon.level}";
        //set screen 1
        for (int i = 0; i < types.Length; i++)
        {
            types[i].gameObject.SetActive(i < pokemon.types.Length);
            if (i < pokemon.data.types.Count)
                types[i].sprite = PokeDatabase.typeSprites[pokemon.types[i].name];
        }
        abilityName.text = pokemon.ability.abilityName;
        abilityDesc.text = pokemon.ability.flavorText;
        //nature
        memo.text = $"<color=red>{pokemon.natureName.ToUpper()}</color> nature,\nmet at Lv<color=red>{Random.Range(1, pokemon.level + 1)}</color>," +
            $"\n<color=red>ROUTE {Random.Range(1, 200)}</color>.";
        //set screen 2
        //item
        itemImage.sprite = pokemon.heldItem?.sprite ?? emptyIcon;
        item.text = pokemon.heldItem?.name ?? "NONE";
        hp.text = data.showHp ? $"{pokemon.battleStats.hp}/{pokemon.stats.hp}" : $"-/{pokemon.stats.hp}";
        atk.text = $"{pokemon.stats.atk}";
        def.text = $"{pokemon.stats.def}";
        sAtk.text = $"{pokemon.stats.sAtk}";
        sDef.text = $"{pokemon.stats.sDef}";
        spd.text = $"{pokemon.stats.spd}";
        //xp, next lv, and bar
        //set screen 3
        power.text = "-";
        accuracy.text = "-";
        description.text = "-";
        for (int i = 0; i < moves.Length; i++)
            moves[i].SetupMove(pokemon.moves[i]);

        gameObject.SetActive(true);
        contextSelection.Select(currentScreen);
        //UpdateScreen(currentScreen);
        base.OpenMenu(data);
    }

    private void UpdateScreen(int screenId = 0)
    {
        if (currentScreen == 2 && screenId != 2) selectMove.SetActive(false);
        if(screenId >= screens.Length) return;
        currentScreen = screenId;
        for (int i = 0; i < screens.Length; i++)
            screens[i].SetActive(currentScreen == i);
    }

    private void OpenMoveDetails(int windowId)
    {
        if(windowId != 2) return;

        if (selectMove.activeSelf &&
            moveSelection.selectedId == moveSelection.itemCount - 1)
        {
            ExitMoveSelection();
            return;
        }
        selectMove.SetActive(true);
        moveSelection.Select(0);
    }

    private void OnMoveSelect(int id)
    {
        selectMove.SetActive(true);
        MoveModel move = id < 4 ? pokemon.moves[id] : null;
        power.text = move is { power: > 0 } ? $"{move.power}" : "-";
        accuracy.text = move is { accuracy: > 0 } ? $"{move.accuracy}" : "-";
        description.text = move != null ? $"{move.description}" : "-";
    }

    private void ExitMoveSelection()
    {
        power.text = "-";
        accuracy.text = "-";
        description.text = "-";
        selectMove.SetActive(false);
        moveSelection.ReleaseSelection();
        contextSelection.Focus();
    }

    private void BaseReturnCall() => ReturnCall(new());
    protected override void ReturnCall(CallbackContext context)
    {
        if (selectMove.activeSelf)
            ExitMoveSelection();
        else
            base.ReturnCall(context);
    }

    public override void CloseMenu()
    {
        currentScreen = 0;
        selectMove.SetActive(false);
        gameObject.SetActive(false);
        base.CloseMenu();
    }

    public static IEnumerator ChoseMove(PickMoveEvent evt)
    {
        bool selected = false;
        if (!instance.gameObject.activeSelf) instance.OpenMenu((false,evt.pickedPokemon));
        instance.contextSelection.Select(moveScreen);
        instance.OpenMoveDetails(moveScreen);
        instance.contextSelection.MouseSelection(false);
        instance.moveSelection.onItemPick -= instance.OpenMoveDetails;
        instance.moveSelection.onItemPick += PickMove;
        instance.cancelAction.performed -= instance.ReturnCall;
        instance.cancelAction.performed += CancelSelection;
        instance.backButton.onClick.RemoveListener(instance.BaseReturnCall);
        instance.backButton.onClick.AddListener(BaseCancel);

        yield return new WaitUntil(MoveSelected);
        
        instance.contextSelection.MouseSelection(true);
        instance.moveSelection.onItemPick += instance.OpenMoveDetails;
        instance.moveSelection.onItemPick -= PickMove;
        instance.cancelAction.performed += instance.ReturnCall;
        instance.cancelAction.performed -= CancelSelection;
        instance.backButton.onClick.AddListener(instance.BaseReturnCall);
        instance.backButton.onClick.RemoveListener(BaseCancel);
        
        yield break;

        bool MoveSelected() => selected;

        void PickMove(int id)
        {
            if (id < instance.moveSelection.itemCount - 1)
            {
                evt.moveSlot = id;
                evt.pickedMove = evt.pickedPokemon.moves[id];
            }

            selected = true;
        }

        void BaseCancel() => CancelSelection(new());
        void CancelSelection(CallbackContext context) => selected = true;
    }
    
    public static void CloseSummaryMenu()
    {
        instance.CloseMenu();
    }
}

public class PickMoveEvent
{
    //setup
    public Pokemon pickedPokemon;
    
    //feedback
    public int moveSlot;
    public MoveModel pickedMove;

    public PickMoveEvent(Pokemon pokemon)
    {
        pickedPokemon = pokemon;
    }
}