using BrainEaters.Cameras;
using BrainEaters.Input;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BrainEaters.EditorTools
{
    public static class MobileControlsBuilder
    {
        [MenuItem("Brain Eaters/Create Mobile Controls UI")]
        public static void CreateMobileControlsUi()
        {
            Canvas canvas = Object.FindFirstObjectByType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObject = new GameObject("GameplayHUDCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
                scaler.matchWidthOrHeight = 0.5f;

            }

            EventSystem eventSystem = Object.FindFirstObjectByType<EventSystem>();
            if (eventSystem == null)
            {
                GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(UnityEngine.InputSystem.UI.InputSystemUIInputModule));
                eventSystem = eventSystemObject.GetComponent<EventSystem>();
            }
            else if (eventSystem.GetComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>() == null)
            {
                Undo.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>(eventSystem.gameObject);
            }

            if (TMP_Settings.defaultFontAsset == null)
            {
                Debug.LogError("TMP default font asset is missing. Import TMP Essential Resources first.");
                return;
            }

            PlayerInputRouter playerInputRouter = Object.FindFirstObjectByType<PlayerInputRouter>();
            KeyboardMouseInputSource keyboardInput = Object.FindFirstObjectByType<KeyboardMouseInputSource>();
            CameraFollow cameraFollow = Object.FindFirstObjectByType<CameraFollow>();
            if (playerInputRouter == null)
            {
                Debug.LogError("Create a Player with PlayerInputRouter first, then run Create Mobile Controls UI.");
                return;
            }

            MobileGameplayInputSource mobileInput = playerInputRouter.GetComponent<MobileGameplayInputSource>();
            if (mobileInput == null)
            {
                mobileInput = Undo.AddComponent<MobileGameplayInputSource>(playerInputRouter.gameObject);
            }

            GameObject root = GetOrCreateUiChild(canvas.transform, "MobileControlsRoot");
            RectTransform rootRect = root.GetComponent<RectTransform>();
            StretchFull(rootRect);
            GameObject visualsRoot = GetOrCreateUiChild(root.transform, "ControlsVisuals");
            RectTransform visualsRect = visualsRoot.GetComponent<RectTransform>();
            StretchFull(visualsRect);
            GameObject visibleJoysticksRoot = GetOrCreateUiChild(visualsRoot.transform, "VisibleJoysticksRoot");
            RectTransform visibleJoysticksRect = visibleJoysticksRoot.GetComponent<RectTransform>();
            StretchFull(visibleJoysticksRect);
            GameObject invisibleJoysticksRoot = GetOrCreateUiChild(visualsRoot.transform, "InvisibleJoysticksRoot");
            RectTransform invisibleJoysticksRect = invisibleJoysticksRoot.GetComponent<RectTransform>();
            StretchFull(invisibleJoysticksRect);

            VirtualJoystick leftJoystick = CreateJoystick(visibleJoysticksRoot.transform, "LeftJoystick", new Vector2(170f, 170f), new Vector2(220f, 220f), new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(190f, 190f));
            GameObject rightRoot = GetOrCreateUiChild(visibleJoysticksRoot.transform, "RightJoystickRoot");
            RectTransform rightRootRect = rightRoot.GetComponent<RectTransform>();
            StretchFull(rightRootRect);
            VirtualJoystick rightJoystick = CreateJoystick(rightRoot.transform, "RightJoystick", new Vector2(-170f, 170f), new Vector2(220f, 220f), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(190f, 190f));
            InvisibleTouchJoystick invisibleLeftJoystick = CreateInvisibleJoystick(invisibleJoysticksRoot.transform, "InvisibleLeftJoystick", new Vector2(0f, 0f), new Vector2(0.5f, 1f));
            InvisibleTouchJoystick invisibleRightJoystick = CreateInvisibleJoystick(invisibleJoysticksRoot.transform, "InvisibleRightJoystick", new Vector2(0.5f, 0f), new Vector2(1f, 1f));

            TouchActionButton chargeButton = CreateTouchButton(visualsRoot.transform, "ChargeButton", "CHARGE", new Vector2(-170f, 420f), new Vector2(180f, 80f), new Vector2(1f, 0f), new Vector2(1f, 0f));
            TouchActionButton bombButton = CreateTouchButton(visualsRoot.transform, "BombButton", "BOMB", new Vector2(-170f, 320f), new Vector2(180f, 80f), new Vector2(1f, 0f), new Vector2(1f, 0f));
            Button modeButton = CreateModeButton(visualsRoot.transform, "ControlModeButton", new Vector2(-130f, -80f), new Vector2(240f, 64f));
            TMP_Text modeLabel = modeButton.GetComponentInChildren<TextMeshProUGUI>(true);

            MobileControlsManager manager = root.GetComponent<MobileControlsManager>();
            if (manager == null)
            {
                manager = Undo.AddComponent<MobileControlsManager>(root);
            }

            SerializedObject inputObject = new SerializedObject(mobileInput);
            inputObject.FindProperty("moveJoystick").objectReferenceValue = leftJoystick;
            inputObject.FindProperty("lookJoystick").objectReferenceValue = rightJoystick;
            inputObject.FindProperty("invisibleMoveJoystick").objectReferenceValue = invisibleLeftJoystick;
            inputObject.FindProperty("invisibleLookJoystick").objectReferenceValue = invisibleRightJoystick;
            inputObject.FindProperty("chargeButton").objectReferenceValue = chargeButton;
            inputObject.FindProperty("bombButton").objectReferenceValue = bombButton;
            inputObject.ApplyModifiedPropertiesWithoutUndo();

            SerializedObject managerObject = new SerializedObject(manager);
            managerObject.FindProperty("controlsRoot").objectReferenceValue = visualsRoot;
            managerObject.FindProperty("visibleJoysticksRoot").objectReferenceValue = visibleJoysticksRoot;
            managerObject.FindProperty("rightJoystickRoot").objectReferenceValue = rightRoot;
            managerObject.FindProperty("invisibleJoysticksRoot").objectReferenceValue = invisibleJoysticksRoot;
            managerObject.FindProperty("modeToggleButton").objectReferenceValue = modeButton;
            managerObject.FindProperty("modeLabel").objectReferenceValue = modeLabel;
            managerObject.FindProperty("playerInputRouter").objectReferenceValue = playerInputRouter;
            managerObject.FindProperty("keyboardMouseInputSource").objectReferenceValue = keyboardInput;
            managerObject.FindProperty("mobileGameplayInputSource").objectReferenceValue = mobileInput;
            managerObject.FindProperty("cameraFollow").objectReferenceValue = cameraFollow;
            managerObject.ApplyModifiedPropertiesWithoutUndo();

            EditorUtility.SetDirty(mobileInput);
            EditorUtility.SetDirty(manager);
            Selection.activeGameObject = root;
            EditorGUIUtility.PingObject(root);
        }

        private static InvisibleTouchJoystick CreateInvisibleJoystick(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject root = GetOrCreateUiChild(parent, name);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;

            Image image = GetOrAddImage(root);
            image.color = new Color(1f, 1f, 1f, 0.001f);
            image.raycastTarget = true;

            InvisibleTouchJoystick joystick = root.GetComponent<InvisibleTouchJoystick>();
            if (joystick == null)
            {
                joystick = Undo.AddComponent<InvisibleTouchJoystick>(root);
            }

            return joystick;
        }

        private static VirtualJoystick CreateJoystick(Transform parent, string name, Vector2 anchoredPosition, Vector2 size, Vector2 anchorMin, Vector2 anchorMax, Vector2 handleSize)
        {
            GameObject root = GetOrCreateUiChild(parent, name);
            RectTransform rootRect = root.GetComponent<RectTransform>();
            rootRect.anchorMin = anchorMin;
            rootRect.anchorMax = anchorMax;
            rootRect.pivot = new Vector2(0.5f, 0.5f);
            rootRect.anchoredPosition = anchoredPosition;
            rootRect.sizeDelta = size;

            Image background = GetOrAddImage(root);
            background.color = new Color(1f, 1f, 1f, 0.18f);
            background.raycastTarget = true;

            GameObject handleObject = GetOrCreateUiChild(root.transform, "Handle");
            RectTransform handleRect = handleObject.GetComponent<RectTransform>();
            handleRect.anchorMin = new Vector2(0.5f, 0.5f);
            handleRect.anchorMax = new Vector2(0.5f, 0.5f);
            handleRect.pivot = new Vector2(0.5f, 0.5f);
            handleRect.anchoredPosition = Vector2.zero;
            handleRect.sizeDelta = handleSize;

            Image handleImage = GetOrAddImage(handleObject);
            handleImage.color = new Color(1f, 1f, 1f, 0.36f);
            handleImage.raycastTarget = false;

            VirtualJoystick joystick = root.GetComponent<VirtualJoystick>();
            if (joystick == null)
            {
                joystick = Undo.AddComponent<VirtualJoystick>(root);
            }

            SerializedObject joystickObject = new SerializedObject(joystick);
            joystickObject.FindProperty("backgroundRect").objectReferenceValue = rootRect;
            joystickObject.FindProperty("handleRect").objectReferenceValue = handleRect;
            joystickObject.ApplyModifiedPropertiesWithoutUndo();
            return joystick;
        }

        private static TouchActionButton CreateTouchButton(Transform parent, string name, string label, Vector2 anchoredPosition, Vector2 size, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject root = GetOrCreateUiChild(parent, name);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            Image image = GetOrAddImage(root);
            image.color = new Color(0.12f, 0.12f, 0.12f, 0.72f);
            image.raycastTarget = true;

            GameObject labelObject = GetOrCreateUiChild(root.transform, "Label");
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            StretchFull(labelRect);

            TextMeshProUGUI text = labelObject.GetComponent<TextMeshProUGUI>();
            if (text == null)
            {
                text = Undo.AddComponent<TextMeshProUGUI>(labelObject);
            }

            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = 26f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.text = label;
            text.raycastTarget = false;

            TouchActionButton touchButton = root.GetComponent<TouchActionButton>();
            if (touchButton == null)
            {
                touchButton = Undo.AddComponent<TouchActionButton>(root);
            }

            return touchButton;
        }

        private static Button CreateModeButton(Transform parent, string name, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject root = GetOrCreateUiChild(parent, name);
            RectTransform rect = root.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(1f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            Image image = GetOrAddImage(root);
            image.color = new Color(0.14f, 0.14f, 0.14f, 0.82f);

            Button button = root.GetComponent<Button>();
            if (button == null)
            {
                button = Undo.AddComponent<Button>(root);
            }

            GameObject labelObject = GetOrCreateUiChild(root.transform, "Label");
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            StretchFull(labelRect);

            TextMeshProUGUI text = labelObject.GetComponent<TextMeshProUGUI>();
            if (text == null)
            {
                text = Undo.AddComponent<TextMeshProUGUI>(labelObject);
            }

            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = 24f;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            text.text = "2 Joysticks";
            text.raycastTarget = false;

            return button;
        }

        private static Image GetOrAddImage(GameObject gameObject)
        {
            Image image = gameObject.GetComponent<Image>();
            if (image == null)
            {
                image = Undo.AddComponent<Image>(gameObject);
            }

            return image;
        }

        private static GameObject GetOrCreateUiChild(Transform parent, string childName)
        {
            Transform existingChild = parent.Find(childName);
            if (existingChild != null)
            {
                return existingChild.gameObject;
            }

            GameObject child = new GameObject(childName, typeof(RectTransform), typeof(CanvasRenderer));
            Undo.RegisterCreatedObjectUndo(child, "Create Mobile Controls UI Object");
            child.transform.SetParent(parent);
            child.transform.localScale = Vector3.one;
            child.transform.localRotation = Quaternion.identity;
            child.transform.localPosition = Vector3.zero;
            return child;
        }

        private static void StretchFull(RectTransform rectTransform)
        {
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
        }
    }
}
