using System;
using BrainEaters.Core;
using UnityEngine;

namespace BrainEaters.Turrets
{
    public class TurretHealth : MonoBehaviour, IDamageable
    {
        [SerializeField] private bool targetableWhenOnline = true;
        [SerializeField] private float maxHealth = 6f;
        [SerializeField] private Transform targetPoint;
        [SerializeField] private float debugCurrentHealth;
        [SerializeField] private bool debugIsTargetable;

        public event Action<TurretHealth> Destroyed;
        public event Action<float> Damaged;

        public float CurrentHealth { get; private set; }
        public bool IsDestroyed { get; private set; }
        public bool IsOnline { get; private set; }
        public bool IsTargetable => targetableWhenOnline && IsOnline && !IsDestroyed;
        public Transform TargetPoint => targetPoint != null ? targetPoint : transform;

        private void Awake()
        {
            ResolveReferences();
            ResetState();
        }

        private void OnEnable()
        {
            UpdateRegistry();
        }

        private void OnDisable()
        {
            TurretTargetRegistry.Unregister(this);
        }

        private void OnValidate()
        {
            ResolveReferences();
        }

        public void ApplyDamage(float amount)
        {
            if (!IsTargetable || amount <= 0f)
            {
                return;
            }

            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            SyncDebugState();
            Damaged?.Invoke(amount);
            if (CurrentHealth <= 0f)
            {
                IsDestroyed = true;
                SyncDebugState();
                UpdateRegistry();
                Destroyed?.Invoke(this);
            }
        }

        public void ResetState()
        {
            CurrentHealth = maxHealth;
            IsDestroyed = false;
            IsOnline = false;
            SyncDebugState();
            UpdateRegistry();
        }

        public void SetOnlineState(bool online, bool resetWhenOffline = true)
        {
            IsOnline = online;
            if (!online && resetWhenOffline)
            {
                CurrentHealth = maxHealth;
                IsDestroyed = false;
            }

            SyncDebugState();
            UpdateRegistry();
        }

        private void ResolveReferences()
        {
            if (targetPoint == null)
            {
                Transform explicitTargetPoint = transform.Find("TargetPoint");
                if (explicitTargetPoint != null)
                {
                    targetPoint = explicitTargetPoint;
                }
            }
        }

        private void SyncDebugState()
        {
            debugCurrentHealth = CurrentHealth;
            debugIsTargetable = IsTargetable;
        }

        private void UpdateRegistry()
        {
            if (IsTargetable)
            {
                TurretTargetRegistry.Register(this);
            }
            else
            {
                TurretTargetRegistry.Unregister(this);
            }
        }
    }
}
