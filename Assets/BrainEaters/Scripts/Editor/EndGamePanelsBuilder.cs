using BrainEaters.UI;
using TMPro;
using UnityEditor;
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

            EndGamePanelController controller = canvas.GetComponent<EndGamePanelController>();
            if (controller == null)
            {
                controller = Undo.AddComponent<EndGamePanelController>(canvas.gameObject);
            }

            GameObject overlayRoot = GetOrCreateUIChild(canvas.transform, "EndGamePanelsRoot");
            RectTransform overlayRect = overlayRoot.GetComponent<RectTransform>();
            StretchFullScreen(overlayRect);

            GameObject winPanel = CreateOrUpdatePanel(overlayRoot.transform, "WinPanel", new Color(0.08f, 0.2f, 0.12f, 0.88f));
            TMP_Text winTitle = CreateOrUpdateLabel(winPanel.transform, "TitleText", "YOU SURVIVED", 42f, new Vector2(0f, -80f), new Vector2(700f, 60f));
            TMP_Text winReport = CreateOrUpdateLabel(winPanel.transform, "ReportText", "Total Enemies Eliminated: 0", 28f, new Vector2(0f, -170f), new Vector2(700f, 260f));
            Button winRetryButton = CreateOrUpdateButton(winPanel.transform, "RetryButton", "Retry", new Vector2(-120f, -470f), new Vector2(220f, 56f));
            Button winBackButton = CreateOrUpdateButton(winPanel.transform, "BackToMenuButton", "Back to Menu", new Vector2(120f, -470f), new Vector2(220f, 56f));

            GameObject losePanel = CreateOrUpdatePanel(overlayRoot.transform, "LosePanel", new Color(0.2f, 0.08f, 0.08f, 0.9f));
            TMP_Text loseTitle = CreateOrUpdateLabel(losePanel.transform, "TitleText", "YOU WERE OVERWHELMED", 42f, new Vector2(0f, -80f), new Vector2(700f, 60f));
            TMP_Text loseReport = CreateOrUpdateLabel(losePanel.transform, "ReportText", "Total Enemies Eliminated: 0", 28f, new Vector2(0f, -170f), new Vector2(700f, 260f));
            Button loseRetryButton = CreateOrUpdateButton(losePanel.transform, "RetryButton", "Retry", new Vector2(-120f, -470f), new Vector2(220f, 56f));
            Button loseBackButton = CreateOrUpdateButton(losePanel.transform, "BackToMenuButton", "Back to Menu", new Vector2(120f, -470f), new Vector2(220f, 56f));

            SerializedObject serializedObject = new SerializedObject(controller);
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

            winPanel.SetActive(false);
            losePanel.SetActive(false);

            Selection.activeGameObject = overlayRoot;
            EditorGUIUtility.PingObject(overlayRoot);
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
