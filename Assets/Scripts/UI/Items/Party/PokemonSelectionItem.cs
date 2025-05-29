using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PokemonSelectionItem : PartySelectionItem
{
    [Space(8)] public PokemonState currentState;
    public enum PokemonState
    {
        Normal,
        Swap,
        Faint
    }

    [Header("Swap Sprites")] 
    [SerializeField] private Sprite normalSwap;
    [SerializeField] private Sprite selectedSwap;
    [Header("Faint Sprites")] 
    [SerializeField] private Sprite normalFaint;
    [SerializeField] private Sprite selectedFaint;

    protected override void OnSelection(int id)
    {
        switch (currentState)
        {
            case PokemonState.Normal:
                base.OnSelection(id);
                break;
            case PokemonState.Swap:
                image.sprite = context.currentSelected == targetItem ? normalSwap : selectedSwap;
                break;
            case PokemonState.Faint:
                image.sprite = context.currentSelected == targetItem ? normalFaint : selectedFaint;
                break;
        }
    }

    public void ChangeState(PokemonState newState)
    {
        currentState = newState;
        OnSelection(context.selectedId);
    }
}
