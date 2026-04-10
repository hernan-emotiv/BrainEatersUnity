using UnityEngine;

namespace BrainEaters.Configs
{
    [CreateAssetMenu(fileName = "EnemyConfig", menuName = "Brain Eaters/Configs/Enemy Config")]
    public class EnemyConfig : ScriptableObject
    {
        [SerializeField] private EnemyType enemyType = EnemyType.Zombie;
        [SerializeField] private string displayName = "Zombie";
        [SerializeField] private float maxHealth = 1f;
        [SerializeField] private float moveSpeed = 3.5f;
        [SerializeField] private float turnSpeed = 540f;
        [SerializeField] private float stopDistance = 1.25f;
        [SerializeField] private float attackRange = 1.6f;
        [SerializeField] private float attackDamage = 1f;
        [SerializeField] private float attackHitDelaySeconds = 0.3f;
        [SerializeField] private float attackDurationSeconds = 0.75f;
        [SerializeField] private float attackCooldownSeconds = 1.1f;
        [SerializeField] private float attackVisualDurationSeconds = 0.18f;
        [SerializeField] private bool useAttackVisual = true;
        [SerializeField] private Vector3 attackHitboxHalfExtents = new Vector3(0.45f, 0.75f, 0.65f);
        [SerializeField] private float destroyDelaySeconds = 1.25f;

        public EnemyType EnemyType => enemyType;
        public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? enemyType.ToString() : displayName;
        public float MaxHealth => maxHealth;
        public float MoveSpeed => moveSpeed;
        public float TurnSpeed => turnSpeed;
        public float StopDistance => stopDistance;
        public float AttackRange => attackRange;
        public float AttackDamage => attackDamage;
        public float AttackHitDelaySeconds => attackHitDelaySeconds;
        public float AttackDurationSeconds => attackDurationSeconds;
        public float AttackCooldownSeconds => attackCooldownSeconds;
        public float AttackVisualDurationSeconds => attackVisualDurationSeconds;
        public bool UseAttackVisual => useAttackVisual;
        public Vector3 AttackHitboxHalfExtents => attackHitboxHalfExtents;
        public float DestroyDelaySeconds => destroyDelaySeconds;
    }
}
