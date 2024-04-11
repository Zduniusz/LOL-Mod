using HarmonyLib;
using UnityEngine;

namespace LOL.GUI.Views;

[HarmonyPatch]
[ViewAttribute(typeof(KickView), "Kick Players", 400, 50)]
public class KickView : IView
{
    private static readonly string[] KickMethods =
    {
        "Normal Kick",
        "Client Init Kick",
        "Workshop Corruption Kick"
    };

    private int _selectedKickMethod;
    public bool IsVisible { get; set; }

    public void Render()
    {
        var clients = Player.Clients;

        if (GameManager.Instance.mMultiplayerManager.GetPlayersInLobby(true) <= 0)
        {
            GUILayout.Label("No players connected.");
            return;
        }

        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();

        foreach (var client in clients)
        {
            if (!Player.IsClientValid(client)) continue;
            var player = new Player(client.ClientID);
            UnityEngine.GUI.enabled = !(player.IsHost() && _selectedKickMethod == 0);
            if (GUILayout.Button(player.GetPlayerDescription()))
                switch (_selectedKickMethod)
                {
                    case 0:
                        KickPlayer(player);
                        break;
                    case 1:
                        ClientInitKick(player);
                        break;
                    case 2:
                        WorkshopCorruptionKick(player);
                        break;
                }

            UnityEngine.GUI.enabled = true;
        }


        GUILayout.EndVertical();

        GUILayout.BeginVertical();
        _selectedKickMethod = GUILayout.SelectionGrid(_selectedKickMethod, KickMethods, 1);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    public void Toggle()
    {
    }

    private static void KickPlayer(Player player) =>
        Network.SendPacketToPlayer(player, P2PPackageHandler.MsgType.KickPlayer, new byte[] { 0x00 });

    private static void ClientInitKick(Player player) =>
        Network.SendPacketToPlayer(player, P2PPackageHandler.MsgType.ClientInit, new byte[] { 0x00 });

    private static void WorkshopCorruptionKick(Player player) =>
        Network.SendPacketToPlayer(player, P2PPackageHandler.MsgType.WorkshopMapsLoaded,
            new byte[2 + 8] { 0x01, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF });
}