using BrainEaters.Configs;

namespace BrainEaters.GameFlow
{
    public class GameplayKillStat
    {
        public EnemyType EnemyType { get; }
        public string DisplayName { get; }
        public int Count { get; }
        public int ScoreValue { get; }
        public int TotalScore => ScoreValue * Count;

        public GameplayKillStat(EnemyType enemyType, string displayName, int count, int scoreValue)
        {
            EnemyType = enemyType;
            DisplayName = displayName;
            Count = count;
            ScoreValue = scoreValue;
        }
    }
}
