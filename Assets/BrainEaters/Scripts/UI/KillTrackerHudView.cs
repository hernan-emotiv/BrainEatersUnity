using System.Collections.Generic;
using BrainEaters.Configs;
using BrainEaters.GameFlow;
using TMPro;
using UnityEngine;

namespace BrainEaters.UI
{
    public class KillTrackerHudView : MonoBehaviour
    {
        [System.Serializable]
        private class KillCounterBinding
        {
            public EnemyType enemyType;
            public TMP_Text countText;
        }

        [SerializeField] private GameManager gameManager;
        [SerializeField] private List<KillCounterBinding> counters = new List<KillCounterBinding>();

        private void Awake()
        {
            ResolveReferences();
            ResetCounters();
        }

        private void OnEnable()
        {
            ResolveReferences();
            if (gameManager != null)
            {
                gameManager.KillStatsChanged += HandleKillStatsChanged;
            }

            ResetCounters();
        }

        private void OnDisable()
        {
            if (gameManager != null)
            {
                gameManager.KillStatsChanged -= HandleKillStatsChanged;
            }
        }

        private void OnValidate()
        {
            ResolveReferences();
        }

        private void ResolveReferences()
        {
            if (gameManager == null)
            {
                gameManager = FindFirstObjectByType<GameManager>();
            }
        }

        private void ResetCounters()
        {
            for (int i = 0; i < counters.Count; i++)
            {
                if (counters[i]?.countText != null)
                {
                    counters[i].countText.text = "x0";
                }
            }
        }

        private void HandleKillStatsChanged(IReadOnlyList<GameplayKillStat> killStats)
        {
            ResetCounters();
            if (killStats == null)
            {
                return;
            }

            for (int i = 0; i < killStats.Count; i++)
            {
                GameplayKillStat stat = killStats[i];
                TMP_Text text = FindCounterText(stat.EnemyType);
                if (text != null)
                {
                    text.text = $"x{stat.Count}";
                }
            }
        }

        private TMP_Text FindCounterText(EnemyType enemyType)
        {
            for (int i = 0; i < counters.Count; i++)
            {
                KillCounterBinding binding = counters[i];
                if (binding != null && binding.enemyType == enemyType)
                {
                    return binding.countText;
                }
            }

            return null;
        }
    }
}
