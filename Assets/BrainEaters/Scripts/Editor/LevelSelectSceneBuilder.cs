using BrainEaters.LevelSelect;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace BrainEaters.EditorTools
{
    public static class LevelSelectSceneBuilder
    {
        [MenuItem("Brain Eaters/Create Level Select UI")]
        public static void CreateLevelSelectUi()
        {
            if (TMP_Settings.defaultFontAsset == null)
            {
                Debug.LogError("TMP default font asset is missing. Import TMP Essential Resources first.");
                return;
            }

            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObject = new GameObject("LevelSelectCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;
            }

            GameObject managerObject = GetOrCreateUiChild(canvas.transform, "LevelSelectManager");
            RectTransform managerRect = managerObject.GetComponent<RectTransform>();
            StretchFull(managerRect);

            LevelSelectMapManager manager = managerObject.GetComponent<LevelSelectMapManager>();
            if (manager == null)
            {
                manager = Undo.AddComponent<LevelSelectMapManager>(managerObject);
            }

            GameObject safeAreaRoot = GetOrCreateUiChild(managerObject.transform, "SafeAreaRoot");
            RectTransform safeAreaRect = safeAreaRoot.GetComponent<RectTransform>();
            StretchFull(safeAreaRect);
            safeAreaRect.offsetMin = new Vector2(32f, 32f);
            safeAreaRect.offsetMax = new Vector2(-32f, -32f);

            TMP_Text titleText = CreateLabel(safeAreaRoot.transform, "TitleText", "Brain Eaters", 42f, new Vector2(0f, -18f), new Vector2(600f, 60f));
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            titleText.rectTransform.anchorMax = new Vector2(0.5f, 1f);

            Button unlockAllButton = CreateButton(safeAreaRoot.transform, "UnlockAllButton", "Unlock All", new Vector2(-390f, -80f), new Vector2(240f, 64f));
            Button resetProgressButton = CreateButton(safeAreaRoot.transform, "ResetProgressButton", "Reset Progress", new Vector2(-140f, -80f), new Vector2(240f, 64f));
            TMP_Text statusText = CreateLabel(safeAreaRoot.transform, "StatusText", "Select a level", 28f, new Vector2(0f, -60f), new Vector2(900f, 50f));

            GameObject scrollView = GetOrCreateUiChild(safeAreaRoot.transform, "LevelScrollView");
            ConfigureScrollView(scrollView.GetComponent<RectTransform>());
            ScrollRect scrollRect = GetOrAddComponent<ScrollRect>(scrollView);
            Image scrollImage = GetOrAddComponent<Image>(scrollView);
            scrollImage.color = new Color(0f, 0f, 0f, 0.12f);
            Mask scrollMask = GetOrAddComponent<Mask>(scrollView);
            scrollMask.showMaskGraphic = true;

            GameObject viewport = GetOrCreateUiChild(scrollView.transform, "Viewport");
            RectTransform viewportRect = viewport.GetComponent<RectTransform>();
            StretchFull(viewportRect);
            Image viewportImage = GetOrAddComponent<Image>(viewport);
            viewportImage.color = new Color(1f, 1f, 1f, 0.01f);
            Mask viewportMask = GetOrAddComponent<Mask>(viewport);
            viewportMask.showMaskGraphic = false;

            GameObject content = GetOrCreateUiChild(viewport.transform, "Content");
            RectTransform contentRect = content.GetComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 0f);
            contentRect.anchorMax = new Vector2(0f, 1f);
            contentRect.pivot = new Vector2(0f, 0.5f);
            contentRect.anchoredPosition = Vector2.zero;
            contentRect.sizeDelta = new Vector2(2400f, 0f);

            GameObject mapBackground = GetOrCreateUiChild(content.transform, "MapBackground");
            RectTransform mapBackgroundRect = mapBackground.GetComponent<RectTransform>();
            StretchFull(mapBackgroundRect);
            Image mapBackgroundImage = GetOrAddComponent<Image>(mapBackground);
            mapBackgroundImage.color = new Color(0.16f, 0.2f, 0.18f, 1f);

            GameObject levelNodesRoot = GetOrCreateUiChild(content.transform, "LevelNodesRoot");
            RectTransform levelNodesRect = levelNodesRoot.GetComponent<RectTransform>();
            StretchFull(levelNodesRect);

            scrollRect.viewport = viewportRect;
            scrollRect.content = contentRect;
            scrollRect.horizontal = true;
            scrollRect.vertical = false;
            scrollRect.movementType = ScrollRect.MovementType.Clamped;

            SerializedObject managerObjectSerialized = new SerializedObject(manager);
            managerObjectSerialized.FindProperty("unlockAllButton").objectReferenceValue = unlockAllButton;
            managerObjectSerialized.FindProperty("resetProgressButton").objectReferenceValue = resetProgressButton;
            managerObjectSerialized.FindProperty("statusText").objectReferenceValue = statusText;
            managerObjectSerialized.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(manager);
            Selection.activeGameObject = managerObject;
            EditorGUIUtility.PingObject(managerObject);
        }

        private static void ConfigureScrollView(RectTransform rect)
        {
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(0f, 0f);
            rect.offsetMax = new Vector2(0f, -120f);
        }

        private static Button CreateButton(Transform parent, string name, string label, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject root = GetOrCreateUiChild(parent, name);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            Image image = root.GetComponent<Image>();
            if (image == null)
            {
                image = Undo.AddComponent<Image>(root);
            }

            image.color = new Color(0.14f, 0.14f, 0.14f, 0.9f);

            Button button = root.GetComponent<Button>();
            if (button == null)
            {
                button = Undo.AddComponent<Button>(root);
            }

            TMP_Text labelText = CreateLabel(root.transform, "Label", label, 24f, Vector2.zero, size);
            RectTransform labelRect = labelText.rectTransform;
            StretchFull(labelRect);
            labelText.alignment = TextAlignmentOptions.Center;
            return button;
        }

        private static TMP_Text CreateLabel(Transform parent, string name, string textValue, float fontSize, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject root = GetOrCreateUiChild(parent, name);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            TextMeshProUGUI text = root.GetComponent<TextMeshProUGUI>();
            if (text == null)
            {
                text = Undo.AddComponent<TextMeshProUGUI>(root);
            }

            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = fontSize;
            text.color = Color.white;
            text.text = textValue;
            text.raycastTarget = false;
            return text;
        }

        private static GameObject GetOrCreateUiChild(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject child = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
            Undo.RegisterCreatedObjectUndo(child, "Create Level Select UI Object");
            child.transform.SetParent(parent);
            child.transform.localScale = Vector3.one;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localPosition = Vector3.zero;
            return child;
        }

        private static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
        {
            T existing = gameObject.GetComponent<T>();
            return existing != null ? existing : Undo.AddComponent<T>(gameObject);
        }

        private static void StretchFull(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }
}
