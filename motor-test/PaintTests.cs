using MarsKod.Dunya;

namespace MotorTest;

// Çizim (Bölge 7, Bölüm 61-70): Kıvılcım kalem inikken geçtiği kareleri boyar; görev, boyanacak karelerin hepsi
// boyanıp fazladan kare boyanmayınca biter.
public class PaintTests
{
    static readonly Cell A = new Cell(1, 0), B = new Cell(2, 0), C = new Cell(3, 0), Far = new Cell(0, 0);

    static PaintBoard Board() => new PaintBoard(new[] { A, B, C });

    // ---- PaintBoard ----

    [Fact]
    public void Kalem_kalkikken_gezmek_boyamaz()
    {
        var board = Board();
        Assert.False(board.Enter(A));
        Assert.False(board.PaintedAt(A));
        Assert.Equal(3, board.Missing);
    }

    [Fact]
    public void Kalemi_indirmek_bulundugu_kareyi_boyar()
    {
        var board = new PaintBoard(new[] { A });
        Assert.True(board.Lower(A));
        Assert.True(board.PenDown);
        Assert.True(board.PaintedAt(A));
        Assert.Equal(0, board.Missing);
        Assert.True(board.Done);
    }

    [Fact]
    public void Kalem_inikken_girilen_kare_boyanir()
    {
        var board = Board();
        board.Lower(A);
        Assert.True(board.Enter(B));
        Assert.True(board.PaintedAt(B));
        Assert.Equal(1, board.Missing);
    }

    [Fact]
    public void Kalem_kaldirilinca_boyamaz_ve_tekrar_indirilince_boyar()
    {
        var board = Board();
        board.Lower(A);
        board.Raise();
        Assert.False(board.PenDown);
        Assert.False(board.Enter(B));
        Assert.True(board.Lower(C));
        Assert.True(board.PaintedAt(C));
        Assert.False(board.PaintedAt(B));
    }

    [Fact]
    public void Hedef_disi_kare_fazla_sayilir()
    {
        var board = Board();
        board.Lower(Far);
        Assert.Equal(1, board.Extra);
        Assert.False(board.Done);
    }

    [Fact]
    public void Ayni_kareyi_iki_kez_boyamak_fazla_degildir()
    {
        var board = new PaintBoard(new[] { A });
        board.Lower(A);
        Assert.False(board.Enter(A)); // ikinci boyama yeni kare değil
        Assert.Equal(0, board.Extra);
        Assert.True(board.Done);
    }

    [Fact]
    public void Iki_kez_ayni_komut_hata_degildir()
    {
        var board = Board();
        board.Lower(A);
        board.Lower(A);
        board.Raise();
        board.Raise();
        Assert.False(board.PenDown);
        Assert.Equal(2, board.Missing);
    }

    [Fact]
    public void Boyanacak_kare_yoksa_cizim_etkin_degildir_ve_bitmis_sayilir()
    {
        var board = new PaintBoard(new Cell[0]);
        Assert.False(board.Active);
        Assert.True(board.Done);
        Assert.Null(board.Problem());
    }

    [Fact]
    public void Eksik_kare_mesaji_sayiyi_yazar()
    {
        var board = Board();
        board.Lower(A);
        var problem = board.Problem();
        Assert.Contains("İşaret yarım kaldı", problem.Title);
        Assert.Contains("2 kare boyanmadı", problem.Text);
    }

    [Fact]
    public void Fazla_kare_mesaji_sayiyi_yazar_ve_pen_up_der()
    {
        var board = new PaintBoard(new[] { A });
        board.Lower(A);
        board.Enter(B);
        board.Enter(C);
        var problem = board.Problem();
        Assert.Contains("İşaret bozuldu", problem.Title);
        Assert.Contains("Fazladan 2 kare boyadın", problem.Text);
        Assert.Contains("pen_up()", problem.Text);
    }

    [Fact]
    public void Kalem_hic_inmediyse_bunu_soyler()
    {
        var board = Board();
        var problem = board.Problem();
        Assert.False(board.EverLowered);
        Assert.Contains("Kalem hiç inmedi", problem.Title);
        Assert.Contains("pen_down()", problem.Text);
    }

    [Fact]
    public void Eksik_ve_fazla_birlikteyse_fazla_mesaji_gelir()
    {
        var board = Board();
        board.Lower(Far);
        Assert.Contains("Fazladan 1 kare", board.Problem().Text);
    }

    [Fact]
    public void Tamamlanan_cizimde_sorun_yoktur()
    {
        var board = Board();
        board.Lower(A);
        board.Enter(B);
        board.Enter(C);
        Assert.Null(board.Problem());
        Assert.True(board.Done);
    }

