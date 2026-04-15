using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BrainEaters.UI
{
    public class HeartHealthView : MonoBehaviour
    {
        [SerializeField] private RectTransform heartContainer;
        [SerializeField] private Image heartTemplate;
        [SerializeField] private bool autoCollectExistingChildren = true;
        [SerializeField] private bool animateVisibility = true;

        private readonly List<Image> spawnedHearts = new List<Image>();
        private readonly List<UiScaleVisibilityAnimator> heartAnimators = new List<UiScaleVisibilityAnimator>();
        private bool initialized;
        private bool hasAppliedInitialState;

        public void SetHealth(int currentHealth, int maxHealth)
        {
            if (heartContainer == null || heartTemplate == null)
            {
                return;
            }

            int safeMaxHealth = Mathf.Max(0, maxHealth);
            EnsureInitialized();
            EnsureHeartCount(safeMaxHealth);

            for (int i = 0; i < spawnedHearts.Count; i++)
            {
                bool shouldExist = i < safeMaxHealth;
                bool shouldBeVisible = i < currentHealth;
                bool finalVisible = shouldExist && shouldBeVisible;

                UiScaleVisibilityAnimator animator = i < heartAnimators.Count ? heartAnimators[i] : null;
                if (animateVisibility && animator != null)
                {
                    animator.SetVisible(finalVisible, !hasAppliedInitialState);
                }
                else
                {
                    spawnedHearts[i].gameObject.SetActive(finalVisible);
                }
            }

            hasAppliedInitialState = true;
        }

        private void EnsureHeartCount(int heartCount)
        {
            while (spawnedHearts.Count < heartCount)
            {
                Image heartInstance = Instantiate(heartTemplate, heartContainer);
                heartInstance.gameObject.name = $"Heart_{spawnedHearts.Count + 1}";
                heartInstance.gameObject.SetActive(true);
                spawnedHearts.Add(heartInstance);
                heartAnimators.Add(GetOrAddHeartAnimator(heartInstance));
            }
        }

        private void EnsureInitialized()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;

            if (!autoCollectExistingChildren || heartContainer == null)
            {
                return;
            }

            spawnedHearts.Clear();

            for (int i = 0; i < heartContainer.childCount; i++)
            {
                Transform child = heartContainer.GetChild(i);
                Image childImage = child.GetComponent<Image>();
                if (childImage == null || childImage == heartTemplate)
                {
                    continue;
                }

                spawnedHearts.Add(childImage);
                heartAnimators.Add(GetOrAddHeartAnimator(childImage));
            }
        }

        private UiScaleVisibilityAnimator GetOrAddHeartAnimator(Image heartImage)
        {
            if (heartImage == null)
            {
                return null;
            }

            UiScaleVisibilityAnimator animator = heartImage.GetComponent<UiScaleVisibilityAnimator>();
            if (animator == null)
            {
                animator = heartImage.gameObject.AddComponent<UiScaleVisibilityAnimator>();
            }

            return animator;
        }
    }
}
