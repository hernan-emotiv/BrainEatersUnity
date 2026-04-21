using System;
using BrainEaters.Configs;
using BrainEaters.Core;
using UnityEngine;
using UnityEngine.AI;

namespace BrainEaters.Enemies
{
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        [SerializeField, HideInInspector] private float maxHealth = 1f;
        [SerializeField, HideInInspector] private float destroyDelaySeconds = 1.25f;
        [SerializeField] private EnemyDeathVisual enemyDeathVisual;
        [SerializeField] private Transform visualRoot;
        [SerializeField] private Color damagedFlashColor = new Color(1f, 0.8f, 0.2f, 1f);
        [SerializeField] private float damagedFlashDurationSeconds = 0.12f;
        [SerializeField] private float damagedScaleMultiplier = 1.08f;

        public event Action<float> Damaged;
        public event Action<EnemyHealth> Died;

        public float CurrentHealth { get; private set; }
        public bool IsAlive { get; private set; }
        public EnemyConfig EnemyConfig { get; private set; }

        private Renderer[] cachedRenderers = System.Array.Empty<Renderer>();
        private Color[] baseColors = System.Array.Empty<Color>();
        private Vector3 baseScale = Vector3.one;
        private float remainingDamageFlashTime;

        private void Awake()
        {
            ResolveReferences();
            ResetState();
        }

        private void OnValidate()
        {
            ResolveReferences();
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
            else
            {
                PlayDamageFeedback(amount);
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
            enemyDeathVisual?.ResetState();
            remainingDamageFlashTime = 0f;
            CacheVisualState();
            RestoreVisualState();
        }

        private void Update()
        {
            if (visualRoot == null || remainingDamageFlashTime <= 0f || !IsAlive)
            {
                return;
            }

            remainingDamageFlashTime -= Time.deltaTime;
            float normalized = Mathf.Clamp01(remainingDamageFlashTime / damagedFlashDurationSeconds);

            for (int i = 0; i < cachedRenderers.Length; i++)
            {
                if (cachedRenderers[i] != null)
                {
                    cachedRenderers[i].material.color = Color.Lerp(baseColors[i], damagedFlashColor, normalized);
                }
            }

            visualRoot.localScale = Vector3.Lerp(baseScale, baseScale * damagedScaleMultiplier, normalized);

            if (remainingDamageFlashTime <= 0f)
            {
                RestoreVisualState();
            }
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

            NavMeshAgent navMeshAgent = GetComponent<NavMeshAgent>();
            if (navMeshAgent != null)
            {
                navMeshAgent.enabled = false;
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

            if (enemyDeathVisual != null)
            {
                enemyDeathVisual.PlayDeath();
            }

            EnemyController controller = GetComponent<EnemyController>();
            if (controller != null)
            {
                controller.enabled = false;
            }

            Destroy(gameObject, destroyDelaySeconds);
        }

        private void ResolveReferences()
        {
            if (enemyDeathVisual == null)
            {
                enemyDeathVisual = GetComponent<EnemyDeathVisual>();
            }

            if (visualRoot == null)
            {
                Transform foundVisualRoot = transform.Find("VisualRoot");
                if (foundVisualRoot == null)
                {
                    foundVisualRoot = transform.Find("Visual");
                }

                visualRoot = foundVisualRoot != null ? foundVisualRoot : transform;
            }
        }

        private void PlayDamageFeedback(float amount)
        {
            RestoreVisualState();
            CacheVisualState();
            remainingDamageFlashTime = damagedFlashDurationSeconds;
            Damaged?.Invoke(amount);
        }

        private void CacheVisualState()
        {
            if (visualRoot == null)
            {
                return;
            }

            cachedRenderers = visualRoot.GetComponentsInChildren<Renderer>(true);
            baseColors = new Color[cachedRenderers.Length];
            baseScale = visualRoot.localScale;

            for (int i = 0; i < cachedRenderers.Length; i++)
            {
                if (cachedRenderers[i] != null)
                {
                    baseColors[i] = cachedRenderers[i].material.color;
                }
            }
        }

        private void RestoreVisualState()
        {
            if (visualRoot == null)
            {
                return;
            }

            for (int i = 0; i < cachedRenderers.Length; i++)
            {
                if (cachedRenderers[i] != null)
                {
                    cachedRenderers[i].material.color = baseColors[i];
                }
            }

            visualRoot.localScale = baseScale;
        }
    }
}
