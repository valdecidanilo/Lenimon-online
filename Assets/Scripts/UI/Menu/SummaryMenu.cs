using LenixSO.Logger;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Logger = LenixSO.Logger.Logger;

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
    [SerializeField] private Image type;
    [SerializeField] private TMP_Text abilityName;
    [SerializeField] private TMP_Text abilityDesc;
    [SerializeField] private TMP_Text memo;
    #endregion
    //Screen 2: stats, item, xp
    #region Screen 2
    [Header("Screen 2")]
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
    [SerializeField] private TMP_Text pp;
    [SerializeField] private TMP_Text description;
    private SummaryMove[] moves;
    #endregion

    private const int screenCount = 3;

    private Pokemon pokemon;
    private int currentScreen = 0;

    private InputAction navigateAction;
    private InputAction confirmAction;

    protected override void Awake()
    {
        base.Awake();
        navigateAction = InputSystem.actions.FindAction("UI/Navigate");
        confirmAction = InputSystem.actions.FindAction("UI/Submit");

        moves = new SummaryMove[4];
        for (int i = 0; i < moves.Length; i++)
        {
            moves[i] = contextSelection[i].GetComponent<SummaryMove>();
        }
    }

    public override void OpenMenu(Pokemon pokemon)
    {
        //set general
        pokemonImage.sprite = pokemon.frontSprite;
        PokeDatabase.SetGenderSprite(gender, pokemon.gender);
        nickname.text = pokemon.name;
        species.text = pokemon.name;
        level.text = $"{pokemon.level}";
        //set screen 1
        //type
        abilityName.text = pokemon.ability.abilityName;
        abilityDesc.text = pokemon.ability.flavorText;
        memo.text = $"<color=red>NATURE</color> nature,\nmet at Lv<color=red>{Random.Range(1, pokemon.level + 1)}</color>," +
            $"\n<color=red>ROUTE {Random.Range(1, 200)}</color>.";
        //set screen 2
        //set screen 3

        gameObject.SetActive(true);
    }

    protected override void Update()
    {
        base.Update();
        if(navigateAction.WasPressedThisFrame()) ChangeScreen(navigateAction.ReadValue<Vector2>());
        if(confirmAction.WasPressedThisFrame()) MoveDetails();
    }

    private void ChangeScreen(Vector2 direction)
    {
        currentScreen = (currentScreen + screenCount + Mathf.FloorToInt(direction.x)) % screenCount;
        Logger.Log($"{direction} => {currentScreen}", LogFlags.Game);

        for (int i = 0; i < screens.Length; i++)
            screens[i].SetActive(currentScreen == i);
    }

    private void MoveDetails()
    {
        if(currentScreen != 2) return;

        selectMove.SetActive(true);
        contextSelection.Select(0);
    }

    protected override void ReturnCall()
    {
        base.ReturnCall();
    }

    public override void CloseMenu()
    {
        gameObject.SetActive(false);
    }
}
