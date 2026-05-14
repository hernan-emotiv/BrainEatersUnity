using UnityEngine;
using UnityEngine.InputSystem;

namespace BrainEaters.Input
{
    public class SimulatedMentalCommandSource : MonoBehaviour, ICommandSignalSource
    {
        [SerializeField] private string chargeCommandId = "pull";
        [SerializeField] private string bombCommandId = "push";
        [SerializeField] private Key chargeKey = Key.C;
        [SerializeField] private Key bombKey = Key.M;
        [SerializeField, Range(0f, 1f)] private float simulatedPower = 0.9f;
        [SerializeField, Range(0f, 1f)] private float simulatedConfidence = 1f;

        private bool wasChargeHeld;
        private bool wasBombHeld;

        public bool HasSignal { get; private set; }
        public MentalCommandSignal LatestSignal { get; private set; }
        public string StatusText { get; private set; } = "Simulated MC idle";

        private void Update()
        {
            HasSignal = false;

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                StatusText = "Simulated MC unavailable: no keyboard";
                return;
            }

            bool chargeHeld = IsKeyPressed(keyboard, chargeKey);
            bool bombHeld = IsKeyPressed(keyboard, bombKey);

            if (bombHeld || wasBombHeld)
            {
                Emit(bombCommandId, bombHeld, wasBombHeld);
            }
            else if (chargeHeld || wasChargeHeld)
            {
                Emit(chargeCommandId, chargeHeld, wasChargeHeld);
            }
            else
            {
                StatusText = "Simulated MC idle";
            }

            wasChargeHeld = chargeHeld;
            wasBombHeld = bombHeld;
        }

        private void Emit(string commandId, bool isHeld, bool wasHeld)
        {
            MentalCommandPhase phase = isHeld
                ? wasHeld ? MentalCommandPhase.Held : MentalCommandPhase.Started
                : MentalCommandPhase.Ended;

            float power = isHeld ? simulatedPower : 0f;
            float confidence = isHeld ? simulatedConfidence : 0f;
            LatestSignal = new MentalCommandSignal(commandId, power, confidence, Time.realtimeSinceStartupAsDouble, phase);
            HasSignal = true;
            StatusText = $"Simulated MC {commandId} {phase} power {power:0.00}";
        }

        private static bool IsKeyPressed(Keyboard keyboard, Key key)
        {
            var control = keyboard[key];
            return control != null && control.isPressed;
        }
    }
}
