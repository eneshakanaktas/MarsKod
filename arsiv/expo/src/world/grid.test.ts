import { isInside } from './grid';

describe('isInside', () => {
  it('ızgara içindeki kareyi kabul eder', () => {
    expect(isInside({ x: 0, y: 0 }, 6, 6)).toBe(true);
    expect(isInside({ x: 5, y: 5 }, 6, 6)).toBe(true);
  });

  it('harita dışını reddeder', () => {
    expect(isInside({ x: -1, y: 0 }, 6, 6)).toBe(false);
    expect(isInside({ x: 6, y: 2 }, 6, 6)).toBe(false);
  });
});
