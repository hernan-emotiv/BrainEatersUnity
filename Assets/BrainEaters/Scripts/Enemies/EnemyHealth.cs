using System;
using BrainEaters.Configs;
using BrainEaters.Core;
using UnityEngine;

namespace BrainEaters.Enemies
{
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        [SerializeField, HideInInspector] private float maxHealth = 1f;
        [SerializeField, HideInInspector] private float destroyDelaySeconds = 1.25f;

        public event Action<EnemyHealth> Died;

        public float CurrentHealth { get; private set; }
        public bool IsAlive { get; private set; }
        public EnemyConfig EnemyConfig { get; private set; }

        private void Awake()
        {
            ResetState();
        }

        public void Configure(EnemyConfig enemyConfig)
        {
            if (enemyConfig == null)
            {
                return;
            }

            EnemyConfig = enemyConfig;
            maxHealth = enemyConfig.MaxHealth;
            destroyDelaySeconds = enemyConfig.DestroyDelaySeconds;
            ResetState();
        }

        public void ApplyDamage(float amount)
        {
            if (!IsAlive)
            {
                return;
            }

            CurrentHealth -= amount;
            if (CurrentHealth <= 0f)
            {
                Die();
            }
        }

        public void Kill()
        {
            if (IsAlive)
            {
                Die();
            }
        }

        private void ResetState()
        {
            CurrentHealth = maxHealth;
            IsAlive = true;
        }

        private void Die()
        {
            IsAlive = false;
            Died?.Invoke(this);

            Collider[] colliders = GetComponentsInChildren<Collider>();
            foreach (Collider colliderComponent in colliders)
            {
                colliderComponent.enabled = false;
            }

            EnemyMovement movement = GetComponent<EnemyMovement>();
            if (movement != null)
            {
                movement.enabled = false;
            }

            EnemyAttack attack = GetComponent<EnemyAttack>();
            if (attack != null)
            {
                attack.enabled = false;
            }

            EnemyAnimatorDriver animatorDriver = GetComponent<EnemyAnimatorDriver>();
            if (animatorDriver != null)
            {
                animatorDriver.PlayDeath();
            }

            EnemyController controller = GetComponent<EnemyController>();
            if (controller != null)
            {
                controller.enabled = false;
            }

            Destroy(gameObject, destroyDelaySeconds);
        }
    }
}
