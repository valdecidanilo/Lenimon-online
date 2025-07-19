using System;

namespace Battle.Interfaces
{
    public interface IBattleSetupStrategy
    {
        public void SetupBattle(GameManager context, Action onComplete);
    }
}