using System;
using System.Collections.Generic;
using UnityEngine;

public class Pokemon
{
    public PokemonData data { get; private set; }
    public string name;
    public int id { get; private set; }

    #region Stats
    //stats
    public int level { get; private set; }
    public int hp { get; private set; }
    public int atk { get; private set; }
    public int def { get; private set; }
    public int sAtk { get; private set; }
    public int sDef { get; private set; }
    public int spd { get; private set; }

    private int ivHp;
    private int ivAtk;
    private int ivDef;
    private int ivSatk;
    private int ivSdef;
    private int ivSpd;

    private int evHp;
    private int evAtk;
    private int evDef;
    private int evSatk;
    private int evSdef;
    private int evSpd;

    private float nAtk = 1;
    private float nDef = 1;
    private float nSatk = 1;
    private float nSdef = 1;
    private float nSpd = 1;
    #endregion

    //MetaData
    public Sprite frontSprite { get; private set; }
    public Sprite backSprite { get; private set; }
    public MoveReference[] moves;

    Checklist dataChecklist;
    public Action onDoneLoading;

    public Pokemon(PokemonData pokemonData, int lv = 1)
    {
        data = pokemonData;
        name = pokemonData.name;
        id = pokemonData.id;

        hp = pokemonData.hpStat;
        atk = pokemonData.atkStat;
        def = pokemonData.defStat;
        sAtk = pokemonData.sAtkStat;
        sDef = pokemonData.sDefStat;
        spd = pokemonData.spdStat;

        LevelUp(lv);

        dataChecklist = new(2);
        dataChecklist.onCompleted += () => onDoneLoading?.Invoke();
    }

    public static void GetLoadedPokemon(PokemonData pokemonData, int level = 1, Action<Pokemon> onPokemonLoaded = null)
    {
        Pokemon pokemon = new(pokemonData, level);
        pokemon.onDoneLoading += PokemonLoaded;
        pokemon.GetRandomMoves();
        pokemon.LoadRequiredData();

        void PokemonLoaded()
        {
            pokemon.onDoneLoading -= PokemonLoaded;
            onPokemonLoaded?.Invoke(pokemon);
        }
    }

    public void LoadRequiredData()
    {
        PokeAPI.GetSprite(data, (sprite) =>
        {
            frontSprite = sprite;
            dataChecklist.FinishStep();
        });
        PokeAPI.GetSprite(data, (sprite) =>
        {
            backSprite = sprite;
            dataChecklist.FinishStep();
        }, true);
    }

    public void LevelUp(int amount)
    {
        if (amount <= 0) return;

        level += amount;
        hp = BasicStatCalculation(data.hpStat, ivHp, evHp, level) + level + 10;
        atk = Mathf.FloorToInt((BasicStatCalculation(data.atkStat, ivAtk, evAtk, level) + 5) * nAtk);
        def = Mathf.FloorToInt((BasicStatCalculation(data.defStat, ivDef, evDef, level) + 5) * nDef);
        sAtk = Mathf.FloorToInt((BasicStatCalculation(data.sAtkStat, ivSatk, evSatk, level) + 5) * nSatk);
        sDef = Mathf.FloorToInt((BasicStatCalculation(data.sDefStat, ivSdef, evSdef, level) + 5) * nSdef);
        spd = Mathf.FloorToInt((BasicStatCalculation(data.spdStat, ivSpd, evSpd, level) + 5) * nSpd);

        int BasicStatCalculation(int baseStat, int iv, int ev, int level)
        {
            return Mathf.Max(0, (((2 * baseStat) + iv + (ev / 4)) * level) / 100);
        }
    }

    private void GetRandomMoves()
    {
        MoveReference[] newMoves = new MoveReference[4];

        for (int i = 0; i < newMoves.Length; i++)
        {
            if (data.moves.Count <= i) break;
            newMoves[i] = data.moves[i];
        }

        moves = newMoves;
    }
}
