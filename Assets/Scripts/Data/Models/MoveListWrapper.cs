using Newtonsoft.Json;
using UnityEngine;

namespace Data.Models
{
    [System.Serializable]
    public class MoveListWrapper
    {
        public int[] id;
        public int[] currentPP;

        public MoveListWrapper()
        {
            id = new int[4];
            currentPP = new int[4];
        }
        public MoveListWrapper(MoveModel[] moves)
        {
            id = new int[4];
            currentPP = new int[4];
            
            for (var i = 0; i < moves.Length; i++)
            {
                if (moves[i] == null)
                {
                    id[i] = 0;
                    currentPP[i] = 0;
                    continue;
                }

                id[i] = moves[i].id;
                currentPP[i] = moves[i].pp;
            }
        }
    }
}