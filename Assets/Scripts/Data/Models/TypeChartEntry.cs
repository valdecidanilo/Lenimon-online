using System.Collections.Generic;

public class TypeChartEntry
{
    public int id;
    public string name;
    public TypeRelations referenceData;

    public Dictionary<TypeChartEntry, float> attackMultiplier { get; private set; }

    private bool inverted = false;

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

    public void InvertChart(bool invert)
    {
        if(invert == inverted) return;
        Dictionary<TypeChartEntry, float> newMultipliers = new();
        foreach (var entry in attackMultiplier)
        {
            if (invert)
                newMultipliers[entry.Key] = entry.Value <= 0 ? 4 : 1f / entry.Value;
            else
                newMultipliers[entry.Key] = entry.Value > 2 ? 0 : 1f / entry.Value;
        }
        inverted = invert;
        attackMultiplier = newMultipliers;
    }
}