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
    private PartyPokemon[] instances;

    public event Action<int> onChangePokemon;
    public event Action<int> onSummaryCall;
    public event Action onItemCall;

    private void SetupNavigation(Pokemon[] pokemons)
    {
        #region Pokemon Navigation
        Selectable backButton = contextSelection[contextSelection.itemCount-1].selectable;
        if (instances == null) GetPartyPokemon();
        List<Selectable> navigationItems = new(pokemons.Length);
        for (int i = 0; i < pokemons.Length; i++)
        {
            PartyPokemon item = instances[i];
            item.SetupPokemon(pokemons[i]);

            Selectable selectable = contextSelection[i].selectable;
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
            Selectable selectable = pokemonOptions[i].selectable;
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

    private void GetPartyPokemon()
    {
        instances = new PartyPokemon[contextSelection.itemCount - 1];
        for (int i = 0; i < instances.Length; i++)
            instances[i] = contextSelection[i].GetComponent<PartyPokemon>();
    }

    protected override void Awake()
    {
        base.Awake();
        if (instances == null) GetPartyPokemon();
        contextSelection.onItemPick += OnPickPokemon;
        pokemonOptions.onItemPick += OnPickOption;
    }
    
    public override void OpenMenu(Pokemon[] pokemons)
    {
        party = pokemons;
        SetupNavigation(pokemons);
        gameObject.SetActive(true);
        contextSelection.MouseSelection(true);
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
        contextSelection.MouseSelection(false);
        optionsWindow.SetActive(true);
        pokemonOptions.Select(0);
    }

    private void OnPickOption(int id)
    {
        ReturnCall();
        switch (id)
        {
            case 0:
                onSummaryCall?.Invoke(contextSelection.selectedId);
                break;
            case 1:
                //chose pokemon
                onChangePokemon?.Invoke(contextSelection.selectedId);
                break;
            case 2:
                //switch pokemon
                break;
            case 3:
                onItemCall?.Invoke();
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
            contextSelection.MouseSelection(true);
            contextSelection.Focus();
        }
    }

    public override void CloseMenu()
    {
        gameObject.SetActive(false);
    }
}
