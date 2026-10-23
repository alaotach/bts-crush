import { Board } from './Board';
import { SpecialSystem } from './SpecialSystem';
import { GameState } from './GameState';
import { MEMBER_CONFIGS, MemberConfig, getMemberConfig } from '../bts/members';
import { Tile, createTile, MemberType } from './Tile';

export type UniqueAbilityType =
  | 'rap_boost'           // RM: Clear random obstacles
  | 'agust_flow'          // Suga: Clear random row
  | 'hope_world'          // J-Hope: Shuffle 25% of board
  | 'worldwide_handsome'  // Jin: Create heart bomb or remove blockers
  | 'promise'             // Jimin: TBD
  | 'winter_bear'         // V: TBD
  | 'golden_maknae'       // Jungkook: Create two specials
  | 'none';

export interface MemberAbilityResult {
  scoreBonus: number;
  affectedTiles: { x: number; y: number }[]; // Positions affected by ability
  specialsCreated: { x: number; y: number; type: string }[]; // Special tiles created
  description: string;
}

export class MemberAbilitySystem {
  private specialSystem: SpecialSystem;

  constructor(specialSystem: SpecialSystem) {
    this.specialSystem = specialSystem;
  }

  /**
   * Activate a member's unique ability
   * @param board The game board
   * @param memberId The member whose ability to activate
   * @returns Result of the ability activation
   */
  activateAbility(board: Board, memberId: string): MemberAbilityResult {
    const config = getMemberConfig(memberId as any);

    switch (config.uniqueAbility) {
      case 'rap_boost':
        return this.activateRapBoost(board);
      case 'agust_flow':
        return this.activateAgustFlow(board);
      case 'hope_world':
        return this.activateHopeWorld(board);
      case 'worldwide_handsome':
        return this.activateWorldwideHandsome(board);
      case 'promise':
        return this.activatePromise(board);
      case 'winter_bear':
        return this.activateWinterBear(board);
      case 'golden_maknae':
        return this.activateGoldenMaknae(board);
      default:
        return {
          scoreBonus: 0,
          affectedTiles: [],
          specialsCreated: [],
          description: 'No ability'
        };
    }
  }

  /**
   * RM's Rap Boost: Clear random obstacles (jelly, ice, crates) or spawn special candies
   */
  private activateRapBoost(board: Board): MemberAbilityResult {
    const affected: { x: number; y: number }[] = [];
    const specials: { x: number; y: number; type: string }[] = [];

    // Try to clear obstacles first
    let clearedObstacles = 0;
    for (let y = 0; y < board.rows; y++) {
      for (let x = 0; x < board.cols; x++) {
        const tile = board.getTile(x, y);
        // Check if tile has an obstacle (obstacle is not null)
        if (tile && tile.obstacle !== null && Math.random() < 0.3) { // 30% chance per obstacle
          affected.push({ x, y });
          board.setTile(x, y, null); // Clear the tile
          clearedObstacles++;
        }
      }
    }

    // If no obstacles cleared, create some special candies
    if (clearedObstacles === 0) {
      const specialCount = Math.min(3, Math.floor(Math.random() * 3) + 1);
      for (let i = 0; i < specialCount; i++) {
        let attempts = 0;
        let pos: { x: number; y: number } | null = null;

        while (attempts < 20 && !pos) {
          const x = Math.floor(Math.random() * board.cols);
          const y = Math.floor(Math.random() * board.rows);
          const tile = board.getTile(x, y);
          if (tile && tile.member !== null && tile.special === null && tile.obstacle === null) {
            pos = { x, y };
          }
          attempts++;
        }

        if (pos) {
          // Create a random special
          const specialTypes = ['mic_horizontal', 'mic_vertical', 'fan_heart', 'chibi'];
          const specialType = specialTypes[Math.floor(Math.random() * specialTypes.length)];
          specials.push({ x: pos.x, y: pos.y, type: specialType });
          // Actually place the special on board would be done by caller
        }
      }
    }

    return {
      scoreBonus: affected.length * 50,
      affectedTiles: affected,
      specialsCreated: specials,
      description: `RM's Rap Boost! Cleared ${affected.length} obstacles`
    };
  }

  /**
   * SUGA's Agust Flow: Clear a random row with musical notes
   */
  private activateAgustFlow(board: Board): MemberAbilityResult {
    const affected: { x: number; y: number }[] = [];
    const specials: { x: number; y: number; type: string }[] = [];

    const rowToClear = Math.floor(Math.random() * board.rows);

    for (let x = 0; x < board.cols; x++) {
      const tile = board.getTile(x, rowToClear);
      if (tile) {
        affected.push({ x, y: rowToClear });
        // Clear normal tiles, leave specials intact for now
        if (tile.member !== null && tile.special === null && tile.obstacle === null) {
          board.setTile(x, rowToClear, null);
        }
      }
    }

    // Create a special candy in the center of the cleared row
    const centerCol = Math.floor(board.cols / 2);
    const centerTile = board.getTile(centerCol, rowToClear);
    if (centerTile && centerTile.member !== null && centerTile.special === null && centerTile.obstacle === null) {
      specials.push({ x: centerCol, y: rowToClear, type: 'album_bomb' });
    }

    return {
      scoreBonus: affected.length * 30,
      affectedTiles: affected,
      specialsCreated: specials,
      description: `SUGA's Agust Flow! Cleared row ${rowToClear + 1}`
    };
  }

