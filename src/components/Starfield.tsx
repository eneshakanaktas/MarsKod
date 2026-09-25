import { BlurMask, Canvas, Circle, Rect, RadialGradient, vec } from '@shopify/react-native-skia';
import { StyleSheet, useWindowDimensions } from 'react-native';

import { colors } from '../theme/colors';

import { rand } from './mars-art';

/** Arka plandaki uzay: koyu gradyan, yıldızlar ve uzakta hafif bir nebula. */
export function Starfield() {
  const { width, height } = useWindowDimensions();
  const stars = Array.from({ length: 90 }, (_, i) => ({
    x: rand(i, 0, 3) * width,
    y: rand(i, 1, 3) * height,
    r: 0.4 + rand(i, 2, 3) * 1.1,
    o: 0.25 + rand(i, 3, 3) * 0.6,
  }));
  return (
    <Canvas style={StyleSheet.absoluteFill}>
      <Rect x={0} y={0} width={width} height={height}>
        <RadialGradient c={vec(width * 0.8, height * 0.15)} r={height * 0.9} colors={[colors.spacePurple, colors.space, colors.spaceDeep]} />
      </Rect>
      <Circle cx={width * 0.15} cy={height * 0.72} r={width * 0.45} color="rgba(255, 122, 61, 0.07)">
        <BlurMask blur={60} style="normal" />
      </Circle>
      {stars.map((star, i) => (
        <Circle key={i} cx={star.x} cy={star.y} r={star.r} color={colors.star} opacity={star.o} />
      ))}
    </Canvas>
  );
}
