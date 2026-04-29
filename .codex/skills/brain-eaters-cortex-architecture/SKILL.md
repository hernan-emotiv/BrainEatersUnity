---
name: brain-eaters-cortex-architecture
description: Design or refactor Emotiv interactive app architecture for Cortex-ready input, mental commands, EEG-driven gameplay actions, fallback simulators, and device-independent control mapping. Use when working on Brain Eaters or any Emotiv Unity/project code involving input abstraction, gameplay commands, Cortex SDK integration readiness, mental command routing, adapters, test doubles, or replacing keyboard/mobile actions with EEG-driven commands.
---

# Brain Eaters Cortex Architecture

## Purpose

Use this skill to keep Emotiv projects ready for Cortex SDK integration without coupling gameplay code to a specific physical input source. Treat Brain Eaters as the first concrete implementation, but apply the same architecture to future Emotiv interactive experiences.

## Core Rule

Gameplay systems must depend on game-intent abstractions, not on keyboard, mobile controls, Cortex SDK classes, websocket messages, or device-specific APIs.

Preferred dependency direction:

```text
Cortex SDK / Keyboard / Mobile / Simulator
-> Input Source Adapter
-> Intent Router / Command Mapper
-> Gameplay Intent Interfaces
-> Gameplay Systems
```

Forbidden dependency direction:

```text
Gameplay System -> Cortex SDK
Gameplay System -> Keyboard.current
Gameplay System -> Touchscreen.current
Gameplay System -> raw mental command string
```

## Workflow

1. Inspect the current input path before changing code.
2. Identify the gameplay intent being expressed, such as move, look, charge, bomb, select, pause, confirm, or retry.
3. Keep or introduce an interface for that intent before wiring a concrete input source.
4. Implement concrete adapters for keyboard/mobile/simulator/Cortex separately.
5. Make Cortex optional at runtime; the game must still run without a device.
6. Validate by proving the same gameplay action works from at least two sources, usually simulator plus existing input.

## When Cortex Is Mentioned

Do not browse or assume the current Cortex API unless the user asks for live SDK/API details. For architectural preparation, depend only on stable concepts:

- A device/session connection layer exists outside gameplay.
- Mental commands are external signals that need normalization.
- Commands may have confidence, power, latency, calibration state, and connection state.
- The game should support fallback simulation when hardware or authentication is unavailable.

If implementation requires exact Cortex endpoints, payloads, authentication, or SDK package details, consult official Emotiv Cortex documentation before coding.

## Reference

Load `references/emotiv-cortex-input-architecture.md` when the task involves:

- Designing or changing input interfaces.
- Adding Mental Command support.
- Preparing a Unity scene or prefab for Cortex-driven actions.
- Refactoring `GameManager`, `PlayerController`, `PlayerInputRouter`, input sources, or command routing.
- Creating tests, simulators, or editor tools for EEG/Mental Command flows.
