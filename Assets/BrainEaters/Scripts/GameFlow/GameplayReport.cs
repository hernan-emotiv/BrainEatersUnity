using System.Collections.Generic;

namespace BrainEaters.GameFlow
{
    public class GameplayReport
    {
        public GameplayState ResultState { get; }
        public int TotalEnemiesEliminated { get; }
        public int TotalScore { get; }
        public float DamageReceived { get; }
        public float ElapsedSeconds { get; }
        public float TargetDurationSeconds { get; }
        public IReadOnlyList<GameplayKillStat> KillStats { get; }

        public GameplayReport(
            GameplayState resultState,
            int totalEnemiesEliminated,
            int totalScore,
            float damageReceived,
            float elapsedSeconds,
            float targetDurationSeconds,
            IReadOnlyList<GameplayKillStat> killStats)
        {
            ResultState = resultState;
            TotalEnemiesEliminated = totalEnemiesEliminated;
            TotalScore = totalScore;
            DamageReceived = damageReceived;
            ElapsedSeconds = elapsedSeconds;
            TargetDurationSeconds = targetDurationSeconds;
            KillStats = killStats ?? new List<GameplayKillStat>();
        }
    }
}
