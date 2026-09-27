"""Gerçek Python 3.12'nin isim listelerini motora yazar:
  - oyun/Assets/Motor/PythonNames.cs (Unity motoru, C#)
  - src/engine/python-names.ts (eski TypeScript motoru, referans)

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

ROOT = Path(__file__).resolve().parent.parent
OUT = ROOT / "src" / "engine" / "python-names.ts"
OUT_CS = ROOT / "oyun" / "Assets" / "Motor" / "PythonNames.cs"


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
    write_cs(data)


def cs_array(items: list[str]) -> str:
    # JSON metin yazımı C#'ta da geçerlidir
    return "{ " + ", ".join(json.dumps(x, ensure_ascii=False) for x in items) + " }"


def write_cs(data: dict) -> None:
    lines = [
        "// OTOMATİK ÜRETİLDİ: scripts/python_isimler.py — elle değiştirme.",
        f"// Kaynak: Python {data['PYTHON_VERSION']}",
        "",
        "using System.Collections.Generic;",
        "",
        "namespace MarsKod.Motor",
        "{",
        "    public static class PythonNames",
        "    {",
        f"        public const string PythonVersion = {json.dumps(data['PYTHON_VERSION'])};",
        "",
        f"        public static readonly string[] BuiltinNames = {cs_array(data['BUILTIN_NAMES'])};",
        "",
        f"        public static readonly string[] ModuleGlobalNames = {cs_array(data['MODULE_GLOBAL_NAMES'])};",
        "",
        f"        public static readonly string[] StdlibModuleNames = {cs_array(data['STDLIB_MODULE_NAMES'])};",
        "",
        "        public static readonly Dictionary<string, string[]> TypeDir = new Dictionary<string, string[]>",
        "        {",
    ]
    for name, names in data["TYPE_DIR"].items():
        lines.append(f"            [{json.dumps(name)}] = new[] {cs_array(names)},")
    lines += ["        };", "    }", "}", ""]
    OUT_CS.parent.mkdir(parents=True, exist_ok=True)
    OUT_CS.write_text("\n".join(lines), encoding="utf-8")
    print(f"Yazıldı: {OUT_CS}")


if __name__ == "__main__":
    main()
