# Cortex Unity Plugin v5 Integration Log

This document tracks the developer experience of integrating and validating Emotiv Cortex Unity Plugin v5.

The purpose is not only to make Brain Eaters work with Cortex v5, but to identify what a public Unity developer would need for a plug-and-play integration.

## Goals

- Validate the plugin v5 setup from a clean developer perspective.
- Document every required step, hidden assumption, and failure mode.
- Identify gaps in README, examples, package structure, runtime setup, and Unity compatibility.
- Produce actionable feedback for the plugin team.
- Keep Brain Eaters decoupled from plugin-specific classes through the existing input/mental-command adapter architecture.

## Architecture Rule

Brain Eaters gameplay must not depend directly on Cortex SDK/plugin classes.

Preferred dependency path:

```text
Cortex Unity Plugin v5
-> Brain Eaters Cortex adapter
-> ICommandSignalSource / MentalCommandSignal
-> MentalCommandGameplayInputSource
-> PlayerInputRouter
-> Gameplay systems
```

## Initial Repository Access Check

Date: 2026-05-13

Provided URLs:
- `https://github.com/Emotiv/unity-plugin-v5/`
- `https://github.com/Emotiv/unity-plugin-v5/blob/main/Src/README.md`

Result:
- Direct raw README access returned `404: Not Found`.
- `git ls-remote https://github.com/Emotiv/unity-plugin-v5.git` required GitHub credentials.

Current interpretation:
- The repository is likely private, not yet published, or requires authenticated GitHub access.

Immediate blocker:
- The repo must be cloned locally or made accessible through an authenticated GitHub connector before we can inspect structure, examples, dependencies, package format, or compile behavior.

## README Review

Date: 2026-05-13

Source:
- `/Users/hernan/dev/emotiv/unity-plugin-v5-README.md`

Key findings:
- The plugin is presented as `Cortex Unity Plugin`.
- Installation path says to import the `unity-plugin-v5` package through `Assets > Import Package`.
- The package claims to include all required native libraries for Android and iOS.
- `Newtonsoft Json Unity Package` is listed as a dependency.
- Credentials are configured through `Tools > Emotiv Cortex SDK`.
- Settings are saved to `Assets/Settings/EmotivCortexSettings.asset`.
- The settings asset is automatically added to Player Settings > Preloaded Assets.
- `CortexRuntimeManager` is the SDK entry point.
- Runtime services are split into `Auth`, `Headset`, and `SimpleBCI`.
- The runtime can be configured from editor-saved settings or by constructing `CortexRuntimeConfig` in code.
- Auth flow uses `GetApiInfoAsync()`, then `LoginAsync()` for mobile if no user is logged in, or `InitAsync()` if the user exists but app initialization is not done.
- Headset flow uses `ScanHeadsetAsync()`, `GetHeadsets()`, and `ConnectHeadsetAsync()`.
- Data streams are selected with `DataSampleType`, including `DeviceInfo` and `MentalCommand`.
- BCI flow uses `LoadProfileAsync(headsetId)`, `StartTrainingAsync(...)`, `AcceptTrainingAsync(...)`, `CancelTrainingAsync(...)`, and `ConfigureMentalCommandAsync(...)`.
- Runtime data can be read with `TakeLatestSample(headsetId, DataSampleType.MentalCommand, out var sample)`.

Positive developer-experience signals:
- The API shape is much cleaner than the previous example repository.
- Service separation is understandable: auth, headset, BCI, data sampling.
- Runtime config from an editor tool is better than hardcoded credentials.
- The Mental Command path is explicit and maps cleanly to Brain Eaters' existing `ICommandSignalSource` adapter.

