using System.Collections.Generic;
using BrainEaters.GameFlow;
using UnityEngine;

namespace BrainEaters.Configs
{
    [CreateAssetMenu(fileName = "LevelConfig", menuName = "Brain Eaters/Configs/Level Config")]
    public class LevelConfig : ScriptableObject
    {
        [SerializeField] private GameModeType gameModeType = GameModeType.Survival;
        [SerializeField] private float survivalDurationSeconds = 60f;
        [SerializeField] private LevelContext levelPrefab;
        [SerializeField] private SpawnConfig spawnConfig;
        [SerializeField] private List<LevelEnemyDefinition> enemyTypes = new List<LevelEnemyDefinition>();

        public GameModeType GameModeType => gameModeType;
        public float SurvivalDurationSeconds => survivalDurationSeconds;
        public LevelContext LevelPrefab => levelPrefab;
        public SpawnConfig SpawnConfig => spawnConfig;
        public IReadOnlyList<LevelEnemyDefinition> EnemyTypes => enemyTypes;
    }
}
