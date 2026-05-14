# Sprint 3 SDK Import Assessment

Date: 2026-05-11

## Objective

Assess how to safely integrate the current `Emotiv/unity-plugin` into Brain Eaters for the Sprint 3 Must Have task:

`Integrate Mental Commands (MC) into Brain Eaters using the current SDK`

The goal is not to import everything immediately. The goal is to identify the smallest safe import path that lets Brain Eaters receive Mental Command data and trigger gameplay through the existing source-independent input architecture.

## Current Plugin Location

Local analysis path:

`External/emotiv-unity-plugin`

Actual repo path:

`/Users/hernan/dev/emotiv/unity-plugin`

This repo is intentionally outside `Assets/` and ignored by git. Unity does not currently import, compile, or package it.

## Official Example Project

Local path:

`/Users/hernan/dev/emotiv/cortex-example/unity`

Relevant files:

- `Assets/SimpleExample.unity`
- `Assets/SimpleExample.cs`
- `Assets/Plugins/AppConfig.cs`
- `Assets/Plugins/Emotiv-Unity-Plugin`
- `Packages/manifest.json`

Submodule status:

- `unity/Assets/Plugins/Emotiv-Unity-Plugin` is initialized.
- Nested `Src/uniwebview` submodule is not initialized, which is expected for desktop-only use.

The official Unity README says:

- Desktop uses EMOTIV Launcher / Cortex Service.
- Desktop must not define `USE_EMBEDDED_LIB`.
- Desktop does not require UniWebView.
- Mobile requires embedded library and UniWebView access.
- `SimpleExample.unity` demonstrates the main workflow.

This is stronger evidence than the standalone `unity-plugin` repo alone. The example project should be treated as the reference for import layout and runtime sequence.

## High-Level Finding

The plugin supports two integration paths:

- Desktop Cortex Service path: connects to `wss://localhost:6868` through EMOTIV Launcher/Cortex Service.
- Embedded/mobile path: uses native embedded Cortex libraries, Android/iOS wrappers, and mobile authentication support.

For Sprint 3 MH validation, the recommended first path is:

`Unity Editor/macOS -> Desktop Cortex Service -> EmotivUnityItf/BCIGameItf -> Brain Eaters ICommandSignalSource adapter`

Reason:

- The downloaded repo does not include the private/native embedded mobile dependencies.
- Desktop Service avoids importing incomplete Android/iOS embedded dependencies.
- Sprint success only requires proving MC can trigger in-game, even if unstable.
- The official `cortex-example/unity` README explicitly states desktop does not require UniWebView and should not define `USE_EMBEDDED_LIB`.

## Plugin Runtime Files

Candidate runtime C# files for desktop-service validation:

- `Authorizer.cs`
- `BCIGameItf.cs`
- `BCITraining.cs`
- `BandPowerDataBuffer.cs`
- `BufferStream.cs`
- `Config.cs`
- `CortexClient.cs`
- `DataBuffer.cs`
- `DataStreamManager.cs`
- `DataStreamProcess.cs`
- `DevDataBuffer.cs`
- `EegMotionDataBuffer.cs`
- `EmbeddedCortexClient.cs`
- `EmotivUnityItf.cs`
- `Headset.cs`
- `HeadsetFinder.cs`
- `MentalStateModel.cs`
- `MyLogger.cs`
- `PMDataBuffer.cs`
- `RecordManager.cs`
- `RegistryConfig.cs`
- `SessionHandler.cs`
- `TrainingHandler.cs`
- `Types.cs`
- `UniWebViewManager.cs`
- `Utils.cs`
- `WebsocketCortexClient.cs`

Notes:

- `EmbeddedCortexClient.cs` is guarded by platform/symbol defines, but it may still be needed because `CortexClient.Instance` references it under `UNITY_ANDROID || UNITY_IOS || USE_EMBEDDED_LIB`.
- `UniWebViewManager.cs` is guarded by `UNITY_ANDROID || UNITY_IOS`; it should not compile into desktop Editor but can remain present if its guards are correct.

## Plugin Binary Dependencies

Desktop-service path appears to need:

- `JsonNet/Newtonsoft.Json.dll`
- `WebSocket4Net.0.15.2/WebSocket4Net.dll`
- `SuperSocket.ClientEngine.Core.0.10.0/SuperSocket.ClientEngine.dll`

