using System;
using LenixSO.Logger;
using TMPro;
using UnityEngine;
using Logger = LenixSO.Logger.Logger;

public class FightMenu : ContextMenu<Pokemon>
{
    [Space(5), SerializeField] private TMP_Text moveType;
    [SerializeField] private TMP_Text movePp;
    [SerializeField] private TMP_Text[] moves;

    private Pokemon pokemon;

    public event Action<int> onPickMove; 

    protected override void Awake()
    {
        base.Awake();
        contextSelection.onSelect += OnSelectionChanged;
        contextSelection.onItemPick += OnMovePick;
    }

    public override void OpenMenu(Pokemon data)
    {
        gameObject.SetActive(true);
        pokemon = data;
        for (int i = 0; i < moves.Length; i++)
        {
            moves[i].text = pokemon.moves[i]?.name ?? "-";
        }

        //first selected
        UpdateMoveData(pokemon.moves[0]);
        contextSelection.Focus();
    }

    public override void CloseMenu()
    {
        gameObject.SetActive(false);
    }

    private void OnSelectionChanged(int id)
    {
        if (id < 0 || id >= pokemon.moves.Length) return;

        UpdateMoveData(pokemon.moves[id]);
    }

    private void UpdateMoveData(MoveModel move)
    {
        moveType.text = move.moveType;
        int ppAmount = move.pp;
        movePp.text = $"{ppAmount}/{ppAmount}";
    }

    private void OnMovePick(int id)
    {
        Logger.Log($"{pokemon.name} will use {pokemon.moves[id].name}",LogFlags.Game);
        onPickMove?.Invoke(id);
    }
}
