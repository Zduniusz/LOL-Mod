using System;
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
        Green,
    }

    private static NetworkPlayer _localPlayer;
    private readonly ConnectedClientData _connectedClient;
    private readonly NetworkPlayer _networkPlayer;
    private readonly PlayerColor _playerColor;


    public Player(CSteamID steamId)
    {
        _playerColor = (PlayerColor)Array.FindIndex(Clients, data => data.ClientID == steamId);
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

    public string GetPlayerName() => _connectedClient.PlayerName;
    public string GetPlayerDescription() => $"{GetPlayerName()} ({GetPlayerColor().ToString()})";
    public CSteamID GetSteamId() => _connectedClient.ClientID;
    public void Kill() => _networkPlayer.UnitWasDamaged(0, true);
    public PlayerColor GetPlayerColor() => _playerColor;
    public bool IsHost() => MatchmakingHandler.Instance.LobbyOwner == GetSteamId();

    [HarmonyPatch(typeof(ChatManager), "Start")]
    [HarmonyPostfix]
    private static void Start()
    {
        Clients = GameManager.Instance.mMultiplayerManager.ConnectedClients;
        if (Clients == null)
            throw new NullReferenceException("Players or Clients are null!");
        Debug.Log("Players and Clients hooked!");
    }

    public static bool IsClientValid(ConnectedClientData clientData) =>
        clientData != null && clientData.ClientID.IsValid() && clientData.Spawned;
}