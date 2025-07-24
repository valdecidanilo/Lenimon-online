using System;
using UnityEngine;
using Random = UnityEngine.Random;
using LenixSO.Logger;
using Logger = LenixSO.Logger.Logger;
using System.Globalization;
using System.Collections;
using Data.Models;
using DB.Data;
using Newtonsoft.Json;
using Utils;

public class Pokemon : ApiData
{
    public PokemonData data { get; private set; }
    public Gender gender { get; private set; }
    public GrowthRate growthRate { get; private set; }
    public bool fainted => battleStats[StatType.hp] <= 0;

    #region Stats
    //stats
    public int level { get; private set; }
    public int experience { get; private set; }
    public int experienceMax { get; private set; }
    public Stats stats { get; private set; }
    public Stats iv{ get; private set; }
    public Stats ev{ get; private set; }
    public Stats nature{ get; private set; }
    public Stats battleStats{ get; set; }
    #endregion
    
    //MetaData
    public Sprite frontSprite { get; private set; }
    public Sprite backSprite { get; private set; }
    public Sprite icon { get; private set; }
    public AbilityData ability;
    public MoveModel[] moves = new MoveModel[4];
    public ItemModel heldItem;
    public string natureName;
    public TypeChartEntry[] types;

    public CoroutineAction<int, int> onHpChanged = new(null);

    //Data loading
    private Checklist dataChecklist;
    public event Action onDoneLoading;

