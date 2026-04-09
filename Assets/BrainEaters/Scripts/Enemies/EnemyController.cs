using BrainEaters.Configs;
using UnityEngine;

namespace BrainEaters.Enemies
{
    [RequireComponent(typeof(EnemyMovement))]
    [RequireComponent(typeof(EnemyHealth))]
    public class EnemyController : MonoBehaviour
    {
        [SerializeField] private EnemyMovement enemyMovement;
        [SerializeField] private EnemyHealth enemyHealth;
        [SerializeField] private Transform target;
        [SerializeField] private EnemyConfig enemyConfig;

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

            if (enemyHealth != null)
            {
                enemyHealth.Configure(enemyConfig);
            }
        }

        private void Update()
        {
            if (target == null || enemyHealth == null || !enemyHealth.IsAlive)
            {
                return;
            }

            enemyMovement.Tick(target.position, Time.deltaTime);
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
        }
    }
}
