using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Battle;
using CallbackContext = UnityEngine.InputSystem.InputAction.CallbackContext;

public class BattleSetup : MonoBehaviour
{
    [Header("Battle Menu")]
    [SerializeField] private GameObject battleMenu;
    [SerializeField] private ContextSelection battleChoice;
    [SerializeField] private Button enemySummaryButton;
    [SerializeField] private Announcer choiceAnnouncer;

    [Header("Menus")]
    [SerializeField] private FightMenu fightMenu;
    [SerializeField] private PartyMenu partyChoice;
    [SerializeField] private SummaryMenu summary;
    [SerializeField] private BagMenu bag;
    [SerializeField] private OptionsMenu options;

    private bool summaryFromParty;
    private int summaryPokemonId;
    private InputAction detailsAction;

    private Trainer player;
    private Opponent opponent;

    private void Awake()
    {
        battleChoice.onItemPick += OnChoicePick;
        detailsAction = InputSystem.actions.FindAction("UI/Details");
        summary.onShiftPokemon += SwitchPokemon;
        summary.onReturn += CloseSummary;
        EnableEnemySummary(false);
        fightMenu.onBattleStateChanged += EnableEnemySummary;
        partyChoice.onSummaryCall += OnPokemonSummaryRequest;
        partyChoice.onReturn += OpenChoiceMenu;
        fightMenu.onReturn += OpenChoiceMenu;
        bag.onReturn += OpenChoiceMenu;
        options.onReturn += OpenChoiceMenu;
    }

    private void OnDestroy()
    {
        EnableEnemySummary(true);
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

    private void SwitchPokemon(int delta)
    {
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
            detailsAction.performed += EnemySummaryInput;
            enemySummaryButton.onClick.AddListener(OpenEnemySummary);
            return;
        }

        detailsAction.performed -= EnemySummaryInput;
        enemySummaryButton.onClick.RemoveListener(OpenEnemySummary);
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
        options.CloseMenu();

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
        options.OpenMenu(0);
    }
    #endregion
}