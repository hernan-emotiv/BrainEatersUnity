using BrainEaters.GameFlow;
using BrainEaters.Turrets;
using TMPro;
using UnityEditor;
using UnityEngine;

namespace BrainEaters.EditorTools
{
    public static class TurretBuilder
    {
        private const string GeneratedPrefabsFolderPath = "Assets/BrainEaters/Prefabs/Generated";
        private const string TurretPrefabPath = GeneratedPrefabsFolderPath + "/ConstructibleTurret.prefab";
        private const string ProjectilePrefabPath = GeneratedPrefabsFolderPath + "/TurretProjectile.prefab";
        public const string TurretPrefabPathForEditor = TurretPrefabPath;

        [MenuItem("Brain Eaters/Build Turret Prefab")]
        public static void BuildTurretPrefab()
        {
            EnsureFolders();

            TurretProjectile projectilePrefab = CreateOrUpdateProjectilePrefab();
            GameObject turretRoot = CreateTurretRoot(projectilePrefab);

            GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(turretRoot, TurretPrefabPath);
            Object.DestroyImmediate(turretRoot);

            Selection.activeObject = prefabAsset;
            EditorGUIUtility.PingObject(prefabAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("Brain Eaters/Create Turret In Scene")]
        public static void CreateTurretInScene()
        {
            GameObject turretPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TurretPrefabPath);
            if (turretPrefab == null)
            {
                BuildTurretPrefab();
                turretPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TurretPrefabPath);
            }

            if (turretPrefab == null)
            {
                Debug.LogError("Could not create or load the turret prefab.");
                return;
            }

            GameObject instance = PrefabUtility.InstantiatePrefab(turretPrefab) as GameObject;
            if (instance == null)
            {
                return;
            }

            instance.name = "ConstructibleTurret";
            Undo.RegisterCreatedObjectUndo(instance, "Create Turret In Scene");
            Selection.activeGameObject = instance;
            EditorGUIUtility.PingObject(instance);
        }

        private static void EnsureFolders()
        {
            if (!AssetDatabase.IsValidFolder("Assets/BrainEaters/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets/BrainEaters", "Prefabs");
            }

            if (!AssetDatabase.IsValidFolder(GeneratedPrefabsFolderPath))
            {
                AssetDatabase.CreateFolder("Assets/BrainEaters/Prefabs", "Generated");
            }
        }

