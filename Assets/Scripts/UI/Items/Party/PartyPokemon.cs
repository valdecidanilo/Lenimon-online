using System;
using System.Collections;
using TMPro;
using UnityEngine;
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

    private const string canLearn = "ABLE";
    private const string cantLearn = "UNABLE";
    private const string learned = "LEARNED";

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

    public bool LearnMoveMode(MoveData move)
    {
        if(pokemon == null) return false;
        defaultMode.SetActive(false);
        learnMode.SetActive(true);

        bool hasMove = false;
        for (int i = 0; i < pokemon.moves.Length; i++)
        {
            MoveModel pkmMove = pokemon.moves[i];
            if (pkmMove == null || pkmMove.Data.name != move.name) continue;

            hasMove = true;
            canLearnText.text = learned;
        }
        bool learnableMove = false;
        if (!hasMove)
        {
            for (int i = 0; i < pokemon.data.moves.Count; i++)
            {
                MoveReference moveRef = pokemon.data.moves[i];
                if (!moveRef.move.name.Equals(move.name, StringComparison.CurrentCultureIgnoreCase)) continue;
                bool tmMove = false;
                for (int j = 0; j < moveRef.learningDetails.Count; j++)
                {
                    var learnDetails = moveRef.learningDetails[j];
                    if (learnDetails.learnMethod != MoveLearnMethod.TM) continue;
                    tmMove = true;
                    break;
                }

                if (!tmMove) continue;
                learnableMove = true;
                break;
            }

            canLearnText.text = learnableMove ? canLearn : cantLearn;
        }

        selectionItem.ChangeState(learnableMove ? PokemonSelectionItem.PokemonState.Swap : selectionItem.currentState);
        return learnableMove;
    }

    private IEnumerator OnPokemonHpChanged(int initialValue, int currentValue)
    {
        yield return BattleVFX.LerpHpBar(pokemon, initialValue, currentValue, hpBar, hp);
    }
}
