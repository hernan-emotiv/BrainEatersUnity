using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BrainEaters.UI
{
    public class ProgressBarView : MonoBehaviour
    {
        [SerializeField] private Image fillImage;
        [SerializeField] private RectTransform fillArea;
        [SerializeField] private TMP_Text valueText;
        [SerializeField] private TMP_Text statusText;

        private RectTransform fillRectTransform;

        private void Awake()
        {
            CacheReferences();
        }

        private void OnValidate()
        {
            CacheReferences();
        }

        public void SetNormalizedValue(float normalizedValue)
        {
            if (fillRectTransform == null)
            {
                CacheReferences();
            }

            if (fillRectTransform == null)
            {
                return;
            }

            float clampedValue = Mathf.Clamp01(normalizedValue);
            if (fillArea == null)
            {
                return;
            }

            float availableWidth = fillArea.rect.width;
            float rightInset = availableWidth * (1f - clampedValue);

            fillRectTransform.anchorMin = new Vector2(0f, 0f);
            fillRectTransform.anchorMax = new Vector2(1f, 1f);
            fillRectTransform.pivot = new Vector2(0f, 0.5f);
            fillRectTransform.offsetMin = new Vector2(0f, 0f);
            fillRectTransform.offsetMax = new Vector2(-rightInset, 0f);
        }

        public void SetValueText(string text)
        {
            if (valueText != null)
            {
                valueText.text = text;
            }
        }

        public void SetStatusText(string text, Color color)
        {
            if (statusText != null)
            {
                statusText.text = text;
                statusText.color = color;
            }
        }

        private void CacheReferences()
        {
            if (fillImage == null)
            {
                return;
            }

            fillRectTransform = fillImage.rectTransform;
            if (fillArea == null)
            {
                fillArea = fillRectTransform.parent as RectTransform;
            }
        }
    }
}
