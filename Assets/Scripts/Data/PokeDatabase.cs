using AddressableAsyncInstances;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using LenixSO.Logger;
using Logger = LenixSO.Logger.Logger;

public static class PokeDatabase
{
    //api database
    #region Database
    public static Dictionary<string, MoveData> moves = new();
    public static Dictionary<string, MoveTypeData> moveTypes = new();
    public static Dictionary<string, AbilityData> abilities = new();
    public static Dictionary<string, ItemData> items = new();
    public static Dictionary<string, Stats> natures = new();
    public static Dictionary<MoveData, TMData> TMs = new();
    public static MoveData StruggleMoveData => moves[PokeAPI.struggleMoveRoute];
    #endregion

    //resources database
    #region Addressable Keys
    private const string genericIconKey = "Sprite/Icons[pokeball_0]";
    private const string genericTMKey = "Item_Icons[TM_Generic]";
    private const string maleIconKey = "maleIcon";
    private const string femaleIconKey = "femaleIcon";
    private const string pokeBallKey = "pokeball";
    private const string statChangeKey = "Stat_Change";
    private const string hpBarKey = "Hp_Bar";
    public static readonly string[] types = 
    {
        "normal",
        "fighting",
        "flying",
        "poison",
        "ground",
        "rock",
        "bug",
        "ghost",
        "steel",
        "fire",
        "water",
        "grass",
        "electric",
        "psychic",
        "ice",
        "dragon",
        "dark",
        "fairy",
        "stellar",
        "unknown",
    };
    private static readonly List<string> natureNames = new();
    #endregion

    #region Resources
    public static Sprite emptySprite;
    public static Sprite genericIcon;
    public static Sprite genericTM;
    public static Sprite maleIcon;
    public static Sprite femaleIcon;
    public static Sprite statChangeSprite;
    public static Sprite pokeBallSprite;
    public static Sprite[] hpBars;
    public static Dictionary<string, Sprite> typeSprites = new(types.Length);
    public static Dictionary<string, TypeChartEntry> typeChart = new(types.Length);

