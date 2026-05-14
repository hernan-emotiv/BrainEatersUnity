# Sprint 3 MC Integration Log

## Goal

Build a working Brain Eaters prototype where a Mental Command can trigger a simple in-game action, while documenting SDK friction, setup complexity, reliability, latency, and marker opportunities.

## Current Scope

- Must Have: Basic MC integration working.
- Must Have: Pain Points & Friction Documentation.
- Should Have: Basic visual feedback for MC.
- Should Have: Initial marker opportunities definition.
- Like To Have: debug visualization and early reusable SDK structure ideas.

## Step Log

### Step 1 - Add plugin repo as local external reference

Date: 2026-05-08

Action:
- Added `/Users/hernan/dev/emotiv/unity-plugin` as a symlink at `External/emotiv-unity-plugin`.
- Added `External/` to `.gitignore` so local analysis repos are not committed or imported by Unity.

Reason:
- Keeping the plugin outside `Assets/` lets us inspect the code without forcing Unity to import SDK scripts, native libraries, asmdefs, or platform-specific plugins prematurely.

Observed friction:
- The plugin contains platform-specific native paths for Android/iOS and optional embedded library requirements. Importing directly before understanding dependencies could destabilize the Unity project.

Developer question captured:
- Should the plugin be vendored into the game repo or consumed as a package/submodule later?

Current answer:
- Analyze externally first. Integrate through a narrow adapter only after we define the Brain Eaters command abstraction.

### Step 2 - Read plugin and official Cortex docs

Date: 2026-05-08

Action:
- Reviewed local plugin README and key source files.
- Reviewed official Cortex documentation for data subscription, mental command samples, BCI profile/training, and marker concepts.

Findings:
- Main Unity facade: `EmotivUnityItf`.
- Gameplay-oriented facade: `BCIGameItf`.
- Mental Command stream: `com`.
- Training/system stream: `sys`.
- Device/contact stream used by plugin example: `dev`.
- Mental Command payload is represented by action id (`act`) and power (`pow`).
- Plugin helper methods include `StartStreamData`, `GetMentalCommandActionPower`, `IsGoodMCAction`, profile loading, training, sensitivity, and profile erasing.

Observed friction:
- Real MC input depends on authorization, headset connection, session creation, profile loading, training, and subscribing to `com`.
- Mobile integration requires embedded/native dependencies that may not be available in this repo by default.

Developer question captured:
- Can we test gameplay mapping before device setup?

Current answer:
- Yes. Add a simulator first so gameplay can validate command routing independently of Cortex setup.

### Step 3 - Add source-independent Mental Command signal layer

Date: 2026-05-08

Action:
- Added `MentalCommandSignal`, `MentalCommandPhase`, and `ICommandSignalSource`.
- Added `SimulatedMentalCommandSource` for deterministic local testing.
- Added `MentalCommandGameplayInputSource` that wraps an existing `IGameplayInputSource` and maps mental command signals into `Charge` and `Bomb` gameplay intents.

Reason:
- Gameplay must not depend on Cortex SDK classes or raw Cortex messages.
- The same action should work from keyboard/mobile and simulated MC before adding the real Cortex adapter.

Default simulation mapping:
- Hold `C`: simulated `pull`, mapped to charge Mental Power.
- Press `M`: simulated `push`, mapped to trigger Brain Bomb.

Validation target:
- Player can still move/look using the fallback input source.
- Holding `C` charges the brain power.
- Pressing `M` triggers the bomb once the bar is ready.

Observed friction:
- Existing `PlayerInputRouter` supports a single active input source, so the MC source needs to wrap/delegate to the current keyboard/mobile source rather than replace it completely.

Developer question captured:
- Should MC commands replace controls or layer on top of them?

Current answer:
- Layer on top. MC should add actions while movement/look can remain keyboard/mobile during testing.

### Step 4 - Add installer for simulated MC input

Date: 2026-05-08

Action:
- Added `Brain Eaters > Cortex > Install Simulated MC Input In Current Scene`.
- The menu installs `SimulatedMentalCommandSource` and `MentalCommandGameplayInputSource` on the Player.
- The menu sets the Player input router to the MC wrapper.
- `MobileControlsManager` now has an optional `Input Source Override` so it does not overwrite the MC wrapper during `Awake`/`OnEnable`.

Reason:
- We need a repeatable setup step that any developer can run without manually wiring private serialized fields.

How to test:
- Open `GameScene`.
- Run `Brain Eaters > Cortex > Install Simulated MC Input In Current Scene`.
- Enter Play Mode.
- Hold `C` to simulate `pull`, which charges Mental Power.
- Press `M` to simulate `push`, which triggers the Brain Bomb if energy is ready.

