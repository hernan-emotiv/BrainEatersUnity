using BrainEaters.Configs;
using BrainEaters.GameFlow;
using BrainEaters.Player;
using BrainEaters.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

namespace BrainEaters.EditorTools
{
    public static class GameplayHudBuilder
    {
        private const string HeartSpritePath = "Assets/BrainEaters/Textures/UI references/heartforgoundonsmall.png";
        private const string BrainSpritePath = "Assets/BrainEaters/Textures/UI references/brain.png";
        private const string MindBarBackgroundPath = "Assets/BrainEaters/Textures/UI references/sliderbarbackground.png";
        private const string MindBarFillPath = "Assets/BrainEaters/Textures/UI references/sliderdisplayforground.png";
        private const string TimerBarPath = "Assets/BrainEaters/Textures/UI references/timer bar.png";
        private const string KillCounterBackgroundPath = "Assets/BrainEaters/Textures/UI references/killcounterbackground crop.png";
        private const string ZombieIconPath = "Assets/BrainEaters/Textures/UI references/zombiesmallhead.png";
        private const string SpecialIconPath = "Assets/BrainEaters/Textures/UI references/franksmallhead.png";
        private const string BossIconPath = "Assets/BrainEaters/Textures/UI references/wolfsmallhead.png";
        private const string HudFontPath = "Assets/TextMesh Pro/Examples & Extras/Resources/Fonts & Materials/Anton SDF.asset";

        [MenuItem("Brain Eaters/UI/Rebuild Gameplay HUD In Current Scene")]
        public static void RebuildGameplayHudInCurrentScene()
        {
            if (TMP_Settings.defaultFontAsset == null)
            {
                Debug.LogError("TMP default font asset is missing. Import TMP Essential Resources first.");
                return;
            }

            Canvas canvas = GetOrCreateCanvas();
            GameObject hudRoot = GetOrCreateHudRoot(canvas.transform);

            GameplayHudController hudController = GetOrAddComponent<GameplayHudController>(canvas.gameObject);
            Transform topRoot = GetOrCreateSection(hudRoot.transform, "Top").transform;
            Transform bottomRoot = GetOrCreateSection(hudRoot.transform, "Bottom").transform;
            Transform bottomLeftRoot = GetOrCreateSection(bottomRoot, "BottomLeft").transform;
            Transform bottomCenterRoot = GetOrCreateSection(bottomRoot, "BottomCenter").transform;
            Transform bottomRightRoot = GetOrCreateSection(bottomRoot, "BottomRight").transform;

            HeartHealthView heartHealthView = CreateHearts(bottomRightRoot);
            ProgressBarView mindPowerBar = CreateMindPower(bottomLeftRoot);
            TMP_Text timerText = CreateTimer(topRoot);
            CreateKillTracker(bottomCenterRoot);

            SerializedObject serializedHud = new SerializedObject(hudController);
            serializedHud.FindProperty("gameManager").objectReferenceValue = Object.FindFirstObjectByType<GameManager>();
            serializedHud.FindProperty("playerHealth").objectReferenceValue = Object.FindFirstObjectByType<PlayerHealth>();
            serializedHud.FindProperty("playerEnergyCharge").objectReferenceValue = Object.FindFirstObjectByType<PlayerEnergyCharge>();
            serializedHud.FindProperty("heartHealthView").objectReferenceValue = heartHealthView;
            serializedHud.FindProperty("bombProgressBar").objectReferenceValue = mindPowerBar;
            serializedHud.FindProperty("timerText").objectReferenceValue = timerText;
            serializedHud.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(hudController);

            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Selection.activeGameObject = hudRoot;
            EditorGUIUtility.PingObject(hudRoot);
        }

        [MenuItem("Brain Eaters/UI/Delete Existing Gameplay HUD Root In Current Scene")]
        public static void DeleteExistingGameplayHudRootInCurrentScene()
        {
            GameObject root = FindExistingHudRoot();
            if (root == null)
            {
                Debug.Log("No Gameplay HUD root found in the current scene.");
                return;
            }

            Undo.DestroyObjectImmediate(root);
            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            Debug.Log("Deleted existing Gameplay HUD root. Run Brain Eaters/UI/Rebuild Gameplay HUD In Current Scene to regenerate it.");
        }

