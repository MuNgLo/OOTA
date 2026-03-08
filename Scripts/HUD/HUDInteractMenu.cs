using Godot;
using OOTA.Enums;
using OOTA.Grid;
using System;
using System.Collections.Generic;

namespace OOTA.HUD;

public partial class HUDInteractMenu : Control
{
    /// <summary>
    /// Size of menu relative to screen height
    /// </summary>
    [Export(PropertyHint.Range, "0,1")] float menuSize = 0.5f;
    [Export(PropertyHint.Range, "0,3")] float entryScaleTweak = 1.0f;

    [Export] PackedScene prefabActionEntry;


    List<PlayerActionStruct> interactions;
    int division = 4;
    public int selectedButton = -1;
    List<HudActionEntry> menuButtons;
    Vector2[] menuLocations;
    Vector2I currentMenuCoord;

    //float Radius => DisplayServer.WindowGetSize().Y * 0.5f * menuSize;
    float Radius => 768 * 0.5f * menuSize;

    public override void _Ready()
    {
        LocalLogic.OnHudInteractMenu += WhenInteractMenu;
        LocalLogic.OnPlayerModeChanged += WhenPlayerModeChanged;

        interactions = new List<PlayerActionStruct>();
        menuButtons = new List<HudActionEntry>();
    }

    private void WhenPlayerModeChanged(object sender, PLAYERMODE e)
    {
        if (e != PLAYERMODE.INTERACTING)
        {
            Hide();
            currentMenuCoord = Vector2I.Down * 2000;
        }
    }

    public override void _Process(double delta)
    {
        if (Visible)
        {
            if (selectedButton != -1 && Input.IsActionJustReleased("Place"))
            {
                interactions[selectedButton].action.Invoke();
                Hide();
                ProcessMode = ProcessModeEnum.Disabled;
                Core.Players.LocalPlayer.Mode = PLAYERMODE.BUILDING;
                return;
            }
            if (!Input.IsActionPressed("Place"))
            {
                Hide();
                ProcessMode = ProcessModeEnum.Disabled;
                Core.Players.LocalPlayer.Mode = PLAYERMODE.BUILDING;
            }
        }
    }

    private void WhenInteractMenu(object sender, List<PlayerActionStruct> newInteractions)
    {
        ProcessMode = ProcessModeEnum.Inherit;
        if (Visible && newInteractions.Count < 1)
        {
            Core.Players.LocalPlayer.Mode = PLAYERMODE.BUILDING;
            return;
        }
        Core.Players.LocalPlayer.Mode = PLAYERMODE.INTERACTING;

        if (currentMenuCoord == newInteractions[0].Coord) { return; }
        interactions = newInteractions;

        // if cursor is visible we use it as screen position
        // Otherwise we use grid coord world location
        Position = GetViewport().GetMousePosition();

        GD.Print($"SomeMenu::carries interactions([{newInteractions.Count}])");

        // Set division
        division = interactions.Count;
        BuildActionEntries();
        ReplotMenuPositions(division);
        for (int i = 0; i < division; i++)
        {
            HudActionEntry btn = menuButtons[i];
            if (btn.GetParent() is null)
            {
                AddChild(btn);
            }
            btn.Scale = Vector2.One * entryScaleTweak;
            btn.PivotOffset = btn.Size * 0.5f;
            btn.Position = menuLocations[i] * Radius - btn.Size * 0.5f;
        }
        KeepButtonRotation();
        Show();
    }





    #region Make The Buttons
    private void BuildActionEntries()
    {
        if (menuButtons is not null) { DeleteAllTextureButtons(); }
        for (int i = 0; i < interactions.Count; i++)
        {
            menuButtons.Add(BuildActionEntry(interactions[i], i));
        }
    }
    private HudActionEntry BuildActionEntry(PlayerActionStruct playerAction, int idx)
    {
        HudActionEntry entry = prefabActionEntry.Instantiate<HudActionEntry>();
        entry.SetupEntry(this, playerAction, idx);
        return entry;
    }
    private void DeleteAllTextureButtons()
    {
        for (int i = 0; i < menuButtons.Count; i++)
        {
            menuButtons[i].QueueFree();
        }
        menuButtons.Clear();
    }
    #endregion

    #region Checked
    private void KeepButtonRotation()
    {
        foreach (Control child in GetChildren())
        {
            child.Rotation = -Rotation;
        }
    }
    private void ReplotMenuPositions(int subDivisions = 1)
    {
        Vector2[] arr = new Vector2[subDivisions];

        float step = 2 * Mathf.Pi / subDivisions;
        for (int i = 0; i < subDivisions; i++)
        {
            float x = Mathf.Cos(i * step);
            float y = Mathf.Sin(i * step);
            arr[i] = new Vector2(x, y).Normalized();
        }
        menuLocations = arr;
    }

    #endregion
}// EOF CLASS
