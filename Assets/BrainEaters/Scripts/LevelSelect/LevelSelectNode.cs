using BrainEaters.Configs;
using BrainEaters.GameFlow;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BrainEaters.LevelSelect
{
    public class LevelSelectNode : MonoBehaviour
    {
        [SerializeField] private LevelConfig levelConfig;
        [SerializeField] private Renderer[] stateRenderers;
        [SerializeField] private Graphic[] stateGraphics;
        [SerializeField] private GameObject[] lockedStateObjects;
        [SerializeField] private GameObject[] unlockedStateObjects;
        [SerializeField] private GameObject[] newStateObjects;
        [SerializeField] private GameObject[] completedStateObjects;
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text stateText;
        [SerializeField] private Color lockedColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        [SerializeField] private Color unlockedColor = new Color(0.35f, 0.75f, 1f, 1f);
        [SerializeField] private Color newColor = new Color(1f, 0.82f, 0.2f, 1f);
        [SerializeField] private Color completedColor = new Color(0.35f, 1f, 0.45f, 1f);

        public LevelConfig LevelConfig => levelConfig;
        public Button Button => button;

        private void OnValidate()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            if (titleText != null && levelConfig != null)
            {
                titleText.text = levelConfig.DisplayName;
            }
        }

        public void ApplyState(LevelAvailabilityState state)
        {
            if (titleText != null && levelConfig != null)
            {
                titleText.text = levelConfig.DisplayName;
            }

            if (stateText != null)
            {
                stateText.text = state switch
                {
                    LevelAvailabilityState.Locked => "LOCKED",
                    LevelAvailabilityState.Unlocked => "OPEN",
                    LevelAvailabilityState.New => "NEW",
                    _ => "DONE"
                };
            }

            Color stateColor = state switch
            {
                LevelAvailabilityState.Locked => lockedColor,
                LevelAvailabilityState.Unlocked => unlockedColor,
                LevelAvailabilityState.New => newColor,
                _ => completedColor
            };

            for (int i = 0; i < stateRenderers.Length; i++)
            {
                Renderer rendererComponent = stateRenderers[i];
                if (rendererComponent == null)
                {
                    continue;
                }

                rendererComponent.material.color = stateColor;
            }

            for (int i = 0; i < stateGraphics.Length; i++)
            {
                Graphic graphic = stateGraphics[i];
                if (graphic == null)
                {
                    continue;
                }

                graphic.color = stateColor;
            }

            ApplyStateObjectVisibility(state);

            if (button != null)
            {
                button.interactable = state != LevelAvailabilityState.Locked;
            }
        }

        private void ApplyStateObjectVisibility(LevelAvailabilityState state)
        {
            SetActiveForStateObjects(lockedStateObjects, state == LevelAvailabilityState.Locked);
            SetActiveForStateObjects(unlockedStateObjects, state == LevelAvailabilityState.Unlocked);
            SetActiveForStateObjects(newStateObjects, state == LevelAvailabilityState.New);
            SetActiveForStateObjects(completedStateObjects, state == LevelAvailabilityState.Completed);
        }

        private static void SetActiveForStateObjects(GameObject[] stateObjects, bool active)
        {
            if (stateObjects == null)
            {
                return;
            }

            for (int i = 0; i < stateObjects.Length; i++)
            {
                GameObject gameObject = stateObjects[i];
                if (gameObject == null)
                {
                    continue;
                }

                gameObject.SetActive(active);
            }
        }
    }
}