        private static GameObject CreateTurretRoot(TurretProjectile projectilePrefab)
        {
            GameObject root = new GameObject("ConstructibleTurret");

            GameObject offlineVisual = CreatePrimitiveChild(root.transform, "OfflineVisual", PrimitiveType.Cylinder);
            offlineVisual.transform.localPosition = new Vector3(0f, 0.4f, 0f);
            offlineVisual.transform.localScale = new Vector3(0.9f, 0.4f, 0.9f);
            offlineVisual.GetComponent<MeshRenderer>().sharedMaterial = EditorMaterialUtility.GetOrCreateLitMaterialAsset("Turret_Offline", new Color(0.35f, 0.28f, 0.22f, 1f));

            GameObject onlineVisual = new GameObject("OnlineVisual");
            onlineVisual.transform.SetParent(root.transform);
            onlineVisual.transform.localPosition = Vector3.zero;
            onlineVisual.transform.localRotation = Quaternion.identity;
            onlineVisual.transform.localScale = Vector3.one;

            GameObject baseVisual = CreatePrimitiveChild(onlineVisual.transform, "Base", PrimitiveType.Cylinder);
            baseVisual.transform.localPosition = new Vector3(0f, 0.3f, 0f);
            baseVisual.transform.localScale = new Vector3(1f, 0.3f, 1f);
            baseVisual.GetComponent<MeshRenderer>().sharedMaterial = EditorMaterialUtility.GetOrCreateLitMaterialAsset("Turret_Base", new Color(0.24f, 0.24f, 0.28f, 1f));

            GameObject headPivot = new GameObject("HeadPivot");
            headPivot.transform.SetParent(onlineVisual.transform);
            headPivot.transform.localPosition = new Vector3(0f, 0.65f, 0f);
            headPivot.transform.localRotation = Quaternion.identity;
            headPivot.transform.localScale = Vector3.one;

            GameObject head = CreatePrimitiveChild(headPivot.transform, "Head", PrimitiveType.Cube);
            head.transform.localPosition = new Vector3(0f, 0.2f, 0f);
            head.transform.localScale = new Vector3(0.7f, 0.4f, 0.7f);
            head.GetComponent<MeshRenderer>().sharedMaterial = EditorMaterialUtility.GetOrCreateLitMaterialAsset("Turret_Head", new Color(0.5f, 0.55f, 0.62f, 1f));

            GameObject barrel = CreatePrimitiveChild(headPivot.transform, "Barrel", PrimitiveType.Cube);
            barrel.transform.localPosition = new Vector3(0f, 0.18f, 0.58f);
            barrel.transform.localScale = new Vector3(0.16f, 0.16f, 0.9f);
            barrel.GetComponent<MeshRenderer>().sharedMaterial = EditorMaterialUtility.GetOrCreateLitMaterialAsset("Turret_Barrel", new Color(0.16f, 0.16f, 0.18f, 1f));

            GameObject muzzle = new GameObject("Muzzle");
            muzzle.transform.SetParent(headPivot.transform);
            muzzle.transform.localPosition = new Vector3(0f, 0.18f, 1.05f);
            muzzle.transform.localRotation = Quaternion.identity;
            muzzle.transform.localScale = Vector3.one;

            GameObject targetPoint = new GameObject("TargetPoint");
            targetPoint.transform.SetParent(root.transform);
            targetPoint.transform.localPosition = new Vector3(0f, 0.9f, 0f);
            targetPoint.transform.localRotation = Quaternion.identity;
            targetPoint.transform.localScale = Vector3.one;

            GameObject buildZoneRoot = new GameObject("BuildZone");
            buildZoneRoot.transform.SetParent(root.transform);
            buildZoneRoot.transform.localPosition = Vector3.zero;
            buildZoneRoot.transform.localRotation = Quaternion.identity;
            buildZoneRoot.transform.localScale = Vector3.one;

            BoxCollider buildCollider = buildZoneRoot.AddComponent<BoxCollider>();
            buildCollider.isTrigger = true;
            buildCollider.center = new Vector3(0f, 0.8f, 0f);
            buildCollider.size = new Vector3(4f, 1.6f, 4f);

            GameObject zoneVisual = CreatePrimitiveChild(buildZoneRoot.transform, "Visual", PrimitiveType.Cylinder);
            zoneVisual.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            zoneVisual.transform.localScale = new Vector3(2f, 0.02f, 2f);
            Object.DestroyImmediate(zoneVisual.GetComponent<CapsuleCollider>());
            MeshRenderer zoneRenderer = zoneVisual.GetComponent<MeshRenderer>();
            zoneRenderer.sharedMaterial = EditorMaterialUtility.GetOrCreateLitMaterialAsset("Turret_BuildZone", new Color(1f, 0.7f, 0.2f, 0.28f));

            GameObject labelRoot = new GameObject("ProgressLabel");
            labelRoot.transform.SetParent(buildZoneRoot.transform);
            labelRoot.transform.localPosition = new Vector3(0f, 1.7f, 0f);
            TextMeshPro label = labelRoot.AddComponent<TextMeshPro>();
            label.text = "BUILD 0%";
            label.alignment = TextAlignmentOptions.Center;
            label.fontSize = 2.5f;
            label.color = Color.white;

            SphereCollider rootCollider = root.AddComponent<SphereCollider>();
            rootCollider.radius = 0.9f;
            rootCollider.center = new Vector3(0f, 0.6f, 0f);

            TurretHealth turretHealth = root.AddComponent<TurretHealth>();
            SerializedObject healthSerialized = new SerializedObject(turretHealth);
            healthSerialized.FindProperty("targetableWhenOnline").boolValue = true;
            healthSerialized.FindProperty("maxHealth").floatValue = 6f;
            healthSerialized.FindProperty("targetPoint").objectReferenceValue = targetPoint.transform;
            healthSerialized.ApplyModifiedPropertiesWithoutUndo();

            TurretBuildZone buildZone = buildZoneRoot.AddComponent<TurretBuildZone>();
            SerializedObject buildZoneSerialized = new SerializedObject(buildZone);
            buildZoneSerialized.FindProperty("buildDurationSeconds").floatValue = 3f;
            buildZoneSerialized.FindProperty("triggerCollider").objectReferenceValue = buildCollider;
            buildZoneSerialized.FindProperty("zoneRenderer").objectReferenceValue = zoneRenderer;
            buildZoneSerialized.FindProperty("progressLabel").objectReferenceValue = label;
            buildZoneSerialized.ApplyModifiedPropertiesWithoutUndo();

            TurretTargeting targeting = root.AddComponent<TurretTargeting>();
            SerializedObject targetingSerialized = new SerializedObject(targeting);
            targetingSerialized.FindProperty("pivot").objectReferenceValue = headPivot.transform;
            targetingSerialized.FindProperty("range").floatValue = 14f;
            targetingSerialized.FindProperty("turnSpeed").floatValue = 270f;
            targetingSerialized.ApplyModifiedPropertiesWithoutUndo();

            TurretWeapon weapon = root.AddComponent<TurretWeapon>();
            SerializedObject weaponSerialized = new SerializedObject(weapon);
            weaponSerialized.FindProperty("muzzle").objectReferenceValue = muzzle.transform;
            weaponSerialized.FindProperty("projectilePrefab").objectReferenceValue = projectilePrefab;
            weaponSerialized.FindProperty("fireIntervalSeconds").floatValue = 0.45f;
            weaponSerialized.FindProperty("projectileSpeed").floatValue = 10f;
            weaponSerialized.FindProperty("projectileDamage").floatValue = 1f;
            weaponSerialized.FindProperty("muzzleForwardOffset").floatValue = 0.45f;
            weaponSerialized.ApplyModifiedPropertiesWithoutUndo();

            TurretController controller = root.AddComponent<TurretController>();
            SerializedObject controllerSerialized = new SerializedObject(controller);
            controllerSerialized.FindProperty("activationMode").enumValueIndex = (int)TurretActivationMode.BuildZone;
            controllerSerialized.FindProperty("canBeDamaged").boolValue = true;
            controllerSerialized.FindProperty("buildZone").objectReferenceValue = buildZone;
            controllerSerialized.FindProperty("turretHealth").objectReferenceValue = turretHealth;
            controllerSerialized.FindProperty("targeting").objectReferenceValue = targeting;
            controllerSerialized.FindProperty("weapon").objectReferenceValue = weapon;
            AssignArray(controllerSerialized.FindProperty("offlineObjects"), offlineVisual);
            AssignArray(controllerSerialized.FindProperty("onlineObjects"), onlineVisual);
            AssignArray(controllerSerialized.FindProperty("buildZoneObjects"), buildZoneRoot);
            controllerSerialized.ApplyModifiedPropertiesWithoutUndo();

            onlineVisual.SetActive(false);
            return root;
        }

