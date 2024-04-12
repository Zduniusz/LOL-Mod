using System;
using UnityEngine;

namespace LOL.GUI.Modals;

public abstract class Modal
{
    private readonly string _name;

    private Rect _windowRect;

    protected Modal(Vector2 size, string name)
    {
        _windowRect = new Rect(Constants.CENTER_VEC - size / 2, size);
        _name = name;
    }

    public event EventHandler<ModalClosedEventArgs> ModalClosed;

    protected abstract void RenderProxy(int id);

    public void Render()
    {
        _windowRect = UnityEngine.GUI.ModalWindow(-1, _windowRect, delegate(int i)
        {
            RenderProxy(i);
            UnityEngine.GUI.DragWindow(Constants.MAX_RECT);
        }, _name);
    }

    public void Spawn()
    {
        if (Manager.CurrentModal != null) return;
        Manager.CurrentModal = this;
    }

    protected internal void Close(object returnValue)
    {
        Manager.CurrentModal = null;
        ModalClosed?.Invoke(this, new ModalClosedEventArgs(returnValue));
    }

    public class ModalClosedEventArgs : EventArgs
    {
        public ModalClosedEventArgs(object returnValue)
        {
            ReturnValue = returnValue;
        }

        public object ReturnValue { get; }
    }
}