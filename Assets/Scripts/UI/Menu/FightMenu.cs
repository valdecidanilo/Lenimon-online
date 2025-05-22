using System;
using System.Collections;
using Battle;
using LenixSO.Logger;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Logger = LenixSO.Logger.Logger;
using Random = UnityEngine.Random;

public class FightMenu : ContextMenu<Pokemon>
{
    [Space(5), SerializeField] private TMP_Text moveType;
    [SerializeField] private TMP_Text movePp;
    [SerializeField] private TMP_Text[] moves;
    [SerializeField] private Announcer battleAnnouncer;

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

    protected override void Awake()
    {
        base.Awake();
        contextSelection.onSelect += OnSelectionChanged;
        contextSelection.onItemPick += OnMovePick;
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
        moveType.text = move.moveType;
        int ppAmount = move.pp;
        movePp.text = $"{ppAmount}/{ppAmount}";
    }
    private void OnMovePick(int id)
    {
        Logger.Log($"{allyPokemon.name} will use {allyPokemon.moves[id].name}", LogFlags.Game);
        //onPickMove?.Invoke(id);
        BeginBattle(allyPokemon.moves[id]);
    }
    #endregion

    #region Battle
    public void BeginBattle(MoveModel playerMove)
    {
        MoveModel opponentMove = null;//chose a move for opponent
        StartCoroutine(BattleSequence(playerMove, opponentMove));
    }
    private IEnumerator BattleSequence(MoveModel allyMove, MoveModel opponentMove)
    {
        BattleEvent evtBattle = new();
        evtBattle.move = allyMove;
        evtBattle.origin = allyPokemon;
        evtBattle.target = GetTarget(evtBattle.move.Data.target.name, allyPokemon, enemyPokemon);
        evtBattle.attackEvent = new(allyPokemon, enemyPokemon, evtBattle.move);

        bool hit = evtBattle.attackEvent.CheckHit(out bool missed, out bool evaded);

        contextSelection.ReleaseSelection();
        if (hit)
        {
            yield return Announcer.Announce($"Allied {allyPokemon.name} used {allyMove.name}", holdTime: 1f);
            yield return evtBattle.move.effect.EffectSequence(evtBattle);
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
                yield return Announcer.Announce($"{allyPokemon.name} attack missed", holdTime: 1f);
            }
            else
            {
                yield return Announcer.Announce($"{enemyPokemon.name} avoided the attack", holdTime: 1f);
            }
        }
        

        //next move
        yield return Announcer.Announce($"{enemyPokemon.name} attacks", holdTime: 1.5f);
        contextSelection.Focus();
        yield return allyPokemon.DamagePokemon(50);
        Announcer.CloseAnnouncement();
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
        float moveTime = .6f;
        float time = 0;
        while (time < moveTime)
        {
            int currentHp = Mathf.RoundToInt(Mathf.Lerp(initialValue, currentValue, time / moveTime));
            hpValue.text = $"{currentHp}/{allyPokemon.stats.hp}";
            ChangeHpBar(allyPokemon, hp, currentHp);
            yield return null;
            time += Time.deltaTime;
        }
        ChangeHpBar(allyPokemon, hp, currentValue);
        hpValue.text = $"{currentValue}/{allyPokemon.stats.hp}";
    }
    private IEnumerator OpponentHpChanged(int initialValue, int currentValue)
    {
        float moveTime = .6f;
        float time = 0;
        while (time < moveTime) 
        {
            ChangeHpBar(enemyPokemon, enemyHp, (int)Mathf.Lerp(initialValue, currentValue, time / moveTime));
            yield return null;
            time += Time.deltaTime;
        }
        ChangeHpBar(enemyPokemon, enemyHp, currentValue);
    }

    private void ChangeHpBar(Pokemon pokemon, Image hpImage, int hpValue)
    {
        hpValue = Mathf.Clamp(hpValue, 0, pokemon.stats.hp);
        float percentage = (float)hpValue / pokemon.stats.hp;
        hpImage.fillAmount = percentage;

        int healthState = 0;
        if (percentage <= .5f) healthState++;
        if (percentage <= .25f) healthState++;

        //change color
        hpImage.sprite = healthStates[healthState];
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
