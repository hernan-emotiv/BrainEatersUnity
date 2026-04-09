using BrainEaters.Configs;

namespace BrainEaters.GameFlow
{
    public class GameplayKillStat
    {
        public EnemyType EnemyType { get; }
        public string DisplayName { get; }
        public int Count { get; }

        public GameplayKillStat(EnemyType enemyType, string displayName, int count)
        {
            EnemyType = enemyType;
            DisplayName = displayName;
            Count = count;
        }
    }
}
