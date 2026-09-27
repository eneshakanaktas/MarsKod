// Mars ızgarasının saf mantığı: telefon koduna bağlı değil, bilgisayarda test edilir.

export type Cell = 'empty' | 'ice' | 'rock';

export interface Position {
  x: number;
  y: number;
}

export function isInside(pos: Position, width: number, height: number): boolean {
  return pos.x >= 0 && pos.y >= 0 && pos.x < width && pos.y < height;
}
