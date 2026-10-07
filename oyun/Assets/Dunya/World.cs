// Mars dünyasının kuralları: ızgara, robotun yeri, buzlar, kayalar, güneş panelleri, toz bulutları, hedef kare ve oyuncunun kullandığı komutlar (move, collect...).
// Saf mantıktır (Unity'den habersiz); sahne yalnızca olanları (WorldEvent) okuyup canlandırır.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using MarsKod.Motor;

namespace MarsKod.Dunya
{
    public enum Direction
    {
        North,
        East,
        South,
        West,
    }

    /// <summary>Izgaradaki bir kare. Col doğuya (sağa), Row kuzeye (ileriye) doğru artar.</summary>
    public readonly struct Cell : IEquatable<Cell>
    {
        public readonly int Col;
        public readonly int Row;

        public Cell(int col, int row)
        {
            Col = col;
            Row = row;
        }

        public Cell Step(Direction d)
        {
            switch (d)
            {
                case Direction.North: return new Cell(Col, Row + 1);
                case Direction.East: return new Cell(Col + 1, Row);
                case Direction.South: return new Cell(Col, Row - 1);
                default: return new Cell(Col - 1, Row);
            }
        }

        public bool Equals(Cell other) => Col == other.Col && Row == other.Row;
        public override bool Equals(object obj) => obj is Cell c && Equals(c);
        public override int GetHashCode() => Col * 397 ^ Row;
        public override string ToString() => "(" + Col + ", " + Row + ")";
    }

    /// <summary>Dünyada olan bir şey; sahne bunları sırayla canlandırır.</summary>
    public abstract class WorldEvent
    {
    }

    /// <summary>Robot bir kare ilerledi.</summary>
    public sealed class Moved : WorldEvent
    {
        public Cell From, To;
        public Direction Direction;
    }

    /// <summary>Robot ilerleyemedi: alanın dışına çıkacaktı, önünde kaya vardı ya da toz bulutunun içindeydi.</summary>
    public sealed class Blocked : WorldEvent
    {
        public Cell At;
        public Direction Direction;
        public bool ByRock;
        public bool ByDust;
    }

    /// <summary>Robot toplamayı denedi. Buz yoksa IceIndex -1.</summary>
    public sealed class Collected : WorldEvent
    {
        public Cell At;
        public int IceIndex;
        /// <summary>Şimdiye kadar toplanan buz sayısı</summary>
        public int Total;
    }

    /// <summary>Robot ice_here() ile bulunduğu kareye baktı (taramayı sahne gösterir).</summary>
    public sealed class Scanned : WorldEvent
    {
        public Cell At;
        public bool Found;
    }

    /// <summary>Robot yapılamayacak bir işi denedi (kristali ya da çalışan paneli toplamak, yanlış paneli onarmak); kural gereği durur.</summary>
    public sealed class Rejected : WorldEvent
    {
        public Cell At;
    }

    /// <summary>Robot panel_power() ile bulunduğu karedeki panelin gücünü ölçtü (panel yoksa 0).</summary>
    public sealed class Measured : WorldEvent
    {
        public Cell At;
        public int Power;
    }

    /// <summary>Robot çatlak paneli onardı; panelin gücü artık World.RepairedPower.</summary>
    public sealed class Repaired : WorldEvent
    {
        public Cell At;
    }

    /// <summary>Robot toz bulutunun içinde bir tur bekledi; bulut inceldi (Left 0 ise dağıldı).</summary>
    public sealed class Waited : WorldEvent
    {
        public Cell At;
        /// <summary>Bulutun dağılması için kalan bekleme sayısı</summary>
        public int Left;
        /// <summary>Bulutun başlangıçtaki bekleme sayısı (sahne bulutu buna göre inceltir)</summary>
        public int Total;
    }

    /// <summary>Robot cable_length() ile bulunduğu karedeki kablo makarasının uzunluğunu ölçtü (makara yoksa 0).</summary>
    public sealed class CableMeasured : WorldEvent
    {
        public Cell At;
        /// <summary>Metre</summary>
        public int Length;
    }

