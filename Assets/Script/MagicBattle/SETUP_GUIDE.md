# Magician's Day Off — "Magic Battle" scene setup guide

`Assets/Scenes/MagicBattle.unity` is **fully built**: every GameObject, every script component,
every cross-reference between scripts, the mirror's Quad + material + render-texture camera,
placeholder materials, and a self-building HUD are already in the scene file. You do not need
to create any GameObjects, components, Canvases, Quads, Cameras, or Materials by hand.

Your job is only to:

1. Install a couple of packages / add Meta Building Blocks (one-time, a few clicks).
2. **Drag your own assets** (models, materials, audio clips) onto the fields listed in the
   table in Step 3 — or just leave them empty and the game runs anyway with simple placeholder
   shapes and silence.

That's it. Everything else (mirror shattering, wall breaking, rhythm notes, HUD, scoring) is
driven entirely by code that is already wired up.

---

## 1. Packages to install

The project already has `com.meta.xr.sdk.core` and `com.unity.ugui` (which bundles TextMeshPro
in Unity 6). You only need to do the following, once:

1. **Window > TextMeshPro > Import TMP Essential Resources** → click **Import**.
   This is the one unavoidable one-click step: without it, TMP text components render blank.
2. Open **Meta > Tools > Building Blocks** (or **Oculus > Tools > Building Blocks** on older SDK
   versions) and add these blocks to the `MagicBattle` scene:
   - **Camera Rig** → adds `OVRCameraRig` (`CenterEyeAnchor`, `LeftHandAnchor`,
     `RightHandAnchor`, `OVRManager`).
   - **Passthrough** → adds `OVRPassthroughLayer`, makes the camera background transparent so
     the real room shows through.
   - **Hand Tracking** → adds `OVRHand` + `OVRSkeleton` to both hand anchors.
   - *(Optional, recommended)* **Room Mesh** → installs MR Utility Kit (MRUK) for automatic
     real-wall detection instead of manually placing the wall marker. See Step 5.
3. Run **Meta > Tools > Project Setup Tool** → **Android** tab → **Fix All** (configures
   Quest-required Player Settings: IL2CPP, ARM64, Passthrough capability, Hand Tracking, etc.)

That's the entire package/Building-Blocks list. No other packages are required.

---

## 2. What the scene already looks like

Opening `Assets/Scenes/MagicBattle.unity`, the Hierarchy contains (all pre-wired):

```
MagicBattle_GameManager        — GameManager: orchestrates the whole sequence
MagicBattle_WallSystem
    WallMarker                 — marks where the real wall is (drag this to match your room)
    WallLocator                 (ManualWallLocator)
    WallCrackWarning             — plays warning sfx/visual before the wall can be punched
    WallBreakController           — detects the fist-punch gesture
    WallBreakEffect                — spawns hole frame + debris + reveals the forest
MagicBattle_MirrorSystem        (MirrorBreakSequence, MirrorReflectionCamera)
    MirrorPlane                 — Quad (auto-built mesh) + MIrrorMat already assigned
    MirrorCamera                 — renders into Mirror.renderTexture already assigned
MagicBattle_Cat                 (CatController, CatPlaceholderBuilder) — inactive until mirror breaks
MagicBattle_CatForestAnchor
MagicBattle_PlayerAnchor
MagicBattle_Forest              (ForestEnvironmentBuilder) — inactive until wall breaks
MagicBattle_RhythmSystem
    SpawnPoint
    HitZonePoint                 (SphereCollider + HitZone)
MagicBattle_HUD                 (HUDController) — builds its own Canvas/Text/Panels at runtime
```

After you add the Building Blocks (Step 1), you'll also have an `OVRCameraRig` at the Hierarchy
root with `CenterEyeAnchor`, `LeftHandAnchor`, `RightHandAnchor` underneath it — these are the
only new objects you need to drag into fields (see the table below), because they don't exist
until the Building Blocks are added.

**Placeholder visuals**: the cat, the forest (trees/ground), and the rhythm note orbs are all
generated automatically at runtime by small "builder" scripts
(`CatPlaceholderBuilder`, `ForestEnvironmentBuilder`) using simple primitive shapes, so the game
is fully playable with zero art. Replace them with real models/materials whenever you like using
the table below — no code or hierarchy changes required either way.

---

## 3. Drag-and-drop reference — what goes where

