using BrainEaters.GameFlow;
using BrainEaters.Player;
using TMPro;
using UnityEngine;

namespace BrainEaters.UI
{
    public class GameplayHudController : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private PlayerHealth playerHealth;
        [SerializeField] private PlayerEnergyCharge playerEnergyCharge;
        [SerializeField] private HeartHealthView heartHealthView;
        [SerializeField] private ProgressBarView bombProgressBar;
        [SerializeField] private TMP_Text timerText;

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
            if (playerHealth != null)
            {
                if (heartHealthView != null)
                {
                    heartHealthView.SetHealth(Mathf.CeilToInt(playerHealth.CurrentHealth), Mathf.CeilToInt(playerHealth.MaxHealth));
                }
            }

            if (playerEnergyCharge != null && bombProgressBar != null)
            {
                bombProgressBar.SetNormalizedValue(playerEnergyCharge.ChargeNormalized);
                bombProgressBar.SetValueText($"{Mathf.CeilToInt(playerEnergyCharge.CurrentEnergy)}/{Mathf.CeilToInt(playerEnergyCharge.MaxEnergy)}");
                bombProgressBar.SetStatusText(playerEnergyCharge.CanTriggerBomb ? "BOMB READY" : "CHARGING", playerEnergyCharge.CanTriggerBomb ? Color.cyan : Color.white);
            }

            if (gameManager != null && timerText != null)
            {
                timerText.text = FormatTime(gameManager.RemainingSurvivalTime);
            }
        }

        public void SetTargets(GameManager manager, PlayerHealth health, PlayerEnergyCharge energyCharge)
        {
            gameManager = manager;
            playerHealth = health;
            playerEnergyCharge = energyCharge;
        }

        private void ResolveReferences()
        {
            if (gameManager == null)
            {
                gameManager = FindFirstObjectByType<GameManager>();
            }

            if (playerHealth == null)
            {
                playerHealth = FindFirstObjectByType<PlayerHealth>();
            }

            if (playerEnergyCharge == null)
            {
                playerEnergyCharge = FindFirstObjectByType<PlayerEnergyCharge>();
            }
        }

        private static string FormatTime(float timeSeconds)
        {
            int totalSeconds = Mathf.Max(0, Mathf.CeilToInt(timeSeconds));
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            return $"{minutes:00}:{seconds:00}";
        }
    }
}
