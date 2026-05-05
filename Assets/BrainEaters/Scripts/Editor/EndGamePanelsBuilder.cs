using BrainEaters.Configs;
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
        private const string ZombieIconPath = "Assets/BrainEaters/Textures/UI references/zombiesmallhead.png";
        private const string SpecialIconPath = "Assets/BrainEaters/Textures/UI references/franksmallhead.png";
        private const string BossIconPath = "Assets/BrainEaters/Textures/UI references/wolfsmallhead.png";

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

            GameObject winPanel = CreateOrUpdatePanel(overlayRoot.transform, "WinPanel", new Color(0f, 0f, 0f, 0.72f));
            UiVisibilityAnimatorUtility.EnsureVisibilityAnimator(winPanel);
            ApplyWoodBoard(winPanel.transform);
            TMP_Text winTitle = CreateOrUpdateLabel(winPanel.transform, "TitleText", "VICTORY!", 54f, new Vector2(0f, 190f), new Vector2(760f, 80f));
            TMP_Text winReport = CreateOrUpdateLabel(winPanel.transform, "ReportText", "Total Enemies Eliminated: 0", 30f, new Vector2(0f, 28f), new Vector2(760f, 270f));
            winReport.gameObject.SetActive(false);
            EndGameScoreReportView winScoreReport = CreateOrUpdateScoreReport(winPanel.transform);
            Button winRetryButton = CreateOrUpdateButton(winPanel.transform, "RetryButton", "Retry", new Vector2(-150f, -250f), new Vector2(280f, 92f), UiSpriteUtility.WoodButtonGreenPath);
            Button winBackButton = CreateOrUpdateButton(winPanel.transform, "BackToMenuButton", "Back to Menu", new Vector2(150f, -250f), new Vector2(280f, 92f), UiSpriteUtility.WoodButtonYellowPath);

            GameObject losePanel = CreateOrUpdatePanel(overlayRoot.transform, "LosePanel", new Color(0f, 0f, 0f, 0.72f));
            UiVisibilityAnimatorUtility.EnsureVisibilityAnimator(losePanel);
            ApplyWoodBoard(losePanel.transform);
            TMP_Text loseTitle = CreateOrUpdateLabel(losePanel.transform, "TitleText", "GAME OVER", 54f, new Vector2(0f, 190f), new Vector2(820f, 80f));
            TMP_Text loseReport = CreateOrUpdateLabel(losePanel.transform, "ReportText", "Total Enemies Eliminated: 0", 30f, new Vector2(0f, 28f), new Vector2(760f, 270f));
            loseReport.gameObject.SetActive(false);
            EndGameScoreReportView loseScoreReport = CreateOrUpdateScoreReport(losePanel.transform);
            Button loseRetryButton = CreateOrUpdateButton(losePanel.transform, "RetryButton", "Retry", new Vector2(-150f, -250f), new Vector2(280f, 92f), UiSpriteUtility.WoodButtonGreenPath);
            Button loseBackButton = CreateOrUpdateButton(losePanel.transform, "BackToMenuButton", "Back to Menu", new Vector2(150f, -250f), new Vector2(280f, 92f), UiSpriteUtility.WoodButtonYellowPath);

            EndGamePanelController controller = EnsureControllerOnRoot(overlayRoot);
            AssignControllerReferences(
                controller,
                winPanel,
                losePanel,
                winTitle,
                winReport,
                loseTitle,
                loseReport,
                winScoreReport,
                loseScoreReport,
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
            ApplyEndGamePanelSkin(rootTransform.gameObject);
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
                ApplyEndGamePanelSkin(prefabRoot);
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
                FindChildComponent<EndGameScoreReportView>(winPanel, "ScoreReport"),
                FindChildComponent<EndGameScoreReportView>(losePanel, "ScoreReport"),
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
            EndGameScoreReportView winScoreReport,
            EndGameScoreReportView loseScoreReport,
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
            serializedObject.FindProperty("winScoreReportView").objectReferenceValue = winScoreReport;
            serializedObject.FindProperty("loseScoreReportView").objectReferenceValue = loseScoreReport;
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

        private static void ApplyEndGamePanelSkin(GameObject overlayRoot)
        {
            if (overlayRoot == null)
            {
                return;
            }

            Transform winPanel = overlayRoot.transform.Find("WinPanel");
            Transform losePanel = overlayRoot.transform.Find("LosePanel");
            if (winPanel != null)
            {
                ApplyDimBackground(winPanel.gameObject);
                ApplyWoodBoard(winPanel);
                SetChildRect(winPanel, "TitleText", new Vector2(0f, 190f), new Vector2(760f, 80f));
                HideLegacyReportText(winPanel);
                CreateOrUpdateScoreReport(winPanel);
                SetChildRect(winPanel, "RetryButton", new Vector2(-150f, -250f), new Vector2(280f, 92f));
                SetChildRect(winPanel, "BackToMenuButton", new Vector2(150f, -250f), new Vector2(280f, 92f));
                ApplyButtonSkin(FindChildComponent<Button>(winPanel.gameObject, "RetryButton"), UiSpriteUtility.WoodButtonGreenPath);
                ApplyButtonSkin(FindChildComponent<Button>(winPanel.gameObject, "BackToMenuButton"), UiSpriteUtility.WoodButtonYellowPath);
            }

            if (losePanel != null)
            {
                ApplyDimBackground(losePanel.gameObject);
                ApplyWoodBoard(losePanel);
                SetChildRect(losePanel, "TitleText", new Vector2(0f, 190f), new Vector2(820f, 80f));
                HideLegacyReportText(losePanel);
                CreateOrUpdateScoreReport(losePanel);
                SetChildRect(losePanel, "RetryButton", new Vector2(-150f, -250f), new Vector2(280f, 92f));
                SetChildRect(losePanel, "BackToMenuButton", new Vector2(150f, -250f), new Vector2(280f, 92f));
                ApplyButtonSkin(FindChildComponent<Button>(losePanel.gameObject, "RetryButton"), UiSpriteUtility.WoodButtonGreenPath);
                ApplyButtonSkin(FindChildComponent<Button>(losePanel.gameObject, "BackToMenuButton"), UiSpriteUtility.WoodButtonYellowPath);
            }
        }

        private static void ApplyDimBackground(GameObject panel)
        {
            Image image = panel.GetComponent<Image>();
            if (image == null)
            {
                image = Undo.AddComponent<Image>(panel);
            }

            image.sprite = null;
            image.color = new Color(0f, 0f, 0f, 0.72f);
        }

        private static void ApplyWoodBoard(Transform panel)
        {
            GameObject board = GetOrCreateUIChild(panel, "WoodBoard");
            RectTransform boardRect = board.GetComponent<RectTransform>();
            boardRect.anchorMin = new Vector2(0.5f, 0.5f);
            boardRect.anchorMax = new Vector2(0.5f, 0.5f);
            boardRect.pivot = new Vector2(0.5f, 0.5f);
            boardRect.anchoredPosition = Vector2.zero;
            boardRect.sizeDelta = new Vector2(980f, 720f);

            Image boardImage = board.GetComponent<Image>();
            if (boardImage == null)
            {
                boardImage = Undo.AddComponent<Image>(board);
            }

            boardImage.sprite = UiSpriteUtility.EnsureSprite(UiSpriteUtility.WoodBorderPath);
            boardImage.color = Color.white;
            boardImage.preserveAspect = true;
            boardImage.raycastTarget = false;
            board.transform.SetAsFirstSibling();
        }

        private static EndGameScoreReportView CreateOrUpdateScoreReport(Transform parent)
        {
            GameObject reportRoot = GetOrCreateUIChild(parent, "ScoreReport");
            RectTransform reportRect = reportRoot.GetComponent<RectTransform>();
            reportRect.anchorMin = new Vector2(0.5f, 0.5f);
            reportRect.anchorMax = new Vector2(0.5f, 0.5f);
            reportRect.pivot = new Vector2(0.5f, 0.5f);
            reportRect.anchoredPosition = new Vector2(0f, -40f);
            reportRect.sizeDelta = new Vector2(720f, 360f);

            GameObject rowsContainer = GetOrCreateUIChild(reportRoot.transform, "Rows");
            RectTransform rowsRect = rowsContainer.GetComponent<RectTransform>();
            rowsRect.anchorMin = new Vector2(0.5f, 1f);
            rowsRect.anchorMax = new Vector2(0.5f, 1f);
            rowsRect.pivot = new Vector2(0.5f, 1f);
            rowsRect.anchoredPosition = new Vector2(0f, 0f);
            rowsRect.sizeDelta = new Vector2(640f, 220f);

            VerticalLayoutGroup layout = rowsContainer.GetComponent<VerticalLayoutGroup>();
            if (layout == null)
            {
                layout = Undo.AddComponent<VerticalLayoutGroup>(rowsContainer);
            }

            layout.spacing = 4f;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = false;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;

            GameObject rowTemplate = CreateOrUpdateScoreRowTemplate(rowsContainer.transform);
            TMP_Text totalScoreText = CreateOrUpdateLabel(reportRoot.transform, "TotalScoreText", "SCORE 0", 60f, new Vector2(0f, -120f), new Vector2(640f, 88f));

            EndGameScoreReportView view = reportRoot.GetComponent<EndGameScoreReportView>();
            if (view == null)
            {
                view = Undo.AddComponent<EndGameScoreReportView>(reportRoot);
            }

            SerializedObject serialized = new SerializedObject(view);
            serialized.FindProperty("rowsContainer").objectReferenceValue = rowsRect;
            serialized.FindProperty("rowTemplate").objectReferenceValue = rowTemplate;
            serialized.FindProperty("totalScoreText").objectReferenceValue = totalScoreText;
            SerializedProperty icons = serialized.FindProperty("enemyIcons");
            icons.arraySize = 3;
            AssignIconBinding(icons.GetArrayElementAtIndex(0), EnemyType.Zombie, ZombieIconPath);
            AssignIconBinding(icons.GetArrayElementAtIndex(1), EnemyType.Special, SpecialIconPath);
            AssignIconBinding(icons.GetArrayElementAtIndex(2), EnemyType.Boss, BossIconPath);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(view);
            return view;
        }

        private static GameObject CreateOrUpdateScoreRowTemplate(Transform parent)
        {
            GameObject row = GetOrCreateUIChild(parent, "ScoreRowTemplate");
            RectTransform rowRect = row.GetComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(640f, 68f);

            LayoutElement layoutElement = row.GetComponent<LayoutElement>();
            if (layoutElement == null)
            {
                layoutElement = Undo.AddComponent<LayoutElement>(row);
            }

            layoutElement.preferredWidth = 640f;
            layoutElement.preferredHeight = 68f;

            Image icon = CreateOrUpdateImage(row.transform, "Icon", null, new Vector2(-252f, 0f), new Vector2(64f, 64f));
            icon.raycastTarget = false;

            TMP_Text formulaText = CreateOrUpdateLabel(row.transform, "FormulaText", "10 points x0", 44f, new Vector2(-46f, 0f), new Vector2(370f, 66f));
            formulaText.alignment = TextAlignmentOptions.MidlineLeft;

            TMP_Text scoreText = CreateOrUpdateLabel(row.transform, "ScoreText", "0", 44f, new Vector2(250f, 0f), new Vector2(160f, 66f));
            scoreText.alignment = TextAlignmentOptions.MidlineRight;

            row.SetActive(false);
            return row;
        }

        private static Image CreateOrUpdateImage(Transform parent, string objectName, Sprite sprite, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject imageObject = GetOrCreateUIChild(parent, objectName);
            RectTransform rect = imageObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            Image image = imageObject.GetComponent<Image>();
            if (image == null)
            {
                image = Undo.AddComponent<Image>(imageObject);
            }

            image.sprite = sprite;
            image.color = Color.white;
            image.preserveAspect = true;
            return image;
        }

        private static void AssignIconBinding(SerializedProperty binding, EnemyType enemyType, string iconPath)
        {
            binding.FindPropertyRelative("enemyType").enumValueIndex = (int)enemyType;
            binding.FindPropertyRelative("icon").objectReferenceValue = UiSpriteUtility.EnsureSprite(iconPath);
        }

        private static void HideLegacyReportText(Transform panel)
        {
            Transform reportText = panel.Find("ReportText");
            if (reportText != null)
            {
                reportText.gameObject.SetActive(false);
            }
        }

        private static TMP_Text CreateOrUpdateLabel(Transform parent, string objectName, string defaultText, float fontSize, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject textObject = GetOrCreateUIChild(parent, objectName);
            RectTransform rectTransform = textObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;

            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            if (text == null)
            {
                text = Undo.AddComponent<TextMeshProUGUI>(textObject);
            }

            text.font = UiSpriteUtility.LoadHudFont();
            text.fontSize = fontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.text = defaultText;
            text.raycastTarget = false;
            return text;
        }

        private static Button CreateOrUpdateButton(Transform parent, string objectName, string labelText, Vector2 anchoredPosition, Vector2 size, string spritePath)
        {
            GameObject buttonObject = GetOrCreateUIChild(parent, objectName);
            RectTransform rectTransform = buttonObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;

            Button button = buttonObject.GetComponent<Button>();
            if (button == null)
            {
                button = Undo.AddComponent<Button>(buttonObject);
            }

            ApplyButtonSkin(button, spritePath);

            TMP_Text label = CreateOrUpdateLabel(buttonObject.transform, "Label", labelText, 32f, Vector2.zero, size);
            label.alignment = TextAlignmentOptions.Center;
            label.fontStyle = FontStyles.Bold;
            label.raycastTarget = false;

            RectTransform labelRect = label.GetComponent<RectTransform>();
            StretchFullScreen(labelRect);

            return button;
        }

        private static void ApplyButtonSkin(Button button, string spritePath)
        {
            if (button == null)
            {
                return;
            }

            Image image = button.GetComponent<Image>();
            if (image == null)
            {
                image = Undo.AddComponent<Image>(button.gameObject);
            }

            image.sprite = UiSpriteUtility.EnsureSprite(spritePath);
            image.color = Color.white;
            image.preserveAspect = true;
            button.targetGraphic = image;
        }

        private static void SetChildRect(Transform parent, string childName, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            RectTransform rect = parent.Find(childName)?.GetComponent<RectTransform>();
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = sizeDelta;

            TMP_Text text = rect.GetComponent<TMP_Text>();
            if (text != null)
            {
                text.font = UiSpriteUtility.LoadHudFont();
                text.color = Color.white;
                text.raycastTarget = false;
            }
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
