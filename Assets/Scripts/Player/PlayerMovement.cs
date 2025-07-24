using UnityEngine;
using System.Collections;
using TMPro;
using GameSettings;

namespace Player
{
    public class PlayerMovement : MonoBehaviour
    {
        private Vector2 currentDirection;
        private Vector2 inputBuffer;
        private bool isMoving;
        private bool isRuning;
        private bool isWaitingToMove;

        [Header("References")]
        [SerializeField] private PlayerEntity playerEntity;
        [SerializeField] private PlayerView view;

        private void Update()
        {
            if (isMoving || playerEntity == null || playerEntity.IsInBattle) return;

            inputBuffer.x = Input.GetAxisRaw("Horizontal");
            inputBuffer.y = Input.GetAxisRaw("Vertical");
            isRuning = Input.GetKey(KeyCode.LeftShift);

            GameSettings.GameSettings.Instance.moveSpeed = isRuning ? 1f : .5f;
            view.SetRunState(isRuning);

            if (inputBuffer.x != 0) inputBuffer.y = 0;

            if (inputBuffer != Vector2.zero)
            {
                if (inputBuffer != currentDirection || !isWaitingToMove)
                {
                    currentDirection = inputBuffer;
                    isWaitingToMove = false;
                    StartCoroutine(HandleTurn());
                }
                else
                {
                    MoveToTarget();
                }
            }
            else
            {
                isWaitingToMove = false;
                view.SetMoveDirection(Vector2.zero, false);
            }
        }

        private IEnumerator HandleTurn()
        {
            view.SetMoveDirection(currentDirection, false);
            view.FlipSprite(currentDirection);
            yield return new WaitForSeconds(GameSettings.GameSettings.Instance.moveTurnDelay);
            isWaitingToMove = true;
        }

        private void MoveToTarget()
        {
            var targetPos = transform.position + (Vector3)currentDirection * GameSettings.GameSettings.Instance.gridSize;
            StartCoroutine(ExecuteMovement(targetPos));
            view.SetMoveDirection(currentDirection, true);
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
                playerEntity.UpdateGrass();
                yield return null;
            }
            transform.position = destination;
            isMoving = false;
            playerEntity.CheckGrass(() =>
            {
                StartCoroutine(StepBack());
            });
        }
        private IEnumerator StepBack()
        {
            Vector3 backward = -currentDirection.normalized * GameSettings.GameSettings.Instance.gridSize;
            var destination = transform.position + backward;

            while (Vector3.Distance(transform.position, destination) > 0.001f)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    destination,
                    GameSettings.GameSettings.Instance.moveSpeed * Time.deltaTime
                );
                yield return null;
            }
            
            transform.position = destination;
            isMoving = false;
        }

    }
}