    // ---- World ----

    // Robot (0,0); boyanacak kareler doğuda (1,0), (2,0), (3,0).
    static World Strip(IEnumerable<string> commands = null) => new World(6, 6, new Cell(0, 0), new Cell[0],
        commands: commands ?? new[] { "move", "pen_down", "pen_up" }, paintCells: new[] { A, B, C });

    const string DrawThree = "move(East)\npen_down()\nmove(East)\nmove(East)\n";

    [Fact]
    public void Kalemle_yurumek_kareleri_boyar_ve_gorevi_bitirir()
    {
        var world = Strip();
        var report = ProgramRun.Execute(DrawThree, world);
        Assert.True(report.Complete);
        Assert.Equal(0, world.Paint.Missing);
        Assert.Equal(0, world.Paint.Extra);
    }

    [Fact]
    public void Fazla_kare_boyamak_gorevi_bitirmez()
    {
        var world = Strip();
        var report = ProgramRun.Execute("pen_down()\n" + DrawThree, world); // başlangıç karesi boyanacak değil
        Assert.False(report.Complete);
        Assert.Equal(1, world.Paint.Extra);
        Assert.Equal(0, world.Paint.Missing);
    }

    [Fact]
    public void Yarim_kalan_cizim_bitirmez()
    {
        var world = Strip();
        var report = ProgramRun.Execute("move(East)\npen_down()\nmove(East)\n", world);
        Assert.False(report.Complete);
        Assert.Equal(1, world.Paint.Missing);
    }

    [Fact]
    public void Kalem_hic_inmediyse_gorev_bitmez()
    {
        var world = Strip();
        var report = ProgramRun.Execute("move(East)\nmove(East)\nmove(East)\n", world);
        Assert.False(report.Complete);
        Assert.False(world.Paint.EverLowered);
    }

    [Fact]
    public void Kural_ile_duran_kod_tamam_sayilmaz()
    {
        var world = Strip();
        // çizim tamam, sonra kalem kalkık alanın dışına yürür
        var report = ProgramRun.Execute(DrawThree + "pen_up()\nmove(East)\nmove(East)\nmove(East)\n", world);
        Assert.NotNull(report.Rule);
        Assert.True(world.Paint.Done);
        Assert.False(report.Complete);
    }

    [Fact]
    public void Cizim_komutlari_cizimsiz_bolumde_kapali()
    {
        var world = Strip(new[] { "move" });
        var report = ProgramRun.Execute("pen_down()\n", world);
        Assert.Contains("Bu komut henüz açılmadı", report.Rule.Title);
    }

    [Fact]
    public void Kalem_olaylari_kaydedilir()
    {
        var report = ProgramRun.Execute(DrawThree, Strip());
        var events = report.Trace.SelectMany(t => t.Events).ToList();
        var pen = Assert.Single(events.OfType<PenChanged>());
        Assert.True(pen.Down);
        Assert.Equal(A, pen.At);
        var painted = events.OfType<Painted>().ToList();
        Assert.Equal(new[] { A, B, C }, painted.Select(e => e.At));
        Assert.All(painted, e => Assert.True(e.Wanted));
    }

    [Fact]
    public void Hedef_disi_boyama_olayi_istenmedi_der()
    {
        var report = ProgramRun.Execute("pen_down()\n", Strip());
        Assert.False(report.Trace.SelectMany(t => t.Events).OfType<Painted>().Single().Wanted);
    }

    [Fact]
    public void Kalem_kalkikken_yurumek_olay_uretmez()
    {
        var report = ProgramRun.Execute("move(East)\nmove(East)\n", Strip());
        Assert.Empty(report.Trace.SelectMany(t => t.Events).OfType<Painted>());
    }

    [Fact]
    public void Ikinci_pen_down_ve_pen_up_yeni_olay_uretmez()
    {
        var report = ProgramRun.Execute("move(East)\npen_down()\npen_down()\npen_up()\npen_up()\n", Strip());
        var events = report.Trace.SelectMany(t => t.Events).ToList();
        Assert.Equal(new[] { true, false }, events.OfType<PenChanged>().Select(e => e.Down));
        Assert.Single(events.OfType<Painted>());
    }

    [Fact]
    public void Cizimsiz_dunya_eskisi_gibi_calisir()
    {
        var world = new World(6, 6, new Cell(0, 0), new[] { new Cell(1, 0) }, commands: new[] { "move", "collect" });
        Assert.False(world.Paint.Active);
        Assert.True(ProgramRun.Execute("move(East)\ncollect()\n", world).Complete);
    }
}