Friction / missing details:
- The README does not show the exact package filename or import source.
- It is unclear whether this is a `.unitypackage`, UPM package, tarball, or project-local package.
- It does not specify supported Unity versions.
- It does not clearly state whether macOS Unity Editor is supported.
- It does not explain whether desktop uses EMOTIV Launcher/Cortex Service or embedded runtime.
- It does not mention `USE_EMBEDDED_LIB`, MD5 redirect schemes, or whether those are obsolete in v5.
- It does not specify iOS `Info.plist` automation or Android manifest/permission automation.
- It does not describe sample scenes, prefabs, or a first-run validation scene.
- It does not document expected error codes or common failure causes.
- It does not explain how long `GetHeadsets()` should be polled after scanning.
- It does not mention connection/profile/training state events, only async calls and polling samples.

Brain Eaters integration interpretation:
- The first adapter should wrap `ICortexRuntimeManager`.
- `HeadsetService.TakeLatestSample(... MentalCommand ...)` should be converted into `MentalCommandSignal`.
- `MentalCommandDataSample.Action` should map to our normalized command name.
- `MentalCommandDataSample.Power` should map to our normalized power/confidence value.
- Brain Eaters gameplay code should remain unchanged.

Proposed adapter target:

```text
CortexRuntimeManager
-> Headset.TakeLatestSample(headsetId, DataSampleType.MentalCommand, out sample)
-> CortexV5MentalCommandSource : ICommandSignalSource
-> MentalCommandGameplayInputSource
-> Brain Eaters gameplay
```

Open validation items:
- Confirm the exact namespace and type signatures from the real plugin code.
- Confirm whether `MentalCommandDataSample.Power` is `0..1`, `0..100`, or another range.
- Confirm whether `Action` returns `"neutral"` when no trained command is detected.
- Confirm how to detect headset disconnects and profile/training state.
- Confirm whether `LoadProfileAsync(headsetId)` creates a profile silently or requires a profile name/user flow.

## Package Inspection

Date: 2026-05-13

Package:
- `/Users/hernan/dev/emotiv/com.emotiv.cortex-5.0.0-release.6.tgz`

Package metadata:
- Package name: `com.emotiv.cortex`
- Version: `5.0.0-release.6`
- Display name: `Emotiv Cortex SDK`
- Unity version: `2021.3`
- Dependency: `com.unity.nuget.newtonsoft-json` `3.2.1`

Important package contents:
- `Runtime/Emotiv.Cortex.Runtime.asmdef`
- `Editor/Emotiv.Cortex.Editor.asmdef`
- `Runtime/API/CortexRuntimeManager.cs`
- `Runtime/API/ICortexRuntimeManager.cs`
- `Runtime/Services/AuthService.cs`
- `Runtime/Services/HeadsetService.cs`
- `Runtime/Services/SimpleBCIService.cs`
- `Runtime/Internal/Core/Android/EmotivCortexLib.aar`
- `Runtime/Internal/Core/Ios/EmotivCortexLib.xcframework`
- `Runtime/Internal/Thirdparty/uniwebview/...`

Packaging observation:
- This file is a Unity Package Manager tarball (`.tgz`), not a classic `.unitypackage`.
- The README instruction “Import via Assets > Import Package” is likely incorrect for this artifact.
- Expected install path for this artifact is Unity Package Manager > Add package from tarball.

Critical platform finding:
- `CortexClientFactory.Create(...)` currently supports only:
  - `UNITY_ANDROID`
  - `UNITY_IOS`
- For every other platform it throws:
  - `PlatformNotSupportedException("No Cortex client available for this platform/build symbols. Supported: UNITY_ANDROID or UNITY_IOS.")`

Impact:
- This package cannot run a real Cortex connection in macOS Unity Editor or desktop builds in its current form.
- It can be inspected and potentially compiled in Unity Editor, but runtime initialization will fail outside Android/iOS.
- First real SDK validation must happen on Android or iOS unless the plugin team provides a desktop/Editor client.

Useful API details confirmed:
- `MentalCommandDataSample` exposes:
  - `string Action`
  - `float Power`
- Mental Command data is parsed from stream format `[action, power]`.
- `HeadsetService.TakeLatestSample(...)` removes the sample after reading it, so consumers should poll reliably.
- `SimpleBCIService.LoadProfileAsync(headsetId)` creates or loads a profile automatically using a headset-type-derived profile name:
  - `Insight_Profile`
  - `Epoc_Profile`
  - `TwoEEGChannels_Profile`
