using System.Linq;
using BepInEx.Logging;
using HarmonyLib;
using LOL.GUI;
using LOL.GUI.Views;

namespace LOL;

[HarmonyPatch]
public class Logger
{
    public static ManualLogSource ExternalLogger { private get; set; }

    public static void LogInfo(string message)
    {
        Log($"[INFO] {message}");
    }

    public static void LogWarning(string message)
    {
        Manager.Views.Values.First(attribute => attribute.HandlerType == typeof(TextLogView)).Handler.IsVisible = true;
        Log($"[WARNING] {message}");
    }

    public static void LogError(string message)
    {
        Manager.Views.Values.First(attribute => attribute.HandlerType == typeof(TextLogView)).Handler.IsVisible = true;
        Log($"[ERROR] {message}");
    }

    public static void LogChat(string author, string message)
    {
        Log($"[CHAT] {author}: {message}");
    }

    private static void Log(string fullMessage)
    {
        ExternalLogger.Log(LogLevel.All, fullMessage);
        TextLogView.longString += fullMessage + "\n";
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(ChatManager), "Talk")]
    private static void Talk(string t, ref NetworkPlayer ___m_NetworkPlayer)
    {
        LogChat(new Player(___m_NetworkPlayer.NetworkSpawnID).GetPlayerName(), t);
    }
}