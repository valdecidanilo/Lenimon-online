using LenixSO.Logger;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Logger = LenixSO.Logger.Logger;
using System.Globalization;

public class Pokemon
{
    public PokemonData data { get; private set; }
    public string name;
    public int id { get; private set; }
    public Gender gender { get; private set; }

    #region Stats
    //stats
    public int level { get; private set; }
    public Stats stats { get; private set; }
    public Stats iv{ get; private set; }
    public Stats ev{ get; private set; }
    public Stats nature{ get; private set; }
    public Stats battleStats{ get; private set; }
    #endregion
    
    //MetaData
    public Sprite frontSprite { get; private set; }
    public Sprite backSprite { get; private set; }
    public Sprite icon { get; private set; }
    public AbilityData ability;
    public MoveModel[] moves;

    //Data loading
    private Checklist dataChecklist;
    public Action onDoneLoading;

    public Pokemon(PokemonData pokemonData, int lv = 1)
    {
        data = pokemonData;
        name = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(pokemonData.name);
        int formId = name.IndexOf('-');
        if (formId > 0)
        {
            name = name.Remove(formId);
        }
        id = pokemonData.id;

        stats = new(
            pokemonData.hpStat,
            pokemonData.atkStat,
            pokemonData.defStat,
            pokemonData.sAtkStat,
            pokemonData.sDefStat,
            pokemonData.spdStat
        );

        iv = new(0, 0, 0, 0, 0, 0, 0, 0);
        ev = new(0, 0, 0, 0, 0, 0, 0, 0);
        nature = new(100, 100, 100, 100, 100, 100, 100, 100);

        moves = new MoveModel[4];
        LevelUp(Mathf.Max(lv, 1));//minimum level is 1

        battleStats = stats;

        dataChecklist = new(0);
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
        LoadSprites();
        GetGender();
        GetAbility();
        GetNature();
        GetRandomMoves();
    }

    public void LevelUp(int amount)
    {
        if (amount <= 0) return;

        level += amount;
        Stats newStats = stats;
        newStats.hp = BasicStatCalculation(data.hpStat, iv[StatType.hp], ev[StatType.hp], level) + level + 10;
        newStats.atk = NonHpCalculation(StatType.atk);
        newStats.def = NonHpCalculation(StatType.def);
        newStats.sAtk = NonHpCalculation(StatType.sAtk);
        newStats.sDef = NonHpCalculation(StatType.sDef);
        newStats.spd = NonHpCalculation(StatType.spd);

        stats = newStats;

        int BasicStatCalculation(int baseStat, int iv, int ev, int level)
        {
            return Mathf.Max(0, (((2 * baseStat) + iv + (ev / 4)) * level) / 100);
        }

        int NonHpCalculation(StatType type)
        {
            return Mathf.FloorToInt((BasicStatCalculation(data.stats[(int)type].base_stat, iv[type], ev[type], level) + 5) * (nature[type] / 100f));
        }
    }


    #region Data Load
    private void LoadSprites()
    {
        dataChecklist.AddStep(3);
        PokeAPI.GetSprite(data, (sprite) =>
        {
            frontSprite = sprite;
            Logger.Log($"{name} Front sprite done loading", LogFlags.PokemonBuild);
            dataChecklist.FinishStep();
        });
        PokeAPI.GetSprite(data, (sprite) =>
        {
            backSprite = sprite;
            Logger.Log($"{name} Back sprite done loading", LogFlags.PokemonBuild);
            dataChecklist.FinishStep();
        }, true);
        PokeAPI.GetIcon(data, (sprite) =>
        {
            icon = sprite;
            Logger.Log($"{name} Icon done loading", LogFlags.PokemonBuild);
            dataChecklist.FinishStep();
        });
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

        MoveReference[] newMoves = new MoveReference[4];
        int moveAmount = Mathf.Min(possibleMoves.Count, 4);
        dataChecklist.AddStep();
        Checklist loadedMoves = new(moveAmount);
        int moveId = Random.Range(0, possibleMoves.Count);
        newMoves[loadedMoves.currentSteps] = possibleMoves[moveId];
        possibleMoves.RemoveAt(moveId);
        PokeAPI.GetMove(newMoves[loadedMoves.currentSteps].move.url, LoadMove);

        void LoadMove(MoveData data)
        {
            moves[loadedMoves.currentSteps] = new MoveModel(data);
            loadedMoves.FinishStep();
            Logger.Log($"{name}[{loadedMoves.currentSteps}/{loadedMoves.requiredSteps}] => {data.name}", LogFlags.PokemonBuild);

            if (loadedMoves.isDone)
            {
                Logger.Log($"{name} Moves done loading", LogFlags.PokemonBuild);
                dataChecklist.FinishStep();
                return;
            }
            moveId = Random.Range(0, possibleMoves.Count);
            newMoves[loadedMoves.currentSteps] = possibleMoves[moveId];
            possibleMoves.RemoveAt(moveId);
            PokeAPI.GetMove(newMoves[loadedMoves.currentSteps].move.url, LoadMove);
        }
    }
    private void GetGender()
    {
        dataChecklist.AddStep();

        WebConnection.GetRequest<Species>(data.species.url, (data) =>
        {
            if (data.genderRate < 0)
            {
                gender = Gender.NonBinary;
            }
            else
            {
                float randomGender = Random.Range(0, 8);
                gender = randomGender < data.genderRate ? Gender.Female : Gender.Male;
            }

            Logger.Log($"{name} Gender done loading: {gender}", LogFlags.PokemonBuild);
            dataChecklist.FinishStep();
        });
    }
    private void GetAbility()
    {
        int abilityId = Random.Range(0, data.abilities.Count);
        PokeAPI.GetAbility(data.abilities[abilityId].reference.url, (abilityData) =>
        {
            ability = abilityData;
            ability.abilityName = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(ability.name.Replace("-", " "));
            //get flavorText
            for (int i = 0; i < ability.flavorTexts.Count; i++)
            {
                FlavorText flavorText = ability.flavorTexts[i];
                if(flavorText.language.name == "en")
                {
                    if(string.IsNullOrEmpty(ability.flavorText) || flavorText.text.Length < ability.flavorText.Length)
                        ability.flavorText = flavorText.text;
                }
            }

            ability.flavorText = ability.flavorText.Replace("\n", " ");
        });
    }
    private void GetNature()
    {

    }
    #endregion
}

public struct Stats
{
    public int hp { get => stats[0]; set => stats[0] = value; }
    public int atk { get => stats[1]; set => stats[1] = value; }
    public int def { get => stats[2]; set => stats[2] = value; }
    public int sAtk { get => stats[3]; set => stats[3] = value; }
    public int sDef { get => stats[4]; set => stats[4] = value; }
    public int spd { get => stats[5]; set => stats[5] = value; }
    public int acc { get => stats[6]; set => stats[6] = value; }
    public int eva { get => stats[7]; set => stats[7] = value; }

    private int[] stats;

    public int this[StatType type]
    { 
        get => stats[(int)type];
        set => stats[(int)type] = value;
    }

    public Stats(int Hp, int Atk, int Def,
        int SAtk, int SDef, int Spd,
        int Acc = 100, int Eva = 100)
    {
        stats = new int[8];
        stats[0] = Hp;
        stats[1] = Atk;
        stats[2] = Def;
        stats[3] = SAtk;
        stats[4] = SDef;
        stats[5] = Spd;
        stats[6] = Acc;
        stats[7] = Eva;
    }
}

public enum Gender
{
    NonBinary = -1,
    Male = 1,
    Female = 2
}