using System.Collections.Generic;
using BrainEaters.Core;
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

        private readonly HashSet<IDamageable> processedTargets = new HashSet<IDamageable>();
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

            Collider[] hits = Physics.OverlapSphere(
                attackOrigin.position,
                radius,
                hitMask,
                QueryTriggerInteraction.Collide);

            int hitCount = 0;
            foreach (Collider hit in hits)
            {
                if (hit == null || hit.transform.root == transform.root)
                {
                    continue;
                }

                IDamageable damageable = hit.GetComponentInParent<IDamageable>();
                if (damageable == null || !processedTargets.Add(damageable))
                {
                    continue;
                }

                damageable.ApplyDamage(damage);
                hitCount++;
            }

            nextReadyTime = Time.time + cooldownSeconds;
            if (bombPulseVisual != null)
            {
                bombPulseVisual.Play(radius);
            }

            Debug.Log($"Bomb triggered. Targets hit: {hitCount}.", this);
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