    /// <summary>Robot report() ile antene bir sayı gönderdi; Correct, antenin beklediği sayı mı.</summary>
    public sealed class ReportSent : WorldEvent
    {
        public Cell At;
        public BigInteger Value;
        public bool Correct;
    }

    /// <summary>
    /// Oyun kuralı yüzünden durma (Python hatası değil): örn. robot alanın dışına çıkmak istedi.
    /// Oyuncunun kodu bunu yakalayamaz; çalıştırma olduğu yerde biter.
    /// </summary>
    public sealed class GameRuleStop : Exception
    {
        public readonly string Title;
        public readonly string Text;

        public GameRuleStop(string title, string text) : base(title)
        {
            Title = title;
            Text = text;
        }
    }

    public sealed class World
    {
        public readonly int Cols, Rows;
        public Cell Robot { get; private set; }
        /// <summary>Robotun kod bitince durması gereken kare; yoksa null</summary>
        public readonly Cell? Target;
        readonly List<Cell> ices;
        readonly bool[] taken;
        readonly HashSet<Cell> rocks;
        /// <summary>Tehlikeli kristaller: üstünden geçilir ama toplanamaz</summary>
        readonly HashSet<Cell> crystals;
        /// <summary>Güneş panelleri ve güçleri (0-100): 0 kırık (parçası toplanır), CrackedBelow'dan az çatlak (onarılır), gerisi sağlam</summary>
        readonly Dictionary<Cell, int> panels;
        /// <summary>Toz bulutları: dağılması için kalan bekleme sayısı (0 = dağıldı). Robot bulutun içindeyken yürüyemez.</summary>
        readonly Dictionary<Cell, int> dust;
        readonly Dictionary<Cell, int> dustTotal;
        /// <summary>Kablo makaralarının uzunlukları (metre); makaralar toplanacaklar arasındadır (Ices)</summary>
        readonly Dictionary<Cell, int> reels;
        /// <summary>Antenin report() ile beklediği sayı; null ise bu bölümde rapor istenmez</summary>
        public readonly BigInteger? ExpectedReport;
        /// <summary>Bu bölümde açık olan komutlar; null ise hepsi açık</summary>
        readonly HashSet<string> allowed;

        /// <summary>Gücü bundan az (ama 0'dan çok) olan panel çatlaktır</summary>
        public const int CrackedBelow = 50;
        /// <summary>Onarılan panelin yeni gücü</summary>
        public const int RepairedPower = 100;

        /// <summary>Son okunduğundan beri olanlar; okuyan temizler.</summary>
        public readonly List<WorldEvent> Events = new List<WorldEvent>();

        public int IceCount => ices.Count;
        public int CollectedCount { get; private set; }
        public int IceLeft => ices.Count - CollectedCount;
        public bool OnTarget => Target == null || Robot.Equals(Target.Value);
        /// <summary>Henüz onarılmamış çatlak panel sayısı</summary>
        public int CrackedLeft => panels.Values.Count(IsCracked);
        /// <summary>Doğru sayı antene gönderildi mi (rapor istenmeyen bölümde hep true)</summary>
        public bool ReportDone => ExpectedReport == null || reported;
        bool reported;
        /// <summary>Görev tamam mı: tüm buzlar toplandı, çatlak panel kalmadı, (istendiyse) doğru rapor gönderildi ve (hedef varsa) robot hedef karede</summary>
        public bool Complete => IceLeft == 0 && CrackedLeft == 0 && ReportDone && OnTarget;

        /// <summary>Oyunda olan tüm komutlar (bölüm dosyasındaki "komutlar" bunlardan seçilir)</summary>
        public static readonly string[] AllCommands = { "move", "collect", "ice_here", "rock_ahead", "panel_power", "repair", "dust_here", "wait", "cable_length", "report" };

        /// <summary>Panel çatlak mı (gücü 0'dan çok, CrackedBelow'dan az)</summary>
        public static bool IsCracked(int power) => power > 0 && power < CrackedBelow;

