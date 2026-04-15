using UnityEngine;

namespace BrainEaters.UI
{
    public static class UiEaseUtility
    {
        public static float Evaluate(UiEase ease, float t)
        {
            t = Mathf.Clamp01(t);

            return ease switch
            {
                UiEase.EaseOutQuad => 1f - ((1f - t) * (1f - t)),
                UiEase.EaseOutCubic => 1f - Mathf.Pow(1f - t, 3f),
                UiEase.EaseOutBack => EaseOutBack(t),
                UiEase.EaseInOutQuad => t < 0.5f ? 2f * t * t : 1f - Mathf.Pow(-2f * t + 2f, 2f) * 0.5f,
                _ => t
            };
        }

        private static float EaseOutBack(float t)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1f;
            float x = t - 1f;
            return 1f + (c3 * x * x * x) + (c1 * x * x);
        }
    }
}
