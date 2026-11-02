using System.Linq;
using MarsKod.Dunya;
using Xunit;

public class RecordingTests
{
    [Fact]
    public void Katalog_sirali_ve_dolu()
    {
        Assert.Equal(new[] { 1, 2, 3 }, Recordings.All.Select(r => r.Number));
        Assert.Equal(Recordings.All.Count, Recordings.All.Select(r => r.File).Distinct().Count());
        foreach (var r in Recordings.All)
        {
            Assert.NotEmpty(r.Lines);
            Assert.All(r.Lines, l => Assert.False(string.IsNullOrWhiteSpace(l)));
            Assert.EndsWith(".ses", r.File);
        }
    }

    // Spec 2.2'deki metin (hikaye-kitabi.md "Kayıtlar" satırı 3 ile aynı olmalı)
    [Fact]
    public void Kayit_3_uzun_metin()
    {
        var r = Recordings.ByNumber(3);
        Assert.Equal("izler.ses", r.File);
        Assert.Equal((7, 64, true), (r.Region, r.Level, r.AtLevelStart));
        Assert.Equal("Ece yine önden koştu. Bir anlığına onu kaybettim. Sonra buldum; bir duvarın önünde, elinde boya kalemi. Ona kızamadım.",
                     string.Join(" ", r.Lines));
    }

    [Fact]
    public void Bolum_basi_kayitlari()
    {
        Assert.Equal(2, Recordings.AtStartOf(52).Number);
        Assert.Equal(3, Recordings.AtStartOf(64).Number);
        Assert.Null(Recordings.AtStartOf(50));   // Kayıt 1 kapanışta, başta değil
        Assert.Null(Recordings.AtStartOf(61));
        Assert.Null(Recordings.ByNumber(9));
    }

    [Fact]
    public void Onizleme_ilk_kelimeler()
    {
        Assert.Equal("Ona ezber değil, beceri…", Recordings.Preview(Recordings.ByNumber(2)));
        Assert.Equal("Bu bir ödev…", Recordings.Preview(Recordings.ByNumber(1), 3));
        Assert.Equal("Ece yine önden koştu.", Recordings.Preview(Recordings.ByNumber(3)));   // tam dört kelime: üç nokta yok
    }

    [Fact]
    public void Dinlenenler_sirayla_listelenir()
    {
        var log = new RecordingLog();
        Assert.False(log.Any);
        Assert.Empty(log.HeardList());
        log.Mark(3); log.Mark(1); log.Mark(3);
        Assert.True(log.Heard(1));
        Assert.False(log.Heard(2));
        Assert.Equal(new[] { 1, 3 }, log.HeardList().Select(r => r.Number));
        Assert.Equal("1,3", log.Save());
    }

    [Fact]
    public void Kayit_metni_geri_okunur_bozuk_parca_atlanir()
    {
        Assert.Equal("1,3", RecordingLog.Load("3,1").Save());
        Assert.Equal("2", RecordingLog.Load("x,2,,99").Save());   // 99 katalogda yok
        Assert.False(RecordingLog.Load("").Any);
        Assert.False(RecordingLog.Load(null).Any);
    }
}
