import { Board } from './Board';
import { GamePhase } from './GamePhase';
import { ObjectiveProgress } from './ObjectiveSystem';

export interface GameStateData {
  board: Board;
  phase: GamePhase;
  score: number;
  combo: number;
  movesRemaining: number;
  timeRemaining: number | null;
  objectives: ObjectiveProgress[];
  cascadeCount: number;
  selectedTile: { x: number; y: number } | null;
  swapTarget: { x: number; y: number } | null;
  lastMatchedPositions: { x: number; y: number }[];
  lastFallAnimations: { from: { x: number; y: number }; to: { x: number; y: number } }[];
  lastSpawnedPositions: { x: number; y: number }[];
  pendingSpecialActivations: { x: number; y: number }[];
  comboFeedback: string | null;
  levelId: number;
}

export class GameState {
  data: GameStateData;

  constructor(data: GameStateData) {
    this.data = data;
  }

  clone(): GameState {
    return new GameState({
      ...this.data,
      board: this.data.board.clone(),
      objectives: JSON.parse(JSON.stringify(this.data.objectives)), // Deep copy for progress
      selectedTile: this.data.selectedTile ? { ...this.data.selectedTile } : null,
      swapTarget: this.data.swapTarget ? { ...this.data.swapTarget } : null,
      lastMatchedPositions: [...this.data.lastMatchedPositions],
      lastFallAnimations: [...this.data.lastFallAnimations],
      lastSpawnedPositions: [...this.data.lastSpawnedPositions],
      pendingSpecialActivations: [...this.data.pendingSpecialActivations]
    });
  }

  get board(): Board { return this.data.board; }
  set board(v: Board) { this.data.board = v; }

  get phase(): GamePhase { return this.data.phase; }
  set phase(v: GamePhase) { this.data.phase = v; }

  get score(): number { return this.data.score; }
  set score(v: number) { this.data.score = v; }

  get combo(): number { return this.data.combo; }
  set combo(v: number) { this.data.combo = v; }

  get movesRemaining(): number { return this.data.movesRemaining; }
  set movesRemaining(v: number) { this.data.movesRemaining = v; }

  get timeRemaining(): number | null { return this.data.timeRemaining; }
  set timeRemaining(v: number | null) { this.data.timeRemaining = v; }

  get objectives(): ObjectiveProgress[] { return this.data.objectives; }
  set objectives(v: ObjectiveProgress[]) { this.data.objectives = v; }

  get cascadeCount(): number { return this.data.cascadeCount; }
  set cascadeCount(v: number) { this.data.cascadeCount = v; }

  get selectedTile(): { x: number; y: number } | null { return this.data.selectedTile; }
  set selectedTile(v: { x: number; y: number } | null) { this.data.selectedTile = v; }

  get swapTarget(): { x: number; y: number } | null { return this.data.swapTarget; }
  set swapTarget(v: { x: number; y: number } | null) { this.data.swapTarget = v; }

  get lastMatchedPositions(): { x: number; y: number }[] { return this.data.lastMatchedPositions; }
  set lastMatchedPositions(v: { x: number; y: number }[]) { this.data.lastMatchedPositions = v; }

  get lastFallAnimations(): { from: { x: number; y: number }; to: { x: number; y: number } }[] { return this.data.lastFallAnimations; }
  set lastFallAnimations(v: { from: { x: number; y: number }; to: { x: number; y: number } }[]) { this.data.lastFallAnimations = v; }

  get lastSpawnedPositions(): { x: number; y: number }[] { return this.data.lastSpawnedPositions; }
  set lastSpawnedPositions(v: { x: number; y: number }[]) { this.data.lastSpawnedPositions = v; }

  get pendingSpecialActivations(): { x: number; y: number }[] { return this.data.pendingSpecialActivations; }
  set pendingSpecialActivations(v: { x: number; y: number }[]) { this.data.pendingSpecialActivations = v; }

  get comboFeedback(): string | null { return this.data.comboFeedback; }
  set comboFeedback(v: string | null) { this.data.comboFeedback = v; }

  get levelId(): number { return this.data.levelId; }
  set levelId(v: number) { this.data.levelId = v; }
}
