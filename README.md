# Unity Action Game — Project Overview

A modular, component-driven Unity action game project featuring dynamic enemies, difficulty scaling, player–enemy interactions, and responsive UI/audio feedback.  
This repository demonstrates clean C# architecture, loose coupling, and data-driven design suitable for modern Unity workflows.

---

## Features

- Component-based gameplay logic  
  Each gameplay system (Player, Enemy, Audio, UI) is built as an independent Unity `MonoBehaviour`.

- Modular Enemy AI  
  Enemies use awareness, health, and contact damage components for autonomous behavior.

- Dynamic Difficulty System  
  Real-time scaling of enemy stats through the `DifficultyCurve` and `EnemyDamageScaler` scripts.

- Responsive Audio Design  
  `UISoundManager` and `Ambience` provide layered sound design with UI and environmental feedback.

- Data-driven Configuration  
  `EnemyArchetype` defines enemy templates, allowing flexible balancing and new archetypes without modifying logic.

- Boss Indicators  
  `BossMarker` provides visual cues for important enemies, improving player awareness.

---

## Architecture Summary

The project follows Unity’s standard lifecycle:
```
Awake() → Start() → Update() → FixedUpdate() → OnDisable() / OnDestroy()
```

Systems communicate primarily via serialized fields and `GetComponent` lookups, ensuring low coupling between scripts.

### Core Systems

| System | Main Script | Responsibility |
|---------|--------------|----------------|
| Player | `PlayerHealth.cs`, `PlayerController.cs` | Handles movement, health, and interaction |
| Enemy | `EnemyHealth.cs`, `EnemyMovement.cs`, `EnemyContactDamage.cs` | Modular AI, pathing, and combat |
| Difficulty | `DifficultyCurve.cs`, `EnemyDamageScaler.cs` | Dynamically adjusts game intensity |
| Audio | `UISoundManager.cs`, `Ambience.cs` | Sound effects and ambient loops |
| Boss | `BossMarker.cs` | Displays UI markers for bosses |
| Data | `EnemyArchetype.cs` | Defines reusable enemy templates |

---

## Example Code Snippets

Enemy contact damage detection and player interaction:
```csharp
// EnemyContactDamage.cs
if (other.CompareTag("Player")) {
    other.GetComponent<PlayerHealth>()?.TakeDamage(contactDamage);
}
```

Dynamic difficulty scaling:
```csharp
// DifficultyCurve.cs
float difficulty = baseDifficulty + (Time.timeSinceLevelLoad * growthRate);
```

---

## Game Flow

1. **Initialization:**  
   On startup, systems like difficulty curves and audio managers initialize global parameters.

2. **Runtime:**  
   Enemies detect players through `PlayerAwareness`, move using `EnemyMovement`, and attack using `EnemyContactDamage`.

3. **Progression:**  
   The difficulty curve increases with time, scaling all enemy stats.

4. **Feedback:**  
   UI and audio managers provide real-time feedback for every action and state change.

---

## Setup Instructions

### Requirements
- Unity 2021.3 LTS or newer  
- C# 9.0 or later  
- Any desktop platform (Windows/macOS/Linux)

### Installation
```bash
git clone https://github.com/Saguny/topdownshooter.git
cd YourGameProject
```
Open the project folder in Unity Hub and select **Open Project**.

---

## Folder Structure

```
Assets/
 ├─ Scripting/
 │   ├─ Player/
 │   ├─ Enemy/
 │   ├─ Game Engine/
 │   ├─ Audio/
 │   └─ UI/
 ├─ Prefabs/
 ├─ Scenes/
 └─ Resources/
```

---

## Design Philosophy

This project emphasizes:
- Loose coupling between scripts  
- Reusability through component logic  
- Clarity via separation of configuration and code  
- Scalability for future content additions

It serves as a foundation for 2D or 3D action and roguelike projects in Unity.

---

## Documentation

For Blog Refer to [Notion.so](https://www.notion.so/Top-Down-Rogue-like-Shooter-Fortgeschrittene-Interaktionstechnologien-296f779cfe0a802f839cedab5b00953e)
(German)
---

## Future Improvements

- Central GameManager for global state management  
- Event bus for inter-system communication  
- Object pooling and caching for improved performance  
- ScriptableObject-based balancing system  

---

## Author

Developed by **Sagun**  
Documentation made by Project Members and Chiichan
