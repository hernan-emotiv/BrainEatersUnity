using System.Collections.Generic;

namespace BrainEaters.GameFlow
{
    public class GameplayReport
    {
        public GameplayState ResultState { get; }
        public int TotalEnemiesEliminated { get; }
        public float DamageReceived { get; }
        public float ElapsedSeconds { get; }
        public float TargetDurationSeconds { get; }
        public IReadOnlyList<GameplayKillStat> KillStats { get; }

        public GameplayReport(
            GameplayState resultState,
            int totalEnemiesEliminated,
            float damageReceived,
            float elapsedSeconds,
            float targetDurationSeconds,
            IReadOnlyList<GameplayKillStat> killStats)
        {
            ResultState = resultState;
            TotalEnemiesEliminated = totalEnemiesEliminated;
            DamageReceived = damageReceived;
            ElapsedSeconds = elapsedSeconds;
            TargetDurationSeconds = targetDurationSeconds;
            KillStats = killStats ?? new List<GameplayKillStat>();
        }
    }
}
