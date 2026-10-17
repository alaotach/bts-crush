# BTS Crush

A complete BTS-themed match-3 puzzle game built with React Native, Expo, React Native Skia, and Reanimated. Designed for Android-first, Google Play Store ready.

> **Disclaimer:** This game is a fan-made project. It is NOT officially affiliated with BTS, BIGHIT MUSIC, HYBE, or any rights holder. All copyrighted material (music, artwork, names) is abstracted via a configuration layer and must be properly licensed before public distribution.

---

## Technology Stack

| Layer | Technology |
|---|---|
| Framework | Expo SDK 57 |
| Language | TypeScript (strict) |
| Rendering | React Native Skia |
| Animations | React Native Reanimated |
| Audio | Expo AV |
| Haptics | Expo Haptics |
| Gestures | React Native Gesture Handler |
| Persistence | AsyncStorage |
| Build | EAS Build (Android) |

---

## Feature Inventory

### 1. Core Match-3 Engine
- Board sizes: 6×6, 7×7, 8×8, custom irregular
- Cell types: normal, holes, blocked, empty, ice, jelly, crate, chain, lock
- Match detection: horizontal, vertical, T-shape, L-shape, 2×2, 5+, 6+
- Overlapping and simultaneous matches
- Chain reactions and cascades
- Gravity system with falling tiles
- New tile spawning and refill
- Dead-board detection and automatic shuffle
- Game state machine (IDLE → SWAPPING → MATCHING → CLEARING → FALLING → SPAWNING → CASCADE → OBJECTIVE_CHECK)

### 2. BTS Member System
Seven member tile types: RM, Jin, SUGA, j-hope, Jimin, V, Jung Kook

Each member has:
- Normal portrait, selected state, matched state, special state
- Celebration and failure reactions
- Idle animation, glow/highlight
- Member-specific frame/accent color
- Sound/voice hook (licensable)
- Fully configurable via asset definitions

### 3. Special Tiles

| Match | Special | Effect |
|---|---|---|
| Match 4 | **Mic Candy** | Clears entire row or column with music-note beam |
| Match 4 | **Lightstick Candy** | Concert light beam sweep |
| Match 5 | **Album Bomb** | Color bomb — clears all tiles of one member type |
| Cross/X | **Stage Bomb** | X-shaped destruction pattern |
| 2×2 | **Chibi Member** | Character pops out, 3×3 or 5×5 area explosion |
| T/L shape | **Fan Heart Bomb** | Expanding heart-shaped area explosion |
| Match 6+ | **Dynamite Candy** | Clears entire row + entire column (cross beam) |
| Special | **Butter Slide** | Moving special that clears tiles in its path |

### 4. Special Combinations
Every special × special pair has a unique interaction:
- Mic + Mic → double line clear
- Mic + Lightstick → cross-shaped beam
- Mic + Album Bomb → multiple rows become Mics
- Album + Album → massive board clear
- Dynamite + Dynamite → board-clearing explosion
- Butter + anything → special movement interactions
- ...and all other combinations

### 5. Combo & Cascade System
- Cascade counter and combo multiplier
- Escalating sound, animation, and haptic feedback
- Floating score numbers with pop animation
- Combo UI: GOOD! → GREAT! → AMAZING! → ENCORE!

### 6. Score System
- Tiered scoring: 3-match (+30), 4-match (+60), 5-match (+100), 6+ (+150)
- Special creation and activation bonuses
- Cascade and combo multipliers
- Remaining-move bonus
- High score tracking

### 7. Objectives
- Score target
- Collect member tiles (single or multiple types)
- Clear specific cells (jelly, ice, crates, chains, locked cells)
- Collect keys to unlock areas
- Bring objects to board bottom
- Activate stage lights on specific cells
- Create N specials, activate N specials
- Perform N combos or cascades
- Timed levels, limited moves, multi-objective levels

