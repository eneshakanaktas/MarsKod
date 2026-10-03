using System;
using UnityEngine;
using UnityEngine.UIElements;

// Acilis sahnesi (Bolum 1 oncesi, atlanabilir, sozsuz): docs/tasarim/senaryo-bolge-01.md.
// Yazilar sirayla belirir; robot bulundugunda tek kisik bip calar. Ekrana dokununca hemen biter.
public class OpeningScene : VisualElement
{
    static readonly string[] Lines = { "Mars Kod Okulu", "Bağlanılıyor…", "Robot bulundu: BKM-7", "Ödev 1 hazır" };
    const int RobotLine = 2;
    const long LineMs = 1300;

    readonly Label label;
    string[] lines = Lines;
    int blipLine = RobotLine;
    Action onDone;
    bool finished;

    public OpeningScene(Font font)
    {
        pickingMode = PickingMode.Position;
        style.position = Position.Absolute;
        style.left = 0; style.right = 0; style.top = 0; style.bottom = 0;
        style.backgroundColor = Color.black;
        style.alignItems = Align.Center;
        style.justifyContent = Justify.Center;
        style.display = DisplayStyle.None;

        label = Ui.Text("", font, 46, new Color(1f, 1f, 1f, 0.92f));
        label.style.letterSpacing = 2;
        Add(label);

        RegisterCallback<ClickEvent>(_ => Finish());
    }

    public void Show(Action done) => Show(Lines, RobotLine, done);

    // Bolge gecisi gibi baska kisa yazi dizileri icin (bip yok: blipLine -1)
    public void Show(string[] texts, int blipAt, Action done)
    {
        lines = texts;
        blipLine = blipAt;
        onDone = done;
        finished = false;
        label.text = "";
        style.display = DisplayStyle.Flex;
        schedule.Execute(() => ShowLine(0)).StartingIn(300);
    }

    void ShowLine(int i)
    {
        if (finished) return;
        if (i >= lines.Length) { Finish(); return; }
        label.text = lines[i];
        if (i == blipLine) Sound.Blip();
        schedule.Execute(() => ShowLine(i + 1)).StartingIn(LineMs);
    }

    void Finish()
    {
        if (finished) return;
        finished = true;
        style.display = DisplayStyle.None;
        onDone?.Invoke();
        onDone = null;
    }
}
