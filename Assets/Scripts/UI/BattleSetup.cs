using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using LenixSO.Logger;
using Logger = LenixSO.Logger.Logger;
using System.Text;
using CallbackContext = UnityEngine.InputSystem.InputAction.CallbackContext;

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
    [Header("Bag Menu")]
    [SerializeField] private BagMenu bag;

    private bool summaryFromParty;
    private int summaryPokemonId;
    private InputAction detailsAction;
    private InputAction summaryNavigationAction;

    //Enemy
    private Pokemon[] enemyParty;

    //Ally
    private Pokemon[] allyParty;
    private Bag playerBag;

    private void Awake()
    {
        detailsAction = InputSystem.actions.FindAction("UI/Details");
        summaryNavigationAction = InputSystem.actions.FindAction("UI/ScrollWheel");
        summaryNavigationAction.performed += SwitchPokemon;
        EnableEnemySummary(true);
        battleChoice.onItemPick += OnChoicePick;
        fightMenu.onReturn += OpenChoiceMenu;
        partyChoice.onReturn += OpenChoiceMenu;
        partyChoice.onChangePokemon += OnAllyChanged;
        partyChoice.onSummaryCall += OnPokemonSummaryRequest;
        summary.onReturn += CloseSummary;
        bag.onReturn += OpenChoiceMenu;
    }

    public void SetupBattle(Pokemon[] allies, Pokemon[] enemies, Bag bag)
    {
        //setup enemy
        allyParty = allies;
        enemyParty = enemies;
        playerBag = bag;
        fightMenu.SetupBattle(allyParty[0], enemyParty[0]);
        OpenParty();
        OpenBag();
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

    #region Summary
    private void OnPokemonSummaryRequest(int id)
    {
        summaryPokemonId = id;
        summaryFromParty = true;
        OpenPokemonSummary(allyParty[id]);
    }

    private void SwitchPokemon(CallbackContext context)
    {
        Vector2 direction = context.ReadValue<Vector2>();
        if(direction.y == 0) return;
        int delta = -Mathf.RoundToInt(Mathf.Sign(direction.y));
        if (delta == 0 || !summaryFromParty || !summary.isOpen) return;
        summaryPokemonId = (summaryPokemonId + delta + allyParty.Length) % allyParty.Length;
        OpenPokemonSummary(allyParty[summaryPokemonId]);
    }

    private void CloseSummary()
    {
        summary.CloseMenu();
        if(summaryFromParty)
            OpenParty();
        else
            OpenChoiceMenu();
        summaryFromParty = false;
    }

    private void EnableEnemySummary(bool enable)
    {
        if (enable)
        {
            detailsAction.performed += EnemySummaryInput;
            enemySummaryButton.onClick.AddListener(OpenEnemySummary);
            return;
        }

        detailsAction.performed -= EnemySummaryInput;
        enemySummaryButton.onClick.RemoveListener(OpenEnemySummary);
    }

    private void OpenEnemySummary() => OpenPokemonSummary(enemyParty[0]);

    private void EnemySummaryInput(CallbackContext context) => OpenEnemySummary();
    #endregion

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
        bag.CloseMenu();

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
        battleMenu.SetActive(false);
        bag.OpenMenu(playerBag);
    }

    private void OpenParty()
    {
        battleChoice.ReleaseSelection();
        battleMenu.SetActive(false);
        partyChoice.OpenMenu(allyParty);
    }

    private void OpenPokemonSummary(Pokemon pokemon)
    {
        battleChoice.ReleaseSelection();
        battleMenu.SetActive(false);
        partyChoice.CloseMenu();
        summary.OpenMenu((summaryFromParty, pokemon));
    }

    private void Run()
    {
        
    }
    #endregion
}
