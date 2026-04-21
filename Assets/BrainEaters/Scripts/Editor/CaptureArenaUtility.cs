using BrainEaters.GameFlow;
using BrainEaters.Turrets;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace BrainEaters.EditorTools
{
    public static class CaptureArenaUtility
    {
        public static void RepairCaptureArena(LevelContext levelContext)
        {
            if (levelContext == null)
            {
                return;
            }

            Transform root = levelContext.transform;
            AssignMaterialsRecursively(root);
            EnsurePlayerSpawnPoint(levelContext);
            levelContext.RefreshSpawnPointsIfNeeded();
            EditorUtility.SetDirty(levelContext);
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            AssetDatabase.SaveAssets();
        }

        public static void EnsurePlayerSpawnPoint(LevelContext levelContext)
        {
            if (levelContext == null || levelContext.PlayerSpawnPoint != null)
            {
                return;
            }

            GameObject root = new GameObject("PlayerSpawn");
            Undo.RegisterCreatedObjectUndo(root, "Create Capture Player Spawn");
            root.transform.SetParent(levelContext.transform);
            root.transform.localPosition = Vector3.zero;
            root.transform.localRotation = Quaternion.identity;

            PlayerSpawnPointBuilder.CreatePlayerSpawnPointObject(root.transform, "PlayerSpawnPoint", new Vector3(-20f, 0f, 0f), Quaternion.Euler(0f, 90f, 0f));
            levelContext.RefreshSpawnPointsIfNeeded();
        }

        public static void EnsureCaptureTurrets(LevelContext levelContext)
        {
            if (levelContext == null)
            {
                return;
            }

            TurretBuilder.BuildTurretPrefab();
            GameObject turretPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(TurretBuilder.TurretPrefabPathForEditor);
            if (turretPrefab == null)
            {
                return;
            }

            Transform turretsRoot = levelContext.transform.Find("Turrets");
            if (turretsRoot == null)
            {
                GameObject turretsObject = new GameObject("Turrets");
                Undo.RegisterCreatedObjectUndo(turretsObject, "Create Capture Turrets Root");
                turretsObject.transform.SetParent(levelContext.transform);
                turretsObject.transform.localPosition = Vector3.zero;
                turretsObject.transform.localRotation = Quaternion.identity;
                turretsRoot = turretsObject.transform;
            }

            if (turretsRoot.childCount == 0)
            {
                CreateCaptureTurretInstance(turretPrefab, turretsRoot, "CaptureTurret_A", new Vector3(-8f, 0f, 8f), Quaternion.Euler(0f, 35f, 0f), TurretActivationMode.BuildZone, null);

                CaptureZone captureZone = levelContext.CaptureZones.Count > 0 ? levelContext.CaptureZones[0] : null;
                CreateCaptureTurretInstance(turretPrefab, turretsRoot, "CaptureTurret_B", new Vector3(10f, 0f, -6f), Quaternion.Euler(0f, -35f, 0f), TurretActivationMode.CaptureZone, captureZone);
            }

            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        }

        private static void CreateCaptureTurretInstance(GameObject turretPrefab, Transform parent, string name, Vector3 localPosition, Quaternion localRotation, TurretActivationMode mode, CaptureZone captureZone)
        {
            GameObject instance = PrefabUtility.InstantiatePrefab(turretPrefab) as GameObject;
            if (instance == null)
            {
                return;
            }

            Undo.RegisterCreatedObjectUndo(instance, "Create Capture Turret");
            instance.name = name;
            instance.transform.SetParent(parent);
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = localRotation;

            TurretController controller = instance.GetComponent<TurretController>();
            if (controller != null)
            {
                SerializedObject serializedObject = new SerializedObject(controller);
                serializedObject.FindProperty("activationMode").enumValueIndex = (int)mode;
                serializedObject.FindProperty("captureZone").objectReferenceValue = captureZone;
                serializedObject.FindProperty("canBeDamaged").boolValue = true;
                serializedObject.ApplyModifiedPropertiesWithoutUndo();
                controller.ResetState();
            }
        }

        private static void AssignMaterialsRecursively(Transform root)
        {
            MeshRenderer[] renderers = root.GetComponentsInChildren<MeshRenderer>(true);
            for (int i = 0; i < renderers.Length; i++)
            {
                MeshRenderer renderer = renderers[i];
                if (renderer == null)
                {
                    continue;
                }

                string materialName;
                Color color;
                if (!TryGetCaptureMaterialDefinition(renderer.transform, out materialName, out color))
                {
                    continue;
                }

                renderer.sharedMaterial = EditorMaterialUtility.GetOrCreateLitMaterialAsset(materialName, color);
                EditorUtility.SetDirty(renderer);
            }
        }

        private static bool TryGetCaptureMaterialDefinition(Transform target, out string materialName, out Color color)
        {
            materialName = null;
            color = Color.white;

            string targetName = target.name;
            if (targetName == "Floor")
            {
                materialName = "Capture_Floor";
                color = new Color(0.18f, 0.2f, 0.22f, 1f);
                return true;
            }

            if (targetName.StartsWith("Wall_"))
            {
                materialName = "Capture_Wall";
                color = new Color(0.24f, 0.27f, 0.3f, 1f);
                return true;
            }

            if (targetName.StartsWith("Slope_"))
            {
                materialName = "Capture_Slope";
                color = new Color(0.42f, 0.36f, 0.2f, 1f);
                return true;
            }

            if (targetName.StartsWith("Obstacle_"))
            {
                materialName = "Capture_Obstacle";
                color = new Color(0.38f, 0.24f, 0.2f, 1f);
                return true;
            }

            if (targetName == "Visual" && target.parent != null && target.parent.name.StartsWith("CaptureZone_"))
            {
                materialName = "Capture_Zone";
                color = new Color(0.2f, 0.7f, 1f, 0.35f);
                return true;
            }

            if (targetName == "GroundMarker" && target.parent != null && target.parent.parent != null && target.parent.parent.name == "PlayerSpawnPoint")
            {
                materialName = "PlayerSpawn_Marker";
                color = new Color(0.2f, 0.7f, 1f, 0.85f);
                return true;
            }

            if (targetName == "GroundMarker")
            {
                materialName = "SpawnPoint_Marker";
                color = new Color(0.2f, 0.9f, 0.6f, 0.7f);
                return true;
            }

            if (targetName == "FacingArrow")
            {
                materialName = "PlayerSpawn_Arrow";
                color = new Color(1f, 1f, 1f, 0.95f);
                return true;
            }

            return false;
        }
    }
}
