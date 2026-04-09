using System;
using UnityEngine;

namespace BrainEaters.Player
{
    public class PlayerHealth : MonoBehaviour
    {
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private bool disableControlsOnDeath = true;
        [SerializeField] private PlayerController playerController;

        public event Action<PlayerHealth> HealthChanged;
        public event Action<float> Damaged;

        public float CurrentHealth { get; private set; }
        public float MaxHealth => maxHealth;
        public float HealthNormalized => maxHealth <= 0f ? 0f : CurrentHealth / maxHealth;
        public bool IsAlive { get; private set; }

        private void Awake()
        {
            ResolveReferences();
            ResetState();
        }

        private void OnValidate()
        {
            ResolveReferences();
        }

        public void ResetState()
        {
            CurrentHealth = maxHealth;
            IsAlive = true;
            HealthChanged?.Invoke(this);
        }

        public void TakeDamage(float amount)
        {
            if (!IsAlive || amount <= 0f)
            {
                return;
            }

            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            Damaged?.Invoke(amount);
            HealthChanged?.Invoke(this);

            if (CurrentHealth <= 0f)
            {
                HandleDeath();
            }
        }

        private void HandleDeath()
        {
            IsAlive = false;

            if (disableControlsOnDeath && playerController != null)
            {
                playerController.enabled = false;
            }

            Debug.Log("Player died.", this);
        }

        private void ResolveReferences()
        {
            if (playerController == null)
            {
                playerController = GetComponent<PlayerController>();
            }
        }
    }
}
