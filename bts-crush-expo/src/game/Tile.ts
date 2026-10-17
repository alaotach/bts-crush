export type MemberType = 'rm' | 'jin' | 'suga' | 'jhope' | 'jimin' | 'v' | 'jungkook';
export type SpecialType = 'mic_horizontal' | 'mic_vertical' | 'lightstick' | 'album_bomb' | 'stage_bomb' | 'chibi' | 'fan_heart' | 'dynamite' | 'butter';
export type ObstacleType = 'jelly' | 'ice' | 'double_ice' | 'crate' | 'double_crate' | 'chain' | 'locked' | 'frozen' | 'chocolate' | 'bomb';
export type CellType = 'normal' | 'hole' | 'blocked' | 'empty' | 'portal_in' | 'portal_out' | 'conveyor_up' | 'conveyor_down' | 'conveyor_left' | 'conveyor_right';

export interface Tile {
  id: string;
  member: MemberType | null;
  special: SpecialType | null;
  obstacle: ObstacleType | null;
  obstacleHP: number;
  x: number;
  y: number;
}

export interface Cell {
  type: CellType;
  portalTarget: { x: number; y: number } | null;
  tile: Tile | null;
}

let nextTileId = 1;

export function createTile(x: number, y: number, member: MemberType | null, special: SpecialType | null = null): Tile {
  return {
    id: `tile_${nextTileId++}_${Date.now()}`,
    member,
    special,
    obstacle: null,
    obstacleHP: 0,
    x,
    y
  };
}

export function isRegularTile(tile: Tile): boolean {
  return tile.member !== null && tile.special === null;
}

export function isSpecialTile(tile: Tile): boolean {
  return tile.special !== null;
}

export const ALL_MEMBERS: MemberType[] = ['rm', 'jin', 'suga', 'jhope', 'jimin', 'v', 'jungkook'];
