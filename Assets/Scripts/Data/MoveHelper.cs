using LenixSO.Logger;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using Logger = LenixSO.Logger.Logger;

public static class MoveHelper
{
    public static List<MoveReference> GetPossibleMoves(Pokemon pokemon, MoveLearnMethod[] methodFilter)
    {
        List<MoveReference> possibleMoves = new(pokemon.data.moves.Count);
        for (int moveId = 0; moveId < pokemon.data.moves.Count; moveId++)
        {
            MoveReference move = pokemon.data.moves[moveId];
            for (int detailsId = 0; detailsId < move.learningDetails.Count; detailsId++)
            {
                LearningDetail learningDetail = move.learningDetails[detailsId];
                bool inFilter = false;
                for (int filter = 0; filter < methodFilter.Length; filter++)
                {
                    if (learningDetail.learnMethod != methodFilter[filter]) continue;
                    inFilter = true;
                    break;
                }
                if (!inFilter) continue;
                if (learningDetail.level > pokemon.level) continue;
                possibleMoves.Add(move);
                break;
            }
        }
        return possibleMoves;
    }

    public static void GetRandomMoves(Pokemon pokemon, MoveLearnMethod[] methodFilter, Action onFinished = null)
    {
        List<MoveReference> possibleMoves = GetPossibleMoves(pokemon, methodFilter);

        //test
        possibleMoves = new List<MoveReference>();
        possibleMoves.Add(new() { move = new() { url = "pokeapi.co/api/v2/move/recover" } });
        //possibleMoves.Add(new() { move = new() { url = "pokeapi.co/api/v2/move/drain-punch" } });
        possibleMoves.Add(new() { move = new() { url = "pokeapi.co/api/v2/move/pin-missile" } });
        possibleMoves.Add(new() { move = new() { url = "pokeapi.co/api/v2/move/rock-tomb" } });

        MoveReference[] newMoves = new MoveReference[4];
        Checklist loadedMoves = new(Mathf.Min(possibleMoves.Count, 4));
        GetRandomMove();

        void GetRandomMove()
        {
            int moveId = Random.Range(0, possibleMoves.Count);
            newMoves[loadedMoves.currentSteps] = possibleMoves[moveId];
            possibleMoves.RemoveAt(moveId);
            PokeAPI.GetMoveData(newMoves[loadedMoves.currentSteps].move.url, LoadMove);

            void LoadMove(MoveData data)
            {
                pokemon.moves[loadedMoves.currentSteps] = new MoveModel(data);
                loadedMoves.FinishStep();
                Logger.Log($"{pokemon.name}[{loadedMoves.currentSteps}/{loadedMoves.requiredSteps}] => {data.name}", LogFlags.PokemonBuild);

                if (loadedMoves.isDone)
                {
                    Logger.Log($"{pokemon.name} Moves done loading", LogFlags.PokemonBuild);
                    onFinished?.Invoke();
                    return;
                }
                GetRandomMove();
            }
        }
    }

    public static IEnumerator LearnMoveSequence(Pokemon pokemon, MoveData moveData)
    {
        MoveModel move = new(moveData);
        int freeSlot = -1;
        for (int i = 0; i < pokemon.moves.Length; i++)
        {
            if(pokemon.moves[i] != null) continue;
            freeSlot = i;
            break;
        }

        if (freeSlot < 0)
        {
            yield return Announcer.Announce($"{pokemon.name} wants to learn {move.name}.", true, .2f);
            yield return Announcer.Announce($"But {pokemon.name} already knows 4 moves.", true, .2f);
            yield return Announcer.Announce($"Choose a move to be overwritten by {move.name}.", true, .2f);
        }

        yield return Announcer.Announce($"{pokemon.name} learned {move.name}!", true, .2f);
    }
}
