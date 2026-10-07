// Veri dosyalarını (bölümler, kod sözlüğü) okumanın ortak parçaları. Dosyalar JSON'dur; kod bilmeyen ekip üyeleri de
// düzenleyebilsin diye yanlışlar Türkçe ve düzeltme örneğiyle anlatılır.

using System;
using System.Collections.Generic;
using System.Linq;

namespace MarsKod.Dunya
{
    /// <summary>Veri dosyasındaki bir yanlış; mesaj Türkçe, dosyayı düzenleyen kişi için</summary>
    public sealed class DataFormatError : Exception
    {
        public DataFormatError(string message) : base(message)
        {
        }
    }

    public static class DataFields
    {
        /// <summary>JSON metnini { ... } nesnesi olarak okur; yalnızca known içindeki alanlara izin verir.</summary>
        public static Dictionary<string, object> ParseObject(string json, ICollection<string> known)
        {
            object root;
            try
            {
                root = Json.Parse(json);
            }
            catch (JsonError e)
            {
                throw new DataFormatError("Dosya JSON olarak okunamadı. " + e.Message);
            }
            if (!(root is Dictionary<string, object> d))
                throw new DataFormatError("Dosya { ile başlayıp } ile bitmeli.");
            CheckKeys(d, known, null);
            return d;
        }

        /// <summary>Bilinmeyen alan varsa yanlış verir; where: iç içe öğe için "3. sayfa" gibi yer bilgisi (yoksa null).</summary>
        public static void CheckKeys(Dictionary<string, object> d, ICollection<string> known, string where)
        {
            foreach (var key in d.Keys)
                if (!known.Contains(key))
                    throw new DataFormatError((where != null ? where + ": " : "") + "Bilinmeyen alan: \"" + key + "\". Kullanılabilen alanlar: " + string.Join(", ", known) + ".");
        }

        public static object Field(Dictionary<string, object> d, string key)
        {
            if (!d.TryGetValue(key, out var v)) throw new DataFormatError("\"" + key + "\" alanı eksik.");
            return v;
        }

        public static int Int(Dictionary<string, object> d, string key)
        {
            if (Field(d, key) is double n && n == Math.Floor(n) && Math.Abs(n) < 1e6) return (int)n;
            throw new DataFormatError("\"" + key + "\" bir tam sayı olmalı (tırnaksız), örn. 3.");
        }

        public static string Text(Dictionary<string, object> d, string key)
        {
            if (Field(d, key) is string s && s.Trim().Length > 0) return s;
            throw new DataFormatError("\"" + key + "\" boş olmayan bir metin olmalı (çift tırnak içinde).");
        }

        public static List<string> TextList(Dictionary<string, object> d, string key)
        {
            if (Field(d, key) is List<object> list && list.All(x => x is string))
                return list.Cast<string>().ToList();
            throw new DataFormatError("\"" + key + "\" bir metin listesi olmalı, örn. [\"birinci\", \"ikinci\"].");
        }

        public static List<int> IntList(Dictionary<string, object> d, string key)
        {
            if (Field(d, key) is List<object> list && list.All(x => x is double n && n == Math.Floor(n) && Math.Abs(n) < 1e6))
                return list.Select(x => (int)(double)x).ToList();
            throw new DataFormatError("\"" + key + "\" tam sayılardan oluşan bir liste olmalı (tırnaksız), örn. [3, 5].");
        }

        /// <summary>Kod, her satırı ayrı bir metin olan liste olarak yazılır (okunması kolay); tek metin de kabul edilir.</summary>
        public static string Code(Dictionary<string, object> d, string key) => CodeOf(Field(d, key), "\"" + key + "\"");

        public static string CodeOf(object v, string where)
        {
            if (v is string s) return Normalize(s);
            if (v is List<object> list && list.All(x => x is string))
                return list.Count == 0 ? "" : Normalize(string.Join("\n", list.Cast<string>()));
            throw new DataFormatError(where + " kod satırlarından oluşan bir liste olmalı, örn. [\"move(East)\", \"collect()\"].");
        }

        static string Normalize(string code)
        {
            code = code.Replace("\r\n", "\n");
            return code.Length == 0 || code.EndsWith("\n") ? code : code + "\n";
        }
    }
}
