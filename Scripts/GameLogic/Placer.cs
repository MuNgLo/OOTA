using Godot;
using OOTA.Enums;
using OOTA.Grid;
using OOTA.Resources;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OOTA.GameLogic;

public partial class Placer : Node
{
    [Export] Node3D worldRoot;
    [Export] Node3D placeBlocker;

    private Vector3 inRightStick;
    private Vector3 cursorWorldPosition;

    PlayerAvatar Avatar => Core.Players.LocalPlayer.Avatar;
    Vector2I blockCheckLastCoord = new Vector2I(int.MinValue, int.MinValue);
    bool isBlocked = true;
    public bool IsBlocked => isBlocked;



    [ExportGroup("BlockCheck NavMesh")]
    [Export] NavigationMesh navMesh;
    NavigationMeshSourceGeometryData3D geoData;
    Rid map;
    Rid region;

    public override void _Ready()
    {
        InitNavMesh();
    }

    public GridLocation gridLocation;
    public override void _PhysicsProcess(double delta)
    {
        if (Core.Players.LocalPlayer is null) { return; }
        if (Core.Players.LocalPlayer.Mode == PLAYERMODE.BUILDING)
        {
            if (ProjectPlacerPosition() && gridLocation is not null)
            {
                IsPathBlocked(gridLocation.Coord);
            }
        }
    }

    private bool ProjectPlacerPosition()
    {
        Vector2I tileCoord = Core.Grid.WorldToCoord(Avatar.GlobalPosition);
        RightStickInputVector();
        // If inRightStick is 0 we lean on cursor pos
        if (inRightStick == Vector3.Zero && Core.PlotMouseWorldPosition(out cursorWorldPosition))
        {
            Vector2I mouseTileCoord = Core.Grid.WorldToCoord(cursorWorldPosition);
            if (Core.Grid.Distance(tileCoord, mouseTileCoord) < 2)
            {
                gridLocation = Core.Grid.GetGridLocation(mouseTileCoord);
                return true;
            }
            gridLocation = Core.Grid.GetGridLocation(Avatar.GlobalPosition);
            return false;
        }
        //MGizmosCSharp.GizmoUtils.DrawShape(placer.GlobalPosition + Vector3.Up, MGizmosCSharp.GSHAPES.DIAMOND, 0.1f, 1.0f, Colors.Pink);
        //MGizmosCSharp.GizmoUtils.DrawShape(Core.Grid.CoordToWorld(playerTileCoord), MGizmosCSharp.GSHAPES.DIAMOND, 0.1f, 1.0f, Colors.BlueViolet);
        float angle = Vector3.Back.SignedAngleTo(inRightStick, Vector3.Up) + Mathf.Pi;
        angle = Mathf.RadToDeg(angle);
        int step = Mathf.FloorToInt(angle / 45.0f);
        //GD.Print($"Angle[{angle}] step[{step}] playerTileCoord[{playerTileCoord}] coord to world pos difference[{(GlobalPosition - Core.Grid.CoordToWorld(playerTileCoord)).Length()}]");
        if (step < 1) { tileCoord += Vector2I.Up; }
        else if (step < 2) { tileCoord += Vector2I.Up + Vector2I.Left; }
        else if (step < 3) { tileCoord += Vector2I.Left; }
        else if (step < 4) { tileCoord += Vector2I.Down + Vector2I.Left; }
        else if (step < 5) { tileCoord += Vector2I.Down; }
        else if (step < 6) { tileCoord += Vector2I.Down + Vector2I.Right; }
        else if (step < 7) { tileCoord += Vector2I.Right; }
        else if (step < 8) { tileCoord += Vector2I.Up + Vector2I.Right; }
        else { tileCoord += Vector2I.Up; }
        gridLocation = Core.Grid.GetGridLocation(tileCoord);
        return true;
    }

