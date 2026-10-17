import { MemberType } from '../game/Tile';

/**
 * Level Definition — ALL level content is data-driven.
 * The game engine reads these definitions, never hardcodes level-specific logic.
 */

export type ObjectiveType =
  | 'score'
  | 'collect_member'
  | 'collect_multiple'
  | 'clear_jelly'
  | 'break_ice'
  | 'destroy_crate'
  | 'break_chain'
  | 'unlock_cell'
  | 'collect_key'
  | 'bring_down'
  | 'activate_stage_light'
  | 'create_special'
  | 'activate_special'
  | 'perform_combo'
  | 'perform_cascade';

export type LevelType =
  | 'score'
  | 'collect'
  | 'jelly'
  | 'ice'
  | 'crate'
  | 'chain'
  | 'keys'
  | 'bring_down'
  | 'create_specials'
  | 'activate_specials'
  | 'combo_challenge'
  | 'timed'
  | 'multi_objective'
  | 'boss';

export interface ObjectiveDef {
  type: ObjectiveType;
  target: number;
  member?: MemberType;
  specialType?: string;
  description?: string;
}

export type CellLayout = number; 
// 0 = hole, 1 = normal, 2 = blocked
// 10 = jelly, 11 = ice, 12 = double_ice
// 20 = crate, 21 = double_crate, 22 = chain, 23 = locked
// 30 = portal_in, 31 = portal_out
// 40 = conveyor_up, 41 = conveyor_down, 42 = conveyor_left, 43 = conveyor_right
// 50 = bomb (with bombCounter)
// 60 = stage_light

export interface PortalDef {
  from: { x: number; y: number };
  to: { x: number; y: number };
}

export interface BombDef {
  x: number;
  y: number;
  counter: number;
}

export interface LevelDefinition {
  id: number;
  world: number;
  chapter: string;
  levelType: LevelType;
  
  // Board shape
  rows: number;
  cols: number;
  layout: CellLayout[][];
  
  // Constraints
  moves?: number;
  timeLimit?: number; // seconds, for timed levels
  
  // Which members are available (subset of 7, default all 7)
  availableMembers?: MemberType[];
  
  // Objectives
  objectives: ObjectiveDef[];
  
  // Obstacle definitions
  portals?: PortalDef[];
  bombs?: BombDef[];
  
  // Scoring thresholds for stars
  starThresholds: [number, number, number]; // 1-star, 2-star, 3-star
  
  // Difficulty metadata
  difficulty: number; // 1-10
  
  // Rewards
  rewards?: {
    type: string;
    value: number;
  }[];
  
  // Special rules
  specialRules?: string[];
}

/**
 * Parse a CellLayout number into its cell type and obstacle info.
 */
export function parseCellLayout(value: CellLayout): {
  cellType: 'normal' | 'hole' | 'blocked';
  obstacle?: string;
  obstacleHP?: number;
} {
  if (value === 0) return { cellType: 'hole' };
  if (value === 1) return { cellType: 'normal' };
  if (value === 2) return { cellType: 'blocked' };
  if (value === 10) return { cellType: 'normal', obstacle: 'jelly', obstacleHP: 1 };
  if (value === 11) return { cellType: 'normal', obstacle: 'ice', obstacleHP: 1 };
  if (value === 12) return { cellType: 'normal', obstacle: 'ice', obstacleHP: 2 };
  if (value === 20) return { cellType: 'normal', obstacle: 'crate', obstacleHP: 1 };
  if (value === 21) return { cellType: 'normal', obstacle: 'crate', obstacleHP: 2 };
  if (value === 22) return { cellType: 'normal', obstacle: 'chain', obstacleHP: 1 };
  if (value === 23) return { cellType: 'normal', obstacle: 'locked', obstacleHP: 1 };
  if (value === 50) return { cellType: 'normal', obstacle: 'bomb', obstacleHP: 1 };
  if (value === 60) return { cellType: 'normal', obstacle: 'stage_light', obstacleHP: 1 };
  
  return { cellType: 'normal' };
}