        private static HeartHealthView CreateHearts(Transform parent)
        {
            GameObject root = GetOrCreateUiChild(parent, "HealthHeartsPanel");
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 0f);
            rect.anchorMax = new Vector2(1f, 0f);
            rect.pivot = new Vector2(1f, 0f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(460f, 104f);

            HorizontalLayoutGroup layout = GetOrAddComponent<HorizontalLayoutGroup>(root);
            layout.spacing = 16f;
            layout.childAlignment = TextAnchor.MiddleRight;
            layout.childControlHeight = false;
            layout.childControlWidth = false;
            layout.childForceExpandHeight = false;
            layout.childForceExpandWidth = false;

            Image template = CreateImage(root.transform, "HeartTemplate", EnsureSprite(HeartSpritePath), new Vector2(72f, 72f));
            template.gameObject.SetActive(false);

            HeartHealthView heartHealthView = GetOrAddComponent<HeartHealthView>(root);
            SerializedObject serialized = new SerializedObject(heartHealthView);
            serialized.FindProperty("heartContainer").objectReferenceValue = rect;
            serialized.FindProperty("heartTemplate").objectReferenceValue = template;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(heartHealthView);
            return heartHealthView;
        }

        private static ProgressBarView CreateMindPower(Transform parent)
        {
            GameObject root = GetOrCreateUiChild(parent, "MentalPowerPanel");
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(0f, 0f);
            rect.pivot = new Vector2(0f, 0f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(454f, 118f);

            Image background = GetOrAddComponent<Image>(root);
            background.sprite = EnsureSprite(MindBarBackgroundPath);
            background.color = Color.white;
            background.type = Image.Type.Simple;
            background.preserveAspect = true;

            Image brainIcon = CreateImage(root.transform, "BrainIcon", EnsureSprite(BrainSpritePath), new Vector2(116f, 116f));
            RectTransform brainRect = brainIcon.rectTransform;
            brainRect.anchorMin = new Vector2(0f, 0.5f);
            brainRect.anchorMax = new Vector2(0f, 0.5f);
            brainRect.pivot = new Vector2(0.5f, 0.5f);
            brainRect.anchoredPosition = new Vector2(28f, 0f);

            GameObject fillArea = GetOrCreateUiChild(root.transform, "FillArea");
            RectTransform fillAreaRect = fillArea.GetComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0f, 0f);
            fillAreaRect.anchorMax = new Vector2(1f, 1f);
            fillAreaRect.offsetMin = new Vector2(92f, 26f);
            fillAreaRect.offsetMax = new Vector2(-34f, -28f);

            Image fill = CreateImage(fillArea.transform, "Fill", EnsureSprite(MindBarFillPath), Vector2.zero);
            RectTransform fillRect = fill.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            fill.type = Image.Type.Simple;
            fill.preserveAspect = false;

            ProgressBarView progressBarView = GetOrAddComponent<ProgressBarView>(root);
            SerializedObject serialized = new SerializedObject(progressBarView);
            serialized.FindProperty("fillImage").objectReferenceValue = fill;
            serialized.FindProperty("fillArea").objectReferenceValue = fillAreaRect;
            serialized.FindProperty("valueText").objectReferenceValue = null;
            serialized.FindProperty("statusText").objectReferenceValue = null;
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(progressBarView);
            return progressBarView;
        }

        private static TMP_Text CreateTimer(Transform parent)
        {
            GameObject root = GetOrCreateUiChild(parent, "GameTimer");
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(262f, 104f);

            Image background = GetOrAddComponent<Image>(root);
            background.sprite = EnsureSprite(TimerBarPath);
            background.color = Color.white;
            background.type = Image.Type.Simple;
            background.preserveAspect = true;

            TMP_Text timerText = CreateText(root.transform, "TimerText", "04:52", 58f, TextAlignmentOptions.Center);
            StretchFull(timerText.rectTransform);
            timerText.rectTransform.offsetMin = new Vector2(0f, 8f);
            timerText.rectTransform.offsetMax = new Vector2(0f, -8f);
            return timerText;
        }

