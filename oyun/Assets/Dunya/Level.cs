// Bölüm dosyası: bir bölümün haritası, görevi, açık komutları, konuları, ipuçları, tipik hataları ve doğru çözümü.
// Dosyalar oyun/Assets/Resources/Bolumler/ içinde JSON'dur; kod bilmeyen ekip üyeleri de düzenleyebilir.
// Biçim ve örnek: docs/tasarim/bolum-dosyasi.md
//
// Harita "resim gibi" yazılır: her satır bir sıra, en üstteki satır kuzey (alanın arkası).
//   R robot   B buz   K kaya   H hedef kare   . boş     (aradaki boşluklar önemsizdir)

using System;
using System.Collections.Generic;
using System.Linq;

namespace MarsKod.Dunya
{
    /// <summary>Bölüm dosyasındaki bir yanlış; mesaj Türkçe, dosyayı düzenleyen kişi için</summary>
    public sealed class LevelFormatError : Exception
    {
        public LevelFormatError(string message) : base(message)
        {
        }
    }

    /// <summary>Oyuncuların sık yaptığı bir hata: kod ve ne olduğunun açıklaması</summary>
    public sealed class TypicalMistake
    {
        public string Code;
        public string Explanation;
    }

    public sealed class Level
    {
        public int Number;
        public string Title;
        /// <summary>Tek cümle görev (ekranın üstünde görünür)</summary>
        public string Goal;
        public int Cols, Rows;
        public Cell Robot;
        public List<Cell> Ices = new List<Cell>();
        public List<Cell> Rocks = new List<Cell>();
        public Cell? Target;
        /// <summary>Bu bölümde açık oyun komutları (move, collect...)</summary>
        public List<string> Commands = new List<string>();
        /// <summary>Öğretilen konular (oyuncu profili bunlara göre tutulacak)</summary>
        public List<string> Topics = new List<string>();
        /// <summary>Kademeli ipuçları: 1. yön gösterir, 2. konuyu hatırlatır, 3. kodun bir kısmını verir</summary>
        public List<string> Hints = new List<string>();
        public List<TypicalMistake> Mistakes = new List<TypicalMistake>();
        /// <summary>Oyuncunun önüne gelen ilk kod (boş olabilir)</summary>
        public string StartCode = "";
        public string Solution;

        public World CreateWorld() => new World(Cols, Rows, Robot, Ices, Rocks, Target, Commands);

        public const char RobotMark = 'R', IceMark = 'B', RockMark = 'K', TargetMark = 'H', EmptyMark = '.';

        // ---- dosyadan okuma ----

        /// <summary>JSON metninden bölümü okur; yanlışta LevelFormatError verir (Türkçe, satır numaralı).</summary>
        public static Level Parse(string json)
        {
            object root;
            try
            {
                root = Json.Parse(json);
            }
            catch (JsonError e)
            {
                throw new LevelFormatError("Dosya JSON olarak okunamadı. " + e.Message);
            }
            if (!(root is Dictionary<string, object> d))
                throw new LevelFormatError("Dosya { ile başlayıp } ile bitmeli.");

            var known = new HashSet<string> { "numara", "baslik", "gorev", "harita", "komutlar", "konular", "ipuclari", "tipik_hatalar", "baslangic_kodu", "cozum" };
            foreach (var key in d.Keys)
                if (!known.Contains(key))
                    throw new LevelFormatError("Bilinmeyen alan: \"" + key + "\". Kullanılabilen alanlar: " + string.Join(", ", known) + ".");

            var level = new Level
            {
                Number = Int(d, "numara"),
                Title = Text(d, "baslik"),
                Goal = Text(d, "gorev"),
                Commands = TextList(d, "komutlar"),
                Topics = TextList(d, "konular"),
                Hints = TextList(d, "ipuclari"),
                Solution = Code(d, "cozum"),
                StartCode = d.ContainsKey("baslangic_kodu") ? Code(d, "baslangic_kodu") : "",
            };
            if (level.Number < 1) throw new LevelFormatError("\"numara\" 1 ya da daha büyük olmalı.");
            if (level.Hints.Count == 0) throw new LevelFormatError("\"ipuclari\" boş olamaz; en az bir ipucu yaz.");
            foreach (var c in level.Commands)
                if (Array.IndexOf(World.AllCommands, c) < 0)
                    throw new LevelFormatError("\"komutlar\" içinde bilinmeyen komut: \"" + c + "\". Oyundaki komutlar: " + string.Join(", ", World.AllCommands) + ".");
            ReadMap(level, TextList(d, "harita"));
            if (d.ContainsKey("tipik_hatalar")) level.Mistakes = ReadMistakes(d["tipik_hatalar"]);
            return level;
        }

