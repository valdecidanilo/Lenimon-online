using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
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
    [SerializeField] private ContextSelection battleChoice;

    [Header("Moves Menu")]
    [SerializeField] private FightMenu fightMenu;

    [Header("Party Menu")]
    [SerializeField] private PartyMenu partyChoice;

    private void Awake()
    {
        battleChoice.onItemPick += OnChoicePick;
        fightMenu.onReturn += OpenChoiceMenu;
        partyChoice.onReturn += OpenChoiceMenu;
    }

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

        OpenChoiceMenu();
    }

    private void OnChoicePick(int choice)
    {
        switch (choice)
        {
            case 0:
                OpenBattleScene();
                break;
            case 1:
                OpenBag();
                break;
            case 2:
                OpenParty();
                break;
            case 3:
                Run();
                break;
            default:
                Debug.Log("choice not found");
                break;
        }
    }

    private void OpenChoiceMenu()
    {
        //disable other windows
        fightMenu.gameObject.SetActive(false);
        partyChoice.gameObject.SetActive(false);

        //open window
        battleMenu.SetActive(true);
        battleChoice.Focus();
    }

    private void OpenBattleScene()
    {
        battleMenu.SetActive(false);
        fightMenu.OpenMenu(allyPokemon);
    }

    private void OpenBag()
    {
        
    }

    private void OpenParty()
    {
        battleMenu.SetActive(false);
        partyChoice.OpenMenu(new[] { allyPokemon });
    }

    private void Run()
    {
        
    }
}
