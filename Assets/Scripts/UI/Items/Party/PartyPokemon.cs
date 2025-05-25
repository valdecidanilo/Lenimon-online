using AddressableAsyncInstances;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class PartyPokemon : MonoBehaviour
{
    [SerializeField] private PokemonSelectionItem selectionItem;

    [Header("Data Reference")] 
    [SerializeField] private TMP_Text nickname;
    [SerializeField] private TMP_Text level;
    [SerializeField] private TMP_Text hp;
    [SerializeField] private Image hpBar;
    [SerializeField] private Image icon;
    [SerializeField] private Image gender;
    [SerializeField] private GameObject itemIcon;
    //Modes
    [Header("Modes"),SerializeField] private GameObject defaultMode;
    [SerializeField] private GameObject learnMode;
    [SerializeField] private TMP_Text canLearnText;

    public Pokemon pokemon { get; private set; }

    public void SetupPokemon(Pokemon newPokemon)
    {
        gameObject.SetActive(newPokemon != null);

        if (newPokemon == null) return;

        defaultMode.SetActive(true);
        learnMode.SetActive(false);
        if(pokemon != null) pokemon.onHpChanged.RemoveCallback(OnPokemonHpChanged);
        pokemon = newPokemon;
        pokemon.onHpChanged.RegisterCallback(OnPokemonHpChanged);

        nickname.text = pokemon.name;
        level.text = $"Lv{pokemon.level}";
        BattleVFX.ChangeHpBar(pokemon, hpBar, pokemon.battleStats[StatType.hp], hp);
        icon.sprite = pokemon.icon;
        PokeDatabase.SetGenderSprite(gender, pokemon.gender);
        itemIcon.SetActive(newPokemon.heldItem != null);
        selectionItem.currentState = pokemon.battleStats[StatType.hp] == 0 ?
            PokemonSelectionItem.PokemonState.Faint :
            PokemonSelectionItem.PokemonState.Normal;
    }

    public void LearnMoveMode(MoveData move)
    {
        defaultMode.SetActive(false);
        learnMode.SetActive(true);
    }

    private IEnumerator OnPokemonHpChanged(int initialValue, int currentValue)
    {
        yield return BattleVFX.LerpHpBar(pokemon, initialValue, currentValue, hpBar, hp);
    }
}
