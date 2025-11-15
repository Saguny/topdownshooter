# Top-Down Roguelike Shooter Engine

A modular and extensible top-down roguelike shooter framework built with Unity 6.  
The project features an event-driven game loop with waves and final rush, dynamic enemy spawning, ScriptableObject-based upgrades, auto-aim and auto-fire combat, aura and AOE abilities, and scalable difficulty curves.

This repository is intended as a foundation for building your own roguelike shooter or as a reference for clean, modular gameplay architecture in Unity 6.

---


![GitHub Contributors](https://contrib.rocks/image?repo=Saguny/TopDownRoguelikeEngine)

[![Readme Card](https://github-readme-stats.vercel.app/api/pin/?username=Saguny&repo=TopDownRoguelikeEngine&show_owner=true)](https://github.com/Saguny/TopDownRoguelikeEngine)





## Features

- Event-driven game loop with wave phases and final rush
- Spawn Director with difficulty scaling, weighted enemy archetypes, bosses, and secret boss logic
- Modular enemy architecture (Archetypes, RegistryA, Awareness, Movement, Contact Damage)
- Player systems: movement, auto-aiming, auto-shooting, health, inventory, pickups, stat context
- ScriptableObject-based upgrade system with multiple upgrade types (fire rate, damage, aura, AOE, etc.)
- Aura and AOE attack systems with radius scaling and periodic damage
- UI systems for upgrades, wave display, timers, health, pause, and game over
- Object pooling and helper utilities for performance and code reuse
- Clear separation of logic through a global event bus (`GameEvents`)

---

## Project Structure

The Unity project is organized as follows (top level under `Assets`):

```text
Assets/
  Animations/
  Data/
  Prefabs/
  Scripting/
  Sounds/
  Sprites/
```

The core code lives in the `Scripting` folder and is further grouped by responsibility:

```text
Assets/Scripting/
  Enemy/
    (EnemyHealth, EnemyMovement, EnemyContactDamage, EnemyArchetype, EnemyRegistry, etc.)

  Game Engine/
    (GameLoopController, GameEvents, SpawnDirector, DifficultyCurve, FinalRushArena, FinalRushArenaController, ObjectPool, etc.)

  Player/
    (PlayerMovement, AutoShooter, AutoAimService, PlayerHealth, PlayerInventory, StatContext, MagnetArea, Aura, AOEAttack, AOEProjectile, etc.)

  QoL/
    (GearSweepOnWaveClear, utility behaviours, helpers)

  UI/
    (UIWaveAndTimer, UpgradeMenuUI, PauseMenu, GameOverScreen, BGM-related UI hooks, etc.)

  Upgrade/
    (UpgradeData, UpgradeType and related logic)
```

The `Data` folder contains ScriptableObjects such as `DifficultyCurve`, `EnemyArchetype` assets and `UpgradeData` assets.  
`Prefabs`, `Animations`, `Sounds` and `Sprites` contain the visual and audio content referenced by the scripts.

---

## Requirements

- Unity 6 (6000.x or newer)  
- New Unity Input System enabled (used by `PlayerMovement` and other input-related scripts)  

URP is optional. The scripts are renderer-agnostic.

---

## Getting Started

### 1. Clone or Download

Clone the repository or download it as a zip and extract it:

```bash
git clone https://github.com/Saguny/TopDownRoguelikeEngine
```

Open the project folder in Unity 6 via the Unity Hub.

---

### 2. Enable the Input System

In Unity:

1. Open `Edit -> Project Settings -> Player`.
2. Under `Active Input Handling`, select `Input System Package` (or `Both` if you need legacy input elsewhere).
3. Let Unity restart the editor if prompted.

---

### 3. Open or Create a Scene

If the repository includes a demo or sample scene, you can open it directly.  
Otherwise, create a new scene and set it up as described below.

---

### 4. Basic Scene Setup

In a new scene:

1. **Player**

   - Create a new GameObject (or use the provided player prefab from `Assets/Prefabs` if available).
   - Add the following components:
     - `PlayerMovement`
     - `AutoShooter`
     - `AutoAimService`
     - `PlayerHealth`
     - `PlayerInventory`
     - `StatContext`
   - Add a collider and Rigidbody2D as required.
   - Give the object the tag `Player`.
   - Optionally, add a child object with `MagnetArea` for gear pickup magnet radius and any aura or AOE prefabs if used.

2. **Game Loop**

   - Create an empty GameObject named `GameLoop`.
   - Add `GameLoopController`.
   - Assign any required references, such as the subjective death effect prefab.

3. **Spawning**

   - Create an empty GameObject named `SpawnDirector`.
   - Add the `SpawnDirector` component.
   - Assign:
     - A `DifficultyCurve` asset from `Assets/Data` (or create one via `Create -> Rogue -> DifficultyCurve`).
     - One or more `EnemyArchetype` assets for regular enemies, chunky enemies, and bosses.
   - Ensure the enemy prefabs referenced by the archetypes are present in `Assets/Prefabs`.

4. **UI**

   - Create a Canvas.
   - Add and wire the scripts from `Assets/Scripting/UI`, such as:
     - `UIWaveAndTimer` for wave and timer display
     - `UpgradeMenuUI` for level-up choices
     - `GameOverScreen` for the game over panel
     - `PauseMenu` for pause handling
   - Connect the required TextMeshProUGUI fields, sliders, and buttons.

5. **Audio (Optional but recommended)**

   - Add a `BGMManager` GameObject to handle background music.
   - Add `UISoundManager` or related components for UI click and hover sounds.
   - Assign AudioClips from `Assets/Sounds`.

6. **ScriptableObjects**

   - In `Assets/Data`, create or edit:
     - `EnemyArchetype` assets via `Create -> Rogue -> EnemyArchetype`
     - `DifficultyCurve` assets via `Create -> Rogue -> DifficultyCurve`
     - `UpgradeData` assets via `Create -> Rogue -> Upgrade`
   - Register the upgrades in `PlayerInventory` so they appear in the upgrade menu.

---

## How The Core Loop Works

At runtime:

1. `GameLoopController` starts a wave loop, tracking total run time and wave time.
2. `GameEvents` broadcasts key events such as wave start, wave cleared, final rush start/end, and enemy killed.
3. `SpawnDirector` listens to these events and spawns enemies based on the current time and the active `DifficultyCurve`.
4. The player moves (`PlayerMovement`), automatically aims at enemies (`AutoAimService`) and fires projectiles (`AutoShooter` and `Projectile`).
5. Enemies track the player (`PlayerAwareness` and `EnemyMovement`) and apply contact damage (`EnemyContactDamage`).
6. When enemies die (`EnemyHealth`), they fire `OnEnemyKilled` events and may drop gears and healing items.
7. `PlayerInventory` listens for kill events, collects gears, and triggers level-up upgrade choices, which modify stats through `StatContext`.
8. When the player dies (`PlayerHealth`), the `GameOverScreen` is displayed and further input is blocked or redirected.

---

## Extending The Project

You can extend the project in several ways:

- Add new enemy types by creating additional `EnemyArchetype` assets and corresponding prefabs.
- Add new upgrade types by extending `UpgradeType` and updating `StatContext` and related logic.
- Tune difficulty with `DifficultyCurve` assets.
- Introduce new weapons or firing patterns by expanding `AutoShooter` or creating new shooter components.
- Implement more complex boss behaviours using custom scripts in the `Enemy` or `Game Engine` folders.

Because most communication is done through the `GameEvents` static class, you can add new systems that plug into the loop without modifying the existing core code.

