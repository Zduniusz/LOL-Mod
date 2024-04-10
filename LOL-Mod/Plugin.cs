using BepInEx;
using HarmonyLib;

namespace LOL;

[HarmonyPatch]
[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
public class Plugin : BaseUnityPlugin
{
    public const string PLUGIN_GUID = "LOL_Mod";
    public const string PLUGIN_NAME = "Zduniusz's LOL-Mod";
    public const string PLUGIN_VERSION = "1.0.0";


    private void Awake()
    {
        LOL.Logger.ExternalLogger = Logger;
        var instance = new Harmony(PLUGIN_GUID);
        instance.PatchAll();
    }

    [HarmonyPatch(typeof(GameManager), "InitAnalytics")]
    [HarmonyPrefix]
    public static bool DisableAnalytics()
    {
        return false;
    }
}