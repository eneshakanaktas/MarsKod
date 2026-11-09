namespace MotorTest;

// Testlerde kullanılan küçük bölüm metinleri (tek yerde)
static class LevelJson
{
    // 72 gibi ekranlı bölüm: robot yürümez, iki_kat(sayi) fonksiyonu sorulur. extra: sona eklenecek alanlar (", \"parcalar\": [...]")
    public static string Screen(string extra = "") =>
        "{\"numara\": 72, \"baslik\": \"a\", \"gorev\": \"b\", \"harita\": [\". . . . . .\", \". . . . . .\", \". . . . . .\", \". . . . . .\", \". . R . . .\", \". . . . . .\"], "
        + "\"ekran\": {\"fonksiyon\": \"iki_kat\", \"satirlar\": [{\"girdi\": 3, \"cevap\": 6, \"gorunur\": true}, {\"girdi\": 7, \"cevap\": 14}, {\"girdi\": 0, \"cevap\": 0}]}, "
        + "\"komutlar\": [], \"konular\": [\"return\"], \"ipuclari\": [\"x\"], \"cozum\": [\"def iki_kat(sayi):\", \"    return sayi * 2\"]" + extra + "}";
}
