import { MemberType } from './Tile';

export interface ObjectiveDef {
  type: string;
  target: number;
  member?: MemberType;
  specialType?: string;
}

export interface ObjectiveProgress {
  definition: ObjectiveDef;
  current: number;
  completed: boolean;
}

export class ObjectiveSystem {
  private progress: ObjectiveProgress[] = [];

  constructor(objectives: ObjectiveDef[]) {
    this.reset(objectives);
  }

  getProgress(): ObjectiveProgress[] {
    return this.progress;
  }

  isAllCompleted(): boolean {
    return this.progress.length > 0 && this.progress.every(p => p.completed);
  }

  onTilesCleared(tiles: { member: MemberType | null }[]): void {
    const memberCounts: Record<string, number> = {};
    for (const tile of tiles) {
      if (tile.member) {
        memberCounts[tile.member] = (memberCounts[tile.member] || 0) + 1;
      }
    }

    for (const p of this.progress) {
      if (!p.completed && p.definition.type === 'collect_member') {
        const m = p.definition.member;
        if (m && memberCounts[m]) {
          p.current += memberCounts[m];
          if (p.current >= p.definition.target) {
            p.current = p.definition.target;
            p.completed = true;
          }
        }
      }
    }
  }

  onScoreChanged(score: number): void {
    for (const p of this.progress) {
      if (!p.completed && p.definition.type === 'score') {
        p.current = score;
        if (p.current >= p.definition.target) {
          p.completed = true;
        }
      }
    }
  }

  onObstacleDestroyed(type: string): void {
    for (const p of this.progress) {
      if (!p.completed) {
        if (
          (p.definition.type === 'clear_jelly' && type === 'jelly') ||
          (p.definition.type === 'break_ice' && type === 'ice') ||
          (p.definition.type === 'destroy_crate' && type === 'crate') ||
          (p.definition.type === 'break_chain' && type === 'chain')
        ) {
          p.current++;
          if (p.current >= p.definition.target) p.completed = true;
        }
      }
    }
  }

  onSpecialCreated(type: string): void {
    for (const p of this.progress) {
      if (!p.completed && p.definition.type === 'create_special') {
        if (!p.definition.specialType || p.definition.specialType === type) {
          p.current++;
          if (p.current >= p.definition.target) p.completed = true;
        }
      }
    }
  }

  onSpecialActivated(type: string): void {
    for (const p of this.progress) {
      if (!p.completed && p.definition.type === 'activate_special') {
        if (!p.definition.specialType || p.definition.specialType === type) {
          p.current++;
          if (p.current >= p.definition.target) p.completed = true;
        }
      }
    }
  }

  onCombo(count: number): void {
    for (const p of this.progress) {
      if (!p.completed && p.definition.type === 'perform_combo') {
        if (count >= p.definition.target) {
          p.current = p.definition.target;
          p.completed = true;
        } else if (count > p.current) {
          p.current = count;
        }
      }
    }
  }

  onCascade(count: number): void {
    for (const p of this.progress) {
      if (!p.completed && p.definition.type === 'perform_cascade') {
        if (count >= p.definition.target) {
          p.current = p.definition.target;
          p.completed = true;
        } else if (count > p.current) {
          p.current = count;
        }
      }
    }
  }

  reset(objectives: ObjectiveDef[]): void {
    this.progress = objectives.map(obj => ({
      definition: obj,
      current: 0,
      completed: false
    }));
  }
}
