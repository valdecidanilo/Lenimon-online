using System;
using System.Collections;
using System.Collections.Generic;
using Battle;
using LenixSO.Logger;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Logger = LenixSO.Logger.Logger;
using Object = UnityEngine.Object;
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
    private Opponent opponent;
    [Header("Enemy")] [SerializeField] private BattlePokemon enemyImage;
    [SerializeField] private TMP_Text enemyName;
    [SerializeField] private TMP_Text enemyLevel;
    [SerializeField] private Image enemyHp;
    [SerializeField] private Image enemyGender;

    //Ally
    private Trainer player;
    [Header("Ally")] [SerializeField] private BattlePokemon pokemonImage;
    [SerializeField] private TMP_Text pokemonName;
    [SerializeField] private TMP_Text level;
    [SerializeField] private Image hp;
    [SerializeField] private TMP_Text hpValue;
    [SerializeField] private Image xp;
    [SerializeField] private Image gender;
    #endregion

    private MoveModel[] movesData;

    public event Action<bool> onBattleStateChanged;

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

        //check if any move has pp
        bool canOnlyStruggle = true;
        for (int i = 0; i < data.moves.Length; i++)
        {
            if (data.moves[i] != null && data.moves[i].pp > 0)
            {
                canOnlyStruggle = false; 
                break;
            }
        }
        movesData = canOnlyStruggle ? new MoveModel[4] { MoveHelper.Struggle(), null, null, null } : data.moves;

        for (int i = 0; i < moves.Length; i++)
            moves[i].text = movesData[i]?.name ?? "-";

        //first selected
        UpdateMoveData(movesData[0]);
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
        if (id < 0 || id >= movesData.Length) return;
        UpdateMoveData(movesData[id]);
    }
    private void UpdateMoveData(MoveModel move)
    {
        moveType.text = move?.moveTypeName ?? "-";
        string currentPP = move?.pp.ToString() ?? "-";
        string maxPP = move?.maxPP.ToString() ?? "-";
        movePp.text = $"{currentPP}/{maxPP}";
    }
    private void OnMovePick(int id)
    {
        if(movesData[id] == null) return;
        Logger.Log($"{player.name} will use {movesData[id].name}", LogFlags.Game);
        //onPickMove?.Invoke(id);
        BeginBattle(movesData[id]);
    }
    #endregion

    #region Battle
    public static void BeginBattle(MoveModel playerMove)
    {
        if (playerMove.pp <= 0)
        {
            Announcer.Announce("You don't have pp for this move!!", awaitInput: true,  onDone: ()=>
            {
                Announcer.CloseAnnouncement();
                instance.contextSelection.Focus();
            });
            return;
        }
        MoveModel opponentMove = instance.ChoseOpponentMove();
        instance.StartCoroutine(instance.BattleSequence(playerMove, opponentMove));
    }
    private MoveModel ChoseOpponentMove() => opponent.ChooseMove(player.activePokemon);

    public static IEnumerator DelayedStartBattle(MoveModel allyMove)
    {
        instance.OpenMenu(instance.player.activePokemon);
        yield return null;
        BeginBattle(allyMove);
    }
    public static IEnumerator StatusChangeEffect(Pokemon target, bool buff)
    {
        BattlePokemon targetPokemon = instance.GetBattlePokemon(target);
        if (ReferenceEquals(targetPokemon, null)) yield break;
        yield return targetPokemon.StatChangeAnimation(buff);
    }
    private IEnumerator BattleSequence(MoveModel allyMove, MoveModel opponentMove)
    {
        onBattleStateChanged?.Invoke(true);
        player.pickedMove ??= allyMove;
        opponent.pickedMove ??= opponentMove;
        //calculate witch one goes first
        List<Trainer> trainerOrder = new(2) { player };
        bool opponentFirst = false;
        if (opponentMove.priority >= allyMove.priority)
        {
            if (opponentMove.priority == allyMove.priority)
            {
                int allySpeed = PokeDatabase.CalculateModifiedStat(
                    player.activePokemon.stats[StatType.spd],
                    player.activePokemon.battleStats[StatType.spd]);

                int opponentSpeed = PokeDatabase.CalculateModifiedStat(
                    opponent.activePokemon.stats[StatType.spd],
                    opponent.activePokemon.battleStats[StatType.spd]);
                opponentFirst = opponentSpeed > allySpeed;
            }
            else opponentFirst = true;
        }
        trainerOrder.Insert(opponentFirst ? 0 : 1, opponent);

        for (int i = 0; i < trainerOrder.Count; i++)
        {
            Pokemon nextPokemon = trainerOrder[(i + 1) % trainerOrder.Count].activePokemon;

            BattleEvent evt = new();
            evt.user = trainerOrder[i];
            evt.move = trainerOrder[i].pickedMove;
            evt.origin = evt.user.activePokemon;
            evt.target = GetTarget(evt.move.Data.target.name, evt.user.activePokemon, nextPokemon);
            evt.attackEvent = new(evt.origin, evt.target, evt.move);
            yield return TurnSequence(evt);
        }

        player.pickedMove = null;
        opponent.pickedMove = null;
        contextSelection.Focus();
        Announcer.CloseAnnouncement();
        ReturnCall(new());
        onBattleStateChanged?.Invoke(false);

        yield break;
        
        IEnumerator TurnSequence(BattleEvent evtBattle)
        {
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
                if (attacker.types[i] != move.moveType) continue;
                typeMod *= 1.5f;
                break;
            }
            //apply modifier
            evtBattle.attackEvent.modifier *= typeMod;
            #endregion

            bool hit = evtBattle.attackEvent.CheckHit(out bool missed);

            contextSelection.ReleaseSelection();
            yield return evtBattle.move.effectMessage.Invoke(evtBattle);
            evtBattle.move.pp--;
            if (hit)
            {
                bool targetSelf = evtBattle.target == evtBattle.origin;
                if (typeMod != 0 || targetSelf)
                {
                    yield return GetBattlePokemon(evtBattle.origin)
                        ?.MoveAnimation(evtBattle.move.typeOfMove, GetBattlePokemon(evtBattle.target));
                    yield return evtBattle.move.effect.EffectSequence(evtBattle);
                }

                typeMod = evtBattle.attackEvent.modifier;//resultant modifier
                bool attackHits = (typeMod != 0 && evtBattle.attackEvent.damageDealt >= 0) ||
                                  (typeMod == 0 && evtBattle.attackEvent.damageDealt < 0);
                if (attackHits && !string.IsNullOrEmpty(typeEffectMessage) && !targetSelf)
                    yield return Announcer.AnnounceCoroutine(typeEffectMessage, holdTime: 1);

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
                    yield return Announcer.AnnounceCoroutine($"But it failed!", holdTime: 1f);
                }
            }
            else
            {
                //missed/evaded text
                if (missed)
                    yield return Announcer.AnnounceCoroutine($"{evtBattle.origin.name} attack missed.", holdTime: 1f);
                else
                    yield return Announcer.AnnounceCoroutine($"{evtBattle.target.name} avoided the attack", holdTime: 1f);
            }
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
            _ => opponent,
        };
    }

    private BattlePokemon GetBattlePokemon(Pokemon target)
    {
        BattlePokemon targetPokemon = null;
        if (target == instance.player.activePokemon) targetPokemon = instance.pokemonImage;
        else if (target == instance.opponent.activePokemon) targetPokemon = instance.enemyImage;
        return targetPokemon;
    }
    private IEnumerator AllyHpChanged(int initialValue, int currentValue)
    {
        yield return BattleVFX.LerpHpBar(player.activePokemon, initialValue, currentValue, hp, hpValue);
    }
    private IEnumerator OpponentHpChanged(int initialValue, int currentValue)
    {
        yield return BattleVFX.LerpHpBar(opponent.activePokemon, initialValue, currentValue, enemyHp);
    }
    #endregion

    #region Visual
    public void SetupBattle(Trainer ally, Opponent enemy)
    {
        //setup enemy
        opponent = enemy;
        SetupEnemy(opponent.activePokemon);
        opponent.activePokemon.onHpChanged.RegisterCallback(OpponentHpChanged);

        //setup ally
        player = ally;
        SetupAlly(player.activePokemon);
        player.activePokemon.onHpChanged.RegisterCallback(AllyHpChanged);
    }
    private void SetupAlly(Pokemon ally)
    {
        player.activePokemon = ally;
        var pokemom = ally;
        pokemonImage.image.sprite = pokemom.backSprite;
        if (pokemonImage.image.sprite == null)
        {
            pokemonImage.image.sprite = pokemom.frontSprite;
            pokemonImage.transform.transform.localScale = Vector3.up + Vector3.left;
        }
        pokemonName.text = pokemom.name;
        level.text = $"Lv{pokemom.level}";
        BattleVFX.ChangeHpBar(pokemom, hp, pokemom.battleStats[StatType.hp], hpValue);
        xp.fillAmount = Random.Range(0, 1);

        PokeDatabase.SetGenderSprite(gender, pokemom.gender);
    }
    private void SetupEnemy(Pokemon newPokemon)
    {
        opponent.activePokemon = newPokemon;
        var pokemon = newPokemon;
        enemyImage.image.sprite = pokemon.frontSprite;
        enemyName.text = pokemon.name;
        enemyLevel.text = $"Lv{pokemon.level}";
        BattleVFX.ChangeHpBar(pokemon, enemyHp, pokemon.battleStats[StatType.hp]);

        PokeDatabase.SetGenderSprite(gender, pokemon.gender);
    }
    public void ChangeAllyPokemon(Pokemon newAlly, bool animate = false)
    {
        player.activePokemon.onHpChanged.RemoveCallback(AllyHpChanged);
        if (!animate)
        {
            SetupAlly(newAlly);
            player.activePokemon = newAlly;
            player.activePokemon.onHpChanged.RegisterCallback(AllyHpChanged);
        }
        else
        {
            SetupAlly(newAlly);
            player.activePokemon = newAlly;
            player.activePokemon.onHpChanged.RegisterCallback(AllyHpChanged);
        }
    }
    public void ChangeOpponentPokemon(Pokemon newOpponent, bool animate = false)
    {
        opponent.activePokemon.onHpChanged.RemoveCallback(OpponentHpChanged);
        if (!animate)
        {
            SetupEnemy(newOpponent);
        }
        opponent.activePokemon = newOpponent;
        opponent.activePokemon.onHpChanged.RegisterCallback(OpponentHpChanged);
    }
    #endregion
}
