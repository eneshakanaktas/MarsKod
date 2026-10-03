// Mars dünyasının kuralları: ızgara, robotun yeri, buzlar, kayalar, güneş panelleri, hedef kare ve oyuncunun kullandığı komutlar (move, collect...).
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

    /// <summary>Robot ilerleyemedi: alanın dışına çıkacaktı ya da önünde kaya vardı.</summary>
    public sealed class Blocked : WorldEvent
    {
        public Cell At;
        public Direction Direction;
        public bool ByRock;
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
        /// <summary>Görev tamam mı: tüm buzlar toplandı, çatlak panel kalmadı ve (hedef varsa) robot hedef karede</summary>
        public bool Complete => IceLeft == 0 && CrackedLeft == 0 && OnTarget;

        /// <summary>Oyunda olan tüm komutlar (bölüm dosyasındaki "komutlar" bunlardan seçilir)</summary>
        public static readonly string[] AllCommands = { "move", "collect", "ice_here", "rock_ahead", "panel_power", "repair" };

        /// <summary>Panel çatlak mı (gücü 0'dan çok, CrackedBelow'dan az)</summary>
        public static bool IsCracked(int power) => power > 0 && power < CrackedBelow;

        public World(int cols, int rows, Cell robot, IEnumerable<Cell> iceCells,
            IEnumerable<Cell> rockCells = null, Cell? target = null, IEnumerable<string> commands = null, IEnumerable<Cell> crystalCells = null,
            IDictionary<Cell, int> panelCells = null)
        {
            Cols = cols;
            Rows = rows;
            Robot = robot;
            ices = new List<Cell>(iceCells);
            taken = new bool[ices.Count];
            rocks = new HashSet<Cell>(rockCells ?? Array.Empty<Cell>());
            crystals = new HashSet<Cell>(crystalCells ?? Array.Empty<Cell>());
            panels = panelCells == null ? new Dictionary<Cell, int>() : new Dictionary<Cell, int>(panelCells);
            Target = target;
            allowed = commands == null ? null : new HashSet<string>(commands);
        }

        public bool Inside(Cell c) => c.Col >= 0 && c.Col < Cols && c.Row >= 0 && c.Row < Rows;

        public bool RockAt(Cell c) => rocks.Contains(c);

        public bool CrystalAt(Cell c) => crystals.Contains(c);

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

        static Direction DirectionOf(object value)
        {
            if (value is string s && Enum.TryParse(s, out Direction d) && s == d.ToString())
                return d;
            throw new GameRuleStop("Bilinmeyen yön",
                "move() bir yön ister: North (kuzey), East (doğu), South (güney) ya da West (batı). Parantezin içine bunlardan birini yaz, örn. move(East).");
        }
    }
}
