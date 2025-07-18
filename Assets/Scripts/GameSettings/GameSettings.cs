using UnityEngine;

namespace GameSettings
{
    [CreateAssetMenu(fileName = "GameSettings", menuName = "SliceMon/Game Settings")]
    public class GameSettings : ScriptableObject
    {
        [Header("Player")]
        public float moveSpeed = .5f;
        public float radiusPlayer = 0.16f;
        
        [Header("Battle and Move Settings")]
        [Range(0f, 1f)]public float encounterBattleChance = 0.1f;
        public float radiusBattle = 0.05f;
        public float moveTurnDelay = 0.15f;

        [Header("Grid Settings")]
        public float gridSize = 0.16f;
        public float radiusGrass = 0.16f;
        public float originY = 0.2f;

        private static GameSettings _instance;
        public static GameSettings Instance
        {
            get
            {
                if (_instance == null)
                    _instance = Resources.Load<GameSettings>("GameSettings");

                return _instance;
            }
        }
    }
}