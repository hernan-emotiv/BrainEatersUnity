using BrainEaters.Configs;
using BrainEaters.GameFlow;
using BrainEaters.Spawning;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace BrainEaters.EditorTools
{
    public static class CollectArenaBuilder
    {
        private const string ArenaRootName = "Arena_Collect_01";
        private const string PrefabsFolderPath = "Assets/BrainEaters/Prefabs";
        private const string CollectPrefabPath = PrefabsFolderPath + "/Arena_Collect_01.prefab";
        private const string LevelConfigPath = "Assets/BrainEaters/Data/LevelConfig_Level03.asset";
        private const string CampaignConfigPath = "Assets/BrainEaters/Data/CampaignConfig.asset";

        [MenuItem("Brain Eaters/Build Collect Arena In Current Scene")]
        public static void BuildCollectArenaInCurrentScene()
        {
            GameObject existingRoot = GameObject.Find(ArenaRootName);
            if (existingRoot != null)
            {
                Undo.DestroyObjectImmediate(existingRoot);
            }

            GameObject arenaRoot = new GameObject(ArenaRootName);
            Undo.RegisterCreatedObjectUndo(arenaRoot, "Create Collect Arena");
            arenaRoot.AddComponent<LevelContext>();

            CreateFloor(arenaRoot.transform);
            CreateWalls(arenaRoot.transform);
            CreatePlateaus(arenaRoot.transform);
            CreateSlopes(arenaRoot.transform);
            CreateObstacles(arenaRoot.transform);
            CreateCollectibles(arenaRoot.transform);
            CreatePlayerSpawnPoint(arenaRoot.transform);
            CreateSpawnPoints(arenaRoot.transform);

            LevelContext levelContext = arenaRoot.GetComponent<LevelContext>();
            if (levelContext != null)
            {
                levelContext.RefreshSpawnPointsIfNeeded();
                EditorUtility.SetDirty(levelContext);
            }

            Selection.activeGameObject = arenaRoot;
            EditorGUIUtility.PingObject(arenaRoot);
        }

        [MenuItem("Brain Eaters/Save Collect Arena Prefab And Configure Level")]
        public static void SaveCollectArenaPrefabAndConfigureLevel()
        {
            GameObject arenaRoot = GameObject.Find(ArenaRootName);
            if (arenaRoot == null)
            {
                Debug.LogError($"Could not find '{ArenaRootName}' in the current scene. Build the collect arena first.");
                return;
            }

            EnsureFolder("Assets/BrainEaters", "Prefabs");

            LevelContext levelContext = arenaRoot.GetComponent<LevelContext>();
            if (levelContext == null)
            {
                levelContext = Undo.AddComponent<LevelContext>(arenaRoot);
            }

            levelContext.RefreshSpawnPointsIfNeeded();
            EditorUtility.SetDirty(levelContext);

            PrefabUtility.SaveAsPrefabAssetAndConnect(arenaRoot, CollectPrefabPath, InteractionMode.UserAction);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            GameObject levelPrefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(CollectPrefabPath);
            if (levelPrefabRoot == null)
            {
                Debug.LogError($"Could not load prefab at '{CollectPrefabPath}'.");
                return;
            }

            ConfigureLevelAsset(levelPrefabRoot.GetComponent<LevelContext>());
            AddLevelToCampaign();

            Debug.Log($"Collect level configured. Prefab saved at '{CollectPrefabPath}' and LevelConfig_Level03 updated.", levelPrefabRoot);
        }

        private static void ConfigureLevelAsset(LevelContext levelPrefab)
        {
            if (levelPrefab == null)
            {
                Debug.LogError("Collect prefab is missing LevelContext.");
                return;
            }

            LevelConfig levelConfig = AssetDatabase.LoadAssetAtPath<LevelConfig>(LevelConfigPath);
            if (levelConfig == null)
            {
                Debug.LogError($"Could not find '{LevelConfigPath}'.");
                return;
            }

            SerializedObject serializedObject = new SerializedObject(levelConfig);
            serializedObject.FindProperty("levelId").stringValue = "level_03";
            serializedObject.FindProperty("displayName").stringValue = "Collect";
            serializedObject.FindProperty("gameModeType").enumValueIndex = (int)GameModeType.Collect;
            serializedObject.FindProperty("levelPrefab").objectReferenceValue = levelPrefab;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(levelConfig);
            AssetDatabase.SaveAssets();
        }

        private static void AddLevelToCampaign()
        {
            CampaignConfig campaignConfig = AssetDatabase.LoadAssetAtPath<CampaignConfig>(CampaignConfigPath);
            LevelConfig levelConfig = AssetDatabase.LoadAssetAtPath<LevelConfig>(LevelConfigPath);
            if (campaignConfig == null || levelConfig == null)
            {
                return;
            }

            SerializedObject serializedObject = new SerializedObject(campaignConfig);
            SerializedProperty levelsProperty = serializedObject.FindProperty("levels");
            for (int i = 0; i < levelsProperty.arraySize; i++)
            {
                if (levelsProperty.GetArrayElementAtIndex(i).objectReferenceValue == levelConfig)
                {
                    return;
                }
            }

            levelsProperty.InsertArrayElementAtIndex(levelsProperty.arraySize);
            levelsProperty.GetArrayElementAtIndex(levelsProperty.arraySize - 1).objectReferenceValue = levelConfig;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(campaignConfig);
            AssetDatabase.SaveAssets();
        }

        private static void EnsureFolder(string parentPath, string folderName)
        {
            string folderPath = $"{parentPath}/{folderName}";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder(parentPath, folderName);
            }
        }

        private static void CreateFloor(Transform parent)
        {
            GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = "Floor";
            floor.transform.SetParent(parent);
            floor.transform.localPosition = new Vector3(0f, -1f, 0f);
            floor.transform.localScale = new Vector3(72f, 2f, 72f);
            floor.GetComponent<MeshRenderer>().sharedMaterial = CreatePreviewMaterial("Collect_Floor", new Color(0.14f, 0.16f, 0.18f, 1f));
            RegisterCreatedObject(floor);
        }

        private static void CreateWalls(Transform parent)
        {
            CreateWall(parent, "Wall_North", new Vector3(0f, 3f, 36f), new Vector3(72f, 6f, 1f));
            CreateWall(parent, "Wall_South", new Vector3(0f, 3f, -36f), new Vector3(72f, 6f, 1f));
            CreateWall(parent, "Wall_East", new Vector3(36f, 3f, 0f), new Vector3(1f, 6f, 72f));
            CreateWall(parent, "Wall_West", new Vector3(-36f, 3f, 0f), new Vector3(1f, 6f, 72f));
        }

        private static void CreateWall(Transform parent, string name, Vector3 localPosition, Vector3 localScale)
        {
            GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
            wall.name = name;
            wall.transform.SetParent(parent);
            wall.transform.localPosition = localPosition;
            wall.transform.localScale = localScale;
            wall.GetComponent<MeshRenderer>().sharedMaterial = CreatePreviewMaterial("Collect_Wall", new Color(0.2f, 0.24f, 0.28f, 1f));
            RegisterCreatedObject(wall);
        }

        private static void CreatePlateaus(Transform parent)
        {
            GameObject platformsRoot = new GameObject("Platforms");
            platformsRoot.transform.SetParent(parent);
            RegisterCreatedObject(platformsRoot);

            CreatePlatform(platformsRoot.transform, "Plateau_Center", new Vector3(-2f, 2f, 0f), new Vector3(18f, 4f, 18f));
            CreatePlatform(platformsRoot.transform, "Plateau_NorthRidge", new Vector3(8f, 6f, 20f), new Vector3(22f, 2f, 12f));
            CreatePlatform(platformsRoot.transform, "Plateau_EastSpine", new Vector3(16f, 6f, 4f), new Vector3(10f, 2f, 20f));
            CreatePlatform(platformsRoot.transform, "Plateau_EastLookout", new Vector3(24f, 8f, -10f), new Vector3(12f, 2f, 12f));
            CreatePlatform(platformsRoot.transform, "Plateau_SouthShelf", new Vector3(-18f, 3f, -20f), new Vector3(14f, 2f, 10f));

            CreateSupport(platformsRoot.transform, "LookoutSupport_A", new Vector3(20f, 4f, -14f), new Vector3(2f, 8f, 2f));
            CreateSupport(platformsRoot.transform, "LookoutSupport_B", new Vector3(28f, 4f, -6f), new Vector3(2f, 8f, 2f));
        }

        private static void CreateSlopes(Transform parent)
        {
            GameObject slopesRoot = new GameObject("Slopes");
            slopesRoot.transform.SetParent(parent);
            RegisterCreatedObject(slopesRoot);

            CreateSlope(slopesRoot.transform, "Slope_WestToCenter", new Vector3(-20f, 1.15f, 0f), new Vector3(24f, 1.5f, 14f), new Vector3(0f, 0f, -14f));
            CreateSlope(slopesRoot.transform, "Slope_CenterToNorth", new Vector3(6f, 4.1f, 10f), new Vector3(20f, 1.5f, 16f), new Vector3(18f, 0f, 0f));
            CreateSlope(slopesRoot.transform, "Slope_SpineToLookout", new Vector3(22f, 8.1f, -6f), new Vector3(12f, 1.5f, 10f), new Vector3(-18f, 0f, 0f));
            CreateSlope(slopesRoot.transform, "Slope_SouthToShelf", new Vector3(-20f, 1.6f, -10f), new Vector3(16f, 1.5f, 12f), new Vector3(-18f, 0f, 0f));
            CreateSlope(slopesRoot.transform, "Slope_CenterSpine", new Vector3(-4f, 3.8f, -10f), new Vector3(12f, 1.5f, 14f), new Vector3(-16f, 0f, 0f));
        }

        private static void CreatePlatform(Transform parent, string name, Vector3 localPosition, Vector3 localScale)
        {
            GameObject platform = GameObject.CreatePrimitive(PrimitiveType.Cube);
            platform.name = name;
            platform.transform.SetParent(parent);
            platform.transform.localPosition = localPosition;
            platform.transform.localScale = localScale;
            platform.GetComponent<MeshRenderer>().sharedMaterial = CreatePreviewMaterial("Collect_Plateau", new Color(0.3f, 0.34f, 0.18f, 1f));
            RegisterCreatedObject(platform);
        }

        private static void CreateSlope(Transform parent, string name, Vector3 localPosition, Vector3 localScale, Vector3 localEulerAngles)
        {
            GameObject slope = GameObject.CreatePrimitive(PrimitiveType.Cube);
            slope.name = name;
            slope.transform.SetParent(parent);
            slope.transform.localPosition = localPosition;
            slope.transform.localScale = localScale;
            slope.transform.localEulerAngles = localEulerAngles;
            slope.GetComponent<MeshRenderer>().sharedMaterial = CreatePreviewMaterial("Collect_Slope", new Color(0.52f, 0.45f, 0.2f, 1f));
            RegisterCreatedObject(slope);
        }

        private static void CreateSupport(Transform parent, string name, Vector3 localPosition, Vector3 localScale)
        {
            GameObject support = GameObject.CreatePrimitive(PrimitiveType.Cube);
            support.name = name;
            support.transform.SetParent(parent);
            support.transform.localPosition = localPosition;
            support.transform.localScale = localScale;
            support.GetComponent<MeshRenderer>().sharedMaterial = CreatePreviewMaterial("Collect_Support", new Color(0.25f, 0.21f, 0.18f, 1f));
            RegisterCreatedObject(support);
        }

        private static void CreateObstacles(Transform parent)
        {
            GameObject obstaclesRoot = new GameObject("Obstacles");
            obstaclesRoot.transform.SetParent(parent);
            RegisterCreatedObject(obstaclesRoot);

            CreateObstacle(obstaclesRoot.transform, "Obstacle_01", new Vector3(-28f, 1.5f, -6f), new Vector3(5f, 3f, 12f));
            CreateObstacle(obstaclesRoot.transform, "Obstacle_02", new Vector3(-12f, 1f, 24f), new Vector3(14f, 2f, 4f));
            CreateObstacle(obstaclesRoot.transform, "Obstacle_03", new Vector3(14f, 1.5f, 28f), new Vector3(6f, 3f, 6f));
            CreateObstacle(obstaclesRoot.transform, "Obstacle_04", new Vector3(30f, 2f, 12f), new Vector3(4f, 4f, 14f));
            CreateObstacle(obstaclesRoot.transform, "Obstacle_05", new Vector3(8f, 1f, -26f), new Vector3(18f, 2f, 4f));
            CreateObstacle(obstaclesRoot.transform, "Obstacle_06", new Vector3(-4f, 3f, 20f), new Vector3(4f, 6f, 4f));
            CreateObstacle(obstaclesRoot.transform, "Obstacle_07", new Vector3(22f, 4f, -20f), new Vector3(3f, 8f, 3f));
        }

        private static void CreateObstacle(Transform parent, string name, Vector3 localPosition, Vector3 localScale)
        {
            GameObject obstacle = GameObject.CreatePrimitive(PrimitiveType.Cube);
            obstacle.name = name;
            obstacle.transform.SetParent(parent);
            obstacle.transform.localPosition = localPosition;
            obstacle.transform.localScale = localScale;
            obstacle.GetComponent<MeshRenderer>().sharedMaterial = CreatePreviewMaterial("Collect_Obstacle", new Color(0.42f, 0.25f, 0.2f, 1f));
            RegisterCreatedObject(obstacle);
        }

        private static void CreateCollectibles(Transform parent)
        {
            GameObject pickupsRoot = new GameObject("Collectibles");
            pickupsRoot.transform.SetParent(parent);
            RegisterCreatedObject(pickupsRoot);

            CreateCollectible(pickupsRoot.transform, "Pickup_A", new Vector3(-30f, 1.2f, 28f));
            CreateCollectible(pickupsRoot.transform, "Pickup_B", new Vector3(-22f, 1.2f, -24f));
            CreateCollectible(pickupsRoot.transform, "Pickup_C", new Vector3(-8f, 4.5f, 0f));
            CreateCollectible(pickupsRoot.transform, "Pickup_D", new Vector3(2f, 4.5f, 8f));
            CreateCollectible(pickupsRoot.transform, "Pickup_E", new Vector3(6f, 8.2f, 20f));
            CreateCollectible(pickupsRoot.transform, "Pickup_F", new Vector3(14f, 8.2f, 24f));
            CreateCollectible(pickupsRoot.transform, "Pickup_G", new Vector3(24f, 10.2f, -10f));
            CreateCollectible(pickupsRoot.transform, "Pickup_H", new Vector3(28f, 10.2f, -6f));
            CreateCollectible(pickupsRoot.transform, "Pickup_I", new Vector3(-18f, 4.2f, -20f));
            CreateCollectible(pickupsRoot.transform, "Pickup_J", new Vector3(30f, 1.2f, 28f));
        }

        private static void CreateCollectible(Transform parent, string name, Vector3 localPosition)
        {
            GameObject pickupRoot = new GameObject(name);
            pickupRoot.transform.SetParent(parent);
            pickupRoot.transform.localPosition = localPosition;
            RegisterCreatedObject(pickupRoot);

            SphereCollider trigger = pickupRoot.AddComponent<SphereCollider>();
            trigger.isTrigger = true;
            trigger.radius = 1f;

            GameObject visualRoot = new GameObject("VisualRoot");
            visualRoot.transform.SetParent(pickupRoot.transform);
            visualRoot.transform.localPosition = Vector3.zero;
            RegisterCreatedObject(visualRoot);

            GameObject coin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            coin.name = "Coin";
            coin.transform.SetParent(visualRoot.transform);
            coin.transform.localPosition = Vector3.zero;
            coin.transform.localEulerAngles = new Vector3(90f, 0f, 0f);
            coin.transform.localScale = new Vector3(0.75f, 0.1f, 0.75f);
            Object.DestroyImmediate(coin.GetComponent<CapsuleCollider>());

            MeshRenderer coinRenderer = coin.GetComponent<MeshRenderer>();
            coinRenderer.sharedMaterial = CreatePreviewMaterial("Collect_Coin", new Color(1f, 0.8f, 0.2f, 1f));
            RegisterCreatedObject(coin);

            GameObject labelRoot = new GameObject("StateLabel");
            labelRoot.transform.SetParent(pickupRoot.transform);
            labelRoot.transform.localPosition = new Vector3(0f, 1.25f, 0f);
            TextMeshPro label = labelRoot.AddComponent<TextMeshPro>();
            label.text = "COLLECT";
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 2.5f;
            label.color = Color.white;

            CollectPickup collectPickup = pickupRoot.AddComponent<CollectPickup>();
            SerializedObject serializedObject = new SerializedObject(collectPickup);
            serializedObject.FindProperty("visualRoot").objectReferenceValue = visualRoot.transform;
            serializedObject.FindProperty("pickupRenderer").objectReferenceValue = coinRenderer;
            serializedObject.FindProperty("stateLabel").objectReferenceValue = label;
            serializedObject.FindProperty("triggerCollider").objectReferenceValue = trigger;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateSpawnPoints(Transform parent)
        {
            GameObject spawnRoot = new GameObject("SpawnPoints");
            spawnRoot.transform.SetParent(parent);
            spawnRoot.transform.localPosition = Vector3.zero;
            RegisterCreatedObject(spawnRoot);

            CreateSpawnPoint(spawnRoot.transform, "SpawnPoint_01", new Vector3(-30f, 0f, -30f));
            CreateSpawnPoint(spawnRoot.transform, "SpawnPoint_02", new Vector3(-30f, 0f, 30f));
            CreateSpawnPoint(spawnRoot.transform, "SpawnPoint_03", new Vector3(30f, 0f, -30f));
            CreateSpawnPoint(spawnRoot.transform, "SpawnPoint_04", new Vector3(30f, 0f, 30f));
            CreateSpawnPoint(spawnRoot.transform, "SpawnPoint_05", new Vector3(0f, 0f, 32f));
            CreateSpawnPoint(spawnRoot.transform, "SpawnPoint_06", new Vector3(32f, 0f, 0f));
        }

        private static void CreatePlayerSpawnPoint(Transform parent)
        {
            GameObject root = new GameObject("PlayerSpawn");
            root.transform.SetParent(parent);
            root.transform.localPosition = Vector3.zero;
            RegisterCreatedObject(root);

            GameObject point = PlayerSpawnPointBuilder.CreatePlayerSpawnPointObject(root.transform, "PlayerSpawnPoint", new Vector3(-28f, 0f, -18f), Quaternion.Euler(0f, 35f, 0f));
            RegisterCreatedObject(point);
        }

        private static void CreateSpawnPoint(Transform parent, string pointName, Vector3 localPosition)
        {
            GameObject point = SpawnPointBuilder.CreateSpawnPointObject(parent, pointName, localPosition);
            RegisterCreatedObject(point);
        }

        private static Material CreatePreviewMaterial(string materialName, Color color)
        {
            return EditorMaterialUtility.GetOrCreateLitMaterialAsset(materialName, color);
        }

        private static void RegisterCreatedObject(GameObject gameObject)
        {
            Undo.RegisterCreatedObjectUndo(gameObject, "Create Collect Arena Piece");
        }
    }
}