Potentially needed only for embedded Windows/auth:

- `IdentityModel/IdentityModel.dll`
- `com.cdm.authentication` package

Current Brain Eaters `Packages/manifest.json` does not explicitly include:

- `com.unity.nuget.newtonsoft-json`

The plugin includes its own `Newtonsoft.Json.dll`, but Unity 6 projects often prefer the Unity package to avoid duplicate Newtonsoft assemblies. This needs validation during import.

## Mobile/Embedded Dependencies

The downloaded repo does not currently include the required embedded mobile libraries:

- `Src/AndroidPlugin/EmotivCortexLib/EmotivCortexLib.aar` is not present.
- `Src/IosPlugin/EmotivCortexLib.xcframework` is not present.
- `Src/uniwebview` exists only as an empty/unpopulated submodule path.

The repo `.gitmodules` references:

`git@github.com:Emotiv/uniwebview.git`

This appears to be private. Without access, mobile/embedded authentication cannot be fully validated from the downloaded repo alone.

## Folders To Avoid Importing Initially

Do not import these into Brain Eaters for the first desktop MC test:

- `Src/AndroidPlugin`
- `Src/IosPlugin`
- `Src/CortexApi`
- `Src/PostProcessBuild`
- `Src/Editor`
- `Src/uniwebview`
- `Src/com.cdm.authentication`, unless embedded/mobile auth is explicitly required for the test

Reason:

- They introduce native dependencies, platform-specific post-build steps, or private dependencies not needed for the first desktop-service validation.

## Recommended Import Strategy

### Phase 1 - Desktop Service Compile Spike

Create a temporary import folder, for example:

`Assets/Plugins/Emotiv-Unity-Plugin`

Import only:

- The same plugin layout used by `cortex-example/unity`.
- `Assets/Plugins/AppConfig.cs` equivalent, adapted for Brain Eaters.
- Runtime C# files needed by desktop service.
- WebSocket4Net DLL.
- SuperSocket ClientEngine DLL.
- Newtonsoft dependency, using the same strategy as the example project.

Do not define:

- `USE_EMBEDDED_LIB`

Expected runtime path:

```text
EmotivUnityItf.Init(clientId, clientSecret, appName)
EmotivUnityItf.Start()
QueryHeadsets()
StartDataStream(["sys", "com", "dev"], headsetId)
LoadProfile(profileName)
Read LatestMentalCommand.act / LatestMentalCommand.pow
```

Observed official example sequence in `SimpleExample.cs` for desktop:

```text
EmotivUnityItf.Instance.Init(
    AppConfig.ClientId,
    AppConfig.ClientSecret,
    AppConfig.AppName,
    AppConfig.AllowSaveLogToFile,
    AppConfig.IsDataBufferUsing,
    AppConfig.AppUrl)
EmotivUnityItf.Instance.Start()
QueryHeadsets()
CreateSessionWithHeadset(headsetId)
LoadProfile(profileName)
SubscribeData(streams including "com" and "sys")
Read LatestMentalCommand.act / pow
```

### Phase 2 - Brain Eaters Adapter

Add an SDK-backed source that implements:

`ICommandSignalSource`

Likely name:

`EmotivCortexMentalCommandSource`

It should:

- Read from `EmotivUnityItf.Instance.LatestMentalCommand` or `BCIGameItf.Instance`.
- Convert `act` to `MentalCommandSignal.CommandId`.
- Convert `pow` to `MentalCommandSignal.Power`.
- Treat confidence as `pow` or `1` initially unless the SDK exposes separate confidence.
- Surface connection/session/profile status as debug text.

It should not:

- Call `PlayerBombAttack`.
- Call `PlayerController`.
- Depend on level-specific objects.
- Leak Cortex SDK types into gameplay classes.

### Phase 3 - Real Device Validation

Validate this sequence:

1. App authorizes.
2. Headset is detected.
3. Session is created.
4. Profile is loaded.
5. `com` stream is subscribed.
6. `LatestMentalCommand.act` changes from `neutral`/`NULL` to a trained action.
7. Brain Eaters receives the normalized command.
8. The command triggers charge/bomb through the existing input wrapper.

## Risks

### Unity Version Gap

