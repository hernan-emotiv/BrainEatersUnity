using BrainEaters.GameFlow;
using BrainEaters.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace BrainEaters.EditorTools
{
    public static class EndGamePanelsBuilder
    {
        [MenuItem("Brain Eaters/Create End Game Panels")]
        public static void CreateEndGamePanels()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("Create a Canvas first, then run Create End Game Panels.");
                return;
            }

            if (TMP_Settings.defaultFontAsset == null)
            {
                Debug.LogError("TMP default font asset is missing. Import TMP Essential Resources first.");
                return;
            }

            GameObject overlayRoot = GetOrCreateUIChild(canvas.transform, "EndGamePanelsRoot");
            RectTransform overlayRect = overlayRoot.GetComponent<RectTransform>();
            StretchFullScreen(overlayRect);

            GameObject winPanel = CreateOrUpdatePanel(overlayRoot.transform, "WinPanel", new Color(0.08f, 0.2f, 0.12f, 0.88f));
            UiVisibilityAnimatorUtility.EnsureVisibilityAnimator(winPanel);
            TMP_Text winTitle = CreateOrUpdateLabel(winPanel.transform, "TitleText", "YOU SURVIVED", 42f, new Vector2(0f, -80f), new Vector2(700f, 60f));
            TMP_Text winReport = CreateOrUpdateLabel(winPanel.transform, "ReportText", "Total Enemies Eliminated: 0", 28f, new Vector2(0f, -170f), new Vector2(700f, 260f));
            Button winRetryButton = CreateOrUpdateButton(winPanel.transform, "RetryButton", "Retry", new Vector2(-120f, -470f), new Vector2(220f, 56f));
            Button winBackButton = CreateOrUpdateButton(winPanel.transform, "BackToMenuButton", "Back to Menu", new Vector2(120f, -470f), new Vector2(220f, 56f));

            GameObject losePanel = CreateOrUpdatePanel(overlayRoot.transform, "LosePanel", new Color(0.2f, 0.08f, 0.08f, 0.9f));
            UiVisibilityAnimatorUtility.EnsureVisibilityAnimator(losePanel);
            TMP_Text loseTitle = CreateOrUpdateLabel(losePanel.transform, "TitleText", "YOU WERE OVERWHELMED", 42f, new Vector2(0f, -80f), new Vector2(700f, 60f));
            TMP_Text loseReport = CreateOrUpdateLabel(losePanel.transform, "ReportText", "Total Enemies Eliminated: 0", 28f, new Vector2(0f, -170f), new Vector2(700f, 260f));
            Button loseRetryButton = CreateOrUpdateButton(losePanel.transform, "RetryButton", "Retry", new Vector2(-120f, -470f), new Vector2(220f, 56f));
            Button loseBackButton = CreateOrUpdateButton(losePanel.transform, "BackToMenuButton", "Back to Menu", new Vector2(120f, -470f), new Vector2(220f, 56f));

            EndGamePanelController controller = EnsureControllerOnRoot(overlayRoot);
            AssignControllerReferences(
                controller,
                winPanel,
                losePanel,
                winTitle,
                winReport,
                loseTitle,
                loseReport,
                winRetryButton,
                loseRetryButton,
                winBackButton,
                loseBackButton,
                Object.FindFirstObjectByType<GameManager>());
            RemoveMisplacedControllers(canvas.gameObject, overlayRoot);

            winPanel.SetActive(false);
            losePanel.SetActive(false);

            EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
            Selection.activeGameObject = overlayRoot;
            EditorGUIUtility.PingObject(overlayRoot);
        }

        [MenuItem("Brain Eaters/UI/Repair End Game Panel Controller In Current Scene")]
        public static void RepairEndGamePanelControllerInCurrentScene()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                Debug.LogError("No Canvas found in the current scene.");
                return;
            }

            Transform rootTransform = canvas.transform.Find("EndGamePanelsRoot");
            if (rootTransform == null)
            {
                Debug.LogError("No EndGamePanelsRoot found under the current scene Canvas.");
                return;
            }

            EndGamePanelController controller = EnsureControllerOnRoot(rootTransform.gameObject);
            AssignControllerReferencesFromHierarchy(controller, Object.FindFirstObjectByType<GameManager>());
            RemoveMisplacedControllers(canvas.gameObject, rootTransform.gameObject);
            EditorSceneManager.MarkSceneDirty(canvas.gameObject.scene);
            Selection.activeGameObject = rootTransform.gameObject;
            EditorGUIUtility.PingObject(rootTransform.gameObject);
            Debug.Log("Repaired EndGamePanelController on EndGamePanelsRoot in the current scene.", rootTransform.gameObject);
        }

        [MenuItem("Brain Eaters/UI/Repair End Game Panels Prefab")]
        public static void RepairEndGamePanelsPrefab()
        {
            const string prefabPath = "Assets/BrainEaters/Prefabs/UI/EndGamePanelsRoot.prefab";
            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(prefabPath);
            try
            {
                EndGamePanelController controller = EnsureControllerOnRoot(prefabRoot);
                AssignControllerReferencesFromHierarchy(controller, null);
                PrefabUtility.SaveAsPrefabAsset(prefabRoot, prefabPath);
                Debug.Log($"Repaired EndGamePanelController references in {prefabPath}.", prefabRoot);
            }
            finally
            {
                PrefabUtility.UnloadPrefabContents(prefabRoot);
            }
        }

        private static EndGamePanelController EnsureControllerOnRoot(GameObject overlayRoot)
        {
            EndGamePanelController controller = overlayRoot.GetComponent<EndGamePanelController>();
            if (controller == null)
            {
                controller = Undo.AddComponent<EndGamePanelController>(overlayRoot);
            }

            return controller;
        }

        private static void AssignControllerReferencesFromHierarchy(EndGamePanelController controller, GameManager gameManager)
        {
            Transform root = controller.transform;
            GameObject winPanel = root.Find("WinPanel")?.gameObject;
            GameObject losePanel = root.Find("LosePanel")?.gameObject;

            AssignControllerReferences(
                controller,
                winPanel,
                losePanel,
                FindChildComponent<TMP_Text>(winPanel, "TitleText"),
                FindChildComponent<TMP_Text>(winPanel, "ReportText"),
                FindChildComponent<TMP_Text>(losePanel, "TitleText"),
                FindChildComponent<TMP_Text>(losePanel, "ReportText"),
                FindChildComponent<Button>(winPanel, "RetryButton"),
                FindChildComponent<Button>(losePanel, "RetryButton"),
                FindChildComponent<Button>(winPanel, "BackToMenuButton"),
                FindChildComponent<Button>(losePanel, "BackToMenuButton"),
                gameManager);
        }

        private static void AssignControllerReferences(
            EndGamePanelController controller,
            GameObject winPanel,
            GameObject losePanel,
            TMP_Text winTitle,
            TMP_Text winReport,
            TMP_Text loseTitle,
            TMP_Text loseReport,
            Button winRetryButton,
            Button loseRetryButton,
            Button winBackButton,
            Button loseBackButton,
            GameManager gameManager)
        {
            SerializedObject serializedObject = new SerializedObject(controller);
            serializedObject.FindProperty("gameManager").objectReferenceValue = gameManager;
            serializedObject.FindProperty("winPanel").objectReferenceValue = winPanel;
            serializedObject.FindProperty("losePanel").objectReferenceValue = losePanel;
            serializedObject.FindProperty("winTitleText").objectReferenceValue = winTitle;
            serializedObject.FindProperty("winReportText").objectReferenceValue = winReport;
            serializedObject.FindProperty("loseTitleText").objectReferenceValue = loseTitle;
            serializedObject.FindProperty("loseReportText").objectReferenceValue = loseReport;
            serializedObject.FindProperty("winRetryButton").objectReferenceValue = winRetryButton;
            serializedObject.FindProperty("loseRetryButton").objectReferenceValue = loseRetryButton;
            serializedObject.FindProperty("winBackToMenuButton").objectReferenceValue = winBackButton;
            serializedObject.FindProperty("loseBackToMenuButton").objectReferenceValue = loseBackButton;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(controller);
        }

        private static T FindChildComponent<T>(GameObject parent, string childName) where T : Component
        {
            if (parent == null)
            {
                return null;
            }

            return parent.transform.Find(childName)?.GetComponent<T>();
        }

        private static void RemoveMisplacedControllers(GameObject canvasObject, GameObject overlayRoot)
        {
            EndGamePanelController[] controllers = canvasObject.GetComponentsInChildren<EndGamePanelController>(true);
            foreach (EndGamePanelController controller in controllers)
            {
                if (controller.gameObject == overlayRoot)
                {
                    continue;
                }

                Undo.DestroyObjectImmediate(controller);
            }
        }

        private static GameObject CreateOrUpdatePanel(Transform parent, string objectName, Color backgroundColor)
        {
            GameObject panel = GetOrCreateUIChild(parent, objectName);
            RectTransform rectTransform = panel.GetComponent<RectTransform>();
            StretchFullScreen(rectTransform);

            Image image = panel.GetComponent<Image>();
            if (image == null)
            {
                image = Undo.AddComponent<Image>(panel);
            }

            image.color = backgroundColor;
            return panel;
        }

        private static TMP_Text CreateOrUpdateLabel(Transform parent, string objectName, string defaultText, float fontSize, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject textObject = GetOrCreateUIChild(parent, objectName);
            RectTransform rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 1f);
            rectTransform.anchorMax = new Vector2(0.5f, 1f);
            rectTransform.pivot = new Vector2(0.5f, 1f);
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;

            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            if (text == null)
            {
                text = Undo.AddComponent<TextMeshProUGUI>(textObject);
            }

            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.text = defaultText;
            text.raycastTarget = false;
            return text;
        }

        private static Button CreateOrUpdateButton(Transform parent, string objectName, string labelText, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject buttonObject = GetOrCreateUIChild(parent, objectName);
            RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 1f);
            rectTransform.anchorMax = new Vector2(0.5f, 1f);
            rectTransform.pivot = new Vector2(0.5f, 1f);
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;

            Image image = buttonObject.GetComponent<Image>();
            if (image == null)
            {
                image = Undo.AddComponent<Image>(buttonObject);
            }

            image.color = new Color(0.18f, 0.14f, 0.08f, 0.95f);

            Button button = buttonObject.GetComponent<Button>();
            if (button == null)
            {
                button = Undo.AddComponent<Button>(buttonObject);
            }

            TMP_Text label = CreateOrUpdateLabel(buttonObject.transform, "Label", labelText, 24f, Vector2.zero, size);
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;

            RectTransform labelRect = label.GetComponent<RectTransform>();
            StretchFullScreen(labelRect);

            return button;
        }

        private static GameObject GetOrCreateUIChild(Transform parent, string childName)
        {
            Transform existingChild = parent.Find(childName);
            if (existingChild != null)
            {
                return existingChild.gameObject;
            }

            GameObject child = new GameObject(childName, typeof(RectTransform), typeof(CanvasRenderer));
            Undo.RegisterCreatedObjectUndo(child, "Create End Game UI Object");
            child.transform.SetParent(parent);
            child.transform.localScale = Vector3.one;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localPosition = Vector3.zero;
            return child;
        }

        private static void StretchFullScreen(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }
}
