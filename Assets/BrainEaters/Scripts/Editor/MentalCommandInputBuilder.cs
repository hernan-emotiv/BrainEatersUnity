using BrainEaters.Input;
using UnityEditor;
using UnityEngine;

namespace BrainEaters.EditorTools
{
    public static class MentalCommandInputBuilder
    {
        [MenuItem("Brain Eaters/Cortex/Install Cortex v5 Session Bootstrap In Current Scene")]
        public static void InstallCortexV5SessionBootstrap()
        {
            CortexV5SessionBootstrap bootstrap = Object.FindFirstObjectByType<CortexV5SessionBootstrap>();
            if (bootstrap == null)
            {
                GameObject bootstrapObject = new GameObject("CortexV5SessionBootstrap");
                Undo.RegisterCreatedObjectUndo(bootstrapObject, "Create Cortex v5 Session Bootstrap");
                bootstrap = Undo.AddComponent<CortexV5SessionBootstrap>(bootstrapObject);
            }

            Selection.activeGameObject = bootstrap.gameObject;
            EditorGUIUtility.PingObject(bootstrap.gameObject);
            EditorUtility.SetDirty(bootstrap);

            Debug.Log("Installed Cortex v5 session bootstrap. Place this in the initial flow scene so login/connect happens before gameplay.", bootstrap);
        }

        [MenuItem("Brain Eaters/Cortex/Install Simulated MC Input In Current Scene")]
        public static void InstallSimulatedMentalCommandInput()
        {
            PlayerInputRouter inputRouter = Object.FindFirstObjectByType<PlayerInputRouter>();
            if (inputRouter == null)
            {
                Debug.LogError("Could not install simulated MC input. No PlayerInputRouter found in the current scene.");
                return;
            }

            GameObject player = inputRouter.gameObject;
            SimulatedMentalCommandSource simulatedSource = player.GetComponent<SimulatedMentalCommandSource>();
            if (simulatedSource == null)
            {
                simulatedSource = Undo.AddComponent<SimulatedMentalCommandSource>(player);
            }

            MentalCommandGameplayInputSource mentalInput = player.GetComponent<MentalCommandGameplayInputSource>();
            if (mentalInput == null)
            {
                mentalInput = Undo.AddComponent<MentalCommandGameplayInputSource>(player);
            }

            MonoBehaviour fallbackInput = FindFallbackInputSource(player, mentalInput);
            SerializedObject mentalSerialized = new SerializedObject(mentalInput);
            mentalSerialized.FindProperty("fallbackInputSource").objectReferenceValue = fallbackInput;
            mentalSerialized.FindProperty("commandSignalSource").objectReferenceValue = simulatedSource;
            mentalSerialized.FindProperty("chargeCommandId").stringValue = "pull";
            mentalSerialized.FindProperty("bombCommandId").stringValue = "push";
            mentalSerialized.FindProperty("minimumPower").floatValue = 0.5f;
            mentalSerialized.FindProperty("minimumConfidence").floatValue = 0.5f;
            mentalSerialized.FindProperty("chargeWhileCommandHeld").boolValue = true;
            mentalSerialized.FindProperty("triggerBombOnCommandStart").boolValue = true;
            mentalSerialized.ApplyModifiedPropertiesWithoutUndo();

            inputRouter.SetInputSource(mentalInput);
            MobileControlsManager mobileControlsManager = Object.FindFirstObjectByType<MobileControlsManager>();
            if (mobileControlsManager != null)
            {
                SerializedObject managerSerialized = new SerializedObject(mobileControlsManager);
                SerializedProperty overrideProperty = managerSerialized.FindProperty("inputSourceOverride");
                if (overrideProperty != null)
                {
                    overrideProperty.objectReferenceValue = mentalInput;
                    managerSerialized.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(mobileControlsManager);
                }
            }

            EditorUtility.SetDirty(inputRouter);
            EditorUtility.SetDirty(simulatedSource);
            EditorUtility.SetDirty(mentalInput);
            Selection.activeGameObject = player;
            EditorGUIUtility.PingObject(player);

            Debug.Log("Installed simulated MC input. Hold C to emit pull/charge, press M to emit push/bomb.", player);
        }

