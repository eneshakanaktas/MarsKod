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
}
