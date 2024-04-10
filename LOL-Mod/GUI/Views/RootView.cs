using System.Linq;
using UnityEngine;

namespace LOL.GUI.Views;

[View(typeof(RootView), "Main Menu", 300, 600)]
public class RootView : IView
{
    public RootView()
    {
        Manager.RootView = this;
    }

    public bool IsVisible { get; set; }

    public void Render()
    {
        for (var i = 0; i < Manager.Views.Count; i++)
        {
            var view = Manager.Views.ElementAt(i);
            if (view.Value.HandlerType == typeof(RootView)) continue;

            if (GUILayout.Button(view.Value.Name))
                view.Value.Toggle();
        }
    }

    void IView.Toggle()
    {
        IsVisible = !IsVisible;
    }
}