        public World(int cols, int rows, Cell robot, IEnumerable<Cell> iceCells,
            IEnumerable<Cell> rockCells = null, Cell? target = null, IEnumerable<string> commands = null, IEnumerable<Cell> crystalCells = null,
            IDictionary<Cell, int> panelCells = null, IDictionary<Cell, int> dustCells = null,
            IDictionary<Cell, int> reelCells = null, BigInteger? expectedReport = null)
        {
            Cols = cols;
            Rows = rows;
            Robot = robot;
            ices = new List<Cell>(iceCells);
            taken = new bool[ices.Count];
            rocks = new HashSet<Cell>(rockCells ?? Array.Empty<Cell>());
            crystals = new HashSet<Cell>(crystalCells ?? Array.Empty<Cell>());
            panels = panelCells == null ? new Dictionary<Cell, int>() : new Dictionary<Cell, int>(panelCells);
            dust = dustCells == null ? new Dictionary<Cell, int>() : new Dictionary<Cell, int>(dustCells);
            dustTotal = new Dictionary<Cell, int>(dust);
            reels = reelCells == null ? new Dictionary<Cell, int>() : new Dictionary<Cell, int>(reelCells);
            ExpectedReport = expectedReport;
            Target = target;
            allowed = commands == null ? null : new HashSet<string>(commands);
        }

        public bool Inside(Cell c) => c.Col >= 0 && c.Col < Cols && c.Row >= 0 && c.Row < Rows;

        public bool RockAt(Cell c) => rocks.Contains(c);

        public bool CrystalAt(Cell c) => crystals.Contains(c);

        /// <summary>Karede henüz dağılmamış toz bulutu var mı</summary>
        public bool DustAt(Cell c) => dust.TryGetValue(c, out int left) && left > 0;

        /// <summary>Karedeki panelin gücü; panel yoksa (ya da kırık panelin parçası toplandıysa) 0</summary>
        public int PanelPowerAt(Cell c) => panels.TryGetValue(c, out int power) ? power : 0;

        /// <summary>Bu karede henüz toplanmamış buzun sırası; yoksa -1</summary>
        public int IceAt(Cell c)
        {
            for (int i = 0; i < ices.Count; i++)
                if (!taken[i] && ices[i].Equals(c)) return i;
            return -1;
        }

        public void Move(Direction d)
        {
            if (DustAt(Robot))
            {
                Events.Add(new Blocked { At = Robot, Direction = d, ByDust = true });
                throw new GameRuleStop("Tozda yol görünmüyor",
                    "Robot toz bulutunun içinde, önünü göremediği için yürümedi. Bu bir oyun kuralı, Python hatası değil: toz geçene kadar beklemelisin. Tozun ne kadar süreceğini bilemezsin; while dust_here(): ile toz varken wait() de.");
            }
            var to = Robot.Step(d);
            if (!Inside(to))
            {
                Events.Add(new Blocked { At = Robot, Direction = d });
                throw new GameRuleStop("Alanın sınırı",
                    "Robot bu yönde alanın dışına çıkacaktı, o yüzden durdu. Bu bir oyun kuralı, Python hatası değil: kodun doğru yazılmış ama robotu alanın dışına götürüyor.");
            }
            if (RockAt(to))
            {
                Events.Add(new Blocked { At = Robot, Direction = d, ByRock = true });
                throw new GameRuleStop("Önünde kaya var",
                    "Robot kayanın içinden geçemez, o yüzden durdu. Bu bir oyun kuralı, Python hatası değil: kodun doğru yazılmış ama robotu kayaya sürüyor. Kayanın etrafından dolaşan bir yol bul.");
            }
            Events.Add(new Moved { From = Robot, To = to, Direction = d });
            Robot = to;
        }

