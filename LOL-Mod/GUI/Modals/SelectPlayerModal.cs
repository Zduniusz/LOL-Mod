using System;
using UnityEngine;

namespace LOL.GUI.Modals;

public class SelectPlayerModal : Modal
{
    private readonly Modal _modal;

    public SelectPlayerModal() : base(new Vector2(170, 200), "Select Player")
    {
        _modal = this;
    }

    protected override void RenderProxy(int id)
    {
        var clients = Player.Clients;

        if (GameManager.Instance.mMultiplayerManager.GetPlayersInLobby(false) <= 0 || clients == null)
        {
            GUILayout.Label("No players connected.");
            if (GUILayout.Button("Close"))
                _modal.Close(null);
            return;
        }

        foreach (var client in clients)
        {
            if (!Player.IsClientValid(client)) continue;
            var player = new Player(client.ClientID);
            if (GUILayout.Button(player.GetPlayerDescription())) _modal.Close(player.GetPlayerColor());
        }
    }

    public class SelectPlayerEventArgs : EventArgs
    {
        public SelectPlayerEventArgs(ModalClosedEventArgs e)
        {
            if (e.ReturnValue == null)
            {
                SelectedPlayerIndex = -1;
                SelectedPlayer = null;
                return;
            }

            SelectedPlayerIndex = (int)e.ReturnValue;
            SelectedPlayer = new Player(SelectedPlayerIndex);
        }

        public int SelectedPlayerIndex { get; }
        public Player SelectedPlayer { get; }
    }
}