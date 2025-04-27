using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class FightMenu : MonoBehaviour
{
    [SerializeField] private ContextSelection movesSelection;
    [Space(5), SerializeField] private TMP_Text moveType;
    [SerializeField] private TMP_Text movePp;
    [SerializeField] private TMP_Text[] moves;

    private Pokemon pokemon;

    public Action onReturn;

    private void Awake()
    {
        movesSelection.onSelect += OnSelectionChanged;
    }

    private void Update()
    {
        if (InputSystem.actions.FindAction("UI/Cancel").WasPressedThisFrame())
            onReturn?.Invoke();
    }

    public void SetMoves(Pokemon pokemon)
    {
        this.pokemon = pokemon;
        for (int i = 0; i < moves.Length; i++)
        {
            moves[i].text = pokemon.moves[i]?.name.Replace("-", " ") ?? "-";
        }

        //first selected
        UpdateMoveData(pokemon.moves[0]);
        movesSelection.Focus();
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
}
