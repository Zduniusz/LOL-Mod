using UnityEngine;

namespace LOL.GUI.Modals;

public class SelectStringModal : Modal
{
    private readonly Modal _modal;
    private readonly string[] _options;

    public SelectStringModal(string[] options) : base(new Vector2(200, 200), "Select Player")
    {
        _options = options;
        _modal = this;
    }

    protected override void RenderProxy(int id)
    {
        for (var index = 0; index < _options.Length; index++)
        {
            var option = _options[index];
            if (GUILayout.Button(option)) _modal.Close(index);
        }
    }
}