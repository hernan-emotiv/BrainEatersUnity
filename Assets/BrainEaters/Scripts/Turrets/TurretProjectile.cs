using BrainEaters.Core;
using System.Collections.Generic;
using UnityEngine;

namespace BrainEaters.Turrets
{
    public class TurretProjectile : MonoBehaviour
    {
        [SerializeField] private float speed = 18f;
        [SerializeField] private float damage = 1f;
        [SerializeField] private float maxLifetimeSeconds = 4f;
        [SerializeField] private float launchSpeedScale = 0.45f;
        [SerializeField] private float visibleScale = 0.38f;
        [SerializeField] private float hitEffectScale = 0.65f;
        [SerializeField] private float hitEffectDurationSeconds = 0.12f;
        [SerializeField] private LayerMask hitMask = Physics.DefaultRaycastLayers;
        [SerializeField] private Rigidbody projectileRigidbody;
        [SerializeField] private Collider projectileCollider;
        [SerializeField] private MeshRenderer projectileRenderer;

        private Vector3 direction = Vector3.forward;
        private float lifetime;
        private readonly List<Collider> ignoredColliders = new List<Collider>();

        private void Awake()
        {
            ResolveReferences();
        }

        private void Update()
        {
            lifetime += Time.deltaTime;
            if (lifetime >= maxLifetimeSeconds)
            {
                Destroy(gameObject);
                return;
            }

            if (projectileRigidbody == null)
            {
                transform.position += direction * (speed * Time.deltaTime);
            }
        }

        public void Launch(Vector3 launchDirection, float projectileSpeed, float projectileDamage, LayerMask projectileHitMask, Transform ownerRoot)
        {
            ResolveReferences();
            direction = launchDirection.sqrMagnitude > 0.0001f ? launchDirection.normalized : transform.forward;
            speed = projectileSpeed * launchSpeedScale;
            damage = projectileDamage;
            hitMask = projectileHitMask;
            lifetime = 0f;
            transform.localScale = Vector3.one * visibleScale;

            transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            IgnoreOwnerCollisions(ownerRoot);

            if (projectileRigidbody != null)
            {
                projectileRigidbody.linearVelocity = direction * speed;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (((1 << other.gameObject.layer) & hitMask.value) == 0)
            {
                return;
            }

            IDamageable damageable = other.GetComponentInParent<IDamageable>();
            if (damageable != null)
            {
                damageable.ApplyDamage(damage);
            }

            SpawnHitEffect(other);
            Destroy(gameObject);
        }

        private void ResolveReferences()
        {
            if (projectileRigidbody == null)
            {
                projectileRigidbody = GetComponent<Rigidbody>();
            }

            if (projectileCollider == null)
            {
                projectileCollider = GetComponent<Collider>();
            }

            if (projectileRenderer == null)
            {
                projectileRenderer = GetComponent<MeshRenderer>();
            }
        }

        private void IgnoreOwnerCollisions(Transform ownerRoot)
        {
            ignoredColliders.Clear();

            if (ownerRoot == null || projectileCollider == null)
            {
                return;
            }

            Collider[] ownerColliders = ownerRoot.GetComponentsInChildren<Collider>(true);
            for (int i = 0; i < ownerColliders.Length; i++)
            {
                Collider ownerCollider = ownerColliders[i];
                if (ownerCollider == null)
                {
                    continue;
                }

                Physics.IgnoreCollision(projectileCollider, ownerCollider, true);
                ignoredColliders.Add(ownerCollider);
            }
        }

        private void SpawnHitEffect(Collider hitCollider)
        {
            Vector3 effectPosition = hitCollider != null
                ? hitCollider.ClosestPoint(transform.position)
                : transform.position;

            GameObject hitEffect = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            hitEffect.name = "TurretHitEffect";
            hitEffect.transform.position = effectPosition;
            hitEffect.transform.rotation = Quaternion.identity;
            hitEffect.transform.localScale = Vector3.one * hitEffectScale;

            Collider effectCollider = hitEffect.GetComponent<Collider>();
            if (effectCollider != null)
            {
                Destroy(effectCollider);
            }

            MeshRenderer hitRenderer = hitEffect.GetComponent<MeshRenderer>();
            if (hitRenderer != null && projectileRenderer != null)
            {
                hitRenderer.sharedMaterial = projectileRenderer.sharedMaterial;
            }

            Destroy(hitEffect, hitEffectDurationSeconds);
        }

        private void OnDestroy()
        {
            if (projectileCollider == null)
            {
                return;
            }

            for (int i = 0; i < ignoredColliders.Count; i++)
            {
                if (ignoredColliders[i] != null)
                {
                    Physics.IgnoreCollision(projectileCollider, ignoredColliders[i], false);
                }
            }
        }
    }
}