Observed friction:
- The existing mobile control manager automatically reassigns the active input source. Without an override, any MC wrapper could be replaced at runtime.

Developer question captured:
- Should the MC input become the default input source in production?

Current answer:
- No. It should be installed/enabled by a composition root or config. For this sprint, the editor menu is enough to validate the loop safely.

### Step 5 - SDK import assessment

Date: 2026-05-11

Action:
- Inspected the downloaded `Emotiv/unity-plugin` repo structure.
- Identified candidate runtime files, desktop dependencies, mobile/embedded dependencies, and missing private/native assets.
- Created detailed assessment at `docs/sprint-3-sdk-import-assessment.md`.

Findings:
- The repo is not currently installed in Unity. It remains external and ignored.
- Desktop Cortex Service path is the lowest-risk first integration route.
- Mobile/embedded path is currently blocked because the downloaded repo does not include `EmotivCortexLib.aar`, `EmotivCortexLib.xcframework`, or populated `uniwebview` submodule.
- First compile spike should avoid `AndroidPlugin`, `IosPlugin`, `CortexApi`, `PostProcessBuild`, `Editor`, and mobile auth folders.

Observed friction:
- The plugin repo appears to be source-oriented, not a ready-to-drop-in package.
- It is unclear whether consumers should import a `.unitypackage`, use UPM, use a submodule, or copy selected `Src` files.
- Dependency strategy for Newtonsoft/WebSocket4Net/SuperSocket in Unity 6 needs confirmation.

Developer question captured:
- What is the official minimal import path for desktop-service-only MC testing in Unity?

Current answer:
- Unknown. Proposed working path is documented in `docs/sprint-3-sdk-import-assessment.md`, but should be confirmed with the plugin team.

### Step 6 - Review "How to use Unity examples" document

Date: 2026-05-11

Action:
- Reviewed `/Users/hernan/Downloads/How to use Unity examples.pdf`.
- Compared its guidance against the local `unity-plugin` repo.

Findings:
- The document is centered on `SimpleExample.unity`, not on importing the standalone `unity-plugin` repo directly into an existing project.
- The expected starting point appears to be `https://github.com/Emotiv/cortex-example/tree/master/unity`.
- The Unity example uses `unity-plugin` as a submodule.
- The document explicitly says to clone the Cortex example repo and initialize submodules.
- `SimpleExample` demonstrates authentication, headset sessions, records, markers, profiles, training, and data subscription.
- Option 1 uses Desktop Cortex Service via EMOTIV Launcher and can be tested with a virtual headset.
- Option 2 uses embedded library, with mobile as preferred platform, and requires additional native/mobile dependencies.
- Embedded Windows/Mac standalone builds are more complex and are not necessarily supported directly from Unity Editor.

Impact on plan:
- Do not infer the official import path from `unity-plugin` alone.
- The better next reference is the full `cortex-example` Unity project, because it should contain `SimpleExample.unity`, `AppConfig.cs`, and the expected folder layout.
- The local `unity-plugin` repo remains useful for source analysis, but it is not sufficient as a complete runnable demo.

Observed friction:
- The plugin repo alone does not contain the documented Unity scene/example.
- Documentation references `cortex-example` as the practical runnable starting point.
- Some instructions mention embedded Windows/Mac, but also state Editor direct running is not supported for embedded Windows.

Developer question captured:
- Should Brain Eaters integration be based on the `cortex-example/unity` folder structure rather than manually importing `unity-plugin/Src`?

Current answer:
- Yes, for learning and de-risking. We should inspect or clone `cortex-example` next, then decide what minimal subset to import into Brain Eaters.

### Step 7 - Inspect local `cortex-example` Unity project

Date: 2026-05-12

Action:
- Inspected `/Users/hernan/dev/emotiv/cortex-example`.
- Verified submodule status.
- Reviewed `unity/README.md`, `Assets/Plugins/AppConfig.cs`, `Assets/SimpleExample.cs`, and plugin layout under `Assets/Plugins/Emotiv-Unity-Plugin`.

Findings:
- `unity/Assets/Plugins/Emotiv-Unity-Plugin` is initialized.
- `unity/Assets/SimpleExample.unity` exists.
- `unity/Assets/SimpleExample.cs` is the actual runnable example driver.
- Desktop path uses `EmotivUnityItf.Init(..., AppConfig.AppUrl)` and `EmotivUnityItf.Start()`.
- `AppConfig.AppUrl` is `wss://localhost:6868` when not using embedded/mobile.
- Desktop must not define `USE_EMBEDDED_LIB`.
- Desktop requires EMOTIV Launcher/Cortex Service, but does not require UniWebView.
- The nested `Src/uniwebview` submodule is not initialized, but that is only needed for mobile/embedded login.
- The example README states a trained profile should be loaded before subscribing to Mental Command / Facial Expression streams, otherwise only neutral actions may be received.

