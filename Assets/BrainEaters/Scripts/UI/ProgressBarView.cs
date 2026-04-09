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
        [SerializeField] private bool preserveRightEdge = true;

        private RectTransform fillRectTransform;
        private float fullWidth = -1f;

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
            if (fullWidth < 0f)
            {
                fullWidth = fillRectTransform.rect.width;
            }

            float targetWidth = fullWidth * clampedValue;
            Vector2 sizeDelta = fillRectTransform.sizeDelta;
            sizeDelta.x = targetWidth;
            fillRectTransform.sizeDelta = sizeDelta;

            if (preserveRightEdge)
            {
                Vector2 anchoredPosition = fillRectTransform.anchoredPosition;
                anchoredPosition.x = 0f;
                fillRectTransform.anchoredPosition = anchoredPosition;
            }
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

            if (fillArea != null)
            {
                fullWidth = fillArea.rect.width;
            }
            else if (fillRectTransform != null && fullWidth < 0f)
            {
                fullWidth = fillRectTransform.rect.width;
            }
        }
    }
}
