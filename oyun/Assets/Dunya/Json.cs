// Küçük JSON okuyucu (bölüm dosyaları için). Unity'nin JsonUtility'si saf C# tarafında kullanılamaz,
// .NET Standard 2.1'de de hazır bir okuyucu yok. Hata mesajları Türkçe ve satır numaralıdır:
// dosyayı elle düzenleyen (kod bilmeyen) ekip üyesi yanlışı kolayca bulabilsin diye.
//
// Sonuç türleri: nesne -> Dictionary<string, object>, dizi -> List<object>, metin -> string,
// sayı -> double, true/false -> bool, null -> null.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace MarsKod.Dunya
{
    public sealed class JsonError : Exception
    {
        /// <summary>1'den başlar</summary>
        public readonly int Line;

        public JsonError(int line, string message) : base(line + ". satır: " + message)
        {
            Line = line;
        }
    }

    public static class Json
    {
        public static object Parse(string text)
        {
            var r = new Reader(text);
            r.SkipSpace();
            var value = r.Value();
            r.SkipSpace();
            if (!r.End) throw r.Error("Dosya bitmesi gereken yerde devam ediyor. Fazladan bir parantez ya da en dışta ikinci bir { } olabilir.");
            return value;
        }

        sealed class Reader
        {
            readonly string s;
            int i;

            public Reader(string text)
            {
                s = text;
                if (s.Length > 0 && s[0] == '﻿') i = 1; // dosya başındaki görünmez BOM işareti
            }

            public bool End => i >= s.Length;

            int LineAt(int pos)
            {
                int line = 1;
                for (int k = 0; k < pos && k < s.Length; k++)
                    if (s[k] == '\n') line++;
                return line;
            }

            public JsonError Error(string message) => new JsonError(LineAt(i), message);

            public void SkipSpace()
            {
                while (i < s.Length && (s[i] == ' ' || s[i] == '\t' || s[i] == '\n' || s[i] == '\r')) i++;
            }

            public object Value()
            {
                if (End) throw Error("Dosya yarıda bitti: bir değer bekleniyordu. Kapanmamış bir { ya da [ olabilir.");
                char c = s[i];
                if (c == '{') return Obj();
                if (c == '[') return Arr();
                if (c == '"') return Str();
                if (c == '-' || (c >= '0' && c <= '9')) return Num();
                if (Word("true")) return true;
                if (Word("false")) return false;
                if (Word("null")) return null;
                if (c == '\'') throw Error("Metinler çift tırnakla yazılır: 'böyle' değil \"böyle\".");
                throw Error("Burada bir değer bekleniyordu (metin \"...\", sayı, true/false, [ ] ya da { }), ama '" + c + "' bulundu.");
            }

            bool Word(string w)
            {
                if (string.CompareOrdinal(s, i, w, 0, w.Length) != 0) return false;
                i += w.Length;
                return true;
            }

            Dictionary<string, object> Obj()
            {
                var d = new Dictionary<string, object>();
                i++; // {
                SkipSpace();
                if (!End && s[i] == '}') { i++; return d; }
                while (true)
                {
                    SkipSpace();
                    if (End || s[i] != '"')
                    {
                        if (!End && s[i] == '}') throw Error("Son öğeden sonra fazladan virgül var; } işaretinden önceki virgülü sil.");
                        throw Error("Burada çift tırnak içinde bir alan adı bekleniyordu, örn. \"baslik\".");
                    }
                    int keyLine = LineAt(i);
                    string key = Str();
                    if (d.ContainsKey(key)) throw new JsonError(keyLine, "\"" + key + "\" alanı iki kez yazılmış.");
                    SkipSpace();
                    if (End || s[i] != ':') throw Error("\"" + key + "\" alan adından sonra iki nokta (:) bekleniyordu.");
                    i++;
                    SkipSpace();
                    d[key] = Value();
                    SkipSpace();
                    if (End) throw Error("Dosya yarıda bitti: } ile kapanmamış bir { var.");
                    if (s[i] == ',') { i++; continue; }
                    if (s[i] == '}') { i++; return d; }
                    throw Error("Burada virgül (,) ya da } bekleniyordu. Önceki satırın sonunda virgül eksik olabilir.");
                }
            }

            List<object> Arr()
            {
                var list = new List<object>();
                i++; // [
                SkipSpace();
                if (!End && s[i] == ']') { i++; return list; }
                while (true)
                {
                    SkipSpace();
                    if (!End && s[i] == ']') throw Error("Son öğeden sonra fazladan virgül var; ] işaretinden önceki virgülü sil.");
                    list.Add(Value());
                    SkipSpace();
                    if (End) throw Error("Dosya yarıda bitti: ] ile kapanmamış bir [ var.");
                    if (s[i] == ',') { i++; continue; }
                    if (s[i] == ']') { i++; return list; }
                    throw Error("Burada virgül (,) ya da ] bekleniyordu. Önceki satırın sonunda virgül eksik olabilir.");
                }
            }

            string Str()
            {
                int start = i;
                i++; // "
                var sb = new StringBuilder();
                while (true)
                {
                    if (End || s[i] == '\n') throw new JsonError(LineAt(start), "Tırnak kapanmamış: metin \" ile başlıyor ama aynı satırda \" ile bitmiyor.");
                    char c = s[i++];
                    if (c == '"') return sb.ToString();
                    if (c != '\\') { sb.Append(c); continue; }
                    if (End) break;
                    char e = s[i++];
                    switch (e)
                    {
                        case '"': sb.Append('"'); break;
                        case '\\': sb.Append('\\'); break;
                        case '/': sb.Append('/'); break;
                        case 'n': sb.Append('\n'); break;
                        case 't': sb.Append('\t'); break;
                        case 'r': sb.Append('\r'); break;
                        case 'b': sb.Append('\b'); break;
                        case 'f': sb.Append('\f'); break;
                        case 'u':
                            if (i + 4 <= s.Length && int.TryParse(s.Substring(i, 4), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out int code))
                            {
                                sb.Append((char)code);
                                i += 4;
                                break;
                            }
                            throw Error("\\u işaretinden sonra 4 haneli bir kod bekleniyordu.");
                        default:
                            throw Error("Metinde geçersiz ters bölü: \\" + e + ". Ters bölünün kendisi için \\\\ yaz.");
                    }
                }
                throw new JsonError(LineAt(start), "Tırnak kapanmamış.");
            }

            double Num()
            {
                int start = i;
                if (s[i] == '-') i++;
                while (i < s.Length && (char.IsDigit(s[i]) || s[i] == '.' || s[i] == 'e' || s[i] == 'E' || s[i] == '+' || s[i] == '-')) i++;
                string t = s.Substring(start, i - start);
                if (double.TryParse(t, NumberStyles.Float, CultureInfo.InvariantCulture, out double v)) return v;
                throw new JsonError(LineAt(start), "Geçersiz sayı: " + t + " (ondalık için nokta kullan, örn. 1.5).");
            }
        }
    }
}