Impact on plan:
- We should use `cortex-example/unity` as the official reference project.
- Next safest validation is to open/test `SimpleExample.unity` before importing into Brain Eaters.
- Brain Eaters import should mimic the example folder layout rather than the standalone repo layout.

Observed friction:
- The `cortex-example` Unity project includes older/example dependencies such as NuGetForUnity and Zenject. We should avoid blindly importing unrelated example assets into Brain Eaters.
- The SDK/example appears to target a Unity 2023-era or older baseline, while Brain Eaters is on Unity 6. This version gap is a compatibility risk and feels outdated for the current project.

Developer question captured:
- Can we copy only `Assets/Plugins/Emotiv-Unity-Plugin` plus an adapted `AppConfig`, or does the plugin rely on other example project assets/packages?
- Which Unity versions are officially supported, and has the plugin been validated with Unity 6?

Current answer:
- Likely yes for desktop-service runtime, but this must be compile-tested.

## Next Steps

1. Focus on MH tasks before SH/LTH work.
2. Optionally open/test `cortex-example/unity/Assets/SimpleExample.unity` with credentials before touching Brain Eaters.
3. Confirm with plugin team whether Brain Eaters should consume `Assets/Plugins/Emotiv-Unity-Plugin` by copying/submodule/package.
4. If approved, run a desktop-service compile spike in Brain Eaters using the example plugin layout.
5. Define the minimal real SDK adapter boundary.
6. Validate real MC flow in this order: authorize, headset discovery, session, profile, `com` stream, gameplay trigger.
7. Capture every blocker/question for the plugin team as it appears.

## MH Task Priority

These are the current Must Have tasks from Sprint 3, in the order we should continue.

### 1. `Integrate Mental Commands (MC) into Brain Eaters using the current SDK`

Current status: In progress.

What is done:
- Plugin repo is available for local analysis under `External/emotiv-unity-plugin`, but is not imported into Unity.
- Gameplay already has a source-independent input layer.
- Simulated MC can trigger Brain Eaters gameplay through the same input path we want the real SDK to use.

Next immediate action:
- Create an import plan for the current SDK that lists exactly which plugin files/packages are required in `Assets/`, which native libraries are missing, and what scripting symbols are required per platform.

Definition of done:
- Current SDK code is imported or wrapped safely enough that Brain Eaters can compile.
- A real SDK-backed command source can read latest Mental Command action/power.

### 2. `Establish a stable baseline flow: Connect device`

Current status: Blocked/Locked until app credentials, runtime target, and device setup are confirmed.

Next immediate action:
- Identify required credentials and runtime assumptions:
- Desktop Cortex Service via EMOTIV Launcher, or embedded library/mobile path.
- Client ID, client secret, app name.
- Whether Brain Eaters should test first on Editor/macOS desktop or on Android/iOS.

Definition of done:
- The app can authorize and list/detect a headset through the current SDK path.

### 3. `Establish a stable baseline flow: Receive MC input`

Current status: In progress.

What is done:
- Simulated MC input works.

Next immediate action:
- Implement the real `ICommandSignalSource` adapter only after the plugin import/compile path is understood.
- Adapter should convert SDK `LatestMentalCommand.act` and `LatestMentalCommand.pow` into `MentalCommandSignal`.

Definition of done:
- Brain Eaters receives a real `com` stream command from Cortex and logs action + power.

### 4. `Establish a stable baseline flow: Trigger simple in-game action (e.g., bomb or interaction)`

Current status: In progress.

What is done:
- Simulated `pull` can map to Mental Power charge.
- Simulated `push` can map to Brain Bomb trigger.

Next immediate action:
- Swap simulated source for SDK-backed source without changing `PlayerController`, `PlayerBombAttack`, or gameplay systems.

Definition of done:
- Real MC command triggers Brain Bomb or an interaction in-game.

### 5. `Identify and document setup complexity: Connection steps`

Current status: In progress.

Current observations:
- The plugin supports two paths: Desktop Cortex Service and Embedded Library.
- Desktop service appears lower-risk for first validation.
- Mobile path requires embedded/native dependencies and authentication/webview setup.

Next immediate action:
- Write the exact setup checklist as we attempt the SDK import and first connection.

### 6. `Identify and document setup complexity: Calibration requirements`

Current status: Blocked/Locked until real profile/training flow is attempted.

Next immediate action:
- Confirm with plugin team whether `LoadProfile` creates/loads automatically and which actions must be trained for this test.

