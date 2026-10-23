import { MemberType } from '../game/Tile';

/**
 * BTS Member asset configuration.
 * 
 * The game engine only knows member IDs (MemberType).
 * This module maps those IDs to actual display assets.
 * All copyrighted material is isolated here — swap assets without touching game logic.
 */

export interface MemberReactionAssets {
  idle: any;         // require('...')
  selected: any;
  matched: any;
  special: any;
  celebration: any;
  failure: any;
}

export interface MemberConfig {
  id: MemberType;
  displayName: string;
  themeColor: string;
  accentColor: string;
  portrait: any;      // require('...')
  reactions?: Partial<MemberReactionAssets>;
  soundHook?: any;    // placeholder for licensed audio
  // Member progression
  baseXPRequired: number;     // XP needed for level 1->2
  xpGrowthFactor: number;     // Multiplier for each level
  // Unique ability
  uniqueAbility: UniqueAbilityType;
  uniqueAbilityCooldown: number; // Moves between ability uses
  uniqueAbilityPower: number;    // Base power of the ability
}

export type UniqueAbilityType =
  | 'rap_boost'           // RM: Clear random obstacles
  | 'agust_flow'          // Suga: Clear random row
  | 'hope_world'          // J-Hope: Shuffle 25% of board
  | 'worldwide_handsome'  // Jin: Create heart bomb or remove blockers
  | 'promise'             // Jimin: TBD
  | 'winter_bear'         // V: TBD
  | 'golden_maknae'       // Jungkook: Create two specials
  | 'none';

export interface MemberState {
  id: MemberType;
  level: number;
  xp: number;
  xpForNextLevel: number;
  isUnlocked: boolean;
  uniqueAbilityCooldownRemaining: number;
}

export const MEMBER_CONFIGS: Record<MemberType, MemberConfig> = {
  rm: {
    id: 'rm',
    displayName: 'RM',
    themeColor: '#3498db',
    accentColor: '#2471a3',
    portrait: require('../../assets/members/rm.png'),
    baseXPRequired: 100,
    xpGrowthFactor: 1.2,
    uniqueAbility: 'rap_boost',
    uniqueAbilityCooldown: 8,
    uniqueAbilityPower: 1,
  },
  jin: {
    id: 'jin',
    displayName: 'Jin',
    themeColor: '#ff9ff3',
    accentColor: '#fd79a8',
    portrait: require('../../assets/members/jin.png'),
    baseXPRequired: 100,
    xpGrowthFactor: 1.2,
    uniqueAbility: 'worldwide_handsome',
    uniqueAbilityCooldown: 6,
    uniqueAbilityPower: 1,
  },
  suga: {
    id: 'suga',
    displayName: 'SUGA',
    themeColor: '#9b59b6',
    accentColor: '#8e44ad',
    portrait: require('../../assets/members/suga.png'),
    baseXPRequired: 100,
    xpGrowthFactor: 1.2,
    uniqueAbility: 'agust_flow',
    uniqueAbilityCooldown: 10,
    uniqueAbilityPower: 1,
  },
  jhope: {
    id: 'jhope',
    displayName: 'j-hope',
    themeColor: '#f1c40f',
    accentColor: '#f39c12',
    portrait: require('../../assets/members/jhope.png'),
    baseXPRequired: 100,
    xpGrowthFactor: 1.2,
    uniqueAbility: 'hope_world',
    uniqueAbilityCooldown: 12,
    uniqueAbilityPower: 1,
  },
  jimin: {
    id: 'jimin',
    displayName: 'Jimin',
    themeColor: '#ffb8b8',
    accentColor: '#e17055',
    portrait: require('../../assets/members/jimin.png'),
    baseXPRequired: 100,
    xpGrowthFactor: 1.2,
    uniqueAbility: 'promise',
    uniqueAbilityCooldown: 8,
    uniqueAbilityPower: 1,
  },
  v: {
    id: 'v',
    displayName: 'V',
    themeColor: '#1abc9c',
    accentColor: '#16a085',
    portrait: require('../../assets/members/v.png'),
    baseXPRequired: 100,
    xpGrowthFactor: 1.2,
    uniqueAbility: 'winter_bear',
    uniqueAbilityCooldown: 10,
    uniqueAbilityPower: 1,
  },
  jungkook: {
    id: 'jungkook',
    displayName: 'Jung Kook',
    themeColor: '#2980b9',
    accentColor: '#1a5276',
    portrait: require('../../assets/members/jungkook.png'),
    baseXPRequired: 100,
    xpGrowthFactor: 1.2,
    uniqueAbility: 'golden_maknae',
    uniqueAbilityCooldown: 6,
    uniqueAbilityPower: 1,
  },
};

export function getMemberConfig(id: MemberType): MemberConfig {
  return MEMBER_CONFIGS[id];
}

export function getMemberColor(id: MemberType): string {
  return MEMBER_CONFIGS[id].themeColor;
}

export function getMemberDisplayName(id: MemberType): string {
  return MEMBER_CONFIGS[id].displayName;
}

/**
 * Calculate XP required for a given level
 * Formula: base * (growthFactor ^ (level - 1))
 */
export function calculateXPForLevel(level: number, baseXP: number, growthFactor: number): number {
  if (level <= 1) return 0;
  return Math.floor(baseXP * Math.pow(growthFactor, level - 1));
}

/**
 * Calculate total XP required to reach a given level (from level 1)
 */
export function calculateTotalXPForLevel(level: number, baseXP: number, growthFactor: number): number {
  if (level <= 1) return 0;
  let total = 0;
  for (let lvl = 1; lvl < level; lvl++) {
    total += calculateXPForLevel(lvl + 1, baseXP, growthFactor);
  }
  return total;
}

/**
 * Get member's current level based on XP
 */
export function getMemberLevelFromXP(xp: number, baseXP: number, growthFactor: number): number {
  if (xp < baseXP) return 1;
  
  let level = 1;
  let xpNeededForNext = calculateXPForLevel(2, baseXP, growthFactor);
  
  while (xp >= xpNeededForNext) {
    level++;
    xpNeededForNext = calculateXPForLevel(level + 1, baseXP, growthFactor);
  }
  
  return level;
}

/**
 * Get XP required for next level
 */
export function getXPForNextLevel(currentLevel: number, baseXP: number, growthFactor: number): number {
  return calculateXPForLevel(currentLevel + 1, baseXP, growthFactor);
}

/**
 * Initialize member state for a new game
 */
export function initializeMemberState(id: MemberType): MemberState {
  const config = getMemberConfig(id);
  return {
    id,
    level: 1,
    xp: 0,
    xpForNextLevel: getXPForNextLevel(1, config.baseXPRequired, config.xpGrowthFactor),
    isUnlocked: true, // All members start unlocked for now
    uniqueAbilityCooldownRemaining: 0,
  };
}
