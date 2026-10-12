using MarsKod.Dunya;

namespace MotorTest;

// Sahnelerde robotun yalnızca boş karelerden yürüdüğü yol (Bölge 6, 51 ve 60): en kısa, engelin üstünden geçmez.
public class GridPathTests
{
    static readonly HashSet<Cell> None = new HashSet<Cell>();
    static readonly string Dir = Path.Combine(Cases.Root, "oyun", "Assets", "Resources", "Bolumler");

    static Level Load(int number) => Level.Parse(File.ReadAllText(Path.Combine(Dir, "bolum-" + number + ".json")));

    [Fact]
    public void Duz_yol_en_kisadir()
    {
        var path = GridPath.Find(6, 6, new Cell(0, 0), new Cell(3, 0), None);
        Assert.Equal(new[] { new Cell(0, 0), new Cell(1, 0), new Cell(2, 0), new Cell(3, 0) }, path);
    }

    [Fact]
    public void Kayanin_etrafindan_dolasir()
    {
        var path = GridPath.Find(6, 6, new Cell(0, 0), new Cell(2, 0), new HashSet<Cell> { new Cell(1, 0) });
        Assert.Equal(5, path.Count);
        Assert.DoesNotContain(new Cell(1, 0), path);
    }

    [Fact]
    public void Yol_yoksa_null()
    {
        var wall = new HashSet<Cell> { new Cell(1, 0), new Cell(1, 1), new Cell(0, 1) };
        Assert.Null(GridPath.Find(6, 6, new Cell(0, 0), new Cell(5, 5), wall));
    }

    [Fact]
    public void Ayni_karede_tek_kare()
    {
        Assert.Equal(new[] { new Cell(2, 2) }, GridPath.Find(6, 6, new Cell(2, 2), new Cell(2, 2), None));
    }

    [Fact]
    public void Satirda_tercihe_en_yakin_bos_kare()
    {
        var blocked = new HashSet<Cell> { new Cell(4, 5) };
        Assert.Equal(new Cell(3, 5), GridPath.OpenInRow(6, 5, 4, blocked));   // eşitlikte sola
        Assert.Null(GridPath.OpenInRow(2, 0, 0, new HashSet<Cell> { new Cell(0, 0), new Cell(1, 0) }));
    }

    [Fact]
    public void Toz_bulutu_engel_degildir()
    {
        var level = Level.Parse(File.ReadAllText(Path.Combine(Dir, "bolum-51.json")));
        Assert.NotEmpty(level.Dust);
        var blocked = GridPath.Obstacles(level, itemsTaken: true);
        foreach (var dust in level.Dust.Keys) Assert.DoesNotContain(dust, blocked);
    }

    [Fact]
    public void Bolum_60_bitince_hedeften_arka_siraya_yol_var()
    {
        var level = Load(60);
        var blocked = GridPath.Obstacles(level, itemsTaken: true);
        var exit = GridPath.OpenInRow(level.Cols, level.Rows - 1, 4, blocked);
        Assert.NotNull(exit);
        Assert.NotNull(GridPath.Find(level.Cols, level.Rows, level.Target.Value, exit.Value, blocked));
    }

    [Fact]
    public void Bolum_51_baslangica_sol_arka_siradan_yol_var()
    {
        var level = Load(51);
        var blocked = GridPath.Obstacles(level, itemsTaken: false);
        var entry = GridPath.OpenInRow(level.Cols, level.Rows - 1, 0, blocked);
        Assert.NotNull(entry);
        Assert.NotNull(GridPath.Find(level.Cols, level.Rows, entry.Value, level.Robot, blocked));
    }
}
