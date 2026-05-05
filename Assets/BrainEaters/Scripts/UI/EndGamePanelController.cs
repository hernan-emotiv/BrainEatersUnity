using BrainEaters.GameFlow;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace BrainEaters.UI
{
    public class EndGamePanelController : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private GameObject winPanel;
        [SerializeField] private GameObject losePanel;
        [SerializeField] private TMP_Text winTitleText;
        [SerializeField] private TMP_Text winReportText;
        [SerializeField] private TMP_Text loseTitleText;
        [SerializeField] private TMP_Text loseReportText;
        [SerializeField] private EndGameScoreReportView winScoreReportView;
        [SerializeField] private EndGameScoreReportView loseScoreReportView;
        [SerializeField] private Button winRetryButton;
        [SerializeField] private Button loseRetryButton;
        [SerializeField] private Button winBackToMenuButton;
        [SerializeField] private Button loseBackToMenuButton;

        private void Awake()
        {
            ResolveReferences();
            SetPanelsVisible(false, false, true);
            BindButtons();
            Debug.Log($"EndGamePanelController Awake. GameManager assigned: {gameManager != null}.", this);
        }

        private void OnEnable()
        {
            ResolveReferences();
            BindButtons();

            if (gameManager != null)
            {
                gameManager.StateChanged += HandleStateChanged;
                gameManager.GameplayFinished += HandleGameplayFinished;
                Debug.Log("EndGamePanelController subscribed to GameManager events.", this);
            }
            else
            {
                Debug.LogWarning("EndGamePanelController could not find a GameManager on enable.", this);
            }
        }

        private void OnDisable()
        {
            if (gameManager != null)
            {
                gameManager.StateChanged -= HandleStateChanged;
                gameManager.GameplayFinished -= HandleGameplayFinished;
            }
        }

        private void OnValidate()
        {
            ResolveReferences();
        }

        private void Update()
        {
            if (gameManager == null)
            {
                return;
            }

            if (gameManager.CurrentState != GameplayState.Won && gameManager.CurrentState != GameplayState.Lost)
            {
                return;
            }

            Keyboard keyboard = Keyboard.current;
            if (keyboard == null)
            {
                return;
            }

            if (keyboard.rKey.wasPressedThisFrame)
            {
                Debug.Log("Keyboard retry requested with R.", this);
                HandleRetryPressed();
            }
            else if (keyboard.escapeKey.wasPressedThisFrame)
            {
                Debug.Log("Keyboard back to menu requested with Escape.", this);
                HandleBackToMenuPressed();
            }
        }

        private void HandleGameplayFinished(GameplayReport report)
        {
            Debug.Log($"EndGamePanelController received GameplayFinished. Result: {report.ResultState}.", this);
            bool didWin = report.ResultState == GameplayState.Won;
            SetPanelsVisible(didWin, !didWin);

            string reportText = BuildReportText(report);

            if (didWin)
            {
                if (winTitleText != null)
                {
                    winTitleText.text = "VICTORY!";
                }

                if (winReportText != null)
                {
                    winReportText.text = reportText;
                }

                if (winScoreReportView != null)
                {
                    winScoreReportView.SetReport(report);
                }
            }
            else
            {
                if (loseTitleText != null)
                {
                    loseTitleText.text = "GAME OVER";
                }

                if (loseReportText != null)
                {
                    loseReportText.text = reportText;
                }

                if (loseScoreReportView != null)
                {
                    loseScoreReportView.SetReport(report);
                }
            }
        }

        private void HandleStateChanged(GameplayState gameplayState)
        {
            if (gameplayState == GameplayState.Initializing || gameplayState == GameplayState.Running)
            {
                SetPanelsVisible(false, false, true);
            }
        }

        private string BuildReportText(GameplayReport report)
        {
            System.Text.StringBuilder builder = new System.Text.StringBuilder();
            builder.AppendLine($"Total Enemies Eliminated: {report.TotalEnemiesEliminated}");

            foreach (GameplayKillStat killStat in report.KillStats)
            {
                builder.AppendLine($"{killStat.DisplayName}: {killStat.Count} x {killStat.ScoreValue} = {killStat.TotalScore}");
            }

            builder.AppendLine($"Score: {report.TotalScore}");
            builder.AppendLine($"Damage Received: {report.DamageReceived:0}");
            builder.Append($"Time Survived: {FormatTime(report.ElapsedSeconds)} / {FormatTime(report.TargetDurationSeconds)}");
            return builder.ToString();
        }

        private void BindButtons()
        {
            Debug.Log(
                $"Binding end game buttons. " +
                $"WinRetry: {winRetryButton != null}, " +
                $"LoseRetry: {loseRetryButton != null}, " +
                $"WinBack: {winBackToMenuButton != null}, " +
                $"LoseBack: {loseBackToMenuButton != null}.",
                this);

            BindButton(winRetryButton, HandleRetryPressed);
            BindButton(loseRetryButton, HandleRetryPressed);
            BindButton(winBackToMenuButton, HandleBackToMenuPressed);
            BindButton(loseBackToMenuButton, HandleBackToMenuPressed);
        }

        private static void BindButton(Button button, UnityEngine.Events.UnityAction callback)
        {
            if (button == null)
            {
                return;
            }

            button.onClick.RemoveListener(callback);
            button.onClick.AddListener(callback);
        }

        private void HandleRetryPressed()
        {
            Debug.Log("Retry button pressed.", this);
            SetPanelsVisible(false, false);

            if (gameManager != null)
            {
                Debug.Log("Retry button is calling GameManager.RetryLevel().", this);
                gameManager.RetryLevel();
            }
            else
            {
                Debug.LogWarning("Retry button pressed but GameManager is missing.", this);
            }
        }

        private void HandleBackToMenuPressed()
        {
            Debug.Log("Back to Menu button pressed.", this);
            SetPanelsVisible(false, false);

            if (gameManager != null)
            {
                Debug.Log("Back to Menu button is calling GameManager.BackToMenu().", this);
                gameManager.BackToMenu();
            }
            else
            {
                Debug.LogWarning("Back to Menu button pressed but GameManager is missing.", this);
            }
        }

        private void SetPanelsVisible(bool showWin, bool showLose, bool instant = false)
        {
            SetPanelVisible(winPanel, showWin, instant);
            SetPanelVisible(losePanel, showLose, instant);
        }

        private static void SetPanelVisible(GameObject panel, bool visible, bool instant)
        {
            if (panel == null)
            {
                return;
            }

            UiVisibilityAnimator animator = panel.GetComponent<UiVisibilityAnimator>();
            if (animator != null)
            {
                animator.SetVisible(visible, instant);
                return;
            }

            panel.SetActive(visible);
        }

        private void ResolveReferences()
        {
            if (gameManager == null)
            {
                gameManager = FindFirstObjectByType<GameManager>();
            }

            winPanel ??= transform.Find("WinPanel")?.gameObject;
            losePanel ??= transform.Find("LosePanel")?.gameObject;

            ResolvePanelReferences(
                winPanel,
                ref winTitleText,
                ref winReportText,
                ref winScoreReportView,
                ref winRetryButton,
                ref winBackToMenuButton);
            ResolvePanelReferences(
                losePanel,
                ref loseTitleText,
                ref loseReportText,
                ref loseScoreReportView,
                ref loseRetryButton,
                ref loseBackToMenuButton);
        }

        private static void ResolvePanelReferences(
            GameObject panel,
            ref TMP_Text titleText,
            ref TMP_Text reportText,
            ref EndGameScoreReportView scoreReportView,
            ref Button retryButton,
            ref Button backToMenuButton)
        {
            if (panel == null)
            {
                return;
            }

            Transform panelTransform = panel.transform;
            titleText ??= panelTransform.Find("TitleText")?.GetComponent<TMP_Text>();
            reportText ??= panelTransform.Find("ReportText")?.GetComponent<TMP_Text>();
            scoreReportView ??= panel.GetComponentInChildren<EndGameScoreReportView>(true);
            retryButton ??= panelTransform.Find("RetryButton")?.GetComponent<Button>();
            backToMenuButton ??= panelTransform.Find("BackToMenuButton")?.GetComponent<Button>();
        }

        private static string FormatTime(float timeSeconds)
        {
            int totalSeconds = Mathf.Max(0, Mathf.FloorToInt(timeSeconds));
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;
            return $"{minutes:00}:{seconds:00}";
        }
    }
}
