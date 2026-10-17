import React, { useEffect, useState, useRef } from 'react';
import { StyleSheet, View, Text as RNText, ScrollView, TouchableOpacity, Dimensions } from 'react-native';
import { SaveManager, SaveData } from '../storage/SaveManager';
import { LevelLoader } from '../levels/LevelLoader';

const { width: SCREEN_WIDTH } = Dimensions.get('window');
const NODE_SIZE = 60;
const MAP_HEIGHT_PER_LEVEL = 100;

interface LevelMapScreenProps {
  onSelectLevel: (levelId: number) => void;
  onOpenCollection: () => void;
  onOpenSettings: () => void;
}

export const LevelMapScreen: React.FC<LevelMapScreenProps> = ({ onSelectLevel, onOpenCollection, onOpenSettings }) => {
  const [saveData, setSaveData] = useState<SaveData | null>(null);
  const levelLoader = useRef(new LevelLoader());
  const scrollViewRef = useRef<ScrollView>(null);

  useEffect(() => {
    const loadData = async () => {
      const data = await SaveManager.getInstance().load();
      setSaveData(data);
      // Scroll to current level after a short delay to ensure rendering
      setTimeout(() => {
        if (scrollViewRef.current && data.currentLevel) {
          const yPos = Math.max(0, (levelLoader.current.getLevelCount() - data.currentLevel) * MAP_HEIGHT_PER_LEVEL - 200);
          scrollViewRef.current.scrollTo({ y: yPos, animated: false });
        }
      }, 100);
    };
    loadData();
  }, []);

  if (!saveData) {
    return (
      <View style={styles.loadingContainer}>
        <RNText style={styles.loadingText}>Loading Map...</RNText>
      </View>
    );
  }

  const levels = levelLoader.current.getWorldLevels(1); // Load world 1
  const totalLevels = levels.length;

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <RNText style={styles.headerTitle}>BTS CRUSH</RNText>
        <View style={{ flexDirection: 'row', gap: 10 }}>
          <TouchableOpacity style={styles.collectionBtn} onPress={onOpenCollection}>
            <RNText style={styles.collectionBtnText}>Collect</RNText>
          </TouchableOpacity>
          <TouchableOpacity style={styles.collectionBtn} onPress={onOpenSettings}>
            <RNText style={styles.collectionBtnText}>⚙️</RNText>
          </TouchableOpacity>
        </View>
      </View>

      <ScrollView 
        ref={scrollViewRef}
        style={styles.mapScroll} 
        contentContainerStyle={{ height: totalLevels * MAP_HEIGHT_PER_LEVEL + 200 }}
        showsVerticalScrollIndicator={false}
      >
        {/* Draw Levels from bottom to top */}
        {levels.slice().reverse().map((level, index) => {
          const actualLevelId = level.id;
          const isUnlocked = saveData.unlockedLevels.includes(actualLevelId);
          const isCurrent = saveData.currentLevel === actualLevelId;
          const stars = saveData.levelStars[actualLevelId]?.stars || 0;
          
          // Calculate winding path position
          const levelIndexFromBottom = totalLevels - 1 - index;
          const yPos = index * MAP_HEIGHT_PER_LEVEL + 100; // From top of scroll view
          // Sine wave for X position
          const normalizedX = (Math.sin(levelIndexFromBottom * 0.8) + 1) / 2;
          const xPos = 40 + normalizedX * (SCREEN_WIDTH - NODE_SIZE - 80);

          return (
            <View key={actualLevelId} style={[styles.nodeContainer, { top: yPos, left: xPos }]}>
              {stars > 0 && (
                <View style={styles.starsContainer}>
                  <RNText style={styles.starsText}>
                    {'★'.repeat(stars)}{'☆'.repeat(3 - stars)}
                  </RNText>
                </View>
              )}
              <TouchableOpacity
                style={[
                  styles.levelNode,
                  isUnlocked ? styles.levelNodeUnlocked : styles.levelNodeLocked,
                  isCurrent && styles.levelNodeCurrent
                ]}
                disabled={!isUnlocked}
                onPress={() => onSelectLevel(actualLevelId)}
              >
                <RNText style={styles.levelNumber}>{actualLevelId}</RNText>
              </TouchableOpacity>
            </View>
          );
        })}
      </ScrollView>
    </View>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#0f0f23',
  },
  loadingContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: '#0f0f23',
  },
  loadingText: {
    color: '#fff',
    fontSize: 20,
  },
  header: {
    height: 90,
    paddingTop: 40,
    paddingHorizontal: 20,
    backgroundColor: '#1a1a2e',
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    borderBottomWidth: 1,
    borderBottomColor: '#333',
    zIndex: 10,
  },
  headerTitle: {
    color: '#FFD700',
    fontSize: 24,
    fontWeight: 'bold',
    letterSpacing: 2,
  },
  collectionBtn: {
    backgroundColor: '#9b59b6',
    paddingHorizontal: 16,
    paddingVertical: 8,
    borderRadius: 20,
  },
  collectionBtnText: {
    color: '#fff',
    fontWeight: 'bold',
  },
  mapScroll: {
    flex: 1,
  },
  nodeContainer: {
    position: 'absolute',
    alignItems: 'center',
    width: NODE_SIZE,
  },
  levelNode: {
    width: NODE_SIZE,
    height: NODE_SIZE,
    borderRadius: NODE_SIZE / 2,
    justifyContent: 'center',
    alignItems: 'center',
    borderWidth: 3,
    shadowColor: '#000',
    shadowOffset: { width: 0, height: 4 },
    shadowOpacity: 0.3,
    shadowRadius: 4,
    elevation: 5,
  },
  levelNodeUnlocked: {
    backgroundColor: '#4a4a8a',
    borderColor: '#9b59b6',
  },
  levelNodeLocked: {
    backgroundColor: '#2a2a3a',
    borderColor: '#444',
  },
  levelNodeCurrent: {
    backgroundColor: '#9b59b6',
    borderColor: '#FFD700',
    borderWidth: 4,
    transform: [{ scale: 1.15 }],
  },
  levelNumber: {
    color: '#fff',
    fontSize: 22,
    fontWeight: 'bold',
  },
  starsContainer: {
    position: 'absolute',
    top: -20,
    width: 80,
    alignItems: 'center',
  },
  starsText: {
    color: '#FFD700',
    fontSize: 16,
    textShadowColor: 'rgba(0,0,0,0.8)',
    textShadowOffset: { width: 1, height: 1 },
    textShadowRadius: 2,
  },
});
