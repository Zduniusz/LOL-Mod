using UnityEngine;

namespace LOL.GUI.Views;

[View(typeof(TextLogView), "Text Log", 450, 250)]
public class TextLogView : IView
{
    public static string longString;

    private Vector2 scrollPosition;
    public bool IsVisible { get; set; }

    public void Render()
    {
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        GUILayout.Label(longString);
        GUILayout.EndScrollView();
    }

    public void Toggle()
    {
    }
}