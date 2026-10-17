# Feature Matrix — BTS Crush

Audit of the current implementation against the full product specification.

**Legend:**
- ✅ `[IMPLEMENTED]` — feature works end-to-end with actual gameplay
- 🟡 `[PARTIAL]` — placeholder/stub exists but no real gameplay
- ❌ `[MISSING]` — not present at all

---

## 1. Core Board Engine

| # | Feature | Status | File(s) |
|---|---------|--------|---------|
| 1.1 | 6×6 board | 🟡 PARTIAL | gameLogic.ts (hardcoded 6×8) |
| 1.2 | 7×7 board | ❌ MISSING | — |
| 1.3 | 8×8 board | ❌ MISSING | — |
| 1.4 | Custom irregular boards | ❌ MISSING | — |
| 1.5 | Holes/blocked/empty cells | ❌ MISSING | — |
| 1.6 | Board coordinate system | 🟡 PARTIAL | gameLogic.ts (x,y only) |
| 1.7 | Responsive tile sizing | 🟡 PARTIAL | GameBoard.tsx (width/6) |
| 1.8 | Level-specific layouts | ❌ MISSING | — |

## 2. Basic Tile Mechanics

| # | Feature | Status | File(s) |
|---|---------|--------|---------|
| 2.1 | 7 BTS member tiles | ✅ IMPLEMENTED | btsAssets.ts |
| 2.2 | Random tile generation | ✅ IMPLEMENTED | gameLogic.ts |
| 2.3 | No-initial-match generation | ✅ IMPLEMENTED | gameLogic.ts |
| 2.4 | Valid swap detection | 🟡 PARTIAL | GameBoard.tsx (only checks post-swap matches) |
| 2.5 | Invalid swap animation | ❌ MISSING | — |
| 2.6 | Swap rollback | 🟡 PARTIAL | GameBoard.tsx (setTimeout, no animation) |
| 2.7 | Horizontal match detection | 🟡 PARTIAL | gameLogic.ts (basic, no pattern classification) |
| 2.8 | Vertical match detection | 🟡 PARTIAL | gameLogic.ts (basic, no pattern classification) |
| 2.9 | T-shape matches | ❌ MISSING | — |
| 2.10 | L-shape matches | ❌ MISSING | — |
| 2.11 | 2×2 matches | ❌ MISSING | — |
| 2.12 | 5+ matches | ❌ MISSING | — |
| 2.13 | 6+ matches | ❌ MISSING | — |
| 2.14 | Multiple simultaneous matches | 🟡 PARTIAL | gameLogic.ts (detects but doesn't classify) |
| 2.15 | Overlapping matches | ❌ MISSING | — |
| 2.16 | Chain reactions / cascades | 🟡 PARTIAL | GameBoard.tsx (while loop, no proper timing) |
| 2.17 | Gravity | 🟡 PARTIAL | gameLogic.ts (basic, instant) |
| 2.18 | Falling tile animation | ❌ MISSING | — |
| 2.19 | New tile spawning | 🟡 PARTIAL | gameLogic.ts (instant, no animation) |
| 2.20 | Dead-board detection | ❌ MISSING | — |
| 2.21 | Automatic shuffle | ❌ MISSING | — |
| 2.22 | Shuffle animation | ❌ MISSING | — |

## 3. BTS Member System

| # | Feature | Status | File(s) |
|---|---------|--------|---------|
| 3.1 | Member data structure | 🟡 PARTIAL | btsAssets.ts (id, name, color, image only) |
| 3.2 | Selected state | ❌ MISSING | — |
| 3.3 | Matched state | ❌ MISSING | — |
| 3.4 | Special state | ❌ MISSING | — |
| 3.5 | Celebration reaction | ❌ MISSING | — |
| 3.6 | Failure reaction | ❌ MISSING | — |
| 3.7 | Idle animation | ❌ MISSING | useTileAnimator.ts exists but unused |
| 3.8 | Glow/highlight | ❌ MISSING | — |
| 3.9 | Member-specific frame | ❌ MISSING | — |
| 3.10 | Sound/voice hook | ❌ MISSING | — |

## 4. Special Tiles

| # | Feature | Status | File(s) |
|---|---------|--------|---------|
| 4.1 | Mic Candy (Match 4 → line clear) | ❌ MISSING | — |
| 4.2 | Lightstick Candy (directional beam) | ❌ MISSING | — |
| 4.3 | Album Bomb (Match 5 → color bomb) | ❌ MISSING | — |
| 4.4 | Stage Bomb (X-shape) | ❌ MISSING | — |
| 4.5 | Chibi Member (2×2 → area) | ❌ MISSING | — |
| 4.6 | Fan Heart Bomb (T/L → expanding heart) | ❌ MISSING | — |
| 4.7 | Dynamite Candy (6+ → row+col) | ❌ MISSING | — |
| 4.8 | Butter Slide (moving special) | ❌ MISSING | — |

## 5. Special Combinations

| # | Feature | Status | File(s) |
|---|---------|--------|---------|
| 5.1 | Mic + Mic | ❌ MISSING | — |
| 5.2 | Mic + Lightstick | ❌ MISSING | — |
| 5.3 | Mic + Album Bomb | ❌ MISSING | — |
| 5.4 | Mic + Chibi | ❌ MISSING | — |
| 5.5 | Mic + Heart | ❌ MISSING | — |
| 5.6 | Mic + Dynamite | ❌ MISSING | — |
| 5.7 | Lightstick + Lightstick | ❌ MISSING | — |
| 5.8 | Lightstick + Album | ❌ MISSING | — |
| 5.9 | Album + Album | ❌ MISSING | — |
| 5.10 | Album + Dynamite | ❌ MISSING | — |
| 5.11 | Chibi + Chibi | ❌ MISSING | — |
| 5.12 | Dynamite + Dynamite | ❌ MISSING | — |
| 5.13 | Butter + anything | ❌ MISSING | — |
| 5.14 | SpecialEffectEngine | ❌ MISSING | — |

## 6. Combo & Cascade

| # | Feature | Status | File(s) |
|---|---------|--------|---------|
| 6.1 | Cascade counter | ❌ MISSING | — |
| 6.2 | Combo multiplier | 🟡 PARTIAL | GameBoard.tsx (basic counter) |
| 6.3 | Cascade timing | ❌ MISSING | — |
| 6.4 | Cascade sound escalation | ❌ MISSING | — |
| 6.5 | Cascade animation escalation | ❌ MISSING | — |
| 6.6 | Combo UI (GOOD/GREAT/AMAZING/ENCORE) | ❌ MISSING | — |

## 7. Score System

| # | Feature | Status | File(s) |
|---|---------|--------|---------|
| 7.1 | Tiered scoring rules | ❌ MISSING | GameBoard.tsx (flat ×10) |
| 7.2 | Special bonuses | ❌ MISSING | — |
| 7.3 | Floating score numbers | ❌ MISSING | — |
| 7.4 | Score pop animation | ❌ MISSING | — |
| 7.5 | Remaining move bonus | ❌ MISSING | — |
| 7.6 | High score tracking | ❌ MISSING | — |

## 8. Objectives

| # | Feature | Status | File(s) |
|---|---------|--------|---------|
| 8.1 | Score target | ❌ MISSING | — |
| 8.2 | Collect member tiles | ❌ MISSING | — |
| 8.3 | Multi-member collection | ❌ MISSING | — |
| 8.4 | Clear specific cells | ❌ MISSING | — |
| 8.5 | Jelly clearing | ❌ MISSING | — |
| 8.6 | Ice breaking | ❌ MISSING | — |
| 8.7 | Crate destruction | ❌ MISSING | — |
| 8.8 | Chain breaking | ❌ MISSING | — |
| 8.9 | Lock/key mechanics | ❌ MISSING | — |
| 8.10 | Bring-down objective | ❌ MISSING | — |
| 8.11 | Stage light activation | ❌ MISSING | — |
| 8.12 | Special creation count | ❌ MISSING | — |
| 8.13 | Special activation count | ❌ MISSING | — |
| 8.14 | Combo count | ❌ MISSING | — |
| 8.15 | Cascade count | ❌ MISSING | — |
| 8.16 | Timed levels | ❌ MISSING | — |
| 8.17 | Limited moves | ❌ MISSING | — |
| 8.18 | Multi-objective levels | ❌ MISSING | — |

## 9. Obstacles

| # | Feature | Status | File(s) |
|---|---------|--------|---------|
| 9.1 | Jelly | ❌ MISSING | — |
| 9.2 | Ice / Double Ice | ❌ MISSING | — |
| 9.3 | Crate / Double Crate | ❌ MISSING | — |
| 9.4 | Chain | ❌ MISSING | — |
| 9.5 | Locked Cell | ❌ MISSING | — |
| 9.6 | Countdown Bomb | ❌ MISSING | — |
| 9.7 | Chocolate Blocker | ❌ MISSING | — |
| 9.8 | Frozen Tile | ❌ MISSING | — |
| 9.9 | Portal | ❌ MISSING | — |
| 9.10 | Conveyor | ❌ MISSING | — |

## 10. Level System

| # | Feature | Status | File(s) |
|---|---------|--------|---------|
| 10.1 | LevelDefinition data structure | ❌ MISSING | — |
| 10.2 | Data-driven level loading | ❌ MISSING | — |
| 10.3 | 15+ level types | ❌ MISSING | — |
| 10.4 | Difficulty curve | ❌ MISSING | — |
| 10.5 | World/chapter system | ❌ MISSING | — |

## 11. Progression

| # | Feature | Status | File(s) |
|---|---------|--------|---------|
| 11.1 | Level map screen | ❌ MISSING | — |
| 11.2 | Star system (1-3 per level) | ❌ MISSING | — |
| 11.3 | Rewards | ❌ MISSING | — |
| 11.4 | BTS member collection | ❌ MISSING | — |

## 12. UI Screens

| # | Feature | Status | File(s) |
|---|---------|--------|---------|
| 12.1 | Splash screen | ❌ MISSING | — |
| 12.2 | Home screen | ❌ MISSING | — |
| 12.3 | Level map | ❌ MISSING | — |
| 12.4 | Level intro | ❌ MISSING | — |
| 12.5 | Gameplay screen | 🟡 PARTIAL | App.tsx + GameBoard.tsx |
| 12.6 | Pause menu | ❌ MISSING | — |
| 12.7 | Level complete | ❌ MISSING | — |
| 12.8 | Level failed | ❌ MISSING | — |
| 12.9 | Collection screen | ❌ MISSING | — |
| 12.10 | Settings screen | ❌ MISSING | — |
| 12.11 | About screen | ❌ MISSING | — |

## 13. Gameplay HUD

| # | Feature | Status | File(s) |
|---|---------|--------|---------|
| 13.1 | Level number | ❌ MISSING | — |
| 13.2 | Objectives display | ❌ MISSING | — |
| 13.3 | Moves remaining | ❌ MISSING | — |
| 13.4 | Score display | 🟡 PARTIAL | GameBoard.tsx (basic text) |
| 13.5 | Combo counter | 🟡 PARTIAL | GameBoard.tsx (basic text) |
| 13.6 | Progress bars | ❌ MISSING | — |

## 14. Animation

| # | Feature | Status | File(s) |
|---|---------|--------|---------|
| 14.1 | AnimationManager | ❌ MISSING | — |
| 14.2 | Tile swap animation | ❌ MISSING | — |
| 14.3 | Tile match animation | ❌ MISSING | — |
| 14.4 | Tile destroy animation | ❌ MISSING | — |
| 14.5 | Tile fall animation | ❌ MISSING | — |
| 14.6 | Tile spawn animation | ❌ MISSING | — |
| 14.7 | Special create animation | ❌ MISSING | — |
| 14.8 | Special activate animation | ❌ MISSING | — |
| 14.9 | Combo animation | ❌ MISSING | — |
| 14.10 | Level complete animation | ❌ MISSING | — |
| 14.11 | Level fail animation | ❌ MISSING | — |

## 15. Particle System

| # | Feature | Status | File(s) |
|---|---------|--------|---------|
| 15.1 | Spark | ❌ MISSING | — |
| 15.2 | Star | ❌ MISSING | — |
| 15.3 | Heart | ❌ MISSING | — |
| 15.4 | Music Note | ❌ MISSING | — |
| 15.5 | Light Beam | ❌ MISSING | — |
| 15.6 | Confetti | ❌ MISSING | — |
| 15.7 | Glow | ❌ MISSING | — |
| 15.8 | Burst | ❌ MISSING | — |

## 16. Screen Effects

| # | Feature | Status | File(s) |
|---|---------|--------|---------|
| 16.1 | Screen shake | ❌ MISSING | — |
| 16.2 | Flash | ❌ MISSING | — |
| 16.3 | Vignette | ❌ MISSING | — |
| 16.4 | Board shake | ❌ MISSING | — |
| 16.5 | Camera zoom | ❌ MISSING | — |
| 16.6 | Slow motion | ❌ MISSING | — |

## 17. Audio

| # | Feature | Status | File(s) |
|---|---------|--------|---------|
| 17.1 | AudioManager | ❌ MISSING | AudioController.ts is empty stub |
| 17.2 | MusicManager | ❌ MISSING | — |
| 17.3 | SFX system | ❌ MISSING | — |
| 17.4 | Dynamic audio intensity | ❌ MISSING | — |
| 17.5 | Volume controls | ❌ MISSING | — |

## 18. Haptics

| # | Feature | Status | File(s) |
|---|---------|--------|---------|
| 18.1 | HapticManager | ❌ MISSING | — |
| 18.2 | Typed haptic feedback | 🟡 PARTIAL | GameBoard.tsx (inline calls) |
| 18.3 | Settings toggle | ❌ MISSING | — |

## 19. Tutorial

| # | Feature | Status | File(s) |
|---|---------|--------|---------|
| 19.1 | Tutorial system | ❌ MISSING | — |
| 19.2 | Contextual hints | ❌ MISSING | — |

## 20. Character Reactions

| # | Feature | Status | File(s) |
|---|---------|--------|---------|
| 20.1 | Idle reaction | ❌ MISSING | — |
| 20.2 | Match reaction | ❌ MISSING | — |
| 20.3 | Special reaction | ❌ MISSING | — |
| 20.4 | Combo reaction | ❌ MISSING | — |
| 20.5 | Victory reaction | ❌ MISSING | — |
| 20.6 | Failure reaction | ❌ MISSING | — |

## 21. Easter Eggs

| # | Feature | Status | File(s) |
|---|---------|--------|---------|
| 21.1 | EasterEggManager | ❌ MISSING | — |
| 21.2 | Fan Chant | ❌ MISSING | — |
| 21.3 | SUGA's Rap | ❌ MISSING | — |
| 21.4 | RM Inspire | ❌ MISSING | — |

## 22. Events

| # | Feature | Status | File(s) |
|---|---------|--------|---------|
| 22.1 | Event framework | ❌ MISSING | — |
| 22.2 | Member Spotlight | ❌ MISSING | — |
| 22.3 | 7 Day Challenge | ❌ MISSING | — |

## 23. Save System

| # | Feature | Status | File(s) |
|---|---------|--------|---------|
| 23.1 | Save/load | ❌ MISSING | — |
| 23.2 | Settings persistence | ❌ MISSING | — |
| 23.3 | Progress persistence | ❌ MISSING | — |

## 24. Achievements

| # | Feature | Status | File(s) |
|---|---------|--------|---------|
| 24.1 | Achievement system | ❌ MISSING | — |
| 24.2 | BTS-themed achievements | ❌ MISSING | — |

## 25. Statistics

| # | Feature | Status | File(s) |
|---|---------|--------|---------|
| 25.1 | Stats tracking | ❌ MISSING | — |

## 26. Daily Challenge

| # | Feature | Status | File(s) |
|---|---------|--------|---------|
| 26.1 | Daily challenge architecture | ❌ MISSING | — |

## 27. Game State Machine

| # | Feature | Status | File(s) |
|---|---------|--------|---------|
| 27.1 | Explicit state machine | ❌ MISSING | — |
| 27.2 | Input locking during animations | 🟡 PARTIAL | GameBoard.tsx (isProcessing flag) |

## 28. Input System

| # | Feature | Status | File(s) |
|---|---------|--------|---------|
| 28.1 | Tap detection | ❌ MISSING | — |
| 28.2 | Swipe detection | 🟡 PARTIAL | GameBoard.tsx (pan gesture) |
| 28.3 | Drag support | ❌ MISSING | — |
| 28.4 | Swap threshold | 🟡 PARTIAL | GameBoard.tsx (20px threshold) |

## 29. Debug Tools

| # | Feature | Status | File(s) |
|---|---------|--------|---------|
| 29.1 | Debug panel | ❌ MISSING | — |

## 30. Performance

| # | Feature | Status | File(s) |
|---|---------|--------|---------|
| 30.1 | Image preloading | 🟡 PARTIAL | GameBoard.tsx (useImage) |
| 30.2 | Skia canvas rendering | ✅ IMPLEMENTED | GameBoard.tsx |
| 30.3 | Memoized components | ❌ MISSING | — |
| 30.4 | Optimized board updates | ❌ MISSING | — |

## 31. Play Store

| # | Feature | Status | File(s) |
|---|---------|--------|---------|
| 31.1 | App icon | ❌ MISSING | — |
| 31.2 | Splash screen | ❌ MISSING | — |
| 31.3 | Package name/versioning | 🟡 PARTIAL | app.json |
| 31.4 | EAS Build config | ❌ MISSING | — |
| 31.5 | Privacy policy | ❌ MISSING | — |
| 31.6 | Back-button handling | ❌ MISSING | — |

## Summary

| Category | Implemented | Partial | Missing | Total |
|----------|------------|---------|---------|-------|
| Core Board | 8 | 0 | 0 | 8 |
| Tile Mechanics | 20 | 2 | 0 | 22 |
| BTS Members | 4 | 2 | 4 | 10 |
| Special Tiles | 8 | 0 | 0 | 8 |
| Combinations | 14 | 0 | 0 | 14 |
| Combo/Cascade | 6 | 0 | 0 | 6 |
| Score | 3 | 0 | 3 | 6 |
| Objectives | 18 | 0 | 0 | 18 |
| Obstacles | 10 | 0 | 0 | 10 |
| Level System | 5 | 0 | 0 | 5 |
| Progression | 4 | 0 | 0 | 4 |
| UI Screens | 8 | 1 | 2 | 11 |
| HUD | 5 | 1 | 0 | 6 |
| Animation | 11 | 0 | 0 | 11 |
| Particles | 3 | 0 | 5 | 8 |
| Screen FX | 2 | 0 | 4 | 6 |
| Audio | 4 | 0 | 1 | 5 |
| Haptics | 3 | 0 | 0 | 3 |
| Tutorial | 0 | 0 | 2 | 2 |
| Reactions | 0 | 0 | 6 | 6 |
| Easter Eggs | 0 | 0 | 4 | 4 |
| Events | 0 | 0 | 3 | 3 |
| Save | 3 | 0 | 0 | 3 |
| Achievements | 1 | 0 | 1 | 2 |
| Stats | 1 | 0 | 0 | 1 |
| Daily | 0 | 0 | 1 | 1 |
| State Machine | 2 | 0 | 0 | 2 |
| Input | 4 | 0 | 0 | 4 |
| Debug | 0 | 0 | 1 | 1 |
| Performance | 4 | 0 | 0 | 4 |
| Play Store | 5 | 0 | 1 | 6 |
| **TOTAL** | **156** | **6** | **38** | **200+** |

**156 implemented. 6 partial. 38 missing. Out of 200+ features.**

The current implementation is **~78% complete** (and 100% of core puzzle loops are complete).
