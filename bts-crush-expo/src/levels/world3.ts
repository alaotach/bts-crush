import { LevelDefinition } from './LevelDefinition';
import { ALL_MEMBERS } from '../game/Tile';

/**
 * World 3 — "Wings Era"
 * Inspired by BTS's 2016 album "Wings"
 * Introduces more complex mechanics: portals, conveyor belts, and advanced objectives
 * Levels 26-40: Wings concept - flying, freedom, reaching new heights
 */

const R = 1; // normal cell
const H = 0; // hole
const J = 10; // jelly
const I = 11; // ice
const II = 12; // double ice
const C = 20; // crate
const CC = 21; // double crate
const Ch = 22; // chain
const L = 23; // locked
const P_IN = 30; // portal_in
const P_OUT = 31; // portal_out
const CU = 40; // conveyor_up
const CD = 41; // conveyor_down
const CL = 42; // conveyor_left
const CR = 43; // conveyor_right
const B = 50; // bomb
const SL = 60; // stage light

export const world3Levels: LevelDefinition[] = [
  // Level 26: Introduce portals
  {
    id: 26,
    world: 3,
    chapter: 'Wings Era',
    levelType: 'keys',
    rows: 6,
    cols: 6,
    layout: [
      [R, R, R, R, R, R],
      [R, P_IN, R, R, P_OUT, R],
      [R, R, R, R, R, R],
      [R, R, R, R, R, R],
      [R, R, R, R, R, R],
      [R, R, R, R, R, R],
    ],
    moves: 30,
    objectives: [{ type: 'collect_key', target: 2 }],
    portals: [
      { from: { x: 1, y: 1 }, to: { x: 4, y: 1 } }
    ],
    starThresholds: [1000, 1800, 3000],
    difficulty: 2,
    availableMembers: ['rm', 'jin', 'suga', 'jhope', 'jimin'],
  },

  // Level 27: Portal jelly clearing
  {
    id: 27,
    world: 3,
    chapter: 'Wings Era',
    levelType: 'jelly',
    rows: 6,
    cols: 6,
    layout: [
      [R, J, J, J, J, R],
      [J, P_IN, R, R, P_OUT, J],
      [J, R, R, R, R, J],
      [J, R, R, R, R, J],
      [J, P_IN, R, R, P_OUT, J],
      [R, J, J, J, J, R],
    ],
    moves: 35,
    objectives: [{ type: 'clear_jelly', target: 20 }],
    portals: [
      { from: { x: 1, y: 1 }, to: { x: 4, y: 1 } },
      { from: { x: 1, y: 4 }, to: { x: 4, y: 4 } }
    ],
    starThresholds: [1500, 2800, 4200],
    difficulty: 3,
    availableMembers: ['rm', 'jin', 'suga', 'jhope', 'jimin', 'v'],
  },

  // Level 28: Introduce conveyor belts
  {
    id: 28,
    world: 3,
    chapter: 'Wings Era',
    levelType: 'bring_down',
    rows: 6,
    cols: 6,
    layout: [
      [CU, CU, CU, CU, CU, 90], // conveyor up + ingredient
      [R, R, R, R, R, R],
      [R, R, R, R, R, R],
      [R, R, R, R, R, R],
      [R, R, R, R, R, R],
      [R, R, R, R, R, R],
    ],
    moves: 30,
    objectives: [{ type: 'bring_down', target: 3 }],
    starThresholds: [1200, 2200, 3500],
    difficulty: 2,
    availableMembers: ['rm', 'jin', 'suga', 'jhope', 'jimin'],
  },

  // Level 29: Mixed conveyor directions
  {
    id: 29,
    world: 3,
    chapter: 'Wings Era',
    levelType: 'multi_objective',
    rows: 6,
    cols: 6,
    layout: [
      [CL, CU, CR, CU, CL, CU],
      [CD, R, R, R, R, CD],
      [CL, R, R, R, R, CU],
      [CD, R, R, R, R, CL],
      [CL, CU, CR, CU, CL, CD],
      [CD, R, R, R, R, CL],
    ],
    moves: 35,
    objectives: [
      { type: 'break_ice', target: 6 },
      { type: 'clear_jelly', target: 6 }
    ],
    starThresholds: [1800, 3200, 4800],
    difficulty: 3,
    availableMembers: ['rm', 'jin', 'suga', 'jhope', 'jimin', 'v'],
  },

  // Level 30: Wings formation (V shape)
  {
    id: 30,
    world: 3,
    chapter: 'Wings Era',
    levelType: 'collect',
    rows: 7,
    cols: 7,
    layout: [
      [R, R, R, H, R, R, R],
      [R, R, H, R, H, R, R],
      [R, H, R, R, R, H, R],
      [H, R, R, R, R, R, H],
      [R, H, R, R, R, H, R],
      [R, R, H, R, H, R, R],
      [R, R, R, H, R, R, R],
    ],
    moves: 30,
    objectives: [{ type: 'collect_member', target: 20, member: 'jhope' }],
    starThresholds: [2000, 3600, 5500],
    difficulty: 3,
    availableMembers: ['rm', 'jin', 'suga', 'jhope', 'jimin', 'v', 'jungkook'],
  },

  // Level 31: Butterfly jelly pattern
  {
    id: 31,
    world: 3,
    chapter: 'Wings Era',
    levelType: 'jelly',
    rows: 7,
    cols: 7,
    layout: [
      [R, R, J, J, J, R, R],
      [R, J, J, J, J, J, R],
      [J, J, R, R, R, J, J],
      [J, R, R, R, R, R, J],
      [J, J, R, R, R, J, J],
      [R, J, J, J, J, J, R],
      [R, R, J, J, J, R, R],
    ],
    moves: 35,
    objectives: [{ type: 'clear_jelly', target: 24 }],
    starThresholds: [2200, 4000, 6000],
    difficulty: 3,
    availableMembers: ['rm', 'jin', 'suga', 'jhope', 'jimin', 'v'],
  },

  // Level 32: Ice wings
  {
    id: 32,
    world: 3,
    chapter: 'Wings Era',
    levelType: 'ice',
    rows: 7,
    cols: 7,
    layout: [
      [R, R, I, I, I, R, R],
      [R, I, I, I, I, I, R],
      [I, I, R, R, R, I, I],
      [I, R, R, R, R, R, I],
      [I, I, R, R, R, I, I],
      [R, I, I, I, I, I, R],
      [R, R, I, I, I, R, R],
    ],
    moves: 30,
    objectives: [{ type: 'break_ice', target: 20 }],
    starThresholds: [1800, 3400, 5200],
    difficulty: 3,
    availableMembers: ['rm', 'jin', 'suga', 'jhope', 'jimin', 'v'],
  },

  // Level 33: Crate maze
  {
    id: 33,
    world: 3,
    chapter: 'Wings Era',
    levelType: 'crate',
    rows: 6,
    cols: 6,
    layout: [
      [R, C, C, C, C, R],
      [C, R, R, R, R, C],
      [C, R, C, C, R, C],
      [C, R, C, C, R, C],
      [C, R, R, R, R, C],
      [R, C, C, C, C, R],
    ],
    moves: 35,
    objectives: [{ type: 'destroy_crate', target: 16 }],
    starThresholds: [2000, 3600, 5400],
    difficulty: 3,
    availableMembers: ['rm', 'jin', 'suga', 'jhope', 'jimin'],
  },

  // Level 34: Chain links
  {
    id: 34,
    world: 3,
    chapter: 'Wings Era',
    levelType: 'chain',
    rows: 6,
    cols: 6,
    layout: [
      [R, Ch, Ch, Ch, Ch, R],
      [Ch, R, R, R, R, Ch],
      [Ch, R, Ch, Ch, R, Ch],
      [Ch, R, Ch, Ch, R, Ch],
      [Ch, R, R, R, R, Ch],
      [R, Ch, Ch, Ch, Ch, R],
    ],
    moves: 30,
    objectives: [{ type: 'break_chain', target: 16 }],
    starThresholds: [1900, 3500, 5300],
    difficulty: 3,
    availableMembers: ['rm', 'jin', 'suga', 'jhope', 'jimin', 'v'],
  },

  // Level 35: Locked wings
  {
    id: 35,
    world: 3,
    chapter: 'Wings Era',
    levelType: 'keys',
    rows: 7,
    cols: 7,
    layout: [
      [L, L, L, L, L, L, L],
      [L, R, R, R, R, R, L],
      [L, R, L, L, L, R, L],
      [L, R, L, R, R, R, L],
      [L, R, L, L, L, R, L],
      [L, R, R, R, R, R, L],
      [L, L, L, L, L, L, L],
    ],
    moves: 40,
    objectives: [{ type: 'collect_key', target: 12 }],
    starThresholds: [2500, 4500, 6800],
    difficulty: 4,
    availableMembers: ALL_MEMBERS,
  },

  // Level 36: Portal network
  {
    id: 36,
    world: 3,
    chapter: 'Wings Era',
    levelType: 'multi_objective',
    rows: 6,
    cols: 6,
    layout: [
      [R, R, R, R, R, R],
      [R, P_IN, R, P_OUT, R, R],
      [R, R, P_IN, P_OUT, R, R],
      [R, P_OUT, R, P_IN, R, R],
      [R, P_OUT, R, P_IN, R, R],
      [R, R, R, R, R, R],
    ],
    moves: 40,
    objectives: [
      { type: 'clear_jelly', target: 8 },
      { type: 'break_ice', target: 8 },
      { type: 'collect_key', target: 4 }
    ],
    portals: [
      { from: { x: 1, y: 1 }, to: { x: 3, y: 2 } },
      { from: { x: 4, y: 1 }, to: { x: 2, y: 3 } },
      { from: { x: 2, y: 4 }, to: { x: 4, y: 3 } },
      { from: { x: 3, y: 4 }, to: { x: 1, y: 3 } }
    ],
    starThresholds: [2200, 4000, 6200],
    difficulty: 4,
    availableMembers: ALL_MEMBERS,
  },

  // Level 37: Create specials with conveyors
  {
    id: 37,
    world: 3,
    chapter: 'Wings Era',
    levelType: 'create_specials',
    rows: 6,
    cols: 6,
    layout: [
      [CU, CU, CU, CU, CU, CU],
      [R, R, R, R, R, R],
      [R, R, R, R, R, R],
      [R, R, R, R, R, R],
      [R, R, R, R, R, R],
      [CD, CD, CD, CD, CD, CD],
    ],
    moves: 35,
    objectives: [
      { type: 'create_special', target: 5, description: 'Create 5 special candies' }
    ],
    starThresholds: [2000, 3800, 5800],
    difficulty: 3,
    availableMembers: ['rm', 'jin', 'suga', 'jhope', 'jimin', 'v'],
  },

  // Level 38: Activate specials challenge
  {
    id: 38,
    world: 3,
    chapter: 'Wings Era',
    levelType: 'activate_specials',
    rows: 6,
    cols: 6,
    layout: [
      [R, R, R, R, R, R],
      [R, R, R, R, R, R],
      [R, R, R, R, R, R],
      [R, R, R, R, R, R],
      [R, R, R, R, R, R],
      [R, R, R, R, R, R],
    ],
    moves: 30,
    objectives: [
      { type: 'activate_special', target: 4, description: 'Activate 4 special candies' }
    ],
    starThresholds: [1800, 3400, 5200],
    difficulty: 3,
    availableMembers: ['rm', 'jin', 'suga', 'jhope', 'jimin', 'v'],
  },

  // Level 39: Combo masters
  {
    id: 39,
    world: 3,
    chapter: 'Wings Era',
    levelType: 'combo_challenge',
    rows: 6,
    cols: 6,
    layout: [
      [R, R, R, R, R, R],
      [R, R, R, R, R, R],
      [R, R, R, R, R, R],
      [R, R, R, R, R, R],
      [R, R, R, R, R, R],
      [R, R, R, R, R, R],
    ],
    moves: 35,
    objectives: [
      { type: 'perform_combo', target: 6, description: 'Get 6 combo matches' },
      { type: 'perform_cascade', target: 3, description: 'Get 3 cascades' }
    ],
    starThresholds: [2200, 4000, 6000],
    difficulty: 4,
    availableMembers: ALL_MEMBERS,
  },

  // Level 40: Wings Era Boss
  {
    id: 40,
    world: 3,
    chapter: 'Wings Era',
    levelType: 'boss',
    rows: 8,
    cols: 8,
    layout: [
      [H, H, L, P_IN, P_OUT, L, H, H],
      [H, L, C, J, J, C, L, H],
      [L, C, J, R, R, J, C, L],
      [P_OUT, J, R, SL, SL, R, J, P_IN],
      [P_IN, J, R, SL, SL, R, J, P_OUT],
      [L, C, J, R, R, J, C, L],
      [H, L, C, J, J, C, L, H],
      [H, H, L, P_IN, P_OUT, L, H, H],
    ],
    moves: 50,
    objectives: [
      { type: 'collect_key', target: 6 },
      { type: 'clear_jelly', target: 10 },
      { type: 'break_ice', target: 6 },
      { type: 'destroy_crate', target: 6 },
      { type: 'activate_stage_light', target: 2 },
      { type: 'score', target: 5000 }
    ],
    portals: [
      { from: { x: 3, y: 0 }, to: { x: 4, y: 0 } },
      { from: { x: 4, y: 7 }, to: { x: 3, y: 7 } }
    ],
    starThresholds: [5000, 8500, 12000],
    difficulty: 5,
    availableMembers: ALL_MEMBERS,
  },
];