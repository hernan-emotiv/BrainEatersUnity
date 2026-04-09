using System.Collections.Generic;
using BrainEaters.Configs;
using BrainEaters.Enemies;
using UnityEngine;

namespace BrainEaters.Spawning
{
    public class SpawnManager : MonoBehaviour
    {
        [SerializeField] private Transform playerTarget;
        [SerializeField] private List<SpawnPoint> spawnPoints = new List<SpawnPoint>();

        private readonly List<EnemyController> activeEnemies = new List<EnemyController>();
        private IReadOnlyList<LevelEnemyDefinition> enemyTypes;
        private SpawnConfig spawnConfig;
        private float nextSpawnTime;
        private int nextSpawnIndex;
        private bool spawningEnabled;

        private void Update()
        {
            CleanupDeadEnemies();

            if (!spawningEnabled || spawnConfig == null || playerTarget == null || spawnPoints.Count == 0)
            {
                return;
            }

            if (enemyTypes == null || enemyTypes.Count == 0)
            {
                return;
            }

            if (activeEnemies.Count >= spawnConfig.MaxAliveEnemies || Time.time < nextSpawnTime)
            {
                return;
            }

            SpawnEnemy();
            nextSpawnTime = Time.time + spawnConfig.SpawnIntervalSeconds;
        }

        public void Initialize(LevelConfig levelConfig, Transform target, List<SpawnPoint> points)
        {
            ClearActiveEnemies();
            spawningEnabled = false;
            spawnConfig = null;
            enemyTypes = null;
            playerTarget = target;
            spawnPoints = points ?? new List<SpawnPoint>();

            if (levelConfig == null)
            {
                Debug.LogError("SpawnManager requires a LevelConfig.", this);
                return;
            }

            spawnConfig = levelConfig.SpawnConfig;
            enemyTypes = levelConfig.EnemyTypes;
            RefreshSpawnPointsIfNeeded();
            nextSpawnIndex = 0;
            nextSpawnTime = Time.time + (spawnConfig != null ? spawnConfig.InitialDelaySeconds : 0f);
            spawningEnabled = spawnConfig != null;
        }

        public void SetSpawningEnabled(bool isEnabled)
        {
            spawningEnabled = isEnabled;
        }

        private void SpawnEnemy()
        {
            SpawnPoint spawnPoint = spawnPoints[nextSpawnIndex % spawnPoints.Count];
            nextSpawnIndex++;

            LevelEnemyDefinition definition = GetNextEnemyDefinition();
            if (definition == null)
            {
                return;
            }

            EnemyController enemyInstance = Instantiate(definition.EnemyPrefab, spawnPoint.Position, spawnPoint.Rotation);
            enemyInstance.gameObject.SetActive(true);
            enemyInstance.Initialize(playerTarget, definition.EnemyConfig);
            activeEnemies.Add(enemyInstance);

            EnemyHealth enemyHealth = enemyInstance.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.Died += HandleEnemyDied;
            }
        }

        private void ClearActiveEnemies()
        {
            for (int i = activeEnemies.Count - 1; i >= 0; i--)
            {
                EnemyController enemy = activeEnemies[i];
                if (enemy != null)
                {
                    Destroy(enemy.gameObject);
                }
            }

            activeEnemies.Clear();
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

        private LevelEnemyDefinition GetNextEnemyDefinition()
        {
            if (enemyTypes == null || enemyTypes.Count == 0)
            {
                return null;
            }

            int startIndex = Random.Range(0, enemyTypes.Count);
            for (int i = 0; i < enemyTypes.Count; i++)
            {
                LevelEnemyDefinition definition = enemyTypes[(startIndex + i) % enemyTypes.Count];
                if (definition != null && definition.IsValid)
                {
                    return definition;
                }
            }

            Debug.LogWarning("SpawnManager could not find a valid enemy definition in LevelConfig.", this);
            return null;
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
