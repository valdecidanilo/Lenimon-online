using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PartyPokemon : MonoBehaviour
{
    [SerializeField] private PokemonSelectionItem selectionItem;
    [SerializeField] private Sprite maleIcon;
    [SerializeField] private Sprite femaleIcon;

    [FormerlySerializedAs("pokemonName")]
    [Header("Data Reference")] 
    [SerializeField] private TMP_Text nickname;
    [SerializeField] private TMP_Text level;
    [SerializeField] private TMP_Text hp;
    [SerializeField] private Image hpBar;
    [SerializeField] private Image icon;
    [SerializeField] private Image gender;

    public Pokemon pokemon { get; private set; }
    
    private void OnEnable()
    {
        gameObject.SetActive(pokemon != null);
        if(pokemon == null) return;
        selectionItem.currentState = pokemon.battleStats[StatType.hp] == 0 ? 
            PokemonSelectionItem.PokemonState.Faint :
            PokemonSelectionItem.PokemonState.Normal;
    }

    public void SetupPokemon(Pokemon newPokemon)
    {
        pokemon = newPokemon;

        nickname.text = pokemon.name;
        level.text = pokemon.level.ToString();
        hp.text = $"{pokemon.battleStats[StatType.hp]}/{pokemon.stats[StatType.hp]}";
        hpBar.fillAmount = (float)pokemon.battleStats[StatType.hp] / pokemon.stats[StatType.hp];
        icon.sprite = pokemon.icon;
        gender.gameObject.SetActive(newPokemon.gender != Gender.NonBinary);
        switch (newPokemon.gender)
        {
            case Gender.Male:
                gender.sprite = maleIcon;
                break;
            case Gender.Female:
                gender.sprite = femaleIcon;
                break;
        }
    }
}
