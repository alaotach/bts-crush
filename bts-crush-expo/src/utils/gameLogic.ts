import { BTSMember, ALL_MEMBERS } from '../constants/btsAssets';

export interface TileData {
  id: string;
  member: BTSMember;
  x: number;
  y: number;
  markedForClear: boolean;
  isSpecial?: 'horizontal' | 'vertical' | 'bomb' | 'rainbow';
}

export const BOARD_WIDTH = 6;
export const BOARD_HEIGHT = 8;

export function generateInitialBoard(): TileData[][] {
  const board: TileData[][] = [];
  for (let y = 0; y < BOARD_HEIGHT; y++) {
    const row: TileData[] = [];
    for (let x = 0; x < BOARD_WIDTH; x++) {
      let newTile: TileData;
      // Ensure no initial matches
      do {
        newTile = createRandomTile(x, y);
      } while (
        (x >= 2 && row[x - 1].member === newTile.member && row[x - 2].member === newTile.member) ||
        (y >= 2 && board[y - 1][x].member === newTile.member && board[y - 2][x].member === newTile.member)
      );
      row.push(newTile);
    }
    board.push(row);
  }
  return board;
}

export function createRandomTile(x: number, y: number): TileData {
  const randomMember = ALL_MEMBERS[Math.floor(Math.random() * ALL_MEMBERS.length)].id;
  return {
    id: `tile_${Math.random().toString(36).substr(2, 9)}`,
    member: randomMember,
    x,
    y,
    markedForClear: false,
  };
}

export function swapTiles(board: TileData[][], x1: number, y1: number, x2: number, y2: number): TileData[][] {
  const newBoard = board.map(row => [...row]);
  
  const tile1 = newBoard[y1][x1];
  const tile2 = newBoard[y2][x2];

  newBoard[y1][x1] = { ...tile2, x: x1, y: y1 };
  newBoard[y2][x2] = { ...tile1, x: x2, y: y2 };

  return newBoard;
}

export interface MatchResult {
  hasMatches: boolean;
  newBoard: TileData[][];
  clearedCount: number;
}

export function processMatches(board: TileData[][]): MatchResult {
  const newBoard = board.map(row => row.map(tile => ({ ...tile })));
  let hasMatches = false;
  let clearedCount = 0;

  // Horizontal check
  for (let y = 0; y < BOARD_HEIGHT; y++) {
    for (let x = 0; x < BOARD_WIDTH - 2; x++) {
      const type = newBoard[y][x].member;
      if (!type) continue;
      
      let matchLength = 1;
      while (x + matchLength < BOARD_WIDTH && newBoard[y][x + matchLength].member === type) {
        matchLength++;
      }

      if (matchLength >= 3) {
        hasMatches = true;
        for (let i = 0; i < matchLength; i++) {
          newBoard[y][x + i].markedForClear = true;
          clearedCount++;
        }
      }
      x += matchLength - 1; // Skip ahead
    }
  }

  // Vertical check
  for (let x = 0; x < BOARD_WIDTH; x++) {
    for (let y = 0; y < BOARD_HEIGHT - 2; y++) {
      const type = newBoard[y][x].member;
      if (!type) continue;

      let matchLength = 1;
      while (y + matchLength < BOARD_HEIGHT && newBoard[y + matchLength][x].member === type) {
        matchLength++;
      }

      if (matchLength >= 3) {
        hasMatches = true;
        for (let i = 0; i < matchLength; i++) {
          newBoard[y + i][x].markedForClear = true;
          clearedCount++;
        }
      }
      y += matchLength - 1; // Skip ahead
    }
  }

  return { hasMatches, newBoard, clearedCount };
}

export function applyGravity(board: TileData[][]): TileData[][] {
  const newBoard = board.map(row => [...row]);

  for (let x = 0; x < BOARD_WIDTH; x++) {
    let emptySlots = 0;
    
    // Check from bottom to top
    for (let y = BOARD_HEIGHT - 1; y >= 0; y--) {
      if (newBoard[y][x].markedForClear) {
        emptySlots++;
      } else if (emptySlots > 0) {
        // Move tile down
        newBoard[y + emptySlots][x] = { ...newBoard[y][x], x, y: y + emptySlots };
        newBoard[y][x] = { ...newBoard[y][x], markedForClear: true }; // Mark old spot as clear to be filled later
      }
    }

    // Refill top empty slots
    for (let y = 0; y < emptySlots; y++) {
      newBoard[y][x] = createRandomTile(x, y);
    }
  }

  return newBoard;
}
