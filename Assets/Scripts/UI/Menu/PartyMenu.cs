using System.Collections.Generic;
using UnityEngine.UI;

public class PartyMenu : ContextMenu<Pokemon[]>
{
    private void SetupNavigation(Pokemon[] pokemons)
    {
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
    }
    
    private void Awake()
    {
        contextSelection.onItemPick += OnPick;
    }
    
    public override void OpenMenu(Pokemon[] pokemons)
    {
        SetupNavigation(pokemons);
        gameObject.SetActive(true);
        contextSelection.Select(0);
    }

    private void OnPick(int id)
    {
        if (id == contextSelection.itemCount - 1) ReturnCall();
    }

    public override void CloseMenu()
    {
        gameObject.SetActive(false);
    }
}
