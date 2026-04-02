# Stable Level - Complete Setup Guide

## Folder Structure

```
Assets/
├── Scripts/
│   └── StableLevel/
│       ├── StableLevelManager.cs    — Central phase controller (6 phases)
│       ├── SceneLoader.cs           — Scene transition trigger
│       ├── ScreenFader.cs           — Fade-to-black transitions
│       ├── LassoPickup.cs           — Phase 2 lasso tool pickup
│       ├── WaveSpawner.cs           — Phase 3 enemy wave system
│       ├── RanchHandEnemy.cs        — Wave enemy (Bullet.cs compatible)
│       ├── DeathNotifier.cs         — (In WaveSpawner.cs) Fallback death event
│       ├── LassoThrow.cs            — Phase 4 lasso input handler
│       ├── LassoProjectile.cs       — Phase 4 lasso projectile
│       ├── PhantomRider.cs          — Phase 4 waypoint rider
│       ├── QuickDrawDuel.cs         — Phase 5 abstract base class
│       ├── StableDuel.cs            — Phase 5 concrete implementation
│       └── StableReward.cs          — Phase 6 relic + speed boost
├── Scenes/
│   ├── MainScene.unity              — Existing hub (add SceneLoader trigger)
│   └── StableScene.unity            — New stable level scene
└── Prefabs/
    └── StableLevel/
        ├── RanchHandEnemy.prefab    — Wave enemy prefab
        └── LassoProjectile.prefab   — Lasso projectile prefab
```

---

## Step-by-Step Unity Editor Setup

### 1. Create the StableScene

1. **File → New Scene** (use the 2D URP template if available)
2. **File → Save Scene As** → `Assets/Scenes/StableScene.unity`
3. **File → Build Settings** → Add both `MainScene` and `StableScene` to the build list

### 2. Configure Layers

1. **Edit → Project Settings → Tags and Layers**
2. Verify Layer 6 = `Enemy` (already exists)
3. Verify Layer 7 = `Lasso` (added by this PR in TagManager.asset)
4. **Edit → Project Settings → Physics 2D**
5. In the **Layer Collision Matrix**, uncheck ALL collisions for the `Lasso` layer EXCEPT with `Enemy`
   - Lasso ↔ Enemy = ✓ (checked)
   - Lasso ↔ Default = ✗ (unchecked)
   - Lasso ↔ Player = ✗ (unchecked)
   - Lasso ↔ Everything else = ✗ (unchecked)

### 3. Set Up the Screen Fader (Required in Both Scenes)

1. In the scene, create **UI → Canvas** named `FadeCanvas`
   - Canvas: Screen Space - Overlay, Sort Order = 999
2. Add child **UI → Image** named `FadePanel`
   - Color: Black (0, 0, 0, 1)
   - Rect Transform: Stretch to fill entire canvas (all anchors = 0,0 to 1,1, all offsets = 0)
3. Add a **CanvasGroup** component to `FadePanel`
   - Alpha: 0
   - Blocks Raycasts: unchecked
4. Create an **Empty GameObject** named `ScreenFader`
5. Add the **ScreenFader** script to it
6. Drag `FadePanel`'s CanvasGroup → `Fade Canvas Group` field
7. Set `Default Fade Duration` = 0.5

### 4. Set Up the Scene Loader in MainScene

1. Open `MainScene.unity`
2. Create an **Empty GameObject** at the stable entrance area, named `StableEntrance_Trigger`
3. Add a **BoxCollider2D** component, check **Is Trigger**
4. Size the collider to be a doorway (~2×1 units)
5. Add the **SceneLoader** script
   - Target Scene Name: `StableScene`
   - Fade Duration: 0.5
6. Also set up the **ScreenFader** in MainScene (see step 3 above)

### 5. Build the StableScene Tilemap Layout

1. Open `StableScene.unity`
2. Create **2D Object → Tilemap → Rectangular** named `Ground`
   - Sorting Layer: `ground`
   - Paint the corral area, stable interior, stalls, paths using existing sand/dirt tiles
