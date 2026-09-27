// Hataların Türkçe açıklaması ve türü (oyuncu profili hata türlerini sayar).
// Metinlerin kendisi ExplanationsTr.cs dosyasındadır; kod bilmeyen ekip üyeleri de düzenleyebilir.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MarsKod.Motor
{
    /// <summary>Hata türleri: oyuncu profili ve ders önerileri bunlara göre çalışır.</summary>
    public enum ErrorCategory
    {
        Yazim,
        Girinti,
        Kosul,
        Atama,
        Isim,
        Tur,
        Donusum,
        Indeks,
        Liste,
        Fonksiyon,
        Dongu,
        SifiraBolme,
        Ozellik,
        Desteklenmeyen,
        Sinir,
    }

    /// <summary>Anlatım tonu. İleride ayarlara "Eglenceli" eklenecek (bkz. yapım planı, Aşama 7).</summary>
    public enum Tone
    {
        Standart,
    }

    public sealed class Explanation
    {
        public ErrorCategory Category;
        /// <summary>Kısa başlık, örn. "Tanımsız isim"</summary>
        public string Title;
        /// <summary>Ne oldu, neden oldu</summary>
        public string Text;
        /// <summary>Ne yapılabilir; yoksa null</summary>
        public string Hint;
        /// <summary>Bu mesaj için özel metin yoksa true (genel açıklama verildi)</summary>
        public bool Generic;
    }

    public sealed class MatchContext
    {
        /// <summary>"Did you mean: 'x'?" önerisi</summary>
        public string Suggestion;
        /// <summary>"Did you forget to import 'x'?" ipucu</summary>
        public string ImportName;
    }

    /// <summary>Bir hata mesajı kalıbı ve Türkçe açıklaması</summary>
    public sealed class Rule
    {
        /// <summary>Python hata türleri, örn. { "SyntaxError" }</summary>
        public string[] Types;
        Regex regex;
        /// <summary>Python'un İngilizce mesajına uyan kalıp ("Did you mean" kısmı önceden ayrılır)</summary>
        public string Pattern
        {
            set => regex = new Regex(value, RegexOptions.CultureInvariant);
        }
        public Regex Regex => regex;
        public ErrorCategory Category;
        public string Title;
        public string Text;
        /// <summary>İpucu metni; yoksa null</summary>
        public string Hint;
        /// <summary>Duruma göre değişen ipucu (Hint yerine)</summary>
        public Func<string[], MatchContext, string> HintFn;
        /// <summary>Genel (yedek) kural mı</summary>
        public bool Generic;
    }

    public static class Explain
    {
        /// <summary>Şablonlardaki {1:tur} gibi süzgeçler</summary>
        static readonly Dictionary<string, Func<string, string>> Filters = new Dictionary<string, Func<string, string>>
        {
            ["tur"] = t =>
            {
                string key = t.EndsWith(" object", StringComparison.Ordinal) ? t.Substring(0, t.Length - " object".Length) : t;
                return TypeNames.TryGetValue(key, out string name) ? name : t;
            },
            ["ifade"] = e => ExprNames.TryGetValue(e, out string name) ? name : e,
            ["kapanis"] = c => c == "(" ? ")" : c == "[" ? "]" : c == "{" ? "}" : c,
            ["ad"] = name => name.Split('.').Last(),
            ["ve"] = list => AndPattern.Replace(list, " ve ", 1),
        };

        static readonly Regex AndPattern = new Regex(",? and ", RegexOptions.CultureInvariant);

        static readonly Dictionary<string, string> TypeNames = new Dictionary<string, string>
        {
            ["int"] = "tam sayı (int)",
            ["float"] = "ondalıklı sayı (float)",
            ["str"] = "metin (str)",
            ["string"] = "metin (str)",
            ["bool"] = "doğru/yanlış değeri (bool)",
            ["NoneType"] = "None (hiçbir şey)",
            ["list"] = "liste",
            ["tuple"] = "demet (tuple)",
            ["range"] = "range",
            ["function"] = "fonksiyon",
            ["builtin_function_or_method"] = "yerleşik fonksiyon",
            ["type"] = "tür",
        };

        static readonly Dictionary<string, string> ExprNames = new Dictionary<string, string>
        {
            ["literal"] = "sabit bir değer (sayı ya da metin)",
            ["expression"] = "bir işlem",
            ["function call"] = "bir fonksiyon çağrısı",
            ["comparison"] = "bir karşılaştırma",
            ["attribute"] = "bir özellik",
            ["subscript"] = "bir eleman",
            ["tuple"] = "bir demet",
            ["list"] = "bir liste",
            ["conditional expression"] = "bir koşullu ifade",
            ["dict literal"] = "bir sözlük",
            ["set display"] = "bir küme",
        };

        static readonly Regex Placeholder = new Regex(@"\{(\d)(?::(\w+))?\}", RegexOptions.CultureInvariant);
        static readonly Regex DidYouMean = new Regex(@"\. Did you mean: '(.+?)'\?", RegexOptions.CultureInvariant);
        static readonly Regex ForgotImport = new Regex(@"did you forget to import '(.+?)'\??$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        static readonly Regex ForgotImportTail = new Regex(@"(\.| Or) did you forget to import '.+?'\??$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        static string Fill(string template, string[] groups)
        {
            return Placeholder.Replace(template, m =>
            {
                int index = m.Groups[1].Value[0] - '0';
                string value = index < groups.Length ? groups[index] : "";
                return m.Groups[2].Success ? Filters[m.Groups[2].Value](value) : value;
            });
        }

        public static Explanation ExplainError(PythonError error, Tone tone = Tone.Standart)
        {
            var suggestionMatch = DidYouMean.Match(error.Message);
            var importMatch = ForgotImport.Match(error.Message);
            string baseMessage = ForgotImportTail.Replace(DidYouMean.Replace(error.Message, "", 1), "", 1);
            var ctx = new MatchContext
            {
                Suggestion = suggestionMatch.Success ? suggestionMatch.Groups[1].Value : null,
                ImportName = importMatch.Success ? importMatch.Groups[1].Value : null,
            };

            foreach (var rule in ExplanationsTr.Rules)
            {
                if (Array.IndexOf(rule.Types, error.Type) < 0) continue;
                var m = rule.Regex.Match(baseMessage);
                if (!m.Success) continue;
                var groups = new string[m.Groups.Count];
                for (int i = 0; i < groups.Length; i++) groups[i] = m.Groups[i].Value;
                string hint = rule.HintFn != null ? rule.HintFn(groups, ctx) : rule.Hint;
                return new Explanation
                {
                    Category = rule.Category,
                    Title = Fill(rule.Title, groups),
                    Text = Fill(rule.Text, groups),
                    Hint = hint == null ? null : Fill(hint, groups),
                    Generic = rule.Generic,
                };
            }
            // Rules listesinin sonunda her tür için genel kural var; buraya ancak bilinmeyen bir türle gelinir.
            throw new InvalidOperationException("Açıklama bulunamadı: " + error.Type);
        }

        public static Explanation ExplainHalt(Halt halt, Tone tone = Tone.Standart)
        {
            if (halt.Kind == "limit")
            {
                var h = ExplanationsTr.HaltTexts[halt.Reason];
                return new Explanation { Category = h.Category, Title = h.Title, Text = h.Text, Hint = h.Hint, Generic = false };
            }
            var feature = ExplanationsTr.FeatureNames[halt.Feature];
            return new Explanation
            {
                Category = ErrorCategory.Desteklenmeyen,
                Title = "Bu oyunda henüz yok",
                Text = "Bu kod gerçek Python'da çalışır, ama " + feature.Name.Replace("{detail}", halt.Detail ?? "")
                    + " bu oyunda henüz desteklenmiyor.",
                Hint = feature.Hint ?? "Aynı işi, oyunun bildiği komutlarla yapmayı dene.",
                Generic = false,
            };
        }
    }
}
