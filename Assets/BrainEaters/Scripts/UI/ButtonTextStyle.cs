using TMPro;
using UnityEngine;

namespace BrainEaters.UI
{
    [CreateAssetMenu(fileName = "ButtonTextStyle", menuName = "Brain Eaters/UI/Button Text Style")]
    public class ButtonTextStyle : ScriptableObject
    {
        [SerializeField] private TMP_FontAsset font;
        [SerializeField] private float fontSize = 46f;
        [SerializeField] private FontStyles fontStyle = FontStyles.Bold;
        [SerializeField] private float characterSpacing = 2f;
        [SerializeField] private Color faceColor = Color.white;
        [SerializeField] private TextAlignmentOptions alignment = TextAlignmentOptions.Center;
        [SerializeField] private Vector2 rectOffsetMin = new Vector2(0f, 8f);
        [SerializeField] private Vector2 rectOffsetMax = new Vector2(0f, -4f);
        [SerializeField] private bool enableWordWrapping;
        [SerializeField] private TextOverflowModes overflowMode = TextOverflowModes.Overflow;
        [SerializeField] private float faceDilate = 0.08f;
        [SerializeField] private float outlineWidth = 0.18f;
        [SerializeField] private float outlineSoftness = 0.02f;
        [SerializeField] private Color outlineColor = new Color(0.42f, 0.25f, 0.08f, 1f);
        [SerializeField] private Vector2 underlayOffset = new Vector2(0.45f, -0.65f);
        [SerializeField] private float underlayDilate = 0.18f;
        [SerializeField] private float underlaySoftness = 0.25f;
        [SerializeField] private Color underlayColor = new Color(0f, 0f, 0f, 0.42f);

        public void ConfigureFont(TMP_FontAsset fontAsset)
        {
            font = fontAsset;
        }

        public void ApplyTo(TMP_Text text, bool forceUppercase = false)
        {
            if (text == null)
            {
                return;
            }

            RectTransform rectTransform = text.rectTransform;
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.offsetMin = rectOffsetMin;
            rectTransform.offsetMax = rectOffsetMax;

            if (font != null)
            {
                text.font = font;
            }

            text.fontSize = fontSize;
            text.fontStyle = forceUppercase ? fontStyle | FontStyles.UpperCase : fontStyle;
            text.characterSpacing = characterSpacing;
            text.color = faceColor;
            text.alignment = alignment;
            text.enableWordWrapping = enableWordWrapping;
            text.overflowMode = overflowMode;
            text.raycastTarget = false;
            ApplyMaterial(text);
        }

        private void ApplyMaterial(TMP_Text text)
        {
            if (text.font == null || text.font.material == null)
            {
                return;
            }

            Material material = new Material(text.font.material)
            {
                name = $"{name}_{text.name}_Material"
            };

            material.SetFloat(ShaderUtilities.ID_FaceDilate, faceDilate);
            material.SetFloat(ShaderUtilities.ID_OutlineWidth, outlineWidth);
            material.SetFloat(ShaderUtilities.ID_OutlineSoftness, outlineSoftness);
            material.SetColor(ShaderUtilities.ID_OutlineColor, outlineColor);
            material.SetFloat(ShaderUtilities.ID_UnderlayOffsetX, underlayOffset.x);
            material.SetFloat(ShaderUtilities.ID_UnderlayOffsetY, underlayOffset.y);
            material.SetFloat(ShaderUtilities.ID_UnderlayDilate, underlayDilate);
            material.SetFloat(ShaderUtilities.ID_UnderlaySoftness, underlaySoftness);
            material.SetColor(ShaderUtilities.ID_UnderlayColor, underlayColor);
            material.EnableKeyword("UNDERLAY_ON");
            text.fontSharedMaterial = material;
        }
    }
}
