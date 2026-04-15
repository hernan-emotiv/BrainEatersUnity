using BrainEaters.Configs;
using UnityEngine;
using UnityEngine.AI;

namespace BrainEaters.Enemies
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyMovement : MonoBehaviour
    {
        [SerializeField] private NavMeshAgent navMeshAgent;
        [SerializeField, HideInInspector] private float moveSpeed = 3.5f;
        [SerializeField, HideInInspector] private float turnSpeed = 540f;
        [SerializeField, HideInInspector] private float stopDistance = 1.25f;

        private void Awake()
        {
            ResolveReferences();
            ConfigureAgent();
        }

        private void OnValidate()
        {
            ResolveReferences();
            ConfigureAgent();
        }

        public void Configure(EnemyConfig enemyConfig)
        {
            if (enemyConfig == null)
            {
                return;
            }

            moveSpeed = enemyConfig.MoveSpeed;
            turnSpeed = enemyConfig.TurnSpeed;
            stopDistance = enemyConfig.StopDistance;
            ConfigureAgent();
        }

        public bool Tick(Vector3 targetPosition, float deltaTime)
        {
            if (navMeshAgent == null || !navMeshAgent.enabled || !navMeshAgent.isOnNavMesh)
            {
                return false;
            }

            Vector3 toTarget = targetPosition - transform.position;
            toTarget.y = 0f;

            float distance = toTarget.magnitude;
            if (distance <= stopDistance || distance <= 0.001f)
            {
                navMeshAgent.isStopped = true;
                return false;
            }

            navMeshAgent.isStopped = false;
            navMeshAgent.SetDestination(targetPosition);

            Vector3 velocity = navMeshAgent.desiredVelocity;
            velocity.y = 0f;
            if (velocity.sqrMagnitude > 0.001f)
            {
                Quaternion desiredRotation = Quaternion.LookRotation(velocity.normalized, Vector3.up);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRotation, turnSpeed * deltaTime);
            }

            return navMeshAgent.remainingDistance > stopDistance;
        }

        public void FaceTowards(Vector3 targetPosition, float deltaTime)
        {
            if (navMeshAgent != null && navMeshAgent.enabled)
            {
                navMeshAgent.isStopped = true;
            }

            Vector3 toTarget = targetPosition - transform.position;
            toTarget.y = 0f;
            if (toTarget.sqrMagnitude <= 0.0001f)
            {
                return;
            }

            Quaternion desiredRotation = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, desiredRotation, turnSpeed * deltaTime);
        }

        private void ResolveReferences()
        {
            if (navMeshAgent == null)
            {
                navMeshAgent = GetComponent<NavMeshAgent>();
            }
        }

        private void ConfigureAgent()
        {
            if (navMeshAgent == null)
            {
                return;
            }

            navMeshAgent.speed = moveSpeed;
            navMeshAgent.angularSpeed = turnSpeed;
            navMeshAgent.stoppingDistance = stopDistance;
            navMeshAgent.acceleration = Mathf.Max(8f, moveSpeed * 4f);
            navMeshAgent.updateRotation = false;
        }
    }
}