  /**
   * J-Hope's Hope World: Shuffle 25% of the board
   */
  private activateHopeWorld(board: Board): MemberAbilityResult {
    const affected: { x: number; y: number }[] = [];
    const specials: { x: number; y: number; type: string }[] = [];

    // Collect all normal tiles
    const normalTiles: { x: number; y: number; member: MemberType }[] = [];
    for (let y = 0; y < board.rows; y++) {
      for (let x = 0; x < board.cols; x++) {
        const tile = board.getTile(x, y);
        if (tile && tile.member !== null && tile.special === null && tile.obstacle === null) {
          normalTiles.push({ x, y, member: tile.member });
        }
      }
    }

    // Shuffle 25% of them
    const shuffleCount = Math.max(1, Math.floor(normalTiles.length * 0.25));
    const indicesToShuffle: number[] = [];

    while (indicesToShuffle.length < shuffleCount) {
      const idx = Math.floor(Math.random() * normalTiles.length);
      if (!indicesToShuffle.includes(idx)) {
        indicesToShuffle.push(idx);
      }
    }

    // Extract members to shuffle
    const membersToShuffle = indicesToShuffle.map(i => normalTiles[i].member);
    // Shuffle the members
    for (let i = membersToShuffle.length - 1; i > 0; i--) {
      const j = Math.floor(Math.random() * (i + 1));
      [membersToShuffle[i], membersToShuffle[j]] = [membersToShuffle[j], membersToShuffle[i]];
    }

    // Apply shuffled members back
    indicesToShuffle.forEach((idx, i) => {
      const pos = normalTiles[idx];
      board.setTile(pos.x, pos.y, createTile(pos.x, pos.y, membersToShuffle[i], null));
      affected.push({ x: pos.x, y: pos.y });
    });

    return {
      scoreBonus: affected.length * 10,
      affectedTiles: affected,
      specialsCreated: specials,
      description: `J-Hope's Hope World! Shuffled ${affected.length} candies`
    };
  }

  /**
   * Jin's Worldwide Handsome: Create heart bomb or remove blockers
   */
  private activateWorldwideHandsome(board: Board): MemberAbilityResult {
    const affected: { x: number; y: number }[] = [];
    const specials: { x: number; y: number; type: string }[] = [];

    // First try to find and remove blockers/obstacles
    let blockersRemoved = 0;
    for (let y = 0; y < board.rows; y++) {
      for (let x = 0; x < board.cols; x++) {
        const tile = board.getTile(x, y);
        // Check if tile has an obstacle (obstacle is not null)
        if (tile && tile.obstacle !== null && Math.random() < 0.2) { // 20% chance
          affected.push({ x, y });
          board.setTile(x, y, null); // Clear blocker
          blockersRemoved++;
        }
      }
    }

    // If no blockers found, create a heart bomb (fan_heart special)
    if (blockersRemoved === 0) {
      // Find a good spot for heart bomb
      let placed = false;
      let attempts = 0;
      while (!placed && attempts < 50) {
        const x = Math.floor(Math.random() * board.cols);
        const y = Math.floor(Math.random() * board.rows);
        const tile = board.getTile(x, y);
        if (tile && tile.member !== null && tile.special === null && tile.obstacle === null) {
          specials.push({ x, y, type: 'fan_heart' });
          placed = true;
        }
        attempts++;
      }

      if (!placed) {
        // Fallback: just put it somewhere
        const x = Math.floor(Math.random() * board.cols);
        const y = Math.floor(Math.random() * board.rows);
        specials.push({ x, y, type: 'fan_heart' });
      }
    }

    return {
      scoreBonus: blockersRemoved * 40,
      affectedTiles: affected,
      specialsCreated: specials,
      description: `Jin's Worldwide Handsome! ${blockersRemoved > 0 ? `Removed ${blockersRemoved} blockers` : 'Created Heart Hammer'}`
    };
  }

