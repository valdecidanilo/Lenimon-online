using System;
using System.Collections;
using Battle;
using LenixSO.Logger;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CallbackContext = UnityEngine.InputSystem.InputAction.CallbackContext;
using Logger = LenixSO.Logger.Logger;
using Random = UnityEngine.Random;

public class FightMenu : ContextMenu<Pokemon>
{
    [Space(5), SerializeField] private TMP_Text moveType;
    [SerializeField] private TMP_Text movePp;
    [SerializeField] private TMP_Text[] moves;
    [SerializeField] private Announcer battleAnnouncer;
    [SerializeField] private Button backButton;

    #region Battle Visuals
    [Header("Battle Visuals")]
    [SerializeField] Sprite[] healthStates;
    //Enemy
    private Pokemon enemyPokemon;
    [Header("Enemy")] [SerializeField] private Image enemyImage;
    [SerializeField] private TMP_Text enemyName;
    [SerializeField] private TMP_Text enemyLevel;
    [SerializeField] private Image enemyHp;
    [SerializeField] private Image enemyGender;

    //Ally
    private Pokemon allyPokemon;
    [Header("Ally")] [SerializeField] private Image pokemonImage;
    [SerializeField] private TMP_Text pokemonName;
    [SerializeField] private TMP_Text level;
    [SerializeField] private Image hp;
    [SerializeField] private TMP_Text hpValue;
    [SerializeField] private Image xp;
    [SerializeField] private Image gender;
    #endregion

    public event Action<int> onPickMove;

    private static FightMenu instance;

    protected override void Awake()
    {
        instance = this;
        base.Awake();
        contextSelection.onSelect += OnSelectionChanged;
        contextSelection.onItemPick += OnMovePick;
        backButton.onClick.AddListener(() => ReturnCall(new()));
    }

    public override void OpenMenu(Pokemon data)
    {
        base.OpenMenu(data);
        Announcer.ChangeAnnouncer(battleAnnouncer);
        gameObject.SetActive(true);
        for (int i = 0; i < moves.Length; i++)
        {
            moves[i].text = allyPokemon?.moves[i]?.name ?? "-";
        }

        //first selected
        UpdateMoveData(allyPokemon.moves[0]);
        contextSelection.Focus();
    }

    public override void CloseMenu()
    {
        gameObject.SetActive(false);
        base.CloseMenu();
    }

    #region Moves
    private void OnSelectionChanged(int id)
    {
        if (id < 0 || id >= allyPokemon.moves.Length) return;
        UpdateMoveData(allyPokemon.moves[id]);
    }
    private void UpdateMoveData(MoveModel move)
    {
        moveType.text = move?.moveTypeName ?? "-";
        string currentPP = move?.pp.ToString() ?? "-";
        string maxPP = move?.Data.pp.ToString() ?? "-";
        movePp.text = $"{currentPP}/{maxPP}";
    }
    private void OnMovePick(int id)
    {
        if(allyPokemon.moves[id] == null) return;
        Logger.Log($"{allyPokemon.name} will use {allyPokemon.moves[id].name}", LogFlags.Game);
        //onPickMove?.Invoke(id);
        BeginBattle(allyPokemon.moves[id]);
    }
    #endregion

    #region Battle
    public static void BeginBattle(MoveModel playerMove)
    {
        MoveModel opponentMove = ChoseOpponentMove(playerMove);
        instance.StartCoroutine(instance.BattleSequence(playerMove, opponentMove));
    }
    private static MoveModel ChoseOpponentMove(MoveModel playerMove)
    {
        return null;
    }

