import { Board } from './Board';
import { SpecialSystem } from './SpecialSystem';

export interface CombinationResult {
  affectedTiles: { x: number; y: number }[];
  scoreBonus: number;
  effectType: string;
}

export class CombinationSystem {
  constructor(private specialSystem: SpecialSystem) {}

  isCombination(board: Board, x1: number, y1: number, x2: number, y2: number): boolean {
    const tile1 = board.getTile(x1, y1);
    const tile2 = board.getTile(x2, y2);
    return tile1 !== null && tile2 !== null && tile1.special !== null && tile2.special !== null;
  }

  executeCombination(board: Board, x1: number, y1: number, x2: number, y2: number): CombinationResult {
    const tile1 = board.getTile(x1, y1)!;
    const tile2 = board.getTile(x2, y2)!;
    const s1 = tile1.special!;
    const s2 = tile2.special!;
    
    // Sort to handle order-agnostic logic
    const specials = [s1, s2].sort();
    const isCombo = (a: string, b: string) => specials[0] === a && specials[1] === b;
    const isMicCombo = (a: string) => specials[0].startsWith('mic_') && specials[1].startsWith('mic_');

    const affectedTiles: { x: number; y: number }[] = [];
    let scoreBonus = 0;
    let effectType = 'normal';

    const addTile = (c: number, r: number) => {
      if (board.isPlayableCell(c, r) && !affectedTiles.find(t => t.x === c && t.y === r)) {
        affectedTiles.push({ x: c, y: r });
      }
    };

    if (isMicCombo('mic_horizontal') || isMicCombo('mic_vertical') || (specials[0].startsWith('mic_') && specials[1].startsWith('mic_'))) {
      // mic + mic
      effectType = 'mic_mic';
      for (let c = 0; c < board.cols; c++) {
        addTile(c, y1);
        addTile(c, y2);
      }
      for (let r = 0; r < board.rows; r++) {
        addTile(x1, r);
        addTile(x2, r);
      }
      scoreBonus = 500;
    } else if ((s1 === 'album_bomb' && s2.startsWith('mic_')) || (s2 === 'album_bomb' && s1.startsWith('mic_'))) {
      // mic + album_bomb
      effectType = 'mic_album';
      const micTile = s1 === 'album_bomb' ? tile2 : tile1;
      const targetMember = micTile.member;
      if (targetMember) {
        for (let r = 0; r < board.rows; r++) {
          for (let c = 0; c < board.cols; c++) {
            const t = board.getTile(c, r);
            if (t && t.member === targetMember) {
              // Convert to mic and add to affected (simulate activation)
              addTile(c, r);
              // Also add line clears for these newly spawned mics (simplified)
              for (let ic = 0; ic < board.cols; ic++) addTile(ic, r);
              for (let ir = 0; ir < board.rows; ir++) addTile(c, ir);
            }
          }
        }
      }
      scoreBonus = 1000;
    } else if ((s1 === 'chibi' && s2.startsWith('mic_')) || (s2 === 'chibi' && s1.startsWith('mic_'))) {
      // mic + chibi
      effectType = 'mic_chibi';
      for (let r = Math.max(0, y1 - 1); r <= Math.min(board.rows - 1, y1 + 1); r++) {
        for (let c = Math.max(0, x1 - 1); c <= Math.min(board.cols - 1, x1 + 1); c++) {
          addTile(c, r);
          for (let ic = 0; ic < board.cols; ic++) addTile(ic, r);
          for (let ir = 0; ir < board.rows; ir++) addTile(c, ir);
        }
      }
      scoreBonus = 600;
    } else if ((s1 === 'dynamite' && s2.startsWith('mic_')) || (s2 === 'dynamite' && s1.startsWith('mic_'))) {
      // mic + dynamite
      effectType = 'mic_dynamite';
      for (let r = Math.max(0, y1 - 1); r <= Math.min(board.rows - 1, y1 + 1); r++) {
        for (let c = 0; c < board.cols; c++) addTile(c, r);
      }
      for (let c = Math.max(0, x1 - 1); c <= Math.min(board.cols - 1, x1 + 1); c++) {
        for (let r = 0; r < board.rows; r++) addTile(c, r);
      }
      scoreBonus = 800;
    } else if (isCombo('album_bomb', 'album_bomb')) {
      // album + album
      effectType = 'album_album';
      for (let r = 0; r < board.rows; r++) {
        for (let c = 0; c < board.cols; c++) {
          addTile(c, r);
        }
      }
      scoreBonus = 2000;
    } else if (isCombo('album_bomb', 'dynamite')) {
      // album + dynamite
      effectType = 'album_dynamite';
      const dynaTile = s1 === 'album_bomb' ? tile2 : tile1;
      const targetMember = dynaTile.member;
      if (targetMember) {
        for (let r = 0; r < board.rows; r++) {
          for (let c = 0; c < board.cols; c++) {
            const t = board.getTile(c, r);
            if (t && t.member === targetMember) {
              addTile(c, r);
              // Dynamite cross for each
              for (let ic = 0; ic < board.cols; ic++) addTile(ic, r);
              for (let ir = 0; ir < board.rows; ir++) addTile(c, ir);
            }
          }
        }
      }
      scoreBonus = 1200;
    } else if (isCombo('chibi', 'chibi')) {
      // chibi + chibi
      effectType = 'chibi_chibi';
      for (let r = Math.max(0, y1 - 2); r <= Math.min(board.rows - 1, y1 + 2); r++) {
        for (let c = Math.max(0, x1 - 2); c <= Math.min(board.cols - 1, x1 + 2); c++) {
          addTile(c, r);
        }
      }
      scoreBonus = 800;
    } else if (isCombo('dynamite', 'dynamite')) {
      // dynamite + dynamite
      effectType = 'dynamite_dynamite';
      for (let r = Math.max(0, y1 - 2); r <= Math.min(board.rows - 1, y1 + 2); r++) {
        for (let c = 0; c < board.cols; c++) addTile(c, r);
      }
      for (let c = Math.max(0, x1 - 2); c <= Math.min(board.cols - 1, x1 + 2); c++) {
        for (let r = 0; r < board.rows; r++) addTile(c, r);
      }
      scoreBonus = 1000;
    } else if ((s1 === 'fan_heart' && s2.startsWith('mic_')) || (s2 === 'fan_heart' && s1.startsWith('mic_'))) {
      // fan_heart + mic
      effectType = 'heart_mic';
      for (let r = 0; r < board.rows; r++) {
        for (let c = 0; c < board.cols; c++) {
          if (Math.abs(r - y1) + Math.abs(c - x1) <= 2) {
            addTile(c, r);
          }
        }
      }
      for (let c = 0; c < board.cols; c++) addTile(c, y1);
      for (let r = 0; r < board.rows; r++) addTile(x1, r);
      scoreBonus = 700;
    } else if (isCombo('fan_heart', 'fan_heart')) {
      // fan_heart + fan_heart
      effectType = 'heart_heart';
      for (let r = 0; r < board.rows; r++) {
        for (let c = 0; c < board.cols; c++) {
          if (Math.abs(r - y1) + Math.abs(c - x1) <= 4) {
            addTile(c, r);
          }
        }
      }
      scoreBonus = 900;
    } else {
      // Any other combo
      effectType = 'dual_normal';
      const res1 = this.specialSystem.activateSpecial(board, x1, y1);
      const res2 = this.specialSystem.activateSpecial(board, x2, y2);
      for (const t of res1.affectedTiles) addTile(t.x, t.y);
      for (const t of res2.affectedTiles) addTile(t.x, t.y);
      scoreBonus = res1.scoreBonus + res2.scoreBonus;
    }

    addTile(x1, y1);
    addTile(x2, y2);

    for (const pos of affectedTiles) {
      board.setTile(pos.x, pos.y, null);
    }

    return { affectedTiles, scoreBonus, effectType };
  }
}
