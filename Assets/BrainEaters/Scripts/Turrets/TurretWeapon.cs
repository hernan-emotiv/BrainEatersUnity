using UnityEngine;

namespace BrainEaters.Turrets
{
    public class TurretWeapon : MonoBehaviour
    {
        [SerializeField] private Transform muzzle;
        [SerializeField] private TurretProjectile projectilePrefab;
        [SerializeField] private float fireIntervalSeconds = 0.45f;
        [SerializeField] private float projectileSpeed = 18f;
        [SerializeField] private float projectileDamage = 1f;
        [SerializeField] private float muzzleForwardOffset = 0.4f;
        [SerializeField] private LayerMask hitMask = Physics.DefaultRaycastLayers;

        private float nextFireTime;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnValidate()
        {
            ResolveReferences();
        }

        public bool TryFireAt(Vector3 worldPosition)
        {
            if (projectilePrefab == null || muzzle == null || Time.time < nextFireTime)
            {
                return false;
            }

            Vector3 direction = worldPosition - muzzle.position;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                return false;
            }

            Vector3 launchDirection = direction.normalized;
            Vector3 spawnPosition = muzzle.position + launchDirection * muzzleForwardOffset;
            TurretProjectile projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
            projectile.Launch(launchDirection, projectileSpeed, projectileDamage, hitMask, transform.root);
            nextFireTime = Time.time + fireIntervalSeconds;
            return true;
        }

        public void ResetState()
        {
            nextFireTime = 0f;
        }

        private void ResolveReferences()
        {
            if (muzzle == null)
            {
                Transform muzzleTransform = transform.Find("Muzzle");
                if (muzzleTransform != null)
                {
                    muzzle = muzzleTransform;
                }
            }
        }
    }
}
