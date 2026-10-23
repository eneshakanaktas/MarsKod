using System;
using UnityEngine;
using UnityEngine.UIElements;

// Atlatma katmani (Bolge 6, 51'in roket inisi): tam ekran saydam katman; dokununca bir kez onTap cagrilir ve katman kapanir.
// Acikken altindaki kod kartina ve dugmelere dokunulamaz.
public class SkipCatcher : VisualElement
{
    Action onTap;

    public bool Open => style.display == DisplayStyle.Flex;

    public SkipCatcher()
    {
        pickingMode = PickingMode.Position;
        style.position = Position.Absolute;
        style.left = 0; style.right = 0; style.top = 0; style.bottom = 0;
        style.backgroundColor = Color.clear;
        style.display = DisplayStyle.None;
        RegisterCallback<ClickEvent>(_ =>
        {
            var tap = onTap;
            Hide();
            tap?.Invoke();
        });
    }

    public void Show(Action tap)
    {
        onTap = tap;
        style.display = DisplayStyle.Flex;
    }

    public void Hide()
    {
        onTap = null;
        style.display = DisplayStyle.None;
    }
}
