using BrainEaters.Enemies;
using UnityEngine;

namespace BrainEaters.Turrets
{
    public class TurretTargeting : MonoBehaviour
    {
        [SerializeField] private Transform pivot;
        [SerializeField] private float range = 14f;
        [SerializeField] private float turnSpeed = 360f;
        [SerializeField] private LayerMask targetMask = Physics.DefaultRaycastLayers;

        private readonly Collider[] overlapResults = new Collider[24];

        public EnemyHealth CurrentTarget { get; private set; }
        public float Range => range;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnValidate()
        {
            ResolveReferences();
        }

        public EnemyHealth AcquireTarget()
        {
            int hitCount = Physics.OverlapSphereNonAlloc(transform.position, range, overlapResults, targetMask, QueryTriggerInteraction.Ignore);

            EnemyHealth bestTarget = null;
            float bestDistanceSqr = float.MaxValue;

            for (int i = 0; i < hitCount; i++)
            {
                Collider hit = overlapResults[i];
                if (hit == null)
                {
                    continue;
                }

                EnemyHealth enemyHealth = hit.GetComponentInParent<EnemyHealth>();
                if (enemyHealth == null || !enemyHealth.IsAlive)
                {
                    continue;
                }

                float distanceSqr = (enemyHealth.transform.position - transform.position).sqrMagnitude;
                if (distanceSqr < bestDistanceSqr)
                {
                    bestDistanceSqr = distanceSqr;
                    bestTarget = enemyHealth;
                }
            }

            CurrentTarget = bestTarget;
            return CurrentTarget;
        }

        public Vector3 GetAimPoint(EnemyHealth target)
        {
            if (target == null)
            {
                return transform.position;
            }

            Collider targetCollider = target.GetComponentInChildren<Collider>();
            if (targetCollider != null)
            {
                return targetCollider.bounds.center;
            }

            return target.transform.position + Vector3.up;
        }

        public bool AimAt(Vector3 worldPosition, float deltaTime)
        {
            Transform rotateTarget = pivot != null ? pivot : transform;
            Vector3 toTarget = worldPosition - rotateTarget.position;
            if (toTarget.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            Quaternion desiredRotation = Quaternion.LookRotation(toTarget.normalized, Vector3.up);
            rotateTarget.rotation = Quaternion.RotateTowards(rotateTarget.rotation, desiredRotation, turnSpeed * deltaTime);

            float angle = Quaternion.Angle(rotateTarget.rotation, desiredRotation);
            return angle <= 8f;
        }

        private void ResolveReferences()
        {
            if (pivot == null)
            {
                pivot = transform;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.2f, 0.8f, 1f, 0.35f);
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}