Do these after adding the Building Blocks (Step 1). Every row is optional *except* the four
marked **(required)**; skipping an optional row just means that feature keeps using its
automatic placeholder/silence instead of your asset.

| Select this object | Component | Field | Drag this | Required? |
|---|---|---|---|---|
| `MagicBattle_MirrorSystem` | Mirror Reflection Camera | **Player Camera** | `OVRCameraRig/.../CenterEyeAnchor`'s `Camera` component | **Required** |
| `OVRCameraRig/.../LeftHandAnchor` | *(Add Component)* **Hand Gesture Recognizer** | — | just add the component, no fields to assign | **Required** |
| `OVRCameraRig/.../RightHandAnchor` | *(Add Component)* **Hand Gesture Recognizer** | — | just add the component, no fields to assign | **Required** |
| `MagicBattle_WallSystem/WallBreakController` | Wall Break Controller | **Left Hand** / **Right Hand** | `LeftHandAnchor` / `RightHandAnchor` | **Required** |
| `MagicBattle_RhythmSystem` | Rhythm Game Manager | **Left Hand** / **Right Hand** | `LeftHandAnchor` / `RightHandAnchor` | **Required** |
| `MagicBattle_WallSystem/WallLocator` | Manual Wall Locator | **Player Head** | `CenterEyeAnchor` transform | optional (only used by the calibration shortcut, Step 5) |
| `MagicBattle_MirrorSystem` | Mirror Break Sequence | **Crack Clip** / **Shatter Clip** | your own crack/shatter sound effects | optional (silent otherwise) |
| `MagicBattle_WallSystem/WallCrackWarning` | Wall Crack Warning | **Crack Loop Clip** | a looping creak/rumble sound | optional |
| `MagicBattle_WallSystem/WallBreakEffect` | Wall Break Effect | **Break Clip** | a crash/rubble sound | optional |
| `MagicBattle_WallSystem/WallBreakEffect` | Wall Break Effect | **Frame Material** / **Debris Material** | already set to simple grey placeholder materials in `Assets/Generated/MagicBattle/` — drag your own in to replace them | optional |
| `MagicBattle_WallSystem/WallBreakEffect` | Wall Break Effect | **Dust Particles** | a `ParticleSystem` prefab/instance, if you build one | optional |
| `MagicBattle_Cat` | *(Hierarchy)* — drag as a **child** | — | your own cat model/prefab, positioned at local `(0,0,0)` | optional (see note below) |
| `MagicBattle_Cat` (or a child of it) | *(Hierarchy)* — drag as a **child** | — | the witch hat prop, `Assets/Models/the-witchs-hat/source/Unity2Skfb.fbx` | optional, purely cosmetic |
| `MagicBattle_Forest` | Forest Environment Builder | **Ground/Trunk/Foliage Material** | your own nature materials | optional |
| `MagicBattle_RhythmSystem` | Rhythm Game Manager | **Note Orb Prefab** | see Step 4 — generate once, or replace with your own colored-ball prefab | optional (a default is auto-created, see Step 4) |
| `MagicBattle_HUD` | HUD Controller | **Face Target** | `CenterEyeAnchor` transform | optional (keeps the auto-built HUD facing the player) |

> **Note on `CatPlaceholderBuilder`**: if the field name above doesn't exactly match what you
> see in the Inspector, that's fine — the script only builds a placeholder capsule+sphere cat
> if no children/renderers are present. The simplest way to use a real model: drag your cat
> model as a **child** of `MagicBattle_Cat` in the Hierarchy and position it at local
> `(0, 0, 0)`; the placeholder builder detects the existing geometry and skips building its own.
> Same technique works for the forest (drop tree/rock models as children of `MagicBattle_Forest`).

---

## 4. Rhythm game content (one click each, optional)

The rhythm system works out of the box with a generated beat map and sphere "note orb", but you
can swap in your own:

1. **Tools > Magic Battle > Create Default Beat Map** — creates
   `Assets/Script/MagicBattle/Rhythm/DefaultBeatMap.asset` (40 notes, ramping difficulty, ~70s).
2. **Tools > Magic Battle > Create Note Orb Prefab** — creates
   `Assets/Script/MagicBattle/Rhythm/NoteOrb.prefab` (a simple colored sphere).
3. Select `MagicBattle_RhythmSystem` → Rhythm Game Manager, and drag these two assets onto
   **Beat Map** and **Note Orb Prefab** if you want to replace the runtime defaults with fixed,
   editable assets.

