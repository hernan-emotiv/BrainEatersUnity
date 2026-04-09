using BrainEaters.Enemies;
using UnityEngine;

namespace BrainEaters.Configs
{
    [System.Serializable]
    public class LevelEnemyDefinition
    {
        [SerializeField] private EnemyController enemyPrefab;
        [SerializeField] private EnemyConfig enemyConfig;

        public EnemyController EnemyPrefab => enemyPrefab;
        public EnemyConfig EnemyConfig => enemyConfig;
        public bool IsValid => enemyPrefab != null && enemyConfig != null;
    }
}
