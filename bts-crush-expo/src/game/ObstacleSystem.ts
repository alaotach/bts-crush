import { ObstacleType, Tile } from './Tile';
import { Board } from './Board';

export interface ObstacleResult {
  damaged: { x: number; y: number; type: ObstacleType }[];
  destroyed: { x: number; y: number; type: ObstacleType }[];
  scoreBonus: number;
}

export class ObstacleSystem {
  processAdjacentMatch(board: Board, matchedPositions: { x: number; y: number }[]): ObstacleResult {
    const damaged: { x: number; y: number; type: ObstacleType }[] = [];
    const destroyed: { x: number; y: number; type: ObstacleType }[] = [];
    let scoreBonus = 0;

    const dirs = [[0, 1], [0, -1], [1, 0], [-1, 0]];
    const processed = new Set<string>();

    for (const pos of matchedPositions) {
      for (const [dx, dy] of dirs) {
        const nx = pos.x + dx;
        const ny = pos.y + dy;
        const key = `${nx},${ny}`;
        
        if (processed.has(key)) continue;
        
        if (board.isPlayableCell(nx, ny)) {
          const tile = board.getTile(nx, ny);
          if (tile && tile.obstacle) {
            processed.add(key);
            tile.obstacleHP -= 1;
            
            if (tile.obstacleHP <= 0) {
              destroyed.push({ x: nx, y: ny, type: tile.obstacle });
              scoreBonus += 50; // Arbitrary score for destroyed obstacle
              tile.obstacle = null;
            } else {
              damaged.push({ x: nx, y: ny, type: tile.obstacle });
              scoreBonus += 10; // Arbitrary score for damaged obstacle
            }
          }
        }
      }
    }

    return { damaged, destroyed, scoreBonus };
  }

  processCountdownBombs(board: Board): { exploded: { x: number; y: number }[]; levelFailed: boolean } {
    const exploded: { x: number; y: number }[] = [];
    let levelFailed = false;

    for (let y = 0; y < board.rows; y++) {
      for (let x = 0; x < board.cols; x++) {
        const tile = board.getTile(x, y);
        if (tile && tile.obstacle === 'bomb') {
          tile.obstacleHP -= 1;
          if (tile.obstacleHP <= 0) {
            exploded.push({ x, y });
            levelFailed = true;
          }
        }
      }
    }

    return { exploded, levelFailed };
  }

  processChocolateSpread(board: Board): { x: number; y: number }[] {
    const newChocolates: { x: number; y: number }[] = [];
    const dirs = [[0, 1], [0, -1], [1, 0], [-1, 0]];
    const currentChocolates: { x: number; y: number }[] = [];

    for (let y = 0; y < board.rows; y++) {
      for (let x = 0; x < board.cols; x++) {
        const tile = board.getTile(x, y);
        if (tile && tile.obstacle === 'chocolate') {
          currentChocolates.push({ x, y });
        }
      }
    }

    for (const choc of currentChocolates) {
      const validTargets: { x: number; y: number }[] = [];
      for (const [dx, dy] of dirs) {
        const nx = choc.x + dx;
        const ny = choc.y + dy;
        if (board.isPlayableCell(nx, ny)) {
          const tile = board.getTile(nx, ny);
          if (tile && !tile.obstacle && !tile.special) {
            validTargets.push({ x: nx, y: ny });
          }
        }
      }

      if (validTargets.length > 0) {
        const target = validTargets[Math.floor(Math.random() * validTargets.length)];
        const targetTile = board.getTile(target.x, target.y);
        if (targetTile) {
          targetTile.obstacle = 'chocolate';
          targetTile.obstacleHP = 1;
          newChocolates.push({ x: target.x, y: target.y });
        }
      }
    }

    return newChocolates;
  }
}
