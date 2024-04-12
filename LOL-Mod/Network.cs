using HarmonyLib;
using Steamworks;

namespace LOL;

[HarmonyPatch]
public class Network
{
    public static void SendPacketToPlayer(Player player, P2PPackageHandler.MsgType msgType, byte[] packet)
    {
        // Logger.LogInfo($"Sending {packet?.Length} bytes to player {player.GetPlayerName()}({player.GetSteamId()})");
        P2PPackageHandler.Instance.SendP2PPacketToUser(player.GetSteamId(), packet, msgType,
            channel: GetChannelForMsgType(P2PPackageHandler.Instance, msgType));
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
                if (GameManager.Instance.mMultiplayerManager.HasBeenInitializedFromServer)
                {
                    Logger.LogWarning(
                        $"Player {new Player(steamIdRemote).GetPlayerName()} ({steamIdRemote}) tried to kick you via ClientInit");
                    return false;
                }

                break;
            case P2PPackageHandler.MsgType.WorkshopMapsLoaded:
                if (SteamFriends.HasFriend(steamIdRemote, EFriendFlags.k_EFriendFlagImmediate))
                    break;
                Logger.LogWarning(
                    $"Player {new Player(steamIdRemote).GetPlayerName()} ({steamIdRemote}) tried to kick you via WorkshopMapsLoaded");
                return false;
        }

        return true;
    }

    [HarmonyPatch(typeof(P2PPackageHandler), "GetChannelForMsgType")]
    [HarmonyReversePatch]
    private static int GetChannelForMsgType(object instance, P2PPackageHandler.MsgType msgType) => 0;

    [HarmonyPatch(typeof(MultiplayerManager), "SendMessageToAllClients")]
    [HarmonyReversePatch]
    public static void SendMessageToAllClients(
        object instance,
        byte[] data,
        P2PPackageHandler.MsgType type,
        bool ignoreServer = false,
        ulong ignoreUserID = 0,
        EP2PSend sendMethod = EP2PSend.k_EP2PSendReliable,
        int channel = 0)
    {
    }

    [HarmonyPatch(typeof(MultiplayerManager), "OnPlayerTalked")]
    [HarmonyPrefix]
    //byte[] data, int channel, ushort id
    private static bool OnPlayerTalked(ref byte[] data, ref int channel, ref ushort id)
    {
        Logger.LogInfo($"Message: {System.Text.Encoding.UTF8.GetString(data)} Channel: {channel} ID: {id}");
        return true;
    }

    [HarmonyPatch(typeof(P2PPackageHandler), "SendP2PPacketToUser",
        argumentTypes: new[]
            { typeof(CSteamID), typeof(byte[]), typeof(P2PPackageHandler.MsgType), typeof(EP2PSend), typeof(int) })]
    [HarmonyPrefix]
    private static bool SendP2PPacketToUser(ref CSteamID clientID, ref byte[] data,
        ref P2PPackageHandler.MsgType messageType, ref EP2PSend sendMethod, ref int channel)
    {
        Logger.LogInfo($"Sending {data.Length} bytes to {clientID}) Channel: {channel} Type: {messageType}");
        return true;
    }
}