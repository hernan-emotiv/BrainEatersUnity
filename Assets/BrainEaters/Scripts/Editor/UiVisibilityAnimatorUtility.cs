using BrainEaters.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace BrainEaters.EditorTools
{
    public static class UiVisibilityAnimatorUtility
    {
        private const string EndGamePanelsPrefabPath = "Assets/BrainEaters/Prefabs/UI/EndGamePanelsRoot.prefab";

        [MenuItem("Brain Eaters/UI/Apply Visibility Animators In Current Scene")]
        public static void ApplyVisibilityAnimatorsInCurrentScene()
        {
            RectTransform[] rectTransforms = Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int appliedCount = 0;

            foreach (RectTransform rectTransform in rectTransforms)
            {
                if (!ShouldApplyAnimator(rectTransform.name))
                {
                    continue;
                }

                EnsureVisibilityAnimator(rectTransform.gameObject);
                appliedCount++;
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log($"Applied Brain Eaters visibility animators to {appliedCount} UI object(s) in the current scene.");
        }

        [MenuItem("Brain Eaters/UI/Apply Visibility Animators To End Game Prefab")]
        public static void ApplyVisibilityAnimatorsToEndGamePrefab()
        {
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(EndGamePanelsPrefabPath);
            try
            {
                int appliedCount = 0;
                appliedCount += TryEnsureChildAnimator(prefabRoot.transform, "WinPanel");
                appliedCount += TryEnsureChildAnimator(prefabRoot.transform, "LosePanel");
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, EndGamePanelsPrefabPath);
                Debug.Log($"Applied Brain Eaters visibility animators to {appliedCount} object(s) in {EndGamePanelsPrefabPath}.");
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        public static UiVisibilityAnimator EnsureVisibilityAnimator(GameObject target)
        {
            CanvasGroup canvasGroup = target.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = Undo.AddComponent<CanvasGroup>(target);
            }

            UiVisibilityAnimator animator = target.GetComponent<UiVisibilityAnimator>();
            if (animator == null)
            {
                animator = Undo.AddComponent<UiVisibilityAnimator>(target);
            }

            ConfigureAnimator(animator, target.GetComponent<RectTransform>(), canvasGroup);
            EditorUtility.SetDirty(target);
            return animator;
        }

        private static int TryEnsureChildAnimator(Transform parent, string childName)
        {
            Transform child = parent.Find(childName);
            if (child == null)
            {
                return 0;
            }

            EnsureVisibilityAnimator(child.gameObject);
            return 1;
        }

        private static bool ShouldApplyAnimator(string objectName)
        {
            return objectName == "TutorialPopupRoot"
                || objectName == "HowToPlayPopupRoot"
                || objectName == "WinPanel"
                || objectName == "LosePanel";
        }

        private static void ConfigureAnimator(UiVisibilityAnimator animator, RectTransform targetRect, CanvasGroup canvasGroup)
        {
            SerializedObject serializedObject = new SerializedObject(animator);
            serializedObject.FindProperty("targetRect").objectReferenceValue = targetRect;
            serializedObject.FindProperty("canvasGroup").objectReferenceValue = canvasGroup;
            serializedObject.FindProperty("visibleScale").vector3Value = Vector3.one;
            serializedObject.FindProperty("hiddenScale").vector3Value = new Vector3(0.86f, 0.86f, 0.86f);
            serializedObject.FindProperty("visibleAlpha").floatValue = 1f;
            serializedObject.FindProperty("hiddenAlpha").floatValue = 0f;
            serializedObject.FindProperty("showDuration").floatValue = 0.18f;
            serializedObject.FindProperty("hideDuration").floatValue = 0.14f;
            serializedObject.FindProperty("showEase").enumValueIndex = (int)UiEase.EaseOutBack;
            serializedObject.FindProperty("hideEase").enumValueIndex = (int)UiEase.EaseInOutQuad;
            serializedObject.FindProperty("useUnscaledTime").boolValue = true;
            serializedObject.FindProperty("deactivateWhenHidden").boolValue = true;
            serializedObject.FindProperty("blockRaycastsWhenVisible").boolValue = true;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(animator);
        }
    }
}
