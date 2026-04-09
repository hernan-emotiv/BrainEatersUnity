using BrainEaters.Configs;
using BrainEaters.Player;
using UnityEngine;

namespace BrainEaters.Enemies
{
    public class EnemyAttack : MonoBehaviour
    {
        [SerializeField] private Transform attackOrigin;
        [SerializeField] private float attackRange = 1.6f;
        [SerializeField] private float attackDamage = 10f;
        [SerializeField] private float attackCooldownSeconds = 1.1f;
        [SerializeField] private float attackVisualDurationSeconds = 0.18f;

        private Transform attackVisual;
        private float nextAttackTime;
        private float remainingVisualTime;

        private void Awake()
        {
            if (attackOrigin == null)
            {
                attackOrigin = transform;
            }

            EnsureAttackVisual();
        }

        private void Update()
        {
            if (attackVisual == null || remainingVisualTime <= 0f)
            {
                return;
            }

            remainingVisualTime -= Time.deltaTime;
            if (remainingVisualTime <= 0f)
            {
                attackVisual.gameObject.SetActive(false);
            }
        }

        public void Configure(EnemyConfig enemyConfig)
        {
            if (enemyConfig == null)
            {
                return;
            }

            attackRange = enemyConfig.AttackRange;
            attackDamage = enemyConfig.AttackDamage;
            attackCooldownSeconds = enemyConfig.AttackCooldownSeconds;
            UpdateAttackVisualTransform();
        }

        public bool IsTargetInRange(Vector3 targetPosition)
        {
            Vector3 toTarget = targetPosition - transform.position;
            toTarget.y = 0f;
            return toTarget.sqrMagnitude <= attackRange * attackRange;
        }

        public bool TryAttack(PlayerHealth playerHealth)
        {
            if (playerHealth == null || !playerHealth.IsAlive || Time.time < nextAttackTime)
            {
                return false;
            }

            playerHealth.TakeDamage(attackDamage);
            nextAttackTime = Time.time + attackCooldownSeconds;
            remainingVisualTime = attackVisualDurationSeconds;

            EnsureAttackVisual();
            attackVisual.position = attackOrigin.position + attackOrigin.forward * (attackRange * 0.65f) + Vector3.up * 0.5f;
            attackVisual.rotation = attackOrigin.rotation;
            attackVisual.gameObject.SetActive(true);
            return true;
        }

        private void EnsureAttackVisual()
        {
            if (attackVisual != null)
            {
                return;
            }

            GameObject visual = GameObject.CreatePrimitive(PrimitiveType.Cube);
            visual.name = "AttackVisual";
            visual.transform.SetParent(transform);

            Collider visualCollider = visual.GetComponent<Collider>();
            if (visualCollider != null)
            {
                visualCollider.enabled = false;
            }

            Renderer rendererComponent = visual.GetComponent<Renderer>();
            rendererComponent.material.color = new Color(1f, 0.4f, 0.15f, 1f);
            rendererComponent.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            rendererComponent.receiveShadows = false;

            attackVisual = visual.transform;
            UpdateAttackVisualTransform();
            attackVisual.gameObject.SetActive(false);
        }

        private void UpdateAttackVisualTransform()
        {
            if (attackVisual == null)
            {
                return;
            }

            attackVisual.localPosition = new Vector3(0f, 0.5f, attackRange * 0.65f);
            attackVisual.localRotation = Quaternion.identity;
            attackVisual.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }
    }
}
