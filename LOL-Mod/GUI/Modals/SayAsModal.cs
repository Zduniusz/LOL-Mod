using System;
using UnityEngine;

namespace LOL.GUI.Modals;

public class SayAsModal : Modal
{
    private readonly Modal _modal;
    private readonly Player _target;
    private bool _invisibleToTarget;
    private string _message = string.Empty;

    public SayAsModal(Player target) : base(new Vector2(170, 200), $"Say as {target.GetPlayerColor().ToString()}")
    {
        _target = target;
        _modal = this;
    }

    protected override void RenderProxy(int id)
    {
        //Add toggle "Invisible To Target"
        _invisibleToTarget = GUILayout.Toggle(_invisibleToTarget, "Invisible To Target");

        //Add text field for message
        _message = GUILayout.TextField(_message);

        //Add button to send message
        if (GUILayout.Button("Send"))
        {
            _modal.Close(new SayAsEventArgs(_message, _invisibleToTarget));
        }
    }

    public class SayAsEventArgs : EventArgs
    {
        public SayAsEventArgs(string message, bool invisibleToTarget)
        {
            Message = message;
            InvisibleToTarget = invisibleToTarget;
        }

        public string Message { get; }
        public bool InvisibleToTarget { get; }
    }
}