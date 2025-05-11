using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using LenixSO.Logger;
using Logger = LenixSO.Logger.Logger;
using System.Text;

public class BattleSetup : MonoBehaviour
{
    [Header("Battle Menu")]
    [SerializeField] private GameObject battleMenu;
    [SerializeField] private ContextSelection battleChoice;
    [SerializeField] private Button enemySummaryButton;
    [SerializeField] private Announcer choiceAnnouncer;

    [Header("Moves Menu")]
    [SerializeField] private FightMenu fightMenu;

    [Header("Party Menu")]
    [SerializeField] private PartyMenu partyChoice;

    [Header("Summary Menu")]
    [SerializeField] private SummaryMenu summary;

    private bool summaryFromParty;
    private int summaryPokemonId;

    //Enemy
    private Pokemon[] enemyParty;

    //Ally
    private Pokemon[] allyParty;

    private void Awake()
    {
        enemySummaryButton.onClick.AddListener(() => OpenPokemonSummary(enemyParty[0]));
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
        if(Keyboard.current.tabKey.wasPressedThisFrame) OpenPokemonSummary(enemyParty[0]);
    }

    public void SetupBattle(Pokemon[] allies, Pokemon[] enemies)
    {
        //setup enemy
        allyParty = allies;
        enemyParty = enemies;
        fightMenu.SetupBattle(allyParty[0], enemyParty[0]);

        List<MoveReference> possibleTMs = MoveHelper.GetPossibleMoves(allyParty[0], new[] { MoveLearnMethod.TM });
        Checklist loadedTMs = new(possibleTMs.Count);
        StringBuilder log = new($"{allyParty[0].name} TM moves are:");
        if (possibleTMs.Count > 0) LoadTMs(possibleTMs[0]);
        
        void LoadTMs(MoveReference moveReference)
        {
            PokeAPI.GetMove(moveReference.move.url, LoadMoveData);
            void LoadMoveData(MoveData moveData)
            {
                PokeAPI.GetTM(moveData, (tm) =>
                {
                    log.Append($"\n{tm.name} => {tm.data.moveData.name}");

                    loadedTMs.FinishStep();
                    if (loadedTMs.isDone)
                    {
                        Logger.Log(log.ToString(), LogFlags.Tests);
                        return;
                    }
                    LoadTMs(possibleTMs[loadedTMs.currentSteps]);
                });
            }
        }

        OpenChoiceMenu();
    }

    private void OnAllyChanged(int newAlly)
    {
        Pokemon cashe = allyParty[newAlly];
        allyParty[newAlly] = allyParty[0];
        allyParty[0] = cashe;
        fightMenu.ChangeAllyPokemon(allyParty[0], true);
        OpenChoiceMenu();
    }

    private void OnMovePick(int id)
    {
        //StartCoroutine(BattleSequence(allyPokemon.moves[id], null));
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
        Announcer.ChangeAnnouncer(choiceAnnouncer);
        StartCoroutine(Announcer.Announce($"What will {allyParty[0].name} do?"));
    }

    private void OpenBattleScene()
    {
        battleMenu.SetActive(false);
        fightMenu.OpenMenu(allyParty[0]);
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