The Unity example/plugin documentation appears to target an older Unity baseline. The example README says Unity 2021+ and the SDK/example context provided to the team indicates a Unity 2023-era setup, while Brain Eaters currently runs on Unity 6.

Impact:

- Package/dependency compatibility may differ.
- Android Gradle plugin/template expectations may be outdated for Unity 6.
- iOS/Xcode post-build expectations may require manual updates.
- DLL compatibility should be compile-tested before assuming the plugin is production-ready in Brain Eaters.

Mitigation:

- Validate first in the official example project.
- Then run a small compile spike in Brain Eaters before building any gameplay-facing adapter.
- Ask the plugin team what Unity versions are officially supported and whether Unity 6 has been tested.

### Duplicate Newtonsoft

The plugin bundles `Newtonsoft.Json.dll`, while Unity projects may use `com.unity.nuget.newtonsoft-json`. Duplicates can cause ambiguous references or runtime load issues.

Mitigation:

- First check whether Brain Eaters already resolves `Newtonsoft.Json`.
- Prefer one Newtonsoft source only.

### Missing Android/iOS Embedded Libraries

The repo does not include `.aar` or `.xcframework` embedded Cortex libraries.

Mitigation:

- Do not attempt mobile embedded integration for the first MH validation.
- Ask plugin team how to obtain and place these libraries.

### Private UniWebView Dependency

`Src/uniwebview` is a private submodule and is empty locally.

Mitigation:

- Avoid mobile auth path for first test.
- Ask if UniWebView is mandatory for iOS/Android and whether there is an alternative auth path.

### Unclear Import Packaging

The repo has an editor script for exporting a Unity package, but no ready package is present in the downloaded checkout.

Mitigation:

- Ask plugin team whether the intended consumption path is package/submodule/copy/unitypackage.
- For sprint spike, isolate copied files under a clearly named third-party folder.

### Profile/Training Flow Ambiguity

The plugin helper `BCIGameItf.CreatePlayer(playerName)` calls `LoadProfile(playerName)`, and comments suggest it may create if missing, but this should be confirmed.

Mitigation:

- Ask plugin team to confirm profile creation/loading behavior and failure modes.

## Questions For Plugin Team From This Assessment

1. Is Desktop Cortex Service through EMOTIV Launcher the recommended first validation path for Unity Editor/macOS?
2. Is `cortex-example/unity/Assets/Plugins/Emotiv-Unity-Plugin` the recommended folder layout to copy into production Unity projects?
3. Is there a prepared `.unitypackage` or UPM package for this plugin, or should teams copy the plugin submodule folder manually?
4. Which exact folders should be imported for desktop-service-only usage?
5. Which Unity versions are officially supported by the current plugin/example, and has Unity 6 been tested?
6. Is the Unity 2023-era SDK/example expected to work unchanged in Unity 6?
7. Should we use bundled `Newtonsoft.Json.dll` or Unity's `com.unity.nuget.newtonsoft-json` package?
8. Are `WebSocket4Net.dll` and `SuperSocket.ClientEngine.dll` still the recommended websocket dependencies for Unity 6?
9. What is the canonical startup sequence for MC in Unity using this plugin?
10. Should profile loading happen before subscribing to `com`, as the README says only neutral actions are received without a trained profile?
11. Does `StartStreamData(["sys","com","dev"], headsetId)` automatically create a session and subscribe, or should session creation and subscribe be split for reliability?
12. Does `LoadProfile(profileName)` create the profile if missing, and how should failures be detected?
13. Which Mental Command action should we train first for a game ability: `push`, `pull`, or another action?
14. Is `LatestMentalCommand.pow` the only threshold signal we should use, or is there a separate confidence/quality value?
15. Does `LatestMentalCommand` update on every `com` sample or only when the command changes?
16. What is the expected frequency/latency of `com` updates in the plugin?
17. Are markers usable without starting a record, or do marker APIs require an active recording session?
18. What files are required for iOS/Android embedded integration, and how do we get `EmotivCortexLib.aar`, `EmotivCortexLib.xcframework`, and `uniwebview` access?

## Current Recommendation

Proceed with a desktop-service compile spike first.

Do not import mobile/embedded folders until the plugin team confirms the native dependency path and we have access to the private assets.
