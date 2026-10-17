import { Board } from './Board';
import { GamePhase } from './GamePhase';
import { MatchDetector } from './MatchDetector';
import { GravitySystem } from './GravitySystem';
import { SpawnSystem } from './SpawnSystem';
import { ScoreSystem } from './ScoreSystem';
import { ShuffleSystem } from './ShuffleSystem';
import { SpecialSystem } from './SpecialSystem';
import { CombinationSystem } from './CombinationSystem';
import { ObstacleSystem } from './ObstacleSystem';
import { ObjectiveSystem } from './ObjectiveSystem';
import { GameState, GameStateData } from './GameState';
import { LevelDefinition, parseCellLayout } from '../levels/LevelDefinition';
import { ALL_MEMBERS } from './Tile';

export class GameEngine {
  private board: Board;
  private matchDetector: MatchDetector;
  private gravitySystem: GravitySystem;
  private spawnSystem: SpawnSystem;
  private scoreSystem: ScoreSystem;
  private shuffleSystem: ShuffleSystem;
  private specialSystem: SpecialSystem;
  private combinationSystem: CombinationSystem;
  private obstacleSystem: ObstacleSystem;
  private objectiveSystem: ObjectiveSystem;
  
  private state!: GameState;
  private levelDef!: LevelDefinition;

  constructor() {
    this.board = new Board(0, 0); // Placeholder, will be re-initialized
    this.matchDetector = new MatchDetector();
    this.gravitySystem = new GravitySystem();
    this.spawnSystem = new SpawnSystem([]);
    this.scoreSystem = new ScoreSystem();
    this.shuffleSystem = new ShuffleSystem();
    this.specialSystem = new SpecialSystem();
    this.combinationSystem = new CombinationSystem(this.specialSystem);
    this.obstacleSystem = new ObstacleSystem();
    this.objectiveSystem = new ObjectiveSystem([]);
  }

  initLevel(levelDef: LevelDefinition): void {
    this.levelDef = levelDef;
    this.board = new Board(levelDef.rows, levelDef.cols);
    
    for (let r = 0; r < levelDef.rows; r++) {
      for (let c = 0; c < levelDef.cols; c++) {
        const layoutVal = levelDef.layout[r][c];
        const parsed = parseCellLayout(layoutVal);
        const cell = this.board.getCell(c, r);
        if (cell) {
          cell.type = parsed.cellType;
        }
      }
    }

    const members = levelDef.availableMembers && levelDef.availableMembers.length > 0 
      ? levelDef.availableMembers 
      : ALL_MEMBERS;
    
    this.spawnSystem = new SpawnSystem(members);
    this.spawnSystem.fillBoardInitial(this.board, this.matchDetector);
    
    this.objectiveSystem = new ObjectiveSystem(levelDef.objectives);

    this.state = new GameState({
      board: this.board,
      phase: GamePhase.IDLE,
      score: 0,
      combo: 0,
      movesRemaining: levelDef.moves || 0,
      timeRemaining: levelDef.timeLimit || null,
      objectives: this.objectiveSystem.getProgress(),
      cascadeCount: 0,
      selectedTile: null,
      swapTarget: null,
      lastMatchedPositions: [],
      lastFallAnimations: [],
      lastSpawnedPositions: [],
      pendingSpecialActivations: [],
      comboFeedback: null,
      levelId: levelDef.id
    });
  }

  trySwap(x1: number, y1: number, x2: number, y2: number): boolean {
    if (this.state.phase !== GamePhase.IDLE) return false;
    if (Math.abs(x1 - x2) + Math.abs(y1 - y2) !== 1) return false;
    if (!this.board.isPlayableCell(x1, y1) || !this.board.isPlayableCell(x2, y2)) return false;

    if (this.combinationSystem.isCombination(this.board, x1, y1, x2, y2)) {
      this.state.movesRemaining--;
      this.state.phase = GamePhase.MATCHING;
      const res = this.combinationSystem.executeCombination(this.board, x1, y1, x2, y2);
      this.state.lastMatchedPositions = res.affectedTiles;
      this.scoreSystem.addScore(res.scoreBonus);
      this.state.score = this.scoreSystem.score;
      this.objectiveSystem.onScoreChanged(this.state.score);
      this.state.phase = GamePhase.CLEARING;
      this.afterClearing();
      return true;
    }

    this.board.swapTiles(x1, y1, x2, y2);
    const matches = this.matchDetector.findAllMatches(this.board);

    if (matches.length === 0) {
      this.state.phase = GamePhase.SWAP_ROLLBACK;
      this.board.swapTiles(x1, y1, x2, y2);
      this.state.phase = GamePhase.IDLE;
      return false;
    }

    this.state.movesRemaining--;
    this.state.phase = GamePhase.MATCHING;
    this.resolveMatches();
    return true;
  }

