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

            if (survivalRoot == null)
            {
                survivalRoot = FindChildGameObject("GameTimer");
            }

            if (captureRoot == null)
            {
                captureRoot = FindChildGameObject("CaptureObjectivePanel");
            }

            if (collectRoot == null)
            {
                collectRoot = FindChildGameObject("CollectObjectivePanel");
            }
        }

        private GameObject FindChildGameObject(string childName)
        {
            Transform child = FindChildRecursive(transform, childName);
            return child != null ? child.gameObject : null;
        }

        private static Transform FindChildRecursive(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                if (child.name == childName)
                {
                    return child;
                }

                Transform nestedChild = FindChildRecursive(child, childName);
                if (nestedChild != null)
                {
                    return nestedChild;
                }
            }

            return null;
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
