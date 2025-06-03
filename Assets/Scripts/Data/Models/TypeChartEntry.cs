using System.Collections.Generic;

public class TypeChartEntry
{
    public int id;
    public string name;
    public TypeRelations referenceData;

    public readonly Dictionary<TypeChartEntry, float> attackMultiplier;

    public TypeChartEntry(PokemonType type)
    {
        id = type.id;
        name = type.name;
        referenceData = type.relations;

        int multipliersCount = referenceData.notEffective?.Count ?? 0;
        multipliersCount += referenceData.superEffective?.Count ?? 0;
        multipliersCount += referenceData.doNotAffect?.Count ?? 0;
        attackMultiplier = new(multipliersCount);
    }

    public float GetMultiplier(TypeChartEntry type) => attackMultiplier.GetValueOrDefault(type, 1);
}