        /// <summary>Bulunduğu karedeki buzu toplar. Buz varsa True döner.</summary>
        public bool Collect()
        {
            if (CrystalAt(Robot))
            {
                Events.Add(new Rejected { At = Robot });
                throw new GameRuleStop("Tehlikeli kristal",
                    "Kırmızı kristal toplanmaz, robot bu yüzden durdu. Bu bir oyun kuralı, Python hatası değil: kristalin üstünden geçebilirsin ama collect() onu toplamaya çalışır. Önce ice_here() ile bak: buz varsa topla.");
            }
            int power = PanelPowerAt(Robot);
            if (power > 0)
            {
                Events.Add(new Rejected { At = Robot });
                throw new GameRuleStop("Çalışan panel sökülmez",
                    "Bu panelin gücü " + power + ", yani çalışıyor; collect() yalnızca kırık panelin (gücü 0) parçasını toplar. Bu bir oyun kuralı, Python hatası değil. Önce panel_power() ile ölç: 0 ise topla.");
            }
            int i = IceAt(Robot);
            if (i >= 0)
            {
                taken[i] = true;
                CollectedCount++;
                panels.Remove(Robot); // kırık panelin parçası toplandıysa karede artık panel yok
            }
            Events.Add(new Collected { At = Robot, IceIndex = i, Total = CollectedCount });
            return i >= 0;
        }

        /// <summary>Bulunduğu karede toplanmamış buz var mı? Toplamaz, yalnızca bakar.</summary>
        public bool IceHere()
        {
            bool found = IceAt(Robot) >= 0;
            Events.Add(new Scanned { At = Robot, Found = found });
            return found;
        }

        /// <summary>O yönde bir adım ilerlerse robot durur mu (kaya ya da alanın sınırı)? İlerlemez, yalnızca bakar.</summary>
        public bool RockAhead(Direction d)
        {
            var to = Robot.Step(d);
            bool blocked = !Inside(to) || RockAt(to);
            Events.Add(new Scanned { At = to, Found = blocked });
            return blocked;
        }

        /// <summary>Bulunduğu karede toz bulutu var mı? Yalnızca bakar.</summary>
        public bool DustHere()
        {
            bool found = DustAt(Robot);
            Events.Add(new Scanned { At = Robot, Found = found });
            return found;
        }

        /// <summary>Toz bulutunun içinde bir tur bekler; bulut incelir. Toz yoksa robot durur (oyun kuralı: boşuna beklenmez).</summary>
        public void Wait()
        {
            if (!DustAt(Robot))
            {
                Events.Add(new Rejected { At = Robot });
                throw new GameRuleStop("Beklenecek toz yok",
                    "Robotun durduğu karede toz yok, beklemek pili boşa harcar; robot bu yüzden durdu. Bu bir oyun kuralı, Python hatası değil: tozun ne kadar süreceğini bilemezsin, o yüzden sayarak değil sorarak bekle: while dust_here(): altında wait().");
            }
            int left = dust[Robot] - 1;
            dust[Robot] = left;
            Events.Add(new Waited { At = Robot, Left = left, Total = dustTotal[Robot] });
        }

        /// <summary>Bulunduğu karedeki (toplanmamış) kablo makarasının uzunluğu, metre; makara yoksa 0. Yalnızca ölçer.</summary>
        public int CableLength()
        {
            int length = IceAt(Robot) >= 0 && reels.TryGetValue(Robot, out int l) ? l : 0;
            Events.Add(new CableMeasured { At = Robot, Length = length });
            return length;
        }

        /// <summary>Sayıyı antene gönderir. Antenin beklediği sayı değilse (ya da bu bölümde rapor istenmiyorsa) robot durur (oyun kuralı).</summary>
        public void Report(BigInteger value)
        {
            bool correct = ExpectedReport != null && ExpectedReport.Value == value;
            Events.Add(new ReportSent { At = Robot, Value = value, Correct = correct });
            if (ExpectedReport == null)
                throw new GameRuleStop("Rapor istenmiyor",
                    "Bu ödevde antene bir sayı göndermen istenmiyor, robot bu yüzden durdu. Bu bir oyun kuralı, Python hatası değil.");
            if (!correct)
                throw new GameRuleStop("Anten bu sayıyı beklemiyordu",
                    "Robot antene " + value + " gönderdi ama anten başka bir sayı bekliyordu; robot bu yüzden durdu. Bu bir oyun kuralı, Python hatası değil. "
                    + "Sayıyı tahmin etme, robota saydır: bir değişken kur (sayac = 0), her seferinde büyüt (sayac += 1), sonunda onu gönder: report(sayac).");
            reported = true;
        }

