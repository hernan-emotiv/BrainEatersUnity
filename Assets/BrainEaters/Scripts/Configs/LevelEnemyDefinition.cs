using BrainEaters.Enemies;
using UnityEngine;

namespace BrainEaters.Configs
{
    [System.Serializable]
    public class LevelEnemyDefinition
    {
        [SerializeField] private EnemyController enemyPrefab;
        [SerializeField] private EnemyConfig enemyConfig;
        [SerializeField, Min(0)] private int spawnWeight = 1;

        public EnemyController EnemyPrefab => enemyPrefab;
        public EnemyConfig EnemyConfig => enemyConfig;
        public int SpawnWeight => spawnWeight > 0 ? spawnWeight : 1;
        public bool IsValid => enemyPrefab != null && enemyConfig != null;
    }
}
