// Oyuncunun kod yazma kademesi (tasarım: docs/superpowers/specs/2026-09-28-kod-klavyesi-design.md §1, K7).
// Acemi: hazır düğmeleri sürükleyip bırakır. Orta: oyunun klavyesi + öneri satırı. Usta: klavye, öneri yok.

namespace MarsKod.Dunya
{
    public enum KeyboardTier
    {
        Acemi = 0,
        Orta = 1,
        Usta = 2,
    }
}
