using UnityEngine;

namespace BrainEaters.Configs
{
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "Brain Eaters/Configs/Enemy Config")]
    public class EnemyConfig : ScriptableObject
    {
        [SerializeField] private float maxHealth = 1f;
        [SerializeField] private float moveSpeed = 3.5f;
        [SerializeField] private float stopDistance = 1.25f;
        [SerializeField] private float attackRange = 1.6f;
        [SerializeField] private float attackDamage = 1f;
        [SerializeField] private float attackCooldownSeconds = 1.1f;

        public float MaxHealth => maxHealth;
        public float MoveSpeed => moveSpeed;
        public float StopDistance => stopDistance;
        public float AttackRange => attackRange;
        public float AttackDamage => attackDamage;
        public float AttackCooldownSeconds => attackCooldownSeconds;
    }
}
