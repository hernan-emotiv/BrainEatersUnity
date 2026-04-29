using BrainEaters.LevelSelect;
using BrainEaters.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace BrainEaters.EditorTools
{
    public static class LevelSelectSceneBuilder
    {
        private const string CampaignConfigPath = "Assets/BrainEaters/Data/CampaignConfig.asset";
        private const string MainMenuReferencePath = "Assets/BrainEaters/Textures/UI references/1st scene.png";
        private const string TutorialReferencePath = "Assets/BrainEaters/Textures/UI references/tutorial popup.png";
        private const string HowToPlayReferencePath = "Assets/BrainEaters/Textures/UI references/how to play popup.png";
        private const string YellowButtonPath = "Assets/BrainEaters/Textures/UI references/Generated/button_yellow.png";
        private const string BlueButtonPath = "Assets/BrainEaters/Textures/UI references/Generated/button_blue.png";
        private const string GreenButtonPath = "Assets/BrainEaters/Textures/UI references/Generated/button_green.png";
        private const string ButtonFontPath = "Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Anton SDF.asset";
        private const string UiDataFolderPath = "Assets/BrainEaters/Data/UI";
        private const string ButtonTextStylePath = UiDataFolderPath + "/ButtonTextStyle_BrainEaters.asset";

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

            MainMenuFlowController mainMenuFlow = managerObject.GetComponent<MainMenuFlowController>();
            if (mainMenuFlow == null)
            {
                mainMenuFlow = Undo.AddComponent<MainMenuFlowController>(managerObject);
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

            Object campaignConfig = AssetDatabase.LoadAssetAtPath<Object>(CampaignConfigPath);

            SerializedObject managerObjectSerialized = new SerializedObject(manager);
            managerObjectSerialized.FindProperty("campaignConfig").objectReferenceValue = campaignConfig;
            managerObjectSerialized.FindProperty("unlockAllButton").objectReferenceValue = unlockAllButton;
            managerObjectSerialized.FindProperty("resetProgressButton").objectReferenceValue = resetProgressButton;
            managerObjectSerialized.FindProperty("statusText").objectReferenceValue = statusText;
            managerObjectSerialized.ApplyModifiedPropertiesWithoutUndo();

            CreateMainMenuFlow(managerObject.transform, safeAreaRoot, mainMenuFlow, campaignConfig);

            EditorUtility.SetDirty(manager);
            EditorUtility.SetDirty(mainMenuFlow);
            Selection.activeGameObject = managerObject;
            EditorGUIUtility.PingObject(managerObject);
        }

        [MenuItem("Brain Eaters/UI/Apply Menu Button Style In Current Scene")]
        public static void ApplyMenuButtonStyleInCurrentScene()
        {
            Sprite yellowButtonSprite = EnsureSprite(YellowButtonPath);
            Sprite blueButtonSprite = EnsureSprite(BlueButtonPath);
            Sprite greenButtonSprite = EnsureSprite(GreenButtonPath);
            ButtonTextStyle buttonTextStyle = EnsureButtonTextStyle();

            Button[] buttons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            int styledCount = 0;
            foreach (Button button in buttons)
            {
                if (button == null || !TryGetSpriteForButton(button.name, yellowButtonSprite, blueButtonSprite, greenButtonSprite, out Sprite sprite))
                {
                    continue;
                }

                Undo.RecordObject(button.gameObject, "Apply Menu Button Style");
                ApplyButtonImageStyle(button, sprite);

                TMP_Text[] labels = button.GetComponentsInChildren<TMP_Text>(true);
                foreach (TMP_Text label in labels)
                {
                    Undo.RecordObject(label, "Apply Menu Button Text Style");
                    ApplyButtonTextStyle(label, buttonTextStyle);
                    EditorUtility.SetDirty(label);
                }

                EditorUtility.SetDirty(button.gameObject);
                styledCount++;
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log($"Applied Brain Eaters menu button style to {styledCount} button(s) in the current scene.");
        }

        private static void CreateMainMenuFlow(Transform managerRoot, GameObject levelSelectRoot, MainMenuFlowController flowController, Object campaignConfig)
        {
            Sprite mainMenuReference = EnsureSprite(MainMenuReferencePath);
            Sprite tutorialReference = EnsureSprite(TutorialReferencePath);
            Sprite howToPlayReference = EnsureSprite(HowToPlayReferencePath);
            Sprite yellowButtonSprite = EnsureSprite(YellowButtonPath);
            Sprite greenButtonSprite = EnsureSprite(GreenButtonPath);
            ButtonTextStyle buttonTextStyle = EnsureButtonTextStyle();

            GameObject mainMenuRoot = GetOrCreateUiChild(managerRoot, "MainMenuRoot");
            StretchFull(mainMenuRoot.GetComponent<RectTransform>());
            Image mainMenuImage = GetOrAddComponent<Image>(mainMenuRoot);
            mainMenuImage.sprite = mainMenuReference;
            mainMenuImage.color = Color.white;
            mainMenuImage.preserveAspect = true;
            mainMenuImage.raycastTarget = false;

            Button howButton = CreateMenuButton(mainMenuRoot.transform, "HowButton", "HOW", new Vector2(-220f, 78f), yellowButtonSprite, buttonTextStyle);
            Button playButton = CreateMenuButton(mainMenuRoot.transform, "PlayButton", "PLAY", new Vector2(220f, 78f), greenButtonSprite, buttonTextStyle);

            GameObject tutorialPopupRoot = CreateReferencePopup(managerRoot, "TutorialPopupRoot", tutorialReference);
            UiVisibilityAnimatorUtility.EnsureVisibilityAnimator(tutorialPopupRoot);
            Button tutorialStartButton = CreateMenuButton(tutorialPopupRoot.transform, "StartButton", "START", new Vector2(0f, 76f), greenButtonSprite, buttonTextStyle);

            GameObject howToPlayPopupRoot = CreateReferencePopup(managerRoot, "HowToPlayPopupRoot", howToPlayReference);
            UiVisibilityAnimatorUtility.EnsureVisibilityAnimator(howToPlayPopupRoot);
            Button howBackButton = CreateMenuButton(howToPlayPopupRoot.transform, "BackButton", "BACK", new Vector2(0f, 76f), yellowButtonSprite, buttonTextStyle);

            SerializedObject flowObject = new SerializedObject(flowController);
            flowObject.FindProperty("campaignConfig").objectReferenceValue = campaignConfig;
            flowObject.FindProperty("mainMenuRoot").objectReferenceValue = mainMenuRoot;
            flowObject.FindProperty("levelSelectRoot").objectReferenceValue = levelSelectRoot;
            flowObject.FindProperty("tutorialPopupRoot").objectReferenceValue = tutorialPopupRoot;
            flowObject.FindProperty("howToPlayPopupRoot").objectReferenceValue = howToPlayPopupRoot;
            flowObject.FindProperty("playButton").objectReferenceValue = playButton;
            flowObject.FindProperty("howButton").objectReferenceValue = howButton;
            flowObject.FindProperty("tutorialStartButton").objectReferenceValue = tutorialStartButton;
            flowObject.FindProperty("howBackButton").objectReferenceValue = howBackButton;
            flowObject.ApplyModifiedPropertiesWithoutUndo();

            mainMenuRoot.SetActive(true);
            levelSelectRoot.SetActive(false);
            tutorialPopupRoot.SetActive(false);
            howToPlayPopupRoot.SetActive(false);
        }

        private static GameObject CreateReferencePopup(Transform parent, string name, Sprite referenceSprite)
        {
            GameObject root = GetOrCreateUiChild(parent, name);
            StretchFull(root.GetComponent<RectTransform>());

            Image image = GetOrAddComponent<Image>(root);
            image.sprite = referenceSprite;
            image.color = Color.white;
            image.preserveAspect = true;
            image.raycastTarget = true;
            return root;
        }

        private static Button CreateMenuButton(Transform parent, string name, string label, Vector2 anchoredPosition, Sprite sprite, ButtonTextStyle buttonTextStyle)
        {
            GameObject root = GetOrCreateUiChild(parent, name);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(300f, 110f);

            Image image = root.GetComponent<Image>();
            if (image == null)
            {
                image = Undo.AddComponent<Image>(root);
            }

            Button button = root.GetComponent<Button>();
            if (button == null)
            {
                button = Undo.AddComponent<Button>(root);
            }

            ApplyButtonImageStyle(button, sprite);

            TMP_Text labelText = CreateLabel(root.transform, "Label", label, 46f, new Vector2(0f, 6f), rect.sizeDelta);
            ApplyButtonTextStyle(labelText, buttonTextStyle);
            return button;
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

        private static Sprite EnsureSprite(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer != null)
            {
                bool importerChanged = false;
                if (importer.textureType != TextureImporterType.Sprite)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spriteImportMode = SpriteImportMode.Single;
                    importerChanged = true;
                }

                if (importer.mipmapEnabled)
                {
                    importer.mipmapEnabled = false;
                    importerChanged = true;
                }

                if (!importer.alphaIsTransparency)
                {
                    importer.alphaIsTransparency = true;
                    importerChanged = true;
                }

                if (importerChanged)
                {
                    importer.SaveAndReimport();
                }
            }

            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (sprite == null)
            {
                Debug.LogWarning($"Could not load UI sprite at {path}.");
            }

            return sprite;
        }

        [MenuItem("Brain Eaters/UI/Create Or Refresh Button Text Style Preset")]
        public static void CreateOrRefreshButtonTextStylePreset()
        {
            ButtonTextStyle style = EnsureButtonTextStyle();
            EditorUtility.SetDirty(style);
            Selection.activeObject = style;
            EditorGUIUtility.PingObject(style);
        }

        [MenuItem("Brain Eaters/UI/Apply Text Style To Selected TMP Text")]
        public static void ApplyTextStyleToSelectedTmpText()
        {
            ApplyTextStyleToSelectedTmpText(false);
        }

        [MenuItem("Brain Eaters/UI/Apply Text Style To Selected TMP Text Uppercase")]
        public static void ApplyTextStyleToSelectedTmpTextUppercase()
        {
            ApplyTextStyleToSelectedTmpText(true);
        }

        private static TMP_FontAsset LoadButtonFont()
        {
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(ButtonFontPath);
            return font != null ? font : TMP_Settings.defaultFontAsset;
        }

        private static ButtonTextStyle EnsureButtonTextStyle()
        {
            ButtonTextStyle style = AssetDatabase.LoadAssetAtPath<ButtonTextStyle>(ButtonTextStylePath);
            if (style != null)
            {
                return style;
            }

            if (!AssetDatabase.IsValidFolder(UiDataFolderPath))
            {
                if (!AssetDatabase.IsValidFolder("Assets/BrainEaters/Data"))
                {
                    AssetDatabase.CreateFolder("Assets/BrainEaters", "Data");
                }

                AssetDatabase.CreateFolder("Assets/BrainEaters/Data", "UI");
            }

            style = ScriptableObject.CreateInstance<ButtonTextStyle>();
            style.ConfigureFont(LoadButtonFont());
            AssetDatabase.CreateAsset(style, ButtonTextStylePath);
            AssetDatabase.SaveAssets();
            return style;
        }

        private static void ApplyTextStyleToSelectedTmpText(bool forceUppercase)
        {
            ButtonTextStyle style = EnsureButtonTextStyle();
            TMP_Text[] selectedTexts = Selection.GetFiltered<TMP_Text>(SelectionMode.Editable | SelectionMode.Deep);
            if (selectedTexts == null || selectedTexts.Length == 0)
            {
                Debug.LogWarning("Select one or more TextMeshProUGUI/TMP_Text objects before applying the Brain Eaters text style.");
                return;
            }

            foreach (TMP_Text text in selectedTexts)
            {
                Undo.RecordObject(text, "Apply Brain Eaters TMP Text Style");
                style.ApplyTo(text, forceUppercase);
                EditorUtility.SetDirty(text);
            }

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log($"Applied Brain Eaters TMP text style to {selectedTexts.Length} selected text object(s). Uppercase: {forceUppercase}.");
        }

        private static bool TryGetSpriteForButton(string buttonName, Sprite yellowButtonSprite, Sprite blueButtonSprite, Sprite greenButtonSprite, out Sprite sprite)
        {
            string normalizedName = buttonName.ToLowerInvariant();
            if (normalizedName.Contains("play") || normalizedName.Contains("start") || normalizedName.Contains("continue"))
            {
                sprite = greenButtonSprite;
                return true;
            }

            if (normalizedName.Contains("how") || normalizedName.Contains("back") || normalizedName.Contains("retry"))
            {
                sprite = yellowButtonSprite;
                return true;
            }

            if (normalizedName.Contains("level") || normalizedName.Contains("select") || normalizedName.Contains("menu"))
            {
                sprite = blueButtonSprite;
                return true;
            }

            sprite = null;
            return false;
        }

        private static void ApplyButtonImageStyle(Button button, Sprite sprite)
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

            image.sprite = sprite;
            image.color = Color.white;
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
            image.raycastTarget = true;
            button.targetGraphic = image;
        }

        private static void ApplyButtonTextStyle(TMP_Text labelText, ButtonTextStyle buttonTextStyle)
        {
            if (labelText == null)
            {
                return;
            }

            if (buttonTextStyle != null)
            {
                buttonTextStyle.ApplyTo(labelText, true);
            }
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
