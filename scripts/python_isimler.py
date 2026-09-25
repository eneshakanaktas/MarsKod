"""Gerçek Python 3.12'nin isim listelerini src/engine/python-names.ts dosyasına yazar.

Motor, "Did you mean: ...?" önerilerini CPython ile birebir aynı üretmek için bu listeleri
(ve sıralarını) kullanır. Python sürümü değişirse yeniden çalıştırılır:
  python scripts/python_isimler.py
"""

import builtins
import json
import subprocess
import sys
import tempfile
from pathlib import Path

OUT = Path(__file__).resolve().parent.parent / "src" / "engine" / "python-names.ts"


def module_globals() -> list[str]:
    # "python dosya.py" ile çalışan bir programın başlangıçtaki global isimleri (sırasıyla)
    with tempfile.NamedTemporaryFile("w", suffix=".py", delete=False) as f:
        # import satırı globals'e isim eklemesin diye __import__ kullanılıyor
        f.write("print(__import__('json').dumps(list(globals())))\n")
    out = subprocess.run([sys.executable, f.name], capture_output=True, text=True, check=True)
    Path(f.name).unlink()
    return json.loads(out.stdout)


def main() -> None:
    sys.stdout.reconfigure(encoding="utf-8")
    if sys.version_info[:2] != (3, 12):
        sys.exit("Python 3.12 gerekli")

    types = {
        "int": int, "float": float, "str": str, "bool": bool, "list": list, "tuple": tuple,
        "NoneType": type(None), "range": range, "dict": dict, "set": set,
        "function": type(lambda: 0), "builtin_function_or_method": type(len), "type": type,
    }
    data = {
        "PYTHON_VERSION": ".".join(map(str, sys.version_info[:3])),
        "BUILTIN_NAMES": list(builtins.__dict__),
        "MODULE_GLOBAL_NAMES": module_globals(),
        "STDLIB_MODULE_NAMES": sorted(sys.stdlib_module_names),
        "TYPE_DIR": {name: dir(t) for name, t in types.items()},
    }

    lines = [
        "// OTOMATİK ÜRETİLDİ: scripts/python_isimler.py — elle değiştirme.",
        f"// Kaynak: Python {data['PYTHON_VERSION']}",
        "",
    ]
    for key, value in data.items():
        lines.append(f"export const {key} = {json.dumps(value, ensure_ascii=False)} as const;")
        lines.append("")
    OUT.write_text("\n".join(lines), encoding="utf-8")
    print(f"Yazıldı: {OUT}")


if __name__ == "__main__":
    main()
