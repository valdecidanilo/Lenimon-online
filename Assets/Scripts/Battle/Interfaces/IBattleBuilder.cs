using System;

namespace Battle.Interfaces
{
    public interface IBattleBuilder
    {
        void Build(Trainer player, Trainer opponent, Action onReady);
    }
}