        /// <summary>Bulunduğu karedeki panelin gücü (0-100; panel yoksa 0). Değiştirmez, yalnızca ölçer.</summary>
        public int PanelPower()
        {
            int power = PanelPowerAt(Robot);
            Events.Add(new Measured { At = Robot, Power = power });
            return power;
        }

        /// <summary>Bulunduğu karedeki çatlak paneli onarır. Kırık, sağlam ya da hiç panel yoksa robot durur (oyun kuralı).</summary>
        public void Repair()
        {
            bool hasPanel = panels.TryGetValue(Robot, out int power);
            if (hasPanel && IsCracked(power))
            {
                panels[Robot] = RepairedPower;
                Events.Add(new Repaired { At = Robot });
                return;
            }
            Events.Add(new Rejected { At = Robot });
            if (!hasPanel)
                throw new GameRuleStop("Burada panel yok",
                    "Robotun durduğu karede onarılacak bir panel yok, o yüzden durdu. Bu bir oyun kuralı, Python hatası değil: repair() yalnızca çatlak panelin üstünde çalışır.");
            if (power == 0)
                throw new GameRuleStop("Kırık panel onarılmaz",
                    "Bu panelin gücü 0, yani tamamen kırık; onarılamaz, robot bu yüzden durdu. Bu bir oyun kuralı, Python hatası değil: kırık panelin parçası collect() ile toplanır.");
            throw new GameRuleStop("Panel zaten sağlam",
                "Bu panelin gücü " + power + "; " + CrackedBelow + " ya da daha fazlası sağlam demek, onarılacak bir şey yok. Robot bu yüzden durdu. Bu bir oyun kuralı, Python hatası değil: önce panel_power() ile ölç, yalnızca gücü " + CrackedBelow + "'den az olanı onar.");
        }

        // --- oyuncunun kullandığı komutlar ---

