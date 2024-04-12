using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using LOL.GUI.Views;
using UnityEngine;

namespace LOL.GUI;

[HarmonyPatch]
public class Manager
{
    private static long _currentWindowId;

    public static readonly Dictionary<long, ViewAttribute> Views = new();

    public static IView RootView;
    public static Modals.Modal CurrentModal;
    public static long CurrentWindowId => ++_currentWindowId;

    [HarmonyPatch(typeof(GameManager), "Awake")]
    [HarmonyPostfix]
    private static void Awake()
    {
        Views.Clear();
        foreach (var type in Assembly.GetCallingAssembly().GetTypes())
        {
            var customAttributes = type.GetCustomAttributes(typeof(ViewAttribute), false);
            if (customAttributes.Length <= 0) continue;

            var view = (ViewAttribute)customAttributes[0];
            var handler = (IView)Activator.CreateInstance(type);
            view.Handler = handler;
            Views.Add(CurrentWindowId, view);
        }
    }

    [HarmonyPatch(typeof(GameManager), "OnGUI")]
    [HarmonyPostfix]
    private static void OnGUI()
    {
        foreach (var view in Views) view.Value.Render();
        CurrentModal?.Render();
    }

    [HarmonyPatch(typeof(GameManager), "Update")]
    [HarmonyPostfix]
    private static void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Q)) return;
        if (Input.GetKey(KeyCode.LeftShift))
            foreach (var viewAttribute in Views.Values)
                viewAttribute.Handler.IsVisible = false;
        else
            RootView.Toggle();
    }

    [HarmonyPatch(typeof(Fighting), "Attack")]
    [HarmonyPrefix]
    private static bool Attack()
    {
        return !RootView.IsVisible;
    }
}