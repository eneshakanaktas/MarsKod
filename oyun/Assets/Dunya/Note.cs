// Robotun read_note() ile okuduğu not (Bölge 8'den). Yazısı bir metin ("KKDB") ya da Bölge 9'da metinlerden oluşan
// bir liste (["KKD", "DG"]). Notun bir yolu vardır (hedef yolun sonu); bölüm dosyası gerekirse yolu ve antenin
// beklediği sayıyı nota ayrıca yazar (Level.ReadNotes).

using System.Collections.Generic;
using System.Linq;
using MarsKod.Motor;

namespace MarsKod.Dunya
{
    public sealed class Note
    {
        /// <summary>Metin notunda yazı; liste notunda null</summary>
        public readonly string TextValue;
        /// <summary>Liste notunun satırları; metin notunda null</summary>
        public readonly IReadOnlyList<string> Lines;
        /// <summary>Notun yolu: robotun karesinden başlar, hedef sonudur</summary>
        public readonly Trail Route;
        /// <summary>Bu notta antenin report() ile beklediği sayı; null ise bölümün "rapor"u geçerli</summary>
        public readonly int? Report;
        /// <summary>Notun harfleri sırayla yürünen yol mu (yazısı ayrı yazılan notta değil: 84'ün okları, 87'nin haritası)</summary>
        public readonly bool LettersAreRoute;

        public Note(string text, Trail route, int? report = null, bool lettersAreRoute = true)
        {
            TextValue = text;
            Route = route;
            Report = report;
            LettersAreRoute = lettersAreRoute;
        }

        public Note(IEnumerable<string> lines, Trail route, int? report = null, bool lettersAreRoute = true)
        {
            Lines = lines.ToList();
            Route = route;
            Report = report;
            LettersAreRoute = lettersAreRoute;
        }

        public bool IsList => Lines != null;

        /// <summary>Notun ekranda görünen yazısı: metin notunda kendisi, liste notunda Python'un yazdığı gibi (['KK', 'D'])</summary>
        public string Text => IsList ? Values.Repr(PythonValue()) : TextValue;

        /// <summary>Oyuncunun koduna verilen değer; liste her okuyuşta yeni (oyuncu değiştirirse not bozulmaz)</summary>
        public object PythonValue() => IsList ? new PyList(Lines.Cast<object>().ToList()) : (object)TextValue;
    }
}
