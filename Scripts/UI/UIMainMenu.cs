using Godot;
using System;

namespace UI.Menus;

public enum MENUSTATE { NONE, START, HOST, JOIN, HOSTING, CONNECTED, PLAYING }
[GlobalClass]
public partial class UIMainMenu : PanelContainer
{
    [Export] RichTextLabel feedBackText;

    [ExportGroup("Navigation Btns")]
    [Export] Button btnHost;
    [Export] Button btnStartHost;
    [Export] Button btnStopHost;
    [Export] Button btnJoin;
    [Export] Button btnConnect;
    [Export] Button btnDisConnect;
    [Export] Button btnExit;
    [Export] Button btnResume;
    [Export] Button btnCloseMenu;
    [Export] Button btnReady;

    [ExportGroup("Host settings")]
    [Export] SpinBox hostMaxPlayers;
    [Export] LineEdit hostPort;
    [Export] LineEdit lobbyKey;

    MENUSTATE state = MENUSTATE.START;
    public MENUSTATE State { get => state; set => ChangeState(value); }

    public event EventHandler<MENUSTATE> OnMenuStateChanged;


    public override void _EnterTree()
    {
        LocalLogic.mainMenu = this;
    }

    public override void _Ready()
    {
        state = MENUSTATE.START;
        OnMenuStateChanged?.Invoke(null, MENUSTATE.START);
        // Hook up buttons
        btnHost.ButtonDown += () => State = MENUSTATE.HOST;
        btnStartHost.ButtonDown += () => { State = MENUSTATE.HOSTING; UIAPI.UIAPICalls.HostStart(); };
        btnStopHost.ButtonDown += () => { State = MENUSTATE.HOST; UIAPI.UIAPICalls.DisconnectFromGame(); };

        btnJoin.ButtonDown += () => State = MENUSTATE.JOIN;
        btnConnect.ButtonDown += () => { State = MENUSTATE.CONNECTED; UIAPI.UIAPICalls.JoinHost(lobbyKey.Text); };
        btnDisConnect.ButtonDown += () => { State = MENUSTATE.START; UIAPI.UIAPICalls.DisconnectFromGame(); };

        btnResume.ButtonDown += () => MMenuSystem.MenuSystem.HideMenu();
        btnExit.ButtonDown += () => UIAPI.UIAPICalls.CloseGame();

        btnCloseMenu.ButtonDown += () => MMenuSystem.MenuSystem.HideMenu();

        btnReady.ButtonDown += () => Core.Rules.PlayerRequestReady();
    }

    internal void ConnectedToGame()
    {

        State = MENUSTATE.PLAYING;
    }

    private void ChangeState(MENUSTATE value)
    {
        if (state != value)
        {
            state = value;
            OnMenuStateChanged?.Invoke(null, state);
            feedBackText.Text = state.ToString();
        }
    }
}// EOF CLASS
