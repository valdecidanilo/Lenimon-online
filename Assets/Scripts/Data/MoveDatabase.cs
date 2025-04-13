using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "new moveDatabase", menuName = "Move Database")]
public class MoveDatabase : ScriptableObject
{
    [SerializeField] private List<string> keys = new();
    [SerializeField] private List<MoveModel> moves = new();

    public MoveModel GetModel(string moveName)
    {
        return moves[keys.IndexOf(moveName)];
    }

    public void LoadMoves(List<MoveReference> references)
    {

    }

    public bool Contains(string moveName) => keys.Contains(moveName);
}
