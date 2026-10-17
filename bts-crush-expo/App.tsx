import { StatusBar } from 'expo-status-bar';
import { StyleSheet, Text, View, SafeAreaView, TouchableOpacity } from 'react-native';
import { GestureHandlerRootView } from 'react-native-gesture-handler';
import { GameScreen } from './src/screens/GameScreen';
import { LevelMapScreen } from './src/screens/LevelMapScreen';
import { CollectionScreen } from './src/screens/CollectionScreen';
import { SaveManager } from './src/storage/SaveManager';
import { SettingsScreen } from './src/screens/SettingsScreen';
import { useState } from 'react';

type Screen = 'map' | 'game' | 'result' | 'collection' | 'settings';

export default function App() {
  const [currentScreen, setCurrentScreen] = useState<Screen>('map');
  const [currentLevel, setCurrentLevel] = useState(1);
  const [lastScore, setLastScore] = useState(0);
  const [lastStars, setLastStars] = useState(0);

  const startLevel = (level: number) => {
    setCurrentLevel(level);
    setCurrentScreen('game');
  };

  const handleLevelComplete = async (score: number, stars: number) => {
    setLastScore(score);
    setLastStars(stars);
    await SaveManager.getInstance().load(); // Ensure loaded
    await SaveManager.getInstance().completeLevel(currentLevel, score, stars);
    setCurrentScreen('result');
  };

  const handleLevelFailed = () => {
    setCurrentScreen('map');
  };

  return (
    <GestureHandlerRootView style={{ flex: 1 }}>
      <StatusBar style="light" />
      
      {currentScreen === 'map' && (
        <LevelMapScreen
          onSelectLevel={startLevel}
          onOpenCollection={() => setCurrentScreen('collection')}
          onOpenSettings={() => setCurrentScreen('settings')}
        />
      )}

      {currentScreen === 'collection' && (
        <CollectionScreen
          onBack={() => setCurrentScreen('map')}
        />
      )}

      {currentScreen === 'settings' && (
        <SettingsScreen
          onBack={() => setCurrentScreen('map')}
        />
      )}

      {currentScreen === 'game' && (
        <GameScreen
          levelId={currentLevel}
          onLevelComplete={handleLevelComplete}
          onLevelFailed={handleLevelFailed}
          onBack={() => setCurrentScreen('map')}
        />
      )}

      {currentScreen === 'result' && (
        <SafeAreaView style={styles.resultContainer}>
          <Text style={styles.resultTitle}>Stage Clear!</Text>
          <Text style={styles.resultStars}>
            {'★'.repeat(lastStars)}{'☆'.repeat(3 - lastStars)}
          </Text>
          <Text style={styles.resultScore}>Score: {lastScore}</Text>
          <TouchableOpacity
            style={styles.nextButton}
            onPress={() => startLevel(currentLevel + 1)}
          >
            <Text style={styles.nextButtonText}>Next Level →</Text>
          </TouchableOpacity>
          <TouchableOpacity
            style={[styles.nextButton, { backgroundColor: '#555' }]}
            onPress={() => setCurrentScreen('map')}
          >
            <Text style={styles.nextButtonText}>Back to Map</Text>
          </TouchableOpacity>
        </SafeAreaView>
      )}
    </GestureHandlerRootView>
  );
}

const styles = StyleSheet.create({
  resultContainer: {
    flex: 1,
    backgroundColor: '#0f0f23',
    alignItems: 'center',
    justifyContent: 'center',
  },
  resultTitle: {
    fontSize: 36,
    fontWeight: 'bold',
    color: '#FFD700',
    marginBottom: 16,
  },
  resultStars: {
    fontSize: 56,
    color: '#FFD700',
    marginBottom: 16,
  },
  resultScore: {
    fontSize: 24,
    color: '#fff',
    marginBottom: 32,
  },
  nextButton: {
    backgroundColor: '#9b59b6',
    paddingHorizontal: 32,
    paddingVertical: 14,
    borderRadius: 12,
    marginBottom: 12,
  },
  nextButtonText: {
    color: '#fff',
    fontSize: 18,
    fontWeight: 'bold',
  },
});
