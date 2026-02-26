using Godot;
using OOTA.Grid;
using System;
using System.Collections.Generic;

namespace OOTA.Buildings;


public partial class Foundation : BuildingBaseClass
{


    [ExportGroup("Connectors")]
    [Export] MeshInstance3D connSouth;
    [Export] MeshInstance3D connWest;
    [Export] MeshInstance3D connNorth;
    [Export] MeshInstance3D connEast;

    Vector2I coord;

    public override void _Ready()
    {
        base._Ready();
        GridManager.OnTileChanged += WhenTileChanges;
        TreeExiting += () => { GridManager.OnTileChanged -= WhenTileChanges; };
        coord = GridManager.WorldToCoord(GlobalPosition);
        connSouth.Hide();
        connWest.Hide();
        connNorth.Hide();
        connEast.Hide();
        WhenTileChanges(null, coord + Vector2I.Up);
    }



    private void WhenTileChanges(object sender, Vector2I otherTileCoord)
    {
        if (otherTileCoord != coord && GridManager.Distance(coord, otherTileCoord) < 2)
        {
            //GD.Print($"Foundation::WhenTileChanges() Distance[{GridManager.Distance(coord, otherTileCoord)}]");
            bool[] flags = GridManager.GetFriendlyFoundationFlag(Team, coord);
            if (flags[0]) { connSouth.Show(); } else { connSouth.Hide(); }
            if (flags[1]) { connWest.Show(); } else { connWest.Hide(); }
            if (flags[2]) { connNorth.Show(); } else { connNorth.Hide(); }
            if (flags[3]) { connEast.Show(); } else { connEast.Hide(); }
        }
    }
}// EOF CLASS
