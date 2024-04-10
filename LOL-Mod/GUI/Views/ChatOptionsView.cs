using System;
using HarmonyLib;
using UnityEngine;

namespace LOL.GUI.Views;

[HarmonyPatch]
[View(typeof(ChatOptionsView), "Chat Options", 300, 200)]
public class ChatOptionsView : IView
{
    [Option("Allow bad words")] public static bool AllowBadWords = true;
    [Option("Auto GG")] public static bool AutoGG;
    [Option("Auto Hi")] public static bool AutoHi;
    public bool IsVisible { get; set; }

    public void Render()
    {
        var fields = GetType().GetFields();
        foreach (var field in fields)
        {
            var cheatOption = (Option)Attribute.GetCustomAttribute(field, typeof(Option));
            if (cheatOption == null) continue;

            if (field.FieldType == typeof(bool))
                field.SetValue(this, GUILayout.Toggle((bool)field.GetValue(this), cheatOption.Name));
        }
    }

    public void Toggle()
    {
    }

    [HarmonyPatch(typeof(DontBeRude), "CensorString")]
    [HarmonyPrefix]
    private static bool CensorString()
    {
        return !AllowBadWords;
    }

    [HarmonyPatch(typeof(GameManager), "AllButOnePlayersDied")]
    [HarmonyPostfix]
    private static void AllButOnePlayersDied()
    {
        throw new NotImplementedException("AllButOnePlayersDied is not implemented");
        // if (AutoGG)
        //     GameManager.Instance.ChatManager.SendChatMessage("gg");
    }
}