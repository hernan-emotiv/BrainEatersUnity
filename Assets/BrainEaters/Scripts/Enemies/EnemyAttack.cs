using BrainEaters.Configs;
using BrainEaters.Core;
using System.Collections.Generic;
using UnityEngine;

namespace BrainEaters.Enemies
{
    public class EnemyAttack : MonoBehaviour
    {
        [SerializeField] private Transform attackOrigin;
        [SerializeField, HideInInspector] private float attackRange = 1.6f;
        [SerializeField, HideInInspector] private float attackDamage = 10f;
        [SerializeField, HideInInspector] private float attackHitDelaySeconds = 0.3f;
        [SerializeField, HideInInspector] private float attackDurationSeconds = 0.75f;
        [SerializeField, HideInInspector] private float attackCooldownSeconds = 1.1f;
        [SerializeField, HideInInspector] private float attackVisualDurationSeconds = 0.18f;
        [SerializeField, HideInInspector] private bool useAttackVisual = true;
        [SerializeField, HideInInspector] private Vector3 hitboxHalfExtents = new Vector3(0.45f, 0.75f, 0.65f);
        [SerializeField] private LayerMask hitMask = Physics.DefaultRaycastLayers;

        private Transform attackVisual;
        private float nextAttackTime;
        private float remainingVisualTime;
        private float attackElapsedTime;
        private bool hasAppliedDamageThisAttack;

        public bool IsAttacking { get; private set; }

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
            UpdateAttackState();

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
            attackHitDelaySeconds = enemyConfig.AttackHitDelaySeconds;
            attackDurationSeconds = enemyConfig.AttackDurationSeconds;
            attackCooldownSeconds = enemyConfig.AttackCooldownSeconds;
            attackVisualDurationSeconds = enemyConfig.AttackVisualDurationSeconds;
            useAttackVisual = enemyConfig.UseAttackVisual;
            hitboxHalfExtents = enemyConfig.AttackHitboxHalfExtents;
            UpdateAttackVisualTransform();
        }

        public bool IsTargetInRange(Vector3 targetPosition)
        {
            Vector3 toTarget = targetPosition - transform.position;
            toTarget.y = 0f;
            return toTarget.sqrMagnitude <= attackRange * attackRange;
        }

        public bool TryAttack()
        {
            if (IsAttacking || Time.time < nextAttackTime)
            {
                return false;
            }

            nextAttackTime = Time.time + attackCooldownSeconds;
            attackElapsedTime = 0f;
            hasAppliedDamageThisAttack = false;
            IsAttacking = true;
            return true;
        }

        private void UpdateAttackState()
        {
            if (!IsAttacking)
            {
                return;
            }

            attackElapsedTime += Time.deltaTime;
            if (!hasAppliedDamageThisAttack && attackElapsedTime >= attackHitDelaySeconds)
            {
                ApplyAttackDamage();
                hasAppliedDamageThisAttack = true;
                remainingVisualTime = attackVisualDurationSeconds;
            }

            if (attackElapsedTime >= attackDurationSeconds)
            {
                IsAttacking = false;
            }
        }

        private void ApplyAttackDamage()
        {
            if (useAttackVisual)
            {
                EnsureAttackVisual();
                attackVisual.position = attackOrigin.position + attackOrigin.forward * (attackRange * 0.65f) + Vector3.up * 0.5f;
                attackVisual.rotation = attackOrigin.rotation;
                attackVisual.gameObject.SetActive(true);
            }

            Vector3 hitCenter = attackOrigin.position + attackOrigin.forward * hitboxHalfExtents.z;
            Collider[] hitColliders = Physics.OverlapBox(hitCenter, hitboxHalfExtents, attackOrigin.rotation, hitMask, QueryTriggerInteraction.Ignore);
            HashSet<IDamageable> damagedTargets = new HashSet<IDamageable>();

            for (int i = 0; i < hitColliders.Length; i++)
            {
                Collider hitCollider = hitColliders[i];
                if (hitCollider == null || hitCollider.transform.root == transform.root)
                {
                    continue;
                }

                IDamageable damageable = hitCollider.GetComponentInParent<IDamageable>();
                if (damageable == null)
                {
                    continue;
                }

                if (!damagedTargets.Add(damageable))
                {
                    continue;
                }

                damageable.ApplyDamage(attackDamage);
            }
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

        private void OnDrawGizmosSelected()
        {
            Transform origin = attackOrigin != null ? attackOrigin : transform;
            Gizmos.color = new Color(1f, 0.4f, 0.15f, 0.35f);
            Matrix4x4 previousMatrix = Gizmos.matrix;
            Gizmos.matrix = Matrix4x4.TRS(origin.position + origin.forward * hitboxHalfExtents.z, origin.rotation, Vector3.one);
            Gizmos.DrawCube(Vector3.zero, hitboxHalfExtents * 2f);
            Gizmos.matrix = previousMatrix;
        }
    }
}
