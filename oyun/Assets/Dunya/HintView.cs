// İpucu balonunun ampul döngüsü: ampule basıldıkça 1. → 2. → 3. ipucu, son basışta kapanır; tekrar açılınca yine 1.'den başlar.
// Görünüm burada tutulur (sıfırlanır); "bölümde en çok kaçıncı ipucuna gidildi" kaydı HintLog'da kalır.

namespace MarsKod.Dunya
{
    public sealed class HintView
    {
        /// <summary>Şu an görünen ipucunun sırası (1'den başlar); 0 = balon kapalı.</summary>
        public int Current { get; private set; }

        public bool Open => Current > 0;

        /// <summary>Ampule basıldı; available kadar ipucu var. Hiç ipucu yoksa balon kapalı kalır.</summary>
        public void Press(int available)
        {
            if (available <= 0) { Current = 0; return; }
            Current = Current == 0 ? 1 : Current < available ? Current + 1 : 0;
        }

        public void Close() => Current = 0;
    }
}
