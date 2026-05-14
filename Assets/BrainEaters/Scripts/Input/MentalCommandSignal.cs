namespace BrainEaters.Input
{
    public readonly struct MentalCommandSignal
    {
        public MentalCommandSignal(string commandId, float power, float confidence, double timestampSeconds, MentalCommandPhase phase)
        {
            CommandId = commandId;
            Power = power;
            Confidence = confidence;
            TimestampSeconds = timestampSeconds;
            Phase = phase;
        }

        public string CommandId { get; }
        public float Power { get; }
        public float Confidence { get; }
        public double TimestampSeconds { get; }
        public MentalCommandPhase Phase { get; }
    }
}
