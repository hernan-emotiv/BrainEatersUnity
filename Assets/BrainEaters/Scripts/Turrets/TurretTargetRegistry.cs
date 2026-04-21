using System.Collections.Generic;
using UnityEngine;

namespace BrainEaters.Turrets
{
    public static class TurretTargetRegistry
    {
        private static readonly List<TurretHealth> ActiveTurrets = new List<TurretHealth>();

        public static void Register(TurretHealth turretHealth)
        {
            if (turretHealth == null || ActiveTurrets.Contains(turretHealth))
            {
                return;
            }

            ActiveTurrets.Add(turretHealth);
        }

        public static void Unregister(TurretHealth turretHealth)
        {
            if (turretHealth == null)
            {
                return;
            }

            ActiveTurrets.Remove(turretHealth);
        }

        public static TurretHealth GetNearestTarget(Vector3 origin)
        {
            TurretHealth bestTarget = null;
            float bestDistanceSqr = float.MaxValue;

            for (int i = ActiveTurrets.Count - 1; i >= 0; i--)
            {
                TurretHealth turretHealth = ActiveTurrets[i];
                if (turretHealth == null || !turretHealth.IsTargetable)
                {
                    ActiveTurrets.RemoveAt(i);
                    continue;
                }

                float distanceSqr = (turretHealth.TargetPoint.position - origin).sqrMagnitude;
                if (distanceSqr < bestDistanceSqr)
                {
                    bestDistanceSqr = distanceSqr;
                    bestTarget = turretHealth;
                }
            }

            return bestTarget;
        }
    }
}
