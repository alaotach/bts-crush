import React, { useEffect, useState, useCallback, useRef, useMemo } from 'react';
import { StyleSheet, Dimensions, View, Text as RNText, TouchableOpacity, Image, ImageBackground } from 'react-native';
import { BlurView } from 'expo-blur';
import { Canvas, Group, useImage, RoundedRect } from '@shopify/react-native-skia';
import { GestureDetector, Gesture } from 'react-native-gesture-handler';
import Animated, { useSharedValue, useAnimatedStyle, withSequence, withTiming, withRepeat, withSpring, runOnJS } from 'react-native-reanimated';
import { GameEngine } from '../game/GameEngine';
import { GamePhase } from '../game/GamePhase';
import { GameStateData } from '../game/GameState';
import { LevelLoader } from '../levels/LevelLoader';
import { MEMBER_CONFIGS } from '../bts/members';
import { HapticManager } from '../haptics/HapticManager';
import { AudioManager } from '../audio/AudioManager';
import { TileRenderer } from '../rendering/TileRenderer';
import { EffectRenderer } from '../rendering/EffectRenderer';
import { Tile } from '../game/Tile';

const { width: SCREEN_WIDTH } = Dimensions.get('window');

interface GameScreenProps {
  levelId: number;
  onLevelComplete?: (score: number, stars: number) => void;
  onLevelFailed?: () => void;
  onBack?: () => void;
}

// Track rendered state for animations
interface RenderedTile extends Tile {
  isMatched: boolean;
}

// Power-up bar config — swap icon sources for your real assets
const POWER_UPS = [
  { id: 'wand', img: require('../../assets/members/Adobe Express - file.png'), count: 3 },
  { id: 'shuffle', img: require('../../assets/members/Adobe Express - file(1).png'), count: 3 },
  { id: 'star', img: require('../../assets/members/Gemini_Generated_Image_goam5kgoam5kgoam-removebg-preview.png'), count: 3 },
  { id: 'wings', img: require('../../assets/members/Gemini_Generated_Image_hebwruhebwruhebw-removebg-preview.png'), count: 3 },
  { id: 'book', img: require('../../assets/members/Gemini_Generated_Image_jj6o01jj6o01jj6o-removebg-preview.png'), count: 3 },
];

// Floating ambience decorations (hearts / sparkles scattered over the background)
const AMBIENT_DECOR = [
  { icon: '💜', top: '4%', left: '6%', size: 18, opacity: 0.5 },
  { icon: '✨', top: '3%', left: '85%', size: 16, opacity: 0.6 },
  { icon: '💫', top: '9%', left: '92%', size: 14, opacity: 0.45 },
  { icon: '💜', top: '30%', left: '3%', size: 14, opacity: 0.4 },
  { icon: '✨', top: '55%', left: '95%', size: 16, opacity: 0.5 },
  { icon: '💫', top: '78%', left: '4%', size: 18, opacity: 0.4 },
];