3. Create another Tilemap named `Obstacles`
   - Sorting Layer: `Buildings`
   - Add a **TilemapCollider2D** component
   - Add a **CompositeCollider2D** component (adds Rigidbody2D automatically)
   - Set Rigidbody2D Body Type = Static
   - On TilemapCollider2D, check **Used by Composite**
   - Paint hay bales, fences, stall walls as obstacle tiles

4. For individual obstacles (hay bales, crates):
   - Create Sprite GameObjects
   - Add **BoxCollider2D** components (NOT triggers, these are solid)
   - Place them around the stable interior

### 6. Set Up the Player in StableScene

1. Copy your Player setup from MainScene (or use a Player prefab)
   - Must have: PlayerMovement, PlayerHealth, Revolver, PlayerShooting, Animator, Rigidbody2D, SpriteRenderer
   - Tag: `Player`
2. Add the **LassoThrow** script to the Player
   - Lasso Projectile Prefab: (create in step 7)
   - Throw Cooldown: 0.8
   - Spawn Offset: 0.5
3. Set up the Camera with **CameraFollow** targeting the Player

### 7. Create the Lasso Projectile Prefab

1. Create an **Empty GameObject** named `LassoProjectile`
2. Add a **SpriteRenderer** (use a rope/lasso sprite or placeholder)
   - Sorting Layer: `player`
3. Add a **Rigidbody2D** — Gravity Scale: 0, Body Type: Kinematic
4. Add a **CircleCollider2D** — Is Trigger: ✓, Radius: 0.3
5. Add the **LassoProjectile** script
   - Speed: 14
   - Max Range: 12
6. Set the GameObject's Layer to `Lasso` (Layer 7)
7. **Prefabs → StableLevel → Drag to create prefab**
8. Delete from scene
9. Assign this prefab to the Player's LassoThrow component → Lasso Projectile Prefab

### 8. Create the Ranch Hand Enemy Prefab

1. Create an **Empty GameObject** named `RanchHandEnemy`
2. Add a **SpriteRenderer** (use a cowboy/ranch hand sprite)
   - Sorting Layer: `player`
