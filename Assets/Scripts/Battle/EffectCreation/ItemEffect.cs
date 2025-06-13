using System.Collections;
using Battle;
using System.Collections.Generic;

public static class ItemEffect
{
    public static Effect GenerateItemEffect(ItemModel item)
    {
        item.checkItemUse = FreeUseItem;
        switch (item.name)
        {
            case "Potion":
                item.checkItemUse = HealItemCheck;
                return new HealEffect(20);
            case "Revive":
                item.checkItemUse = RevivalItemCheck;
                return new HealEffect(50, HealEffect.HealType.Hp);
            case "Super Potion":
                item.checkItemUse = HealItemCheck;
                return new HealEffect(50);
            case "Soda Pop":
                item.checkItemUse = HealItemCheck;
                return new HealEffect(60);
            case "Lemonade":
                item.checkItemUse = HealItemCheck;
                return new HealEffect(80);
            case "Moomoo Milk":
                item.checkItemUse = HealItemCheck;
                return new HealEffect(100);
            case "Hyper Potion":
                item.checkItemUse = HealItemCheck;
                return new HealEffect(200);
            case "Max Potion":
                item.checkItemUse = HealItemCheck;
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

    private static bool FreeUseItem(Pokemon pokemon, out string failMessage)
    {
        failMessage = string.Empty;
        return true;
    }
    
    private static bool HealItemCheck(Pokemon pokemon, out string failMessage)
    {
        failMessage = string.Empty;
        int hp = pokemon.battleStats.hp;
        if (hp <= 0) failMessage = "You can't heal a fainted pokemon!!";
        else if(hp >= pokemon.stats.hp) failMessage = $"{pokemon.name} is already at full HP!!";
        return string.IsNullOrEmpty(failMessage);
    }
    
    private static bool RevivalItemCheck(Pokemon pokemon, out string failMessage)
    {
        failMessage = string.Empty;
        int hp = pokemon.battleStats.hp;
        if (hp > 0) failMessage = $"{pokemon.name} has not fainted!!";
        return string.IsNullOrEmpty(failMessage);
    }

    public static IEnumerator ItemMessage(BattleEvent evt)
    {
        yield return Announcer.AnnounceCoroutine($"{evt.user.name} used a {evt.move.name}.", holdTime: .6f);
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

    public static MoveModel CreateMockMove(ItemModel item)
    {
        MoveData data = new();
        data.id = -1;
        data.name = item.name;
        data.pp = 1;
        data.priority = 99;
        data.type = new() { name = "unknown" };
        data.target = new() { name = "user" };
        data.moveTypeData = new() { id = MoveType.Item };
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
        model.effectMessage = new(ItemMessage);

        return model;
    }
}