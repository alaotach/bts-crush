import React, { useEffect, useState } from 'react';
import { Circle, Group, Paint } from '@shopify/react-native-skia';
import { useSharedValue, useDerivedValue, withTiming, Easing, runOnJS } from 'react-native-reanimated';

interface Effect {
  id: string;
  x: number;
  y: number;
  color: string;
  type: 'match' | 'explosion';
}

interface EffectRendererProps {
  effect: Effect;
  tileSize: number;
  onComplete: (id: string) => void;
}

export const EffectRenderer: React.FC<EffectRendererProps> = ({ effect, tileSize, onComplete }) => {
  const progress = useSharedValue(0);
  const opacity = useSharedValue(1);

  const cx = effect.x * tileSize + tileSize / 2;
  const cy = effect.y * tileSize + tileSize / 2;
  
  useEffect(() => {
    progress.value = withTiming(1, { duration: 250, easing: Easing.out(Easing.ease) });
    opacity.value = withTiming(0, { duration: 250, easing: Easing.out(Easing.ease) }, (finished) => {
      'worklet';
      if (finished && onComplete) {
        runOnJS(onComplete)(effect.id);
      }
    });
  }, [onComplete, effect.id]);

  const numParticles = effect.type === 'explosion' ? 12 : 6;
  const distance = effect.type === 'explosion' ? tileSize * 1.5 : tileSize * 0.8;
  const particleSize = effect.type === 'explosion' ? 4 : 2;

  // We can render a set of small circles moving outward
  // We'll use a single Skia Group, but we need to animate the particles.
  // Since we can't easily loop over shared values for each particle without derived values per particle,
  // we can use a custom Skia drawing path or derived array.
  // But actually, just rendering 6 tiny circles with useDerivedValue for their positions is fine.
  
  return (
    <Group opacity={opacity}>
      {Array.from({ length: numParticles }).map((_, i) => {
        const angle = (i * Math.PI * 2) / numParticles;
        return (
          <Particle 
            key={i} 
            cx={cx} cy={cy} 
            angle={angle} 
            distance={distance} 
            progress={progress} 
            color={effect.color} 
            size={particleSize} 
          />
        );
      })}
    </Group>
  );
};

const Particle = ({ cx, cy, angle, distance, progress, color, size }: any) => {
  const transform = useDerivedValue(() => {
    const travel = progress.value * distance;
    return [
      { translateX: cx + Math.cos(angle) * travel - cx },
      { translateY: cy + Math.sin(angle) * travel - cy }
    ];
  });
  
  return (
    <Group transform={transform}>
      <Circle cx={cx} cy={cy} r={size} color={color} />
    </Group>
  );
};
