// Python 3.12'yi bulup scripts/python_referans.py'yi çalıştırır.
// Windows'ta "python" komutu bazen Microsoft Store'a yönlendirdiği için birkaç yol denenir.
import { spawnSync } from 'node:child_process';
import { existsSync } from 'node:fs';
import { join } from 'node:path';

const script = join(import.meta.dirname, '..', '..', '..', 'scripts', 'python_referans.py');

const candidates = [
  process.env.PYTHON && [process.env.PYTHON],
  process.env.LOCALAPPDATA && [join(process.env.LOCALAPPDATA, 'Programs', 'Python', 'Python312', 'python.exe')],
  ['py', '-3.12'],
  ['python3.12'],
  ['python3'],
  ['python'],
].filter(Boolean);

for (const [cmd, ...args] of candidates) {
  if (cmd.includes('\\') && !existsSync(cmd)) continue;
  const check = spawnSync(cmd, [...args, '-c', 'import sys; print(sys.version_info[:2] == (3, 12))'], {
    encoding: 'utf8',
  });
  if (check.stdout?.trim() !== 'True') continue;
  const run = spawnSync(cmd, [...args, script], { stdio: 'inherit' });
  process.exit(run.status ?? 1);
}

console.error('Python 3.12 bulunamadı. Kurmak için: winget install -e --id Python.Python.3.12');
process.exit(1);
