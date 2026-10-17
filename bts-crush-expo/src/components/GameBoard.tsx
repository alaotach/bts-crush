import React, { useEffect, useState, useCallback } from 'react';
import { StyleSheet, Dimensions, Text as RNText } from 'react-native';
import { Canvas, Group, Image, useImage, RoundedRect, Rect } from '@shopify/react-native-skia';
import { generateInitialBoard, BOARD_WIDTH, BOARD_HEIGHT, TileData, swapTiles, processMatches, applyGravity } from '../utils/gameLogic';
import { BTS_MEMBERS } from '../constants/btsAssets';
import { GestureDetector, Gesture } from 'react-native-gesture-handler';
import * as Haptics from 'expo-haptics';

const { width } = Dimensions.get('window');
const TILE_SIZE = width / BOARD_WIDTH;

export const GameBoard = () => {
  const [board, setBoard] = useState<TileData[][]>([]);
  const [isProcessing, setIsProcessing] = useState(false);
  const [score, setScore] = useState(0);
  const [combo, setCombo] = useState(0);

  // Load images
  const images = {
    rm: useImage(BTS_MEMBERS.rm.image),
    jin: useImage(BTS_MEMBERS.jin.image),
    suga: useImage(BTS_MEMBERS.suga.image),
    jhope: useImage(BTS_MEMBERS.jhope.image),
    jimin: useImage(BTS_MEMBERS.jimin.image),
    v: useImage(BTS_MEMBERS.v.image),
    jungkook: useImage(BTS_MEMBERS.jungkook.image),
  };

  useEffect(() => {
    setBoard(generateInitialBoard());
  }, []);

  const resolveBoard = useCallback(async (currentBoard: TileData[][], currentCombo: number) => {
    setIsProcessing(true);
    let tempBoard = currentBoard;
    let comboMultiplier = currentCombo;

    while (true) {
      const matchResult = processMatches(tempBoard);
      if (!matchResult.hasMatches) break;

      comboMultiplier++;
      setCombo(comboMultiplier);
      setScore(s => s + (matchResult.clearedCount * 10 * comboMultiplier));
      
      // Haptics based on combo
      if (comboMultiplier > 3) Haptics.notificationAsync(Haptics.NotificationFeedbackType.Success);
      else Haptics.impactAsync(Haptics.ImpactFeedbackStyle.Medium);

      // Show matched state briefly
      setBoard(matchResult.newBoard);
      await new Promise(resolve => setTimeout(resolve, 300)); 

      // Apply gravity
      tempBoard = applyGravity(matchResult.newBoard);
      setBoard(tempBoard);
      await new Promise(resolve => setTimeout(resolve, 300));
    }
    
    setIsProcessing(false);
    if (comboMultiplier === 0) {
      setCombo(0);
    }
  }, []);

  const handleSwipe = useCallback((startX: number, startY: number, endX: number, endY: number) => {
    if (isProcessing) return;

    const x1 = Math.floor(startX / TILE_SIZE);
    const y1 = Math.floor(startY / TILE_SIZE);
    
    // Determine direction
    const dx = endX - startX;
    const dy = endY - startY;
    
    if (Math.abs(dx) < 20 && Math.abs(dy) < 20) return; // Too small swipe

    let x2 = x1;
    let y2 = y1;

    if (Math.abs(dx) > Math.abs(dy)) {
      x2 = dx > 0 ? x1 + 1 : x1 - 1;
    } else {
      y2 = dy > 0 ? y1 + 1 : y1 - 1;
    }

    // Bounds check
    if (x2 < 0 || x2 >= BOARD_WIDTH || y2 < 0 || y2 >= BOARD_HEIGHT) return;

    // Swap
    Haptics.impactAsync(Haptics.ImpactFeedbackStyle.Light);
    const swappedBoard = swapTiles(board, x1, y1, x2, y2);
    setBoard(swappedBoard);

    // Check if swap is valid
    const matchCheck = processMatches(swappedBoard);
    if (matchCheck.hasMatches) {
      resolveBoard(swappedBoard, 0);
    } else {
      // Revert swap after short delay
      setTimeout(() => {
        setBoard(board);
        Haptics.notificationAsync(Haptics.NotificationFeedbackType.Error);
      }, 300);
    }
  }, [board, isProcessing, resolveBoard]);

  const pan = Gesture.Pan()
    .runOnJS(true)
    .onEnd((e) => {
      handleSwipe(e.x - e.translationX, e.y - e.translationY, e.x, e.y);
    });

  const allImagesLoaded = Object.values(images).every(img => img !== null);
  if (!allImagesLoaded) return null;

  return (
    <>
      <GestureDetector gesture={pan}>
        <Canvas style={styles.canvas}>
          <Rect x={0} y={0} width={width} height={TILE_SIZE * BOARD_HEIGHT} color="#1a1a2e" />

          {board.map((row, y) =>
            row.map((tile, x) => {
              const img = images[tile.member];
              if (!img) return null;
              
              // If marked for clear, shrink/hide it (simple visual trick before gravity applies)
              const visualSize = tile.markedForClear ? 0 : TILE_SIZE - 8;
              const offset = tile.markedForClear ? TILE_SIZE / 2 : 4;

              return (
                <Group key={tile.id} transform={[{ translateX: x * TILE_SIZE }, { translateY: y * TILE_SIZE }]}>
                  <RoundedRect x={2} y={2} width={TILE_SIZE - 4} height={TILE_SIZE - 4} color={tile.markedForClear ? '#fff' : BTS_MEMBERS[tile.member].themeColor} r={10} />
                  {visualSize > 0 && (
                    <Image 
                      image={img}
                      x={offset}
                      y={offset}
                      width={visualSize}
                      height={visualSize}
                      fit="contain"
                    />
                  )}
                </Group>
              );
            })
          )}
        </Canvas>
      </GestureDetector>
      {/* Simple Score UI overlay in React Native, outside Skia Canvas */}
      <React.Fragment>
          <RNText style={styles.scoreText}>Score: {score}</RNText>
          {combo > 1 && <RNText style={styles.comboText}>{combo}x COMBO!</RNText>}
      </React.Fragment>
    </>
  );
};

const styles = StyleSheet.create({
  canvas: {
    width: width,
    height: TILE_SIZE * BOARD_HEIGHT,
    backgroundColor: '#1a1a2e',
    borderRadius: 15,
    overflow: 'hidden'
  },
  scoreText: {
    position: 'absolute',
    top: -40,
    left: 20,
    fontSize: 24,
    color: 'white',
    fontWeight: 'bold'
  },
  comboText: {
    position: 'absolute',
    top: -40,
    right: 20,
    fontSize: 24,
    color: '#ff9ff3',
    fontWeight: 'bold'
  }
});
