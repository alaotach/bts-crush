import { LevelDefinition } from './LevelDefinition';
import { ALL_MEMBERS } from '../game/Tile';

/**
 * World 2 — "Dark & Wild Era"
 * Inspired by BTS's 2014 album "Dark & Wild"
 * Edgier concepts, introduces more complex objectives and obstacles
 * Levels 11-25: Introduce jelly, ice, crates, and chained objectives
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

export const world2Levels: LevelDefinition[] = [
  // Level 11: Introduce jelly
  {
    id: 11,
    world: 2,
    chapter: 'Dark & Wild Era',
    levelType: 'jelly',
    rows: 6,
    cols: 6,
    layout: [
      [R, R, R, R, R, R],
      [R, J, J, J, J, R],
      [R, J, R, R, J, R],
      [R, J, R, R, J, R],
      [R, J, J, J, J, R],
      [R, R, R, R, R, R],
    ],
    moves: 25,
    objectives: [{ type: 'clear_jelly', target: 12 }],
    starThresholds: [800, 1500, 2500],
    difficulty: 2,
    availableMembers: ['rm', 'jin', 'suga', 'jhope', 'jimin'],
  },

  // Level 12: More jelly clearing
  {
    id: 12,
    world: 2,
    chapter: 'Dark & Wild Era',
    levelType: 'jelly',
    rows: 6,
    cols: 6,
    layout: [
      [J, J, J, J, J, J],
      [J, R, R, R, R, J],
      [J, R, R, R, R, J],
      [J, R, R, R, R, J],
      [J, R, R, R, R, J],
      [J, J, J, J, J, J],
    ],
    moves: 30,
    objectives: [{ type: 'clear_jelly', target: 24 }],
    starThresholds: [1200, 2200, 3500],
    difficulty: 2,
    availableMembers: ['rm', 'jin', 'suga', 'jhope', 'jimin', 'v'],
  },

  // Level 13: Ice introduction
  {
    id: 13,
    world: 2,
    chapter: 'Dark & Wild Era',
    levelType: 'ice',
    rows: 6,
    cols: 6,
    layout: [
      [R, R, R, R, R, R],
      [R, I, I, I, I, R],
      [R, I, R, R, I, R],
      [R, I, R, R, I, R],
      [R, I, I, I, I, R],
      [R, R, R, R, R, R],
    ],
    moves: 25,
    objectives: [{ type: 'break_ice', target: 12 }],
    starThresholds: [1000, 1800, 3000],
    difficulty: 2,
    availableMembers: ['rm', 'jin', 'suga', 'jhope', 'jimin'],
  },

  // Level 14: Mixed jelly and ice
  {
    id: 14,
    world: 2,
    chapter: 'Dark & Wild Era',
    levelType: 'jelly_ice',
    rows: 6,
    cols: 6,
    layout: [
      [R, J, J, J, J, R],
      [J, I, R, R, I, J],
      [J, R, R, R, R, J],
      [J, R, R, R, R, J],
      [J, I, R, R, I, J],
      [R, J, J, J, J, R],
    ],
    moves: 30,
    objectives: [
      { type: 'clear_jelly', target: 12 },
      { type: 'break_ice', target: 8 }
    ],
    starThresholds: [1500, 2800, 4000],
    difficulty: 3,
    availableMembers: ['rm', 'jin', 'suga', 'jhope', 'jimin', 'v'],
  },

  // Level 15: Crate introduction
  {
    id: 15,
    world: 2,
    chapter: 'Dark & Wild Era',
    levelType: 'crate',
    rows: 6,
    cols: 6,
    layout: [
      [R, R, R, R, R, R],
      [R, C, C, C, C, R],
      [R, C, R, R, C, R],
      [R, C, R, R, C, R],
      [R, C, C, C, C, R],
      [R, R, R, R, R, R],
    ],
    moves: 25,
    objectives: [{ type: 'destroy_crate', target: 12 }],
    starThresholds: [1200, 2200, 3500],
    difficulty: 2,
    availableMembers: ['rm', 'jin', 'suga', 'jhope', 'jimin'],
  },

  // Level 16: Chain introduction
  {
    id: 16,
    world: 2,
    chapter: 'Dark & Wild Era',
    levelType: 'chain',
    rows: 6,
    cols: 6,
    layout: [
      [R, R, R, R, R, R],
      [R, Ch, Ch, Ch, Ch, R],
      [R, Ch, R, R, Ch, R],
      [R, Ch, R, R, Ch, R],
      [R, Ch, Ch, Ch, Ch, R],
      [R, R, R, R, R, R],
    ],
    moves: 25,
    objectives: [{ type: 'break_chain', target: 12 }],
    starThresholds: [1300, 2400, 3800],
    difficulty: 3,
    availableMembers: ['rm', 'jin', 'suga', 'jhope', 'jimin', 'v'],
  },

  // Level 17: Locked cells
  {
    id: 17,
    world: 2,
    chapter: 'Dark & Wild Era',
    levelType: 'keys',
    rows: 6,
    cols: 6,
    layout: [
      [L, L, R, R, L, L],
      [L, R, R, R, R, L],
      [R, R, R, R, R, R],
      [R, R, R, R, R, R],
      [L, R, R, R, R, L],
      [L, L, R, R, L, L],
    ],
    moves: 30,
    objectives: [{ type: 'collect_key', target: 8 }],
    starThresholds: [1400, 2600, 4200],
    difficulty: 3,
    availableMembers: ['rm', 'jin', 'suga', 'jhope', 'jimin', 'v', 'jungkook'],
  },

  // Level 18: Bring down (ingredients)
  {
    id: 18,
    world: 2,
    chapter: 'Dark & Wild Era',
    levelType: 'bring_down',
    rows: 6,
    cols: 6,
    layout: [
      [R, R, R, R, R, 90], // 90 represents ingredient to bring down
      [R, R, R, R, R, R],
      [R, R, R, R, R, R],
      [R, R, R, R, R, R],
      [R, R, R, R, R, R],
      [R, R, R, R, R, R],
    ],
    moves: 25,
    objectives: [{ type: 'bring_down', target: 3 }],
    starThresholds: [1000, 1800, 3000],
    difficulty: 2,
    availableMembers: ['rm', 'jin', 'suga', 'jhope', 'jimin'],
  },

  // Level 19: Create specials challenge
  {
    id: 19,
    world: 2,
    chapter: 'Dark & Wild Era',
    levelType: 'create_specials',
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
      { type: 'create_special', target: 4, description: 'Create 4 special candies' }
    ],
    starThresholds: [1600, 3000, 4500],
    difficulty: 3,
    availableMembers: ['rm', 'jin', 'suga', 'jhope', 'jimin', 'v'],
  },

  // Level 20: Activate specials
  {
    id: 20,
    world: 2,
    chapter: 'Dark & Wild Era',
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
    moves: 25,
    objectives: [
      { type: 'activate_special', target: 3, description: 'Activate 3 special candies' }
    ],
    starThresholds: [1400, 2600, 4000],
    difficulty: 3,
    availableMembers: ['rm', 'jin', 'suga', 'jhope', 'jimin', 'v'],
  },

  // Level 21: Combo challenge
  {
    id: 21,
    world: 2,
    chapter: 'Dark & Wild Era',
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
    moves: 30,
    objectives: [
      { type: 'perform_combo', target: 4, description: 'Get 4 combo matches' }
    ],
    starThresholds: [1800, 3200, 5000],
    difficulty: 3,
    availableMembers: ['rm', 'jin', 'suga', 'jhope', 'jimin', 'v'],
  },

  // Level 22: Timed challenge
  {
    id: 22,
    world: 2,
    chapter: 'Dark & Wild Era',
    levelType: 'timed',
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
    moves: 0, // No moves limit
    timeLimit: 60, // 60 seconds
    objectives: [{ type: 'score', target: 2000 }],
    starThresholds: [2000, 3500, 5000],
    difficulty: 3,
    availableMembers: ['rm', 'jin', 'suga', 'jhope', 'jimin', 'v'],
  },

  // Level 23: Multi-objective
  {
    id: 23,
    world: 2,
    chapter: 'Dark & Wild Era',
    levelType: 'multi_objective',
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
      { type: 'score', target: 2500 },
      { type: 'collect_member', target: 12, member: 'rm' },
      { type: 'create_special', target: 3 }
    ],
    starThresholds: [2500, 4500, 6500],
    difficulty: 4,
    availableMembers: ALL_MEMBERS,
  },

  // Level 24: Boss level - Mixed obstacles
  {
    id: 24,
    world: 2,
    chapter: 'Dark & Wild Era',
    levelType: 'boss',
    rows: 7,
    cols: 7,
    layout: [
      [L, H, H, R, H, H, L],
      [H, C, J, I, J, C, H],
      [H, J, R, R, R, J, H],
      [H, I, R, R, R, I, H],
      [H, J, R, R, R, J, H],
      [H, C, J, I, J, C, H],
      [L, H, H, R, H, H, L],
    ],
    moves: 40,
    objectives: [
      { type: 'collect_key', target: 4 },
      { type: 'clear_jelly', target: 8 },
      { type: 'break_ice', target: 4 },
      { type: 'destroy_crate', target: 4 }
    ],
    starThresholds: [3000, 5500, 8000],
    difficulty: 5,
    availableMembers: ALL_MEMBERS,
  },

  // Level 25: Ultimate challenge
  {
    id: 25,
    world: 2,
    chapter: 'Dark & Wild Era',
    levelType: 'boss',
    rows: 8,
    cols: 8,
    layout: [
      [H, H, L, L, L, L, H, H],
      [H, L, C, C, C, C, L, H],
      [L, C, J, I, I, J, C, L],
      [L, C, I, R, R, I, C, L],
      [L, C, I, R, R, I, C, L],
      [L, C, J, I, I, J, C, L],
      [H, L, C, C, C, C, L, H],
      [H, H, L, L, L, L, H, H],
    ],
    moves: 45,
    objectives: [
      { type: 'collect_key', target: 8 },
      { type: 'clear_jelly', target: 12 },
      { type: 'break_ice', target: 8 },
      { type: 'destroy_crate', target: 8 },
      { type: 'score', target: 4000 }
    ],
    starThresholds: [4000, 7000, 10000],
    difficulty: 5,
    availableMembers: ALL_MEMBERS,
  },
];