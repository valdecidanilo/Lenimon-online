using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BattleSetup : MonoBehaviour
{
    //Enemy
    private Pokemon enemyPokemon;
    [Header("Enemy")]
    [SerializeField] private Image enemyImage;
    [SerializeField] private TMP_Text enemyName;
    [SerializeField] private TMP_Text enemyLevel;
    [SerializeField] private Image enemyHp;

    //Ally
    private Pokemon allyPokemon;
    [Header("Ally")]
    [SerializeField] private Image pokemonImage;
    [SerializeField] private TMP_Text pokemonName;
    [SerializeField] private TMP_Text level;
    [SerializeField] private Image hp;
    [SerializeField] private TMP_Text hpValue;
    [SerializeField] private Image xp;

    [Header("Battle Menu")]
    [SerializeField] private GameObject battleMenu;

    [Header("Moves Menu")]
    [SerializeField] private FightMenu fightMenu;

    public void SetupBattle(Pokemon ally, Pokemon enemy)
    {
        //setup enemy
        enemyPokemon = enemy;
        enemyImage.sprite = enemy.frontSprite;
        enemyName.text = enemy.name;
        enemyLevel.text = $"Lv{enemy.level}";
        enemyHp.fillAmount = 1;

        //setup ally
        allyPokemon = ally;
        pokemonImage.sprite = ally.backSprite;
        pokemonName.text = ally.name;
        level.text = $"Lv{ally.level}";
        hpValue.text = $"{allyPokemon.hp}/{allyPokemon.hp}";
        hp.fillAmount = 1;
        xp.fillAmount = Random.Range(0, 1);

        battleMenu.SetActive(false); 
        fightMenu.gameObject.SetActive(true);
        fightMenu.SetMoves(ally);
    }
}
