using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using CallbackContext = UnityEngine.InputSystem.InputAction.CallbackContext;

public class PartyMenu : ContextMenu<Pokemon[]>
{
    [SerializeField] private Announcer partyAnnouncer;
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

    private static PartyMenu instance;

    protected override void Awake()
    {
        instance = this;
        base.Awake();
        GetPartyPokemon();
        contextSelection.onItemPick += OnPickPokemon;
        pokemonOptions.onItemPick += OnPickOption;
    }

    private void SetupNavigation()
    {
        #region Pokemon Navigation
        Selectable backButton = contextSelection[contextSelection.itemCount-1].selectable;
        List<Selectable> navigationItems = new(party.Length);
        for (int i = 0; i < instances.Length; i++)
        {
            PartyPokemon item = instances[i];
            item.SetupPokemon(i < party.Length ? party[i] : null);

            if (i >= party.Length) continue;

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
    
    public override void OpenMenu(Pokemon[] pokemons)
    {
        Announcer.ChangeAnnouncer(partyAnnouncer);
        party = pokemons;
        SetupNavigation();
        gameObject.SetActive(true);
        contextSelection.MouseSelection(true);
        contextSelection.Select(0);
        base.OpenMenu(party);
    }

    private void OnPickPokemon(int id)
    {
        if (id == contextSelection.itemCount - 1)
        {
            ReturnCall(new());
            return;
        }

        text.SetActive(false);
        contextSelection.MouseSelection(false);
        optionsWindow.SetActive(true);
        pokemonOptions.Select(0);
    }

    private void OnPickOption(int id)
    {
        ReturnCall(new());
        switch (id)
        {
            case 0:
                onSummaryCall?.Invoke(contextSelection.selectedId);
                break;
            case 1:
                //chose pokemon
                if (contextSelection.selectedId == 0)
                {
                    Announcer.Announce("This pokemon is already in battle!", true,
                        onDone: () =>
                        {
                            Announcer.CloseAnnouncement();
                            contextSelection.Focus();
                        });
                    return;
                }
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

    protected override void ReturnCall(CallbackContext context)
    {
        if (!optionsWindow.activeSelf)
            base.ReturnCall(context);
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
        base.CloseMenu();
        gameObject.SetActive(false);
    }

    public static IEnumerator PickPokemon(PickPokemonEvent evt)
    {
        if (evt.currentPokemonOnly)
        {
            evt.pickedPokemon = instance.party[0];
            evt.isCurrent = true;
            yield break;
        }
        //setup
        bool selected = false;
        List<bool> moveLearnability = new(instance.party.Length);
        if (!instance.gameObject.activeSelf) instance.OpenMenu(instance.party);
        else instance.contextSelection.Focus();
        instance.contextSelection.onItemPick -= instance.OnPickPokemon;
        instance.contextSelection.onItemPick += SelectPokemon;
        instance.cancelAction.performed -= instance.ReturnCall;
        instance.cancelAction.performed += CancelSelection;

        if (evt.move != null)
        {
            for (int i = 0; i < instance.party.Length; i++)
                moveLearnability.Add(instance.instances[i].LearnMoveMode(evt.move.data.moveData));
        }

        yield return new WaitUntil(PokemonSelected);

        //reset
        instance.contextSelection.onItemPick += instance.OnPickPokemon;
        instance.contextSelection.onItemPick -= SelectPokemon;
        instance.cancelAction.performed += instance.ReturnCall;
        instance.cancelAction.performed -= CancelSelection;
        yield break;

        void SelectPokemon(int id)
        {
            if (id < instance.contextSelection.itemCount - 1) evt.pickedPokemon = instance.party[id];
            selected = true;
            evt.isCurrent = id == 0;
            if (evt.move == null || id >= moveLearnability.Count) return;
            evt.canLearnTM = moveLearnability[id];
        }

        bool PokemonSelected() => selected;

        void CancelSelection(CallbackContext context) => selected = true;
    }

    public static void ClosePartyMenu()
    {
        instance.CloseMenu();
    }
}

public class PickPokemonEvent
{
    //setup
    public bool currentPokemonOnly;
    public TMModel move;
    
    //feedback
    public bool isCurrent;
    public Pokemon pickedPokemon;
    public bool canLearnTM;

    public PickPokemonEvent(bool currentOnly, TMModel tm = null)
    {
        currentPokemonOnly = currentOnly;
        move = tm;
    }
}