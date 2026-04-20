using BrainEaters.GameFlow;
using UnityEditor;
using UnityEngine;

namespace BrainEaters.EditorTools
{
    public static class PlayerSpawnPointBuilder
    {
        [MenuItem("Brain Eaters/Create Player Spawn Point")]
        public static void CreatePlayerSpawnPointFromMenu()
        {
            Transform parent = Selection.activeTransform;
            GameObject spawnPoint = CreatePlayerSpawnPointObject(parent, "PlayerSpawnPoint", Vector3.zero, Quaternion.identity);
            Selection.activeGameObject = spawnPoint;
            EditorGUIUtility.PingObject(spawnPoint);
        }

        public static GameObject CreatePlayerSpawnPointObject(Transform parent, string pointName, Vector3 localPosition, Quaternion localRotation)
        {
            GameObject point = new GameObject(pointName);
            Undo.RegisterCreatedObjectUndo(point, "Create Player Spawn Point");

            if (parent != null)
            {
                point.transform.SetParent(parent);
                point.transform.localPosition = localPosition;
                point.transform.localRotation = localRotation;
            }
            else
            {
                point.transform.position = localPosition;
                point.transform.rotation = localRotation;
            }

            point.AddComponent<PlayerSpawnPoint>();
            CreateVisuals(point.transform);
            return point;
        }

        private static void CreateVisuals(Transform root)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Undo.RegisterCreatedObjectUndo(marker, "Create Player Spawn Marker");
            marker.name = "GroundMarker";
            marker.transform.SetParent(root);
            marker.transform.localPosition = new Vector3(0f, 0.02f, 0f);
            marker.transform.localScale = new Vector3(1.2f, 0.02f, 1.2f);

            Collider markerCollider = marker.GetComponent<Collider>();
            if (markerCollider != null)
            {
                markerCollider.enabled = false;
            }

            Renderer markerRenderer = marker.GetComponent<Renderer>();
            markerRenderer.sharedMaterial = EditorMaterialUtility.GetOrCreateLitMaterialAsset("PlayerSpawn_Marker", new Color(0.2f, 0.7f, 1f, 0.85f));

            GameObject arrow = GameObject.CreatePrimitive(PrimitiveType.Cube);
            Undo.RegisterCreatedObjectUndo(arrow, "Create Player Spawn Arrow");
            arrow.name = "FacingArrow";
            arrow.transform.SetParent(root);
            arrow.transform.localPosition = new Vector3(0f, 0.15f, 0.9f);
            arrow.transform.localScale = new Vector3(0.25f, 0.25f, 1.2f);

            Collider arrowCollider = arrow.GetComponent<Collider>();
            if (arrowCollider != null)
            {
                arrowCollider.enabled = false;
            }

            Renderer arrowRenderer = arrow.GetComponent<Renderer>();
            arrowRenderer.sharedMaterial = EditorMaterialUtility.GetOrCreateLitMaterialAsset("PlayerSpawn_Arrow", new Color(1f, 1f, 1f, 0.95f));
        }
    }
}
