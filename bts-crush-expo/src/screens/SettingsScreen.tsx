import React, { useEffect, useState } from 'react';
import { StyleSheet, View, Text as RNText, TouchableOpacity, Switch, Alert } from 'react-native';
import { SaveManager, GameSettings } from '../storage/SaveManager';
import { HapticManager } from '../haptics/HapticManager';
import { AudioManager } from '../audio/AudioManager';

interface SettingsScreenProps {
  onBack: () => void;
}

export const SettingsScreen: React.FC<SettingsScreenProps> = ({ onBack }) => {
  const [settings, setSettings] = useState<GameSettings | null>(null);

  useEffect(() => {
    const loadData = async () => {
      const data = await SaveManager.getInstance().load();
      setSettings(data.settings);
    };
    loadData();
  }, []);

  const updateSetting = async (key: keyof GameSettings, value: boolean) => {
    if (!settings) return;
    const newSettings = { ...settings, [key]: value };
    setSettings(newSettings);
    await SaveManager.getInstance().updateSettings({ [key]: value });

    // Apply immediately
    if (key === 'hapticsEnabled') {
      HapticManager.getInstance().setEnabled(value);
    }
  };

  const handleReset = () => {
    Alert.alert(
      "Reset Progress",
      "Are you sure you want to reset all your progress? This cannot be undone.",
      [
        { text: "Cancel", style: "cancel" },
        { 
          text: "Reset", 
          style: "destructive",
          onPress: async () => {
            await SaveManager.getInstance().resetAll();
            Alert.alert("Reset", "Your progress has been reset.");
            onBack();
          }
        }
      ]
    );
  };

  if (!settings) {
    return (
      <View style={styles.loadingContainer}>
        <RNText style={styles.loadingText}>Loading Settings...</RNText>
      </View>
    );
  }

  return (
    <View style={styles.container}>
      <View style={styles.header}>
        <TouchableOpacity style={styles.backBtn} onPress={onBack}>
          <RNText style={styles.backBtnText}>← Back</RNText>
        </TouchableOpacity>
        <RNText style={styles.headerTitle}>Settings</RNText>
        <View style={{ width: 60 }} />
      </View>

      <View style={styles.content}>
        <View style={styles.settingRow}>
          <RNText style={styles.settingLabel}>Sound Effects (SFX)</RNText>
          <Switch 
            value={settings.sfxEnabled} 
            onValueChange={(v) => updateSetting('sfxEnabled', v)} 
            trackColor={{ false: '#444', true: '#9b59b6' }}
            thumbColor={'#fff'}
          />
        </View>

        <View style={styles.settingRow}>
          <RNText style={styles.settingLabel}>Music</RNText>
          <Switch 
            value={settings.musicEnabled} 
            onValueChange={(v) => updateSetting('musicEnabled', v)} 
            trackColor={{ false: '#444', true: '#9b59b6' }}
            thumbColor={'#fff'}
          />
        </View>

        <View style={styles.settingRow}>
          <RNText style={styles.settingLabel}>Haptic Feedback</RNText>
          <Switch 
            value={settings.hapticsEnabled} 
            onValueChange={(v) => updateSetting('hapticsEnabled', v)} 
            trackColor={{ false: '#444', true: '#9b59b6' }}
            thumbColor={'#fff'}
          />
        </View>

        <View style={styles.settingRow}>
          <RNText style={styles.settingLabel}>Reduced Motion</RNText>
          <Switch 
            value={settings.reducedMotion} 
            onValueChange={(v) => updateSetting('reducedMotion', v)} 
            trackColor={{ false: '#444', true: '#9b59b6' }}
            thumbColor={'#fff'}
          />
        </View>

        <View style={{ marginTop: 40, alignItems: 'center' }}>
          <TouchableOpacity style={styles.resetBtn} onPress={handleReset}>
            <RNText style={styles.resetBtnText}>Reset All Progress</RNText>
          </TouchableOpacity>
        </View>
      </View>
    </View>
  );
};

const styles = StyleSheet.create({
  container: { flex: 1, backgroundColor: '#0f0f23' },
  loadingContainer: { flex: 1, justifyContent: 'center', alignItems: 'center', backgroundColor: '#0f0f23' },
  loadingText: { color: '#fff', fontSize: 20 },
  header: { height: 90, paddingTop: 40, paddingHorizontal: 20, backgroundColor: '#1a1a2e', flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', borderBottomWidth: 1, borderBottomColor: '#333' },
  backBtn: { padding: 8 },
  backBtnText: { color: '#fff', fontSize: 16 },
  headerTitle: { color: '#FFD700', fontSize: 20, fontWeight: 'bold' },
  content: { padding: 20 },
  settingRow: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'center', paddingVertical: 16, borderBottomWidth: 1, borderBottomColor: '#222' },
  settingLabel: { color: '#fff', fontSize: 18 },
  resetBtn: { backgroundColor: '#c0392b', paddingHorizontal: 24, paddingVertical: 12, borderRadius: 8 },
  resetBtnText: { color: '#fff', fontSize: 16, fontWeight: 'bold' },
});
