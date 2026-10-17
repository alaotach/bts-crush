import AsyncStorage from '@react-native-async-storage/async-storage';

/**
 * SaveManager — persistent game data using AsyncStorage.
 * 
 * Handles saving/loading of all progression, settings, stats,
 * and collection data.
 */

export interface GameSettings {
  musicEnabled: boolean;
  musicVolume: number;
  sfxEnabled: boolean;
  sfxVolume: number;
  hapticsEnabled: boolean;
  reducedMotion: boolean;
  notificationsEnabled: boolean;
}

export interface LevelStars {
  [levelId: number]: {
    stars: number;
    highScore: number;
    completed: boolean;
  };
}

export interface MemberStats {
  matchCount: number;
  specialsCreated: number;
  timesUsed: number;
}

export interface GameStats {
  gamesPlayed: number;
  levelsCompleted: number;
  totalMatches: number;
  totalCombos: number;
  highestCombo: number;
  highestScore: number;
  specialsCreated: number;
  specialCombos: number;
  memberUsage: Record<string, MemberStats>;
}

export interface AchievementData {
  [achievementId: string]: {
    unlocked: boolean;
    unlockedAt?: number;
    progress?: number;
  };
}

export interface SaveData {
  currentLevel: number;
  unlockedLevels: number[];
  levelStars: LevelStars;
  settings: GameSettings;
  stats: GameStats;
  achievements: AchievementData;
  lastDailyChallenge?: string; // ISO date
  dailyChallengeScore?: number;
}

const SAVE_KEY = '@bts_crush_save';

const DEFAULT_SETTINGS: GameSettings = {
  musicEnabled: true,
  musicVolume: 0.7,
  sfxEnabled: true,
  sfxVolume: 1.0,
  hapticsEnabled: true,
  reducedMotion: false,
  notificationsEnabled: true,
};

const DEFAULT_STATS: GameStats = {
  gamesPlayed: 0,
  levelsCompleted: 0,
  totalMatches: 0,
  totalCombos: 0,
  highestCombo: 0,
  highestScore: 0,
  specialsCreated: 0,
  specialCombos: 0,
  memberUsage: {},
};

function getDefaultSave(): SaveData {
  return {
    currentLevel: 1,
    unlockedLevels: [1],
    levelStars: {},
    settings: { ...DEFAULT_SETTINGS },
    stats: { ...DEFAULT_STATS },
    achievements: {},
  };
}

export class SaveManager {
  private static instance: SaveManager;
  private data: SaveData;
  private loaded: boolean = false;

  private constructor() {
    this.data = getDefaultSave();
  }

  static getInstance(): SaveManager {
    if (!SaveManager.instance) {
      SaveManager.instance = new SaveManager();
    }
    return SaveManager.instance;
  }

  async load(): Promise<SaveData> {
    try {
      const raw = await AsyncStorage.getItem(SAVE_KEY);
      if (raw) {
        const parsed = JSON.parse(raw) as Partial<SaveData>;
        this.data = {
          ...getDefaultSave(),
          ...parsed,
          settings: { ...DEFAULT_SETTINGS, ...(parsed.settings || {}) },
          stats: { ...DEFAULT_STATS, ...(parsed.stats || {}) },
        };
      }
    } catch (e) {
      this.data = getDefaultSave();
    }
    this.loaded = true;
    return this.data;
  }

  async save(): Promise<void> {
    try {
      await AsyncStorage.setItem(SAVE_KEY, JSON.stringify(this.data));
    } catch (e) {
      // Silently fail — don't crash the game over persistence
    }
  }

  getData(): SaveData {
    return this.data;
  }

  getSettings(): GameSettings {
    return this.data.settings;
  }

  async updateSettings(settings: Partial<GameSettings>): Promise<void> {
    this.data.settings = { ...this.data.settings, ...settings };
    await this.save();
  }

  async completeLevel(levelId: number, score: number, stars: number): Promise<void> {
    const existing = this.data.levelStars[levelId];
    this.data.levelStars[levelId] = {
      stars: Math.max(existing?.stars || 0, stars),
      highScore: Math.max(existing?.highScore || 0, score),
      completed: true,
    };

    // Unlock next level
    const nextLevel = levelId + 1;
    if (!this.data.unlockedLevels.includes(nextLevel)) {
      this.data.unlockedLevels.push(nextLevel);
    }

    this.data.currentLevel = Math.max(this.data.currentLevel, nextLevel);
    this.data.stats.levelsCompleted++;
    this.data.stats.highestScore = Math.max(this.data.stats.highestScore, score);
    this.data.stats.gamesPlayed++;

    await this.save();
  }

  async updateStats(update: Partial<GameStats>): Promise<void> {
    this.data.stats = { ...this.data.stats, ...update };
    await this.save();
  }

  async unlockAchievement(id: string): Promise<void> {
    if (!this.data.achievements[id]?.unlocked) {
      this.data.achievements[id] = {
        unlocked: true,
        unlockedAt: Date.now(),
      };
      await this.save();
    }
  }

  async resetAll(): Promise<void> {
    this.data = getDefaultSave();
    await AsyncStorage.removeItem(SAVE_KEY);
  }

  isLoaded(): boolean {
    return this.loaded;
  }
}
