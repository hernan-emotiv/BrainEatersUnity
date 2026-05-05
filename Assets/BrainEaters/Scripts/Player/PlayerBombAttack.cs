using System.Collections.Generic;
using BrainEaters.Core;
using BrainEaters.Enemies;
using UnityEngine;

namespace BrainEaters.Player
{
    public class PlayerBombAttack : MonoBehaviour
    {
        [SerializeField] private Transform attackOrigin;
        [SerializeField] private BombPulseVisual bombPulseVisual;
        [SerializeField] private float radius = 6f;
        [SerializeField] private float damage = 999f;
        [SerializeField] private float cooldownSeconds = 0.75f;
        [SerializeField] private LayerMask hitMask = ~0;
        [SerializeField] private bool launchEnemiesOnHit = true;
        [SerializeField] private float enemyLaunchForce = 10f;
        [SerializeField] private float enemyLaunchUpwardForce = 4.5f;
        [SerializeField] private float enemyLaunchTorque = 7f;
        [SerializeField] private float enemyLaunchKillDelaySeconds = 0.75f;

        private readonly HashSet<IDamageable> processedTargets = new HashSet<IDamageable>();
        private readonly HashSet<IBombActivatable> processedActivatables = new HashSet<IBombActivatable>();
        private float nextReadyTime;

        private void Awake()
        {
            if (attackOrigin == null)
            {
                attackOrigin = transform;
            }

            if (bombPulseVisual == null)
            {
                bombPulseVisual = GetComponent<BombPulseVisual>();
            }
        }

        public bool TryTrigger(PlayerEnergyCharge energyCharge)
        {
            if (energyCharge == null)
            {
                return false;
            }

            if (Time.time < nextReadyTime)
            {
                return false;
            }

            if (!energyCharge.TrySpendBombEnergy())
            {
                Debug.Log("Bomb not ready: not enough energy.", this);
                return false;
            }

            processedTargets.Clear();
            processedActivatables.Clear();

            Collider[] hits = Physics.OverlapSphere(
                attackOrigin.position,
                radius,
                hitMask,
                QueryTriggerInteraction.Collide);

            int hitCount = 0;
            int activationCount = 0;
            foreach (Collider hit in hits)
            {
                if (hit == null || hit.transform.root == transform.root)
                {
                    continue;
                }

                IBombActivatable activatable = hit.GetComponentInParent<IBombActivatable>();
                if (activatable != null && activatable.CanActivateBomb && processedActivatables.Add(activatable))
                {
                    activatable.ActivateBomb();
                    activationCount++;
                }

                IDamageable damageable = hit.GetComponentInParent<IDamageable>();
                if (damageable == null || !processedTargets.Add(damageable))
                {
                    continue;
                }

                if (!TryLaunchEnemy(hit))
                {
                    damageable.ApplyDamage(damage);
                }

                hitCount++;
            }

            nextReadyTime = Time.time + cooldownSeconds;
            if (bombPulseVisual != null)
            {
                bombPulseVisual.Play(radius);
            }

            Debug.Log($"Bomb triggered. Targets hit: {hitCount}. Activatables triggered: {activationCount}.", this);
            return true;
        }

        private bool TryLaunchEnemy(Collider hit)
        {
            if (!launchEnemiesOnHit || hit == null)
            {
                return false;
            }

            EnemyPhysicsLaunch launcher = hit.GetComponentInParent<EnemyPhysicsLaunch>();
            EnemyHealth enemyHealth = hit.GetComponentInParent<EnemyHealth>();
            if (enemyHealth == null)
            {
                return false;
            }

            if (launcher == null)
            {
                launcher = enemyHealth.gameObject.AddComponent<EnemyPhysicsLaunch>();
            }

            Vector3 launchDirection = enemyHealth.transform.position - attackOrigin.position;
            launchDirection.y = 0f;
            if (launchDirection.sqrMagnitude < 0.0001f)
            {
                launchDirection = attackOrigin.forward;
            }

            launchDirection.Normalize();
            Vector3 force = launchDirection * enemyLaunchForce + Vector3.up * enemyLaunchUpwardForce;
            Vector3 torque = Random.onUnitSphere * enemyLaunchTorque;
            launcher.LaunchAndKill(force, torque, enemyLaunchKillDelaySeconds);
            return true;
        }

        private void OnDrawGizmosSelected()
        {
            Transform origin = attackOrigin != null ? attackOrigin : transform;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(origin.position, radius);
        }
    }
}
