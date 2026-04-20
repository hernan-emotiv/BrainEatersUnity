using BrainEaters.Configs;
using BrainEaters.GameFlow;
using TMPro;
using UnityEngine;

namespace BrainEaters.UI
{
    public class ObjectiveProgressTextController : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private GameModeType targetMode = GameModeType.Collect;
        [SerializeField] private TMP_Text valueText;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private string collectTitle = "COLLECTED";
        [SerializeField] private string captureTitle = "CAPTURED";
        [SerializeField] private string valueFormat = "{0}/{1}";

        private void Awake()
        {
            ResolveReferences();
            RefreshAll();
        }

        private void OnEnable()
        {
            ResolveReferences();
            if (gameManager != null)
            {
                gameManager.ObjectiveModeChanged += HandleObjectiveModeChanged;
                gameManager.ObjectiveProgressChanged += HandleObjectiveProgressChanged;
            }

            RefreshAll();
        }

        private void OnDisable()
        {
            if (gameManager != null)
            {
                gameManager.ObjectiveModeChanged -= HandleObjectiveModeChanged;
                gameManager.ObjectiveProgressChanged -= HandleObjectiveProgressChanged;
            }
        }

        private void OnValidate()
        {
            ResolveReferences();
            RefreshTitle();
        }

        private void ResolveReferences()
        {
            if (gameManager == null)
            {
                gameManager = FindFirstObjectByType<GameManager>();
            }
        }

        private void HandleObjectiveModeChanged(GameModeType _)
        {
            RefreshAll();
        }

        private void HandleObjectiveProgressChanged(GameModeType mode, int current, int total)
        {
            if (mode != targetMode)
            {
                return;
            }

            RefreshTitle();
            SetValue(current, total);
        }

        private void RefreshAll()
        {
            RefreshTitle();

            if (gameManager == null)
            {
                SetValue(0, 0);
                return;
            }

            if (targetMode == GameModeType.Collect)
            {
                SetValue(gameManager.CollectedPickupsCount, gameManager.TotalCollectPickups);
                return;
            }

            if (targetMode == GameModeType.Capture)
            {
                SetValue(gameManager.CapturedZonesCount, gameManager.TotalCaptureZones);
                return;
            }

            SetValue(0, 0);
        }

        private void RefreshTitle()
        {
            if (titleText == null)
            {
                return;
            }

            titleText.text = targetMode switch
            {
                GameModeType.Capture => captureTitle,
                GameModeType.Collect => collectTitle,
                _ => string.Empty
            };
        }

        private void SetValue(int current, int total)
        {
            if (valueText != null)
            {
                valueText.text = string.Format(valueFormat, current, total);
            }
        }
    }
}
