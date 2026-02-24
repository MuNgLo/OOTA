using Godot;
using System;
using System.Net;

namespace OOTA.GameLogic;
[GlobalClass]
public partial class UIAPICalls : Node
{
    private static UIAPICalls instance;
    public override void _EnterTree()
    {
        instance = this;
    }
    /// <summary>
    /// Starts a host under the current profile
    /// State will turn loaded as host setup is done
    /// </summary>
    internal static void HostStart()
    {
        Core.Lobby.StartHost(27015, 5);
    }
    /// <summary>
    /// Stops a currently running host
    /// State turns back to loaded after cleanup
    /// </summary>
    internal static void HostStop()
    {
        //Core.State = GAMESTATE.LOADING;
        DisconnectFromGame();
    }

    #region Single purpose methods
    /// <summary>
    /// Do what is necessary before closing the whole program
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    internal static void CloseGame()
    {
        instance.GetTree().Quit();
    }

    internal static void SelectCharacter(int characterID)
    {
        //Core.Game.PlayerRequestCharacter(characterID);
    }

    internal static void JoinHost(string lobbyKey)
    {
        if (Core.StringToAddressAndPort(lobbyKey, out IPAddress ip, out int port))
        {
            //Core.Configs.PlayerConfig.lobbyLast = lobbyKey;
            //Core.Configs.SavePlayerConfig();
            Core.Lobby.JoinHost(ip, port);
            //MMenuSystem.MenuSystem.GoToMenu("MainMenu");
        }
        else
        {
            GD.Print($"UIAPI::JoinHost() Invalid Lobby Key.");
        }
    }

    internal static void DisconnectFromGame()
    {
        if (!instance.Multiplayer.HasMultiplayerPeer()) { return; }
        MultiplayerApi MP = instance.Multiplayer;
        if (MP.IsServer())
        {
            //Core.State = GAMESTATE.LOADING;
            GD.Print($"UIAPICalls::DisconnectFromGame() Host disconnecting.");

            Core.Lobby.StopHost();
        }
        else
        {
            //Core.State = GAMESTATE.LOADING;
            Core.Lobby.LeaveHost();
            //Core.Local.UnLoadCurrentLocalScene();
        }
    }

    internal static async void RunAutoNetworkDetection()
    {
        string result = await Core.Lobby.ProbeNetworkForInfo(27015);
        if (result == string.Empty) { return; }
        string[] strs = result.Split(':');
        //Core.Configs.HostConfig.HostIPString = strs[0];
        //Core.Configs.SaveHostConfig();
    }

    #endregion
}// EOF CLASS
