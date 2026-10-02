// Bölüm dosyası: bir bölümün haritası, görevi, açık komutları, konuları, ipuçları, tipik hataları ve doğru çözümü.
// Dosyalar oyun/Assets/Resources/Bolumler/ içinde JSON'dur; kod bilmeyen ekip üyeleri de düzenleyebilir.
// Biçim ve örnek: docs/tasarim/bolum-dosyasi.md
//
// Harita "resim gibi" yazılır: her satır bir sıra, en üstteki satır kuzey (alanın arkası).
//   R robot   B buz   E enerji hücresi   K kaya   T tehlikeli kristal   H hedef kare   . boş     (aradaki boşluklar önemsizdir)
//   Toplanacak işaretleri (B, E...) Collectible.cs'te; bir bölümde tek tür toplanır.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using MarsKod.Motor;
using static MarsKod.Dunya.DataFields;

namespace MarsKod.Dunya
{
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
        /// <summary>Üst başlıktaki önek ("BÖLÜM" varsayılan; hikâye gerektirirse "ÖDEV" gibi)</summary>
        public string Label = "BÖLÜM";
        /// <summary>Tek cümle görev (ekranın üstünde görünür)</summary>
        public string Goal;
        /// <summary>Bölüm başlarken başlığın altında kısa süre görünen program metni (boş olabilir)</summary>
        public string Intro = "";
        /// <summary>Bölüm sonu ekranında görünen program metni (boş olabilir)</summary>
        public string Outro = "";
        public int Cols, Rows;
        public Cell Robot;
        /// <summary>Toplanacak nesnelerin kareleri (türü Item; tarihsel adı Ices)</summary>
        public List<Cell> Ices = new List<Cell>();
        /// <summary>Bu bölümde toplanan şey (buz, enerji hücresi...)</summary>
        public Collectible Item = Collectible.Ice;
        public List<Cell> Rocks = new List<Cell>();
        /// <summary>Tehlikeli kırmızı kristaller (üstünden geçilir, toplanmaz)</summary>
        public List<Cell> Crystals = new List<Cell>();
        public Cell? Target;
        /// <summary>Bu bölümde açık oyun komutları (move, collect...)</summary>
        public List<string> Commands = new List<string>();
        /// <summary>Öğretilen konular (oyuncu profili bunlara göre tutulacak)</summary>
        public List<string> Topics = new List<string>();
        /// <summary>Acemi paletindeki düğmeler (her biri tek satır kod). Dosyada yoksa komutlardan türetilir.</summary>
        public List<string> Pieces = new List<string>();
        /// <summary>Bu bölümde açılan Python kelimeleri (öneri satırı için; for, in, range...)</summary>
        public List<string> PythonWords = new List<string>();
        /// <summary>Kademeli ipuçları: 1. yön gösterir, 2. konuyu hatırlatır, 3. kodun bir kısmını verir</summary>
        public List<string> Hints = new List<string>();
        public List<TypicalMistake> Mistakes = new List<TypicalMistake>();
        /// <summary>Oyuncunun önüne gelen ilk kod (boş olabilir)</summary>
        public string StartCode = "";
        public string Solution;

        public World CreateWorld() => new World(Cols, Rows, Robot, Ices, Rocks, Target, Commands, Crystals);

        public const char RobotMark = 'R', RockMark = 'K', CrystalMark = 'T', TargetMark = 'H', EmptyMark = '.';

        // ---- dosyadan okuma ----