> To make your own song: duplicate `DefaultBeatMap.asset` (or **Assets > Create > Magic Battle >
> Beat Map**) and edit the `Notes` array (time in seconds + required gesture) in the Inspector.

---

## 5. Position the real wall behind the player

The wall-break mechanic needs to know where your **real** living-room wall is, so the hole and
forest line up with it.

- **Manual (default, already works)**: select `MagicBattle_WallSystem/WallMarker` and move/
  rotate it in the Scene view (or Transform fields) to match your real wall — its blue (Z) axis
  must point back into the room, towards the player. Default placement is 2.2 m behind the
  origin; adjust to your room.
- **On-device shortcut**: with the headset connected (Quest Link) and the game in Play Mode,
  face your real wall, select `WallLocator`, and use the Inspector's gear icon →
  **Calibrate In Front Of Player**. This snaps `WallMarker` to 1 m in front of wherever you're
  currently facing.
- **Automatic (optional upgrade)**: if you installed the **Room Mesh** Building Block (MRUK) in
  Step 1, add the scripting define `MAGICBATTLE_MRUK` (**Project Settings > Player > Other
  Settings > Scripting Define Symbols**) to compile in `MRUKWallLocator`, which reads your
  scanned room's actual wall instead of the manual marker. Swap the `ManualWallLocator`
  component on `WallLocator` for `MRUKWallLocator` and assign it to
  `MagicBattle_GameManager`'s **Wall Locator Component** field (and `WallBreakController`'s /
  `WallBreakEffect`'s references to it) if you do this.

---

## 6. Known placeholders you can replace

Everything below already works with a generated placeholder; replace any of them with real
assets at your own pace by dragging onto the fields from the table in Step 3 — nothing else in
the scene needs to change when you do.

- **Cat model** — currently a capsule+sphere placeholder (`CatPlaceholderBuilder`).
- **Witch hat prop** — not placed; `Assets/Models/the-witchs-hat/` is available to drag in.
- **Forest trees/ground** — procedurally placed cone/cylinder trees (`ForestEnvironmentBuilder`).
- **Wall frame/debris materials** — simple grey URP/Lit materials in
  `Assets/Generated/MagicBattle/`.
- **Rhythm note orbs** — plain colored spheres.
- **All audio** — silent until you drag clips onto the `AudioClip` fields listed in Step 3
  (an `AudioSource` is created automatically by each script the first time it runs).
- **HUD visuals** — a plain world-space Canvas with TMP text and dark panels, built automatically
  by `HUDController` the first time the scene runs (see **Auto Build If Empty** on that
  component — set it to false and assign `Message Text` / `Miss Counter Text` / `Win Panel` /
  `Lose Panel` yourself if you want a custom-designed HUD instead).

---

## 7. Gameplay flow recap

1. Player looks in the mirror (reflection rendered live via `MirrorReflectionCamera`).
2. After `crackDuration` seconds, the mirror cracks, then shatters into physics shards, revealing
   the (initially hidden) cat.
3. Simultaneously, `WallCrackWarning` starts a creak sound behind the player on the real wall.
4. Player makes a **fist** gesture aimed at the wall (`WallBreakController`) → `WallBreakEffect`
   spawns a hole frame, scatters debris with physics, and reveals the forest.
5. `RhythmGameManager` starts spawning colored note orbs that fly from `SpawnPoint` toward
   `HitZonePoint`; the player must perform the matching hand gesture as each orb reaches the hit
   zone.
6. Each miss increments a counter (`GameEvents.OnNoteMissed`) and visually moves the cat closer.
   Missing `maxMisses` (default 20) triggers `GameEvents.OnPlayerLose`; clearing the beat map
   without maxing out misses triggers `GameEvents.OnPlayerWin`.

---

## 8. Final checks before building to device

- `Edit > Project Settings > XR Plug-in Management` → Android tab → **OpenXR** enabled with the
  **Meta Quest Support** feature group ticked.
- Passthrough capability + Hand Tracking enabled (handled by **Project Setup Tool > Fix All**).
- Scene added to **Build Settings** (`File > Build Settings > Add Open Scenes`).
- Play once in the Editor with Quest Link/Air Link connected to confirm the mirror shows a live
  reflection, the wall punch triggers at your real wall location, and notes spawn/fly correctly
  before deploying to the headset.
