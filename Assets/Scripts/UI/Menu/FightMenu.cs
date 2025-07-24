using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Battle;
using LenixSO.Logger;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;
using Logger = LenixSO.Logger.Logger;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class FightMenu : ContextMenu<Pokemon>
{
    [SerializeField] private GameObject fightMenu;
    [Space(5), SerializeField] private TMP_Text moveType;
    [SerializeField] private TMP_Text movePp;
    [SerializeField] private TMP_Text[] moves;
    [SerializeField] private Announcer battleAnnouncer;
    [SerializeField] private Button backButton;
    public Action OnEndBattle;
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
        fightMenu.SetActive(true);

        //check if any move has pp
        var canOnlyStruggle = data.moves.All(currentMove => currentMove == null || currentMove.pp <= 0);
        movesData = canOnlyStruggle ? new [] { MoveHelper.Struggle(), null, null, null } : data.moves;

        for (var i = 0; i < moves.Length; i++)
            moves[i].text = movesData[i]?.name ?? "-";

        //first selected
        UpdateMoveData(movesData[0]);
        contextSelection.Focus();
    }

    public override void CloseMenu()
    {
        fightMenu.SetActive(false);
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
        var currentPP = move?.pp.ToString() ?? "-";
        var maxPP = move?.maxPP.ToString() ?? "-";
        movePp.text = $"{currentPP}/{maxPP}";
    }
    private void OnMovePick(int id)
    {
        if(movesData[id] == null) return;
        
        //onPickMove?.Invoke(id);
        BeginBattle(movesData[id]);
    }
    #endregion

    #region Static Methods
    public static void BeginBattle(MoveModel playerMove)
    {
        if (!instance.fightMenu.activeSelf) instance.OpenMenu(instance.player.activePokemon);
        
        if (playerMove.pp <= 0)
        {
            Announcer.Announce("You don't have pp for this move!!", awaitInput: true, onDone: () =>
            {
                Announcer.CloseAnnouncement();
                instance.contextSelection.Focus();
            });
            return;
        }
        MoveModel opponentMove = instance.ChoseOpponentMove();
        instance.StartCoroutine(instance.BattleSequence(playerMove, opponentMove));
    }
    public static IEnumerator StatusChangeEffect(Pokemon target, bool buff)
    {
        BattlePokemon targetPokemon = instance.GetBattlePokemon(target);
        if (ReferenceEquals(targetPokemon, null)) yield break;
        yield return targetPokemon.StatChangeAnimation(buff);
    }
    public static IEnumerator ChangePokemon(Trainer trainer, int pokemonPartyId)
    {
        BattlePokemon battlePokemon;
        int side = 1;
        trainer.activePokemon.ResetBattleStats();
        if (trainer == instance.player)
        {
            trainer.party[0] = trainer.party[pokemonPartyId];
            trainer.party[pokemonPartyId] = trainer.activePokemon;
            pokemonPartyId = 0;
            trainer.activePokemon.onHpChanged.RemoveCallback(instance.AllyHpChanged);
            battlePokemon = instance.pokemonImage;
        }
        else
        {
            trainer.activePokemon.onHpChanged.RemoveCallback(instance.OpponentHpChanged);
            battlePokemon = instance.enemyImage;
            side = -1;
        }

        bool skipSwitchOut = trainer.activePokemon.fainted;
        trainer.activePokemon = trainer.party[pokemonPartyId];
        yield return instance.ChangePokemonSequence(battlePokemon, trainer, trainer.activePokemon, skipSwitchOut, side);
    }
    public static void EnableFightAnnouncer() => Announcer.ChangeAnnouncer(instance.battleAnnouncer);
    #endregion

    #region Battle
    private MoveModel ChoseOpponentMove() => opponent.ChooseMove(player.activePokemon);
    private IEnumerator BattleSequence(MoveModel allyMove, MoveModel opponentMove)
    {
        //remove callbacks
        cancelAction.performed -= ReturnCall;
        Debug.Log($"{allyMove.name} | {opponentMove.name}");
        onBattleStateChanged?.Invoke(true);
        player.pickedMove ??= allyMove;
        opponent.pickedMove ??= opponentMove;
        //calculate witch one goes first
        List<Trainer> trainerOrder = new(2) { player };
        var opponentFirst = false;
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

        //each take its turns
        var flinchNext = false;
        for (var i = 0; i < trainerOrder.Count; i++)
        {
            var otherTrainer = trainerOrder[(i + 1) % trainerOrder.Count];

            BattleEvent evt = new();
            evt.flinchTarget = flinchNext;
            evt.move = trainerOrder[i].pickedMove;
            evt.user = trainerOrder[i];
            evt.origin = evt.user.activePokemon;
            evt.targetTrainer = GetTarget(evt.move.Data.target.name, evt.user, otherTrainer);
            evt.target = evt.targetTrainer.activePokemon;
            evt.attackEvent = new(evt.origin, evt.target, evt.move);
            yield return TurnSequence(evt);
            flinchNext = evt.flinchTarget;
            foreach (var t in trainerOrder.Where(t => t.activePokemon.fainted)) yield return ReplaceFaintedPokemon(t);

            if (evt.user != evt.targetTrainer && 
                evt.target != evt.targetTrainer.activePokemon) break;
        }

        player.pickedMove = null;
        opponent.pickedMove = null;
        
        //return callbacks
        cancelAction.performed += ReturnCall;
        
        contextSelection.Focus();
        Announcer.CloseAnnouncement();
        ReturnCall(new());
        onBattleStateChanged?.Invoke(false);

        yield break;
        
        IEnumerator TurnSequence(BattleEvent evtBattle)
        {
            if (evtBattle.flinchTarget)
            {
                yield return Announcer.AnnounceCoroutine($"{evtBattle.origin.name.ToUpper()} flinched!", holdTime: .6f);
                yield break;
            }

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
                _ => $"It doesn't affect {defender.name.ToUpper()}..."
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

            var hit = evtBattle.attackEvent.CheckHit(out bool missed);

            contextSelection.ReleaseSelection();
            yield return evtBattle.move.effectMessage?.Invoke(evtBattle);
            evtBattle.move.pp--;
            if (hit)
            {
                var targetSelf = evtBattle.target == evtBattle.origin;
                if (typeMod != 0 || targetSelf)
                {
                    yield return GetBattlePokemon(evtBattle.origin)
                        ?.MoveAnimation(evtBattle.move.typeOfMove, GetBattlePokemon(evtBattle.target));
                    yield return evtBattle.move.effect.EffectSequence(evtBattle);
                }

                typeMod = evtBattle.attackEvent.modifier;//resultant modifier
                var attackHits = (typeMod != 0 && evtBattle.attackEvent.damageDealt >= 0) ||
                                  (typeMod == 0 && evtBattle.attackEvent.damageDealt < 0);
                if (attackHits && !string.IsNullOrEmpty(typeEffectMessage) && !targetSelf)
                    yield return Announcer.AnnounceCoroutine(typeEffectMessage, holdTime: 1);

                //sub effect
                if (evtBattle.attackEvent.modifier > 0)
                    yield return TriggerSubEffect(evtBattle.move.effect);

                IEnumerator TriggerSubEffect(Effect effect)
                {
                    if (effect.subEffect == null) yield break;
                    //check chance
                    var r = Random.Range(1, 101);
                    if (r > effect.subEffectChance) yield break;
                    effect.subEffectSetup?.Invoke(evtBattle);
                    if (evtBattle.target.fainted) yield break;
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
                    yield return Announcer.AnnounceCoroutine($"{evtBattle.origin.name.ToUpper()} attack missed.", holdTime: 1f);
                else
                    yield return Announcer.AnnounceCoroutine($"{evtBattle.target.name.ToUpper()} avoided the attack.", holdTime: 1f);
            }
        }

        IEnumerator ReplaceFaintedPokemon(Trainer trainer)
        {
            bool pickedPokemon = false;
            while (!pickedPokemon)
            {
                PickPokemonEvent pickEvent = new(fainted: false, current: false);
                yield return trainer.PickPokemon(pickEvent);
                if (!pickEvent.noValidPokemon)
                {
                    if (!(pickEvent.partyId < 0))//not canceled
                    {
                        EnableFightAnnouncer();
                        yield return Announcer.AnnounceCoroutine("");
                        if (pickEvent.partyId < 0 || pickEvent.pickedPokemon.fainted) continue;
                        yield return ChangePokemon(trainer, pickEvent.partyId);
                        pickedPokemon = true;
                    }
                }
                else //defeated
                {
                    if (trainer == player)
                    {
                        //reload scene
                        yield return Announcer.AnnounceCoroutine($"{player.name.ToUpper()} were defeated by {opponent.name.ToUpper()}...", true, .5f);
                    }
                    else
                    {
                        //TODO: load new enemy
                        yield return Announcer.AnnounceCoroutine($"{player.name.ToUpper()} defeated {opponent.name.ToUpper()}!", true, .5f);
                    }
                    OnEndBattle?.Invoke();
                }
            }
        }
    }
    private Trainer GetTarget(string target, Trainer self, Trainer other)
    {
        return target switch
        {
            "user" => self,
            "user-and-allies" => self,
            "selected-pokemon" => other,
            "all-opponents" => other,
            "all-other-pokemon" => other,
            _ => other,
        };
    }
    private BattlePokemon GetBattlePokemon(Pokemon target)
    {
        BattlePokemon targetPokemon = null;
        if (target == instance.player.activePokemon) targetPokemon = instance.pokemonImage;
        else if (target == instance.opponent.activePokemon) targetPokemon = instance.enemyImage;
        return targetPokemon;
    }
    #endregion

    #region Visual
    public void SetupBattle(Trainer ally, Opponent enemy)
    {
        //setup enemy
        opponent = enemy;
        SetupEnemy(opponent.activePokemon);
        enemyImage.SaveAsDefaultValues();
        opponent.activePokemon.onHpChanged.RegisterCallback(OpponentHpChanged);

        //setup ally
        player = ally;
        SetupAlly(player.activePokemon);
        pokemonImage.SaveAsDefaultValues();
        player.activePokemon.onHpChanged.RegisterCallback(AllyHpChanged);
    }

    public void ExitBattle()
    {
        opponent.activePokemon.onHpChanged.RemoveCallback(OpponentHpChanged);
        player.activePokemon.onHpChanged.RemoveCallback(AllyHpChanged);
    }
    private void SetupAlly(Pokemon ally)
    {
        var pokemon = ally;
        SetupAllySprite(pokemon);
        pokemonName.text = pokemon.name.ToUpper();
        level.text = $"Lv{pokemon.level}";
        BattleVFX.ChangeHpBar(pokemon, hp, pokemon.battleStats[StatType.hp], hpValue);
        xp.fillAmount = Random.Range(0, 1);

        PokeDatabase.SetGenderSprite(gender, pokemon.gender);
    }
    private void SetupAllySprite(Pokemon pokemon)
    {
        pokemonImage.image.sprite = pokemon.backSprite ?? pokemon.frontSprite;
        pokemonImage.image.transform.localScale = Vector3.up + (!pokemon.backSprite ? Vector3.left : Vector3.right);
    }
    private void SetupEnemy(Pokemon newPokemon)
    {
        var pokemon = newPokemon;
        enemyImage.image.sprite = pokemon.frontSprite;
        enemyName.text = pokemon.name.ToUpper();
        enemyLevel.text = $"Lv{pokemon.level}";
        BattleVFX.ChangeHpBar(pokemon, enemyHp, pokemon.battleStats[StatType.hp]);

        PokeDatabase.SetGenderSprite(gender, pokemon.gender);
    }
    private IEnumerator AllyHpChanged(int initialValue, int currentValue)
    {
        yield return BattleVFX.LerpHpBar(player.activePokemon, initialValue, currentValue, hp, hpValue);
        if (currentValue <= 0) yield return pokemonImage.FaintAnimation();
    }
    private IEnumerator OpponentHpChanged(int initialValue, int currentValue)
    {
        yield return BattleVFX.LerpHpBar(opponent.activePokemon, initialValue, currentValue, enemyHp);
        if (currentValue <= 0) yield return enemyImage.FaintAnimation();
    }

    private IEnumerator ChangePokemonSequence(BattlePokemon battlePokemon, Trainer trainer, Pokemon newPokemon,
        bool skipSwitchOut, int side = 1)
    {
        if (!skipSwitchOut)
            yield return battlePokemon.SwitchOutAnimation(side);
        yield return new WaitForSeconds(.6f);
        yield return Announcer.AnnounceCoroutine($"{trainer.name.ToUpper()} sent out {newPokemon.name.ToUpper()}.", holdTime: .6f);
        Sprite sprite;
        if (side == 1)
        {
            battlePokemon.ResetBattlePokemon();
            SetupAlly(newPokemon);
            battlePokemon.SaveAsDefaultValues();
            sprite = battlePokemon.image.sprite;
            player.activePokemon.onHpChanged.RegisterCallback(AllyHpChanged);
        }
        else
        {
            SetupEnemy(newPokemon);
            sprite = newPokemon.frontSprite;
            opponent.activePokemon.onHpChanged.RegisterCallback(OpponentHpChanged);
        }

        yield return battlePokemon.SwitchInAnimation(sprite, side);
    }

    #endregion
}