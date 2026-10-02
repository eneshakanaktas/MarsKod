using System;
using System.Collections.Generic;
using MarsKod.Dunya;
using UnityEngine;
using UnityEngine.UIElements;

// Mini sinav ekrani (her 5 bolumden sonra, zorunlu; tasarim belgesi §6): soru + secenekler.
// Yanlis secim kirmizi yanip soner, tekrar denenebilir. Dogru secilince aciklama + "Devam" gorunur.
// Son soru da dogru cevaplaninca "Bitir" ile onPassed cagrilir. Kapatma dugmesi yoktur: zorunlu.
public class QuizView : VisualElement
{
    readonly Font fMed, fSemi, fBold;
    readonly Color ink, accent, buttonBg, hairline, errorRed, cellFill;

    readonly Label progress, promptLabel, explainLabel;
    readonly VisualElement choicesCol;
    readonly Label continueText;
    readonly VisualElement continueBtn;

    Quiz quiz;
    int index;
    bool answeredCorrectly;
    Action onPassed;

    public QuizView(Font fMed, Font fSemi, Font fBold, Color ink, Color accent, Color buttonBg, Color hairline, Color errorRed, Color cellFill)
    {
        this.fMed = fMed; this.fSemi = fSemi; this.fBold = fBold;
        this.ink = ink; this.accent = accent; this.buttonBg = buttonBg; this.hairline = hairline;
        this.errorRed = errorRed; this.cellFill = cellFill;

        pickingMode = PickingMode.Position;
        style.position = Position.Absolute;
        style.left = 0; style.right = 0; style.top = 0; style.bottom = 0;
        style.backgroundColor = new Color(0.055f, 0.05f, 0.07f, 1f);
        style.display = DisplayStyle.None;
        RegisterCallback<PointerDownEvent>(e => e.StopPropagation());

        var top = new VisualElement { pickingMode = PickingMode.Ignore };
        top.style.paddingLeft = 52; top.style.paddingRight = 52;
        top.style.paddingTop = Ui.SafeTop() + 44; top.style.paddingBottom = 8;
        top.Add(Ui.Text("Mini sınav", fBold, 56, ink));
        progress = Ui.Text("", fSemi, 30, new Color(1f, 1f, 1f, 0.5f));
        progress.style.marginTop = 8;
        top.Add(progress);
        Add(top);

        var scroll = new ScrollView(ScrollViewMode.Vertical) { pickingMode = PickingMode.Position };
        scroll.style.flexGrow = 1;
        scroll.style.paddingLeft = 52; scroll.style.paddingRight = 52;
        scroll.verticalScrollerVisibility = ScrollerVisibility.Hidden;
        scroll.horizontalScrollerVisibility = ScrollerVisibility.Hidden;

        promptLabel = Ui.Text("", fBold, 44, ink);
        promptLabel.style.whiteSpace = WhiteSpace.Normal;
        promptLabel.style.marginTop = 28; promptLabel.style.marginBottom = 32;
        scroll.Add(promptLabel);

        choicesCol = new VisualElement { pickingMode = PickingMode.Ignore };
        scroll.Add(choicesCol);

        explainLabel = Ui.Text("", fMed, 32, new Color(1f, 1f, 1f, 0.85f));
        explainLabel.style.whiteSpace = WhiteSpace.Normal;
        explainLabel.style.marginTop = 28;
        explainLabel.style.display = DisplayStyle.None;
        scroll.Add(explainLabel);
        scroll.Add(new VisualElement { style = { height = 220 } }); // altta Devam dugmesinin arkasinda kalmasin
        Add(scroll);

        var bottom = new VisualElement { pickingMode = PickingMode.Ignore };
        bottom.style.position = Position.Absolute;
        bottom.style.left = 44; bottom.style.right = 44; bottom.style.bottom = 36 + Ui.SafeBottom();
        continueText = Ui.Text("Devam", fSemi, 42, Color.white);
        continueBtn = ContinueButton(continueText);
        continueBtn.style.display = DisplayStyle.None;
        bottom.Add(continueBtn);
        Add(bottom);
    }

    public bool Open => style.display == DisplayStyle.Flex;

    // Deneme icin: n. secenegin (0'dan baslar) ya da Devam/Bitir dugmesinin paneldeki ortasi; yoksa null.
    public Vector2? ChoiceCenter(int i) => Open && i >= 0 && i < choicesCol.childCount ? choicesCol[i].worldBound.center : (Vector2?)null;
    public Vector2? ContinueCenter() => Open && continueBtn.style.display == DisplayStyle.Flex ? continueBtn.worldBound.center : (Vector2?)null;

    public void Show(Quiz quiz, Action onPassed)
    {
        this.quiz = quiz;
        this.onPassed = onPassed;
        index = 0;
        style.display = DisplayStyle.Flex;
        ShowQuestion();
    }

    void ShowQuestion()
    {
        var q = quiz.Questions[index];
        progress.text = "Soru " + (index + 1) + "/" + quiz.Questions.Count;
        promptLabel.text = q.Prompt;
        explainLabel.style.display = DisplayStyle.None;
        continueBtn.style.display = DisplayStyle.None;
        answeredCorrectly = false;

        choicesCol.Clear();
        for (int i = 0; i < q.Choices.Count; i++)
            choicesCol.Add(ChoiceButton(q, i));
    }

    VisualElement ChoiceButton(QuizQuestion q, int i)
    {
        var b = new VisualElement { pickingMode = PickingMode.Position };
        b.style.backgroundColor = buttonBg;
        Ui.Radius(b, 28);
        Ui.Border(b, 2, hairline);
        b.style.paddingTop = 24; b.style.paddingBottom = 24; b.style.paddingLeft = 32; b.style.paddingRight = 32;
        b.style.marginBottom = 18;
        var label = Ui.Text(q.Choices[i], fMed, 34, ink);
        label.style.whiteSpace = WhiteSpace.Normal;
        b.Add(label);
        b.RegisterCallback<ClickEvent>(_ =>
        {
            if (answeredCorrectly) return;
            if (i == q.Correct)
            {
                answeredCorrectly = true;
                b.style.backgroundColor = cellFill;
                label.style.color = Color.black;
                explainLabel.text = q.Explain;
                explainLabel.style.display = DisplayStyle.Flex;
                bool last = index == quiz.Questions.Count - 1;
                continueText.text = last ? "Bitir" : "Devam";
                continueBtn.style.display = DisplayStyle.Flex;
            }
            else
            {
                b.style.backgroundColor = errorRed;
                b.schedule.Execute(() => b.style.backgroundColor = buttonBg).StartingIn(400);
            }
        });
        return b;
    }

    VisualElement ContinueButton(Label text)
    {
        var b = new VisualElement { pickingMode = PickingMode.Position };
        b.style.height = 116;
        b.style.backgroundColor = accent;
        Ui.Radius(b, 58);
        b.style.alignItems = Align.Center;
        b.style.justifyContent = Justify.Center;
        b.Add(text);
        b.RegisterCallback<ClickEvent>(_ =>
        {
            if (index < quiz.Questions.Count - 1) { index++; ShowQuestion(); }
            else { style.display = DisplayStyle.None; onPassed?.Invoke(); }
        });
        return b;
    }
}