3. Add a **Rigidbody2D** — Gravity Scale: 0, Freeze Rotation Z: ✓
4. Add a **BoxCollider2D** — Is Trigger: ✗ (solid)
5. Add an **Animator** (optional — can use GhostEnemy's controller or create a simple one)
6. Set Tag: `Enemy`
7. Set Layer: `Enemy` (Layer 6)
8. Add the **RanchHandEnemy** script:
   - Move Speed: 2
   - Attack Range: 0.9
   - Chase Range: 10
   - Attack Damage: 12
   - Attack Cooldown: 1.2
   - Max Health: 3
   - XP On Death: 3
   - XP Pickup Prefab: (assign existing XPOrb prefab)
9. **Prefabs → StableLevel → Drag to create prefab**
10. Delete from scene

### 9. Set Up Phase Root Objects

Create empty GameObjects as containers for each phase:

```
StableScene Hierarchy:
├── Player
├── Main Camera
├── FadeCanvas
│   └── FadePanel (with CanvasGroup)
├── ScreenFader
├── StableLevelManager
├── --- Phase Roots ---
├── Phase1_EnterStable        ← Contains entrance area, trigger to advance
│   ├── StableGates (sprite)
│   ├── EntranceTrigger (BoxCollider2D, IsTrigger)
│   └── ...
├── Phase2_SearchStable       ← Contains lasso pickup
│   ├── StableInterior (sprites/tiles)
│   ├── LassoPickup (with CircleCollider2D)
│   └── PickupPromptCanvas
│       └── PromptText (TMP)
├── Phase3_FightWaves         ← Contains wave spawner + gate
│   ├── WaveSpawner
│   ├── SpawnPoint_1 (empty transforms)
│   ├── SpawnPoint_2
│   ├── GateBlocker (sprite + BoxCollider2D)
│   └── ...
├── Phase4_ChaseRider         ← Contains rider + waypoints
│   ├── PhantomRider
│   ├── Waypoint_1
│   ├── Waypoint_2
│   ├── ... (8-12 waypoints in an oval)
│   └── OvalTrack (visual)
├── Phase5_QuickDraw          ← Contains duel UI
│   ├── StableDuel
│   └── DuelCanvas
│       ├── DuelOverlay (CanvasGroup)
│       └── DuelStatusText (TMP)
├── Phase6_Reward             ← Contains relic + message
│   ├── StableReward
│   ├── RelicPickup (sprite + CircleCollider2D, IsTrigger)
│   └── RewardCanvas
│       └── RewardText (TMP)
└── UI Canvas (Health, Ammo, XP, Coins)
```

### 10. Configure the StableLevelManager

1. Create an **Empty GameObject** named `StableLevelManager`
2. Add the **StableLevelManager** script
3. In the Inspector, assign:
   - **Phase Roots** array (6 slots):
     - [0] = Phase1_EnterStable
     - [1] = Phase2_SearchStable
     - [2] = Phase3_FightWaves
     - [3] = Phase4_ChaseRider
     - [4] = Phase5_QuickDraw
     - [5] = Phase6_Reward
   - **Lasso Pickup**: The LassoPickup component in Phase2
   - **Wave Spawner**: The WaveSpawner component in Phase3
   - **Phantom Rider**: The PhantomRider component in Phase4
   - **Lasso Throw**: The LassoThrow component on the Player
   - **Player Health**: The PlayerHealth component on the Player
   - **Stable Duel**: The StableDuel component in Phase5
   - **Stable Reward**: The StableReward component in Phase6

### 11. Configure Phase 1 — Enter the Stable

1. Inside `Phase1_EnterStable`, create a trigger zone (BoxCollider2D, Is Trigger)
2. Add a small script or use the StableLevelManager's OnEnterStable event to advance to Phase 2 when the player enters
3. Alternative: Add a second SceneLoader-style trigger that calls `StableLevelManager.AdvancePhase()`

### 12. Configure Phase 2 — Lasso Pickup

1. Inside `Phase2_SearchStable`:
2. Create `LassoPickup` GameObject:
   - Add SpriteRenderer (lasso/rope sprite)
   - Add CircleCollider2D, Is Trigger: ✓
   - Add LassoPickup script
   - Assign Prompt Text (create a WorldSpace Canvas child with TMP text "[E] Pick up Lasso")
   - Assign Level Manager reference
   - Prompt Range: 1.5

### 13. Configure Phase 3 — Wave Spawner

1. Inside `Phase3_FightWaves`:
2. Create empty `SpawnPoint` transforms at positions around the arena
3. Create `GateBlocker` — a sprite with BoxCollider2D blocking the exit
4. Create `WaveSpawner` GameObject:
   - Add WaveSpawner script
   - Configure Waves array:
     - Wave 1: 3× RanchHandEnemy prefab, 3 spawn positions
     - Wave 2: 5× RanchHandEnemy prefab, 5 spawn positions
     - Wave 3: 4× RanchHandEnemy + 2× GhostEnemy prefab, 6 spawn positions
   - Gate Object: GateBlocker
   - Level Manager: StableLevelManager

### 14. Configure Phase 4 — Chase the Phantom Rider

1. Inside `Phase4_ChaseRider`:
2. Create 8-12 **Empty GameObjects** as waypoints arranged in an oval
   - Name them `Waypoint_0` through `Waypoint_11`
   - Space them evenly in an oval track (~15×10 units)
3. Create `PhantomRider` GameObject:
   - Add SpriteRenderer (horse+rider sprite), Sorting Layer: `player`
   - Add BoxCollider2D (size to match sprite), Is Trigger: ✗
   - Set Tag: `Enemy`, Layer: `Enemy`
   - Add Animator (see Animator setup below)
   - Add PhantomRider script:
     - Waypoints: Drag all waypoint transforms in order
     - Base Speed: 4
     - Speed Increase Per Hit: 0.8
     - Hits Required: 3
     - Level Manager: StableLevelManager

### 15. Configure Phase 5 — Quick Draw Duel

1. Inside `Phase5_QuickDraw`:
2. Create a **Canvas** (Screen Space - Overlay) named `DuelCanvas`
3. Add child **Image** named `DuelOverlay`
   - Color: Semi-transparent black (0, 0, 0, 0.7)
   - Rect: Stretch to fill
   - Add **CanvasGroup** component, Alpha: 0
4. Add child **TextMeshPro - Text** named `DuelStatusText`
   - Font Size: 64
   - Alignment: Center
   - Anchor: Center of screen
5. Create `StableDuel` GameObject:
   - Add StableDuel script:
     - Duel Overlay: DuelOverlay's CanvasGroup
     - Duel Status Text: DuelStatusText
     - Gunshot Clip: (assign existing gunshot AudioClip from the revolver)
     - Min Draw Delay: 1
     - Max Draw Delay: 3
     - Reaction Window: 0.5
     - Level Manager: StableLevelManager
   - Hook up **On Player Won** event → StableLevelManager.AdvancePhase (already done in code)
   - Hook up **On Player Lost** event → (optional: restart duel, show retry UI)

### 16. Configure Phase 6 — Reward

1. Inside `Phase6_Reward`:
2. Create `RelicPickup` GameObject:
   - Add SpriteRenderer (glowing relic sprite)
   - Add CircleCollider2D, Is Trigger: ✓
   - Start INACTIVE (the script activates it)
3. Create a UI Canvas with a TMP Text for the reward message
4. Add **StableReward** script to the RelicPickup (or a separate object):
   - Relic Pickup Object: RelicPickup
   - Relic Spawn Position: (set world position)
   - Speed Multiplier: 1.5
   - Reward Message Text: The TMP text element
   - Main Hub Scene Name: `MainScene`
   - Message Display Time: 3

---

## PhantomRider Animator Setup

### States
1. **Idle** — Default state, rider sitting on horse
2. **Gallop** — Rider moving along track (looping animation)
3. **Hit** — Flash/stagger when hit by lasso
4. **Dismount** — Rider falls off horse

### Parameters
| Name | Type | Description |
|------|------|-------------|
| IsMoving | Bool | True when rider is active on track |
| IsHit | Trigger | Fired on lasso hit |
| IsDismounted | Trigger | Fired when rider is pulled off |

### Transitions
| From | To | Condition |
|------|-----|-----------|
| Idle | Gallop | IsMoving = true |
| Gallop | Idle | IsMoving = false |
| Gallop | Hit | IsHit trigger |
| Hit | Gallop | (Auto, after Hit clip ends) |
| Any State | Dismount | IsDismounted trigger |

### Setup Steps
1. **Window → Animation → Animator**
2. Create a new Animator Controller: `Assets/Animations/PhantomRider/PhantomRiderController.controller`
3. Create animation clips:
   - `PhantomRider_Idle.anim` — Static pose
   - `PhantomRider_Gallop.anim` — Looping gallop cycle (mark as Loop)
   - `PhantomRider_Hit.anim` — Quick flash/stagger (0.2s, NOT looping)
   - `PhantomRider_Dismount.anim` — Fall animation (NOT looping)
4. Add states and transitions as described above
5. Assign the controller to the PhantomRider's Animator component

---

## How to Extend QuickDrawDuel.cs for Other Levels

The `QuickDrawDuel` abstract base class is designed so each team member can create their own duel variant. Here's how:

### Step 1: Create a New Script

```csharp
using UnityEngine;
using TMPro;

/// <summary>
/// Custom duel for [YourLevel]. Extends QuickDrawDuel base class.
/// </summary>
public class YourLevelDuel : QuickDrawDuel
{
    // Add your custom serialized fields here
    [SerializeField] private float customTiming = 1.0f;

    // Public method to start your duel from your level manager
    public void BeginDuel()
    {
        StartDuel(); // This freezes the player and shows the overlay
    }

    protected override void OnDuelStart()
    {
        // Set up your duel's initial state
        // Start coroutines, show instructions, etc.
    }

    protected override void OnPlayerShoot()
    {
        // Handle what happens when the player clicks/shoots
        PlayGunshot(); // Plays the gunshot SFX
    }

    protected override void OnEnemyShoot()
    {
        // Handle what happens when the enemy shoots
        PlayGunshot();
    }

    protected override void OnDuelEnd(bool playerWon)
    {
        // Clean up, show results, advance your level
        // The base class already fires OnPlayerWon/OnPlayerLost events
    }
}
```

### Step 2: Key Methods You Can Use

| Method | Description |
|--------|-------------|
| `StartDuel()` | Freezes player, shows overlay. Call from your public trigger method. |
| `EndDuel(bool playerWon)` | Unfreezes player, fires win/lose events. Call when duel resolves. |
| `PlayGunshot()` | Plays the assigned gunshot AudioClip. |
| `HideOverlayAfterDelay(float)` | Coroutine that fades out the overlay. |

### Step 3: Available Fields

| Field | Type | Description |
|-------|------|-------------|
| `duelOverlay` | CanvasGroup | The dark overlay shown during duel |
| `duelStatusText` | TMP_Text | Text element for status messages |
| `gunshotClip` | AudioClip | Sound effect for gunshots |
| `audioSource` | AudioSource | Auto-created AudioSource |
| `playerMovement` | PlayerMovement | Reference to player (set by StartDuel) |
| `duelActive` | bool | Whether the duel is currently in progress |

### Step 4: Hook Up Events

In the Unity Inspector, you can assign callbacks to:
- **On Player Won** — Called when player wins (e.g., advance phase, spawn reward)
- **On Player Lost** — Called when player loses (e.g., retry, show death screen)

### Example Variants

- **Rapid Fire Duel**: Player must shoot 5 targets in sequence within a time limit
- **Moving Target Duel**: A target moves left/right; player must time their shot
- **Showdown Duel**: Both player and enemy take turns shooting, health-bar style
- **Card Draw Duel**: UI shows playing cards; player must click the ace

---

## Testing Checklist

- [ ] Player spawns in StableScene at the entrance
- [ ] Phase 1 → Phase 2 transition works when player reaches stable interior
- [ ] Lasso pickup shows "[E] Pick up Lasso" within 1.5 units
- [ ] Pressing E collects lasso and triggers Phase 3
- [ ] Enemy waves spawn at configured positions
- [ ] Bullets from revolver damage RanchHandEnemy
- [ ] Gate opens when all waves are cleared
- [ ] PhantomRider moves along waypoints continuously
- [ ] Pressing F throws lasso projectile toward mouse
- [ ] Lasso only collides with Enemy layer objects
- [ ] 3 lasso hits dismount the rider (with speed increase + red flash)
- [ ] Player damage resets hit counter during chase
- [ ] Quick draw duel freezes player movement
- [ ] "DRAW!" appears after 1-3 second random delay
- [ ] Clicking within 0.5s = win, too late = "TOO SLOW!", too early = "TOO EARLY!"
- [ ] Winning duel triggers Phase 6
- [ ] Relic pickup spawns and is collectible
- [ ] Speed is permanently increased by 1.5x
- [ ] "StableRelicCollected" is saved to PlayerPrefs
- [ ] UI message shows for 3 seconds then loads MainScene
- [ ] All phase transitions have 0.5s black screen fade
- [ ] All UI uses TextMeshPro (TMP_Text)
- [ ] All obstacles use BoxCollider2D
