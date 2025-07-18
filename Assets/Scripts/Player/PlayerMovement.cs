using System;
using System.Collections;
using UnityEngine;
using GameSettings;
using Utils;
using Logger = LenixSO.Logger.Logger;
using Random = UnityEngine.Random;

namespace Player
{
    public class PlayerMovement : MonoBehaviour
    {
        
        private Vector2 currentDirection;
        private Vector2 inputBuffer;
        private bool isMoving;
        private bool isRuning;
        public bool isInBattle;
        private bool isWaitingToMove;
        private bool isTurning;
        private Collider2D[] grassResults = new Collider2D[10];
        
        [Header("References")]
        [SerializeField] private PlayerAnimatorController animatorController;
        [SerializeField] private BattleTriggerZone battleTriggerZone;
        [SerializeField] private LayerMask grassLayer;
        
        
        private void Update()
        {
            if (isMoving || isInBattle) return;

            inputBuffer.x = Input.GetAxisRaw("Horizontal");
            inputBuffer.y = Input.GetAxisRaw("Vertical");
            isRuning = Input.GetKey(KeyCode.LeftShift);
            GameSettings.GameSettings.Instance.moveSpeed = isRuning ? 1f : .5f;
            animatorController.SetRunState(isRuning);

            if (inputBuffer.x != 0) inputBuffer.y = 0;

            if (inputBuffer != Vector2.zero)
            {
                if (inputBuffer != currentDirection || !isWaitingToMove)
                {
                    currentDirection = inputBuffer;
                    isWaitingToMove = false;
                    StartCoroutine(HandleTurn());
                }
                else if (inputBuffer == currentDirection && !isTurning)
                {
                    MoveToTarget();
                }
            }
            else
            {
                isWaitingToMove = false;
                animatorController.SetMoveDirection(Vector2.zero, false);
            }
        }
        private IEnumerator HandleTurn()
        {
            animatorController.SetMoveDirection(currentDirection, false);
            UpdateSpriteFlip();
            yield return new WaitForSeconds(GameSettings.GameSettings.Instance.moveTurnDelay);
            isWaitingToMove = true;
        }

        private void UpdateSpriteFlip()
        {
            if (currentDirection.x != 0)
            {
                animatorController.spriteRenderer.flipX = currentDirection.x < 0;
            }
        }

        private void MoveToTarget()
        {
            var targetPos = transform.position + (Vector3)currentDirection * GameSettings.GameSettings.Instance.gridSize;
            
            StartCoroutine(ExecuteMovement(targetPos));
            animatorController.SetMoveDirection(currentDirection, true);
        }

        private IEnumerator ExecuteMovement(Vector3 destination)
        {
            isMoving = true;
    
            while (Vector3.Distance(transform.position, destination) > 0.001f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position, 
                    destination, 
                    GameSettings.GameSettings.Instance.moveSpeed * Time.deltaTime
                );
                UpdateGrass();
                yield return null;
            }
            
            transform.position = destination;
            isMoving = false;
    
            if (inputBuffer == Vector2.zero)
                animatorController.SetMoveDirection(Vector2.zero, false);
            CheckBattleInGrass();
        }

        private void UpdateGrass()
        {
            var position = new Vector2(transform.position.x, transform.position.y - GameSettings.GameSettings.Instance.originY);
            Physics2D.OverlapCircleNonAlloc(position, GameSettings.GameSettings.Instance.radiusPlayer, grassResults, grassLayer);
                foreach (var gr in grassResults)
                    if(gr != null)
                        gr.gameObject.GetComponent<GrassOverlayLayer>().UpdateVisibility(position.y);
        }
        private void CheckBattleInGrass()
        {
            var position = new Vector2(transform.position.x, transform.position.y - GameSettings.GameSettings.Instance.originY);
            
            var grass = Physics2D.OverlapCircle(position,
                GameSettings.GameSettings.Instance.radiusGrass,
                grassLayer);
            if (grass == null) return;
            var g = grass.GetComponent<GrassOverlayLayer>();
            g.PlayParticles();
            var roll = Random.Range(0f, 1f);
            if (roll < GameSettings.GameSettings.Instance.encounterBattleChance)
            {
                FindedBattle();
            }
        }
        private void FindedBattle()
        {
            Logger.Log("Finded Battle in Grass");
            isInBattle = true;
            inputBuffer = Vector2.zero;
            animatorController.SetIdleState(true);
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            var origin = new Vector2(transform.position.x, transform.position.y - GameSettings.GameSettings.Instance.originY);
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(origin, GameSettings.GameSettings.Instance.radiusGrass);
            
            Gizmos.color = Color.cyan;
            var originBattleZone = new Vector2(battleTriggerZone.Position.x, battleTriggerZone.Position.y - GameSettings.GameSettings.Instance.originY);
            Gizmos.DrawWireSphere(originBattleZone, GameSettings.GameSettings.Instance.radiusPlayer);

        }
#endif
    }
}