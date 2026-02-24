using Godot;
using MLobby;
using System;

namespace OOTA.UI;

public partial class UIPlayerList : GridContainer
{
    [Export] Control playerList;
    [Export] LineEdit playerNameEdit;
    [Export] float playerListDuration = 1.0f;
    [Export] PackedScene prefabPlayerEntry;

    bool isShowing = false;
    Vector2 ogPos;
    Vector2 RightPos => ogPos + Vector2.Right * playerList.Size.X * 0.95f;
    public override void _Ready()
    {
        ogPos = playerList.Position;
        MLobbyPlayerEvents.OnPlayersChanged += UpdatePlayerList;
        playerNameEdit.TextSubmitted += WhenPlayerNameSubmitted;
    }

    private void WhenPlayerNameSubmitted(string newName)
    {
        Core.Rules.PlayerRequestNameChange(newName);
    }

    private void UpdatePlayerList(object sender, EventArgs args)
    {
        //GD.Print($"UpdatePlayerList() playerCounty[{Core.Players.All.Count}]");
        ClearsList();
        foreach (MLobbyPlayer player in Core.Players.All)
        {
            AddPlayerEntry(player as OOTAPlayer);
        }
        if (GetChildCount() > 0 && !isShowing)
        {
            MoveOut();
        }
        if (GetChildCount() < 1 && isShowing)
        {
            MoveIn();
        }
    }

    private void MoveIn()
    {
        isShowing = false;
        var tween = CreateTween();
        tween.TweenProperty(playerList, "position", ogPos, playerListDuration)
              .SetTrans(Tween.TransitionType.Linear)
              .SetEase(Tween.EaseType.InOut);
    }

    private void MoveOut()
    {
        isShowing = true;
        var tween = CreateTween();
        tween.TweenProperty(playerList, "position", RightPos, playerListDuration)
              .SetTrans(Tween.TransitionType.Linear)
              .SetEase(Tween.EaseType.InOut);
    }

    private void AddPlayerEntry(OOTAPlayer player)
    {
        // Name Ready Ping
        Control entry = prefabPlayerEntry.Instantiate<Control>();
        entry.GetNode<Label>("Ping").Text = "-";
        entry.GetNode<RichTextLabel>("Name").Text = player.PlayerName;
        entry.GetNode<CheckBox>("Ready").SetPressedNoSignal(player.IsReady);
        AddChild(entry,true);
        // Sync the name field
        if (Multiplayer.GetUniqueId() == player.PeerID)
        {
            playerNameEdit.Text = player.PlayerName;
        }
    }

    private void ClearsList()
    {
        foreach (Node child in GetChildren())
        {
            child.QueueFree();
        }
    }
}// EOF CLASS