### 8. Obstacles
| Obstacle | Behavior |
|---|---|
| Jelly | Cleared by adjacent match |
| Ice / Double Ice | Layered, needs multiple matches |
| Crate / Double Crate | Destroyed by adjacent match, layered |
| Chain | Requires adjacent clearing |
| Locked Cell | Unlock via key collection |
| Countdown Bomb | Decreases each move, level fail on zero |
| Chocolate Blocker | Spreads to nearby cells |
| Frozen Tile | Needs multiple matches to thaw |
| Portal | Teleports tiles |
| Conveyor | Moves tiles in a direction |

### 9. Level System
- Data-driven level definitions (no hardcoded level logic)
- LevelDefinition: id, world, rows, cols, layout, moves, timeLimit, objectives, obstacles, difficulty, rewards
- 15+ level types (score, collect, jelly, ice, crate, chain, keys, bring-down, create specials, combo challenge, timed, multi-objective, boss)
- Designed difficulty curve with gradual mechanic introduction
- World/chapter progression with BTS-inspired themes

### 10. Progression
- Level map with worlds, paths, nodes, locked/completed states
- Star system (1-3 stars per level based on performance)
- Rewards: stars, points, character cards, cosmetic frames, collectibles
- BTS member collection screen with stats and unlockables

### 11. UI Screens
- Splash → Home → Level Map → Level Intro → Gameplay → Pause → Level Complete → Level Failed
- Collection → Character Details → Achievements → Settings → About
- Event screen architecture

### 12. Gameplay HUD
- Level number, objectives with icons, moves remaining, score
- Combo counter, progress bars, objective completion animation

### 13. Animation Engine
- Dedicated AnimationManager with systems for: swap, match, destroy, fall, spawn, special create, special activate, combo, cascade, level complete, level fail
- Particle system: sparks, stars, hearts, music notes, light beams, confetti, smoke, glow, burst, fireworks, stage lights
- Screen effects: shake, flash, vignette, glow, zoom, slow-motion

### 14. Audio Engine
- AudioManager with playSFX(), playMusic(), stopMusic(), volume controls
- Sound categories: tap, swap, match, special creation/activation, explosion, combo, cascade, objective, victory, failure, unlock, reward, menu
- Dynamic music intensity reacting to gameplay
- MusicManager supporting menu, gameplay, special level, victory, defeat tracks
- Asset abstraction for future licensed music

### 15. Haptic Engine
- HapticManager with typed feedback: lightTap, mediumTap, heavyImpact, specialActivation, combo, victory
- Settings toggle for reduced haptics

### 16. Tutorial System
- Contextual tutorials (not front-loaded dumps)
- Progressive introduction: basic matching → specials → combinations → obstacles

### 17. Character Reactions
- Idle: subtle movement
- Match: happy reaction
- Special: excited
- Huge combo: surprised/excited
- Victory: celebration
- Failure: sad/disappointed

### 18. Easter Egg System
- Extensible EasterEggManager
- Fan Chant: ARMY crowd clears nearby tiles
- SUGA's Rap: rhythmic wave through board
- RM Inspire: converts random tiles to specials

### 19. BTS-Themed Events Architecture
- Member Spotlight, 7 Day Challenge, Encore, Special Stage, Fan Challenge
- Event definitions: start/end, levels, objectives, rewards, visuals, audio

### 20. Save System
- Persist: currentLevel, unlockedLevels, stars, highScores, collection, settings, eventProgress, achievements
- AsyncStorage-backed

### 21. Achievement System
- First Match, First Special, First Combo, Combo Master, Specialist, 5-Star Fan, Perfect Level, milestone achievements

### 22. Statistics
- gamesPlayed, levelsCompleted, totalMatches, totalCombos, highestCombo, highestScore, specialsCreated, specialCombos, memberUsage

### 23. Daily Challenge Architecture
- Unique board, special objective, daily score, reward (offline/local)

### 24. Settings
- Music, Sound Effects, Haptics, Reduced Motion, Notifications toggles

### 25. Debug Tools
- Hidden debug panel: set level, add/remove moves, spawn specials, trigger combos/cascades/victory/failure, shuffle board

