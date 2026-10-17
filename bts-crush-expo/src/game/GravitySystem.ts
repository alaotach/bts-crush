import { Board } from './Board';

export class GravitySystem {
  applyGravity(board: Board): {from: {x:number, y:number}, to: {x:number, y:number}}[] {
    const movements: {from: {x:number, y:number}, to: {x:number, y:number}}[] = [];
    
    for (let x = 0; x < board.cols; x++) {
      for (let y = board.rows - 1; y >= 0; y--) {
        if (!board.isPlayableCell(x, y)) continue;
        
        if (board.getTile(x, y) === null) {
          for (let yAbove = y - 1; yAbove >= 0; yAbove--) {
            if (!board.isPlayableCell(x, yAbove)) continue;
            
            const tile = board.getTile(x, yAbove);
            if (tile !== null) {
              board.setTile(x, y, tile);
              board.setTile(x, yAbove, null);
              movements.push({from: {x, y: yAbove}, to: {x, y}});
              break;
            }
          }
        }
      }
    }
    
    return movements;
  }

  getEmptyPlayableCells(board: Board): {x: number, y: number}[] {
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
