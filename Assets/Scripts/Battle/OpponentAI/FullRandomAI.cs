using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullRandomAI : Opponent
{
    public override MoveModel ChooseMove(Pokemon pokemon)
    {
        MoveModel pickedMove = activePokemon.moves[Random.Range(0, 4)];
        while (pickedMove == null) pickedMove = activePokemon.moves[Random.Range(0, 4)];
        return pickedMove;
    }
}
