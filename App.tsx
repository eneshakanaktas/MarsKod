import { Canvas } from '@shopify/react-native-skia';
import { useFonts } from 'expo-font';
import { StatusBar } from 'expo-status-bar';
import { Pressable, StyleSheet, Text, useWindowDimensions, View } from 'react-native';

import { Ice, Rock } from './src/components/mars-art';
import { MarsMap, type MapBuilding } from './src/components/MarsMap';
import { Starfield } from './src/components/Starfield';
import { colors } from './src/theme/colors';
import { fonts, FONT_FILES } from './src/theme/fonts';
import type { Cell } from './src/world/grid';

// GÖRSEL DENEME EKRANI: tarzı görmek için. Gerçek bölüm verisi Aşama 2'de, gerçek kod
// alanı Aşama 3'te gelecek.
const DEMO_CELLS: Cell[][] = [
  ['empty', 'empty', 'ice', 'empty', 'empty', 'rock'],
  ['empty', 'rock', 'empty', 'empty', 'ice', 'empty'],
  ['empty', 'empty', 'empty', 'empty', 'empty', 'empty'],
  ['ice', 'empty', 'empty', 'empty', 'empty', 'empty'],
  ['empty', 'empty', 'rock', 'empty', 'empty', 'ice'],
  ['empty', 'ice', 'empty', 'empty', 'rock', 'empty'],
];
const DEMO_BUILDINGS: MapBuilding[] = [
  { kind: 'depot', x: 3, y: 2 },
  { kind: 'lab', x: 4, y: 3 },
  { kind: 'greenhouse', x: 0, y: 5 },
];

type Token = [text: string, color: string];
const DEMO_CODE: Token[][] = [
  [['for', colors.code.keyword], [' i ', colors.code.plain], ['in', colors.code.keyword], [' ', colors.code.plain], ['range', colors.code.function], ['(', colors.code.plain], ['3', colors.code.number], ['):', colors.code.plain]],
  [['    ', colors.code.plain], ['move', colors.code.function], ['(', colors.code.plain], ['"east"', colors.code.string], [')', colors.code.plain]],
  [['    ', colors.code.plain], ['if', colors.code.keyword], [' ', colors.code.plain], ['ice_here', colors.code.function], ['():', colors.code.plain]],
  [['        ', colors.code.plain], ['collect', colors.code.function], ['()', colors.code.plain]],
];

function ResourceChip({ kind, count, goal }: { kind: 'ice' | 'rock'; count: number; goal?: number }) {
  return (
    <View style={styles.chip}>
      <Canvas style={{ width: 26, height: 26 }}>
        {kind === 'ice' ? <Ice x={-2} y={-2} s={30} /> : <Rock gx={0} gy={0} x={-4} y={-5} s={34} />}
      </Canvas>
      <Text style={styles.chipText}>
        {count}
        {goal !== undefined && <Text style={styles.chipGoal}>/{goal}</Text>}
      </Text>
    </View>
  );
}

export default function App() {
  const { width } = useWindowDimensions();
  const mapWidth = width - 32;
  const [fontsLoaded] = useFonts(FONT_FILES);
  if (!fontsLoaded) return <View style={styles.root} />;

  return (
    <View style={styles.root}>
      <Starfield />
      <View style={styles.content}>
        <View style={styles.header}>
          <View>
            <Text style={styles.world}>DÜNYA 1 · İNİŞ BÖLGESİ</Text>
            <Text style={styles.level}>Bölüm 3: Buz Avı</Text>
          </View>
          <View style={styles.chips}>
            <ResourceChip kind="ice" count={1} goal={3} />
            <ResourceChip kind="rock" count={0} />
          </View>
        </View>

        <View style={styles.mapFrame}>
          <MarsMap cells={DEMO_CELLS} robot={{ x: 1, y: 2 }} buildings={DEMO_BUILDINGS} width={mapWidth} />
        </View>

        <View style={styles.mission}>
          <Text style={styles.missionText}>🎯 Doğudaki buzları topla ve depoya getir.</Text>
        </View>

        <View style={styles.codePanel}>
          {DEMO_CODE.map((line, i) => (
            <View key={i} style={styles.codeLine}>
              <Text style={styles.lineNumber}>{i + 1}</Text>
              <Text style={styles.code}>
                {line.map(([text, color], j) => (
                  <Text key={j} style={{ color }}>
                    {text}
                  </Text>
                ))}
              </Text>
            </View>
          ))}
        </View>

        <View style={styles.actions}>
          <Pressable style={[styles.button, styles.secondaryButton]}>
            <Text style={styles.secondaryText}>⏯ Adım adım</Text>
          </Pressable>
          <Pressable style={[styles.button, styles.primaryButton]}>
            <Text style={styles.primaryText}>▶ Çalıştır</Text>
          </Pressable>
        </View>
      </View>
      <StatusBar style="light" />
    </View>
  );
}

const styles = StyleSheet.create({
  root: { flex: 1, backgroundColor: colors.space },
  content: { flex: 1, paddingHorizontal: 16, paddingTop: 52, gap: 12 },
  header: { flexDirection: 'row', justifyContent: 'space-between', alignItems: 'flex-end' },
  world: { color: colors.secondary, fontSize: 12, fontFamily: fonts.label, letterSpacing: 2 },
  level: { color: colors.text, fontSize: 24, fontFamily: fonts.heading, marginTop: -2 },
  chips: { flexDirection: 'row', gap: 8 },
  chip: {
    flexDirection: 'row',
    alignItems: 'center',
    gap: 2,
    backgroundColor: 'rgba(20, 26, 46, 0.85)',
    borderColor: colors.panelBorder,
    borderWidth: 1,
    borderRadius: 14,
    paddingHorizontal: 8,
    paddingVertical: 2,
  },
  chipText: { color: colors.text, fontSize: 16, fontFamily: fonts.heading },
  chipGoal: { color: colors.textMuted, fontFamily: fonts.body },
  // Harita kendi kenarını ve yamacını çizer (diorama); çerçeve gerekmez.
  mapFrame: { marginBottom: -8 },
  mission: {
    backgroundColor: 'rgba(57, 213, 255, 0.08)',
    borderColor: 'rgba(57, 213, 255, 0.3)',
    borderWidth: 1,
    borderRadius: 12,
    paddingHorizontal: 12,
    paddingVertical: 8,
  },
  missionText: { color: colors.text, fontSize: 15, fontFamily: fonts.body },
  codePanel: {
    flex: 1,
    backgroundColor: colors.panel,
    borderColor: colors.panelBorder,
    borderWidth: 1,
    borderRadius: 14,
    paddingVertical: 10,
    paddingHorizontal: 8,
  },
  codeLine: { flexDirection: 'row', paddingVertical: 2 },
  lineNumber: { width: 26, color: colors.code.lineNumber, fontFamily: fonts.code, fontSize: 15, textAlign: 'right', marginRight: 10 },
  code: { fontFamily: fonts.code, fontSize: 15 },
  actions: { flexDirection: 'row', gap: 10, paddingBottom: 24 },
  button: { flex: 1, borderRadius: 14, paddingVertical: 14, alignItems: 'center' },
  primaryButton: { backgroundColor: colors.primary },
  primaryText: { color: '#1A0E08', fontSize: 18, fontFamily: fonts.heading },
  secondaryButton: { backgroundColor: 'rgba(57, 213, 255, 0.12)', borderWidth: 1, borderColor: 'rgba(57, 213, 255, 0.45)' },
  secondaryText: { color: colors.secondary, fontSize: 18, fontFamily: fonts.heading },
});
