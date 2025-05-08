using System;
using System.Collections;
using System.Collections.Generic;
using Battle;
using LenixSO.Logger;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Logger = LenixSO.Logger.Logger;
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
    private int summaryPokemonId;

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
        fightMenu.onPickMove += OnMovePick;
        partyChoice.onReturn += OpenChoiceMenu;
        partyChoice.onChangePokemon += OnAllyChanged;
        partyChoice.onSummaryCall += (id)=>
        {
            summaryPokemonId = id;
            summaryFromParty = true;
            OpenPokemonSummary(allyParty[id]);
        };
        summary.onShiftPokemon += (delta) =>
        {
            if (delta == 0 || !summaryFromParty) return;
            summaryPokemonId = (summaryPokemonId + delta + allyParty.Length) % allyParty.Length;
            OpenPokemonSummary(allyParty[summaryPokemonId]);
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
        enemyPokemon.onDamaged += OpponentDamaged;

        //setup ally
        allyParty = allies;
        SetupAlly(allyParty[0]);
        allyPokemon.onDamaged += AllyDamaged;

        OpenChoiceMenu();
    }

    private void SetupAlly(Pokemon ally)
    {
        allyPokemon = ally;
        pokemonImage.sprite = allyPokemon.backSprite;
        pokemonName.text = allyPokemon.name;
        level.text = $"Lv{allyPokemon.level}";
        hpValue.text = $"{allyPokemon.battleStats[StatType.hp]}/{allyPokemon.stats[StatType.hp]}";
        hp.fillAmount = allyPokemon.battleStats[StatType.hp] / (float)allyPokemon.stats[StatType.hp];
        xp.fillAmount = Random.Range(0, 1);

        PokeDatabase.SetGenderSprite(gender, allyPokemon.gender);
    }

    private void SetupEnemy(Pokemon enemy)
    {
        enemyPokemon = enemy;
        enemyImage.sprite = enemyPokemon.frontSprite;
        enemyName.text = enemyPokemon.name;
        enemyLevel.text = $"Lv{enemyPokemon.level}";
        enemyHp.fillAmount = enemy.battleStats[StatType.hp] / (float)enemy.stats[StatType.hp];

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

    private void OnMovePick(int id)
    {
        StartCoroutine(BattleSequence(allyPokemon.moves[id], null));
    }

    private IEnumerator BattleSequence(MoveModel allyMove, MoveModel opponentMove)
    {
        BattleEvent evtBattle = new();
        evtBattle.move = allyMove;
        evtBattle.origin = allyPokemon;
        evtBattle.target = GetTarget(evtBattle.move.Data.target.name, allyPokemon, enemyPokemon);
        evtBattle.attackEvent = new(allyPokemon, enemyPokemon, evtBattle.move);

        bool hit = evtBattle.attackEvent.CheckHit(out bool missed, out bool evaded);

        if (hit)
        {
            yield return evtBattle.move.effect.EffectSequence(evtBattle);
            if (evtBattle.failed)
            {
                //fail text
            }
        }
        else
        {
            //missed/evaded text
        }
    }

    private Pokemon GetTarget(string target, Pokemon self, Pokemon opponent)
    {
        return target switch
        {
            "user" => self,
            "user-and-allies" => self,
            "selected-pokemon" => opponent,
            "all-opponents" => opponent,
            "all-other-pokemon" => opponent,
        };
    }

    private void AllyDamaged(int initialValue, int currentValue)
    {
        SetupAlly(allyPokemon);
    }
    private void OpponentDamaged(int initialValue, int currentValue)
    {
        Logger.Log("Enemy damaged",LogFlags.Game);
        SetupEnemy(enemyPokemon);
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
