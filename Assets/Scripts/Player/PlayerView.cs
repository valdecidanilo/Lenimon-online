using TMPro;
using UnityEngine;

namespace Player
{
    public class PlayerView : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private PlayerAnimatorController animatorController;

        public void UpdateNickname(string nickname, bool inBattle)
        {
            if (nameText == null) return;
            var displayName = inBattle ? $"<size=150%><sprite=12></size>\n{nickname}" : nickname;
            nameText.SetText(displayName);
        }

        public void SetMoveDirection(Vector2 dir, bool isMoving)
        {
            animatorController.SetMoveDirection(dir, isMoving);
        }

        public void SetRunState(bool isRunning)
        {
            animatorController.SetRunState(isRunning);
        }

        public void SetIdleState(bool idle)
        {
            animatorController.SetIdleState(idle);
        }

        public void FlipSprite(Vector2 direction)
        {
            if (direction.x != 0)
                animatorController.spriteRenderer.flipX = direction.x < 0;
        }
    }
}