### 26. Performance
- Texture caching, image preloading via Skia
- Limited particle count, animation cleanup
- Memoized components, optimized board updates
- Target: smooth 60 FPS on normal Android hardware

### 27. Play Store Readiness
- App icon, splash screen, adaptive icon
- Package name, versioning, release build (Android App Bundle)
- Permissions audit, privacy policy, about screen
- Crash handling, proper back-button behavior

---

## Project Architecture

```
src/
├── game/                    # Pure TypeScript game engine (zero UI deps)
│   ├── GameEngine.ts
│   ├── GameState.ts
│   ├── GamePhase.ts
│   ├── Board.ts
│   ├── Tile.ts
│   ├── MatchDetector.ts
│   ├── MoveResolver.ts
│   ├── GravitySystem.ts
│   ├── SpawnSystem.ts
│   ├── SpecialSystem.ts
│   ├── CombinationSystem.ts
│   ├── ComboSystem.ts
│   ├── ScoreSystem.ts
│   ├── ObjectiveSystem.ts
│   ├── ObstacleSystem.ts
│   └── ShuffleSystem.ts
├── levels/
│   ├── LevelDefinition.ts
│   ├── LevelLoader.ts
│   ├── world1.ts
│   ├── world2.ts
│   └── ...
├── rendering/
│   ├── GameCanvas.tsx
│   ├── TileRenderer.tsx
│   ├── BoardRenderer.tsx
│   ├── SpecialRenderer.tsx
│   ├── ParticleRenderer.tsx
│   └── effects/
├── animation/
│   ├── AnimationManager.ts
│   ├── TileAnimations.ts
│   ├── SpecialAnimations.ts
│   ├── ComboAnimations.ts
│   └── ScreenEffects.ts
├── audio/
│   ├── AudioManager.ts
│   ├── MusicManager.ts
│   └── SFXManager.ts
├── haptics/
│   └── HapticManager.ts
├── bts/
│   ├── members.ts
│   ├── assets.ts
│   └── reactions.ts
├── progression/
│   ├── LevelProgress.ts
│   ├── Stars.ts
│   ├── Rewards.ts
│   └── Achievements.ts
├── screens/
│   ├── HomeScreen.tsx
│   ├── MapScreen.tsx
│   ├── GameScreen.tsx
│   ├── CollectionScreen.tsx
│   ├── SettingsScreen.tsx
│   ├── ResultScreen.tsx
│   └── ...
├── storage/
│   └── SaveManager.ts
├── debug/
│   └── DebugPanel.tsx
└── components/
    └── ...
```

---

## Milestones

| # | Milestone | Status |
|---|---|---|
| 1 | Audit existing code, create FEATURE_MATRIX.md | 🔴 |
| 2 | Refactor game architecture (engine, state machine, board) | 🔴 |
| 3 | Complete board engine (all match types, gravity, shuffle, dead-board) | 🔴 |
| 4 | All basic special tiles (Mic, Lightstick, Album, Stage, Chibi, Heart, Dynamite, Butter) | 🔴 |
| 5 | Special combinations (every pair interaction) | 🔴 |
| 6 | Obstacles and objective framework | 🔴 |
| 7 | Animation/effects system (particles, screen effects, Skia rendering) | 🔴 |
| 8 | Audio/haptics engine | 🔴 |
| 9 | Progression, level map, worlds, stars | 🔴 |
| 10 | Collection, achievements, events, daily challenges | 🔴 |
| 11 | Performance and testing | 🔴 |
| 12 | Play Store production build | 🔴 |

---

## Assets

Member portraits are in `assets/members/`. The game engine uses abstract member IDs — the rendering layer maps IDs to assets. This allows artwork to be replaced without changing game logic.

## Legal

- Do NOT embed copyrighted BTS music
- Do NOT copy official promotional artwork
- Audio system uses placeholder/royalty-free audio during development
- All BTS-related assets are abstracted via configuration for easy swap
- Rights/licensing must be resolved before public distribution