        private static TurretProjectile CreateOrUpdateProjectilePrefab()
        {
            GameObject existingPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(ProjectilePrefabPath);
            if (existingPrefab != null)
            {
                GameObject prefabContents = PrefabUtility.LoadPrefabContents(ProjectilePrefabPath);
                ConfigureProjectilePrefab(prefabContents);
                PrefabUtility.SaveAsPrefabAsset(prefabContents, ProjectilePrefabPath);
                PrefabUtility.UnloadPrefabContents(prefabContents);
                return AssetDatabase.LoadAssetAtPath<GameObject>(ProjectilePrefabPath).GetComponent<TurretProjectile>();
            }

            GameObject projectile = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            projectile.name = "TurretProjectile";
            ConfigureProjectilePrefab(projectile);
            GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(projectile, ProjectilePrefabPath);
            Object.DestroyImmediate(projectile);
            return prefabAsset.GetComponent<TurretProjectile>();
        }

        private static void ConfigureProjectilePrefab(GameObject projectile)
        {
            projectile.transform.localPosition = Vector3.zero;
            projectile.transform.localRotation = Quaternion.identity;
            projectile.transform.localScale = Vector3.one * 0.38f;

            SphereCollider sphereCollider = projectile.GetComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            sphereCollider.radius = 0.5f;

            Rigidbody rigidbody = projectile.GetComponent<Rigidbody>();
            if (rigidbody == null)
            {
                rigidbody = projectile.AddComponent<Rigidbody>();
            }

            rigidbody.useGravity = false;
            rigidbody.isKinematic = false;
            rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            MeshRenderer renderer = projectile.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = EditorMaterialUtility.GetOrCreateLitMaterialAsset("Turret_Projectile", new Color(0.2f, 0.8f, 1f, 1f));

            TurretProjectile turretProjectile = projectile.GetComponent<TurretProjectile>();
            if (turretProjectile == null)
            {
                turretProjectile = projectile.AddComponent<TurretProjectile>();
            }

            SerializedObject projectileSerialized = new SerializedObject(turretProjectile);
            projectileSerialized.FindProperty("speed").floatValue = 10f;
            projectileSerialized.FindProperty("damage").floatValue = 1f;
            projectileSerialized.FindProperty("maxLifetimeSeconds").floatValue = 4f;
            projectileSerialized.FindProperty("launchSpeedScale").floatValue = 1f;
            projectileSerialized.FindProperty("visibleScale").floatValue = 0.38f;
            projectileSerialized.FindProperty("hitEffectScale").floatValue = 0.65f;
            projectileSerialized.FindProperty("hitEffectDurationSeconds").floatValue = 0.12f;
            projectileSerialized.FindProperty("projectileRigidbody").objectReferenceValue = rigidbody;
            projectileSerialized.FindProperty("projectileCollider").objectReferenceValue = sphereCollider;
            projectileSerialized.FindProperty("projectileRenderer").objectReferenceValue = renderer;
            projectileSerialized.ApplyModifiedPropertiesWithoutUndo();
        }

        private static GameObject CreatePrimitiveChild(Transform parent, string name, PrimitiveType primitiveType)
        {
            GameObject child = GameObject.CreatePrimitive(primitiveType);
            child.name = name;
            child.transform.SetParent(parent);
            child.transform.localPosition = Vector3.zero;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localScale = Vector3.one;
            return child;
        }

        private static void AssignArray(SerializedProperty property, Object value)
        {
            property.arraySize = 1;
            property.GetArrayElementAtIndex(0).objectReferenceValue = value;
        }
    }
}
