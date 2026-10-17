import { useSharedValue, withSpring, withTiming, withSequence, Easing } from 'react-native-reanimated';

export function useTileAnimator() {
  const scale = useSharedValue(1);
  const opacity = useSharedValue(1);
  const xOffset = useSharedValue(0);
  const yOffset = useSharedValue(0);

  const startIdle = () => {
    // Subtle breathing
    scale.value = withSequence(
      withTiming(1.05, { duration: 1500, easing: Easing.inOut(Easing.ease) }),
      withTiming(1, { duration: 1500, easing: Easing.inOut(Easing.ease) })
    );
  };

  const startSelected = () => {
    scale.value = withSpring(1.2, { damping: 10, stiffness: 100 });
  };

  const startMatch = () => {
    scale.value = withSequence(
      withSpring(1.3, { damping: 12 }),
      withTiming(0, { duration: 300 })
    );
    opacity.value = withTiming(0, { duration: 300 });
  };

  const translateTo = (dx: number, dy: number) => {
    xOffset.value = withSpring(dx);
    yOffset.value = withSpring(dy);
  };

  return {
    scale,
    opacity,
    xOffset,
    yOffset,
    startIdle,
    startSelected,
    startMatch,
    translateTo,
  };
}
