import { LevelDefinition } from './LevelDefinition';

/**
 * World 1 — "Practice Room"
 * Soft, gentle introduction to the game.
 * Levels 1-10: Basic matching, no obstacles, gradually introduce specials.
 */

const R = 1; // normal cell
const H = 0; // hole

export const world1Levels: LevelDefinition[] = [
  // Level 1: Basic 3-match tutorial
  {
    id: 1,
    world: 1,
    chapter: 'Practice Room',
    levelType: 'score',
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
    moves: 20,
    objectives: [{ type: 'score', target: 500 }],
    starThresholds: [500, 1000, 2000],
    difficulty: 1,
    availableMembers: ['rm', 'jin', 'suga', 'jhope', 'jimin'],
  },

  // Level 2: Collect specific member
  {
    id: 2,
    world: 1,
    chapter: 'Practice Room',
    levelType: 'collect',
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
    objectives: [{ type: 'collect_member', target: 15, member: 'rm' }],
    starThresholds: [600, 1200, 2500],
    difficulty: 1,
    availableMembers: ['rm', 'jin', 'suga', 'jhope', 'jimin'],
  },

  // Level 3: Higher score target
  {
    id: 3,
    world: 1,
    chapter: 'Practice Room',
    levelType: 'score',
    rows: 7,
    cols: 7,
    layout: [
      [R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R],
    ],
    moves: 25,
    objectives: [{ type: 'score', target: 1500 }],
    starThresholds: [1500, 3000, 5000],
    difficulty: 2,
    availableMembers: ['rm', 'jin', 'suga', 'jhope', 'jimin'],
  },

  // Level 4: Multi-collect
  {
    id: 4,
    world: 1,
    chapter: 'Practice Room',
    levelType: 'collect',
    rows: 7,
    cols: 7,
    layout: [
      [R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R],
    ],
    moves: 30,
    objectives: [
      { type: 'collect_member', target: 10, member: 'jin' },
      { type: 'collect_member', target: 10, member: 'v' },
    ],
    starThresholds: [1000, 2000, 4000],
    difficulty: 2,
    availableMembers: ['rm', 'jin', 'suga', 'v', 'jimin'],
  },

  // Level 5: First board with holes
  {
    id: 5,
    world: 1,
    chapter: 'Practice Room',
    levelType: 'score',
    rows: 7,
    cols: 7,
    layout: [
      [H, R, R, R, R, R, H],
      [R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R],
      [H, R, R, R, R, R, H],
    ],
    moves: 25,
    objectives: [{ type: 'score', target: 2000 }],
    starThresholds: [2000, 4000, 7000],
    difficulty: 3,
    availableMembers: ['rm', 'jin', 'suga', 'jhope', 'jimin', 'v'],
  },

  // Level 6: Introduce specials objective
  {
    id: 6,
    world: 1,
    chapter: 'Practice Room',
    levelType: 'create_specials',
    rows: 7,
    cols: 7,
    layout: [
      [R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R],
    ],
    moves: 30,
    objectives: [
      { type: 'create_special', target: 3, description: 'Create 3 special candies' },
    ],
    starThresholds: [1500, 3000, 5000],
    difficulty: 3,
    availableMembers: ['rm', 'jin', 'suga', 'jhope', 'jimin', 'v'],
  },

  // Level 7: Bigger board, more members
  {
    id: 7,
    world: 1,
    chapter: 'Practice Room',
    levelType: 'score',
    rows: 8,
    cols: 8,
    layout: [
      [R, R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R, R],
    ],
    moves: 30,
    objectives: [{ type: 'score', target: 3000 }],
    starThresholds: [3000, 6000, 10000],
    difficulty: 3,
    availableMembers: ['rm', 'jin', 'suga', 'jhope', 'jimin', 'v', 'jungkook'],
  },

  // Level 8: Irregular board
  {
    id: 8,
    world: 1,
    chapter: 'Practice Room',
    levelType: 'collect',
    rows: 7,
    cols: 7,
    layout: [
      [H, H, R, R, R, H, H],
      [H, R, R, R, R, R, H],
      [R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R],
      [H, R, R, R, R, R, H],
      [H, H, R, R, R, H, H],
    ],
    moves: 25,
    objectives: [
      { type: 'collect_member', target: 15, member: 'jungkook' },
    ],
    starThresholds: [2000, 4000, 7000],
    difficulty: 4,
  },

  // Level 9: Combo challenge
  {
    id: 9,
    world: 1,
    chapter: 'Practice Room',
    levelType: 'combo_challenge',
    rows: 7,
    cols: 7,
    layout: [
      [R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R],
    ],
    moves: 30,
    objectives: [
      { type: 'perform_combo', target: 3, description: 'Get 3 cascade combos' },
    ],
    starThresholds: [2500, 5000, 8000],
    difficulty: 4,
  },

  // Level 10: Multi-objective boss
  {
    id: 10,
    world: 1,
    chapter: 'Practice Room',
    levelType: 'multi_objective',
    rows: 8,
    cols: 8,
    layout: [
      [R, R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R, R],
      [R, R, R, R, R, R, R, R],
    ],
    moves: 35,
    objectives: [
      { type: 'score', target: 5000 },
      { type: 'collect_member', target: 20, member: 'rm' },
      { type: 'create_special', target: 5 },
    ],
    starThresholds: [5000, 10000, 15000],
    difficulty: 5,
  },
];
