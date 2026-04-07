using UnityEngine;

namespace BrainEaters.Player
{
    public class PlayerEnergyCharge : MonoBehaviour
    {
        [SerializeField] private float maxEnergy = 100f;
        [SerializeField] private float chargeRatePerSecond = 30f;
        [SerializeField] private float bombEnergyCost = 50f;

        public float CurrentEnergy { get; private set; }
        public float MaxEnergy => maxEnergy;
        public float BombEnergyCost => bombEnergyCost;
        public float ChargeNormalized => maxEnergy <= 0f ? 0f : CurrentEnergy / maxEnergy;
        public bool CanTriggerBomb => CurrentEnergy >= bombEnergyCost;

        public void Tick(bool isCharging, float deltaTime)
        {
            if (!isCharging || deltaTime <= 0f)
            {
                return;
            }

            AddEnergy(chargeRatePerSecond * deltaTime);
        }

        public void AddEnergy(float amount)
        {
            CurrentEnergy = Mathf.Clamp(CurrentEnergy + amount, 0f, maxEnergy);
        }

        public bool TrySpendBombEnergy()
        {
            return TrySpend(bombEnergyCost);
        }

        public bool TrySpend(float amount)
        {
            if (CurrentEnergy < amount)
            {
                return false;
            }

            CurrentEnergy -= amount;
            return true;
        }
    }
}
