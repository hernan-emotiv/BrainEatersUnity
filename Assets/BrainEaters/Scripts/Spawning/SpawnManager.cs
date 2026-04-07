using System.Collections.Generic;
using BrainEaters.Enemies;
using UnityEngine;

namespace BrainEaters.Spawning
{
    public class SpawnManager : MonoBehaviour
    {
        [SerializeField] private EnemyController enemyPrefab;
        [SerializeField] private Transform playerTarget;
        [SerializeField] private List<SpawnPoint> spawnPoints = new List<SpawnPoint>();
        [SerializeField] private float initialDelaySeconds = 1f;
        [SerializeField] private float spawnIntervalSeconds = 2f;
        [SerializeField] private int maxAliveEnemies = 15;

        private readonly List<EnemyController> activeEnemies = new List<EnemyController>();
        private float nextSpawnTime;
        private int nextSpawnIndex;

        private void Start()
        {
            RefreshSpawnPointsIfNeeded();
            nextSpawnTime = Time.time + initialDelaySeconds;
        }

        private void Update()
        {
            CleanupDeadEnemies();

            if (enemyPrefab == null || playerTarget == null || spawnPoints.Count == 0)
            {
                return;
            }

            if (activeEnemies.Count >= maxAliveEnemies || Time.time < nextSpawnTime)
            {
                return;
            }

            SpawnEnemy();
            nextSpawnTime = Time.time + spawnIntervalSeconds;
        }

        public void SetEnemyPrefab(EnemyController prefab)
        {
            enemyPrefab = prefab;
        }

        public void SetPlayerTarget(Transform target)
        {
            playerTarget = target;
        }

        public void SetSpawnPoints(List<SpawnPoint> points)
        {
            spawnPoints = points;
        }

        private void SpawnEnemy()
        {
            SpawnPoint spawnPoint = spawnPoints[nextSpawnIndex % spawnPoints.Count];
            nextSpawnIndex++;

            EnemyController enemyInstance = Instantiate(enemyPrefab, spawnPoint.Position, spawnPoint.Rotation);
            enemyInstance.gameObject.SetActive(true);
            enemyInstance.Initialize(playerTarget);
            activeEnemies.Add(enemyInstance);

            EnemyHealth enemyHealth = enemyInstance.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.Died += HandleEnemyDied;
            }
        }

        private void HandleEnemyDied(EnemyHealth enemyHealth)
        {
            if (enemyHealth == null)
            {
                return;
            }

            enemyHealth.Died -= HandleEnemyDied;
            EnemyController enemyController = enemyHealth.GetComponent<EnemyController>();
            if (enemyController != null)
            {
                activeEnemies.Remove(enemyController);
            }
        }

        private void CleanupDeadEnemies()
        {
            for (int i = activeEnemies.Count - 1; i >= 0; i--)
            {
                if (activeEnemies[i] == null)
                {
                    activeEnemies.RemoveAt(i);
                }
            }
        }

        private void RefreshSpawnPointsIfNeeded()
        {
            if (spawnPoints.Count > 0)
            {
                return;
            }

            SpawnPoint[] discoveredPoints = FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);
            spawnPoints.AddRange(discoveredPoints);
        }
    }
}
