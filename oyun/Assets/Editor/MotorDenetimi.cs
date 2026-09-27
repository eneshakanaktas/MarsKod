using System;
using System.IO;
using MarsKod.Motor;
using UnityEditor;
using UnityEngine;

// Python motorunu Unity'nin kendi calisma ortaminda denetler: tests/python-cases icindeki her ornegi
// calistirip gercek Python'un sonucuyla karsilastirir. (Bilgisayardaki `dotnet test` ile ayni ornekler;
// burada amac Unity'nin ortaminin da ayni sonucu verdigini gormek.)
// Menu: MarsKod > Motor denetimi
// Komut satiri: Unity.exe -batchmode -quit -projectPath . -executeMethod MotorDenetimi.Calistir -logFile denetim.log
public static class MotorDenetimi
{
    [Serializable]
    class Referans
    {
        public string python;
        public string output;
        public Hata error;
    }

    [Serializable]
    class Hata
    {
        public string type;
        public string message;
        public int line;
    }

    [MenuItem("MarsKod/Motor denetimi")]
    public static void Calistir()
    {
        string klasor = Path.GetFullPath(Path.Combine(Application.dataPath, "..", "..", "tests", "python-cases"));
        int gecen = 0, kalan = 0;
        foreach (string py in Directory.GetFiles(klasor, "*.py", SearchOption.AllDirectories))
        {
            string ad = Path.GetFileName(Path.GetDirectoryName(py)) + "/" + Path.GetFileNameWithoutExtension(py);
            var beklenen = JsonUtility.FromJson<Referans>(File.ReadAllText(Path.ChangeExtension(py, ".json")));
            // JsonUtility bos hatayi da nesne olarak kurar; turu bossa hata yok demektir
            string beklenenHata = beklenen.error != null && !string.IsNullOrEmpty(beklenen.error.type)
                ? beklenen.error.type + ": " + beklenen.error.message + " @" + beklenen.error.line
                : null;

            var sonuc = PythonEngine.RunPython(File.ReadAllText(py).Replace("\r\n", "\n"));
            string hata = sonuc.Error != null ? sonuc.Error.Type + ": " + sonuc.Error.Message + " @" + sonuc.Error.Line : null;

            if (sonuc.Halt == null && hata == beklenenHata && sonuc.Output == beklenen.output)
            {
                gecen++;
                continue;
            }
            kalan++;
            Debug.LogError("MOTOR DENETIMI FARKLI: " + ad
                + "\n  beklenen hata: " + (beklenenHata ?? "-") + "\n  motorun hatasi: " + (hata ?? "-")
                + "\n  durma: " + (sonuc.Halt != null ? sonuc.Halt.ToString() : "-")
                + "\n  beklenen cikti: " + beklenen.output + "\n  motorun ciktisi: " + sonuc.Output);
        }

        string ozet = "MOTOR DENETIMI: " + gecen + " ornek gecti, " + kalan + " ornek farkli.";
        if (kalan == 0) Debug.Log(ozet);
        else Debug.LogError(ozet);
        if (Application.isBatchMode) EditorApplication.Exit(kalan == 0 ? 0 : 1);
    }
}