  resolveMatches(): void {
    let scoreGained = 0;
    const clearedTiles: { member: string | null }[] = [];
    const positionsToClear = new Set<string>();
    const activationQueue: {x: number, y: number}[] = [];

    // Find initial matches
    const matches = this.matchDetector.findAllMatches(this.board);

    // If album bomb was swapped with a normal tile, trySwap would have placed it here?
    // Wait, trySwap handles combinations, but album_bomb + normal tile is a special case.
    
    for (const match of matches) {
      const specialType = this.specialSystem.getSpecialForMatch(match);
      if (specialType) {
        const t = match.tiles[0];
        const newSpecial = this.specialSystem.createSpecialTile(t.x, t.y, match.member, specialType);
        this.board.setTile(t.x, t.y, newSpecial);
        this.objectiveSystem.onSpecialCreated(specialType);
      }

      for (const t of match.tiles) {
        const tile = this.board.getTile(t.x, t.y);
        // If the tile is ALREADY a special (and not the one we just created), queue it for activation
        if (tile && tile.special && !(t.x === match.tiles[0].x && t.y === match.tiles[0].y && specialType)) {
          activationQueue.push({x: t.x, y: t.y});
        }
        positionsToClear.add(`${t.x},${t.y}`);
      }
    }

    // Process activation queue (cascading specials)
    while (activationQueue.length > 0) {
      const pos = activationQueue.shift()!;
      const tile = this.board.getTile(pos.x, pos.y);
      if (tile && tile.special) {
        this.objectiveSystem.onSpecialActivated(tile.special);
        const res = this.specialSystem.activateSpecial(this.board, pos.x, pos.y);
        scoreGained += res.scoreBonus;
        
        for (const affected of res.affectedTiles) {
          const key = `${affected.x},${affected.y}`;
          if (!positionsToClear.has(key)) {
            positionsToClear.add(key);
            const affectedTile = this.board.getTile(affected.x, affected.y);
            if (affectedTile && affectedTile.special) {
              activationQueue.push({x: affected.x, y: affected.y});
            }
          }
        }
      }
    }

    const arrPositions = Array.from(positionsToClear).map(s => {
      const parts = s.split(',');
      return { x: parseInt(parts[0], 10), y: parseInt(parts[1], 10) };
    });

    const obsRes = this.obstacleSystem.processAdjacentMatch(this.board, arrPositions);
    scoreGained += obsRes.scoreBonus;
    for (const d of obsRes.destroyed) {
      this.objectiveSystem.onObstacleDestroyed(d.type);
    }

    for (const pos of arrPositions) {
      const tile = this.board.getTile(pos.x, pos.y);
      if (tile) {
        clearedTiles.push({ member: tile.member });
        // Don't clear if it was JUST turned into a special this turn
        const isNewSpecial = matches.some(m => m.tiles[0].x === pos.x && m.tiles[0].y === pos.y && this.specialSystem.getSpecialForMatch(m) !== null);
        if (!isNewSpecial) {
           this.board.setTile(pos.x, pos.y, null);
        }
      }
    }

    this.objectiveSystem.onTilesCleared(clearedTiles as any);
    this.scoreSystem.addScore(scoreGained + matches.length * 100); 
    this.state.score = this.scoreSystem.score;
    this.objectiveSystem.onScoreChanged(this.state.score);
    this.state.lastMatchedPositions = arrPositions;
    this.state.phase = GamePhase.CLEARING;
    this.afterClearing();
  }

  afterClearing(): void {
    this.state.phase = GamePhase.FALLING;
    const falls = this.gravitySystem.applyGravity(this.board);
    this.state.lastFallAnimations = falls;

    this.state.phase = GamePhase.SPAWNING;
    const spawns = this.spawnSystem.spawnTiles(this.board);
    this.state.lastSpawnedPositions = spawns;

    this.state.phase = GamePhase.CASCADE;
    if (this.matchDetector.hasAnyMatch(this.board)) {
      this.state.cascadeCount++;
      this.objectiveSystem.onCascade(this.state.cascadeCount);
      this.state.comboFeedback = this.getComboFeedback();
      // YIELD to renderer here. The GameScreen will call engine.resolveMatches() on the next tick!
    } else {
      this.state.cascadeCount = 0;
      this.state.comboFeedback = null;
      this.state.phase = GamePhase.OBJECTIVE_CHECK;
      
      this.state.objectives = this.objectiveSystem.getProgress();
      
      if (this.objectiveSystem.isAllCompleted()) {
        this.state.phase = GamePhase.LEVEL_COMPLETE;
      } else if (this.state.movesRemaining <= 0) {
        this.state.phase = GamePhase.LEVEL_FAILED;
      } else if (!this.matchDetector.hasAnyValidMove(this.board)) {
        this.state.phase = GamePhase.SHUFFLING;
        this.shuffleSystem.shuffle(this.board);
        this.state.phase = GamePhase.IDLE;
      } else {
        this.state.phase = GamePhase.IDLE;
      }
    }
  }

  getState(): GameStateData {
    return this.state.data;
  }

  pause(): void {
    if (this.state.phase === GamePhase.IDLE) {
      this.state.phase = GamePhase.PAUSED;
    }
  }

  resume(): void {
    if (this.state.phase === GamePhase.PAUSED) {
      this.state.phase = GamePhase.IDLE;
    }
  }

  getComboFeedback(): string | null {
    if (this.state.cascadeCount >= 5) return 'ENCORE';
    if (this.state.cascadeCount >= 4) return 'AMAZING';
    if (this.state.cascadeCount >= 3) return 'GREAT';
    if (this.state.cascadeCount >= 2) return 'GOOD';
    return null;
  }
}