    public static IEnumerator DelayedStartBattle(MoveModel allyMove)
    {
        instance.OpenMenu(instance.allyPokemon);
        yield return null;
        BeginBattle(allyMove);
    }
    private IEnumerator BattleSequence(MoveModel allyMove, MoveModel opponentMove)
    {
        BattleEvent evtBattle = new();
        evtBattle.user = "You";//or "Opponent"
        evtBattle.move = allyMove;
        evtBattle.origin = allyPokemon;
        evtBattle.target = GetTarget(evtBattle.move.Data.target.name, allyPokemon, enemyPokemon);
        evtBattle.attackEvent = new(allyPokemon, enemyPokemon, evtBattle.move);

        bool hit = evtBattle.attackEvent.CheckHit(out bool missed);

        contextSelection.ReleaseSelection();
        if (hit)
        {
            yield return evtBattle.move.effectMessage.Invoke(evtBattle);

            #region Type Shenanigans
            //Add type chart and STAB modifiers
            //add type modifiers
            Pokemon attacker = evtBattle.origin;
            Pokemon defender = evtBattle.target;
            MoveModel move = evtBattle.move;
            float typeMod = 1;
            //type modifiers
            for (int i = 0; i < defender.types.Length; i++)
                typeMod *= move.moveType.GetMultiplier(defender.types[i]);
            
            //attack type effectiveness text
            string typeEffectMessage = typeMod switch
            {
                1 => string.Empty,
                > 1 => "It's super effective!",
                < 1 and > 0 => "It's not very effective...",
                _ => $"It doesn't affect {defender.name}..."
            };

            //STAB
            for (int i = 0; i < attacker.types.Length; i++)
            {
                if(attacker.types[i] != move.moveType) continue;
                typeMod *= 1.5f;
                break;
            }
            //apply modifier
            evtBattle.attackEvent.modifier *= typeMod;
            #endregion

            bool targetSelf = evtBattle.target == evtBattle.origin;
            if (typeMod != 0 || targetSelf)
                yield return evtBattle.move.effect.EffectSequence(evtBattle);

            typeMod = evtBattle.attackEvent.modifier;//resultant modifier
            bool attackHits = (typeMod != 0 && evtBattle.attackEvent.damageDealt >= 0) || 
                              (typeMod == 0 && evtBattle.attackEvent.damageDealt < 0);
            if (attackHits && !string.IsNullOrEmpty(typeEffectMessage))
                yield return Announcer.Announce(typeEffectMessage, holdTime: 1);
            
            //sub effect
            if (evtBattle.attackEvent.modifier > 0) yield return TriggerSubEffect(evtBattle.move.effect);

            IEnumerator TriggerSubEffect(Effect effect)
            {
                if (effect.subEffect == null) yield break;
                //check chance
                int r = Random.Range(1, 101);
                if (r > effect.subEffectChance) yield break;
                effect.subEffectSetup?.Invoke(evtBattle);
                yield return effect.subEffect.EffectSequence(evtBattle);
                yield return TriggerSubEffect(effect.subEffect);
            }

            if (evtBattle.failed)
            {
                //fail text
            }
        }
        else
        {
            //missed/evaded text
            if (missed)
            {
                yield return Announcer.Announce($"{allyPokemon.name} attack missed.", holdTime: 1f);
            }
            else
            {
                yield return Announcer.Announce($"{enemyPokemon.name} avoided the attack", holdTime: 1f);
            }
        }
        

        //next move
        yield return Announcer.Announce($"{enemyPokemon.name} attacks.", holdTime: 1.5f);
        contextSelection.Focus();
        yield return allyPokemon.DamagePokemon(50);
        Announcer.CloseAnnouncement();
        ReturnCall(new());
        
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
    private IEnumerator AllyHpChanged(int initialValue, int currentValue)
    {
        yield return BattleVFX.LerpHpBar(allyPokemon, initialValue, currentValue, hp, hpValue);
    }
    private IEnumerator OpponentHpChanged(int initialValue, int currentValue)
    {
        yield return BattleVFX.LerpHpBar(enemyPokemon, initialValue, currentValue, enemyHp);
    }
    #endregion

    #region Visual
    public void SetupBattle(Pokemon ally, Pokemon opponent)
    {
        //setup enemy
        enemyPokemon = opponent;
        SetupEnemy(enemyPokemon);
        enemyPokemon.onHpChanged.RegisterCallback(OpponentHpChanged);

        //setup ally
        allyPokemon = ally;
        SetupAlly(allyPokemon);
        allyPokemon.onHpChanged.RegisterCallback(AllyHpChanged);
    }
    private void SetupAlly(Pokemon ally)
    {
        allyPokemon = ally;
        pokemonImage.sprite = allyPokemon.backSprite;
        pokemonName.text = allyPokemon.name;
        level.text = $"Lv{allyPokemon.level}";
        BattleVFX.ChangeHpBar(allyPokemon, hp, allyPokemon.battleStats[StatType.hp], hpValue);
        xp.fillAmount = Random.Range(0, 1);

        PokeDatabase.SetGenderSprite(gender, allyPokemon.gender);
    }
    private void SetupEnemy(Pokemon enemy)
    {
        enemyPokemon = enemy;
        enemyImage.sprite = enemyPokemon.frontSprite;
        enemyName.text = enemyPokemon.name;
        enemyLevel.text = $"Lv{enemyPokemon.level}";
        BattleVFX.ChangeHpBar(enemyPokemon, enemyHp, enemyPokemon.battleStats[StatType.hp]);

        PokeDatabase.SetGenderSprite(gender, enemyPokemon.gender);
    }
    public void ChangeAllyPokemon(Pokemon newAlly, bool animate = false)
    {
        allyPokemon.onHpChanged.RemoveCallback(AllyHpChanged);
        if (!animate)
        {
            SetupAlly(newAlly);
            allyPokemon = newAlly;
            allyPokemon.onHpChanged.RegisterCallback(AllyHpChanged);
        }
        else
        {
            SetupAlly(newAlly);
            allyPokemon = newAlly;
            allyPokemon.onHpChanged.RegisterCallback(AllyHpChanged);
        }
    }
    public void ChangeOpponentPokemon(Pokemon newOpponent, bool animate = false)
    {
        enemyPokemon.onHpChanged.RemoveCallback(OpponentHpChanged);
        if (!animate)
        {
            SetupEnemy(newOpponent);
        }
        enemyPokemon = newOpponent;
        enemyPokemon.onHpChanged.RegisterCallback(OpponentHpChanged);
    }
    #endregion
}