### 7. `Identify and document setup complexity: Latency / responsiveness`

Current status: Blocked/Locked until real MC stream is available.

Next immediate action:
- Add timestamp logging around SDK signal receipt and gameplay trigger once the adapter exists.

### 8. `Create a minimal test scene (isolated from full gameplay if needed)`

Current status: In progress.

Current decision:
- We are using `GameScene` plus a menu installer as the minimal test path for now.

Open question:
- If SDK import creates scene complexity, create a separate `CortexMCTestScene`.

### 9. `Log initial observations on reliability and usability`

Current status: In progress.

Current observations:
- Simulated signal path is reliable.
- Current uncertainty is SDK setup/import/auth/session/profile, not gameplay routing.

## Step 8 - Clarify macOS Test Paths

Date: 2026-05-12

Context:
- The plugin/example documentation describes two different macOS paths, and they should not be mixed.
- `USE_EMBEDDED_LIB`, the MD5 hash, custom URL schemes, and manual native library/framework setup apply to the Embedded Library path.
- The lower-risk first validation path on Mac is Desktop Cortex Service through EMOTIV Launcher, which should not define `USE_EMBEDDED_LIB`.

Recommended first test on macOS:
1. Open `/Users/hernan/dev/emotiv/cortex-example/unity` in Unity.
2. Open `Assets/SimpleExample.unity`.
3. Confirm `USE_EMBEDDED_LIB` is not present in Project Settings > Player > Scripting Define Symbols.
4. Configure `Assets/Plugins/AppConfig.cs` with `ClientId`, `ClientSecret`, and `AppName`.
5. Keep the desktop service URL path active: `AppUrl = "wss://localhost:6868"` when not embedded/mobile.
6. Open EMOTIV Launcher, log in, and ensure Cortex Service is running.
7. Press Play in Unity Editor.
8. Authorize/grant the app if Launcher prompts for permission.
9. Use the example flow: query headset, create session, load profile, subscribe to `com`/`sys`/`dev`, then verify Mental Command output.

Embedded macOS path:
- Only use this if the plugin team confirms it is required or supported for the current test.
- Define `USE_EMBEDDED_LIB`.
- Generate the MD5 of the Cortex application `clientId`.
- Register a custom URL scheme in Unity/macOS Player Settings using `emotiv-{md5(clientId)}`.
- Place the macOS Embedded Cortex native library/framework files under the path required by the example documentation.
- Expect possible standalone/Xcode build steps instead of a clean Unity Editor-only flow.

Current decision:
- Use Desktop Cortex Service first for Sprint 3 Must Have validation.
- Treat embedded macOS as a follow-up risk area because the repository notes that desktop embedded support is under development/not production-ready.

## Questions For Plugin Team

These questions should be reviewed with the team maintaining the plugin.

1. For first Unity Editor validation on macOS, should we use Desktop Cortex Service through EMOTIV Launcher, or should we already use the Embedded Library path?
2. Which files from `unity-plugin/Src` are intended to be copied/imported into a Unity project, and which folders are build-only/internal?
3. Is the plugin intended to be consumed as a Unity package, a git submodule under `Assets`, or by exporting/importing a `.unitypackage`?
4. Are `clientId`, `clientSecret`, and `appName` enough for desktop testing, or do we also need license keys or app registration settings?
5. What is the expected setup sequence for Mental Commands in Unity: `Init`, `Start`, `QueryHeadsets`, `StartStreamData`, `LoadProfile`, then train/receive `com`, or a different order?
6. Does `BCIGameItf.CreatePlayer(playerName)` always create a profile if missing, or can it fail silently depending on headset/profile compatibility?
7. Which Mental Command action should Brain Eaters use for the first test: `pull`, `push`, or a different action with better reliability?
8. Does the `com` stream only emit trained actions, or can it emit untrained/neutral states before profile training?
9. What is the recommended way to detect connection/auth/session/profile/training state changes for UI/debug feedback?
10. Are there known latency characteristics or expected sampling frequency for `LatestMentalCommand` / `com` updates?
11. For iOS/Android, are `EmotivCortexLib.xcframework`, `EmotivCortexLib.aar`, and UniWebView mandatory for the current plugin version?
12. Are marker APIs expected to work through this same plugin path, and should marker calls be tied to active recording/session state?
13. Is Embedded Library on macOS supported in Unity Editor, or only in standalone builds?
14. For embedded macOS, what exact files must be copied into `Assets/Plugins/MacOS`, and are Xcode project edits still required?
15. Is the custom URL scheme format always `emotiv-{md5(clientId)}`, and should the MD5 be lowercase hex with no separators?
