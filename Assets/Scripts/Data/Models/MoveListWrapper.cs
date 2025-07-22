namespace Data.Models
{
    [System.Serializable]
    public class MoveListWrapper
    {
        public int[] id;
        public int[] currentPP;

        public MoveListWrapper(MoveModel[] moves)
        {
            id = new int[4];
            currentPP = new int[4];
            for (var i = 0; i < moves.Length; i++)
            {
                if(moves[i] == null) continue;
                id[i] = moves[i].id;
                currentPP[i] = moves[i].pp;
            }
        }
    }
}