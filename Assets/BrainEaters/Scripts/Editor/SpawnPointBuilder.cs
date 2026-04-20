using BrainEaters.Spawning;
using UnityEditor;
using UnityEngine;

namespace BrainEaters.EditorTools
{
    public static class SpawnPointBuilder
    {
        private const string DefaultSpawnPointName = "SpawnPoint";

        [MenuItem("Brain Eaters/Create Spawn Point")]
        public static void CreateSpawnPointFromMenu()
        {
            Transform parent = Selection.activeTransform;
            GameObject spawnPoint = CreateSpawnPointObject(parent, DefaultSpawnPointName, Vector3.zero);
            Selection.activeGameObject = spawnPoint;
            EditorGUIUtility.PingObject(spawnPoint);
        }

        public static GameObject CreateSpawnPointObject(Transform parent, string pointName, Vector3 localPosition)
        {
            GameObject point = new GameObject(pointName);
            Undo.RegisterCreatedObjectUndo(point, "Create Spawn Point");

            if (parent != null)
            {
                point.transform.SetParent(parent);
                point.transform.localPosition = localPosition;
                point.transform.localRotation = Quaternion.identity;
            }
            else
            {
                point.transform.position = localPosition;
            }

            SpawnPoint spawnPointComponent = point.AddComponent<SpawnPoint>();
            CreateVisuals(point.transform);
            EditorUtility.SetDirty(spawnPointComponent);
            return point;
        }

        private static void CreateVisuals(Transform root)
        {
            GameObject visualRoot = new GameObject("VisualRoot");
            Undo.RegisterCreatedObjectUndo(visualRoot, "Create Spawn Point Visuals");
            visualRoot.transform.SetParent(root);
            visualRoot.transform.localPosition = Vector3.zero;
            visualRoot.transform.localRotation = Quaternion.identity;
            visualRoot.transform.localScale = Vector3.one;

            CreateGroundMarker(visualRoot.transform);
            ParticleSystem ambient = CreateAmbientParticles(visualRoot.transform);
            ParticleSystem burst = CreateSpawnBurstParticles(visualRoot.transform);

            SerializedObject serializedObject = new SerializedObject(root.GetComponent<SpawnPoint>());
            serializedObject.FindProperty("ambientParticles").objectReferenceValue = ambient;
            serializedObject.FindProperty("spawnBurstParticles").objectReferenceValue = burst;
            serializedObject.ApplyModifiedPropertiesWithoutUndo();
        }

        private static void CreateGroundMarker(Transform parent)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Undo.RegisterCreatedObjectUndo(marker, "Create Spawn Marker");
            marker.name = "GroundMarker";
            marker.transform.SetParent(parent);
            marker.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            marker.transform.localScale = new Vector3(1.6f, 0.02f, 1.6f);

            Collider markerCollider = marker.GetComponent<Collider>();
            if (markerCollider != null)
            {
                markerCollider.enabled = false;
            }

            Renderer rendererComponent = marker.GetComponent<Renderer>();
            rendererComponent.sharedMaterial = EditorMaterialUtility.GetOrCreateLitMaterialAsset("SpawnPoint_Marker", new Color(0.2f, 0.9f, 0.6f, 0.7f));
        }

        private static ParticleSystem CreateAmbientParticles(Transform parent)
        {
            GameObject particleObject = new GameObject("AmbientParticles");
            Undo.RegisterCreatedObjectUndo(particleObject, "Create Ambient Spawn Particles");
            particleObject.transform.SetParent(parent);
            particleObject.transform.localPosition = Vector3.zero;

            ParticleSystem particleSystem = particleObject.AddComponent<ParticleSystem>();
            var main = particleSystem.main;
            main.loop = true;
            main.startLifetime = 0.8f;
            main.startSpeed = 0.35f;
            main.startSize = 0.15f;
            main.startColor = new Color(0.5f, 1f, 0.8f, 0.65f);
            main.maxParticles = 20;

            var emission = particleSystem.emission;
            emission.rateOverTime = 8f;

            var shape = particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.6f;

            return particleSystem;
        }

        private static ParticleSystem CreateSpawnBurstParticles(Transform parent)
        {
            GameObject particleObject = new GameObject("SpawnBurstParticles");
            Undo.RegisterCreatedObjectUndo(particleObject, "Create Spawn Burst Particles");
            particleObject.transform.SetParent(parent);
            particleObject.transform.localPosition = Vector3.zero;

            ParticleSystem particleSystem = particleObject.AddComponent<ParticleSystem>();
            var main = particleSystem.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startLifetime = 0.45f;
            main.startSpeed = 2.5f;
            main.startSize = 0.18f;
            main.startColor = new Color(1f, 0.9f, 0.5f, 1f);
            main.maxParticles = 20;

            var emission = particleSystem.emission;
            emission.rateOverTime = 0f;
            emission.SetBursts(new[]
            {
                new ParticleSystem.Burst(0f, 14)
            });

            var shape = particleSystem.shape;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.35f;

            return particleSystem;
        }
    }
}
