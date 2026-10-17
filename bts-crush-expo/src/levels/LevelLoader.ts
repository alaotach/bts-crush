import { LevelDefinition } from './LevelDefinition';
import { world1Levels } from './world1';

/**
 * LevelLoader — loads and provides level definitions.
 * 
 * All levels are registered here. The GameEngine asks for a level by ID
 * and gets a fully-defined LevelDefinition object.
 */

const allLevels: LevelDefinition[] = [
  ...world1Levels,
  // Future worlds:
  // ...world2Levels,
  // ...world3Levels,
];

export class LevelLoader {
  private levels: Map<number, LevelDefinition>;

  constructor() {
    this.levels = new Map();
    for (const level of allLevels) {
      this.levels.set(level.id, level);
    }
  }

  getLevel(id: number): LevelDefinition | null {
    return this.levels.get(id) || null;
  }

  getLevelCount(): number {
    return this.levels.size;
  }

  getWorldLevels(world: number): LevelDefinition[] {
    return allLevels.filter(l => l.world === world);
  }

  getAllWorlds(): number[] {
    const worlds = new Set<number>();
    for (const level of allLevels) {
      worlds.add(level.world);
    }
    return Array.from(worlds).sort((a, b) => a - b);
  }

  getNextLevel(currentId: number): LevelDefinition | null {
    return this.levels.get(currentId + 1) || null;
  }
}
