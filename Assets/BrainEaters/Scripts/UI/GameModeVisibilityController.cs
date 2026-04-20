using BrainEaters.Configs;
using BrainEaters.GameFlow;
using UnityEngine;

namespace BrainEaters.UI
{
    public class GameModeVisibilityController : MonoBehaviour
    {
        [SerializeField] private GameManager gameManager;
        [SerializeField] private GameObject survivalRoot;
        [SerializeField] private GameObject captureRoot;
        [SerializeField] private GameObject collectRoot;
        [SerializeField] private GameObject fallbackRoot;

        private void Awake()
        {
            ResolveReferences();
            ApplyMode(gameManager != null ? gameManager.ActiveGameMode : GameModeType.Survival);
        }

        private void OnEnable()
        {
            ResolveReferences();
            if (gameManager != null)
            {
                gameManager.ObjectiveModeChanged += ApplyMode;
            }

            ApplyMode(gameManager != null ? gameManager.ActiveGameMode : GameModeType.Survival);
        }

        private void OnDisable()
        {
            if (gameManager != null)
            {
                gameManager.ObjectiveModeChanged -= ApplyMode;
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

        private void ApplyMode(GameModeType mode)
        {
            SetActive(survivalRoot, mode == GameModeType.Survival);
            SetActive(captureRoot, mode == GameModeType.Capture);
            SetActive(collectRoot, mode == GameModeType.Collect);
            SetActive(fallbackRoot, mode != GameModeType.Survival && mode != GameModeType.Capture && mode != GameModeType.Collect);
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null)
            {
                target.SetActive(active);
            }
        }
    }
}