        private static KillTrackerHudView CreateKillTracker(Transform parent)
        {
            GameObject root = GetOrCreateUiChild(parent, "KillTrackerPanel");
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = new Vector2(470f, 118f);

            Image background = GetOrAddComponent<Image>(root);
            background.sprite = EnsureSprite(KillCounterBackgroundPath);
            background.color = Color.white;
            background.type = Image.Type.Simple;
            background.preserveAspect = true;

            TMP_Text zombieText = CreateKillCounter(root.transform, "ZombieCounter", EnsureSprite(ZombieIconPath), new Vector2(-132f, 1f));
            TMP_Text specialText = CreateKillCounter(root.transform, "SpecialCounter", EnsureSprite(SpecialIconPath), new Vector2(0f, 1f));
            TMP_Text bossText = CreateKillCounter(root.transform, "BossCounter", EnsureSprite(BossIconPath), new Vector2(132f, 1f));

            KillTrackerHudView killTracker = GetOrAddComponent<KillTrackerHudView>(root);
            SerializedObject serialized = new SerializedObject(killTracker);
            serialized.FindProperty("gameManager").objectReferenceValue = Object.FindFirstObjectByType<GameManager>();
            SerializedProperty counters = serialized.FindProperty("counters");
            counters.arraySize = 3;
            AssignKillCounter(counters.GetArrayElementAtIndex(0), EnemyType.Zombie, zombieText);
            AssignKillCounter(counters.GetArrayElementAtIndex(1), EnemyType.Special, specialText);
            AssignKillCounter(counters.GetArrayElementAtIndex(2), EnemyType.Boss, bossText);
            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(killTracker);
            return killTracker;
        }

        private static TMP_Text CreateKillCounter(Transform parent, string name, Sprite iconSprite, Vector2 anchoredPosition)
        {
            GameObject root = GetOrCreateUiChild(parent, name);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(118f, 64f);

            Image icon = CreateImage(root.transform, "Icon", iconSprite, new Vector2(54f, 54f));
            RectTransform iconRect = icon.rectTransform;
            iconRect.anchorMin = new Vector2(0f, 0.5f);
            iconRect.anchorMax = new Vector2(0f, 0.5f);
            iconRect.pivot = new Vector2(0.5f, 0.5f);
            iconRect.anchoredPosition = new Vector2(28f, 0f);

            TMP_Text countText = CreateText(root.transform, "CountText", "x0", 38f, TextAlignmentOptions.MidlineLeft);
            RectTransform textRect = countText.rectTransform;
            textRect.anchorMin = new Vector2(0f, 0f);
            textRect.anchorMax = new Vector2(1f, 1f);
            textRect.offsetMin = new Vector2(58f, 0f);
            textRect.offsetMax = Vector2.zero;
            return countText;
        }

        private static void AssignKillCounter(SerializedProperty property, EnemyType enemyType, TMP_Text countText)
        {
            property.FindPropertyRelative("enemyType").enumValueIndex = (int)enemyType;
            property.FindPropertyRelative("countText").objectReferenceValue = countText;
        }

        private static Canvas GetOrCreateCanvas()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas != null)
            {
                EnsureCanvasScaler(canvas);
                return canvas;
            }

            GameObject canvasObject = new GameObject("GameplayHUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            Undo.RegisterCreatedObjectUndo(canvasObject, "Create Gameplay HUD Canvas");
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            EnsureCanvasScaler(canvas);
            return canvas;
        }

        private static GameObject FindExistingHudRoot()
        {
            RectTransform[] rects = Object.FindObjectsByType<RectTransform>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            foreach (RectTransform rect in rects)
            {
                if (rect == null)
                {
                    continue;
                }

                string objectName = rect.gameObject.name;
                if (objectName == "HUDRoot" || objectName == "HUDRoot (Safe Area)")
                {
                    return rect.gameObject;
                }
            }

            return null;
        }