    public static void PreloadAssets()
    {
        Checklist preloadedAssets = new(1);
        LoadingScreen.AddOrChangeQueue(preloadedAssets, "Loading assets...");
        //setup invert type chart
        preloadedAssets.onCompleted += () => { foreach (var type in typeChart.Values) type.InvertChart(OptionsMenu.invertChart); };
        if (emptySprite != null)//already loaded data
        {
            preloadedAssets.FinishStep();
            return;
        }

        #region LocalSprites
        //gender sprites
        AAAsset<Sprite>.LoadAsset(maleIconKey, (sprite) =>
        {
            maleIcon = sprite;
            preloadedAssets.FinishStep();
        });
        preloadedAssets.AddStep();
        AAAsset<Sprite>.LoadAsset(femaleIconKey, (sprite) =>
        {
            femaleIcon = sprite;
            preloadedAssets.FinishStep();
        });

        //type sprites
        for (int i = 0; i < types.Length; i++)
        {
            string typeKey = types[i];
            preloadedAssets.AddStep();
            AAAsset<Sprite>.LoadAsset($"type[{typeKey}]", (sprite) =>
            {
                typeSprites[typeKey] = sprite;
                preloadedAssets.FinishStep();
            });
        }

        //Generic sprites
        preloadedAssets.AddStep();
        AAAsset<Sprite>.LoadAsset("Empty", (sprite) =>
        {
            emptySprite = sprite;
            preloadedAssets.FinishStep();
        });
        preloadedAssets.AddStep();
        AAAsset<Sprite>.LoadAsset(genericIconKey, (sprite) =>
        {
            genericIcon = sprite;
            preloadedAssets.FinishStep();
        });
        preloadedAssets.AddStep();
        AAAsset<Sprite>.LoadAsset(genericTMKey, (sprite) =>
        {
            genericTM = sprite;
            preloadedAssets.FinishStep();
        });
        preloadedAssets.AddStep();
        AAAsset<Sprite>.LoadAsset(statChangeKey, (sprite) =>
        {
            statChangeSprite = sprite;
            preloadedAssets.FinishStep();
        });
        preloadedAssets.AddStep();
        AAAsset<Sprite>.LoadAsset(pokeBallKey, (sprite) =>
        {
            pokeBallSprite = sprite;
            preloadedAssets.FinishStep();
        });

        //hp bar
        hpBars = new Sprite[3];
        preloadedAssets.AddStep(3);
        for (int i = 0; i < hpBars.Length; i++)
        {
            int id = i;
            AAAsset<Sprite>.LoadAsset($"{hpBarKey}[{id}]", sprite =>
            {
                hpBars[id] = sprite;
                preloadedAssets.FinishStep();
            });
        }
        #endregion

        #region ApiData
        //Natures
        preloadedAssets.AddStep();
        WebConnection.GetRequest<ApiRequestList>("https://pokeapi.co/api/v2/nature", (data) =>
        {
            Logger.Log($"Pokemon Natures", LogFlags.DataCheck);
            for (int i = 0; i < data.results.Count; i++)
            {
                string name = data.results[i].name;
                natureNames.Add(name);
                Stats nature = new(100, 100, 100, 100, 100, 100, 100, 100);
                int decreaseId = (i / 5) + 1;
                int increaseId = (i % 5) + 1;
                nature[(StatType)increaseId] += 10;
                nature[(StatType)decreaseId] -= 10;
                Logger.Log($"{name} => +{(StatType)increaseId}; -{(StatType)decreaseId}", LogFlags.DataCheck);
                natures[name] = nature;
            }
            preloadedAssets.FinishStep();
        });

        //TypeChart
        preloadedAssets.AddStep();
        WebConnection.GetRequest<ApiRequestList>("https://pokeapi.co/api/v2/type", (data) =>
        {
            Checklist typeList = new(data.results.Count);
            typeList.onCompleted += () =>
            {
                Logger.Log($"Pokemon Types: {data.results.Count}", LogFlags.DataCheck);
                
                for (int i = 0; i < types.Length; i++)
                {
                    int inGameId = i;
                    string typeKey = types[inGameId];
                    var typeData = typeChart[typeKey];
                    TypeRelations relations = typeData.referenceData;
                    var chartEntry = typeChart[typeKey];
                    bool invert = OptionsMenu.invertChart;
                    ApplyModifier(2, chartEntry.attackMultiplier, relations.superEffective);
                    ApplyModifier(.5f, chartEntry.attackMultiplier, relations.notEffective);
                    ApplyModifier(0, chartEntry.attackMultiplier, relations.doNotAffect);

                    void ApplyModifier(float modifier, Dictionary<TypeChartEntry, float> dictionary, 
                        List<ApiReference> reference)
                    {
                        for (int id = 0; id < reference.Count; id++)
                        {
                            string otherKey = reference[id].name;
                            if (!typeChart.TryGetValue(otherKey, out var otherEntry)) continue;
                            dictionary[otherEntry] = modifier;
                        }
                    }
                    
                    //raw log
                    StringBuilder sb = new($"type: {typeData.name} => {typeData.attackMultiplier.Count} attacking interactions");
                    sb.Append($"\n{relations.superEffective.Count} super effective");
                    sb.Append($"\n{relations.notEffective.Count} not effective");
                    sb.Append($"\n{relations.weakTo.Count} weak to");
                    sb.Append($"\n{relations.resistantTo.Count} resistant to");
                    sb.Append($"\n{relations.immuneTo.Count} immune to");
                    sb.Append($"\n{relations.doNotAffect.Count} don't affect");
                    Logger.Log(sb.ToString(), LogFlags.DataCheck);
                }
                
                preloadedAssets.FinishStep();
            };
            
            for (int i = 0; i < data.results.Count; i++)
            {
                int inGameId = i;
                WebConnection.GetRequest<PokemonType>(data.results[i].url, (typeData) =>
                {
                    string typeKey = types[inGameId];
                    TypeChartEntry chartEntry = new(typeData);
                    typeChart[typeKey] = chartEntry;
                    typeList.FinishStep();
                });
            }
        });

        //Struggle move
        preloadedAssets.AddStep();
        PokeAPI.GetMoveData(PokeAPI.struggleMoveRoute, (data) => preloadedAssets.FinishStep());

        #endregion
    }
    #endregion

    //Utility methods
    public static void SetGenderSprite(Image image, Gender gender)
    {
        image.gameObject.SetActive(gender != Gender.NonBinary);
        if (gender == Gender.NonBinary) return;
        image.sprite = gender switch
        {
            Gender.Male => maleIcon,
            Gender.Female => femaleIcon
        };
    }

    public static Stats GetRandomNature(ref string name)
    {
        name = natureNames[Random.Range(0, natureNames.Count)];
        return natures[name];
    }
    public static Stats GetNature(string name)
    {
        return natures[name];
    }

    public static TypeChartEntry GetType(string type) => typeChart.GetValueOrDefault(type, typeChart[types[^1]]);

    /// <summary>
    /// Method don't work on Hp, Acc, or Evasion
    /// </summary>
    public static int CalculateModifiedStat(int statValue, int stage)
    {
        int upper = 2 + Mathf.Max(stage, 0);
        float lower = 2 - Mathf.Min(stage, 0);
        return Mathf.FloorToInt(statValue * (upper / lower));
    }
}