  /**
   * Jimin's Promise: Match candies of a specific type
   */
  private activatePromise(board: Board): MemberAbilityResult {
    const affected: { x: number; y: number }[] = [];
    const specials: { x: number; y: number; type: string }[] = [];

    // Pick a random member type
    const memberTypes = ['rm', 'jin', 'suga', 'jhope', 'jimin', 'v', 'jungkook'];
    const targetMember = memberTypes[Math.floor(Math.random() * memberTypes.length)];

    // Change candies to match target type and create matches
    const converted: { x: number; y: number }[] = [];

    for (let y = 0; y < board.rows; y++) {
      for (let x = 0; x < board.cols; x++) {
        const tile = board.getTile(x, y);
        if (tile && tile.member !== null && tile.special === null && tile.obstacle === null && tile.member !== targetMember) {
          // Convert to target member with 40% probability
          if (Math.random() < 0.4) {
            board.setTile(x, y, createTile(x, y, targetMember as MemberType, null));
            affected.push({ x, y });
            converted.push({ x, y });
          }
        }
      }
    }

    return {
      scoreBonus: affected.length * 15,
      affectedTiles: affected,
      specialsCreated: specials,
      description: `Jimin's Promise! Converted ${affected.length} candies to ${targetMember}`
    };
  }

  /**
   * V's Winter Bear: Create icy blasts in cross pattern
   */
  private activateWinterBear(board: Board): MemberAbilityResult {
    const affected: { x: number; y: number }[] = [];
    const specials: { x: number; y: number; type: string }[] = [];

    // Pick random center point
    const centerX = Math.floor(Math.random() * board.cols);
    const centerY = Math.floor(Math.random() * board.rows);

    // Clear vertical line
    for (let y = 0; y < board.rows; y++) {
      const tile = board.getTile(centerX, y);
      if (tile) {
        affected.push({ x: centerX, y });
        if (tile.member !== null && tile.special === null && tile.obstacle === null) {
          board.setTile(centerX, y, null);
        }
      }
    }

    // Clear horizontal line
    for (let x = 0; x < board.cols; x++) {
      const tile = board.getTile(x, centerY);
      if (tile) {
        affected.push({ x, y: centerY });
        if (tile.member !== null && tile.special === null && tile.obstacle === null) {
          board.setTile(x, centerY, null);
        }
      }
    }

    // Remove duplicates from center (counted twice)
    const uniqueAffected = Array.from(new Set(affected.map(pos => `${pos.x},${pos.y}`)))
      .map(str => {
        const [x, y] = str.split(',').map(Number);
        return { x, y };
      });

    // Create a special at center
    const centerTile = board.getTile(centerX, centerY);
    if (centerTile && centerTile.member !== null && centerTile.special === null && centerTile.obstacle === null) {
      specials.push({ x: centerX, y: centerY, type: 'dynamite' });
    }

    return {
      scoreBonus: uniqueAffected.length * 25,
      affectedTiles: uniqueAffected,
      specialsCreated: specials,
      description: `V's Winter Bear! Ice blast cleared line`
    };
  }

  /**
   * Jungkoop's Golden Maknae: Create two special candies
   */
  private activateGoldenMaknae(board: Board): MemberAbilityResult {
    const affected: { x: number; y: number }[] = [];
    const specials: { x: number; y: number; type: string }[] = [];

    // Create two special candies at random positions
    const specialTypes = ['mic_horizontal', 'mic_vertical', 'album_bomb', 'fan_heart', 'chibi'];

    for (let i = 0; i < 2; i++) {
      let placed = false;
      let attempts = 0;
      while (!placed && attempts < 30) {
        const x = Math.floor(Math.random() * board.cols);
        const y = Math.floor(Math.random() * board.rows);
        const tile = board.getTile(x, y);
        if (tile && tile.member !== null && tile.special === null && tile.obstacle === null) {
          const specialType = specialTypes[Math.floor(Math.random() * specialTypes.length)];
          specials.push({ x, y, type: specialType });
          affected.push({ x, y });
          placed = true;
        }
        attempts++;
      }

      if (!placed) {
        // Fallback to any position
        const x = Math.floor(Math.random() * board.cols);
        const y = Math.floor(Math.random() * board.rows);
        const specialType = specialTypes[Math.floor(Math.random() * specialTypes.length)];
        specials.push({ x, y, type: specialType });
        affected.push({ x, y });
      }
    }

    return {
      scoreBonus: affected.length * 60,
      affectedTiles: affected,
      specialsCreated: specials,
      description: `Jung Kook's Golden Maknae! Created 2 special candies`
    };
  }

  /**
   * Check if a member's ability is ready to use
   */
  isAbilityReady(memberState: any): boolean {
    return memberState.uniqueAbilityCooldownRemaining <= 0;
  }

  /**
   * Use a member's ability (if ready) and apply cooldown
   */
  useAbility(board: Board, memberId: string, memberState: any): MemberAbilityResult | null {
    if (!this.isAbilityReady(memberState)) {
      return null;
    }

    const result = this.activateAbility(board, memberId);
    memberState.uniqueAbilityCooldownRemaining = getMemberConfig(memberId as any).uniqueAbilityCooldown;
    return result;
  }

  /**
   * Update cooldowns (called after each move)
   */
  updateCooldowns(memberStates: Record<string, any>): void {
    for (const memberId in memberStates) {
      const state = memberStates[memberId];
      if (state.uniqueAbilityCooldownRemaining > 0) {
        state.uniqueAbilityCooldownRemaining--;
      }
    }
  }
}