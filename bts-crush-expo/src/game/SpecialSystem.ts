import { MemberType, SpecialType, Tile, createTile } from './Tile';
import { Board } from './Board';
import { Match, MatchType } from './MatchDetector';

export interface ActivationResult {
  affectedTiles: { x: number; y: number }[];
  scoreBonus: number;
  secondarySpecials: { x: number; y: number; special: SpecialType }[];
}

export class SpecialSystem {
  getSpecialForMatch(match: Match): SpecialType | null {
    switch (match.type) {
      case MatchType.MATCH_4_HORIZONTAL:
        return 'mic_horizontal';
      case MatchType.MATCH_4_VERTICAL:
        return 'mic_vertical';
      case MatchType.MATCH_5:
        return 'album_bomb';
      case MatchType.T_SHAPE:
      case MatchType.L_SHAPE:
        return 'fan_heart';
      case MatchType.MATCH_2X2:
        return 'chibi';
      case MatchType.MATCH_6_PLUS:
        return 'dynamite';
      case MatchType.MATCH_3:
      default:
        return null;
    }
  }

  createSpecialTile(x: number, y: number, member: MemberType, special: SpecialType): Tile {
    return createTile(x, y, member, special);
  }

  activateSpecial(board: Board, x: number, y: number): ActivationResult {
    const tile = board.getTile(x, y);
    if (!tile || !tile.special) {
      return { affectedTiles: [], scoreBonus: 0, secondarySpecials: [] };
    }

    const special = tile.special;
    const member = tile.member; // For album_bomb
    const affectedTiles: { x: number; y: number }[] = [];
    const secondarySpecials: { x: number; y: number; special: SpecialType }[] = [];
    let scoreBonus = 0;

    switch (special) {
      case 'mic_horizontal':
        for (let c = 0; c < board.cols; c++) {
          if (board.isPlayableCell(c, y)) affectedTiles.push({ x: c, y });
        }
        scoreBonus = 100;
        break;
      case 'mic_vertical':
        for (let r = 0; r < board.rows; r++) {
          if (board.isPlayableCell(x, r)) affectedTiles.push({ x, y: r });
        }
        scoreBonus = 100;
        break;
      case 'lightstick':
        // Defaulting to horizontal since no direction parameter is passed, per instructions
        for (let c = 0; c < board.cols; c++) {
          if (board.isPlayableCell(c, y)) affectedTiles.push({ x: c, y });
        }
        scoreBonus = 100;
        break;
      case 'album_bomb':
        if (member) {
          for (let r = 0; r < board.rows; r++) {
            for (let c = 0; c < board.cols; c++) {
              const t = board.getTile(c, r);
              if (t && t.member === member) {
                affectedTiles.push({ x: c, y: r });
              }
            }
          }
        }
        scoreBonus = 300;
        break;
      case 'stage_bomb': // X-shape diagonal
        for (let r = 0; r < board.rows; r++) {
          for (let c = 0; c < board.cols; c++) {
            if (Math.abs(r - y) === Math.abs(c - x)) {
              if (board.isPlayableCell(c, r)) affectedTiles.push({ x: c, y: r });
            }
          }
        }
        scoreBonus = 200;
        break;
      case 'chibi': // 3x3 area
        for (let r = Math.max(0, y - 1); r <= Math.min(board.rows - 1, y + 1); r++) {
          for (let c = Math.max(0, x - 1); c <= Math.min(board.cols - 1, x + 1); c++) {
            if (board.isPlayableCell(c, r)) affectedTiles.push({ x: c, y: r });
          }
        }
        scoreBonus = 150;
        break;
      case 'fan_heart': // manhattan distance <= 2
        for (let r = 0; r < board.rows; r++) {
          for (let c = 0; c < board.cols; c++) {
            if (Math.abs(r - y) + Math.abs(c - x) <= 2) {
              if (board.isPlayableCell(c, r)) affectedTiles.push({ x: c, y: r });
            }
          }
        }
        scoreBonus = 200;
        break;
      case 'dynamite': // Cross pattern (row + col)
        for (let c = 0; c < board.cols; c++) {
          if (board.isPlayableCell(c, y)) affectedTiles.push({ x: c, y });
        }
        for (let r = 0; r < board.rows; r++) {
          if (r !== y && board.isPlayableCell(x, r)) affectedTiles.push({ x, y: r });
        }
        scoreBonus = 250;
        break;
      case 'butter': // Horizontal across board
        for (let c = 0; c < board.cols; c++) {
          if (board.isPlayableCell(c, y)) affectedTiles.push({ x: c, y });
        }
        scoreBonus = 100;
        break;
    }

    // Ensure the activated tile itself is included and set to null to avoid infinite loops if it hits itself
    if (!affectedTiles.find(t => t.x === x && t.y === y)) {
      affectedTiles.push({ x, y });
    }

    // Setting affected cells' tiles to null
    for (const pos of affectedTiles) {
      board.setTile(pos.x, pos.y, null);
    }

    return { affectedTiles, scoreBonus, secondarySpecials };
  }
}
