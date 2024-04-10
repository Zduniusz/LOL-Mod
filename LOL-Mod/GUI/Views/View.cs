using System;
using UnityEngine;

namespace LOL.GUI.Views;

public enum ViewType
{
    NormalWindow,
    TransparentWindow
}

[AttributeUsage(AttributeTargets.Class)]
public class ViewAttribute : Attribute
{
    private readonly bool _draggable;
    private readonly Vector2 _size;

    public ViewAttribute(Type handler, string name, uint size_x, uint size_y, bool draggable = true)
    {
        Name = name;
        HandlerType = handler;
        _draggable = draggable;
        _size = new Vector2(size_x, size_y);

        Id = Manager.CurrentWindowId;

        WindowRect = new Rect(Constants.CENTER_VEC - _size / 2, _size);
        Debug.Log($"View {name} created!");
    }

    public long Id { get; }
    public string Name { get; }
    public IView Handler { private get; set; }
    public Type HandlerType { get; }
    private Rect WindowRect { get; set; }

    public void Render()
    {
        if (Handler.IsVisible) WindowRect = GUILayout.Window((int)Id, WindowRect, RenderProxy, Name);
    }

    private void RenderProxy(int id)
    {
        Handler.Render();
        if (_draggable) UnityEngine.GUI.DragWindow(Constants.MAX_RECT);
    }

    public void Toggle()
    {
        Handler.IsVisible = !Handler.IsVisible;
        Handler.Toggle();
    }
}

class Option : Attribute
{
    public readonly string Name;

    public Option(string name)
    {
        Name = name;
    }
}

public interface IView
{
    bool IsVisible { get; set; }
    void Render();
    void Toggle();
}