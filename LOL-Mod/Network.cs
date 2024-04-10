using HarmonyLib;
using Steamworks;

namespace LOL;

[HarmonyPatch]
public class Network
{
    public static void SendPacketToPlayer(Player player, P2PPackageHandler.MsgType msgType, byte[] packet)
    {
        // Logger.LogInfo($"Sending {packet.Length} bytes to player {player.GetPlayerName()}({player.GetSteamId()})");
        P2PPackageHandler.Instance.SendP2PPacketToUser(player.GetSteamId(), packet, msgType);
    }

    [HarmonyPatch(typeof(P2PPackageHandler), "CheckMessageType")]
    [HarmonyPrefix]
    private static bool CheckForMaliciousPacket(ref CSteamID steamIdRemote, P2PPackageHandler.MsgType type)
    {
        switch (type)
        {
            case P2PPackageHandler.MsgType.KickPlayer:
                Logger.LogWarning(
                    $"Player {new Player(steamIdRemote).GetPlayerName()} ({steamIdRemote}) tried to kick you via KickPlayer");
                return false;
            case P2PPackageHandler.MsgType.ClientInit:
                if (!new Player(steamIdRemote).IsHost() ||
                    GameManager.Instance.mMultiplayerManager.HasBeenInitializedFromServer)
                {
                    Logger.LogWarning(
                        $"Player {new Player(steamIdRemote).GetPlayerName()} ({steamIdRemote}) tried to kick you via ClientInit");
                    return false;
                }

                break;
        }

        return true;
    }
}