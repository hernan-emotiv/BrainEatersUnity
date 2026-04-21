using BrainEaters.Configs;
using BrainEaters.Player;
using BrainEaters.Turrets;
using UnityEngine;

namespace BrainEaters.Enemies
{
    [RequireComponent(typeof(EnemyMovement))]
    [RequireComponent(typeof(EnemyHealth))]
    [RequireComponent(typeof(EnemyAttack))]
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private EnemyMovement enemyMovement;
        [SerializeField] private EnemyHealth enemyHealth;
        [SerializeField] private EnemyAttack enemyAttack;
        [SerializeField] private EnemyAnimatorDriver enemyAnimatorDriver;
        [SerializeField] private EnemyHopVisual enemyHopVisual;
        [SerializeField] private Transform target;
        [SerializeField] private EnemyConfig enemyConfig;

        private PlayerHealth targetHealth;
        private TurretHealth turretTargetHealth;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnValidate()
        {
            ResolveReferences();
        }

        public void Initialize(Transform targetTransform, EnemyConfig config)
        {
            target = targetTransform;
            enemyConfig = config;

            if (enemyMovement != null)
            {
                enemyMovement.Configure(enemyConfig);
            }

            if (enemyAttack != null)
            {
                enemyAttack.Configure(enemyConfig);
            }

            if (enemyHealth != null)
            {
                enemyHealth.Configure(enemyConfig);
            }

            if (enemyAnimatorDriver != null)
            {
                enemyAnimatorDriver.ResetState();
            }

            if (enemyHopVisual != null)
            {
                enemyHopVisual.ResetState();
            }

            targetHealth = targetTransform != null ? targetTransform.GetComponent<PlayerHealth>() : null;
            turretTargetHealth = null;
        }

        private void Update()
        {
            if (enemyHealth == null || !enemyHealth.IsAlive)
            {
                return;
            }

            ResolveCombatTarget();
            if (target == null)
            {
                return;
            }

            if (targetHealth == null && turretTargetHealth == null)
            {
                targetHealth = target.GetComponent<PlayerHealth>();
            }

            if (enemyAttack != null && enemyAttack.IsAttacking)
            {
                enemyHopVisual?.SetMoving(false);
                enemyMovement.FaceTowards(target.position, Time.deltaTime);
                return;
            }

            if (enemyAttack != null && enemyAttack.IsTargetInRange(target.position))
            {
                enemyHopVisual?.SetMoving(false);
                enemyMovement.FaceTowards(target.position, Time.deltaTime);

                if (enemyAttack.TryAttack())
                {
                    enemyAnimatorDriver?.PlayAttack();
                }
                else
                {
                    enemyAnimatorDriver?.PlayIdle();
                }

                return;
            }

            bool moved = enemyMovement.Tick(target.position, Time.deltaTime);
            enemyHopVisual?.SetMoving(moved);
            if (moved)
            {
                enemyAnimatorDriver?.PlayWalk();
            }
            else
            {
                enemyAnimatorDriver?.PlayIdle();
            }
        }

        private void ResolveReferences()
        {
            if (enemyMovement == null)
            {
                enemyMovement = GetComponent<EnemyMovement>();
            }

            if (enemyHealth == null)
            {
                enemyHealth = GetComponent<EnemyHealth>();
            }

            if (enemyAttack == null)
            {
                enemyAttack = GetComponent<EnemyAttack>();
            }

            if (enemyAnimatorDriver == null)
            {
                enemyAnimatorDriver = GetComponent<EnemyAnimatorDriver>();
            }

            if (enemyHopVisual == null)
            {
                enemyHopVisual = GetComponent<EnemyHopVisual>();
            }
        }

        private void ResolveCombatTarget()
        {
            Transform playerTarget = targetHealth != null ? targetHealth.transform : target;
            if (playerTarget != null && targetHealth == null)
            {
                targetHealth = playerTarget.GetComponent<PlayerHealth>();
            }

            TurretHealth nearestTurret = TurretTargetRegistry.GetNearestTarget(transform.position);
            bool canTargetPlayer = targetHealth != null && targetHealth.IsAlive;
            bool canTargetTurret = nearestTurret != null && nearestTurret.IsTargetable;

            if (!canTargetPlayer && !canTargetTurret)
            {
                target = null;
                turretTargetHealth = null;
                return;
            }

            if (canTargetPlayer && !canTargetTurret)
            {
                target = targetHealth.transform;
                turretTargetHealth = null;
                return;
            }

            if (!canTargetPlayer && canTargetTurret)
            {
                turretTargetHealth = nearestTurret;
                target = turretTargetHealth.TargetPoint;
                return;
            }

            float playerDistanceSqr = (targetHealth.transform.position - transform.position).sqrMagnitude;
            float turretDistanceSqr = (nearestTurret.TargetPoint.position - transform.position).sqrMagnitude;
            if (turretDistanceSqr < playerDistanceSqr)
            {
                turretTargetHealth = nearestTurret;
                target = turretTargetHealth.TargetPoint;
            }
            else
            {
                turretTargetHealth = null;
                target = targetHealth.transform;
            }
        }
    }
}