export const GameScreen: React.FC<GameScreenProps> = ({
  levelId,
  onLevelComplete,
  onLevelFailed,
  onBack,
}) => {
  const engineRef = useRef<GameEngine>(new GameEngine());
  const levelLoaderRef = useRef<LevelLoader>(new LevelLoader());
  const haptics = useMemo(() => HapticManager.getInstance(), []);
  const audio = useMemo(() => AudioManager.getInstance(), []);

  const [gameState, setGameState] = useState<GameStateData | null>(null);
  const [renderedTiles, setRenderedTiles] = useState<RenderedTile[]>([]);
  const [effects, setEffects] = useState<any[]>([]);
  const [isAnimating, setIsAnimating] = useState(false);

  const shakeX = useSharedValue(0);
  const shakeStyle = useAnimatedStyle(() => ({
    transform: [{ translateX: shakeX.value }]
  }));

  const triggerShake = useCallback((intensity: number) => {
    shakeX.value = withSequence(
      withTiming(intensity, { duration: 50 }),
      withTiming(-intensity, { duration: 50 }),
      withTiming(intensity / 2, { duration: 50 }),
      withTiming(-intensity / 2, { duration: 50 }),
      withTiming(0, { duration: 50 })
    );
  }, [shakeX]);

  // Load member images via Skia
  const memberImages = {
    rm: useImage(MEMBER_CONFIGS.rm.portrait),
    jin: useImage(MEMBER_CONFIGS.jin.portrait),
    suga: useImage(MEMBER_CONFIGS.suga.portrait),
    jhope: useImage(MEMBER_CONFIGS.jhope.portrait),
    jimin: useImage(MEMBER_CONFIGS.jimin.portrait),
    v: useImage(MEMBER_CONFIGS.v.portrait),
    jungkook: useImage(MEMBER_CONFIGS.jungkook.portrait),
  };

  const removeEffect = useCallback((id: string) => {
    setEffects(prev => prev.filter(e => e.id !== id));
  }, []);

  // Reconciliation: Sync GameEngine board with RenderedTiles
  const syncTiles = useCallback((state: GameStateData) => {
    setRenderedTiles(prev => {
      const nextTiles = [...prev];
      const currentBoardTileIds = new Set<string>();

      for (let y = 0; y < state.board.rows; y++) {
        for (let x = 0; x < state.board.cols; x++) {
          const tile = state.board.getTile(x, y);
          if (tile) {
            currentBoardTileIds.add(tile.id);
            const existingIdx = nextTiles.findIndex(t => t.id === tile.id);
            if (existingIdx >= 0) {
              nextTiles[existingIdx] = {
                ...nextTiles[existingIdx],
                x: tile.x,
                y: tile.y,
                special: tile.special,
                obstacle: tile.obstacle
              };
            } else {
              nextTiles.push({ ...tile, isMatched: false });
            }
          }
        }
      }

      let hasMatches = false;
      let hasSpecials = false;
      const newEffects: any[] = [];

      for (let i = 0; i < nextTiles.length; i++) {
        const t = nextTiles[i];
        if (!currentBoardTileIds.has(t.id) && !t.isMatched) {
          // Clone the object to prevent Reanimated worklet mutation errors!
          nextTiles[i] = { ...t, isMatched: true };
          hasMatches = true;

          if (t.special) {
            hasSpecials = true;
            audio.playSFX('explosion');
          } else {
            audio.playSFX('match');
          }

          // Spawn match effect
          newEffects.push({
            id: `effect_${t.id}_${Date.now()}_${Math.random()}`,
            x: t.x,
            y: t.y,
            color: t.member ? MEMBER_CONFIGS[t.member].themeColor : '#ffffff',
            type: t.special ? 'explosion' : 'match'
          });
        }
      }

      if (hasSpecials) triggerShake(15);

      if (newEffects.length > 0) {
        setEffects(prevEffects => [...prevEffects, ...newEffects]);
      }

      if (hasMatches) {
        setTimeout(() => {
          setRenderedTiles(current => current.filter(t => !t.isMatched));
        }, 300);
      }

      return nextTiles;
    });
  }, [audio, triggerShake]);

  // Initialize level
  useEffect(() => {
    const levelDef = levelLoaderRef.current.getLevel(levelId);
    if (levelDef) {
      engineRef.current.initLevel(levelDef);
      const state = engineRef.current.getState();
      setGameState(state);
      syncTiles(state);
    }
  }, [levelId, syncTiles]);

  const boardRows = gameState?.board?.rows ?? 6;
  const boardCols = gameState?.board?.cols ?? 6;
  const BOARD_PADDING = 16;
  const availableWidth = SCREEN_WIDTH - BOARD_PADDING * 2;
  const TILE_SIZE = Math.floor(availableWidth / boardCols);
  const BOARD_WIDTH_PX = TILE_SIZE * boardCols;
  const BOARD_HEIGHT_PX = TILE_SIZE * boardRows;

  const dragX = useSharedValue(0);
  const dragY = useSharedValue(0);
  const activeDragPos = useSharedValue<{x: number, y: number} | null>(null);

  const processSwap = useCallback((x1: number, y1: number, x2: number, y2: number) => {
    if (isAnimating) {
      activeDragPos.value = null;
      dragX.value = 0;
      dragY.value = 0;
      return;
    }
    setIsAnimating(true);

    const engine = engineRef.current;
    const valid = engine.trySwap(x1, y1, x2, y2);

    if (valid) {
      // VALID SWAP!
      audio.playSFX('swap');
      haptics.trigger('tileSwap');

      // Update React state (which sets new targetX/targetY). 
      // The TileRenderer's useEffect will natively absorb the dragX/dragY offset 
      // and immediately clear activeDragPos to prevent snapbacks.
      const state = engine.getState();
      setGameState({ ...state });
      syncTiles(state);

      // Start the cascade animation loop
      const animateLoop = () => {
        const loopState = engine.getState();
        setGameState({ ...loopState });
        syncTiles(loopState);

        if (loopState.phase === GamePhase.LEVEL_COMPLETE) {
          haptics.trigger('victory');
          audio.playSFX('victory');
          setTimeout(() => onLevelComplete?.(loopState.score, calculateStars(loopState.score)), 1000);
          return;
        }

        if (loopState.phase === GamePhase.LEVEL_FAILED) {
          haptics.trigger('failure');
          audio.playSFX('failure');
          setTimeout(() => onLevelFailed?.(), 1000);
          return;
        }

        if (loopState.phase === GamePhase.IDLE) {
          setIsAnimating(false);
          return;
        }

        if (loopState.phase === GamePhase.CASCADE) {
          haptics.trigger('cascade');
          audio.playSFX('cascade');
          setTimeout(() => {
             engine.resolveMatches();
             animateLoop();
          }, 150);
          return;
        }

        if (loopState.phase === GamePhase.FALLING || loopState.phase === GamePhase.SPAWNING) {
          setTimeout(() => {
             engine.resolveMatches();
             animateLoop();
          }, 150);
          return;
        }

        setTimeout(() => {
           engine.resolveMatches();
           animateLoop();
        }, 150);
      };
      
      // Start immediately
      setTimeout(animateLoop, 20);

    } else {
      // INVALID SWAP!
      // Animate the tiles physically bouncing back from their dragged position
      audio.playSFX('swap');
      
      dragX.value = withSpring(0, { damping: 14, stiffness: 250 });
      dragY.value = withSpring(0, { damping: 14, stiffness: 250 });
      
      setTimeout(() => {
        activeDragPos.value = null;
        setIsAnimating(false);
      }, 150);
    }
  }, [activeDragPos, dragX, dragY, audio, haptics, onLevelComplete, onLevelFailed]);


  const isAnimatingShared = useSharedValue(isAnimating);
  useEffect(() => {
    isAnimatingShared.value = isAnimating;
  }, [isAnimating]);

  const pan = Gesture.Pan()
    .runOnJS(true)
    .onBegin((e) => {
      if (isAnimatingShared.value || !gameState) return;
      const col = Math.floor(e.x / TILE_SIZE);
      const row = Math.floor(e.y / TILE_SIZE);
      activeDragPos.value = { x: col, y: row };
      dragX.value = 0;
      dragY.value = 0;
    })
    .onUpdate((e) => {
      if (isAnimatingShared.value || !activeDragPos.value) return;

      const dx = e.translationX;
      const dy = e.translationY;

      // Determine dominant axis
      if (Math.abs(dx) > Math.abs(dy)) {
        dragX.value = Math.max(-TILE_SIZE, Math.min(TILE_SIZE, dx));
        dragY.value = 0;
      } else {
        dragX.value = 0;
        dragY.value = Math.max(-TILE_SIZE, Math.min(TILE_SIZE, dy));
      }
    })
    .onEnd((e) => {
      if (isAnimatingShared.value || !activeDragPos.value) return;

      const x1 = activeDragPos.value.x;
      const y1 = activeDragPos.value.y;
      const finalDragX = dragX.value;
      const finalDragY = dragY.value;

      if (Math.abs(finalDragX) < 15 && Math.abs(finalDragY) < 15) {
        // Clear drag state visually
        activeDragPos.value = null;
        dragX.value = 0;
        dragY.value = 0;
        return; // Too small, ignore
      }

      let x2 = x1;
      let y2 = y1;

      if (Math.abs(finalDragX) > Math.abs(finalDragY)) {
        x2 = finalDragX > 0 ? x1 + 1 : x1 - 1;
      } else {
        y2 = finalDragY > 0 ? y1 + 1 : y1 - 1;
      }

      if (x2 < 0 || x2 >= boardCols || y2 < 0 || y2 >= boardRows) {
        activeDragPos.value = null;
        dragX.value = 0;
        dragY.value = 0;
        return;
      }

      // DO NOT clear drag state here! Handoff to processSwap.
      processSwap(x1, y1, x2, y2);
    });

  const calculateStars = (score: number): number => {
    const levelDef = levelLoaderRef.current.getLevel(levelId);
    if (!levelDef) return 1;
    if (score >= levelDef.starThresholds[2]) return 3;
    if (score >= levelDef.starThresholds[1]) return 2;
    return 1;
  };

  const getComboText = (): string | null => {
    if (!gameState) return null;
    if (gameState.cascadeCount >= 5) return 'ENCORE!';
    if (gameState.cascadeCount >= 4) return 'AMAZING!';
    if (gameState.cascadeCount >= 3) return 'GREAT!';
    if (gameState.cascadeCount >= 2) return 'GOOD!';
    return null;
  };

  const allImagesLoaded = Object.values(memberImages).every(img => img !== null);
  if (!gameState || !allImagesLoaded) {
    return (
      <View style={styles.loadingContainer}>
        <RNText style={styles.loadingText}>Loading...</RNText>
      </View>
    );
  }

  const comboText = getComboText();
  const stars = calculateStars(gameState.score);

  // Primary collectible objective (drives the "Collect Members" card). Falls back gracefully
  // if this level has no collect-type objective defined.
  const collectObjective = gameState.objectives.find(
    (o: any) => o.definition?.type === 'collect' || o.definition?.member
  ) ?? gameState.objectives[0];
  const collectMember = collectObjective?.definition?.member as keyof typeof MEMBER_CONFIGS | undefined;

  return (
    <ImageBackground 
      source={require('../../assets/bg.png')} 
      style={styles.container}
      resizeMode="cover"
    >

      {/* Ambient floating hearts / sparkles */}
      {AMBIENT_DECOR.map((d, i) => (
        <RNText
          key={`decor_${i}`}
          style={[
            styles.ambientIcon,
            { top: d.top as any, left: d.left as any, fontSize: d.size, opacity: d.opacity },
          ]}
        >
          {d.icon}
        </RNText>
      ))}

      {/* Top Header */}
      <View style={styles.header}>
        <TouchableOpacity style={styles.iconBtn} onPress={onBack} activeOpacity={0.8}>
          <RNText style={styles.iconBtnText}>{'‹'}</RNText>
        </TouchableOpacity>

        <View style={styles.headerCenter}>
          <RNText style={styles.headerTitle}>BTS</RNText>
          <RNText style={styles.headerSubtitle}>Together with ARMY</RNText>
        </View>

        <TouchableOpacity style={styles.iconBtn} onPress={() => engineRef.current.pause()} activeOpacity={0.8}>
          <RNText style={styles.iconBtnText}>{'⚙'}</RNText>
        </TouchableOpacity>
      </View>

      {/* Status Panel (Glassmorphism) */}
      <View style={styles.statusPanel}>
        <View style={styles.statusRow}>
          {/* Level pill */}
          <View style={styles.levelCol}>
            <View style={styles.levelPill}>
              <RNText style={styles.levelText}>LEVEL {gameState.levelId}</RNText>
            </View>
            <RNText style={styles.levelSubtext}>Let's go{'\n'}ARMY!</RNText>
          </View>

          {/* Collect members card */}
          <View style={styles.collectCard}>
            <RNText style={styles.collectTitle}>Collect Members</RNText>
            <View style={styles.collectAvatarWrap}>
              {collectMember ? (
                <Image 
                  source={MEMBER_CONFIGS[collectMember as keyof typeof MEMBER_CONFIGS]?.portrait} 
                  style={{ width: 50, height: 50 }} 
                  resizeMode="cover" 
                />
              ) : (
                <View style={styles.collectAvatarFallback} />
              )}
            </View>
            <RNText style={styles.collectCount}>
              {collectObjective?.current ?? 0} / {collectObjective?.definition?.target ?? '-'}
            </RNText>
          </View>

          {/* Score card */}
          <View style={styles.scoreCard}>
            <View style={styles.scoreHeaderRow}>
              <RNText style={styles.crownIcon}>👑</RNText>
              <RNText style={styles.scoreTitle}>Score</RNText>
            </View>
            <RNText style={styles.scoreValue}>{gameState.score.toLocaleString()}</RNText>
            <View style={styles.starsRow}>
              {[0, 1, 2].map(i => (
                <RNText key={i} style={[styles.starPip, i < stars ? styles.starPipActive : styles.starPipInactive]}>
                  ★
                </RNText>
              ))}
            </View>
          </View>

          {/* Moves badge */}
          <View style={styles.movesPill}>
            <RNText style={styles.movesLabel}>MOVES</RNText>
            <RNText style={styles.movesValue}>{gameState.movesRemaining}</RNText>
          </View>
        </View>
      </View>

      {comboText && (
        <View style={styles.comboOverlay}>
          <RNText style={styles.comboText}>{comboText}</RNText>
        </View>
      )}

      {/* Board */}
      <View style={styles.boardFrame}>
        <GestureDetector gesture={pan}>
          <Animated.View style={shakeStyle}>
            <Canvas style={[styles.canvas, { width: BOARD_WIDTH_PX, height: BOARD_HEIGHT_PX }]}>
              <RoundedRect x={0} y={0} width={BOARD_WIDTH_PX} height={BOARD_HEIGHT_PX} r={16} color="rgba(26, 22, 56, 0.6)" />

              {/* Cell Backgrounds */}
              {Array.from({ length: boardRows }, (_, y) =>
                Array.from({ length: boardCols }, (_, x) => {
                  const cell = gameState.board.getCell(x, y);
                  if (!cell || cell.type === 'hole') return null;
                  const px = x * TILE_SIZE;
                  const py = y * TILE_SIZE;
                  const cellBg = cell.type === 'blocked' ? '#3a3355' : '#2a2454';
                  return (
                    <RoundedRect key={`cell_${x}_${y}`} x={px + 3} y={py + 3} width={TILE_SIZE - 6} height={TILE_SIZE - 6} r={TILE_SIZE / 2} color={cellBg} />
                  );
                })
              )}

              {/* Effects (under tiles) */}
              {effects.map(effect => (
                <EffectRenderer
                  key={effect.id}
                  effect={effect}
                  tileSize={TILE_SIZE}
                  onComplete={removeEffect}
                />
              ))}

              {/* Animated Tiles */}
              {renderedTiles.map(tile => (
                <TileRenderer
                  key={tile.id}
                  tile={tile}
                  tileSize={TILE_SIZE}
                  image={tile.member ? memberImages[tile.member] : null}
                  isMatched={tile.isMatched}
                  dragX={dragX}
                  dragY={dragY}
                  activeDragPos={activeDragPos}
                />
              ))}
            </Canvas>
          </Animated.View>
        </GestureDetector>
      </View>

      {/* Power-up bar */}
      <View style={styles.powerBar}>
        {POWER_UPS.map(p => (
          <TouchableOpacity key={p.id} style={styles.powerSlot} activeOpacity={0.75}>
            <Image source={p.img} style={{ width: 36, height: 36 }} resizeMode="contain" />
            <View style={styles.powerBadge}>
              <RNText style={styles.powerBadgeText}>{p.count}</RNText>
            </View>
          </TouchableOpacity>
        ))}
      </View>

      <RNText style={styles.phaseText}>
        {gameState.phase}{gameState.cascadeCount > 0 ? ` | Cascade: ${gameState.cascadeCount}` : ''}
      </RNText>

      {gameState.phase === GamePhase.PAUSED && (
        <View style={styles.resultOverlay}>
          <RNText style={styles.resultTitle}>Paused</RNText>
          <TouchableOpacity
            style={{ backgroundColor: '#9b59b6', padding: 16, borderRadius: 12, marginBottom: 16, width: 200, alignItems: 'center' }}
            onPress={() => engineRef.current.resume()}
          >
            <RNText style={{ color: '#fff', fontSize: 18, fontWeight: 'bold' }}>Resume</RNText>
          </TouchableOpacity>
          <TouchableOpacity
            style={{ backgroundColor: '#555', padding: 16, borderRadius: 12, width: 200, alignItems: 'center' }}
            onPress={onBack}
          >
            <RNText style={{ color: '#fff', fontSize: 18, fontWeight: 'bold' }}>Quit Level</RNText>
          </TouchableOpacity>
        </View>
      )}

      {gameState.phase === GamePhase.LEVEL_COMPLETE && (
        <View style={styles.resultOverlay}>
          <RNText style={styles.resultTitle}>Stage Clear!</RNText>
          <RNText style={styles.resultScore}>Score: {gameState.score}</RNText>
          <RNText style={styles.resultStars}>
            {'★'.repeat(stars)}{'☆'.repeat(3 - stars)}
          </RNText>
        </View>
      )}

      {gameState.phase === GamePhase.LEVEL_FAILED && (
        <View style={[styles.resultOverlay, { backgroundColor: 'rgba(120, 0, 40, 0.92)' }]}>
          <RNText style={styles.resultTitle}>Try Again!</RNText>
          <RNText style={styles.resultScore}>Score: {gameState.score}</RNText>
        </View>
      )}
    </ImageBackground>
  );
};