        static void ReadMap(Level level, List<string> rows)
        {
            if (rows.Count == 0) throw new LevelFormatError("\"harita\" boş olamaz.");
            var lines = rows.Select(r => r.Replace(" ", "")).ToList();
            level.Rows = lines.Count;
            level.Cols = lines[0].Length;
            if (level.Cols == 0) throw new LevelFormatError("Haritanın ilk satırı boş.");
            bool robot = false;
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Length != level.Cols)
                    throw new LevelFormatError("Haritanın " + (i + 1) + ". satırında " + lines[i].Length + " kare var, ilk satırda " + level.Cols + ". Tüm satırlar aynı uzunlukta olmalı.");
                int row = level.Rows - 1 - i; // en üstteki satır kuzey
                for (int col = 0; col < level.Cols; col++)
                {
                    var cell = new Cell(col, row);
                    switch (lines[i][col])
                    {
                        case RobotMark:
                            if (robot) throw new LevelFormatError("Haritada birden fazla robot (" + RobotMark + ") var; tek robot olmalı.");
                            robot = true;
                            level.Robot = cell;
                            break;
                        case IceMark: level.Ices.Add(cell); break;
                        case RockMark: level.Rocks.Add(cell); break;
                        case TargetMark:
                            if (level.Target != null) throw new LevelFormatError("Haritada birden fazla hedef (" + TargetMark + ") var; en fazla bir hedef olabilir.");
                            level.Target = cell;
                            break;
                        case EmptyMark: break;
                        default:
                            throw new LevelFormatError("Haritanın " + (i + 1) + ". satırında bilinmeyen işaret: '" + lines[i][col] + "'. Kullanılabilenler: "
                                + RobotMark + " robot, " + IceMark + " buz, " + RockMark + " kaya, " + TargetMark + " hedef, " + EmptyMark + " boş.");
                    }
                }
            }
            if (!robot) throw new LevelFormatError("Haritada robot (" + RobotMark + ") yok.");
            if (level.Ices.Count == 0 && level.Target == null)
                throw new LevelFormatError("Haritada görev yok: en az bir buz (" + IceMark + ") ya da bir hedef (" + TargetMark + ") olmalı.");
            if (level.Ices.Count > 0 && !level.Commands.Contains("collect"))
                throw new LevelFormatError("Haritada buz var ama \"komutlar\" içinde \"collect\" yok; buzlar toplanamaz.");
        }

        static object Field(Dictionary<string, object> d, string key)
        {
            if (!d.TryGetValue(key, out var v)) throw new LevelFormatError("\"" + key + "\" alanı eksik.");
            return v;
        }

        static int Int(Dictionary<string, object> d, string key)
        {
            if (Field(d, key) is double n && n == Math.Floor(n) && Math.Abs(n) < 1e6) return (int)n;
            throw new LevelFormatError("\"" + key + "\" bir tam sayı olmalı (tırnaksız), örn. 3.");
        }

        static string Text(Dictionary<string, object> d, string key)
        {
            if (Field(d, key) is string s && s.Trim().Length > 0) return s;
            throw new LevelFormatError("\"" + key + "\" boş olmayan bir metin olmalı (çift tırnak içinde).");
        }

        static List<string> TextList(Dictionary<string, object> d, string key)
        {
            if (Field(d, key) is List<object> list && list.All(x => x is string))
                return list.Cast<string>().ToList();
            throw new LevelFormatError("\"" + key + "\" bir metin listesi olmalı, örn. [\"birinci\", \"ikinci\"].");
        }

        // Kod, her satırı ayrı bir metin olan liste olarak yazılır (okunması kolay); tek metin de kabul edilir.
        static string Code(Dictionary<string, object> d, string key) => CodeOf(Field(d, key), "\"" + key + "\"");

        static string CodeOf(object v, string where)
        {
            if (v is string s) return Normalize(s);
            if (v is List<object> list && list.All(x => x is string))
                return list.Count == 0 ? "" : Normalize(string.Join("\n", list.Cast<string>()));
            throw new LevelFormatError(where + " kod satırlarından oluşan bir liste olmalı, örn. [\"move(East)\", \"collect()\"].");
        }

        static string Normalize(string code)
        {
            code = code.Replace("\r\n", "\n");
            return code.Length == 0 || code.EndsWith("\n") ? code : code + "\n";
        }

        static List<TypicalMistake> ReadMistakes(object v)
        {
            const string shape = "\"tipik_hatalar\" bir liste olmalı; her öğe { \"kod\": [...], \"aciklama\": \"...\" } biçiminde.";
            if (!(v is List<object> list)) throw new LevelFormatError(shape);
            var result = new List<TypicalMistake>();
            for (int i = 0; i < list.Count; i++)
            {
                if (!(list[i] is Dictionary<string, object> m) || !m.ContainsKey("kod") || !(m.TryGetValue("aciklama", out var a) && a is string text) || m.Count != 2)
                    throw new LevelFormatError(shape + " (" + (i + 1) + ". öğe uymuyor)");
                result.Add(new TypicalMistake { Code = CodeOf(m["kod"], "\"tipik_hatalar\" " + (i + 1) + ". öğenin \"kod\" alanı"), Explanation = text });
            }
            return result;
        }
    }

    /// <summary>
    /// Bölüm denetleyici: bölümün gerçekten oynanabilir olduğunu sınar. Doğru çözüm görevi bitirmeli;
    /// tipik hatalar ve başlangıç kodu bitirmemeli (bitiriyorsa bölüm ya da açıklama yanlıştır).
    /// </summary>
    public static class LevelCheck
    {
        /// <summary>Bulunan sorunlar (Türkçe); boşsa bölüm sağlam.</summary>
        public static List<string> Problems(Level level)
        {
            var problems = new List<string>();
            var report = ProgramRun.Execute(level.Solution, level.CreateWorld());
            if (report.Stopped)
                problems.Add("Doğru çözüm " + (report.StopLine?.ToString() ?? "?") + ". satırda durdu: " + Reason(report));
            else if (!report.Complete)
                problems.Add("Doğru çözüm sonuna kadar çalıştı ama görevi bitirmedi.");

            if (level.StartCode.Trim().Length > 0 && ProgramRun.Execute(level.StartCode, level.CreateWorld()).Complete)
                problems.Add("Başlangıç kodu görevi zaten bitiriyor; oyuncuya iş kalmıyor.");

            for (int i = 0; i < level.Mistakes.Count; i++)
                if (ProgramRun.Execute(level.Mistakes[i].Code, level.CreateWorld()).Complete)
                    problems.Add((i + 1) + ". tipik hata görevi bitiriyor; hata örneği gerçekten hatalı olmalı.");
            return problems;
        }

        static string Reason(RunReport r)
        {
            if (r.Rule != null) return r.Rule.Title;
            if (r.Error != null) return r.Error.Type + ": " + r.Error.Message;
            return r.Halt.Kind + " (" + r.Halt.Reason + ")";
        }
    }
}
