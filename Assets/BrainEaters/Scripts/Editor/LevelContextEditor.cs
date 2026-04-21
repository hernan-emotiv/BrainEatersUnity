using BrainEaters.GameFlow;
using UnityEditor;
using UnityEngine;

namespace BrainEaters.EditorTools
{
    [CustomEditor(typeof(LevelContext))]
    public class LevelContextEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            LevelContext levelContext = (LevelContext)target;
            if (levelContext == null)
            {
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Arena Tools", EditorStyles.boldLabel);

            if (GUILayout.Button("Repair Capture Arena"))
            {
                CaptureArenaUtility.RepairCaptureArena(levelContext);
            }

            if (GUILayout.Button("Ensure Player Spawn Point"))
            {
                CaptureArenaUtility.EnsurePlayerSpawnPoint(levelContext);
            }

            if (GUILayout.Button("Add Capture Turrets"))
            {
                CaptureArenaUtility.EnsureCaptureTurrets(levelContext);
            }
        }
    }
}
