import React, { useEffect } from 'react';
import { Group, RoundedRect, Image, Circle, Shadow } from '@shopify/react-native-skia';
import { useSharedValue, useDerivedValue, withSpring, withTiming, Easing, withSequence, runOnJS } from 'react-native-reanimated';
import { Tile } from '../game/Tile';
import { MEMBER_CONFIGS } from '../bts/members';

import { SharedValue } from 'react-native-reanimated';

interface TileRendererProps {
  tile: Tile;
  tileSize: number;
  image: any;
  isMatched: boolean;
  dragX?: SharedValue<number>;
  dragY?: SharedValue<number>;
  activeDragPos?: SharedValue<{x: number, y: number} | null>;
}

export const TileRenderer: React.FC<TileRendererProps> = ({ tile, tileSize, image, isMatched, dragX, dragY, activeDragPos }) => {
  const pad = 3;
  const innerSize = tileSize - pad * 2;
  const targetX = tile.x * tileSize;
  const targetY = tile.y * tileSize;

  // Animation values
  const translateX = useSharedValue(targetX);
  // Start completely above the board so it falls all the way down
  const translateY = useSharedValue(-tileSize);
  
  // Start slightly small and transparent for entry animation
  const scale = useSharedValue(0.85);
  const opacity = useSharedValue(0);

  // Entry animation
  useEffect(() => {
    scale.value = withSequence(
      withTiming(1.03, { duration: 150, easing: Easing.out(Easing.ease) }),
      withTiming(1, { duration: 100 })
    );
    opacity.value = withTiming(1, { duration: 150 });
  }, []);

  const lastExtraX = useSharedValue(0);
  const lastExtraY = useSharedValue(0);

  const transform = useDerivedValue(() => {
    let extraX = 0;
    let extraY = 0;
    let extraScale = 0;

    if (activeDragPos && activeDragPos.value && dragX && dragY) {
      const isDragged = activeDragPos.value.x === tile.x && activeDragPos.value.y === tile.y;
      
      const dx = activeDragPos.value.x - tile.x;
      const dy = activeDragPos.value.y - tile.y;
      const dragXVal = dragX.value;
      const dragYVal = dragY.value;

      let isNeighbor = false;
      if (Math.abs(dragXVal) > Math.abs(dragYVal)) {
        if (dragXVal > 0 && dx === -1 && dy === 0) isNeighbor = true;
        if (dragXVal < 0 && dx === 1 && dy === 0) isNeighbor = true;
      } else {
        if (dragYVal > 0 && dy === -1 && dx === 0) isNeighbor = true;
        if (dragYVal < 0 && dy === 1 && dx === 0) isNeighbor = true;
      }

      if (isDragged) {
        extraX = dragXVal;
        extraY = dragYVal;
        extraScale = 0.05; // tiny lift
      } else if (isNeighbor) {
        extraX = -dragXVal;
        extraY = -dragYVal;
      }
    }

    lastExtraX.value = extraX;
    lastExtraY.value = extraY;

    return [
      { translateX: translateX.value + extraX },
      { translateY: translateY.value + extraY },
      { scale: scale.value + extraScale }
    ];
  });

  // When target x/y changes, animate to new position (swaps and falls)
  useEffect(() => {
    if (translateX.value !== targetX || translateY.value !== targetY) {
      // Absorb the active drag offset so the spring starts exactly from where the user dragged it
      if (lastExtraX.value !== 0) {
        translateX.value += lastExtraX.value;
        lastExtraX.value = 0;
        // Handoff: clear the global drag so transform stops adding it
        if (dragX) dragX.value = 0;
        if (activeDragPos) activeDragPos.value = null;
      }
      if (lastExtraY.value !== 0) {
        translateY.value += lastExtraY.value;
        lastExtraY.value = 0;
        if (dragY) dragY.value = 0;
        if (activeDragPos) activeDragPos.value = null;
      }

      // Use spring for physical-feeling snaps and drops
      translateX.value = withSpring(targetX, { damping: 14, stiffness: 150 });
      translateY.value = withSpring(targetY, { damping: 14, stiffness: 150 });
    }
  }, [targetX, targetY]);

  // When matched, run a destruction animation
  useEffect(() => {
    if (isMatched) {
      scale.value = withSequence(
        withTiming(1.05, { duration: 80, easing: Easing.out(Easing.ease) }),
        withTiming(0.95, { duration: 80 }),
        withTiming(0.7, { duration: 100 }),
        withTiming(0, { duration: 50 })
      );
      opacity.value = withTiming(0, { duration: 310 });
      translateY.value = withTiming(targetY - 15, { duration: 310, easing: Easing.out(Easing.ease) });
    }
  }, [isMatched]);

  if (!tile.member) return null;

  return (
    <Group 
      transform={transform}
      origin={{ x: tileSize / 2, y: tileSize / 2 }} // Local center
      opacity={opacity}
    >
      {/* Cheap glowing border (fast) */}
      <Circle cx={tileSize / 2} cy={tileSize / 2} r={(innerSize / 2) + 2} color={MEMBER_CONFIGS[tile.member].themeColor} opacity={0.3} />
      <Circle cx={tileSize / 2} cy={tileSize / 2} r={innerSize / 2} color={MEMBER_CONFIGS[tile.member].themeColor} />

      {/* Inner background dark */}
      <Circle cx={tileSize / 2} cy={tileSize / 2} r={(innerSize / 2) - 1.5} color="#15122b" />

      {/* Member image */}
      {image && (
        <Image
          image={image}
          x={pad - 2}
          y={pad - 2}
          width={innerSize + 4}
          height={innerSize + 4}
          fit="contain"
        />
      )}

      {/* Special indicator */}
      {tile.special && (
        <SpecialGraphic type={tile.special} tileSize={tileSize} />
      )}

      {/* Obstacle overlay */}
      {tile.obstacle && (
        <RoundedRect
          x={pad}
          y={pad}
          width={innerSize}
          height={innerSize}
          r={8}
          color="rgba(100, 200, 255, 0.4)"
        />
      )}
    </Group>
  );
};

