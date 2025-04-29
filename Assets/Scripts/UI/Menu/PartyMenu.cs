using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PartyMenu : ContextMenu<Pokemon[]>
{
    [Header("Text")]
    [SerializeField] private GameObject text;
    [Header("Options")]
    [SerializeField] private GameObject optionsWindow;
    [SerializeField] private ContextSelection pokemonOptions;

    private Pokemon[] party;

    public event Action<Pokemon> onChangePokemon;
    public event Action onSummaryCall;
    public event Action onItemCall;


    private void SetupNavigation(Pokemon[] pokemons)
    {
        #region Pokemon Navigation
        Selectable backButton = contextSelection[contextSelection.itemCount-1].GetComponent<Selectable>();
        List<Selectable> navigationItems = new(pokemons.Length);
        for (int i = 0; i < pokemons.Length; i++)
        {
            PartyPokemon item = contextSelection[i].GetComponent<PartyPokemon>();
            item.SetupPokemon(pokemons[i]);

            Selectable selectable = item.GetComponent<Selectable>();
            navigationItems.Add(selectable);
            Navigation navigation = selectable.navigation;
            navigation.mode = Navigation.Mode.Explicit;
            navigation.selectOnLeft = navigationItems[0];
            if (i > 0)
            {
                navigation.selectOnUp = navigationItems[i - 1];
                
                //set up selectable to itself
                Navigation prevNavigation = navigationItems[i - 1].navigation;
                prevNavigation.selectOnDown = selectable;
                navigationItems[i - 1].navigation = prevNavigation;
            }

            selectable.navigation = navigation;
        }

        Navigation lastNavigation = navigationItems[^1].navigation;
        lastNavigation.mode = Navigation.Mode.Explicit;
        lastNavigation.selectOnDown = backButton;
        navigationItems[^1].navigation = lastNavigation;
        
        Navigation backNavigation = backButton.navigation;
        backNavigation.mode = Navigation.Mode.Explicit;
        backNavigation.selectOnUp = navigationItems[^1];
        backButton.navigation = backNavigation;
        #endregion

        #region Options Navigation
        navigationItems = new(pokemonOptions.itemCount);
        for (int i = 0; i < pokemonOptions.itemCount; i++)
        {
            Selectable selectable = pokemonOptions[i].GetComponent<Selectable>();
            navigationItems.Add(selectable);
            Navigation navigation = selectable.navigation;
            navigation.mode = Navigation.Mode.Explicit;
            if (i > 0)
            {
                navigation.selectOnUp = navigationItems[i - 1];

                //set up selectable to itself
                Navigation prevNavigation = navigationItems[i - 1].navigation;
                prevNavigation.selectOnDown = selectable;
                navigationItems[i - 1].navigation = prevNavigation;
            }

            selectable.navigation = navigation;
        }
        #endregion
    }

    private void Awake()
    {
        contextSelection.onItemPick += OnPickPokemon;
        pokemonOptions.onItemPick += OnPickOption;
    }
    
    public override void OpenMenu(Pokemon[] pokemons)
    {
        party = pokemons;
        SetupNavigation(pokemons);
        gameObject.SetActive(true);
        contextSelection.Select(0);
    }

    private void OnPickPokemon(int id)
    {
        if (id == contextSelection.itemCount - 1)
        {
            ReturnCall();
            return;
        }

        text.SetActive(false);
        optionsWindow.SetActive(true);
        pokemonOptions.Select(0);
    }

    private void OnPickOption(int id)
    {
        switch (id)
        {
            case 0:
                onSummaryCall?.Invoke();
                break;
            case 1:
                //chose pokemon
                ReturnCall();
                onChangePokemon?.Invoke(party[contextSelection.selectedId]);
                break;
            case 2:
                //switch pokemon
                break;
            case 3:
                onItemCall?.Invoke();
                break;
            case 4:
                ReturnCall();
                break;
        }
    }

    protected override void ReturnCall()
    {
        if (!optionsWindow.activeSelf)
            base.ReturnCall();
        else
        {
            text.SetActive(true);
            optionsWindow.SetActive(false);
            contextSelection.Focus();
        }
    }

    public override void CloseMenu()
    {
        gameObject.SetActive(false);
    }
}
