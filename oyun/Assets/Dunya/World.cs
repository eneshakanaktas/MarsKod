// Mars dünyasının kuralları: ızgara, robotun yeri, buzlar ve oyuncunun kullandığı komutlar (move, collect...).
// Saf mantıktır (Unity'den habersiz); sahne yalnızca olanları (WorldEvent) okuyup canlandırır.

using System;
using System.Collections.Generic;
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

    /// <summary>Robot ilerleyemedi (alanın dışına çıkacaktı).</summary>
    public sealed class Blocked : WorldEvent
    {
        public Cell At;
        public Direction Direction;
    }

    /// <summary>Robot toplamayı denedi. Buz yoksa IceIndex -1.</summary>
    public sealed class Collected : WorldEvent
    {
        public Cell At;
        public int IceIndex;
        /// <summary>Şimdiye kadar toplanan buz sayısı</summary>
        public int Total;
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
        readonly List<Cell> ices;
        readonly bool[] taken;

        /// <summary>Son okunduğundan beri olanlar; okuyan temizler.</summary>
        public readonly List<WorldEvent> Events = new List<WorldEvent>();

        public int IceCount => ices.Count;
        public int CollectedCount { get; private set; }
        public bool Complete => CollectedCount == ices.Count;

        public World(int cols, int rows, Cell robot, IEnumerable<Cell> iceCells)
        {
            Cols = cols;
            Rows = rows;
            Robot = robot;
            ices = new List<Cell>(iceCells);
            taken = new bool[ices.Count];
        }

        public bool Inside(Cell c) => c.Col >= 0 && c.Col < Cols && c.Row >= 0 && c.Row < Rows;

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
            Events.Add(new Moved { From = Robot, To = to, Direction = d });
            Robot = to;
        }

        /// <summary>Bulunduğu karedeki buzu toplar. Buz varsa True döner.</summary>
        public bool Collect()
        {
            int i = IceAt(Robot);
            if (i >= 0)
            {
                taken[i] = true;
                CollectedCount++;
            }
            Events.Add(new Collected { At = Robot, IceIndex = i, Total = CollectedCount });
            return i >= 0;
        }

        // --- oyuncunun kullandığı komutlar ---

        /// <summary>Motora dışarıdan verilen isimler: move, collect ve yönler (North, East, South, West).</summary>
        public Dictionary<string, object> Commands()
        {
            var commands = new Dictionary<string, object>
            {
                ["move"] = new PyBuiltin("move", (args, kwargs) =>
                {
                    NoKeywords("move", kwargs);
                    if (args.Count != 1) throw Values.PyError("TypeError", "move() takes exactly one argument (" + args.Count + " given)");
                    Move(DirectionOf(args[0]));
                    return null;
                }),
                ["collect"] = new PyBuiltin("collect", (args, kwargs) =>
                {
                    NoKeywords("collect", kwargs);
                    if (args.Count != 0) throw Values.PyError("TypeError", "collect() takes no arguments (" + args.Count + " given)");
                    return Collect();
                }),
            };
            // Yönler şimdilik metin olarak tutulur: print(East) -> East
            foreach (Direction d in Enum.GetValues(typeof(Direction))) commands[d.ToString()] = d.ToString();
            return commands;
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
