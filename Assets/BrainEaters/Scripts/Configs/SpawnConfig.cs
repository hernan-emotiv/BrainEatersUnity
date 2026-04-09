using UnityEngine;

namespace BrainEaters.Configs
{
    [CreateAssetMenu(fileName = "SpawnConfig", menuName = "Brain Eaters/Configs/Spawn Config")]
    public class SpawnConfig : ScriptableObject
    {
        [SerializeField] private float initialDelaySeconds = 1f;
        [SerializeField] private float spawnIntervalSeconds = 2f;
        [SerializeField] private int maxAliveEnemies = 15;

        public float InitialDelaySeconds => initialDelaySeconds;
        public float SpawnIntervalSeconds => spawnIntervalSeconds;
        public int MaxAliveEnemies => maxAliveEnemies;
    }
}
