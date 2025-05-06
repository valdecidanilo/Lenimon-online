using LenixSO.Logger;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Logger = LenixSO.Logger.Logger;
using Random = UnityEngine.Random;

public class SummaryMenu : ContextMenu<Pokemon>
{
    [Header("Screens")]
    [SerializeField] private GameObject[] screens;
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
    private SummaryMove[] moves;
    #endregion

    public event Action<int> onShiftPokemon;

    private const int screenCount = 3;
    private Sprite emptyIcon;

    private Pokemon pokemon;
    private int currentScreen = 0;

    private InputAction navigateAction;
    private InputAction confirmAction;

    protected override void Awake()
    {
        base.Awake();
        navigateAction = InputSystem.actions.FindAction("UI/Navigate");
        confirmAction = InputSystem.actions.FindAction("UI/Submit");

        contextSelection.onSelect += OnMoveSelect;
    }

    public override void OpenMenu(Pokemon target)
    {
        emptyIcon ??= itemImage.sprite;
        if(moves == null)
        {
            moves = new SummaryMove[4];
            for (int i = 0; i < moves.Length; i++)
            {
                moves[i] = contextSelection[i].GetComponent<SummaryMove>();
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
        UpdateScreen();
    }

    protected override void Update()
    {
        base.Update();
        if(navigateAction.WasPressedThisFrame()) ChangeScreen(navigateAction.ReadValue<Vector2>());
        if(confirmAction.WasPressedThisFrame()) OpenMoveDetails();
    }

    private void ChangeScreen(Vector2 direction)
    {
        if(selectMove.activeSelf) return;
        UpdateScreen((currentScreen + screenCount + Mathf.FloorToInt(direction.x)) % screenCount);
        onShiftPokemon?.Invoke(Mathf.FloorToInt(direction.y));
    }

    private void UpdateScreen(int screenId = 0)
    {
        if(screenId >= screens.Length) return;
        currentScreen = screenId;
        for (int i = 0; i < screens.Length; i++)
            screens[i].SetActive(currentScreen == i);
    }

    private void OpenMoveDetails()
    {
        if(currentScreen != 2) return;

        if (selectMove.activeSelf &&
            contextSelection.selectedId == contextSelection.itemCount - 1)
        {
            ExitMoveSelection();
            return;
        }
        selectMove.SetActive(true);
        contextSelection.Select(0);
    }

    private void OnMoveSelect(int id)
    {
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
        contextSelection.ReleaseSelection();
    }

    protected override void ReturnCall()
    {
        if (selectMove.activeSelf)
            ExitMoveSelection();
        else
            base.ReturnCall();
    }

    public override void CloseMenu()
    {
        selectMove.SetActive(false);
        gameObject.SetActive(false);
    }
}
