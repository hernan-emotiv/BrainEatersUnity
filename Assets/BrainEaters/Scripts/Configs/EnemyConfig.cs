using UnityEngine;

namespace BrainEaters.Configs
{
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "Brain Eaters/Configs/Enemy Config")]
    public class EnemyConfig : ScriptableObject
    {
        [SerializeField] private float maxHealth = 1f;
        [SerializeField] private float moveSpeed = 3.5f;
        [SerializeField] private float stopDistance = 1.25f;

        public float MaxHealth => maxHealth;
        public float MoveSpeed => moveSpeed;
        public float StopDistance => stopDistance;
    }
}
