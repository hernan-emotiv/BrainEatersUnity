using BrainEaters.Cameras;
using BrainEaters.Configs;
using BrainEaters.Enemies;
using BrainEaters.GameFlow;
using BrainEaters.Input;
using BrainEaters.Player;
using BrainEaters.Spawning;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BrainEaters.EditorTools
{
    public static class PlayableSceneBuilder
    {
        private const string DataFolderPath = "Assets/BrainEaters/Data";
        private const string GeneratedDataFolderPath = "Assets/BrainEaters/Data/Generated";
        private const string PrefabsFolderPath = "Assets/BrainEaters/Prefabs";
        private const string GeneratedPrefabsFolderPath = "Assets/BrainEaters/Prefabs/Generated";

        private const string EnemyConfigPath = GeneratedDataFolderPath + "/EnemyConfig_ZombieBasic.asset";
        private const string SpawnConfigPath = GeneratedDataFolderPath + "/SpawnConfig_Level01.asset";
        private const string LevelConfigPath = GeneratedDataFolderPath + "/LevelConfig_Level01.asset";
        private const string EnemyPrefabPath = GeneratedPrefabsFolderPath + "/EnemyBasic.prefab";

        private const string PlayerName = "Player";
        private const string SpawnManagerName = "SpawnManager";
        private const string GameManagerName = "GameManager";
        private const string DirectionalLightName = "Directional Light";

        [MenuItem("Brain Eaters/Build Playable GameScene Setup")]
        public static void BuildPlayableGameSceneSetup()
        {
            EnsureFolders();

            ArenaBuilder.BuildArenaInCurrentScene();
            GameObject arenaRoot = GameObject.Find("Arena_Level01");
            SpawnPoint[] spawnPoints = Object.FindObjectsByType<SpawnPoint>(FindObjectsSortMode.None);

            EnemyConfig enemyConfig = CreateOrUpdateEnemyConfig();
            SpawnConfig spawnConfig = CreateOrUpdateSpawnConfig();
            EnemyController enemyPrefab = CreateOrUpdateEnemyPrefab();
            LevelConfig levelConfig = CreateOrUpdateLevelConfig(spawnConfig, enemyPrefab, enemyConfig);

            PlayerController playerController = CreateOrUpdatePlayer();
            CameraFollow cameraFollow = CreateOrUpdateCamera(playerController.transform);
            SpawnManager spawnManager = CreateOrUpdateSpawnManager();
            GameManager gameManager = CreateOrUpdateGameManager(levelConfig, playerController, spawnManager, cameraFollow, spawnPoints);

            CreateOrUpdateDirectionalLight();

            Selection.activeGameObject = gameManager.gameObject;
            EditorGUIUtility.PingObject(gameManager.gameObject);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());

            Debug.Log($"Playable Brain Eaters setup created in scene '{SceneManager.GetActiveScene().name}'. Arena root: {(arenaRoot != null ? arenaRoot.name : "missing")}", gameManager);
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets/BrainEaters", "Data");
            EnsureFolder(DataFolderPath, "Generated");
            EnsureFolder("Assets/BrainEaters", "Prefabs");
            EnsureFolder(PrefabsFolderPath, "Generated");
        }

        private static void EnsureFolder(string parentPath, string folderName)
        {
            string folderPath = $"{parentPath}/{folderName}";
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                AssetDatabase.CreateFolder(parentPath, folderName);
            }
        }

        private static EnemyConfig CreateOrUpdateEnemyConfig()
        {
            EnemyConfig asset = AssetDatabase.LoadAssetAtPath<EnemyConfig>(EnemyConfigPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<EnemyConfig>();
                AssetDatabase.CreateAsset(asset, EnemyConfigPath);
            }

            SerializedObject serializedObject = new SerializedObject(asset);
            serializedObject.FindProperty("maxHealth").floatValue = 1f;
            serializedObject.FindProperty("moveSpeed").floatValue = 3.5f;
            serializedObject.FindProperty("stopDistance").floatValue = 1.25f;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static SpawnConfig CreateOrUpdateSpawnConfig()
        {
            SpawnConfig asset = AssetDatabase.LoadAssetAtPath<SpawnConfig>(SpawnConfigPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<SpawnConfig>();
                AssetDatabase.CreateAsset(asset, SpawnConfigPath);
            }

            SerializedObject serializedObject = new SerializedObject(asset);
            serializedObject.FindProperty("initialDelaySeconds").floatValue = 1f;
            serializedObject.FindProperty("spawnIntervalSeconds").floatValue = 2f;
            serializedObject.FindProperty("maxAliveEnemies").intValue = 10;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static LevelConfig CreateOrUpdateLevelConfig(SpawnConfig spawnConfig, EnemyController enemyPrefab, EnemyConfig enemyConfig)
        {
            LevelConfig asset = AssetDatabase.LoadAssetAtPath<LevelConfig>(LevelConfigPath);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<LevelConfig>();
                AssetDatabase.CreateAsset(asset, LevelConfigPath);
            }

            SerializedObject serializedObject = new SerializedObject(asset);
            serializedObject.FindProperty("gameModeType").enumValueIndex = (int)GameModeType.Survival;
            serializedObject.FindProperty("survivalDurationSeconds").floatValue = 60f;
            serializedObject.FindProperty("spawnConfig").objectReferenceValue = spawnConfig;

            SerializedProperty enemyTypesProperty = serializedObject.FindProperty("enemyTypes");
            enemyTypesProperty.arraySize = 1;
            SerializedProperty enemyTypeEntry = enemyTypesProperty.GetArrayElementAtIndex(0);
            enemyTypeEntry.FindPropertyRelative("enemyPrefab").objectReferenceValue = enemyPrefab;
            enemyTypeEntry.FindPropertyRelative("enemyConfig").objectReferenceValue = enemyConfig;

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(asset);
            return asset;
        }

        private static EnemyController CreateOrUpdateEnemyPrefab()
        {
            GameObject prefabRoot = AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefabPath);
            if (prefabRoot != null)
            {
                GameObject prefabContents = PrefabUtility.LoadPrefabContents(EnemyPrefabPath);
                ConfigureEnemyPrefabRoot(prefabContents);
                PrefabUtility.SaveAsPrefabAsset(prefabContents, EnemyPrefabPath);
                PrefabUtility.UnloadPrefabContents(prefabContents);
                return AssetDatabase.LoadAssetAtPath<GameObject>(EnemyPrefabPath).GetComponent<EnemyController>();
            }

            GameObject enemy = GameObject.CreatePrimitive(PrimitiveType.Cube);
            enemy.name = "EnemyBasic";
            ConfigureEnemyPrefabRoot(enemy);

            GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(enemy, EnemyPrefabPath);
            Object.DestroyImmediate(enemy);
            AssetDatabase.SaveAssets();
            return prefabAsset.GetComponent<EnemyController>();
        }

        private static PlayerController CreateOrUpdatePlayer()
        {
            PlayerController existingPlayer = Object.FindFirstObjectByType<PlayerController>();
            if (existingPlayer != null)
            {
                ConfigurePlayer(existingPlayer.gameObject);
                return existingPlayer;
            }

            GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            player.name = PlayerName;
            player.transform.position = new Vector3(0f, 1f, 0f);
            Undo.RegisterCreatedObjectUndo(player, "Create Player");

            ConfigurePlayer(player);
            return player.GetComponent<PlayerController>();
        }

        private static void ConfigurePlayer(GameObject player)
        {
            player.transform.position = new Vector3(0f, 1f, 0f);
            player.transform.rotation = Quaternion.identity;
            player.transform.localScale = Vector3.one;

            CapsuleCollider capsuleCollider = player.GetComponent<CapsuleCollider>();
            if (capsuleCollider != null)
            {
                Object.DestroyImmediate(capsuleCollider);
            }

            MeshFilter rootMeshFilter = player.GetComponent<MeshFilter>();
            if (rootMeshFilter != null)
            {
                Object.DestroyImmediate(rootMeshFilter);
            }

            MeshRenderer rootMeshRenderer = player.GetComponent<MeshRenderer>();
            if (rootMeshRenderer != null)
            {
                Object.DestroyImmediate(rootMeshRenderer);
            }

            CharacterController characterController = player.GetComponent<CharacterController>();
            if (characterController == null)
            {
                characterController = Undo.AddComponent<CharacterController>(player);
            }

            characterController.center = Vector3.zero;
            characterController.height = 2f;
            characterController.radius = 0.45f;

            EnsurePlayerVisual(player.transform);

            KeyboardMouseInputSource inputSource = GetOrAddComponent<KeyboardMouseInputSource>(player);
            PlayerInputRouter inputRouter = GetOrAddComponent<PlayerInputRouter>(player);
            GetOrAddComponent<PlayerMovement>(player);
            PlayerEnergyCharge energyCharge = GetOrAddComponent<PlayerEnergyCharge>(player);
            PlayerBombAttack bombAttack = GetOrAddComponent<PlayerBombAttack>(player);
            PlayerController controller = GetOrAddComponent<PlayerController>(player);

            inputRouter.SetInputSource(inputSource);

            SerializedObject energySerializedObject = new SerializedObject(energyCharge);
            energySerializedObject.FindProperty("maxEnergy").floatValue = 100f;
            energySerializedObject.FindProperty("chargeRatePerSecond").floatValue = 30f;
            energySerializedObject.FindProperty("bombEnergyCost").floatValue = 50f;
            energySerializedObject.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject bombSerializedObject = new SerializedObject(bombAttack);
            bombSerializedObject.FindProperty("radius").floatValue = 6f;
            bombSerializedObject.FindProperty("damage").floatValue = 999f;
            bombSerializedObject.FindProperty("cooldownSeconds").floatValue = 0.75f;
            bombSerializedObject.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(controller);
        }

        private static CameraFollow CreateOrUpdateCamera(Transform target)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                GameObject cameraObject = new GameObject("Main Camera");
                Undo.RegisterCreatedObjectUndo(cameraObject, "Create Main Camera");
                mainCamera = cameraObject.AddComponent<Camera>();
                cameraObject.tag = "MainCamera";
                cameraObject.AddComponent<AudioListener>();
            }
            else if (mainCamera.GetComponent<AudioListener>() == null)
            {
                mainCamera.gameObject.AddComponent<AudioListener>();
            }

            mainCamera.transform.position = new Vector3(0f, 8f, -8f);
            mainCamera.transform.rotation = Quaternion.Euler(35f, 0f, 0f);

            CameraFollow cameraFollow = GetOrAddComponent<CameraFollow>(mainCamera.gameObject);
            cameraFollow.SetTarget(target);
            return cameraFollow;
        }

        private static SpawnManager CreateOrUpdateSpawnManager()
        {
            SpawnManager spawnManager = Object.FindFirstObjectByType<SpawnManager>();
            if (spawnManager != null)
            {
                return spawnManager;
            }

            GameObject gameObject = new GameObject(SpawnManagerName);
            Undo.RegisterCreatedObjectUndo(gameObject, "Create SpawnManager");
            return gameObject.AddComponent<SpawnManager>();
        }

        private static GameManager CreateOrUpdateGameManager(LevelConfig levelConfig, PlayerController playerController, SpawnManager spawnManager, CameraFollow cameraFollow, SpawnPoint[] spawnPoints)
        {
            GameManager gameManager = Object.FindFirstObjectByType<GameManager>();
            if (gameManager == null)
            {
                GameObject gameObject = new GameObject(GameManagerName);
                Undo.RegisterCreatedObjectUndo(gameObject, "Create GameManager");
                gameManager = gameObject.AddComponent<GameManager>();
            }

            SerializedObject serializedObject = new SerializedObject(gameManager);
            serializedObject.FindProperty("levelConfig").objectReferenceValue = levelConfig;
            serializedObject.FindProperty("playerController").objectReferenceValue = playerController;
            serializedObject.FindProperty("spawnManager").objectReferenceValue = spawnManager;
            serializedObject.FindProperty("cameraFollow").objectReferenceValue = cameraFollow;

            SerializedProperty spawnPointsProperty = serializedObject.FindProperty("spawnPoints");
            spawnPointsProperty.arraySize = spawnPoints.Length;
            for (int i = 0; i < spawnPoints.Length; i++)
            {
                spawnPointsProperty.GetArrayElementAtIndex(i).objectReferenceValue = spawnPoints[i];
            }

            serializedObject.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(gameManager);
            return gameManager;
        }

        private static void CreateOrUpdateDirectionalLight()
        {
            Light existingLight = Object.FindFirstObjectByType<Light>();
            if (existingLight != null && existingLight.type == LightType.Directional)
            {
                existingLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
                return;
            }

            GameObject lightObject = new GameObject(DirectionalLightName);
            Undo.RegisterCreatedObjectUndo(lightObject, "Create Directional Light");
            Light light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.2f;
            light.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static void ConfigureEnemyPrefabRoot(GameObject enemy)
        {
            enemy.transform.localPosition = Vector3.zero;
            enemy.transform.localRotation = Quaternion.identity;
            enemy.transform.localScale = Vector3.one;

            MeshFilter rootMeshFilter = enemy.GetComponent<MeshFilter>();
            if (rootMeshFilter != null)
            {
                Object.DestroyImmediate(rootMeshFilter);
            }

            MeshRenderer rootMeshRenderer = enemy.GetComponent<MeshRenderer>();
            if (rootMeshRenderer != null)
            {
                Object.DestroyImmediate(rootMeshRenderer);
            }

            BoxCollider collider = GetOrAddComponent<BoxCollider>(enemy);
            collider.center = new Vector3(0f, 0.75f, 0f);
            collider.size = new Vector3(1f, 1.5f, 1f);

            EnsureEnemyVisual(enemy.transform);
            GetOrAddComponent<EnemyMovement>(enemy);
            GetOrAddComponent<EnemyHealth>(enemy);
            GetOrAddComponent<EnemyController>(enemy);
        }

        private static void EnsurePlayerVisual(Transform playerRoot)
        {
            GameObject visual = GetOrCreateChild(playerRoot, "Visual");
            EnsurePrimitiveVisual(visual, PrimitiveType.Capsule, Vector3.zero, Vector3.one);
        }

        private static void EnsureEnemyVisual(Transform enemyRoot)
        {
            GameObject visual = GetOrCreateChild(enemyRoot, "Visual");
            EnsurePrimitiveVisual(visual, PrimitiveType.Cube, new Vector3(0f, 0.75f, 0f), new Vector3(1f, 1.5f, 1f));
        }

        private static GameObject GetOrCreateChild(Transform parent, string childName)
        {
            Transform existingChild = parent.Find(childName);
            if (existingChild != null)
            {
                return existingChild.gameObject;
            }

            GameObject child = new GameObject(childName);
            child.transform.SetParent(parent);
            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = Vector3.one;
            return child;
        }

        private static void EnsurePrimitiveVisual(GameObject visualRoot, PrimitiveType primitiveType, Vector3 localPosition, Vector3 localScale)
        {
            BoxCollider boxCollider = visualRoot.GetComponent<BoxCollider>();
            if (boxCollider != null)
            {
                Object.DestroyImmediate(boxCollider);
            }

            CapsuleCollider capsuleCollider = visualRoot.GetComponent<CapsuleCollider>();
            if (capsuleCollider != null)
            {
                Object.DestroyImmediate(capsuleCollider);
            }

            MeshFilter meshFilter = visualRoot.GetComponent<MeshFilter>();
            MeshRenderer meshRenderer = visualRoot.GetComponent<MeshRenderer>();

            GameObject tempPrimitive = GameObject.CreatePrimitive(primitiveType);
            MeshFilter primitiveMeshFilter = tempPrimitive.GetComponent<MeshFilter>();
            MeshRenderer primitiveMeshRenderer = tempPrimitive.GetComponent<MeshRenderer>();

            if (meshFilter == null)
            {
                meshFilter = Undo.AddComponent<MeshFilter>(visualRoot);
            }

            if (meshRenderer == null)
            {
                meshRenderer = Undo.AddComponent<MeshRenderer>(visualRoot);
            }

            meshFilter.sharedMesh = primitiveMeshFilter.sharedMesh;
            meshRenderer.sharedMaterials = primitiveMeshRenderer.sharedMaterials;

            Object.DestroyImmediate(tempPrimitive);

            visualRoot.transform.localPosition = localPosition;
            visualRoot.transform.localRotation = Quaternion.identity;
            visualRoot.transform.localScale = localScale;
        }

        private static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
        {
            T component = gameObject.GetComponent<T>();
            if (component == null)
            {
                component = Undo.AddComponent<T>(gameObject);
            }

            return component;
        }
    }
}
