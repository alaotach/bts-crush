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
}

export const MEMBER_CONFIGS: Record<MemberType, MemberConfig> = {
  rm: {
    id: 'rm',
    displayName: 'RM',
    themeColor: '#3498db',
    accentColor: '#2471a3',
    portrait: require('../../assets/members/rm.png'),
  },
  jin: {
    id: 'jin',
    displayName: 'Jin',
    themeColor: '#ff9ff3',
    accentColor: '#fd79a8',
    portrait: require('../../assets/members/jin.png'),
  },
  suga: {
    id: 'suga',
    displayName: 'SUGA',
    themeColor: '#9b59b6',
    accentColor: '#8e44ad',
    portrait: require('../../assets/members/suga.png'),
  },
  jhope: {
    id: 'jhope',
    displayName: 'j-hope',
    themeColor: '#f1c40f',
    accentColor: '#f39c12',
    portrait: require('../../assets/members/jhope.png'),
  },
  jimin: {
    id: 'jimin',
    displayName: 'Jimin',
    themeColor: '#ffb8b8',
    accentColor: '#e17055',
    portrait: require('../../assets/members/jimin.png'),
  },
  v: {
    id: 'v',
    displayName: 'V',
    themeColor: '#1abc9c',
    accentColor: '#16a085',
    portrait: require('../../assets/members/v.png'),
  },
  jungkook: {
    id: 'jungkook',
    displayName: 'Jung Kook',
    themeColor: '#2980b9',
    accentColor: '#1a5276',
    portrait: require('../../assets/members/jungkook.png'),
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
