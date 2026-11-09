// Bilgi kartı: bir bölümde yeni bir şey ilk kez göründüğünde oyuncuya açılan kısa açıklama. Oyun, bölüme ilk
// girişte kartı kendisi açar; oyuncu "Anladım" der, sonra sol üstteki i düğmesiyle istediği zaman tekrar açar.
// Bölüm dosyasında: "bilgi": { "baslik": "...", "metin": ["paragraf", ...] }

using System.Collections.Generic;
using System.Linq;
using static MarsKod.Dunya.DataFields;

namespace MarsKod.Dunya
{
    public sealed class LevelInfo
    {
        public const int MaxParagraphs = 4;

        public readonly string Title;
        /// <summary>Paragraflar; `kod` parçaları kod renginde gösterilir</summary>
        public readonly IReadOnlyList<string> Paragraphs;

        LevelInfo(string title, List<string> paragraphs)
        {
            Title = title;
            Paragraphs = paragraphs;
        }

        /// <summary>Bölüm dosyasındaki "bilgi" alanının değerini okur; yanlışta DataFormatError verir.</summary>
        public static LevelInfo Parse(object value)
        {
            if (!(value is Dictionary<string, object> d))
                throw new DataFormatError("\"bilgi\" bir nesne olmalı: { \"baslik\": \"...\", \"metin\": [\"...\"] }.");
            CheckKeys(d, new[] { "baslik", "metin" }, "\"bilgi\"");
            string title = Text(d, "baslik");
            var paragraphs = TextList(d, "metin");
            if (paragraphs.Count < 1 || paragraphs.Count > MaxParagraphs)
                throw new DataFormatError("\"bilgi\" içindeki \"metin\" 1-" + MaxParagraphs + " paragraf içermeli (şimdi " + paragraphs.Count + ").");
            if (paragraphs.Any(p => p.Trim().Length == 0))
                throw new DataFormatError("\"bilgi\" içindeki \"metin\" paragrafları boş olamaz.");
            return new LevelInfo(title, paragraphs);
        }
    }
}
