# Emotiv Cortex Input Architecture Reference

## Product Intent

Emotiv interactive projects are not ordinary games. Their purpose is to create experiences where EEG devices can enrich interaction, training, accessibility, and user development while remaining entertaining and usable without hardware during development.

Brain Eaters is the first reference project. Its architecture should make future Cortex SDK integration close to plug-and-play: external mental command signals should be mapped into existing gameplay actions without rewriting the gameplay systems.

## Architectural Layers

Use these layers unless the project has a stronger existing convention:

```text
External Input Layer
Keyboard, mobile UI, simulated mental commands, Cortex SDK/device events.

Adapter Layer
Converts source-specific events into project-neutral command events.

Command Mapping Layer
Maps command events into gameplay intents and handles thresholds, cooldowns, calibration state, and fallback policy.

Gameplay Intent Layer
Interfaces consumed by gameplay, such as movement, view, action triggers, menu navigation, and session control.

Gameplay Systems
Player, abilities, GameManager, UI flow, scoring, level progression.
```

## Naming Guidance

Prefer names that separate source from intent:

- `IGameplayInputSource`: source of normalized gameplay input.
- `ICommandSignalSource`: source of abstract command signals before gameplay mapping.
- `MentalCommandSignal`: external mental command event with metadata.
- `GameplayIntent`: game-level action after mapping.
- `InputCommandRouter`: routes source signals to gameplay intent consumers.
- `CortexMentalCommandAdapter`: Cortex-specific adapter.
- `SimulatedMentalCommandSource`: editor/test fallback.

Avoid names that bake the provider into gameplay:

- `CortexPlayerController`
- `EmotivBombButton`
- `MentalCommandGameManager`
- `KeyboardBrainBomb`

## Mental Command Signal Shape

Use a normalized data shape before touching gameplay. Adjust names to match the codebase:

```csharp
public readonly struct MentalCommandSignal
{
    public string CommandId { get; }
    public float Power { get; }
    public float Confidence { get; }
    public double TimestampSeconds { get; }
    public MentalCommandPhase Phase { get; }
}
```

`CommandId` should be semantic enough to map, but not gameplay-specific. Examples: `push`, `pull`, `lift`, `neutral`, `mental_bomb`.

`Power` and `Confidence` should be preserved separately. Do not collapse them into a boolean too early unless the gameplay truly only needs an edge trigger.

## Mapping Rules

Keep mapping configurable. A mental command should not directly call `PlayerBombAttack` or `GameManager`.

Preferred flow:

```text
MentalCommandSignal("push", confidence 0.84)
-> mapping profile says "push" means ChargeBrainBomb
-> router emits GameplayAction.ChargePressed/ChargeHeld
-> existing player action system consumes it
```

Mapping profiles should support:

- Command-to-action binding.
- Minimum confidence threshold.
- Hold vs pulse behavior.
- Cooldown/debounce.
- Optional per-level overrides.
- Disabled/fallback states when device is disconnected or untrained.

## Runtime Requirements

Every Cortex-ready feature must support:

- No-device mode for development and demos.
- Simulator mode for deterministic testing.
- Explicit connection/calibration state surfaced to UI or debug tools.
- Graceful degradation when Cortex disconnects.
- Logging that helps distinguish input failure from gameplay failure.

## Unity Integration Rules

Do not put Cortex SDK logic in scene-specific gameplay components.

Prefer one of these composition roots:

- A persistent `EmotivServices` or `InputServices` prefab.
- A scene-level installer/bootstrapper.
- A ScriptableObject configuration asset for mappings and thresholds.

Scene and prefab references should point to interfaces or neutral components where Unity serialization allows it. If Unity cannot serialize an interface, use a small MonoBehaviour bridge that implements the interface and keep provider-specific code behind it.

## Validation Checklist

Before considering a Cortex/input architecture change complete, verify:

- The gameplay action works without Cortex.
- The same action can be triggered by existing input and a simulated mental command.
- Gameplay code does not import Cortex SDK namespaces.
- Raw Cortex payloads do not cross into gameplay systems.
- Thresholds and mappings are not hardcoded in player/gameplay classes.
- Device unavailable state does not break scene loading or menu flow.
- Debug logs identify source, mapped action, and rejection reason for ignored commands.

## Brain Eaters Current Intent Examples

Brain Eaters actions that should remain source-independent:

- Movement and camera/look controls.
- Charge brain power.
- Trigger brain bomb.
- Select level.
- Start gameplay.
- Retry level.
- Return to level select or menu.
- Pause/settings/tutorial access.

Potential mental command mapping examples:

- Mental command `push` -> charge or fire brain bomb.
- Mental command `neutral` -> stop charging.
- Mental command `lift` -> special ability or menu confirm.

Treat these as examples, not final design. The exact mapping should be driven by gameplay UX, EEG reliability, and training flow.

## Implementation Bias

Prefer small, reversible refactors:

- Extract interfaces before introducing Cortex implementation.
- Add simulator first, then Cortex adapter.
- Keep existing keyboard/mobile behavior as regression coverage.
- Do not build a large SDK wrapper until the actual SDK version and payload contracts are confirmed.
