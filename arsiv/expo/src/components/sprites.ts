// Hazır çizimler (sprite). Kaynak: Kenney "Sci-Fi RTS" paketi, lisans CC0 (her amaçla serbest).
// Ayrıntı: assets/sprites/kenney-scifi-rts/License.txt ve docs/tasarim/gorsel-yon.md

import { useImage, type SkImage } from '@shopify/react-native-skia';

const SOURCES = {
  rover: require('../../assets/sprites/kenney-scifi-rts/rover.png'),
  'ice-1': require('../../assets/sprites/kenney-scifi-rts/ice-1.png'),
  'ice-2': require('../../assets/sprites/kenney-scifi-rts/ice-2.png'),
  'ice-3': require('../../assets/sprites/kenney-scifi-rts/ice-3.png'),
  'ice-4': require('../../assets/sprites/kenney-scifi-rts/ice-4.png'),
  'ice-pebble': require('../../assets/sprites/kenney-scifi-rts/ice-pebble.png'),
  'rock-1': require('../../assets/sprites/kenney-scifi-rts/rock-1.png'),
  'rock-2': require('../../assets/sprites/kenney-scifi-rts/rock-2.png'),
  'rock-3': require('../../assets/sprites/kenney-scifi-rts/rock-3.png'),
  'ore-1': require('../../assets/sprites/kenney-scifi-rts/ore-1.png'),
  'ore-2': require('../../assets/sprites/kenney-scifi-rts/ore-2.png'),
  pebble: require('../../assets/sprites/kenney-scifi-rts/pebble.png'),
  depot: require('../../assets/sprites/kenney-scifi-rts/depot.png'),
  lab: require('../../assets/sprites/kenney-scifi-rts/lab.png'),
  greenhouse: require('../../assets/sprites/kenney-scifi-rts/greenhouse.png'),
  'water-tanks': require('../../assets/sprites/kenney-scifi-rts/water-tanks.png'),
  antenna: require('../../assets/sprites/kenney-scifi-rts/antenna.png'),
  'ice-sheet': require('../../assets/sprites/kenney-scifi-rts/ice-sheet.png'),
} as const;

export type SpriteName = keyof typeof SOURCES;
export type Sprites = Record<SpriteName, SkImage>;

const NAMES = Object.keys(SOURCES) as SpriteName[];

/** Bütün çizimleri yükler; hepsi hazır olana kadar null döner. */
export function useSprites(): Sprites | null {
  // İsim listesi sabit olduğu için hook'lar her seferinde aynı sırada çağrılır.
  const images = NAMES.map((name) => useImage(SOURCES[name]));
  if (images.some((img) => !img)) return null;
  return Object.fromEntries(NAMES.map((name, i) => [name, images[i]!])) as Sprites;
}
