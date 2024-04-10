using UnityEngine;

namespace LOL.GUI;

public class Constants
{
    public static readonly Vector2 MAX_VEC = new(Screen.width, Screen.height);
    public static readonly Vector2 CENTER_VEC = new(Screen.width / 2, Screen.height / 2);
    public static readonly Vector2 ZERO_VEC = new(0, 0);

    public static readonly Rect MAX_RECT = new(0, 0, Screen.width, Screen.height);
}