    public Pokemon(PokemonData pokemonData, int lv = 1)
    {
        data = pokemonData;
        name = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(pokemonData.species.name);
        name = name.Replace('-', ' ');
        id = pokemonData.id;
        types = new TypeChartEntry[data.types.Count];
        for (var i = 0; i < types.Length; i++)
            types[i] = PokeDatabase.GetType(data.types[i].type.name);

        stats = new Stats(
            pokemonData.hpStat,
            pokemonData.atkStat,
            pokemonData.defStat,
            pokemonData.sAtkStat,
            pokemonData.sDefStat,
            pokemonData.spdStat
        );

        iv = new Stats(
            Random.Range(0, 32),
            Random.Range(0, 32),
            Random.Range(0, 32),
            Random.Range(0, 32),
            Random.Range(0, 32),
            Random.Range(0, 32)
        );
        ev = new Stats(0, 0, 0, 0, 0, 0, 0, 0);
        nature = PokeDatabase.GetRandomNature(ref natureName);

        moves = new MoveModel[4];
        LevelUp(Mathf.Max(lv, 1));//minimum level is 1

        battleStats = new Stats(stats.hp, 0, 0, 0, 0, 0, 0, 0);

        dataChecklist = new Checklist(0);
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
    
    public static void GetLoadedPokemon(PokemonModel pokemonData, Action<Pokemon> onPokemonLoaded = null)
    {
        PokeAPI.GetPokemonData(pokemonData.PokeApiId, (data) =>
        {
            Pokemon pokemon = new(data);
            pokemon.LoadSprites();
            pokemon.GetSpecieData();
            pokemon.GetAbility();
            pokemon.GetHeldItem();
            
            pokemon.onDoneLoading += PokemonLoaded;
            void PokemonLoaded()
            {
                pokemon.name = pokemonData.Name;
                pokemon.nature = PokeDatabase.GetNature(pokemonData.NatureName);
                pokemon.gender = pokemonData.Gender;
                pokemon.battleStats.hp = pokemonData.CurrentHp;
                pokemon.level = pokemonData.Level;
                pokemon.experience = pokemonData.Experience;
                
                pokemon.iv = JsonConvert.DeserializeObject<Stats>(pokemonData.IvJson);
                pokemon.ev = JsonConvert.DeserializeObject<Stats>(pokemonData.EvJson);
                
                var moveList = JsonConvert.DeserializeObject<MoveListWrapper>(pokemonData.MovesJson);
                Checklist movesLoader = new(pokemon.moves.Length);
                for (var i = 0; i < pokemon.moves.Length; i++)
                {
                    var id = i;
                    if (id >= moveList.id.Length || moveList.id[id] == 0)
                    {
                        movesLoader.FinishStep();
                        continue;
                    }
                    PokeAPI.GetMoveData(moveList.id[id], (move) =>
                    {
                        pokemon.moves[id] = new MoveModel(move)
                        {
                            pp = moveList.currentPP[id]
                        };
                        movesLoader.FinishStep();
                    });
                }
                
                movesLoader.onCompleted += () =>
                {
                    pokemon.onDoneLoading -= PokemonLoaded;
                    onPokemonLoaded?.Invoke(pokemon);
                };
            }
        });
    }

    public void LoadRequiredData()
    {
        LoadSprites();
        GetSpecieData();
        GetAbility();
        GetRandomMoves();
        GetHeldItem();
    }

    public void LevelUp(int amount)
    {
        if (amount <= 0) return;

        level += amount;
        UpdateStats();
    }

    private void UpdateStats()
    {
        stats.hp = data.name != "shedinja" ? PokeMath.BasicStatCalculation(data.hpStat, iv[StatType.hp], ev[StatType.hp], level) + level + 10 : 1;
        stats.atk = PokeMath.NonHpCalculation(this, StatType.atk);
        stats.def = PokeMath.NonHpCalculation(this, StatType.def);
        stats.sAtk = PokeMath.NonHpCalculation(this, StatType.sAtk);
        stats.sDef = PokeMath.NonHpCalculation(this, StatType.sDef);
        stats.spd = PokeMath.NonHpCalculation(this, StatType.spd);
    }

    public void ResetBattleStats()
    {
        battleStats[StatType.atk] = 0;
        battleStats[StatType.def] = 0;
        battleStats[StatType.sAtk] = 0;
        battleStats[StatType.sDef] = 0;
        battleStats[StatType.spd] = 0;
        battleStats[StatType.acc] = 0;
        battleStats[StatType.eva] = 0;
    }

    public IEnumerator DamagePokemon(int value)
    {
        int currentHp = battleStats.hp;
        battleStats.hp = Mathf.Max(currentHp - value, 0);
        int newHp = battleStats.hp;
        if(currentHp == newHp) yield break;
        yield return onHpChanged?.Invoke(currentHp, newHp);
        if (newHp > 0) yield break;
        yield return new WaitForSeconds(.5f);
        yield return Announcer.AnnounceCoroutine($"{name} fainted!", holdTime: 1.2f);
    }
    public IEnumerator HealPokemon(int value)
    {
        int currentHp = battleStats.hp;
        battleStats.hp = Mathf.Min(currentHp + value, stats.hp);
        int newHp = battleStats.hp;
        if (currentHp == newHp) yield break;
        yield return onHpChanged?.Invoke(currentHp, newHp);
    }

    #region Data Load
    private void LoadSprites()
    {
        dataChecklist.AddStep(3);
        PokeAPI.GetPokemonSprite(data, (sprite) =>
        {
            frontSprite = sprite;
            Logger.Log($"{name} Front sprite done loading", LogFlags.PokemonBuild);
            dataChecklist.FinishStep();

            PokeAPI.GetIcon(data, (sprite) =>
            {
                icon = sprite ?? frontSprite;
                Logger.Log($"{name} Icon done loading", LogFlags.PokemonBuild);
                dataChecklist.FinishStep();
            });
        });
        PokeAPI.GetPokemonSprite(data, (sprite) =>
        {
            backSprite = sprite;
            Logger.Log($"{name} Back sprite done loading", LogFlags.PokemonBuild);
            dataChecklist.FinishStep();
        }, true);
    }
    private void GetRandomMoves()
    {
        dataChecklist.AddStep();
        MoveHelper.GetRandomMoves(this, new[] { MoveLearnMethod.LevelUp }, () => dataChecklist.FinishStep());
    }
    private void GetSpecieData()
    {
        dataChecklist.AddStep();
        WebConnection.GetRequest<Species>(data.species.url, (data) =>
        {
            if (data.genderRate < 0)
                gender = Gender.NonBinary;
            else
            {
                float randomGender = Random.Range(0, 8);
                gender = randomGender < data.genderRate ? Gender.Female : Gender.Male;
            }
            Logger.Log($"{name} Gender done loading: {gender}", LogFlags.PokemonBuild);
            WebConnection.GetRequest<GrowthRate>(data.growthRate.url, (gRate) =>
            {
                growthRate = gRate;
                Logger.Log($"{name} Growth rate done loading: {growthRate.growthRate}", LogFlags.PokemonBuild);
                dataChecklist.FinishStep();
            });
        });
    }
    private void GetAbility()
    {
        dataChecklist.AddStep();
        var abilityId = Random.Range(0, data.abilities.Count);
        PokeAPI.GetAbility(data.abilities[abilityId].reference.url, (abilityData) =>
        {
            ability = abilityData;
            ability.abilityName = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(ability.name.Replace("-", " "));
            ability.flavorText = PokeAPI.SmallestFlavorText(ability.flavorTexts).Replace("\n", " ");
            dataChecklist.FinishStep();
        });
    }
    private void GetHeldItem()
    {
        int rand = Random.Range(1, 100);
        int itemCheck = 0;
        string itemRoute = string.Empty;

        for (int i = 0; i < data.held_items.Count; i++)
        {
            HeldItem item = data.held_items[i];
            int rarity = item.version_details[0].rarity;
            itemCheck += rarity;
            if (itemCheck <= rand) continue;
            itemRoute = item.item.url;
            Logger.Log($"{name} has a item:\n{item.item.name}[{i}] ({rarity} rarity) => {itemCheck} | {rand}", LogFlags.DataCheck);
            break;
        }
        if (!string.IsNullOrEmpty(itemRoute))
        {
            dataChecklist.AddStep();
            PokeAPI.GetItem(itemRoute, (item) =>
            {
                heldItem = item;
                dataChecklist.FinishStep();
            });
        }
    }
    #endregion
}

public class Stats
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

    public static Stats Copy(Stats target)
    {
        return new(target.hp, target.atk, target.def, target.sAtk,
            target.sDef, target.spd, target.acc, target.eva);
    }
}

public enum Gender
{
    NonBinary = -1,
    Male = 1,
    Female = 2
}