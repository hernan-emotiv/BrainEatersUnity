using BrainEaters.Configs;
using BrainEaters.Enemies;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace BrainEaters.EditorTools
{
    public static class FastEnemyBuilder
    {
        private const string GeneratedDataFolderPath = "Assets/BrainEaters/Data/Generated";
        private const string GeneratedPrefabsFolderPath = "Assets/BrainEaters/Prefabs/Generated";
        private const string EnemyConfigPath = GeneratedDataFolderPath + "/EnemyConfig_CreeperFast.asset";
        private const string EnemyPrefabPath = GeneratedPrefabsFolderPath + "/EnemyFast_Creeper.prefab";

        [MenuItem("Brain Eaters/Build Fast Enemy Assets")]
        public static void BuildFastEnemyAssets()
        {
            EnsureFolders();

            EnemyConfig enemyConfig = CreateOrUpdateEnemyConfig();
            EnemyController enemyPrefab = CreateOrUpdateEnemyPrefab();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Object focusTarget = enemyPrefab != null ? enemyPrefab.gameObject : enemyConfig;
            Selection.activeObject = focusTarget;
            EditorGUIUtility.PingObject(focusTarget);
            Debug.Log($"Fast enemy assets ready. Config: '{EnemyConfigPath}', Prefab: '{EnemyPrefabPath}'.", focusTarget);
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/BrainEaters/Data"))
            {
                AssetDatabase.CreateFolder("Assets/BrainEaters", "Data");
            }

            if (!AssetDatabase.IsValidFolder(GeneratedDataFolderPath))
            {
                AssetDatabase.CreateFolder("Assets/BrainEaters/Data", "Generated");
            }

            if (!AssetDatabase.IsValidFolder("Assets/BrainEaters/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets/BrainEaters", "Prefabs");
            }

            if (!AssetDatabase.IsValidFolder(GeneratedPrefabsFolderPath))
            {
                AssetDatabase.CreateFolder("Assets/BrainEaters/Prefabs", "Generated");
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
            serializedObject.FindProperty("enemyType").enumValueIndex = (int)EnemyType.Special;
            serializedObject.FindProperty("displayName").stringValue = "Creeper Fast";
            serializedObject.FindProperty("maxHealth").floatValue = 1f;
            serializedObject.FindProperty("moveSpeed").floatValue = 2.25f;
            serializedObject.FindProperty("turnSpeed").floatValue = 900f;
            serializedObject.FindProperty("stopDistance").floatValue = 1f;
            serializedObject.FindProperty("attackRange").floatValue = 1.35f;
            serializedObject.FindProperty("attackDamage").floatValue = 1f;
            serializedObject.FindProperty("attackHitDelaySeconds").floatValue = 0.2f;
            serializedObject.FindProperty("attackDurationSeconds").floatValue = 0.55f;
            serializedObject.FindProperty("attackCooldownSeconds").floatValue = 0.8f;
            serializedObject.FindProperty("attackVisualDurationSeconds").floatValue = 0.12f;
            serializedObject.FindProperty("useAttackVisual").boolValue = true;
            serializedObject.FindProperty("attackHitboxHalfExtents").vector3Value = new Vector3(0.45f, 0.7f, 0.55f);
            serializedObject.FindProperty("destroyDelaySeconds").floatValue = 1.5f;
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

            GameObject enemy = new GameObject("EnemyFast_Creeper");
            ConfigureEnemyPrefabRoot(enemy);

            GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(enemy, EnemyPrefabPath);
            Object.DestroyImmediate(enemy);
            return prefabAsset.GetComponent<EnemyController>();
        }

        private static void ConfigureEnemyPrefabRoot(GameObject enemy)
        {
            enemy.transform.localPosition = Vector3.zero;
            enemy.transform.localRotation = Quaternion.identity;
            enemy.transform.localScale = Vector3.one;

            BoxCollider collider = GetOrAddComponent<BoxCollider>(enemy);
            collider.center = new Vector3(0f, 0.7f, 0f);
            collider.size = new Vector3(0.95f, 1.4f, 0.95f);

            NavMeshAgent navMeshAgent = GetOrAddComponent<NavMeshAgent>(enemy);
            navMeshAgent.radius = 0.42f;
            navMeshAgent.height = 1.4f;
            navMeshAgent.baseOffset = 0f;
            navMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
            navMeshAgent.autoBraking = false;

            EnsureVisuals(enemy.transform);
            Transform attackOrigin = EnsureAttackOrigin(enemy.transform);

            GetOrAddComponent<EnemyMovement>(enemy);
            GetOrAddComponent<EnemyHealth>(enemy);
            EnemyAttack enemyAttack = GetOrAddComponent<EnemyAttack>(enemy);
            GetOrAddComponent<EnemyController>(enemy);

            EnemyHopVisual hopVisual = GetOrAddComponent<EnemyHopVisual>(enemy);
            SerializedObject hopSerializedObject = new SerializedObject(hopVisual);
            hopSerializedObject.FindProperty("hopAmplitude").floatValue = 0.18f;
            hopSerializedObject.FindProperty("hopFrequency").floatValue = 10f;
            hopSerializedObject.FindProperty("landingSmoothing").floatValue = 14f;
            Transform visualRoot = enemy.transform.Find("VisualRoot");
            hopSerializedObject.FindProperty("visualRoot").objectReferenceValue = visualRoot;
            hopSerializedObject.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject attackSerializedObject = new SerializedObject(enemyAttack);
            attackSerializedObject.FindProperty("attackOrigin").objectReferenceValue = attackOrigin;
            attackSerializedObject.ApplyModifiedPropertiesWithoutUndo();

            EnemyDeathVisual deathVisual = GetOrAddComponent<EnemyDeathVisual>(enemy);
            SerializedObject deathSerializedObject = new SerializedObject(deathVisual);
            deathSerializedObject.FindProperty("visualRoot").objectReferenceValue = visualRoot;
            deathSerializedObject.FindProperty("deathDurationSeconds").floatValue = 0.28f;
            deathSerializedObject.FindProperty("collapsedScale").vector3Value = new Vector3(1.2f, 0.18f, 1.2f);
            deathSerializedObject.FindProperty("fallenEulerAngles").vector3Value = new Vector3(0f, 0f, 82f);
            deathSerializedObject.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(enemy);
        }

        private static void EnsureVisuals(Transform enemyRoot)
        {
            GameObject visualRoot = GetOrCreateChild(enemyRoot, "VisualRoot");
            visualRoot.transform.localPosition = Vector3.zero;
            visualRoot.transform.localRotation = Quaternion.identity;
            visualRoot.transform.localScale = Vector3.one;

            GameObject body = GetOrCreatePrimitiveChild(visualRoot.transform, "Body", PrimitiveType.Cube);
            body.transform.localPosition = new Vector3(0f, 0.7f, 0f);
            body.transform.localScale = new Vector3(0.9f, 1.4f, 0.9f);
            body.GetComponent<MeshRenderer>().sharedMaterial = EditorMaterialUtility.GetOrCreateLitMaterialAsset("EnemyFast_Body", new Color(0.28f, 0.85f, 0.28f, 1f));

            GameObject head = GetOrCreatePrimitiveChild(visualRoot.transform, "Head", PrimitiveType.Cube);
            head.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            head.transform.localScale = new Vector3(0.65f, 0.65f, 0.65f);
            head.GetComponent<MeshRenderer>().sharedMaterial = EditorMaterialUtility.GetOrCreateLitMaterialAsset("EnemyFast_Head", new Color(0.18f, 0.55f, 0.18f, 1f));

            GameObject eyeLeft = GetOrCreatePrimitiveChild(visualRoot.transform, "EyeLeft", PrimitiveType.Cube);
            eyeLeft.transform.localPosition = new Vector3(-0.14f, 1.64f, 0.33f);
            eyeLeft.transform.localScale = new Vector3(0.1f, 0.16f, 0.08f);
            eyeLeft.GetComponent<MeshRenderer>().sharedMaterial = EditorMaterialUtility.GetOrCreateLitMaterialAsset("EnemyFast_Eye", Color.black);

            GameObject eyeRight = GetOrCreatePrimitiveChild(visualRoot.transform, "EyeRight", PrimitiveType.Cube);
            eyeRight.transform.localPosition = new Vector3(0.14f, 1.64f, 0.33f);
            eyeRight.transform.localScale = new Vector3(0.1f, 0.16f, 0.08f);
            eyeRight.GetComponent<MeshRenderer>().sharedMaterial = EditorMaterialUtility.GetOrCreateLitMaterialAsset("EnemyFast_Eye", Color.black);
        }

        private static Transform EnsureAttackOrigin(Transform enemyRoot)
        {
            GameObject attackOrigin = GetOrCreateChild(enemyRoot, "AttackOrigin");
            attackOrigin.transform.localPosition = new Vector3(0f, 0.65f, 0.48f);
            attackOrigin.transform.localRotation = Quaternion.identity;
            attackOrigin.transform.localScale = Vector3.one * 0.5f;
            return attackOrigin.transform;
        }

        private static GameObject GetOrCreateChild(Transform parent, string name)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject child = new GameObject(name);
            child.transform.SetParent(parent);
            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = Vector3.one;
            return child;
        }

        private static GameObject GetOrCreatePrimitiveChild(Transform parent, string name, PrimitiveType primitiveType)
        {
            Transform existing = parent.Find(name);
            if (existing != null)
            {
                return existing.gameObject;
            }

            GameObject child = GameObject.CreatePrimitive(primitiveType);
            child.name = name;
            child.transform.SetParent(parent);
            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = Vector3.one;
            return child;
        }

        private static T GetOrAddComponent<T>(GameObject gameObject) where T : Component
        {
            T existing = gameObject.GetComponent<T>();
            return existing != null ? existing : gameObject.AddComponent<T>();
        }
    }
}
