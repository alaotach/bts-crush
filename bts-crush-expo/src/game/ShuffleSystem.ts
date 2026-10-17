import { Board } from './Board';
import { MatchDetector } from './MatchDetector';
import { Tile } from './Tile';

export class ShuffleSystem {
  shuffle(board: Board): void {
    const detector = new MatchDetector();
    let maxAttempts = 100;
    
    while (maxAttempts > 0) {
      maxAttempts--;
      
      const tilesToShuffle: Tile[] = [];
      const positions: {x: number, y: number}[] = [];
      
      board.forEachCell((cell, x, y) => {
        if (cell.tile && cell.tile.member && !cell.tile.special && !cell.tile.obstacle) {
          tilesToShuffle.push(cell.tile);
          positions.push({x, y});
        }
      });
      
      for (let i = tilesToShuffle.length - 1; i > 0; i--) {
        const j = Math.floor(Math.random() * (i + 1));
        [tilesToShuffle[i], tilesToShuffle[j]] = [tilesToShuffle[j], tilesToShuffle[i]];
      }
      
      for (let i = 0; i < positions.length; i++) {
        const pos = positions[i];
        const tile = tilesToShuffle[i];
        board.setTile(pos.x, pos.y, tile);
      }
      
      if (!detector.hasAnyMatch(board) && detector.hasAnyValidMove(board)) {
        break;
      }
    }
  }

  needsShuffle(board: Board, matchDetector: MatchDetector): boolean {
    return !matchDetector.hasAnyValidMove(board);
  }
}
