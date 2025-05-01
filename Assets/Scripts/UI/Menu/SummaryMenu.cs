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
    //All sceens: front sprite, nickname/name, lv, gender, pokeball?
    [Header("General")]
    [SerializeField] private Image sprite;
    [SerializeField] private Image gender;
    [SerializeField] private Image pokeball;
    [SerializeField] private TMP_Text nickname;
    [SerializeField] private TMP_Text species;
    [SerializeField] private TMP_Text level;
    //Screen 1: type, ability/description, nature/met lv/ route
    [Header("Screen 1")]
    [SerializeField] private Image type;
    [SerializeField] private TMP_Text abilityName;
    [SerializeField] private TMP_Text abilityDesc;
    [SerializeField] private TMP_Text memo;
    //Screen 2: stats, item, xp
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
    //Screen 3: moves/descr

    private Pokemon pokemon;
    private InputAction navigateAction;

    protected override void Awake()
    {
        base.Awake();
        navigateAction = InputSystem.actions.FindAction("UI/Navigate");
    }

    public override void OpenMenu(Pokemon data)
    {
        gameObject.SetActive(true);
    }

    protected override void Update()
    {
        base.Update();
        if(navigateAction.WasPressedThisFrame()) ChangeWindow(navigateAction.ReadValue<Vector2>());
    }

    private void ChangeWindow(Vector2 direction)
    {
        Logger.Log($"{direction}", LogFlags.Game);
    }

    public override void CloseMenu()
    {
        gameObject.SetActive(false);
    }
}
