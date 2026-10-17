import React, { useEffect, useState } from 'react';
import { StyleSheet, View, Text as RNText, ScrollView, TouchableOpacity, Image } from 'react-native';
import { SaveManager, SaveData } from '../storage/SaveManager';
import { MEMBER_CONFIGS } from '../bts/members';
import { ALL_MEMBERS } from '../game/Tile';

interface CollectionScreenProps {
  onBack: () => void;
}

export const CollectionScreen: React.FC<CollectionScreenProps> = ({ onBack }) => {
  const [saveData, setSaveData] = useState<SaveData | null>(null);

  useEffect(() => {
    const loadData = async () => {
      const data = await SaveManager.getInstance().load();
      setSaveData(data);
    };
    loadData();
  }, []);

  if (!saveData) {
    return (
      <View style={styles.loadingContainer}>
        <RNText style={styles.loadingText}>Loading Collection...</RNText>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backBtn} onPress={onBack}>
          <RNText style={styles.backBtnText}>← Back</RNText>
        </TouchableOpacity>
        <RNText style={styles.headerTitle}>Collection</RNText>
        <View style={{ width: 60 }} />
      </View>

      <ScrollView style={styles.content} contentContainerStyle={styles.scrollContent}>
        <View style={styles.statsCard}>
          <RNText style={styles.statsTitle}>Overall Progress</RNText>
          <View style={styles.statsGrid}>
            <View style={styles.statBox}>
              <RNText style={styles.statValue}>{saveData.stats.levelsCompleted}</RNText>
              <RNText style={styles.statLabel}>Levels Cleared</RNText>
            </View>
            <View style={styles.statBox}>
              <RNText style={styles.statValue}>{saveData.stats.highestScore}</RNText>
              <RNText style={styles.statLabel}>High Score</RNText>
            </View>
            <View style={styles.statBox}>
              <RNText style={styles.statValue}>{saveData.stats.totalCombos}</RNText>
              <RNText style={styles.statLabel}>Total Combos</RNText>
            </View>
          </View>
        </View>

        <RNText style={styles.sectionTitle}>Achievements</RNText>
        <View style={styles.achievementsList}>
          {Object.entries(saveData.achievements).length === 0 ? (
            <RNText style={{ color: '#888', fontStyle: 'italic', marginBottom: 20 }}>No achievements unlocked yet.</RNText>
          ) : (
            Object.entries(saveData.achievements).map(([id, data]) => (
              <View key={id} style={styles.achievementCard}>
                <RNText style={styles.achievementTitle}>{id}</RNText>
                <RNText style={styles.achievementDate}>{new Date(data.unlockedAt || 0).toLocaleDateString()}</RNText>
              </View>
            ))
          )}
        </View>

        <RNText style={styles.sectionTitle}>Members</RNText>
        
        <View style={styles.membersGrid}>
          {ALL_MEMBERS.map(member => {
            const config = MEMBER_CONFIGS[member];
            const stats = saveData.stats.memberUsage[member] || { matchCount: 0, specialsCreated: 0, timesUsed: 0 };
            
            return (
              <View key={member} style={[styles.memberCard, { borderColor: config.themeColor }]}>
                <Image source={config.portrait} style={styles.memberImage} resizeMode="cover" />
                <View style={styles.memberInfo}>
                  <RNText style={[styles.memberName, { color: config.themeColor }]}>{config.displayName}</RNText>
                  <RNText style={styles.memberStat}>Matched: {stats.matchCount}</RNText>
                  <RNText style={styles.memberStat}>Specials: {stats.specialsCreated}</RNText>
                </View>
              </View>
            );
          })}
        </View>
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
  },
  backBtn: {
    padding: 8,
  },
  backBtnText: {
    color: '#fff',
    fontSize: 16,
  },
  headerTitle: {
    color: '#FFD700',
    fontSize: 20,
    fontWeight: 'bold',
  },
  content: {
    flex: 1,
  },
  scrollContent: {
    padding: 20,
    paddingBottom: 40,
  },
  statsCard: {
    backgroundColor: '#1a1a2e',
    borderRadius: 16,
    padding: 20,
    marginBottom: 30,
    borderWidth: 1,
    borderColor: '#333',
  },
  statsTitle: {
    color: '#FFD700',
    fontSize: 18,
    fontWeight: 'bold',
    marginBottom: 16,
    textAlign: 'center',
  },
  statsGrid: {
    flexDirection: 'row',
    justifyContent: 'space-between',
  },
  statBox: {
    alignItems: 'center',
    flex: 1,
  },
  statValue: {
    color: '#fff',
    fontSize: 24,
    fontWeight: 'bold',
    marginBottom: 4,
  },
  statLabel: {
    color: '#888',
    fontSize: 12,
  },
  sectionTitle: {
    color: '#fff',
    fontSize: 22,
    fontWeight: 'bold',
    marginBottom: 16,
  },
  membersGrid: {
    flexDirection: 'row',
    flexWrap: 'wrap',
    justifyContent: 'space-between',
  },
  memberCard: {
    width: '48%',
    backgroundColor: '#1a1a2e',
    borderRadius: 12,
    borderWidth: 2,
    marginBottom: 16,
    overflow: 'hidden',
  },
  memberImage: {
    width: '100%',
    height: 120,
    backgroundColor: '#111',
  },
  memberInfo: {
    padding: 12,
  },
  memberName: {
    fontSize: 18,
    fontWeight: 'bold',
    marginBottom: 8,
  },
  memberStat: {
    color: '#aaa',
    fontSize: 12,
    marginBottom: 2,
  },
  achievementsList: {
    marginBottom: 20,
  },
  achievementCard: {
    backgroundColor: '#2a2a4a',
    padding: 12,
    borderRadius: 8,
    marginBottom: 8,
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
  },
  achievementTitle: {
    color: '#FFD700',
    fontWeight: 'bold',
    fontSize: 16,
  },
  achievementDate: {
    color: '#888',
    fontSize: 12,
  },
});