        /// <summary>JSON metninden bölümü okur; yanlışta DataFormatError verir (Türkçe, satır numaralı).</summary>
        public static Level Parse(string json)
        {
            var known = new HashSet<string> { "numara", "baslik", "etiket", "gorev", "giris", "bitis", "harita", "komutlar", "konular", "parcalar", "python_kelimeleri", "ipuclari", "tipik_hatalar", "baslangic_kodu", "cozum" };
            var d = DataFields.ParseObject(json, known);

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
            if (d.ContainsKey("etiket")) level.Label = Text(d, "etiket");
            if (d.ContainsKey("giris")) level.Intro = Text(d, "giris");
            if (d.ContainsKey("bitis")) level.Outro = Text(d, "bitis");
            if (level.Number < 1) throw new DataFormatError("\"numara\" 1 ya da daha büyük olmalı.");
            if (level.Hints.Count == 0) throw new DataFormatError("\"ipuclari\" boş olamaz; en az bir ipucu yaz.");
            foreach (var c in level.Commands)
                if (Array.IndexOf(World.AllCommands, c) < 0)
                    throw new DataFormatError("\"komutlar\" içinde bilinmeyen komut: \"" + c + "\". Oyundaki komutlar: " + string.Join(", ", World.AllCommands) + ".");
            ReadMap(level, TextList(d, "harita"));
            level.Pieces = d.ContainsKey("parcalar") ? NonEmptyList(d, "parcalar") : DerivePieces(level.Commands);
            if (d.ContainsKey("python_kelimeleri")) level.PythonWords = TextList(d, "python_kelimeleri");
            if (d.ContainsKey("tipik_hatalar")) level.Mistakes = ReadMistakes(d["tipik_hatalar"]);
            return level;
        }

        /// <summary>"parcalar" yazılmamışsa açık komutlardan düğme satırları: move → dört yön, collect → collect().</summary>
        public static List<string> DerivePieces(IEnumerable<string> commands)
        {
            var pieces = new List<string>();
            foreach (var c in commands)
            {
                if (c == "move") pieces.AddRange(Enum.GetNames(typeof(Direction)).Select(dir => "move(" + dir + ")"));
                else if (c == "rock_ahead") pieces.AddRange(Enum.GetNames(typeof(Direction)).Select(dir => "rock_ahead(" + dir + ")"));
                else pieces.Add(c + "()");
            }
            return pieces;
        }

        static List<string> NonEmptyList(Dictionary<string, object> d, string key)
        {
            var list = TextList(d, key);
            if (list.Count == 0) throw new DataFormatError("\"" + key + "\" boş olamaz; en az bir satır yaz (ya da alanı hiç yazma).");
            return list;
        }

        static void ReadMap(Level level, List<string> rows)
        {
            if (rows.Count == 0) throw new DataFormatError("\"harita\" boş olamaz.");
            var lines = rows.Select(r => r.Replace(" ", "")).ToList();
            level.Rows = lines.Count;
            level.Cols = lines[0].Length;
            if (level.Cols == 0) throw new DataFormatError("Haritanın ilk satırı boş.");
            bool robot = false;
            var kinds = new HashSet<Collectible>();
            for (int i = 0; i < lines.Count; i++)
            {
                if (lines[i].Length != level.Cols)
                    throw new DataFormatError("Haritanın " + (i + 1) + ". satırında " + lines[i].Length + " kare var, ilk satırda " + level.Cols + ". Tüm satırlar aynı uzunlukta olmalı.");
                int row = level.Rows - 1 - i; // en üstteki satır kuzey
                for (int col = 0; col < level.Cols; col++)
                {
                    var cell = new Cell(col, row);
                    switch (lines[i][col])
                    {
                        case RobotMark:
                            if (robot) throw new DataFormatError("Haritada birden fazla robot (" + RobotMark + ") var; tek robot olmalı.");
                            robot = true;
                            level.Robot = cell;
                            break;
                        case RockMark: level.Rocks.Add(cell); break;
                        case CrystalMark: level.Crystals.Add(cell); break;
                        case TargetMark:
                            if (level.Target != null) throw new DataFormatError("Haritada birden fazla hedef (" + TargetMark + ") var; en fazla bir hedef olabilir.");
                            level.Target = cell;
                            break;
                        case EmptyMark: break;
                        default:
                            if (Collectibles.Marks.TryGetValue(lines[i][col], out var kind))
                            {
                                kinds.Add(kind);
                                level.Ices.Add(cell);
                                break;
                            }
                            throw new DataFormatError("Haritanın " + (i + 1) + ". satırında bilinmeyen işaret: '" + lines[i][col] + "'. Kullanılabilenler: "
                                + RobotMark + " robot, " + MarkList() + ", " + RockMark + " kaya, " + CrystalMark + " kristal, " + TargetMark + " hedef, " + EmptyMark + " boş.");
                    }
                }
            }
            if (!robot) throw new DataFormatError("Haritada robot (" + RobotMark + ") yok.");
            if (kinds.Count > 1)
                throw new DataFormatError("Haritada iki tür toplanacak var (" + string.Join(", ", kinds.Select(Collectibles.Name)) + "); bir bölümde tek tür olmalı.");
            if (kinds.Count == 1) level.Item = kinds.First();
            if (level.Ices.Count == 0 && level.Target == null)
                throw new DataFormatError("Haritada görev yok: toplanacak bir şey (" + MarkList() + ") ya da bir hedef (" + TargetMark + ") olmalı.");
            if (level.Ices.Count > 0 && !level.Commands.Contains("collect"))
                throw new DataFormatError("Haritada toplanacak " + Collectibles.Name(level.Item) + " var ama \"komutlar\" içinde \"collect\" yok; toplanamaz.");
            if (level.Commands.Contains("ice_here") && level.Item != Collectible.Ice)
                throw new DataFormatError("\"ice_here\" yalnızca buz (B) olan bölümlerde açılabilir; bu bölümde " + Collectibles.Name(level.Item) + " toplanıyor.");
        }

