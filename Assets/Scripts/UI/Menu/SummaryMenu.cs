using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using CallbackContext = UnityEngine.InputSystem.InputAction.CallbackContext;

public class SummaryMenu : ContextMenu<Pokemon>
{
    [Header("Screens")]
    [SerializeField] private GameObject[] screens;
    [SerializeField] private Button backButton;
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
    private Sprite emptyIcon;

    private Pokemon pokemon;
    private int currentScreen = 0;

    protected override void Awake()
    {
        base.Awake();

        contextSelection.onSelect += UpdateScreen;
        contextSelection.onItemPick += OpenMoveDetails;
        moveSelection.onSelect += OnMoveSelect;
        backButton.onClick.AddListener(() => base.ReturnCall(new()));
    }

    public override void OpenMenu(Pokemon target)
    {
        emptyIcon ??= itemImage.sprite;
        if(moves == null)
        {
            moves = new SummaryMove[4];
            for (int i = 0; i < moves.Length; i++)
            {
                moves[i] = moveSelection[i].GetComponent<SummaryMove>();
            }
        }

        pokemon = target;
        //set general
        pokemonImage.sprite = target.frontSprite;
        PokeDatabase.SetGenderSprite(gender, target.gender);
        nickname.text = target.name;
        species.text = $"/{target.name}";
        level.text = $"{target.level}";
        //set screen 1
        for (int i = 0; i < types.Length; i++)
        {
            types[i].gameObject.SetActive(i < target.data.types.Count);
            if (i < target.data.types.Count)
                types[i].sprite = PokeDatabase.typeSprites[target.data.types[i].type.name];
        }
        abilityName.text = target.ability.abilityName;
        abilityDesc.text = target.ability.flavorText;
        //nature
        memo.text = $"<color=red>{target.natureName.ToUpper()}</color> nature,\nmet at Lv<color=red>{Random.Range(1, target.level + 1)}</color>," +
            $"\n<color=red>ROUTE {Random.Range(1, 200)}</color>.";
        //set screen 2
        //item
        itemImage.sprite = target.heldItem?.sprite ?? emptyIcon;
        item.text = target.heldItem?.name ?? "NONE";
        hp.text = $"{target.battleStats.hp}/{target.stats.hp}";
        atk.text = $"{target.stats.atk}";
        def.text = $"{target.stats.def}";
        sAtk.text = $"{target.stats.sAtk}";
        sDef.text = $"{target.stats.sDef}";
        spd.text = $"{target.stats.spd}";
        //xp, next lv, and bar
        //set screen 3
        power.text = "-";
        accuracy.text = "-";
        description.text = "-";
        for (int i = 0; i < moves.Length; i++)
            moves[i].SetupMove(target.moves[i]);

        gameObject.SetActive(true);
        contextSelection.Select(currentScreen);
        //UpdateScreen(currentScreen);
        base.OpenMenu(target);
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
}
