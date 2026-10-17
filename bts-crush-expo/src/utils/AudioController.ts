import { Audio } from 'expo-av';

export class AudioController {
  static async playSound(type: 'good' | 'great' | 'amazing' | 'encore' | 'match') {
    // Basic boilerplate for loading and playing audio via Expo AV
    // In production, these should be pre-loaded in state.
    // const { sound } = await Audio.Sound.createAsync(require('../../assets/sounds/match.wav'));
    // await sound.playAsync();
  }

  static async playBGM() {
    // const { sound } = await Audio.Sound.createAsync(require('../../assets/sounds/bgm.mp3'));
    // await sound.setIsLoopingAsync(true);
    // await sound.playAsync();
  }
}
