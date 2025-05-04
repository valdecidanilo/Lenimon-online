using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class BattleSetup : MonoBehaviour
{
    [Header("Battle Menu")]
    [SerializeField] private GameObject battleMenu;
    [SerializeField] private ContextSelection battleChoice;

    [Header("Moves Menu")]
    [SerializeField] private FightMenu fightMenu;

    [Header("Party Menu")]
    [SerializeField] private PartyMenu partyChoice;

    [Header("Summary Menu")]
    [SerializeField] private SummaryMenu summary;

    private bool summaryFromParty;

    #region Battle Visuals

    //Enemy
    private Pokemon enemyPokemon;
    private Pokemon[] enemyParty;
    [Header("Enemy")]
    [SerializeField] private Image enemyImage;
    [SerializeField] private TMP_Text enemyName;
    [SerializeField] private TMP_Text enemyLevel;
    [SerializeField] private Image enemyHp;
    [SerializeField] private Image enemyGender;

    //Ally
    private Pokemon allyPokemon;
    private Pokemon[] allyParty;
    [Header("Ally")]
    [SerializeField] private Image pokemonImage;
    [SerializeField] private TMP_Text pokemonName;
    [SerializeField] private TMP_Text level;
    [SerializeField] private Image hp;
    [SerializeField] private TMP_Text hpValue;
    [SerializeField] private Image xp;
    [SerializeField] private Image gender;
    #endregion

    private void Awake()
    {
        battleChoice.onItemPick += OnChoicePick;
        fightMenu.onReturn += OpenChoiceMenu;
        partyChoice.onReturn += OpenChoiceMenu;
        partyChoice.onChangePokemon += OnAllyChanged;
        partyChoice.onSummaryCall += (pkm)=>
        {
            summaryFromParty = true;
            OpenPokemonSummary(pkm);
        };
        summary.onReturn += ()=>
        {
            summary.CloseMenu();
            if(summaryFromParty)
                OpenParty();
            else
                OpenChoiceMenu();
            summaryFromParty = false;
        };
    }

    private void Update()
    {
        if(Keyboard.current.tabKey.wasPressedThisFrame) OpenPokemonSummary(enemyPokemon);
    }

    public void SetupBattle(Pokemon[] allies, Pokemon[] enemies)
    {
        //setup enemy
        enemyParty = enemies;
        SetupEnemy(enemyParty[0]);

        //setup ally
        allyParty = allies;
        SetupAlly(allyParty[0]);

        OpenChoiceMenu();
    }

    private void SetupAlly(Pokemon ally)
    {
        allyPokemon = ally;
        pokemonImage.sprite = allyPokemon.backSprite;
        pokemonName.text = allyPokemon.name;
        level.text = $"Lv{allyPokemon.level}";
        hpValue.text = $"{allyPokemon.stats[StatType.hp]}/{allyPokemon.stats[StatType.hp]}";
        hp.fillAmount = 1;
        xp.fillAmount = Random.Range(0, 1);

        PokeDatabase.SetGenderSprite(gender, allyPokemon.gender);
    }

    private void SetupEnemy(Pokemon enemy)
    {
        enemyPokemon = enemy;
        enemyImage.sprite = enemyPokemon.frontSprite;
        enemyName.text = enemyPokemon.name;
        enemyLevel.text = $"Lv{enemyPokemon.level}";
        enemyHp.fillAmount = 1;

        PokeDatabase.SetGenderSprite(gender, enemyPokemon.gender);
    }

    private void OnAllyChanged(int newAlly)
    {
        Pokemon cashe = allyParty[newAlly];
        allyParty[newAlly] = allyParty[0];
        allyParty[0] = cashe;
        SetupAlly(allyParty[0]);
        OpenChoiceMenu();
    }

    #region Window Changes
    private void OnChoicePick(int choice)
    {
        switch (choice)
        {
            case 0:
                OpenBattleScene();
                break;
            case 1:
                OpenBag();
                break;
            case 2:
                OpenParty();
                break;
            case 3:
                Run();
                break;
            default:
                Debug.Log("choice not found");
                break;
        }
    }

    private void OpenChoiceMenu()
    {
        //disable other windows
        fightMenu.CloseMenu();
        partyChoice.CloseMenu();
        summary.CloseMenu();

        //open window
        battleMenu.SetActive(true);
        battleChoice.Select(0);
    }

    private void OpenBattleScene()
    {
        battleMenu.SetActive(false);
        fightMenu.OpenMenu(allyPokemon);
    }

    private void OpenBag()
    {
        
    }

    private void OpenParty()
    {
        battleChoice.ReleaseSelection();
        partyChoice.OpenMenu(allyParty);
    }

    private void OpenPokemonSummary(Pokemon pokemon)
    {
        battleChoice.ReleaseSelection();
        partyChoice.CloseMenu();
        summary.OpenMenu(pokemon);
    }

    private void Run()
    {
        
    }
    #endregion
}
