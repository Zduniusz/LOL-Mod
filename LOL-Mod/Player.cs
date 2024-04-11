using System;
using System.Linq;
using HarmonyLib;
using Steamworks;
using UnityEngine;

namespace LOL;

[HarmonyPatch]
public class Player
{
    // int, string, string
    public enum PlayerColor
    {
        Yellow,
        Blue,
        Red,
        Green
    }

    private static NetworkPlayer _localPlayer;
    private readonly ConnectedClientData _connectedClient;
    private readonly NetworkPlayer _networkPlayer;
    private readonly PlayerColor _playerColor;


    public Player(CSteamID steamId)
    {
        _playerColor = (PlayerColor)Array.FindIndex(Clients, client => client != null && client.ClientID == steamId);
        _connectedClient = Clients[(int)_playerColor];
        _networkPlayer = _connectedClient.PlayerObject.GetComponent<NetworkPlayer>();
    }

    public Player(int playerIndex)
    {
        _playerColor = (PlayerColor)playerIndex;
        _connectedClient = Clients[playerIndex];
        _networkPlayer = _connectedClient.PlayerObject.GetComponent<NetworkPlayer>();
    }

    public static ConnectedClientData[] Clients { get; private set; }

    public string GetPlayerName()
    {
        return _connectedClient.PlayerName;
    }

    public string GetPlayerDescription()
    {
        return $"{GetPlayerName()} ({GetPlayerColor().ToString()})";
    }

    public CSteamID GetSteamId()
    {
        return _connectedClient.ClientID;
    }

    public void Kill()
    {
        _networkPlayer.UnitWasDamaged(0, true);
    }

    public PlayerColor GetPlayerColor()
    {
        return _playerColor;
    }

    public bool IsHost()
    {
        return MatchmakingHandler.Instance.LobbyOwner == GetSteamId();
    }

    [HarmonyPatch(typeof(ChatManager), "Start")]
    [HarmonyPostfix]
    private static void Start(ref NetworkPlayer ___m_NetworkPlayer)
    {
        if (!___m_NetworkPlayer.HasLocalControl)
            return;
        var chatManager = ___m_NetworkPlayer.GetComponentInChildren<ChatManager>();
        chatManager.Talk("Press Q to show the menu!");
    }

    [HarmonyPatch(typeof(ChatManager), "Start")]
    [HarmonyPostfix]
    private static void Start()
    {
        Clients = GameManager.Instance.mMultiplayerManager.ConnectedClients;
        _localPlayer = Clients.First(client => IsClientValid(client) && client.ControlledLocally).PlayerObject
            .GetComponent<NetworkPlayer>();
        if (Clients == null)
            throw new NullReferenceException("Players or Clients are null!");
        Debug.Log("Players and Clients hooked!");
    }

    public static bool IsClientValid(ConnectedClientData clientData)
    {
        return clientData != null && clientData.ClientID.IsValid() && clientData.Spawned;
    }
}