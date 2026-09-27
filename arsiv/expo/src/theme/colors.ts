// MarsKod renk paleti. Görsel yön: sıcak Mars toprağı + soğuk teknoloji ışıkları.
// Ayrıntı: docs/tasarim/gorsel-yon.md

export const colors = {
  // Uzay arka planı
  space: '#0B0C1D',
  spaceDeep: '#07071A',
  spacePurple: '#1C1236',
  star: '#FFFFFF',

  // Mars toprağı
  ground: ['#C2562F', '#BA502C', '#C75C33', '#BE5530'],
  groundSeam: '#7E2F16',
  groundDark: '#9A4022',
  groundLight: '#D9774A',
  crater: '#A84826',

  // Kaynaklar
  iceLight: '#EFFCFF',
  ice: '#BDEFFC',
  iceDark: '#6FC6E0',
  iceGlow: 'rgba(120, 225, 255, 0.45)',
  rock: '#4B2F2B',
  rockLight: '#6E4842',
  rockSpeck: '#F0A04B',

  // Robot ve yapılar
  hull: '#F4EEE6',
  hullShade: '#D6CCBF',
  tread: '#2E2F3A',
  accent: '#FF7A3D',
  visor: '#39D5FF',
  window: '#26304A',
  plant: '#5BC96B',
  plantDark: '#3A9A55',
  shadow: 'rgba(40, 10, 0, 0.35)',

  // Arayüz
  panel: '#141A2E',
  panelBorder: '#26304A',
  text: '#EDE7F6',
  textMuted: '#8A90A8',
  primary: '#FF7A3D',
  secondary: '#39D5FF',

  // Kod renkleri
  code: {
    keyword: '#FF9E64',
    function: '#7DCFFF',
    string: '#9ECE6A',
    number: '#E0AF68',
    plain: '#E6E9F5',
    lineNumber: '#4B5270',
  },
} as const;
