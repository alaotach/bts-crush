import { Board } from './Board';
import { MemberType, Tile } from './Tile';

export enum MatchType {
  MATCH_3 = 'MATCH_3',
  MATCH_4_HORIZONTAL = 'MATCH_4_HORIZONTAL',
  MATCH_4_VERTICAL = 'MATCH_4_VERTICAL',
  MATCH_5 = 'MATCH_5',
  MATCH_6_PLUS = 'MATCH_6_PLUS',
  T_SHAPE = 'T_SHAPE',
  L_SHAPE = 'L_SHAPE',
  MATCH_2X2 = 'MATCH_2X2'
}

export interface Match {
  tiles: { x: number; y: number }[];
  type: MatchType;
  member: MemberType;
}

export class MatchDetector {
  findAllMatches(board: Board): Match[] {
    const matches: Match[] = [];
    
    const horizRuns = this.findHorizontalRuns(board);
    const vertRuns = this.findVerticalRuns(board);
    
    const allRuns = [...horizRuns, ...vertRuns];
    const grouped = this.groupOverlappingRuns(allRuns);
    
    for (const group of grouped) {
      const { tiles, member } = this.mergeGroup(group);
      const isHorizInGroup = group.some(r => r.dir === 'h');
      const isVertInGroup = group.some(r => r.dir === 'v');
      const maxLenH = isHorizInGroup ? Math.max(...group.filter(r => r.dir === 'h').map(r => r.tiles.length)) : 0;
      const maxLenV = isVertInGroup ? Math.max(...group.filter(r => r.dir === 'v').map(r => r.tiles.length)) : 0;

      let type = MatchType.MATCH_3;
      
      if (tiles.length >= 6) {
        type = MatchType.MATCH_6_PLUS;
      } else if (maxLenH >= 5 || maxLenV >= 5) {
        type = MatchType.MATCH_5;
      } else if (isHorizInGroup && isVertInGroup) {
        if (maxLenH === 3 && maxLenV === 3 && tiles.length === 5) {
            const hRun = group.find(r => r.dir === 'h')!;
            const vRun = group.find(r => r.dir === 'v')!;
            const intersect = hRun.tiles.find(t1 => vRun.tiles.some(t2 => t1.x === t2.x && t1.y === t2.y));
            if (intersect) {
                const isHEnd = intersect.x === hRun.tiles[0].x || intersect.x === hRun.tiles[hRun.tiles.length-1].x;
                const isVEnd = intersect.y === vRun.tiles[0].y || intersect.y === vRun.tiles[vRun.tiles.length-1].y;
                if (isHEnd && isVEnd) type = MatchType.L_SHAPE;
                else type = MatchType.T_SHAPE;
            } else {
                type = MatchType.T_SHAPE;
            }
        } else {
            type = MatchType.T_SHAPE;
        }
      } else if (maxLenH === 4) {
        type = MatchType.MATCH_4_HORIZONTAL;
      } else if (maxLenV === 4) {
        type = MatchType.MATCH_4_VERTICAL;
      }

      matches.push({ tiles, type, member });
    }

    const squares2x2 = this.find2x2Matches(board);
    for (const sq of squares2x2) {
      const overlap = matches.some(m => 
        m.tiles.some(t1 => sq.tiles.some(t2 => t1.x === t2.x && t1.y === t2.y))
      );
      if (!overlap) {
         matches.push(sq);
      }
    }

    return matches;
  }

  private findHorizontalRuns(board: Board) {
    const runs: {tiles: {x:number,y:number}[], member: MemberType, dir: string}[] = [];
    for (let y = 0; y < board.rows; y++) {
      let currentRun: {x:number,y:number}[] = [];
      let currentMember: MemberType | null = null;
      for (let x = 0; x < board.cols; x++) {
        const tile = board.getTile(x, y);
        if (tile && tile.member) {
          if (tile.member === currentMember) {
            currentRun.push({x, y});
          } else {
            if (currentRun.length >= 3 && currentMember) {
              runs.push({tiles: currentRun, member: currentMember, dir: 'h'});
            }
            currentMember = tile.member;
            currentRun = [{x, y}];
          }
        } else {
          if (currentRun.length >= 3 && currentMember) {
            runs.push({tiles: currentRun, member: currentMember, dir: 'h'});
          }
          currentMember = null;
          currentRun = [];
        }
      }
      if (currentRun.length >= 3 && currentMember) {
        runs.push({tiles: currentRun, member: currentMember, dir: 'h'});
      }
    }
    return runs;
  }

