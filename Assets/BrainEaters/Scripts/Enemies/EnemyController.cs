using BrainEaters.Configs;
using BrainEaters.Player;
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
        [SerializeField] private Transform target;
        [SerializeField] private EnemyConfig enemyConfig;

        private PlayerHealth targetHealth;

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

            targetHealth = targetTransform != null ? targetTransform.GetComponent<PlayerHealth>() : null;
        }

        private void Update()
        {
            if (target == null || enemyHealth == null || !enemyHealth.IsAlive)
            {
                return;
            }

            if (targetHealth == null)
            {
                targetHealth = target.GetComponent<PlayerHealth>();
            }

            if (enemyAttack != null && enemyAttack.IsAttacking)
            {
                enemyMovement.FaceTowards(target.position, Time.deltaTime);
                return;
            }

            if (enemyAttack != null && enemyAttack.IsTargetInRange(target.position))
            {
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
        }
    }
}
