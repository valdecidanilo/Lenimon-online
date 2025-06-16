using System;
using UnityEngine;
using Random = UnityEngine.Random;
using LenixSO.Logger;
using Logger = LenixSO.Logger.Logger;
using System.Globalization;
using System.Collections;

public class Pokemon : ApiData
{
    public PokemonData data { get; private set; }
    public Gender gender { get; private set; }

    public bool fainted => battleStats[StatType.hp] <= 0;

    #region Stats
    //stats
    public int level { get; private set; }
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
    public MoveModel[] moves;
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
        for (int i = 0; i < types.Length; i++)
            types[i] = PokeDatabase.GetType(data.types[i].type.name);

        stats = new(
            pokemonData.hpStat,
            pokemonData.atkStat,
            pokemonData.defStat,
            pokemonData.sAtkStat,
            pokemonData.sDefStat,
            pokemonData.spdStat
        );

        iv = new(
            Random.Range(0, 32),
            Random.Range(0, 32),
            Random.Range(0, 32),
            Random.Range(0, 32),
            Random.Range(0, 32),
            Random.Range(0, 32)
        );
        ev = new(0, 0, 0, 0, 0, 0, 0, 0);
        nature = PokeDatabase.GetRandomNature(ref natureName);

        moves = new MoveModel[4];
        LevelUp(Mathf.Max(lv, 1));//minimum level is 1

        battleStats = new(stats.hp, 0, 0, 0, 0, 0, 0, 0);

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
        stats.hp = data.name != "shedinja" ? BasicStatCalculation(data.hpStat, iv[StatType.hp], ev[StatType.hp], level) + level + 10 : 1;
        stats.atk = NonHpCalculation(StatType.atk);
        stats.def = NonHpCalculation(StatType.def);
        stats.sAtk = NonHpCalculation(StatType.sAtk);
        stats.sDef = NonHpCalculation(StatType.sDef);
        stats.spd = NonHpCalculation(StatType.spd);

        int BasicStatCalculation(int baseStat, int iv, int ev, int level)
        {
            return Mathf.Max(0, (((2 * baseStat) + iv + (ev / 4)) * level) / 100);
        }

        int NonHpCalculation(StatType type)
        {
            return Mathf.FloorToInt((BasicStatCalculation(data.stats[(int)type].base_stat, iv[type], ev[type], level) + 5) * (nature[type] / 100f));
        }
    }

    public IEnumerator DamagePokemon(int value)
    {
        int currentHp = battleStats.hp;
        battleStats.hp = Mathf.Max(currentHp - value, 0);
        int newHp = battleStats.hp;
        if(currentHp == newHp) yield break;
        yield return onHpChanged?.Invoke(currentHp, newHp);
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
    private void GetGender()
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
            dataChecklist.FinishStep();
        });
    }
    private void GetAbility()
    {
        dataChecklist.AddStep();
        int abilityId = Random.Range(0, data.abilities.Count);
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