        /// <summary>Motora dışarıdan verilen isimler: oyun komutları (AllCommands) ve yönler (North, East, South, West).</summary>
        public Dictionary<string, object> Commands()
        {
            var commands = new Dictionary<string, object>
            {
                ["move"] = new PyBuiltin("move", (args, kwargs) =>
                {
                    Unlocked("move");
                    NoKeywords("move", kwargs);
                    if (args.Count != 1) throw Values.PyError("TypeError", "move() takes exactly one argument (" + args.Count + " given)");
                    Move(DirectionOf(args[0]));
                    return null;
                }),
                ["collect"] = new PyBuiltin("collect", (args, kwargs) =>
                {
                    Unlocked("collect");
                    NoKeywords("collect", kwargs);
                    if (args.Count != 0) throw Values.PyError("TypeError", "collect() takes no arguments (" + args.Count + " given)");
                    return Collect();
                }),
                ["ice_here"] = new PyBuiltin("ice_here", (args, kwargs) =>
                {
                    Unlocked("ice_here");
                    NoKeywords("ice_here", kwargs);
                    if (args.Count != 0) throw Values.PyError("TypeError", "ice_here() takes no arguments (" + args.Count + " given)");
                    return IceHere();
                }),
                ["rock_ahead"] = new PyBuiltin("rock_ahead", (args, kwargs) =>
                {
                    Unlocked("rock_ahead");
                    NoKeywords("rock_ahead", kwargs);
                    if (args.Count != 1) throw Values.PyError("TypeError", "rock_ahead() takes exactly one argument (" + args.Count + " given)");
                    return RockAhead(DirectionOf(args[0]));
                }),
                ["panel_power"] = new PyBuiltin("panel_power", (args, kwargs) =>
                {
                    Unlocked("panel_power");
                    NoKeywords("panel_power", kwargs);
                    if (args.Count != 0) throw Values.PyError("TypeError", "panel_power() takes no arguments (" + args.Count + " given)");
                    return new BigInteger(PanelPower());
                }),
                ["repair"] = new PyBuiltin("repair", (args, kwargs) =>
                {
                    Unlocked("repair");
                    NoKeywords("repair", kwargs);
                    if (args.Count != 0) throw Values.PyError("TypeError", "repair() takes no arguments (" + args.Count + " given)");
                    Repair();
                    return null;
                }),
                ["dust_here"] = new PyBuiltin("dust_here", (args, kwargs) =>
                {
                    Unlocked("dust_here");
                    NoKeywords("dust_here", kwargs);
                    if (args.Count != 0) throw Values.PyError("TypeError", "dust_here() takes no arguments (" + args.Count + " given)");
                    return DustHere();
                }),
                ["wait"] = new PyBuiltin("wait", (args, kwargs) =>
                {
                    Unlocked("wait");
                    NoKeywords("wait", kwargs);
                    if (args.Count != 0) throw Values.PyError("TypeError", "wait() takes no arguments (" + args.Count + " given)");
                    Wait();
                    return null;
                }),
                ["cable_length"] = new PyBuiltin("cable_length", (args, kwargs) =>
                {
                    Unlocked("cable_length");
                    NoKeywords("cable_length", kwargs);
                    if (args.Count != 0) throw Values.PyError("TypeError", "cable_length() takes no arguments (" + args.Count + " given)");
                    return new BigInteger(CableLength());
                }),
                ["report"] = new PyBuiltin("report", (args, kwargs) =>
                {
                    Unlocked("report");
                    NoKeywords("report", kwargs);
                    if (args.Count != 1) throw Values.PyError("TypeError", "report() takes exactly one argument (" + args.Count + " given)");
                    Report(NumberOf(args[0]));
                    return null;
                }),
            };
            // Yönler şimdilik metin olarak tutulur: print(East) -> East
            foreach (Direction d in Enum.GetValues(typeof(Direction))) commands[d.ToString()] = d.ToString();
            return commands;
        }

        // Bölümde henüz açılmamış bir komut çağrılırsa: Python hatası değil, oyun kuralı
        void Unlocked(string name)
        {
            if (allowed == null || allowed.Contains(name)) return;
            var open = new List<string>();
            foreach (var c in AllCommands)
                if (allowed.Contains(c)) open.Add(c + "()");
            throw new GameRuleStop("Bu komut henüz açılmadı",
                name + "() gerçek bir oyun komutu ama bu bölümde henüz kullanılamıyor; ilerideki bölümlerde açılacak. Bu bölümde kullanabileceklerin: "
                + string.Join(", ", open) + ".");
        }

        static void NoKeywords(string name, Dictionary<string, object> kwargs)
        {
            if (kwargs.Count > 0) throw Values.PyError("TypeError", name + "() takes no keyword arguments");
        }

        // report() yalnızca tam sayı gönderir (True/False ve ondalıklı sayı da kabul edilmez: anten metre ya da adet sayar)
        static BigInteger NumberOf(object value)
        {
            if (value is BigInteger n) return n;
            throw new GameRuleStop("Antene yalnızca sayı gönderilir",
                "report() bir tam sayı ister, örn. report(3) ya da report(toplam). Gönderdiğin değer (" + Values.Repr(value) + ") bir tam sayı değil. Bu bir oyun kuralı, Python hatası değil.");
        }

        static Direction DirectionOf(object value)
        {
            if (value is string s && Enum.TryParse(s, out Direction d) && s == d.ToString())
                return d;
            throw new GameRuleStop("Bilinmeyen yön",
                "move() bir yön ister: North (kuzey), East (doğu), South (güney) ya da West (batı). Parantezin içine bunlardan birini yaz, örn. move(East).");
        }
    }
}
