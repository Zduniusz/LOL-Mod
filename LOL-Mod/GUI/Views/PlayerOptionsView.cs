using System;
using System.Linq;
using LOL.GUI.Modals;
using UnityEngine;

namespace LOL.GUI.Views;

[ViewAttribute(typeof(PlayerOptionsView), "Player Options", 200, 50)]
public class PlayerOptionsView : IView
{
    private Player _selectedPlayer;
    public bool IsVisible { get; set; }

    public void Render()
    {
        if (GUILayout.Button((_selectedPlayer == null || !_selectedPlayer.IsValid())
                ? "Select player"
                : $"Selected player: {_selectedPlayer.GetPlayerName()}"))
        {
            var modal = new SelectPlayerModal();
            modal.ModalClosed += (sender, args) =>
                _selectedPlayer = new SelectPlayerModal.SelectPlayerEventArgs(args).SelectedPlayer;
            modal.Spawn();
        }

        if (_selectedPlayer == null || !_selectedPlayer.IsValid()) return;

        GUILayout.Space(20);

        var methods = GetType().GetMethods();
        foreach (var method in methods)
        {
            var option = (Option)Attribute.GetCustomAttribute(method, typeof(Option));
            if (option == null) continue;

            if (GUILayout.Button(option.Name))
                method.Invoke(null, new object[] { _selectedPlayer });
        }
    }

    public void Toggle()
    {
    }

    [Option("Kill Player")]
    public static void KillPlayer(Player selectedPlayer) => selectedPlayer.Kill();

    [Option("Kick Player")]
    public static void KickPlayer(Player selectedPlayer)
    {
        var options = Enum.GetNames(typeof(Player.KickMethod)).Select(s => s.Replace("_", " ")).ToArray();
        SelectStringModal modal = new(options);
        modal.ModalClosed += (sender, args) => selectedPlayer.Kick((Player.KickMethod)args.ReturnValue);
        modal.Spawn();
    }

    [Option("Say as")]
    public static void SayAs(Player selectedPlayer)
    {
        var modal = new SayAsModal(selectedPlayer);
        modal.ModalClosed += (_, args) =>
        {
            if (args.ReturnValue is not SayAsModal.SayAsEventArgs eventArgs) return;
            var messageBytes = System.Text.Encoding.UTF8.GetBytes(eventArgs.Message);
            var eventChannel = (int)selectedPlayer.GetPlayerColor() * 2 + 2 + 1;
            var ignoreID = eventArgs.InvisibleToTarget ? selectedPlayer.GetSteamId().m_SteamID : 0;
            Network.SendMessageToAllClients(GameManager.Instance.mMultiplayerManager,
                messageBytes, P2PPackageHandler.MsgType.PlayerTalked, channel: eventChannel, ignoreUserID: ignoreID);
        };
        modal.Spawn();
    }
}