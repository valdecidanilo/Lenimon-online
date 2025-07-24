using System;
using Battle;
using UnityEngine;
using Utils;
using Random = UnityEngine.Random;

namespace Player
{
    public class PlayerEntity : MonoBehaviour
    {
        public string Nickname { get; private set; }
        public bool IsInBattle { get; private set; }
        
        private Collider2D[] grassResults = new Collider2D[10];
        public LayerMask grassLayer;
        [SerializeField] public Trainer trainer = new ();
        [SerializeField] private PlayerView view;

        private void Awake()
        {
            SetNickname("Chupivarú");
        }
        public void UpdateGrass()
        {
            var position = new Vector2(transform.position.x, transform.position.y - GameSettings.GameSettings.Instance.originY);
            Physics2D.OverlapCircleNonAlloc(position, GameSettings.GameSettings.Instance.radiusPlayer, grassResults, grassLayer);
            foreach (var gr in grassResults)
                if(gr != null)
                    gr.gameObject.GetComponent<GrassOverlayLayer>().UpdateVisibility(position.y);
        }

        public void SetNickname(string nickname)
        {
            Nickname = nickname;
            view?.UpdateNickname(nickname, IsInBattle);
        }

        public void SetBattleState(bool inBattle)
        {
            IsInBattle = inBattle;
            view?.UpdateNickname(Nickname, inBattle);
            view?.SetIdleState(true);
            AudioManager.Instance.PlayBattle();
        }
        public void CheckGrass(Action denied = null)
        {
            Vector2 position = new(transform.position.x, transform.position.y - GameSettings.GameSettings.Instance.originY);
            var grass = Physics2D.OverlapCircle(position,
                GameSettings.GameSettings.Instance.radiusGrass, grassLayer);

            if (grass == null) return;
            if (trainer.party.Count == 0) return;
            if (trainer.party[0].fainted)
            {
                Debug.Log("Todos os pokémons estão desmaiados");
                denied?.Invoke();
                return;
            }
            grass.GetComponent<GrassOverlayLayer>().PlayParticles();
            var roll = Random.Range(0f, 1f);
            if (!(roll < GameSettings.GameSettings.Instance.encounterBattleChance)) return;
            SetBattleState(true);
            GameManager.OnInitializeTest?.Invoke();
        }
    }
}