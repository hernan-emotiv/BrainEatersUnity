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

        private readonly List<Image> spawnedHearts = new List<Image>();
        private bool initialized;

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

                spawnedHearts[i].gameObject.SetActive(shouldExist && shouldBeVisible);
            }
        }

        private void EnsureHeartCount(int heartCount)
        {
            while (spawnedHearts.Count < heartCount)
            {
                Image heartInstance = Instantiate(heartTemplate, heartContainer);
                heartInstance.gameObject.name = $"Heart_{spawnedHearts.Count + 1}";
                heartInstance.gameObject.SetActive(true);
                spawnedHearts.Add(heartInstance);
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
            }
        }
    }
}