   private void RightStickInputVector()
    {
        // Building input vector Right stick
        inRightStick = Vector3.Zero;
        inRightStick += Vector3.Right * Input.GetAxis("RSLeft", "RSRight");
        inRightStick += Vector3.Back * Input.GetAxis("RSUp", "RSDown");
    }



    #region Nav Mesh related
    /// <summary>
    /// Initialize the navMesh
    /// </summary>
    private void InitNavMesh()
    {
        // Create a new navigation map.
        map = NavigationServer3D.MapCreate();
        NavigationServer3D.MapSetUp(map, Vector3.Up);
        NavigationServer3D.MapSetActive(map, true);

        // Create a navigation region and assign it to the map.
        region = NavigationServer3D.RegionCreate();
        NavigationServer3D.RegionSetEnabled(region, true);
        NavigationServer3D.RegionSetMap(region, map);
        geoData = new NavigationMeshSourceGeometryData3D();
        NavigationServer3D.MapChanged += WhenMapChanged;
    }
    /// <summary>
    /// When the navMesh is done rebuilding, Start checking the path
    /// </summary>
    /// <param name="mapThatChanged"></param>
    private void WhenMapChanged(Rid mapThatChanged)
    {
        if (!Multiplayer.HasMultiplayerPeer()) { return; }
        if (Core.Grid.LeftTeamGoal is null) { return; }
        if (map == mapThatChanged)
        {
            CheckPath();
        }
    }
    /// <summary>
    /// Wait to physics tick. Then check path between goals and<br/>
    /// update the isBlocked bool
    /// </summary>
    private async void CheckPath()
    {
        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);
        Vector3[] arr = NavigationServer3D.MapGetPath(
            map,
            Core.Grid.LeftTeamGoal.GlobalPosition,
            Core.Grid.RightTeamGoal.GlobalPosition,
            true
            );
        if (arr.Length > 0)
        {
            isBlocked = arr[arr.Length - 1].DistanceTo(Core.Grid.RightTeamGoal.GlobalPosition) > 1.0f;
        }
    }
    /// <summary>
    /// Moves the block collider in position<br/>
    /// Clears the geo data and rebuilds it<br/>
    /// Will call IsPathBlocked2 when build is done
    /// </summary>
    /// <param name="coord"></param>
    public void IsPathBlocked(Vector2I coord)
    {
        if (blockCheckLastCoord == coord) { return; }
        blockCheckLastCoord = coord;

        if (Core.Grid.GetFreeNeighbors(coord).Count >= 7)
        {
            GD.Print($"Cant be blocked!! neighborCount[{Core.Grid.GetFreeNeighbors(coord).Count}]");
            isBlocked = false;
            return;
        }

        //GD.Print($"Placer::IsPathBlocked() Updating block check for coord[{coord}]");
        placeBlocker.Reparent(worldRoot);
        placeBlocker.GlobalPosition = Core.Grid.CoordToWorld(coord);
        isBlocked = true;
        geoData.Clear();
        NavigationServer3D.ParseSourceGeometryData(navMesh, geoData, worldRoot, Callable.From(IsPathBlocked2));
    }
    /// <summary>
    /// When geometry has been processed, start baking a new navMesh<br/>
    /// Will call IsPathBlocked3 when bake is done
    /// </summary>
    private void IsPathBlocked2()
    {
        NavigationServer3D.BakeFromSourceGeometryDataAsync(navMesh, geoData, Callable.From(IsPathBlocked3));
    }
    /// <summary>
    /// Update the navMesh for the region<br/>
    /// reset the blocker collider (maybe do that after geo bake if it starts causing issues)
    /// </summary>
    private void IsPathBlocked3()
    {
        //GD.Print($"[{Multiplayer.GetUniqueId()}]Placer::RebuildDone()");
        NavigationServer3D.RegionSetNavigationMesh(region, navMesh);
        placeBlocker.Reparent(GetTree().Root, true);
        placeBlocker.GlobalPosition = Vector3.Down * 20.0f;
    }
    #endregion
}// EOF CLASS
