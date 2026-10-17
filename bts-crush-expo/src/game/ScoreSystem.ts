import { Match, MatchType } from './MatchDetector';
import { SpecialType } from './Tile';

export class ScoreSystem {
  private _score: number = 0;
  private _combo: number = 0;

  get score(): number {
    return this._score;
  }

  get combo(): number {
    return this._combo;
  }

  calculateMatchScore(match: Match): number {
    switch (match.type) {
      case MatchType.MATCH_3: return 30;
      case MatchType.MATCH_4_HORIZONTAL:
      case MatchType.MATCH_4_VERTICAL: return 60;
      case MatchType.MATCH_2X2: return 50;
      case MatchType.T_SHAPE:
      case MatchType.L_SHAPE: return 80;
      case MatchType.MATCH_5: return 100;
      case MatchType.MATCH_6_PLUS: return 150;
      default: return 30;
    }
  }

  calculateSpecialScore(specialType: SpecialType): number {
    return 100;
  }

  calculateActivationScore(specialType: SpecialType, tilesCleared: number): number {
    return tilesCleared * 10;
  }

  addScore(points: number): void {
    const comboMulti = this.getComboMultiplier();
    this._score += Math.floor(points * comboMulti);
  }

  incrementCombo(): void {
    this._combo++;
  }

  resetCombo(): void {
    this._combo = 0;
  }

  getComboMultiplier(): number {
    return 1 + (this._combo * 0.5);
  }

  calculateRemainingMovesBonus(movesLeft: number): number {
    return movesLeft * 50;
  }

  reset(): void {
    this._score = 0;
    this._combo = 0;
  }
}
