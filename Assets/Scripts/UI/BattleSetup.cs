using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class BattleSetup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] Sprite male;
    [SerializeField] Sprite female;

    //Enemy
    private Pokemon enemyPokemon;
    private Pokemon[] enemyParty;
    [Header("Enemy")]
    [SerializeField] private Image enemyImage;
    [SerializeField] private TMP_Text enemyName;
    [SerializeField] private TMP_Text enemyLevel;
    [SerializeField] private Image enemyHp;
    [SerializeField] private Image enemyGender;

    //Ally
    private Pokemon allyPokemon;
    private Pokemon[] allyParty;
    [Header("Ally")]
    [SerializeField] private Image pokemonImage;
    [SerializeField] private TMP_Text pokemonName;
    [SerializeField] private TMP_Text level;
    [SerializeField] private Image hp;
    [SerializeField] private TMP_Text hpValue;
    [SerializeField] private Image xp;
    [SerializeField] private Image gender;

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
        partyChoice.onChangePokemon += OnAllyChanged;
    }

    public void SetupBattle(Pokemon[] allies, Pokemon[] enemies)
    {
        //setup enemy
        enemyParty = enemies;
        SetupEnemy(enemyParty[0]);

        //setup ally
        allyParty = allies;
        SetupAlly(allyParty[0]);

        OpenChoiceMenu();
    }

    private void SetupAlly(Pokemon ally)
    {
        allyPokemon = ally;
        pokemonImage.sprite = allyPokemon.backSprite;
        pokemonName.text = allyPokemon.name;
        level.text = $"Lv{allyPokemon.level}";
        hpValue.text = $"{allyPokemon.stats[StatType.hp]}/{allyPokemon.stats[StatType.hp]}";
        hp.fillAmount = 1;
        xp.fillAmount = Random.Range(0, 1);
        gender.gameObject.SetActive(allyPokemon.gender != Gender.NonBinary);
        switch (allyPokemon.gender)
        {
            case Gender.Male:
                gender.sprite = male;
                break;
            case Gender.Female:
                gender.sprite = female;
                break;
        }
    }

    private void SetupEnemy(Pokemon enemy)
    {
        enemyPokemon = enemy;
        enemyImage.sprite = enemyPokemon.frontSprite;
        enemyName.text = enemyPokemon.name;
        enemyLevel.text = $"Lv{enemyPokemon.level}";
        enemyHp.fillAmount = 1;
        enemyGender.gameObject.SetActive(enemyPokemon.gender != Gender.NonBinary);
        switch (enemyPokemon.gender)
        {
            case Gender.Male:
                enemyGender.sprite = male;
                break;
            case Gender.Female:
                enemyGender.sprite = female;
                break;
        }
    }

    private void OnAllyChanged(Pokemon newAlly)
    {
        SetupAlly(newAlly);
        OpenChoiceMenu();
    }

    #region Window Changes
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
        partyChoice.OpenMenu(allyParty);
    }

    private void Run()
    {
        
    }
    #endregion
}
