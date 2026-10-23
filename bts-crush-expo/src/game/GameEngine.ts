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
import { ALL_MEMBERS, MemberType } from './Tile';
import { MemberAbilitySystem } from './MemberAbilitySystem';
import { MemberAbilityResult } from './MemberAbilitySystem';
import { getMemberConfig } from '../bts/members';
import { ClubSystem } from './ClubSystem';

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
  private memberAbilitySystem: MemberAbilitySystem;

  private state!: GameState;
  private levelDef!: LevelDefinition;

  // Track member states for XP and abilities
  private memberStates: Record<string, any> = {};
  private clubSystem: ClubSystem;

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
    this.memberAbilitySystem = new MemberAbilitySystem(this.specialSystem);
    this.clubSystem = ClubSystem.getInstance();

    // Initialize member states for all members
    ALL_MEMBERS.forEach(memberId => {
      this.memberStates[memberId] = { uniqueAbilityCooldownRemaining: 0 };
    });
  }

  // Award XP to a member and check for level up
  private awardMemberXP(memberId: string, xpAmount: number): void {
    if (!this.memberStates[memberId]) return;

    const state = this.memberStates[memberId];
    state.xp += xpAmount;

    // Update club contribution for XP earned
    this.clubSystem.updateContribution('xpEarned', xpAmount);

    // Check for level up
    const config = getMemberConfig(memberId as MemberType);
    const xpForNextLevel = this.getXPForNextLevel(state.level, config.baseXPRequired, config.xpGrowthFactor);
    if (state.xp >= xpForNextLevel) {
      state.level++;
      state.xp = state.xp - xpForNextLevel; // Carry over excess XP
      state.xpForNextLevel = this.getXPForNextLevel(state.level, config.baseXPRequired, config.xpGrowthFactor);

      // Trigger level up effect (could add visual/audio feedback here)
      console.log(`Member ${memberId} leveled up to ${state.level}!`);
    }
  }

  // Helper to calculate XP for next level
  private getXPForNextLevel(level: number, baseXP: number, growthFactor: number): number {
    if (level < 1) return baseXP;
    return Math.floor(baseXP * Math.pow(growthFactor, level - 1));
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

    // Initialize member states with XP system
    const memberStates: Record<string, any> = {};
    const membersToInitialize = levelDef.availableMembers && levelDef.availableMembers.length > 0
      ? levelDef.availableMembers
      : ALL_MEMBERS;

    membersToInitialize.forEach(memberId => {
      const config = getMemberConfig(memberId as MemberType);
      memberStates[memberId] = {
        level: 1,
        xp: 0,
        xpForNextLevel: this.getXPForNextLevel(1, config.baseXPRequired, config.xpGrowthFactor),
        isUnlocked: true,
        uniqueAbilityCooldownRemaining: 0
      };
    });

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

    // Store member states in engine for ability system
    this.memberStates = memberStates;
  }

  trySwap(x1: number, y1: number, x2: number, y2: number): boolean {
    if (this.state.phase !== GamePhase.IDLE) return false;
    if (Math.abs(x1 - x2) + Math.abs(y1 - y2) !== 1) return false;
    if (!this.board.isPlayableCell(x1, y1) || !this.board.isPlayableCell(x2, y2)) return false;

    // Get members involved in the swap
    const tile1 = this.board.getTile(x1, y1);
    const tile2 = this.board.getTile(x2, y2);
    const member1 = tile1?.member ?? null;
    const member2 = tile2?.member ?? null;

    // Check if combination swap
    if (this.combinationSystem.isCombination(this.board, x1, y1, x2, y2)) {
      this.state.movesRemaining--;
      this.state.phase = GamePhase.MATCHING;
      const res = this.combinationSystem.executeCombination(this.board, x1, y1, x2, y2);
      this.state.lastMatchedPositions = res.affectedTiles;
      this.scoreSystem.addScore(res.scoreBonus);
      this.state.score = this.scoreSystem.score;
      this.objectiveSystem.onScoreChanged(this.state.score);
      // Update club contribution for score and matches
      this.clubSystem.updateContribution('score', res.scoreBonus);
      // Approximate match count for combination - could be refined based on actual tiles affected
      const matchCount = Math.ceil(res.affectedTiles.length / 3); // Rough estimate
      this.clubSystem.updateContribution('matches', matchCount);
      this.state.phase = GamePhase.CLEARING;

      // Award XP to members involved in combination
      if (member1) this.awardMemberXP(member1, 5);
      if (member2 && member2 !== member1) this.awardMemberXP(member2, 5);

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

    // Award XP to members involved in regular match
    if (member1) this.awardMemberXP(member1, 3);
    if (member2 && member2 !== member1) this.awardMemberXP(member2, 3);

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

    let specialsCreated = 0;
    for (const match of matches) {
      const specialType = this.specialSystem.getSpecialForMatch(match);
      if (specialType) {
        const t = match.tiles[0];
        const newSpecial = this.specialSystem.createSpecialTile(t.x, t.y, match.member, specialType);
        this.board.setTile(t.x, t.y, newSpecial);
        this.objectiveSystem.onSpecialCreated(specialType);
        specialsCreated++;
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

    // Update club contribution for specials created
    this.clubSystem.updateContribution('specialsCreated', specialsCreated);

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
    // Update club contribution for obstacles destroyed
    this.clubSystem.updateContribution('specialsCreated', obsRes.destroyed.length); // Using specialsCreated as a proxy for now
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
    const scoreAdded = scoreGained + matches.length * 100;
    this.scoreSystem.addScore(scoreAdded);
    this.state.score = this.scoreSystem.score;
    this.objectiveSystem.onScoreChanged(this.state.score);
    // Update club contribution for score and matches
    this.clubSystem.updateContribution('score', scoreAdded);
    this.clubSystem.updateContribution('matches', matches.length);
    // Update club contribution for score
    this.clubSystem.updateContribution('score', scoreAdded);
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
        // Update club contribution for level completed
        this.clubSystem.updateContribution('levelsCompleted', 1);
      } else if (this.state.movesRemaining <= 0) {
        this.state.phase = GamePhase.LEVEL_FAILED;
      } else if (!this.matchDetector.hasAnyValidMove(this.board)) {
        this.state.phase = GamePhase.SHUFFLING;
        this.shuffleSystem.shuffle(this.board);
        this.state.phase = GamePhase.IDLE;
      } else {
        this.state.phase = GamePhase.IDLE;
      }

      // Update member ability cooldowns after each move/cascade sequence
      this.memberAbilitySystem.updateCooldowns(this.memberStates);

      // Check for ability activation opportunities (e.g., after significant cascades)
      if (this.state.cascadeCount >= 2) {
        this.checkForAbilityActivation();
      }
    }
  }

  /**
   * Check if any member's ability should activate based on game state
   */
  private checkForAbilityActivation(): void {
    // For now, activate the highest level member's ability that's ready
    // In a full implementation, this would be triggered by player action
    let highestLevelMember: string | null = null;
    let highestLevel = 0;

    for (const memberId in this.memberStates) {
      const state = this.memberStates[memberId];
      if (state.level > highestLevel) {
        highestLevel = state.level;
        highestLevelMember = memberId;
      }
    }

    if (highestLevelMember && this.memberAbilitySystem.isAbilityReady(this.memberStates[highestLevelMember])) {
      // Activate the ability
      const result = this.memberAbilitySystem.useAbility(this.board, highestLevelMember, this.memberStates[highestLevelMember]);
      if (result) {
        // Apply ability effects
        this.applyAbilityResult(result, highestLevelMember);

        // Add score from ability
        this.scoreSystem.addScore(result.scoreBonus);
        this.state.score = this.scoreSystem.score;
        this.objectiveSystem.onScoreChanged(this.state.score);
        // Update club contribution for score from ability
        this.clubSystem.updateContribution('score', result.scoreBonus);

        console.log(`Member ${highestLevelMember}'s ability activated: ${result.description}`);
      }
    }
  }

  /**
   * Apply the results of an ability activation to the game board
   */
  private applyAbilityResult(result: MemberAbilityResult, memberId: string): void {
    // Clear affected tiles
    for (const pos of result.affectedTiles) {
      this.board.setTile(pos.x, pos.y, null);
    }

    // Create special tiles from ability result
    for (const special of result.specialsCreated) {
      // Create the special tile based on type
      let specialTile = null;
      switch (special.type) {
        case 'mic_horizontal':
          specialTile = this.specialSystem.createSpecialTile(special.x, special.y, memberId as MemberType, 'mic_horizontal');
          break;
        case 'mic_vertical':
          specialTile = this.specialSystem.createSpecialTile(special.x, special.y, memberId as MemberType, 'mic_vertical');
          break;
        case 'album_bomb':
          specialTile = this.specialSystem.createSpecialTile(special.x, special.y, memberId as MemberType, 'album_bomb');
          break;
        case 'fan_heart':
          specialTile = this.specialSystem.createSpecialTile(special.x, special.y, memberId as MemberType, 'fan_heart');
          break;
        case 'chibi':
          specialTile = this.specialSystem.createSpecialTile(special.x, special.y, memberId as MemberType, 'chibi');
          break;
        case 'dynamite':
          specialTile = this.specialSystem.createSpecialTile(special.x, special.y, memberId as MemberType, 'dynamite');
          break;
        default:
          // Default to a horizontal mic for unknown types
          specialTile = this.specialSystem.createSpecialTile(special.x, special.y, memberId as MemberType, 'mic_horizontal');
          break;
      }

      if (specialTile) {
        this.board.setTile(special.x, special.y, specialTile);
      }
    }

    // Update club contribution for specials created by abilities
    this.clubSystem.updateContribution('specialsCreated', result.specialsCreated.length);
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
