import { Board } from './Board';
import { MemberType, Tile, createTile } from './Tile';
import { MatchDetector } from './MatchDetector';

export class SpawnSystem {
  private availableMembers: MemberType[];

  constructor(availableMembers: MemberType[]) {
    this.availableMembers = availableMembers;
  }

  setAvailableMembers(members: MemberType[]): void {
    this.availableMembers = members;
  }

  /** Fill entire board with random tiles, ensuring no initial matches */
  fillBoardInitial(board: Board, matchDetector: MatchDetector): void {
    for (let y = 0; y < board.rows; y++) {
      for (let x = 0; x < board.cols; x++) {
        if (!board.isPlayableCell(x, y)) continue;

        let bestMember: MemberType = this.availableMembers[0];
        for (let attempt = 0; attempt < 20; attempt++) {
          bestMember = this.availableMembers[Math.floor(Math.random() * this.availableMembers.length)];

          // Check horizontal
          const left1 = board.getTile(x - 1, y)?.member;
          const left2 = board.getTile(x - 2, y)?.member;
          // Check vertical
          const up1 = board.getTile(x, y - 1)?.member;
          const up2 = board.getTile(x, y - 2)?.member;

          const matchH = (left1 === bestMember && left2 === bestMember);
          const matchV = (up1 === bestMember && up2 === bestMember);

          if (!matchH && !matchV) break;
        }

        board.setTile(x, y, createTile(x, y, bestMember, null));
      }
    }
  }

  /** Spawn tiles at given positions. If no positions given, find all empty playable cells. */
  spawnTiles(board: Board, positions?: {x: number, y: number}[]): {x: number, y: number}[] {
    const targets = positions || this.getEmptyPlayableCells(board);
    const spawned: {x: number, y: number}[] = [];
    
    for (const pos of targets) {
      if (!board.isPlayableCell(pos.x, pos.y) || board.getTile(pos.x, pos.y) !== null) continue;

      let bestMember: MemberType = this.availableMembers[0];
      
      for (let attempt = 0; attempt < 10; attempt++) {
        const randIdx = Math.floor(Math.random() * this.availableMembers.length);
        bestMember = this.availableMembers[randIdx];
        
        const left1 = board.getTile(pos.x - 1, pos.y)?.member;
        const left2 = board.getTile(pos.x - 2, pos.y)?.member;
        const up1 = board.getTile(pos.x, pos.y - 1)?.member;
        const up2 = board.getTile(pos.x, pos.y - 2)?.member;
        const right1 = board.getTile(pos.x + 1, pos.y)?.member;
        const right2 = board.getTile(pos.x + 2, pos.y)?.member;
        const down1 = board.getTile(pos.x, pos.y + 1)?.member;
        const down2 = board.getTile(pos.x, pos.y + 2)?.member;

        const matchHLeft = (left1 === bestMember && left2 === bestMember);
        const matchHRight = (right1 === bestMember && right2 === bestMember);
        const matchHCenter = (left1 === bestMember && right1 === bestMember);
        const matchVUp = (up1 === bestMember && up2 === bestMember);
        const matchVDown = (down1 === bestMember && down2 === bestMember);
        const matchVCenter = (up1 === bestMember && down1 === bestMember);

        if (!matchHLeft && !matchHRight && !matchHCenter && !matchVUp && !matchVDown && !matchVCenter) {
          break;
        }
      }
      
      const tile = createTile(pos.x, pos.y, bestMember, null);
      board.setTile(pos.x, pos.y, tile);
      spawned.push(pos);
    }
    
    return spawned;
  }

  private getEmptyPlayableCells(board: Board): {x: number, y: number}[] {
    const emptyCells: {x: number, y: number}[] = [];
    for (let y = 0; y < board.rows; y++) {
      for (let x = 0; x < board.cols; x++) {
        if (board.isPlayableCell(x, y) && board.getTile(x, y) === null) {
          emptyCells.push({x, y});
        }
      }
    }
    return emptyCells;
  }
}