        private static GameObject GetOrCreateHudRoot(Transform canvasTransform)
        {
            GameObject root = FindExistingHudRoot();
            if (root == null)
            {
                root = GetOrCreateUiChild(canvasTransform, "HUDRoot (Safe Area)");
            }

            root.transform.SetParent(canvasTransform, false);
            StretchFull(root.GetComponent<RectTransform>());
            return root;
        }

        private static GameObject GetOrCreateSection(Transform parent, string name)
        {
            GameObject section = GetOrCreateUiChild(parent, name);
            RectTransform rect = section.GetComponent<RectTransform>();

            if (name == "Top")
            {
                rect.anchorMin = new Vector2(0f, 1f);
                rect.anchorMax = new Vector2(1f, 1f);
                rect.pivot = new Vector2(0.5f, 1f);
                rect.offsetMin = new Vector2(0f, -130f);
                rect.offsetMax = Vector2.zero;
                return section;
            }

            if (name == "Bottom")
            {
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(1f, 0f);
                rect.pivot = new Vector2(0.5f, 0f);
                rect.offsetMin = Vector2.zero;
                rect.offsetMax = new Vector2(0f, 160f);
                return section;
            }

            if (name == "BottomLeft")
            {
                rect.anchorMin = new Vector2(0f, 0f);
                rect.anchorMax = new Vector2(0f, 0f);
                rect.pivot = new Vector2(0f, 0f);
                rect.anchoredPosition = new Vector2(64f, 48f);
                rect.sizeDelta = new Vector2(454f, 118f);
                return section;
            }

            if (name == "BottomCenter")
            {
                rect.anchorMin = new Vector2(0.5f, 0f);
                rect.anchorMax = new Vector2(0.5f, 0f);
                rect.pivot = new Vector2(0.5f, 0f);
                rect.anchoredPosition = new Vector2(0f, 46f);
                rect.sizeDelta = new Vector2(470f, 118f);
                return section;
            }

            if (name == "BottomRight")
            {
                rect.anchorMin = new Vector2(1f, 0f);
                rect.anchorMax = new Vector2(1f, 0f);
                rect.pivot = new Vector2(1f, 0f);
                rect.anchoredPosition = new Vector2(-78f, 52f);
                rect.sizeDelta = new Vector2(460f, 104f);
                return section;
            }

            return section;
        }

        private static void EnsureCanvasScaler(Canvas canvas)
        {
            CanvasScaler scaler = GetOrAddComponent<CanvasScaler>(canvas.gameObject);
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        private static TMP_Text CreateText(Transform parent, string name, string textValue, float fontSize, TextAlignmentOptions alignment)
        {
            GameObject textObject = GetOrCreateUiChild(parent, name);
            TextMeshProUGUI text = GetOrAddComponent<TextMeshProUGUI>(textObject);
            text.text = textValue;
            text.font = LoadHudFont();
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.color = Color.white;
            text.raycastTarget = false;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            return text;
        }

        private static Image CreateImage(Transform parent, string name, Sprite sprite, Vector2 size)
        {
            GameObject imageObject = GetOrCreateUiChild(parent, name);
            RectTransform rect = imageObject.GetComponent<RectTransform>();
            if (size != Vector2.zero)
            {
                rect.sizeDelta = size;
            }

            Image image = GetOrAddComponent<Image>(imageObject);
            image.sprite = sprite;
            image.color = Color.white;
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
            return image;
        }

        private static TMP_FontAsset LoadHudFont()
        {
            TMP_FontAsset font = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(HudFontPath);
            return font != null ? font : TMP_Settings.defaultFontAsset;
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
                Debug.LogWarning($"Could not load HUD sprite at {path}.");
            }

            return sprite;
        }

        private static GameObject GetOrCreateUiChild(Transform parent, string childName)
        {
            Transform existingChild = parent.Find(childName);
            if (existingChild != null)
            {
                return existingChild.gameObject;
            }

            GameObject child = new GameObject(childName, typeof(RectTransform), typeof(CanvasRenderer));
            Undo.RegisterCreatedObjectUndo(child, "Create Gameplay HUD Object");
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

        private static void StretchFull(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}
