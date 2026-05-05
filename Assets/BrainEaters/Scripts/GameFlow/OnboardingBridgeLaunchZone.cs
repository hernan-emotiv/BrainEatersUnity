using System.Collections.Generic;
using BrainEaters.Enemies;
using UnityEngine;

namespace BrainEaters.GameFlow
{
    public class OnboardingBridgeLaunchZone : MonoBehaviour
    {
        [SerializeField] private BoxCollider launchArea;
        [SerializeField] private Vector3 launchDirection = new Vector3(0f, 0.45f, 1f);
        [SerializeField] private float horizontalImpulse = 14f;
        [SerializeField] private float upwardImpulse = 8f;
        [SerializeField] private float torqueImpulse = 9f;
        [SerializeField] private float killDelaySeconds = 1.1f;

        private readonly HashSet<EnemyHealth> trackedEnemies = new HashSet<EnemyHealth>();

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnValidate()
        {
            ResolveReferences();
        }

        public int LaunchTrackedEnemies()
        {
            ResolveReferences();
            RefreshTrackedEnemiesFromArea();

            Vector3 direction = transform.TransformDirection(launchDirection).normalized;
            if (direction.sqrMagnitude <= 0.0001f)
            {
                direction = transform.forward;
            }

            int launchedCount = 0;
            List<EnemyHealth> enemiesToLaunch = new List<EnemyHealth>(trackedEnemies);
            for (int i = 0; i < enemiesToLaunch.Count; i++)
            {
                EnemyHealth enemyHealth = enemiesToLaunch[i];
                if (enemyHealth == null || !enemyHealth.IsAlive)
                {
                    continue;
                }

                EnemyPhysicsLaunch launcher = enemyHealth.GetComponent<EnemyPhysicsLaunch>();
                if (launcher == null)
                {
                    launcher = enemyHealth.gameObject.AddComponent<EnemyPhysicsLaunch>();
                }

                Vector3 force = direction * horizontalImpulse + Vector3.up * upwardImpulse;
                Vector3 torque = Random.insideUnitSphere * torqueImpulse;
                launcher.LaunchAndKill(force, torque, killDelaySeconds);
                launchedCount++;
            }

            trackedEnemies.Clear();
            return launchedCount;
        }

        private void OnTriggerEnter(Collider other)
        {
            EnemyHealth enemyHealth = other.GetComponentInParent<EnemyHealth>();
            if (enemyHealth != null && enemyHealth.IsAlive)
            {
                trackedEnemies.Add(enemyHealth);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            EnemyHealth enemyHealth = other.GetComponentInParent<EnemyHealth>();
            if (enemyHealth != null)
            {
                trackedEnemies.Remove(enemyHealth);
            }
        }

        private void ResolveReferences()
        {
            if (launchArea == null)
            {
                launchArea = GetComponent<BoxCollider>();
            }

            if (launchArea != null)
            {
                launchArea.isTrigger = true;
            }
        }

        private void RefreshTrackedEnemiesFromArea()
        {
            if (launchArea == null)
            {
                return;
            }

            Vector3 worldCenter = launchArea.transform.TransformPoint(launchArea.center);
            Vector3 halfExtents = Vector3.Scale(launchArea.size, launchArea.transform.lossyScale) * 0.5f;
            Collider[] overlaps = Physics.OverlapBox(worldCenter, halfExtents, launchArea.transform.rotation, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide);

            for (int i = 0; i < overlaps.Length; i++)
            {
                EnemyHealth enemyHealth = overlaps[i] != null ? overlaps[i].GetComponentInParent<EnemyHealth>() : null;
                if (enemyHealth != null && enemyHealth.IsAlive)
                {
                    trackedEnemies.Add(enemyHealth);
                }
            }
        }
    }
}