        /// <summary>"B buz, E enerji hücresi" (hata mesajları için)</summary>
        static string MarkList() => string.Join(", ", Collectibles.Marks.Select(m => m.Key + " " + Collectibles.Name(m.Value)));

        static List<TypicalMistake> ReadMistakes(object v)
        {
            const string shape = "\"tipik_hatalar\" bir liste olmalı; her öğe { \"kod\": [...], \"aciklama\": \"...\" } biçiminde.";
            if (!(v is List<object> list)) throw new DataFormatError(shape);
            var result = new List<TypicalMistake>();
            for (int i = 0; i < list.Count; i++)
            {
                if (!(list[i] is Dictionary<string, object> m) || !m.ContainsKey("kod") || !(m.TryGetValue("aciklama", out var a) && a is string text) || m.Count != 2)
                    throw new DataFormatError(shape + " (" + (i + 1) + ". öğe uymuyor)");
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

            problems.AddRange(PieceProblems(level));

            for (int i = 0; i < level.Mistakes.Count; i++)
                if (ProgramRun.Execute(level.Mistakes[i].Code, level.CreateWorld()).Complete)
                    problems.Add((i + 1) + ". tipik hata görevi bitiriyor; hata örneği gerçekten hatalı olmalı.");
            return problems;
        }

        static readonly Regex Identifier = new Regex(@"[A-Za-z_]\w*");

        /// <summary>Düğme satırları ve Python kelimeleri: her parça tek başına geçerli bir Python satırı olmalı,
        /// yalnızca bölümde açık komutları kullanmalı; kelimeleri motor tanımalı.</summary>
        static List<string> PieceProblems(Level level)
        {
            var problems = new List<string>();
            foreach (var piece in level.Pieces)
            {
                if (piece.Contains("\n") || piece.Trim().Length == 0)
                {
                    problems.Add("\"parcalar\" içindeki \"" + piece + "\" tek satırlık kod olmalı.");
                    continue;
                }
                // "else:" (ve ileride "elif ...:") tek başına Python'a geçersiz (önünde if ister); gecerliligini
                // sinamak icin onune sahte bir if eklenir, parcanin kendisi degismez.
                string trimmed = piece.TrimEnd();
                string source = trimmed == "else:" || trimmed.StartsWith("elif ")
                    ? "if True:\n    pass\n" + piece + "\n    pass\n"
                    : trimmed.EndsWith(":") ? piece + "\n    pass\n" : piece + "\n";
                try
                {
                    Parser.Parse(source);
                }
                catch (Exception e) when (e is PythonException || e is UnsupportedFeature)
                {
                    problems.Add("\"parcalar\" içindeki \"" + piece + "\" geçerli bir Python satırı değil.");
                    continue;
                }
                foreach (Match m in Identifier.Matches(piece))
                    if (Array.IndexOf(World.AllCommands, m.Value) >= 0 && !level.Commands.Contains(m.Value))
                        problems.Add("\"parcalar\" içindeki \"" + piece + "\" bu bölümde açık olmayan \"" + m.Value + "\" komutunu kullanıyor.");
            }
            foreach (var w in level.PythonWords)
                if (!Suggestions.IsPythonWord(w))
                    problems.Add("\"python_kelimeleri\" içindeki \"" + w + "\" motorun tanıdığı bir Python kelimesi değil.");
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