const SpecialGraphic: React.FC<{ type: string; tileSize: number }> = ({ type, tileSize }) => {
  const cx = tileSize / 2;
  const cy = tileSize / 2;
  
  switch (type) {
    case 'mic_horizontal':
      return <RoundedRect x={cx - 15} y={cy - 4} width={30} height={8} r={4} color="#00ffcc" />;
    case 'mic_vertical':
      return <RoundedRect x={cx - 4} y={cy - 15} width={8} height={30} r={4} color="#00ffcc" />;
    case 'album_bomb':
      return <Circle cx={cx} cy={cy} r={12} color="#ff00ff" />;
    case 'stage_bomb':
      return (
        <Group>
          <RoundedRect x={cx - 15} y={cy - 15} width={30} height={30} r={6} color="rgba(255, 255, 0, 0.5)" />
          <Circle cx={cx} cy={cy} r={6} color="#ffff00" />
        </Group>
      );
    case 'fan_heart':
      return <Circle cx={cx} cy={cy} r={10} color="#ff0055" />;
    case 'chibi':
      return <Circle cx={cx} cy={cy} r={14} color="#ffffff" />;
    case 'dynamite':
      return (
        <Group>
          <RoundedRect x={cx - 15} y={cy - 4} width={30} height={8} r={4} color="#ff3300" />
          <RoundedRect x={cx - 4} y={cy - 15} width={8} height={30} r={4} color="#ff3300" />
        </Group>
      );
    case 'butter':
      return <RoundedRect x={cx - 12} y={cy - 10} width={24} height={20} r={4} color="#ffcc00" />;
    case 'lightstick':
      return <Circle cx={cx} cy={cy} r={10} color="#00ffff" />;
    default:
      return <Circle cx={tileSize - 10} cy={tileSize - 10} r={6} color="#FFD700" />;
  }
};
