import * as Haptics from 'expo-haptics';

/**
 * HapticManager — centralized haptic feedback system.
 * 
 * All haptic calls go through here so they can be globally
 * toggled via settings without changing game code.
 */

type HapticType =
  | 'lightTap'
  | 'mediumTap'
  | 'heavyImpact'
  | 'tileSelect'
  | 'tileSwap'
  | 'invalidSwap'
  | 'match'
  | 'specialCreate'
  | 'specialActivate'
  | 'combo'
  | 'cascade'
  | 'victory'
  | 'failure'
  | 'menuTap';

export class HapticManager {
  private static instance: HapticManager;
  private enabled: boolean = true;

  private constructor() {}

  static getInstance(): HapticManager {
    if (!HapticManager.instance) {
      HapticManager.instance = new HapticManager();
    }
    return HapticManager.instance;
  }

  setEnabled(enabled: boolean): void {
    this.enabled = enabled;
  }

  isEnabled(): boolean {
    return this.enabled;
  }

  trigger(type: HapticType): void {
    if (!this.enabled) return;

    switch (type) {
      case 'lightTap':
      case 'tileSelect':
      case 'menuTap':
        Haptics.impactAsync(Haptics.ImpactFeedbackStyle.Light);
        break;

      case 'mediumTap':
      case 'tileSwap':
      case 'match':
        Haptics.impactAsync(Haptics.ImpactFeedbackStyle.Medium);
        break;

      case 'heavyImpact':
      case 'specialActivate':
      case 'cascade':
        Haptics.impactAsync(Haptics.ImpactFeedbackStyle.Heavy);
        break;

      case 'specialCreate':
        Haptics.notificationAsync(Haptics.NotificationFeedbackType.Success);
        break;

      case 'invalidSwap':
        Haptics.notificationAsync(Haptics.NotificationFeedbackType.Error);
        break;

      case 'combo':
        Haptics.notificationAsync(Haptics.NotificationFeedbackType.Warning);
        break;

      case 'victory':
        // Sequence of haptics for victory feel
        Haptics.notificationAsync(Haptics.NotificationFeedbackType.Success);
        setTimeout(() => {
          if (this.enabled) Haptics.impactAsync(Haptics.ImpactFeedbackStyle.Heavy);
        }, 200);
        setTimeout(() => {
          if (this.enabled) Haptics.notificationAsync(Haptics.NotificationFeedbackType.Success);
        }, 400);
        break;

      case 'failure':
        Haptics.notificationAsync(Haptics.NotificationFeedbackType.Error);
        setTimeout(() => {
          if (this.enabled) Haptics.impactAsync(Haptics.ImpactFeedbackStyle.Heavy);
        }, 300);
        break;
    }
  }
}
