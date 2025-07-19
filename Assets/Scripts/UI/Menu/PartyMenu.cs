using System;
using System.Collections;
using System.Collections.Generic;
using Battle;
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
    public event Action<int> onSummaryCall;

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
            if (item != null)
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
        for (var i = 0; i < instances.Length; i++)
        {
            instances[i] = contextSelection[i].GetComponent<PartyPokemon>();
            if (instances[i] == null)
                Debug.LogError($"[PartyMenu] contextSelection[{i}] não tem PartyPokemon!", contextSelection[i]);
        }
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
                FightMenu.BeginBattle(MoveEffectCreator.SwitchPokemonMove(contextSelection.selectedId));
                CloseMenu();
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
            evt.partyId = 0;
            evt.pickedPokemon = instance.party[0];
            evt.isCurrent = true;
            yield break;
        }

        bool validPokemonExists = false;
        for (int i = 0; i < instance.party.Length; i++)
        {
            evt.pickedPokemon = instance.party[i];
            validPokemonExists |= evt.CheckPokemon();
        }
        evt.noValidPokemon = !validPokemonExists;
        if (!validPokemonExists) yield break;
        evt.pickedPokemon = null;
        
        //setup
        bool selected = false;
        bool cancel = false;
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

        do
        {
            yield return new WaitUntil(PokemonSelected);
            selected = false;
            if(cancel) break;
            if (!evt.canBeFainted && evt.pickedPokemon.fainted)
                yield return Announcer.AnnounceCoroutine("You can't switch to a fainted pokemon!", true,
                    onDone: ResetPick);
            else if (!evt.canSelectCurrent && evt.isCurrent)
                yield return Announcer.AnnounceCoroutine("Pokemon is already in battle!", true,
                    onDone: ResetPick);
        } while (!evt.CheckPokemon());

        //reset
        instance.contextSelection.onItemPick += instance.OnPickPokemon;
        instance.contextSelection.onItemPick -= SelectPokemon;
        instance.cancelAction.performed += instance.ReturnCall;
        instance.cancelAction.performed -= CancelSelection;
        yield break;

        void SelectPokemon(int id)
        {
            if (id < instance.contextSelection.itemCount - 1)
            {
                evt.partyId = id;
                evt.pickedPokemon = instance.party[id];
                selected = true;
            }
            else cancel = true;
            evt.isCurrent = id == 0;
            if (evt.move == null || id >= moveLearnability.Count) return;
            evt.canLearnTM = moveLearnability[id];
        }

        bool PokemonSelected() => selected || cancel;

        void CancelSelection(CallbackContext context) => cancel = true;

        void ResetPick()
        {
            cancel = false;
            evt.pickedPokemon = null;
            evt.partyId = -1;
            Announcer.CloseAnnouncement();
            instance.contextSelection.Focus();
        }
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
    
    //flags
    public bool canSelectCurrent = true;
    public bool canBeFainted = true;
    
    //feedback
    public int partyId = -1;
    public Pokemon pickedPokemon;
    public bool isCurrent;
    public bool canLearnTM;
    public bool noValidPokemon;

    /// <param name="currentOnly">if the current pokemon is the only option</param>
    /// <param name="fainted">if selected pokemon can be fainted</param>
    /// <param name="current">if the selected pokemon can be the current one</param>
    /// <param name="tm">tm to learn, if aplicable</param>
    public PickPokemonEvent(bool currentOnly = false, bool fainted = true, bool current = true, TMModel tm = null)
    {
        currentPokemonOnly = currentOnly;
        canBeFainted = fainted;
        canSelectCurrent = current;
        move = tm;
    }

    public bool CheckPokemon()
    {
        if (pickedPokemon == null) return false;
        bool valid = true;
        valid &= canSelectCurrent || !isCurrent;
        valid &= canBeFainted || !pickedPokemon.fainted;
        return valid;
    }
}