        [MenuItem("Brain Eaters/Cortex/Install Cortex v5 MC Input In Current Scene")]
        public static void InstallCortexV5MentalCommandInput()
        {
            PlayerInputRouter inputRouter = Object.FindFirstObjectByType<PlayerInputRouter>();
            if (inputRouter == null)
            {
                Debug.LogError("Could not install Cortex v5 MC input. No PlayerInputRouter found in the current scene.");
                return;
            }

            GameObject player = inputRouter.gameObject;
            CortexV5MentalCommandSource cortexSource = player.GetComponent<CortexV5MentalCommandSource>();
            if (cortexSource == null)
            {
                cortexSource = Undo.AddComponent<CortexV5MentalCommandSource>(player);
            }

            MentalCommandGameplayInputSource mentalInput = player.GetComponent<MentalCommandGameplayInputSource>();
            if (mentalInput == null)
            {
                mentalInput = Undo.AddComponent<MentalCommandGameplayInputSource>(player);
            }

            MonoBehaviour fallbackInput = FindFallbackInputSource(player, mentalInput);
            SerializedObject mentalSerialized = new SerializedObject(mentalInput);
            mentalSerialized.FindProperty("fallbackInputSource").objectReferenceValue = fallbackInput;
            mentalSerialized.FindProperty("commandSignalSource").objectReferenceValue = cortexSource;
            mentalSerialized.FindProperty("chargeCommandId").stringValue = "pull";
            mentalSerialized.FindProperty("bombCommandId").stringValue = "push";
            mentalSerialized.FindProperty("minimumPower").floatValue = 0.5f;
            mentalSerialized.FindProperty("minimumConfidence").floatValue = 0.5f;
            mentalSerialized.FindProperty("chargeWhileCommandHeld").boolValue = true;
            mentalSerialized.FindProperty("triggerBombOnCommandStart").boolValue = true;
            mentalSerialized.ApplyModifiedPropertiesWithoutUndo();

            inputRouter.SetInputSource(mentalInput);
            MobileControlsManager mobileControlsManager = Object.FindFirstObjectByType<MobileControlsManager>();
            if (mobileControlsManager != null)
            {
                SerializedObject managerSerialized = new SerializedObject(mobileControlsManager);
                SerializedProperty overrideProperty = managerSerialized.FindProperty("inputSourceOverride");
                if (overrideProperty != null)
                {
                    overrideProperty.objectReferenceValue = mentalInput;
                    managerSerialized.ApplyModifiedPropertiesWithoutUndo();
                    EditorUtility.SetDirty(mobileControlsManager);
                }
            }

            EditorUtility.SetDirty(inputRouter);
            EditorUtility.SetDirty(cortexSource);
            EditorUtility.SetDirty(mentalInput);
            Selection.activeGameObject = player;
            EditorGUIUtility.PingObject(player);

            Debug.Log("Installed Cortex v5 MC input. Real runtime connection requires com.emotiv.cortex and Android/iOS build support.", player);
        }

        private static MonoBehaviour FindFallbackInputSource(GameObject player, MentalCommandGameplayInputSource mentalInput)
        {
            MobileGameplayInputSource mobileInput = player.GetComponent<MobileGameplayInputSource>();
            if (mobileInput != null)
            {
                return mobileInput;
            }

            KeyboardMouseInputSource keyboardInput = player.GetComponent<KeyboardMouseInputSource>();
            if (keyboardInput != null)
            {
                return keyboardInput;
            }

            MonoBehaviour[] behaviours = player.GetComponents<MonoBehaviour>();
            for (int i = 0; i < behaviours.Length; i++)
            {
                MonoBehaviour behaviour = behaviours[i];
                if (behaviour != null && behaviour != mentalInput && behaviour is IGameplayInputSource)
                {
                    return behaviour;
                }
            }

            return null;
        }
    }
}
