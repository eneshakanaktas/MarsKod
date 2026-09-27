// Motoru gerçek Python ile karşılaştırır.
// Her tests/python-cases/<grup>/<örnek>.py için yanındaki .json, gerçek Python'un sonucudur
// (npm run python-referans ile üretilir). Motor aynı sonucu birebir vermelidir.
import { readdirSync, readFileSync } from 'node:fs';
import { join } from 'node:path';

import { runPython, type RunResult } from '../src/engine';

// Motor bir grubu desteklemeye başlayınca grubun adı buraya eklenir.
// Listede olmayan gruplar "atlandı" (skipped) görünür; böylece ilerleme sayıyla izlenir.
const ENABLED_GROUPS: string[] = [
  '01-temel',
  '02-kosul',
  '03-dongu',
  '04-fonksiyon',
  '05-liste',
  '06-yazim-hatasi',
  '07-calisma-hatasi',
  '08-cumle-hatasi',
  '09-calisma-ayrinti',
];

const CASES_DIR = join(__dirname, '..', '..', '..', 'tests', 'python-cases');

interface Reference extends RunResult {
  python: string;
}

const groups = readdirSync(CASES_DIR, { withFileTypes: true })
  .filter((d) => d.isDirectory())
  .map((d) => d.name)
  .sort();

describe('referans dosyaları', () => {
  for (const group of groups) {
    for (const file of readdirSync(join(CASES_DIR, group)).filter((f) => f.endsWith('.py'))) {
      it(`${group}/${file} için Python 3.12 referansı var`, () => {
        const jsonPath = join(CASES_DIR, group, file.replace(/\.py$/, '.json'));
        let ref: Reference;
        try {
          ref = JSON.parse(readFileSync(jsonPath, 'utf8'));
        } catch {
          throw new Error(`${group}/${file} için referans yok. Çalıştır: npm run python-referans`);
        }
        expect(ref.python).toMatch(/^3\.12\./);
      });
    }
  }

  it('açık grupların hepsi gerçekten var', () => {
    for (const group of ENABLED_GROUPS) expect(groups).toContain(group);
  });
});

for (const group of groups) {
  const enabled = ENABLED_GROUPS.includes(group);
  describe(`motor = gerçek Python: ${group}`, () => {
    const files = readdirSync(join(CASES_DIR, group)).filter((f) => f.endsWith('.py'));
    for (const file of files) {
      const test = enabled ? it : it.skip;
      test(file, () => {
        const source = readFileSync(join(CASES_DIR, group, file), 'utf8').replace(/\r\n/g, '\n');
        const ref: Reference = JSON.parse(
          readFileSync(join(CASES_DIR, group, file.replace(/\.py$/, '.json')), 'utf8'),
        );
        const result = runPython(source);
        expect(result).toEqual({ output: ref.output, error: ref.error, halt: null });
      });
    }
  });
}
