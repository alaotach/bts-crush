// import { Audio, AVPlaybackSource } from 'expo-av';

// Mocking AVPlaybackSource since expo-av native module is failing in the user's Expo Go
type AVPlaybackSource = any;

/**
 * AudioManager — handles all game audio.
 * Currently mocked out to prevent 'cannot find native module exponentAV' errors.
 */

type SFXType =
  | 'tap'
  | 'swap'
  | 'invalid_swap'
  | 'match'
  | 'special_create'
  | 'special_activate'
  | 'explosion'
  | 'combo_good'
  | 'combo_great'
  | 'combo_amazing'
  | 'combo_encore'
  | 'cascade'
  | 'objective_complete'
  | 'victory'
  | 'failure'
  | 'unlock'
  | 'reward'
  | 'menu_tap'
  | 'shuffle';

type MusicTrack = 'menu' | 'gameplay' | 'special_level' | 'victory' | 'defeat';

interface AudioConfig {
  sfx: Partial<Record<SFXType, AVPlaybackSource>>;
  music: Partial<Record<MusicTrack, AVPlaybackSource>>;
}

export class AudioManager {
  private static instance: AudioManager;

  private currentTrack: MusicTrack | null = null;
  private musicVolume: number = 0.7;
  private sfxVolume: number = 1.0;
  private musicEnabled: boolean = true;
  private sfxEnabled: boolean = true;
  private config: AudioConfig = { sfx: {}, music: {} };

  private constructor() {}

  static getInstance(): AudioManager {
    if (!AudioManager.instance) {
      AudioManager.instance = new AudioManager();
    }
    return AudioManager.instance;
  }

  setConfig(config: AudioConfig): void {
    this.config = config;
  }

  async playSFX(type: SFXType): Promise<void> {
    if (!this.sfxEnabled) return;
    const source = this.config.sfx[type];
    if (!source) return;
    // console.log(`[Audio Mock] Playing SFX: ${type}`);
  }

  async playMusic(track: MusicTrack): Promise<void> {
    if (!this.musicEnabled) return;
    if (this.currentTrack === track) return;

    const source = this.config.music[track];
    if (!source) return;

    await this.stopMusic();
    this.currentTrack = track;
    // console.log(`[Audio Mock] Playing Music: ${track}`);
  }

  async stopMusic(): Promise<void> {
    this.currentTrack = null;
  }

  async pauseMusic(): Promise<void> {
    // Mock
  }

  async resumeMusic(): Promise<void> {
    // Mock
  }

  setMusicVolume(volume: number): void {
    this.musicVolume = Math.max(0, Math.min(1, volume));
  }

  setSFXVolume(volume: number): void {
    this.sfxVolume = Math.max(0, Math.min(1, volume));
  }

  setMusicEnabled(enabled: boolean): void {
    this.musicEnabled = enabled;
    if (!enabled) {
      this.stopMusic();
    }
  }

  setSFXEnabled(enabled: boolean): void {
    this.sfxEnabled = enabled;
  }

  getMusicVolume(): number { return this.musicVolume; }
  getSFXVolume(): number { return this.sfxVolume; }
  isMusicEnabled(): boolean { return this.musicEnabled; }
  isSFXEnabled(): boolean { return this.sfxEnabled; }
}
