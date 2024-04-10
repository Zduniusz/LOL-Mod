using System;
using HarmonyLib;
using UnityEngine;

namespace LOL.GUI.Views;

[HarmonyPatch]
[View(typeof(FightingView), "Fighting Hacks", 150, 50)]
public class FightingView : IView
{
    [Option("Infinite Ammo")] public static bool Infinite_Ammo;
    [Option("Fast Punch")] public static bool Fast_Punch;
    [Option("Super fire rate")] public static bool Super_Fire_Rate;
    [Option("No Recoil")] public static bool No_Recoil;

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

        var methods = GetType().GetMethods();
        foreach (var method in methods)
        {
            var cheatOption = (Option)Attribute.GetCustomAttribute(method, typeof(Option));
            if (cheatOption == null) continue;

            if (GUILayout.Button(cheatOption.Name))
                method.Invoke(null, null);
        }
    }

    public void Toggle()
    {
    }

    [Option("Kill Player")]
    public static void KillPlayer()
    {
        SelectPlayerModal modal = new();
        modal.ModalClosed += (sender, args) =>
        {
            var player = new SelectPlayerModal.SelectPlayerEventArgs(args).SelectedPlayer;
            if (player == null) return;
            player.Kill();
        };
        modal.Spawn();
    }

    [HarmonyPatch(typeof(Fighting), "Attack")]
    [HarmonyPostfix]
    private static void Attack(ref int ___bulletsLeft, ref float ___punchCD, ref float ___counter, ref Weapon ___weapon)
    {
        if (Infinite_Ammo)
            ___bulletsLeft++;
        if ((Fast_Punch && ___weapon == null) || (Super_Fire_Rate && ___weapon != null))
        {
            ___punchCD = 10f;
            ___counter = 10f;
        }
    }

    [HarmonyPatch(typeof(Weapon), "ActuallyShoot")]
    [HarmonyPrefix]
    private static void
        ActuallyShoot( /*ref bool networkForce, ref Vector3 shootVectorOverride, ref Vector3 shootPositionOverride,*/
            /*ref Transform ___shootPosition, ref Transform ___transform, */ ref float ___spread, ref float ___recoil)
    {
        if (No_Recoil)
        {
            // networkForce = true;
            // var forward = ___transform.forward;
            // shootVectorOverride = new Vector3(0, forward.z, forward.y);
            // shootPositionOverride = ___shootPosition.position;
            ___spread = 0;
            ___recoil = 0;
        }
    }
}