- `AuthService.LoginAsync()` is mobile-only and throws `NotSupportedException` outside Android/iOS.
- Mobile auth still computes `emotiv-{md5(clientId)}` for UniWebView redirect handling.

Developer-experience issues found:
- README does not state that runtime currently supports only Android/iOS.
- README does not warn that macOS/Editor runtime validation will throw `PlatformNotSupportedException`.
- README mentions Android/iOS native libraries but does not explicitly say desktop is unsupported.
- README import instructions conflict with the delivered `.tgz` package format.
- Package includes UniWebView binaries internally, but the README does not explain licensing/maintenance expectations for this third-party dependency.

Recommended next validation:
1. Install the `.tgz` through Package Manager in a clean Unity project or isolated branch.
2. Confirm whether it compiles in Unity 6 despite package metadata targeting Unity `2021.3`.
3. Confirm whether `Tools > Emotiv Cortex SDK` appears.
4. Confirm whether `Assets/Settings/EmotivCortexSettings.asset` is created and added to Preloaded Assets.
5. Do not expect real runtime connection in Editor/macOS.
6. For real connection validation, build to Android or iOS.

## First Evaluation Checklist

When the repository is available locally, inspect:

1. Unity version used by the plugin and examples.
2. Whether the plugin is delivered as a UPM package, `.unitypackage`, `Assets/` folder, git submodule, or manual copy.
3. Whether examples run in Unity Editor on macOS without additional native setup.
4. Required credentials: client ID, client secret, app name, license keys, app registration, redirect URLs.
5. Required runtime path: EMOTIV Launcher/Cortex Service, embedded library, mobile libraries, or all of them.
6. Required scripting symbols such as `USE_EMBEDDED_LIB`.
7. Whether Mental Command examples include profile load/training/subscription flow.
8. Whether the README explains how to get from fresh clone to first received `com` event.
9. Whether API events/states are easy to consume from gameplay code.
10. Whether errors are surfaced clearly enough for external developers.

## Plug-And-Play Quality Criteria

The plugin should ideally provide:

- A minimal sample scene that receives Mental Commands in under 10 minutes.
- A single prefab or bootstrap component for app credentials, connection, session, profile, and stream subscription.
- Clear separation between desktop service, embedded desktop, Android, and iOS paths.
- A documented event API for connection state, session state, profile state, stream data, and errors.
- A simulated/mock source for development without headset access.
- A Unity package installation path that does not require copying unclear internal folders.
- Troubleshooting for auth failures, no headset detected, no profile loaded, neutral-only commands, missing native libraries, and unsupported Unity versions.

## Questions For Plugin Team

1. Is `unity-plugin-v5` intended to be public soon, or is access currently restricted?
2. What is the intended install method for public Unity developers?
3. What Unity versions are officially supported by v5?
4. Is macOS Unity Editor supported for first validation, or only standalone builds?
5. Should first-time developers use EMOTIV Launcher/Cortex Service or embedded libraries?
6. Does v5 include a minimal Mental Command sample scene?
7. Does v5 include a mock/simulated data source for development without a headset?
8. What is the expected path from app credentials to receiving the first `com` event?
9. Are profile creation/loading/training handled by the plugin, or must the developer orchestrate every step?
10. What are the known platform-specific setup requirements for iOS and Android?
11. Is v5 intentionally mobile-only for release `5.0.0-release.6`?
12. Will v5 support macOS/Windows Editor or desktop runtime via Cortex Service, or is that out of scope?
13. Should the README install step say Package Manager > Add package from tarball instead of Assets > Import Package?
14. What is the expected developer workflow for testing Mental Commands without deploying to Android/iOS every iteration?
15. Is Unity `2021.3` the minimum supported version, or the only validated version?
16. Is the custom URL scheme format always `emotiv-{md5(clientId)}`, and should the MD5 be lowercase hex with no separators?
