using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

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
    public MoveModel[] moves;

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

        moves = new MoveModel[4];
        LevelUp(Mathf.Max(lv, 1));//minimum level is 1

        dataChecklist = new(6);
        dataChecklist.onCompleted += () => onDoneLoading?.Invoke();
    }

    public static void GetLoadedPokemon(PokemonData pokemonData, int level = 1, Action<Pokemon> onPokemonLoaded = null)
    {
        Pokemon pokemon = new(pokemonData, level);
        pokemon.onDoneLoading += PokemonLoaded;
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

        GetRandomMoves();
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
        List<MoveReference> possibleMoves = new(data.moves.Count);

        //get only level up moves
        for (int i = 0; i < data.moves.Count; i++)
        {
            MoveReference move = data.moves[i];
            for (int j = 0; j < move.learningDetails.Count; j++)
            {
                LearningDetail learningDetail = move.learningDetails[j];
                if (learningDetail.learnMethod != MoveLearnMethod.LevelUp) continue;
                if (learningDetail.level > level) continue;

                possibleMoves.Add(move);
                break;
            }
        }

        if(possibleMoves.Count < 4)
        {
            int difference = 4 - possibleMoves.Count;
            for (int i = 0; i < difference; i++)
            {
                dataChecklist.FinishStep();
            }
        }

        MoveReference[] newMoves = new MoveReference[4];
        int moveAmount = Mathf.Min(possibleMoves.Count, 4);
        for (int i = 0; i < moveAmount; i++)
        {
            int moveId = Random.Range(0, possibleMoves.Count);
            newMoves[i] = possibleMoves[moveId];
            possibleMoves.RemoveAt(moveId);
            int id = i;
            PokeAPI.GetMove(newMoves[i].move.name, (data) =>
            {
                moves[id] = new MoveModel(data);
                dataChecklist.FinishStep();
            });
        }
    }
}
