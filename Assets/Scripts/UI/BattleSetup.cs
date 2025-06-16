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

    private Trainer player;
    private Opponent opponent;

    private void Awake()
    {
        detailsAction = InputSystem.actions.FindAction("UI/Details");
        summaryNavigationAction = InputSystem.actions.FindAction("UI/ScrollWheel");
        summaryNavigationAction.performed += SwitchPokemon;
        fightMenu.onBattleStateChanged += EnableEnemySummary;
        EnableEnemySummary(false);
        battleChoice.onItemPick += OnChoicePick;
        fightMenu.onReturn += OpenChoiceMenu;
        partyChoice.onReturn += OpenChoiceMenu;
        partyChoice.onSummaryCall += OnPokemonSummaryRequest;
        summary.onReturn += CloseSummary;
        bag.onReturn += OpenChoiceMenu;
    }

    public void SetupBattle(Trainer Player, Opponent Opponent)
    {
        //setup enemy
        player = Player;
        opponent = Opponent;
        player.activePokemon = player.party[0];
        opponent.activePokemon = opponent.party[0];
        fightMenu.SetupBattle(player, opponent);
        OpenParty();
        OpenBag();
        OpenChoiceMenu();
    }

    #region Summary
    private void OnPokemonSummaryRequest(int id)
    {
        summaryPokemonId = id;
        summaryFromParty = true;
        OpenPokemonSummary(player.party[id]);
    }

    private void SwitchPokemon(CallbackContext context)
    {
        Vector2 direction = context.ReadValue<Vector2>();
        if(direction.y == 0) return;
        int delta = -Mathf.RoundToInt(Mathf.Sign(direction.y));
        if (delta == 0 || !summaryFromParty || !summary.isOpen) return;
        summaryPokemonId = (summaryPokemonId + delta + player.party.Length) % player.party.Length;
        OpenPokemonSummary(player.party[summaryPokemonId]);
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

    private void EnableEnemySummary(bool disable)
    {
        enemySummaryButton.interactable = !disable;
        if (!disable)
        {
            detailsAction.performed -= EnemySummaryInput;
            enemySummaryButton.onClick.RemoveListener(OpenEnemySummary);
            return;
        }

        detailsAction.performed += EnemySummaryInput;
        enemySummaryButton.onClick.AddListener(OpenEnemySummary);
    }

    private void OpenEnemySummary() => OpenPokemonSummary(opponent.activePokemon);

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
        battleChoice.Focus();
        Announcer.ChangeAnnouncer(choiceAnnouncer);
        StartCoroutine(Announcer.AnnounceCoroutine($"What will {player.activePokemon.name} do?"));
    }

    private void OpenBattleScene()
    {
        battleMenu.SetActive(false);
        fightMenu.OpenMenu(player.activePokemon);
    }

    private void OpenBag()
    {
        battleMenu.SetActive(false);
        bag.OpenMenu(player.bag);
    }

    private void OpenParty()
    {
        battleChoice.ReleaseSelection();
        battleMenu.SetActive(false);
        partyChoice.OpenMenu(player.party);
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