const styles = StyleSheet.create({
  container: {
    flex: 1,
    backgroundColor: '#160b30',
    alignItems: 'center',
  },
  loadingContainer: {
    flex: 1,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: '#160b30',
  },
  loadingText: {
    color: '#fff',
    fontSize: 20,
  },

  // ---- Background ----
  bgGradients: {
    ...StyleSheet.absoluteFill,
    backgroundColor: '#1c0f3f',
  },
  bgTop: {
    position: 'absolute',
    top: 0,
    left: 0,
    right: 0,
    height: '35%',
    backgroundColor: '#2d1660',
    opacity: 0.9,
  },
  bgMid: {
    position: 'absolute',
    top: '20%',
    left: 0,
    right: 0,
    height: '45%',
    backgroundColor: '#1c0f3f',
  },
  bgBottom: {
    position: 'absolute',
    bottom: 0,
    left: 0,
    right: 0,
    height: '30%',
    backgroundColor: '#120825',
  },
  bgGlowLeft: {
    position: 'absolute',
    top: '5%',
    left: -60,
    width: 160,
    height: 160,
    borderRadius: 80,
    backgroundColor: '#a06cff',
    opacity: 0.18,
  },
  bgGlowRight: {
    position: 'absolute',
    top: '2%',
    right: -60,
    width: 180,
    height: 180,
    borderRadius: 90,
    backgroundColor: '#ff9ff3',
    opacity: 0.15,
  },
  ambientIcon: {
    position: 'absolute',
    zIndex: 1,
  },

  // ---- Header ----
  header: {
    width: '100%',
    flexDirection: 'row',
    justifyContent: 'space-between',
    alignItems: 'center',
    paddingHorizontal: 20,
    paddingTop: 50,
    paddingBottom: 8,
    zIndex: 2,
  },
  iconBtn: {
    width: 44,
    height: 44,
    borderRadius: 22,
    backgroundColor: 'rgba(255,255,255,0.16)',
    justifyContent: 'center',
    alignItems: 'center',
    borderWidth: 1,
    borderColor: 'rgba(255,255,255,0.35)',
  },
  iconBtnText: {
    color: '#fff',
    fontSize: 20,
    fontWeight: 'bold',
  },
  headerCenter: {
    position: 'absolute',
    left: 0,
    right: 0,
    top: 0,
    alignItems: 'center',
    zIndex: 10,
    elevation: 10,
  },
  headerTitle: {
    color: '#fff',
    fontSize: 48,
    fontWeight: '900',
    letterSpacing: 6,
    textShadowColor: 'rgba(255, 255, 255, 0.9)',
    textShadowOffset: { width: 0, height: 0 },
    textShadowRadius: 15,
  },
  headerSubtitle: {
    color: '#fff',
    fontSize: 16,
    fontStyle: 'italic',
    marginTop: -5,
    textShadowColor: 'rgba(255, 255, 255, 0.8)',
    textShadowOffset: { width: 0, height: 0 },
    textShadowRadius: 10,
  },

  // ---- Status panel ----
  statusPanel: {
    width: '92%',
    backgroundColor: 'rgba(255, 255, 255, 0.12)',
    borderRadius: 22,
    padding: 10,
    marginBottom: 16,
    borderWidth: 1,
    borderColor: 'rgba(255, 255, 255, 0.35)',
    shadowColor: '#fff',
    shadowOpacity: 0.15,
    shadowRadius: 10,
    shadowOffset: { width: 0, height: 0 },
    zIndex: 2,
  },
  statusRow: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  levelCol: {
    alignItems: 'flex-start',
    marginRight: 6,
  },
  levelPill: {
    backgroundColor: '#4a3580',
    paddingHorizontal: 12,
    paddingVertical: 5,
    borderRadius: 16,
  },
  levelText: {
    color: '#fff',
    fontSize: 11,
    fontWeight: 'bold',
  },
  levelSubtext: {
    color: 'rgba(255,255,255,0.55)',
    fontSize: 9,
    marginTop: 4,
    lineHeight: 11,
  },
  collectCard: {
    width: 90,
    backgroundColor: 'rgba(255, 240, 235, 1)',
    borderRadius: 24,
    paddingVertical: 12,
    paddingHorizontal: 8,
    marginHorizontal: 12,
    alignItems: 'center',
    marginTop: -25, // Pops out of the top
    marginBottom: -15, // Pops out of the bottom
    shadowColor: '#000',
    shadowOpacity: 0.2,
    shadowRadius: 10,
    shadowOffset: { width: 0, height: 5 },
    elevation: 5,
  },
  collectTitle: {
    color: '#8a5a6a',
    fontSize: 12,
    fontWeight: 'bold',
    marginBottom: 6,
    textAlign: 'center',
    lineHeight: 14,
  },
  collectAvatarWrap: {
    width: 50,
    height: 50,
    borderRadius: 25,
    backgroundColor: '#8a5a6a',
    overflow: 'hidden',
    marginBottom: 6,
    borderWidth: 2,
    borderColor: '#ff9ff3',
    justifyContent: 'center',
    alignItems: 'center',
  },
  collectAvatarCanvas: {
    width: 50,
    height: 50,
  },
  collectAvatarFallback: {
    width: 50,
    height: 50,
    borderRadius: 25,
    backgroundColor: '#6A4F9E',
  },
  collectCount: {
    color: '#5a3a4a',
    fontSize: 16,
    fontWeight: 'bold',
  },
  scoreCard: {
    flex: 1,
    backgroundColor: 'rgba(255, 255, 255, 0.16)',
    borderRadius: 16,
    paddingVertical: 6,
    paddingHorizontal: 8,
    marginRight: 6,
    alignItems: 'center',
  },
  scoreHeaderRow: {
    flexDirection: 'row',
    alignItems: 'center',
  },
  crownIcon: {
    fontSize: 12,
    marginRight: 3,
  },
  scoreTitle: {
    color: '#FFD1DC',
    fontSize: 11,
    fontWeight: 'bold',
  },
  scoreValue: {
    color: '#fff',
    fontSize: 18,
    fontWeight: 'bold',
    marginTop: 2,
  },
  starsRow: {
    flexDirection: 'row',
    marginTop: 3,
  },
  starPip: {
    fontSize: 13,
    marginHorizontal: 1,
  },
  starPipActive: {
    color: '#FFD700',
  },
  starPipInactive: {
    color: 'rgba(255,255,255,0.3)',
  },
  movesPill: {
    backgroundColor: '#6A4F9E',
    width: 58,
    height: 58,
    borderRadius: 29,
    justifyContent: 'center',
    alignItems: 'center',
    borderWidth: 2,
    borderColor: 'rgba(255,255,255,0.5)',
  },
  movesLabel: {
    color: '#FFD1DC',
    fontSize: 9,
    fontWeight: 'bold',
  },
  movesValue: {
    color: '#fff',
    fontSize: 22,
    fontWeight: 'bold',
  },

  // ---- Combo ----
  comboOverlay: {
    position: 'absolute',
    top: '40%',
    zIndex: 10,
    backgroundColor: 'rgba(0,0,0,0.7)',
    paddingHorizontal: 30,
    paddingVertical: 10,
    borderRadius: 20,
  },
  comboText: {
    color: '#FFD700',
    fontSize: 32,
    fontWeight: 'bold',
    textAlign: 'center',
    textShadowColor: '#ff9ff3',
    textShadowOffset: { width: 0, height: 0 },
    textShadowRadius: 10,
  },

  // ---- Board ----
  boardFrame: {
    padding: 8,
    borderRadius: 24,
    backgroundColor: 'rgba(255,255,255,0.06)',
    borderWidth: 1,
    borderColor: 'rgba(255,255,255,0.25)',
    zIndex: 2,
  },
  canvas: {
    borderRadius: 18,
    overflow: 'hidden',
  },

  // ---- Power-up bar ----
  powerBar: {
    marginTop: 14,
    flexDirection: 'row',
    backgroundColor: 'rgba(20, 10, 40, 0.75)',
    borderRadius: 20,
    borderWidth: 1,
    borderColor: 'rgba(255,255,255,0.2)',
    paddingVertical: 8,
    paddingHorizontal: 10,
    zIndex: 2,
  },
  powerSlot: {
    width: 48,
    height: 48,
    borderRadius: 14,
    backgroundColor: 'rgba(255,255,255,0.08)',
    marginHorizontal: 4,
    justifyContent: 'center',
    alignItems: 'center',
  },
  powerIcon: {
    fontSize: 20,
  },
  powerBadge: {
    position: 'absolute',
    bottom: -4,
    right: -4,
    backgroundColor: '#4a3580',
    borderRadius: 8,
    minWidth: 16,
    height: 16,
    justifyContent: 'center',
    alignItems: 'center',
    paddingHorizontal: 3,
    borderWidth: 1,
    borderColor: 'rgba(255,255,255,0.6)',
  },
  powerBadgeText: {
    color: '#fff',
    fontSize: 9,
    fontWeight: 'bold',
  },

  phaseText: {
    color: 'rgba(255,255,255,0.35)',
    fontSize: 10,
    marginTop: 8,
  },

  // ---- Overlays ----
  resultOverlay: {
    position: 'absolute',
    top: 0,
    left: 0,
    right: 0,
    bottom: 0,
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: 'rgba(30, 0, 80, 0.9)',
    zIndex: 20,
  },
  resultTitle: {
    color: '#FFD700',
    fontSize: 36,
    fontWeight: 'bold',
    marginBottom: 16,
  },
  resultScore: {
    color: '#fff',
    fontSize: 24,
    marginBottom: 12,
  },
  resultStars: {
    fontSize: 48,
    color: '#FFD700',
  },
});