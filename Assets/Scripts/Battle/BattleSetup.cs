using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Battle;
using Player;
using CallbackContext = UnityEngine.InputSystem.InputAction.CallbackContext;

public class BattleSetup : MonoBehaviour
{
    [Header("Battle Menu")] 
    [SerializeField] private GameObject battleScene;
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

    [HideInInspector] public PlayerEntity currentPlayer;
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
        fightMenu.OnEndBattle += CloseBattle;
        partyChoice.onSummaryCall += OnPokemonSummaryRequest;
        partyChoice.onReturn += OpenChoiceMenu;
        fightMenu.onReturn += OpenChoiceMenu;
        bag.onReturn += OpenChoiceMenu;
        options.onReturn += OpenChoiceMenu;
        battleScene.SetActive(false);
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
        summaryPokemonId = (summaryPokemonId + delta + player.party.Count) % player.party.Count;
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

    private void CloseBattle()
    {
        battleScene.SetActive(false);
        currentPlayer.SetBattleState(false);
        AudioManager.Instance.PlayGame();
    }
    private void EnableEnemySummary(bool disable)
    {
        if (enemySummaryButton == null) return;
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
                OpenBattleMenu();
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
        StartCoroutine(Announcer.AnnounceCoroutine($"What will {player.activePokemon.name.ToUpper()} do?"));
    }

    public void OpenBattleScene()
    {
        battleScene.SetActive(true);
    }
    private void OpenBattleMenu()
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
        StartCoroutine(TryRun());
    }

    private void BattleWin()
    {
        AudioManager.Instance.PlayWin();
        fightMenu.ExitBattle();
    }

    private void BattleLose()
    {
        fightMenu.ExitBattle();
    }
    private IEnumerator TryRun()
    {
        var percent = Random.Range(0, 100);
        var success = percent > 50;
        
        if(success)
        {
            yield return Announcer.AnnounceCoroutine($"Escaped safely.");
            
            /*  Fazer a transição de tela e volta pro jogo.
             
             yield return playerUI.TransitionEndBattle(onComplete =>
            {
                if (!onComplete) return;
                uI.ResetBattleUI();
                Destroy(opponentPokeData.gameObject);
            });
            
            */
            yield return null;
            CloseBattle();
            fightMenu.ExitBattle();
        }
        else
        {
            yield return Announcer.AnnounceCoroutine($"I couldn't escape");
            yield return new WaitForSeconds(1.5f);
            yield return Announcer.AnnounceCoroutine($"What will {player.activePokemon.name.ToUpper()} do?");
        }
    }
    #endregion
}