using System.Collections;
using Battle;
using System.Collections.Generic;

public static class ItemEffect
{
    public static Effect GenerateItemEffect(ItemModel item)
    {
        switch (item.name)
        {
            case "Potion":
                return new HealEffect(20);
            case "Super Potion":
            case "Fresh Water":
                return new HealEffect(50);
            case "Soda Pop":
                return new HealEffect(60);
            case "Lemonade":
                return new HealEffect(80);
            case "Moomoo Milk":
                return new HealEffect(100);
            case "Hyper Potion":
                return new HealEffect(200);
            case "Max Potion":
                return new HealEffect(100, HealEffect.HealType.Hp);

            case "X Attack":
                item.activePokemonOnly = true;
                return new StatChangeEffect(new List<StatChange>{ new StatChange()
                {
                    change = 2,
                    stat = StatToApi(StatType.atk)
                }});
            case "X Defense":
                item.activePokemonOnly = true;
                return new StatChangeEffect(new List<StatChange>{ new StatChange()
                {
                    change = 2,
                    stat = StatToApi(StatType.def)
                }});
            case "X Sp Atk":
                item.activePokemonOnly = true;
                return new StatChangeEffect(new List<StatChange>{ new StatChange()
                {
                    change = 2,
                    stat = StatToApi(StatType.sAtk)
                }});
            case "X Sp Def":
                item.activePokemonOnly = true;
                return new StatChangeEffect(new List<StatChange>{ new StatChange()
                {
                    change = 2,
                    stat = StatToApi(StatType.sDef)
                }});
            case "X Speed":
                item.activePokemonOnly = true;
                return new StatChangeEffect(new List<StatChange>{ new StatChange()
                {
                    change = 2,
                    stat = StatToApi(StatType.spd)
                }});
            case "X Accuracy":
                item.activePokemonOnly = true;
                return new StatChangeEffect(new List<StatChange>{ new StatChange()
                {
                    change = 2,
                    stat = StatToApi(StatType.acc)
                }});
        }

        return null;
    }

    public static IEnumerator ItemMessage(BattleEvent evt)
    {
        yield return Announcer.Announce($"{evt.user} used a {evt.move.name}.", holdTime: .6f);
    }

    private static ApiReference StatToApi(StatType type)
    {
        string apiName = type switch
        {
            StatType.atk => "attack",
            StatType.def => "defense",
            StatType.sAtk => "special-attack",
            StatType.sDef => "special-defense",
            StatType.spd => "speed",
            StatType.acc => "accuracy",
            StatType.eva => "evasion",
        };
        return new () { name = apiName };
    }

    private static MoveModel CreateMockMove(ItemModel item)
    {
        MoveData data = new();
        data.id = -1;
        data.name = item.name;
        data.pp = 1;
        data.priority = 99;
        data.type = new() { name = "unknown" };
        data.target = new() { name = "user" };
        data.moveTypeData = new() { id = MoveType.Status };
        data.flavorTexts = new() {
            new FlavorText() {
                text = item.effect,
                language = new(){name = "en"}
            }
        };
        data.meta = new() {
            category = new() { name = "unique" }
        };
        
        MoveModel model = new(data);
        model.effect = item.battleEffect;

        return model;
    }
}