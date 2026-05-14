using UnityEngine;

namespace BrainEaters.Input
{
    public class MentalCommandGameplayInputSource : MonoBehaviour, IGameplayInputSource
    {
        [SerializeField] private MonoBehaviour fallbackInputSource;
        [SerializeField] private MonoBehaviour commandSignalSource;
        [SerializeField] private string chargeCommandId = "pull";
        [SerializeField] private string bombCommandId = "push";
        [SerializeField, Range(0f, 1f)] private float minimumPower = 0.5f;
        [SerializeField, Range(0f, 1f)] private float minimumConfidence = 0.5f;
        [SerializeField] private bool chargeWhileCommandHeld = true;
        [SerializeField] private bool triggerBombOnCommandStart = true;

        private IGameplayInputSource fallback;
        private ICommandSignalSource commandSource;
        private bool mentalChargeHeld;
        private bool mentalBombPressed;
        private string lastRejectedReason = "No signal";

        public Vector2 Move => fallback?.Move ?? Vector2.zero;
        public Vector2 Look => fallback?.Look ?? Vector2.zero;
        public bool UsesFacingRelativeMovement => fallback != null && fallback.UsesFacingRelativeMovement;
        public bool UsesDeltaLookInput => fallback != null && fallback.UsesDeltaLookInput;
        public bool IsChargeHeld => (fallback != null && fallback.IsChargeHeld) || mentalChargeHeld;
        public bool WasBombPressedThisFrame => (fallback != null && fallback.WasBombPressedThisFrame) || mentalBombPressed;
        public string DebugStatus => commandSource != null ? commandSource.StatusText : "No command source";
        public string LastRejectedReason => lastRejectedReason;

        private void Awake()
        {
            ResolveReferences();
        }

        private void OnValidate()
        {
            ResolveReferences();
        }

        private void Update()
        {
            ResolveReferences();
            mentalBombPressed = false;
            mentalChargeHeld = false;

            if (commandSource == null || !commandSource.HasSignal)
            {
                lastRejectedReason = commandSource == null ? "No command source" : "No signal";
                return;
            }

            MentalCommandSignal signal = commandSource.LatestSignal;
            if (!PassesThreshold(signal))
            {
                lastRejectedReason = $"Rejected {signal.CommandId}: power {signal.Power:0.00}, confidence {signal.Confidence:0.00}";
                return;
            }

            if (chargeWhileCommandHeld && Matches(signal, chargeCommandId))
            {
                mentalChargeHeld = signal.Phase == MentalCommandPhase.Started || signal.Phase == MentalCommandPhase.Held;
            }

            if (triggerBombOnCommandStart && Matches(signal, bombCommandId))
            {
                mentalBombPressed = signal.Phase == MentalCommandPhase.Started;
            }

            lastRejectedReason = "Accepted";
        }

        private void ResolveReferences()
        {
            fallback = fallbackInputSource as IGameplayInputSource;
            commandSource = commandSignalSource as ICommandSignalSource;
        }

        private bool PassesThreshold(MentalCommandSignal signal)
        {
            return signal.Power >= minimumPower && signal.Confidence >= minimumConfidence;
        }

        private static bool Matches(MentalCommandSignal signal, string commandId)
        {
            return !string.IsNullOrWhiteSpace(commandId)
                && string.Equals(signal.CommandId, commandId, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
