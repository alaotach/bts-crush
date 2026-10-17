import { Cell, CellType, MemberType, Tile } from './Tile';

export class Board {
  private grid: Cell[][];
  private _rows: number;
  private _cols: number;

  constructor(rows: number, cols: number, layout?: CellType[][]) {
    this._rows = rows;
    this._cols = cols;
    this.grid = [];

    for (let r = 0; r < rows; r++) {
      const row: Cell[] = [];
      for (let c = 0; c < cols; c++) {
        row.push({
          type: (layout && layout[r] && layout[r][c]) ? layout[r][c] : 'normal',
          portalTarget: null,
          tile: null
        });
      }
      this.grid.push(row);
    }
  }

  get rows(): number {
    return this._rows;
  }

  get cols(): number {
    return this._cols;
  }

  getCell(x: number, y: number): Cell | null {
    if (this.isValidPosition(x, y)) {
      return this.grid[y][x];
    }
    return null;
  }

  getTile(x: number, y: number): Tile | null {
    const cell = this.getCell(x, y);
    return cell ? cell.tile : null;
  }

  setTile(x: number, y: number, tile: Tile | null): void {
    const cell = this.getCell(x, y);
    if (cell) {
      cell.tile = tile;
      if (tile) {
        tile.x = x;
        tile.y = y;
      }
    }
  }

  swapTiles(x1: number, y1: number, x2: number, y2: number): void {
    const tile1 = this.getTile(x1, y1);
    const tile2 = this.getTile(x2, y2);
    
    this.setTile(x1, y1, tile2);
    this.setTile(x2, y2, tile1);
  }

  isValidPosition(x: number, y: number): boolean {
    return x >= 0 && x < this._cols && y >= 0 && y < this._rows;
  }

  isPlayableCell(x: number, y: number): boolean {
    const cell = this.getCell(x, y);
    return cell !== null && cell.type === 'normal';
  }

  getNeighbors(x: number, y: number): {x: number, y: number}[] {
    const neighbors: {x: number, y: number}[] = [];
    const dirs = [[0, -1], [0, 1], [-1, 0], [1, 0]];
    for (const [dx, dy] of dirs) {
      const nx = x + dx;
      const ny = y + dy;
      if (this.isValidPosition(nx, ny) && this.isPlayableCell(nx, ny)) {
        neighbors.push({ x: nx, y: ny });
      }
    }
    return neighbors;
  }

  isAdjacent(x1: number, y1: number, x2: number, y2: number): boolean {
    return (Math.abs(x1 - x2) === 1 && y1 === y2) || (Math.abs(y1 - y2) === 1 && x1 === x2);
  }

  forEachCell(callback: (cell: Cell, x: number, y: number) => void): void {
    for (let y = 0; y < this._rows; y++) {
      for (let x = 0; x < this._cols; x++) {
        callback(this.grid[y][x], x, y);
      }
    }
  }

  clone(): Board {
    const layout = this.grid.map(row => row.map(cell => cell.type));
    const newBoard = new Board(this._rows, this._cols, layout);
    
    this.forEachCell((cell, x, y) => {
      if (cell.tile) {
        newBoard.setTile(x, y, { ...cell.tile });
      }
      if (cell.portalTarget) {
        const newCell = newBoard.getCell(x, y);
        if (newCell) {
          newCell.portalTarget = { ...cell.portalTarget };
        }
      }
    });
    
    return newBoard;
  }

  toGrid(): (MemberType | null)[][] {
    return this.grid.map(row => row.map(cell => cell.tile ? cell.tile.member : null));
  }
}