  private findVerticalRuns(board: Board) {
    const runs: {tiles: {x:number,y:number}[], member: MemberType, dir: string}[] = [];
    for (let x = 0; x < board.cols; x++) {
      let currentRun: {x:number,y:number}[] = [];
      let currentMember: MemberType | null = null;
      for (let y = 0; y < board.rows; y++) {
        const tile = board.getTile(x, y);
        if (tile && tile.member) {
          if (tile.member === currentMember) {
            currentRun.push({x, y});
          } else {
            if (currentRun.length >= 3 && currentMember) {
              runs.push({tiles: currentRun, member: currentMember, dir: 'v'});
            }
            currentMember = tile.member;
            currentRun = [{x, y}];
          }
        } else {
          if (currentRun.length >= 3 && currentMember) {
            runs.push({tiles: currentRun, member: currentMember, dir: 'v'});
          }
          currentMember = null;
          currentRun = [];
        }
      }
      if (currentRun.length >= 3 && currentMember) {
        runs.push({tiles: currentRun, member: currentMember, dir: 'v'});
      }
    }
    return runs;
  }

  private groupOverlappingRuns(runs: {tiles: {x:number,y:number}[], member: MemberType, dir: string}[]) {
    const groups: typeof runs[] = [];
    const used = new Set<number>();
    
    for (let i = 0; i < runs.length; i++) {
      if (used.has(i)) continue;
      
      const group = [runs[i]];
      used.add(i);
      
      let added = true;
      while (added) {
        added = false;
        for (let j = 0; j < runs.length; j++) {
          if (used.has(j)) continue;
          
          const runToTest = runs[j];
          if (runToTest.member !== group[0].member) continue;
          
          const overlaps = group.some(gRun => 
            gRun.tiles.some(t1 => runToTest.tiles.some(t2 => t1.x === t2.x && t1.y === t2.y))
          );
          
          if (overlaps) {
            group.push(runToTest);
            used.add(j);
            added = true;
          }
        }
      }
      groups.push(group);
    }
    return groups;
  }

  private mergeGroup(group: {tiles: {x:number,y:number}[], member: MemberType, dir: string}[]) {
    const tiles: {x:number,y:number}[] = [];
    const member = group[0].member;
    const seen = new Set<string>();
    
    for (const run of group) {
      for (const t of run.tiles) {
        const key = `${t.x},${t.y}`;
        if (!seen.has(key)) {
          seen.add(key);
          tiles.push(t);
        }
      }
    }
    return { tiles, member };
  }

  private find2x2Matches(board: Board): Match[] {
    const matches: Match[] = [];
    for (let y = 0; y < board.rows - 1; y++) {
      for (let x = 0; x < board.cols - 1; x++) {
        const t1 = board.getTile(x, y);
        const t2 = board.getTile(x + 1, y);
        const t3 = board.getTile(x, y + 1);
        const t4 = board.getTile(x + 1, y + 1);
        
        if (t1 && t2 && t3 && t4 && 
            t1.member && t1.member === t2.member && 
            t1.member === t3.member && t1.member === t4.member) {
          matches.push({
            tiles: [
              {x: t1.x, y: t1.y},
              {x: t2.x, y: t2.y},
              {x: t3.x, y: t3.y},
              {x: t4.x, y: t4.y}
            ],
            type: MatchType.MATCH_2X2,
            member: t1.member
          });
        }
      }
    }
    return matches;
  }

  hasAnyMatch(board: Board): boolean {
    return this.findAllMatches(board).length > 0;
  }

  hasAnyValidMove(board: Board): boolean {
    const b = board.clone();
    for (let y = 0; y < b.rows; y++) {
      for (let x = 0; x < b.cols; x++) {
        if (!b.isPlayableCell(x, y)) continue;
        
        if (x < b.cols - 1 && b.isPlayableCell(x + 1, y)) {
          b.swapTiles(x, y, x + 1, y);
          if (this.hasAnyMatch(b)) return true;
          b.swapTiles(x, y, x + 1, y);
        }
        
        if (y < b.rows - 1 && b.isPlayableCell(x, y + 1)) {
          b.swapTiles(x, y, x, y + 1);
          if (this.hasAnyMatch(b)) return true;
          b.swapTiles(x, y, x, y + 1);
        }
      }
    }
    return false;
  }
}
