using System.Collections.Generic;
using BrainEaters.Configs;
using BrainEaters.GameFlow;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BrainEaters.UI
{
    public class EndGameScoreReportView : MonoBehaviour
    {
#pragma warning disable 0649
        [System.Serializable]
        private class EnemyIconBinding
        {
            public EnemyType enemyType;
            public Sprite icon;
        }
#pragma warning restore 0649

        [SerializeField] private RectTransform rowsContainer;
        [SerializeField] private GameObject rowTemplate;
        [SerializeField] private TMP_Text totalScoreText;
        [SerializeField] private List<EnemyIconBinding> enemyIcons = new List<EnemyIconBinding>();

        private readonly List<GameObject> activeRows = new List<GameObject>();

        public void SetReport(GameplayReport report)
        {
            ClearRows();
            if (report == null)
            {
                SetTotalScore(0);
                return;
            }

            IReadOnlyList<GameplayKillStat> killStats = report.KillStats;
            if (killStats != null)
            {
                for (int i = 0; i < killStats.Count; i++)
                {
                    CreateRow(killStats[i]);
                }
            }

            SetTotalScore(report.TotalScore);
        }

        private void OnEnable()
        {
            if (rowTemplate != null)
            {
                rowTemplate.SetActive(false);
            }
        }

        private void ClearRows()
        {
            for (int i = activeRows.Count - 1; i >= 0; i--)
            {
                if (activeRows[i] != null)
                {
                    Destroy(activeRows[i]);
                }
            }

            activeRows.Clear();
        }

        private void CreateRow(GameplayKillStat stat)
        {
            if (rowTemplate == null || rowsContainer == null || stat == null)
            {
                return;
            }

            GameObject row = Instantiate(rowTemplate, rowsContainer);
            row.name = $"{stat.EnemyType}ScoreRow";
            row.SetActive(true);
            activeRows.Add(row);

            Image icon = row.transform.Find("Icon")?.GetComponent<Image>();
            if (icon != null)
            {
                icon.sprite = FindIcon(stat.EnemyType);
                icon.enabled = icon.sprite != null;
            }

            TMP_Text formulaText = row.transform.Find("FormulaText")?.GetComponent<TMP_Text>();
            if (formulaText != null)
            {
                formulaText.text = $"{stat.ScoreValue} points x{stat.Count}";
            }

            TMP_Text scoreText = row.transform.Find("ScoreText")?.GetComponent<TMP_Text>();
            if (scoreText != null)
            {
                scoreText.text = stat.TotalScore.ToString();
            }
        }

        private void SetTotalScore(int totalScore)
        {
            if (totalScoreText != null)
            {
                totalScoreText.text = $"SCORE {totalScore}";
            }
        }

        private Sprite FindIcon(EnemyType enemyType)
        {
            for (int i = 0; i < enemyIcons.Count; i++)
            {
                EnemyIconBinding binding = enemyIcons[i];
                if (binding != null && binding.enemyType == enemyType)
                {
                    return binding.icon;
                }
            }

            return null;
        }
    }
}
