"""Gerçek Python ile referans sonuç üretir.

tests/python-cases/ altındaki her .py dosyasını ayrı bir Python işlemi olarak çalıştırır
(oyuncunun bilgisayarında göreceği şekliyle) ve sonucu yanına .json olarak yazar:
  {"python": "3.12.x", "output": "...", "error": null | {"type", "message", "line"}}

Çalıştırma: py -3.12 scripts/python_referans.py
"""

import json
import os
import re
import subprocess
import sys
from pathlib import Path

REQUIRED = (3, 12)
CASES_DIR = Path(__file__).resolve().parent.parent / "tests" / "python-cases"
TIMEOUT_SECONDS = 5

# Traceback'teki 'File "...", line N' satırları
FILE_LINE = re.compile(r'^\s*File "(?P<file>[^"]+)", line (?P<line>\d+)')
# Son satır: "HataTürü: mesaj" (mesajsız hatalarda sadece "HataTürü")
LAST_LINE = re.compile(r"^(?P<type>[A-Za-z_][A-Za-z0-9_]*)(?::\s?(?P<message>.*))?$")


def parse_error(stderr: str, case_path: Path) -> dict:
    lines = [l for l in stderr.replace("\r\n", "\n").split("\n") if l.strip()]
    last = LAST_LINE.match(lines[-1].strip())
    if not last:
        raise ValueError(f"Hata satırı çözülemedi: {lines[-1]!r}")

    # Oyuncunun dosyasına ait en son satır numarası = hatanın olduğu satır
    line = None
    for l in lines:
        m = FILE_LINE.match(l)
        if m and Path(m.group("file")).resolve() == case_path.resolve():
            line = int(m.group("line"))

    return {"type": last.group("type"), "message": last.group("message") or "", "line": line}


def run_case(case_path: Path) -> dict:
    env = {**os.environ, "PYTHONIOENCODING": "utf-8", "PYTHONUTF8": "1"}
    proc = subprocess.run(
        [sys.executable, "-X", "utf8", str(case_path)],
        capture_output=True,
        timeout=TIMEOUT_SECONDS,
        env=env,
    )
    stdout = proc.stdout.decode("utf-8").replace("\r\n", "\n")
    stderr = proc.stderr.decode("utf-8")
    error = parse_error(stderr, case_path) if proc.returncode != 0 else None
    return {
        "python": ".".join(map(str, sys.version_info[:3])),
        "output": stdout,
        "error": error,
    }


def main() -> int:
    sys.stdout.reconfigure(encoding="utf-8")
    if sys.version_info[:2] != REQUIRED:
        print(
            f"Python {REQUIRED[0]}.{REQUIRED[1]} gerekli, bu {sys.version.split()[0]}. "
            "Hata mesajları sürümden sürüme değiştiği için herkes aynı sürümü kullanmalı."
        )
        return 1

    cases = sorted(CASES_DIR.rglob("*.py"))
    for case in cases:
        try:
            result = run_case(case)
        except subprocess.TimeoutExpired:
            print(f"ZAMAN AŞIMI (bitmeyen döngü?): {case.relative_to(CASES_DIR)}")
            return 1
        out = case.with_suffix(".json")
        out.write_text(json.dumps(result, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
        status = f"{result['error']['type']} (satır {result['error']['line']})" if result["error"] else "tamam"
        print(f"{case.relative_to(CASES_DIR).as_posix():45} {status}")

    print(f"\n{len(cases)} örnek için referans üretildi.")
    return 0


if __name__ == "__main__":
    sys.exit(main())
