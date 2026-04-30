using BrainEaters.Configs;
using BrainEaters.Enemies;
using BrainEaters.GameFlow;
using BrainEaters.Spawning;
using BrainEaters.Turrets;
using BrainEaters.UI;
using TMPro;
using Unity.AI.Navigation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace BrainEaters.EditorTools
{
    public static class OnboardingArenaBuilder
    {
        private const string ArenaName = "Arena_Onboarding_Bridge_01";
        private const string PrefabPath = "Assets/BrainEaters/Prefabs/Arena_Onboarding_Bridge_01.prefab";
        private const string LevelConfigPath = "Assets/BrainEaters/Data/LevelConfig_Level01.asset";
        private const string SpawnConfigPath = "Assets/BrainEaters/Data/SpawnConfig_Level01.asset";
        private const string CampaignConfigPath = "Assets/BrainEaters/Data/CampaignConfig.asset";
        private const string EnemyPrefabPath = "Assets/BrainEaters/Prefabs/Enemies/EnemyBasic_Frank.prefab";
        private const string EnemyConfigPath = "Assets/BrainEaters/Data/EnemyConfig_ZombieBasic.asset";
        private const string GeneratedMaterialsFolderPath = "Assets/BrainEaters/Materials/Generated";

        private static readonly Color GroundColor = new Color(0.32f, 0.42f, 0.28f, 1f);
        private static readonly Color BridgeColor = new Color(0.46f, 0.24f, 0.1f, 1f);
        private static readonly Color GateColor = new Color(0.18f, 0.16f, 0.14f, 1f);
        private static readonly Color SignColor = new Color(0.55f, 0.31f, 0.12f, 1f);

        [MenuItem("Brain Eaters/Onboarding/Build Bridge Arena In Current Scene")]
        public static void BuildInCurrentScene()
        {
            GameObject existing = GameObject.Find(ArenaName);
            if (existing != null)
            {
                Object.DestroyImmediate(existing);
            }

            GameObject arenaRoot = CreateArena();
            Selection.activeGameObject = arenaRoot;
            EditorGUIUtility.PingObject(arenaRoot);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            Debug.Log($"Built onboarding arena in current scene: {ArenaName}", arenaRoot);
        }

        [MenuItem("Brain Eaters/Onboarding/Save Bridge Arena Prefab And Configure Level 01")]
        public static void SavePrefabAndConfigureLevel01()
        {
            EnsureFolder("Assets/BrainEaters", "Prefabs");
            EnsureFolder("Assets/BrainEaters", "Data");

            GameObject arenaRoot = CreateArena();
            GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(arenaRoot, PrefabPath);
            Object.DestroyImmediate(arenaRoot);

            ConfigureLevelAssets(prefabAsset.GetComponent<LevelContext>());
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = prefabAsset;
            EditorGUIUtility.PingObject(prefabAsset);
            Debug.Log($"Saved onboarding arena prefab and configured Level 01: {PrefabPath}", prefabAsset);
        }

        [MenuItem("Brain Eaters/Onboarding/Upgrade Existing Bridge Gate To Double Door")]
        public static void UpgradeExistingBridgeGateToDoubleDoor()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath) == null)
            {
                Debug.LogError($"Cannot upgrade onboarding gate because prefab does not exist: {PrefabPath}");
                return;
            }

            GameObject prefabRoot = PrefabUtility.LoadPrefabContents(PrefabPath);
            Material gateMaterial = GetOrCreateMaterialAsset("Onboarding_Gate", GateColor);

            GameObject oldGate = FindChild(prefabRoot.transform, "ClosedGate");
            if (oldGate != null)
            {
                Object.DestroyImmediate(oldGate);
            }

            GameObject gateRoot = CreateGate(prefabRoot.transform, gateMaterial);
            OnboardingBridgeObjective objective = prefabRoot.GetComponentInChildren<OnboardingBridgeObjective>(true);
            OnboardingBridgeLaunchZone launchZone = prefabRoot.GetComponentInChildren<OnboardingBridgeLaunchZone>(true);
            if (objective != null)
            {
                ConfigureBridgeObjective(
                    objective,
                    FindChild(prefabRoot.transform, "BridgePivot")?.transform,
                    gateRoot.transform,
                    gateRoot.GetComponent<Collider>(),
                    gateRoot.GetComponent<OnboardingGateTarget>(),
                    launchZone,
                    FindChild(prefabRoot.transform, "MentalBombLeverIndicator")?.transform);
            }

            NavMeshSurface surface = prefabRoot.GetComponent<NavMeshSurface>();
            if (surface != null)
            {
                surface.BuildNavMesh();
            }

            PrefabUtility.SaveAsPrefabAsset(prefabRoot, PrefabPath);
            PrefabUtility.UnloadPrefabContents(prefabRoot);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"Upgraded onboarding gate to double doors in prefab: {PrefabPath}");
        }

        private static GameObject CreateArena()
        {
            GameObject root = new GameObject(ArenaName);
            LevelContext levelContext = root.AddComponent<LevelContext>();

            EnsureGeneratedMaterialFolder();
            Material groundMaterial = GetOrCreateMaterialAsset("Onboarding_Ground", GroundColor);
            Material bridgeMaterial = GetOrCreateMaterialAsset("Onboarding_Bridge", BridgeColor);
            Material gateMaterial = GetOrCreateMaterialAsset("Onboarding_Gate", GateColor);
            Material signMaterial = GetOrCreateMaterialAsset("Onboarding_Sign", SignColor);

            CreateCube(root.transform, "SafeZoneFloor", new Vector3(0f, -0.1f, -7f), new Vector3(14f, 0.2f, 10f), groundMaterial);
            CreateCube(root.transform, "EnemySideFloor", new Vector3(0f, -0.1f, 12.5f), new Vector3(12f, 0.2f, 8f), groundMaterial);
            CreateCube(root.transform, "LeftRavineWall", new Vector3(-5.5f, 0.55f, 3f), new Vector3(0.5f, 1.1f, 18f), gateMaterial);
            CreateCube(root.transform, "RightRavineWall", new Vector3(5.5f, 0.55f, 3f), new Vector3(0.5f, 1.1f, 18f), gateMaterial);

            PlayerSpawnPoint playerSpawnPoint = CreateMarker<PlayerSpawnPoint>(root.transform, "PlayerSpawnPoint", new Vector3(0f, 1f, -10f), Quaternion.LookRotation(Vector3.forward));

            Transform bridgePivot = CreateEmpty(root.transform, "BridgePivot", new Vector3(0f, 0f, -0.25f), Quaternion.identity);
            CreateCube(bridgePivot, "BridgeDeck", new Vector3(0f, 0.12f, 4.9f), new Vector3(4.2f, 0.25f, 10.6f), bridgeMaterial);
            for (int i = 0; i < 6; i++)
            {
                CreateCube(bridgePivot, $"BridgePlank_{i + 1:00}", new Vector3(0f, 0.28f, 0.4f + i * 1.7f), new Vector3(4.4f, 0.12f, 0.32f), bridgeMaterial);
            }

            GameObject launchZoneObject = new GameObject("BridgeLaunchZone");
            launchZoneObject.transform.SetParent(root.transform);
            launchZoneObject.transform.SetLocalPositionAndRotation(new Vector3(0f, 1.1f, 4.8f), Quaternion.identity);
            BoxCollider launchCollider = launchZoneObject.AddComponent<BoxCollider>();
            launchCollider.isTrigger = true;
            launchCollider.size = new Vector3(5f, 2.4f, 10f);
            OnboardingBridgeLaunchZone launchZone = launchZoneObject.AddComponent<OnboardingBridgeLaunchZone>();

            GameObject gateRoot = CreateGate(root.transform, gateMaterial);
            Collider gateBlocker = gateRoot.GetComponent<Collider>();
            OnboardingGateTarget gateTarget = gateRoot.GetComponent<OnboardingGateTarget>();

            GameObject objectiveRoot = new GameObject("BombBridgeObjective");
            objectiveRoot.transform.SetParent(root.transform);
            objectiveRoot.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
            BoxCollider activationArea = objectiveRoot.AddComponent<BoxCollider>();
            activationArea.isTrigger = true;
            activationArea.center = new Vector3(0f, 1.2f, -7f);
            activationArea.size = new Vector3(13f, 2.5f, 9f);
            OnboardingBridgeObjective objective = objectiveRoot.AddComponent<OnboardingBridgeObjective>();
            Transform lever = CreateLever(objectiveRoot.transform, signMaterial);
            ConfigureBridgeObjective(objective, bridgePivot, gateRoot.transform, gateBlocker, gateTarget, launchZone, lever);

            CreateSignAndPopup(root.transform, signMaterial);

            SpawnPoint[] spawnPoints =
            {
                CreateMarker<SpawnPoint>(root.transform, "SpawnPoint_Left", new Vector3(-2.5f, 0f, 14f), Quaternion.LookRotation(Vector3.back)),
                CreateMarker<SpawnPoint>(root.transform, "SpawnPoint_Center", new Vector3(0f, 0f, 15f), Quaternion.LookRotation(Vector3.back)),
                CreateMarker<SpawnPoint>(root.transform, "SpawnPoint_Right", new Vector3(2.5f, 0f, 14f), Quaternion.LookRotation(Vector3.back))
            };

            CollectPickup pickup = CreateGoalPickup(root.transform);
            ConfigureLevelContext(levelContext, playerSpawnPoint, spawnPoints, pickup);
            AddNavMeshSurface(root);

            return root;
        }

        private static GameObject CreateGate(Transform parent, Material material)
        {
            GameObject gate = new GameObject("ClosedGate");
            gate.transform.SetParent(parent);
            gate.transform.SetLocalPositionAndRotation(new Vector3(0f, 0f, -1.05f), Quaternion.identity);
            BoxCollider blocker = gate.AddComponent<BoxCollider>();
            blocker.center = new Vector3(0f, 1.2f, 0f);
            blocker.size = new Vector3(4.9f, 2.4f, 0.45f);

            Transform leftPivot = CreateEmpty(gate.transform, "LeftDoorPivot", new Vector3(-2.4f, 0f, 0f), Quaternion.identity);
            Transform rightPivot = CreateEmpty(gate.transform, "RightDoorPivot", new Vector3(2.4f, 0f, 0f), Quaternion.identity);
            CreateCube(leftPivot, "LeftDoor", new Vector3(1.2f, 1.2f, 0f), new Vector3(2.35f, 2.4f, 0.35f), material);
            CreateCube(rightPivot, "RightDoor", new Vector3(-1.2f, 1.2f, 0f), new Vector3(2.35f, 2.4f, 0.35f), material);

            GameObject targetPoint = new GameObject("TargetPoint");
            targetPoint.transform.SetParent(gate.transform);
            targetPoint.transform.localPosition = new Vector3(0f, 1.2f, -0.55f);

            TurretHealth turretHealth = gate.AddComponent<TurretHealth>();
            SerializedObject turretSerialized = new SerializedObject(turretHealth);
            turretSerialized.FindProperty("targetableWhenOnline").boolValue = true;
            turretSerialized.FindProperty("maxHealth").floatValue = 9999f;
            turretSerialized.FindProperty("targetPoint").objectReferenceValue = targetPoint.transform;
            turretSerialized.ApplyModifiedPropertiesWithoutUndo();

            OnboardingGateTarget gateTarget = gate.AddComponent<OnboardingGateTarget>();
            SerializedObject targetSerialized = new SerializedObject(gateTarget);
            targetSerialized.FindProperty("targetHealth").objectReferenceValue = turretHealth;
            SerializedProperty feedbackRoots = targetSerialized.FindProperty("feedbackRoots");
            feedbackRoots.arraySize = 2;
            feedbackRoots.GetArrayElementAtIndex(0).objectReferenceValue = leftPivot;
            feedbackRoots.GetArrayElementAtIndex(1).objectReferenceValue = rightPivot;
            targetSerialized.ApplyModifiedPropertiesWithoutUndo();
            return gate;
        }

        private static Transform CreateLever(Transform parent, Material material)
        {
            GameObject leverRoot = new GameObject("MentalBombLeverIndicator");
            leverRoot.transform.SetParent(parent);
            leverRoot.transform.SetLocalPositionAndRotation(new Vector3(2.8f, 0f, -5.7f), Quaternion.identity);
            CreateCube(leverRoot.transform, "Base", new Vector3(0f, 0.2f, 0f), new Vector3(0.9f, 0.25f, 0.9f), material);
            Transform handle = CreateCube(leverRoot.transform, "Handle", new Vector3(0f, 0.9f, 0f), new Vector3(0.18f, 1.2f, 0.18f), material).transform;
            handle.localRotation = Quaternion.Euler(0f, 0f, -28f);
            return leverRoot.transform;
        }

        private static void CreateSignAndPopup(Transform parent, Material material)
        {
            GameObject signRoot = new GameObject("MentalPowerSign");
            signRoot.transform.SetParent(parent);
            signRoot.transform.SetLocalPositionAndRotation(new Vector3(-2.8f, 0f, -5.7f), Quaternion.LookRotation(Vector3.back));
            CreateCube(signRoot.transform, "Post", new Vector3(0f, 0.75f, 0f), new Vector3(0.18f, 1.5f, 0.18f), material);
            CreateCube(signRoot.transform, "Board", new Vector3(0f, 1.65f, 0f), new Vector3(2.2f, 1.1f, 0.18f), material);

            BoxCollider trigger = signRoot.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.center = new Vector3(0f, 1f, -0.2f);
            trigger.size = new Vector3(4.5f, 2.3f, 3f);

            GameObject canvasObject = CreateOnboardingPopupCanvas(signRoot.transform, out TMP_Text titleText, out TMP_Text bodyText, out Button continueButton);
            OnboardingPopupTrigger popupTrigger = signRoot.AddComponent<OnboardingPopupTrigger>();
            SerializedObject popupSerialized = new SerializedObject(popupTrigger);
            popupSerialized.FindProperty("popupRoot").objectReferenceValue = canvasObject.transform.Find("Panel").gameObject;
            popupSerialized.FindProperty("titleText").objectReferenceValue = titleText;
            popupSerialized.FindProperty("bodyText").objectReferenceValue = bodyText;
            popupSerialized.FindProperty("continueButton").objectReferenceValue = continueButton;
            popupSerialized.FindProperty("title").stringValue = "Use Your Mind";
            popupSerialized.FindProperty("body").stringValue = "Stand in the safe zone and charge your Mental Power. When the bar is full, trigger the Brain Bomb near the lever to launch the monsters off the bridge.";
            popupSerialized.FindProperty("pauseGame").boolValue = true;
            popupSerialized.FindProperty("showOnlyOnce").boolValue = true;
            popupSerialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject CreateOnboardingPopupCanvas(Transform parent, out TMP_Text titleText, out TMP_Text bodyText, out Button continueButton)
        {
            GameObject canvasObject = new GameObject("OnboardingPopupCanvas");
            canvasObject.transform.SetParent(parent);
            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 80;
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(2532f, 1170f);
            scaler.matchWidthOrHeight = 0.5f;
            canvasObject.AddComponent<GraphicRaycaster>();

            GameObject panel = new GameObject("Panel");
            panel.transform.SetParent(canvasObject.transform, false);
            RectTransform panelRect = panel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.5f, 0.5f);
            panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(980f, 520f);
            panelRect.anchoredPosition = Vector2.zero;
            Image panelImage = panel.AddComponent<Image>();
            panelImage.color = new Color(0.36f, 0.18f, 0.07f, 0.96f);
            CanvasGroup group = panel.AddComponent<CanvasGroup>();
            group.alpha = 1f;
            UiVisibilityAnimator animator = panel.AddComponent<UiVisibilityAnimator>();
            SerializedObject animatorSerialized = new SerializedObject(animator);
            animatorSerialized.FindProperty("targetRect").objectReferenceValue = panelRect;
            animatorSerialized.FindProperty("canvasGroup").objectReferenceValue = group;
            animatorSerialized.FindProperty("playOnEnable").boolValue = false;
            animatorSerialized.ApplyModifiedPropertiesWithoutUndo();

            titleText = CreatePopupText(panel.transform, "Title", "Use Your Mind", 64f, FontStyles.Bold, new Vector2(0f, 150f), new Vector2(820f, 90f));
            bodyText = CreatePopupText(panel.transform, "Body", "Charge your Mental Power, then use the Brain Bomb near the bridge lever.", 38f, FontStyles.Normal, new Vector2(0f, 25f), new Vector2(800f, 210f));
            bodyText.alignment = TextAlignmentOptions.Center;

            GameObject buttonObject = new GameObject("ContinueButton");
            buttonObject.transform.SetParent(panel.transform, false);
            RectTransform buttonRect = buttonObject.AddComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.sizeDelta = new Vector2(300f, 88f);
            buttonRect.anchoredPosition = new Vector2(0f, -180f);
            Image buttonImage = buttonObject.AddComponent<Image>();
            buttonImage.color = new Color(0.35f, 0.78f, 0.22f, 1f);
            continueButton = buttonObject.AddComponent<Button>();
            CreatePopupText(buttonObject.transform, "Label", "CONTINUE", 38f, FontStyles.Bold, Vector2.zero, new Vector2(280f, 74f));

            panel.SetActive(false);
            return canvasObject;
        }

        private static TMP_Text CreatePopupText(Transform parent, string name, string text, float size, FontStyles style, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            GameObject textObject = new GameObject(name);
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = sizeDelta;
            rect.anchoredPosition = anchoredPosition;

            TextMeshProUGUI tmp = textObject.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.fontStyle = style;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.color = Color.white;
            tmp.enableAutoSizing = false;
            tmp.raycastTarget = false;
            return tmp;
        }

        private static CollectPickup CreateGoalPickup(Transform parent)
        {
            GameObject pickupRoot = new GameObject("OnboardingGoalPickup");
            pickupRoot.transform.SetParent(parent);
            pickupRoot.transform.SetLocalPositionAndRotation(new Vector3(0f, 0.7f, 15.8f), Quaternion.identity);
            SphereCollider trigger = pickupRoot.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 0.9f;

            GameObject visualRoot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            visualRoot.name = "VisualRoot";
            visualRoot.transform.SetParent(pickupRoot.transform);
            visualRoot.transform.localPosition = Vector3.zero;
            visualRoot.transform.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            Collider visualCollider = visualRoot.GetComponent<Collider>();
            if (visualCollider != null)
            {
                Object.DestroyImmediate(visualCollider);
            }

            Renderer renderer = visualRoot.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = GetOrCreateMaterialAsset("Onboarding_Goal", new Color(1f, 0.86f, 0.24f, 1f));
            }

            return pickupRoot.AddComponent<CollectPickup>();
        }

        private static void ConfigureBridgeObjective(OnboardingBridgeObjective objective, Transform bridgePivot, Transform gateRoot, Collider gateBlocker, OnboardingGateTarget gateTarget, OnboardingBridgeLaunchZone launchZone, Transform activationIndicator)
        {
            SerializedObject serialized = new SerializedObject(objective);
            serialized.FindProperty("bridgePivot").objectReferenceValue = bridgePivot;
            serialized.FindProperty("gateRoot").objectReferenceValue = gateRoot;
            serialized.FindProperty("leftGatePivot").objectReferenceValue = gateRoot != null ? gateRoot.Find("LeftDoorPivot") : null;
            serialized.FindProperty("rightGatePivot").objectReferenceValue = gateRoot != null ? gateRoot.Find("RightDoorPivot") : null;
            serialized.FindProperty("gateBlocker").objectReferenceValue = gateBlocker;
            serialized.FindProperty("gateTarget").objectReferenceValue = gateTarget;
            serialized.FindProperty("launchZone").objectReferenceValue = launchZone;
            serialized.FindProperty("activationIndicator").objectReferenceValue = activationIndicator;
            serialized.FindProperty("bridgeRaiseAngle").floatValue = -64f;
            serialized.FindProperty("bridgeRaiseDuration").floatValue = 0.42f;
            serialized.FindProperty("launchDelaySeconds").floatValue = 0.08f;
            serialized.FindProperty("gateOpenDuration").floatValue = 0.55f;
            serialized.FindProperty("leftGateOpenAngle").floatValue = 105f;
            serialized.FindProperty("rightGateOpenAngle").floatValue = -105f;
            serialized.FindProperty("stopSpawningOnActivation").boolValue = true;
            serialized.FindProperty("killRemainingEnemiesOnActivation").boolValue = true;
            serialized.FindProperty("killRemainingEnemiesDelay").floatValue = 1.25f;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureLevelContext(LevelContext context, PlayerSpawnPoint playerSpawnPoint, SpawnPoint[] spawnPoints, CollectPickup pickup)
        {
            SerializedObject serialized = new SerializedObject(context);
            serialized.FindProperty("playerSpawnPoint").objectReferenceValue = playerSpawnPoint;

            SerializedProperty spawnPointsProperty = serialized.FindProperty("spawnPoints");
            spawnPointsProperty.arraySize = spawnPoints.Length;
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                spawnPointsProperty.GetArrayElementAtIndex(i).objectReferenceValue = spawnPoints[i];
            }

            SerializedProperty collectPickupsProperty = serialized.FindProperty("collectPickups");
            collectPickupsProperty.arraySize = 1;
            collectPickupsProperty.GetArrayElementAtIndex(0).objectReferenceValue = pickup;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void ConfigureLevelAssets(LevelContext levelPrefab)
        {
            SpawnConfig spawnConfig = AssetDatabase.LoadAssetAtPath<SpawnConfig>(SpawnConfigPath);
            LevelConfig levelConfig = AssetDatabase.LoadAssetAtPath<LevelConfig>(LevelConfigPath);
            EnemyController enemyPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefabPath)?.GetComponent<EnemyController>();
            EnemyConfig enemyConfig = AssetDatabase.LoadAssetAtPath<EnemyConfig>(EnemyConfigPath);

            if (spawnConfig == null || levelConfig == null || enemyPrefab == null || enemyConfig == null || levelPrefab == null)
            {
                Debug.LogError("Could not configure onboarding Level 01. Missing LevelConfig, SpawnConfig, enemy prefab/config, or generated arena prefab.");
                return;
            }

            SerializedObject spawnSerialized = new SerializedObject(spawnConfig);
            spawnSerialized.FindProperty("initialDelaySeconds").floatValue = 0.5f;
            spawnSerialized.FindProperty("spawnIntervalSeconds").floatValue = 0.85f;
            spawnSerialized.FindProperty("maxAliveEnemies").intValue = 18;
            spawnSerialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(spawnConfig);

            SerializedObject levelSerialized = new SerializedObject(levelConfig);
            levelSerialized.FindProperty("levelId").stringValue = "onboarding_bridge_01";
            levelSerialized.FindProperty("displayName").stringValue = "Onboarding";
            levelSerialized.FindProperty("gameModeType").enumValueIndex = (int)GameModeType.Collect;
            levelSerialized.FindProperty("survivalDurationSeconds").floatValue = 300f;
            levelSerialized.FindProperty("levelPrefab").objectReferenceValue = levelPrefab;
            levelSerialized.FindProperty("spawnConfig").objectReferenceValue = spawnConfig;
            SerializedProperty enemyTypes = levelSerialized.FindProperty("enemyTypes");
            enemyTypes.arraySize = 1;
            SerializedProperty enemy = enemyTypes.GetArrayElementAtIndex(0);
            enemy.FindPropertyRelative("enemyPrefab").objectReferenceValue = enemyPrefab;
            enemy.FindPropertyRelative("enemyConfig").objectReferenceValue = enemyConfig;
            levelSerialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(levelConfig);

            CampaignConfig campaign = AssetDatabase.LoadAssetAtPath<CampaignConfig>(CampaignConfigPath);
            if (campaign != null)
            {
                SerializedObject campaignSerialized = new SerializedObject(campaign);
                SerializedProperty levels = campaignSerialized.FindProperty("levels");
                EnsureLevelIsFirst(levels, levelConfig);
                campaignSerialized.ApplyModifiedPropertiesWithoutUndo();
                EditorUtility.SetDirty(campaign);
            }
        }

        private static void EnsureLevelIsFirst(SerializedProperty levels, LevelConfig levelConfig)
        {
            int existingIndex = -1;
            for (int i = 0; i < levels.arraySize; i++)
            {
                if (levels.GetArrayElementAtIndex(i).objectReferenceValue == levelConfig)
                {
                    existingIndex = i;
                    break;
                }
            }

            if (existingIndex < 0)
            {
                levels.InsertArrayElementAtIndex(0);
                levels.GetArrayElementAtIndex(0).objectReferenceValue = levelConfig;
                return;
            }

            if (existingIndex == 0)
            {
                return;
            }

            Object existingValue = levels.GetArrayElementAtIndex(existingIndex).objectReferenceValue;
            levels.DeleteArrayElementAtIndex(existingIndex);
            levels.InsertArrayElementAtIndex(0);
            levels.GetArrayElementAtIndex(0).objectReferenceValue = existingValue;
        }

        private static void AddNavMeshSurface(GameObject root)
        {
            NavMeshSurface surface = root.AddComponent<NavMeshSurface>();
            surface.collectObjects = CollectObjects.Children;
            surface.useGeometry = NavMeshCollectGeometry.RenderMeshes;
            surface.defaultArea = 0;
            surface.BuildNavMesh();
        }

        private static GameObject CreateCube(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Material material)
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cube.name = name;
            cube.transform.SetParent(parent);
            cube.transform.localPosition = localPosition;
            cube.transform.localRotation = Quaternion.identity;
            cube.transform.localScale = localScale;

            Renderer renderer = cube.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
            }

            return cube;
        }

        private static T CreateMarker<T>(Transform parent, string name, Vector3 localPosition, Quaternion localRotation) where T : Component
        {
            GameObject marker = new GameObject(name);
            marker.transform.SetParent(parent);
            marker.transform.localPosition = localPosition;
            marker.transform.localRotation = localRotation;
            return marker.AddComponent<T>();
        }

        private static Transform CreateEmpty(Transform parent, string name, Vector3 localPosition, Quaternion localRotation)
        {
            GameObject empty = new GameObject(name);
            empty.transform.SetParent(parent);
            empty.transform.localPosition = localPosition;
            empty.transform.localRotation = localRotation;
            return empty.transform;
        }

        private static GameObject FindChild(Transform root, string childName)
        {
            if (root == null)
            {
                return null;
            }

            Transform[] children = root.GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < children.Length; i++)
            {
                if (children[i] != null && children[i].name == childName)
                {
                    return children[i].gameObject;
                }
            }

            return null;
        }

        private static Material GetOrCreateMaterialAsset(string name, Color color)
        {
            EnsureGeneratedMaterialFolder();
            string path = $"{GeneratedMaterialsFolderPath}/{name}.mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(ResolveDefaultShader());
                AssetDatabase.CreateAsset(material, path);
            }

            material.name = name;
            material.color = color;
            EditorUtility.SetDirty(material);
            return material;
        }

        private static Shader ResolveDefaultShader()
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
            }

            if (shader == null)
            {
                shader = Shader.Find("Hidden/InternalErrorShader");
            }

            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            return shader;
        }

        private static void EnsureGeneratedMaterialFolder()
        {
            EnsureFolder("Assets/BrainEaters", "Materials");
            EnsureFolder("Assets/BrainEaters/Materials", "Generated");
        }

        private static void EnsureFolder(string parentPath, string folderName)
        {
            string folderPath = $"{parentPath}/{folderName}";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder(parentPath, folderName);
            }
        }
    }
}
