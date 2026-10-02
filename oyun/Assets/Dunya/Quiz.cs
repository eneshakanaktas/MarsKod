// Mini sınav dosyası: her 5 bölümde bir, o ana kadarki konuları özetleyen soru-cevap (tasarım belgesi §6).
// Dosyalar oyun/Assets/Resources/Sinavlar/ içinde JSON'dur: sinav-05.json, sinav-10.json...
// Kod bilmeyen ekip üyeleri de düzenleyebilir; yanlış yazılırsa dotnet test Türkçe söyler.

using System.Collections.Generic;
using System.Linq;
using static MarsKod.Dunya.DataFields;

namespace MarsKod.Dunya
{
    public sealed class QuizQuestion
    {
        public string Prompt;
        public List<string> Choices = new List<string>();
        /// <summary>Doğru seçeneğin sırası (0'dan başlar)</summary>
        public int Correct;
        /// <summary>Cevapladıktan sonra gösterilen kısa açıklama (doğru ya da yanlış, ikisinde de)</summary>
        public string Explain;
    }

    public sealed class Quiz
    {
        /// <summary>Bu sınav hangi bölüm bitince çıkar (örn. 5, 10, 15, 20)</summary>
        public int AfterLevel;
        public List<QuizQuestion> Questions = new List<QuizQuestion>();

        public static Quiz Parse(string json)
        {
            var known = new HashSet<string> { "bolum", "sorular" };
            var d = ParseObject(json, known);
            var quiz = new Quiz { AfterLevel = Int(d, "bolum") };
            if (quiz.AfterLevel < 1) throw new DataFormatError("\"bolum\" 1 ya da daha büyük olmalı.");
            quiz.Questions = ReadQuestions(Field(d, "sorular"));
            if (quiz.Questions.Count == 0) throw new DataFormatError("\"sorular\" boş olamaz; en az bir soru yaz.");
            return quiz;
        }

        static readonly string[] QuestionFields = { "soru", "secenekler", "dogru", "aciklama" };

        static List<QuizQuestion> ReadQuestions(object v)
        {
            const string shape = "\"sorular\" bir liste olmalı; her öğe { \"soru\": \"...\", \"secenekler\": [...], \"dogru\": 0, \"aciklama\": \"...\" } biçiminde.";
            if (!(v is List<object> list)) throw new DataFormatError(shape);
            var result = new List<QuizQuestion>();
            for (int i = 0; i < list.Count; i++)
            {
                if (!(list[i] is Dictionary<string, object> q))
                    throw new DataFormatError(shape + " (" + (i + 1) + ". öğe uymuyor)");
                CheckKeys(q, QuestionFields, (i + 1) + ". soru");
                var question = new QuizQuestion
                {
                    Prompt = Text(q, "soru"),
                    Choices = TextList(q, "secenekler"),
                    Correct = Int(q, "dogru"),
                    Explain = Text(q, "aciklama"),
                };
                if (question.Choices.Count < 2)
                    throw new DataFormatError((i + 1) + ". sorunun \"secenekler\" alanında en az 2 seçenek olmalı.");
                if (question.Correct < 0 || question.Correct >= question.Choices.Count)
                    throw new DataFormatError((i + 1) + ". sorunun \"dogru\" alanı seçeneklerden birinin sırası olmalı (0'dan " + (question.Choices.Count - 1) + "'e kadar).");
                result.Add(question);
            }
            return result;
        }
    }

    /// <summary>Sınav denetleyici: "bolum" gerçek bir bölüm numarasına denk gelmeli.</summary>
    public static class QuizCheck
    {
        public static List<string> Problems(Quiz quiz, IList<Level> levels)
        {
            var problems = new List<string>();
            if (levels.Count > 0 && !levels.Any(l => l.Number == quiz.AfterLevel))
                problems.Add("\"bolum\" (" + quiz.AfterLevel + ") hiçbir bölüm dosyasına denk gelmiyor.");
            return problems;
        }
    }
}
