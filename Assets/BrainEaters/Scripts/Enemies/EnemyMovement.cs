using BrainEaters.Configs;
using UnityEngine;

namespace BrainEaters.Enemies
{
    public class EnemyMovement : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 3.5f;
        [SerializeField] private float turnSpeed = 540f;
        [SerializeField] private float stopDistance = 1.25f;

        public void Configure(EnemyConfig enemyConfig)
        {
            if (enemyConfig == null)
            {
                return;
            }

            moveSpeed = enemyConfig.MoveSpeed;
            stopDistance = enemyConfig.StopDistance;
        }

        public void Tick(Vector3 targetPosition, float deltaTime)
        {
            Vector3 toTarget = targetPosition - transform.position;
            toTarget.y = 0f;

            float distance = toTarget.magnitude;
            if (distance <= stopDistance || distance <= 0.001f)
            {
                return;
            }

            Vector3 direction = toTarget / distance;
            Quaternion desiredRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRotation, turnSpeed * deltaTime);
            transform.position += direction * (moveSpeed * deltaTime);
        }
    }
}
