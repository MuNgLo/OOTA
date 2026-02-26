using Godot;
using OOTA.Enums;
using OOTA.Grid;
using OOTA.Resources;
using System;
using System.Threading.Tasks;

namespace OOTA.GameLogic;

public partial class Placer : Node
{
    [Export] MeshInstance3D placeGhost;
    [Export] Node3D worldRoot;
    [Export] Node3D placeBlocker;

    PlayerAvatar avatar;
    int towerIDX = 0;
    private Vector3 inRightStick;
    private Vector3 cursorWorldPosition;



    Vector2I blockCheckLastCoord = new Vector2I(int.MinValue, int.MinValue);
    bool isBlocked = true;

    [Export] NavigationMesh navMesh;
    NavigationMeshSourceGeometryData3D geoData;
    Rid map;
    Rid region;

    public override void _Ready()
    {
        LocalLogic.OnAvatarAssigned += WhenAvatarAssigned;
        placeGhost.Hide();

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


    public override void _PhysicsProcess(double delta)
    {
        if (avatar is null) { return; }
        if (avatar.mode == PLAYERMODE.BUILDING)
        {
            TowerResource tw = Core.Rules.towers.GetTowerByIndex(towerIDX);
            if (!Core.Players.LocalPlayer.CanPay(tw.cost))
            {
                HidePlacement();
                return;
            }
            ConstructInputVector();
            GridLocation gridLocation = ProjectPlacerPosition();

            if (gridLocation is null || !gridLocation.CanFit(tw))
            {
                HidePlacement();
            }
            else
            {
                IsPathBlocked(gridLocation.Coord);

                if (isBlocked)
                {
                    HidePlacement();
                }
                else
                {
                    ShowPlacement(tw, gridLocation);
                    if (Input.IsActionPressed("Place"))
                    {
                        Core.Rules.RequestPlaceTower(towerIDX, gridLocation.Coord);
                    }
                }
            }
            // If in build mode accept tower index shift
            if (Input.IsActionJustPressed("BuildSelectLeft"))
            {
                towerIDX--;
                if (towerIDX < 0) { towerIDX = Core.Rules.towers.MaxIndex; }
            }
            if (Input.IsActionJustPressed("BuildSelectRight"))
            {
                towerIDX++;
                if (towerIDX > Core.Rules.towers.MaxIndex) { towerIDX = 0; }
            }
        }
        else
        {
            HidePlacement();
        }
    }




    private void IsPathBlocked(Vector2I coord)
    {
        if (blockCheckLastCoord == coord) { return; }
        GD.Print($"Placer::IsPathBlocked() Updating block check for coord[{coord}]");
        blockCheckLastCoord = coord;
        placeBlocker.Reparent(worldRoot);
        placeBlocker.GlobalPosition = GridManager.CoordToWorld(coord);
        isBlocked = true;
        geoData.Clear();
        NavigationServer3D.ParseSourceGeometryData(navMesh, geoData, worldRoot, Callable.From(BuildPlacerMeshPart2));
    }

    private void BuildPlacerMeshPart2()
    {
        NavigationServer3D.BakeFromSourceGeometryDataAsync(navMesh, geoData, Callable.From(RebuildDone));
    }
    private void RebuildDone()
    {
        GD.Print($"Placer::RebuildDone()");
        NavigationServer3D.RegionSetNavigationMesh(region, navMesh);
        placeBlocker.Reparent(GetTree().Root, true);
        placeBlocker.GlobalPosition = Vector3.Down * 20.0f;

    }

    private void WhenMapChanged(Rid mapThatChanged)
    {
        if(GridManager.LeftTeamGoal is null) {return;}
        if(map == mapThatChanged)
        {
            CheckPath();
        }
    }

    //bool isCheckingPath = false;
    private async void CheckPath()
    {
        //if (isCheckingPath) { return; }
        //isCheckingPath = true;
        GD.Print($"CheckPath");
        // This one probably is the ticket for a calm mind
        //GD.Print($"NavigationServer3D region iteration ID?[{NavigationServer3D.RegionGetIterationId(region)}]");

        await ToSignal(GetTree(), SceneTree.SignalName.PhysicsFrame);

        Vector3[] arr = NavigationServer3D.MapGetPath(
            map, 
            GridManager.LeftTeamGoal.GlobalPosition, 
            GridManager.RightTeamGoal.GlobalPosition, 
            true
            );
        
        
        if (arr.Length > 0)
        {
            isBlocked = arr[arr.Length - 1].DistanceTo(GridManager.RightTeamGoal.GlobalPosition) > 1.0f;
            //MGizmosCSharp.GizmoUtils.DrawShape(arr[arr.Length - 1], MGizmosCSharp.GSHAPES.DIAMOND, 0.5f, 0.5f, Colors.Red);
            //MGizmosCSharp.GizmoUtils.DrawShape(GridManager.RightTeamGoal.GlobalPosition + Vector3.Up, MGizmosCSharp.GSHAPES.DIAMOND, 0.5f, 0.5f, Colors.Blue);
            //MGizmosCSharp.GizmoUtils.DrawLine(arr, 0.5f, Colors.Red);
        }
        GD.Print($"Placer::CheckPath() arr.Length[{arr.Length}] blocked[{isBlocked}]");
        //isCheckingPath = false;
    }



    private void ConstructInputVector()
    {
        // Building input vector Right stick
        inRightStick = Vector3.Zero;
        inRightStick += Vector3.Right * Input.GetAxis("RSLeft", "RSRight");
        inRightStick += Vector3.Back * Input.GetAxis("RSUp", "RSDown");

        if (inRightStick == Vector3.Zero && Core.PlotMouseWorldPosition(out cursorWorldPosition))
        // Mode dependent Mouse
        {
            cursorWorldPosition.Y = avatar.GlobalPosition.Y;
            inRightStick = avatar.GlobalPosition.DirectionTo(cursorWorldPosition);
        }
    }

    private void HidePlacement()
    {
        placeGhost.Hide();
        placeGhost.PhysicsInterpolationMode = MeshInstance3D.PhysicsInterpolationModeEnum.Off;
    }

    private void ShowPlacement(TowerResource tw, GridLocation gridLocation)
    {
        if (gridLocation is null) { return; }
        if (gridLocation.Foundation is not null)
        {
            placeGhost.GlobalPosition = gridLocation.Foundation.GlobalPosition + Vector3.Up * 0.669f;
        }
        else
        {
            placeGhost.GlobalPosition = GridManager.CoordToWorld(gridLocation.Coord);
        }
        placeGhost.Mesh = tw.mesh;
        placeGhost.Show();
        placeGhost.PhysicsInterpolationMode = MeshInstance3D.PhysicsInterpolationModeEnum.On;
        placeGhost.ResetPhysicsInterpolation();
    }


    private GridLocation ProjectPlacerPosition()
    {
        Vector2I playerTileCoord = GridManager.WorldToCoord(avatar.GlobalPosition);

        //MGizmosCSharp.GizmoUtils.DrawShape(placer.GlobalPosition + Vector3.Up, MGizmosCSharp.GSHAPES.DIAMOND, 0.1f, 1.0f, Colors.Pink);
        //MGizmosCSharp.GizmoUtils.DrawShape(GridManager.CoordToWorld(playerTileCoord), MGizmosCSharp.GSHAPES.DIAMOND, 0.1f, 1.0f, Colors.BlueViolet);


        float angle = Vector3.Back.SignedAngleTo(inRightStick, Vector3.Up) + Mathf.Pi;
        angle = Mathf.RadToDeg(angle);
        int step = Mathf.FloorToInt(angle / 45.0f);
        //GD.Print($"Angle[{angle}] step[{step}] playerTileCoord[{playerTileCoord}] coord to world pos difference[{(GlobalPosition - GridManager.CoordToWorld(playerTileCoord)).Length()}]");
        if (step < 1) { playerTileCoord += Vector2I.Up; }
        else if (step < 2) { playerTileCoord += Vector2I.Up + Vector2I.Left; }
        else if (step < 3) { playerTileCoord += Vector2I.Left; }
        else if (step < 4) { playerTileCoord += Vector2I.Down + Vector2I.Left; }
        else if (step < 5) { playerTileCoord += Vector2I.Down; }
        else if (step < 6) { playerTileCoord += Vector2I.Down + Vector2I.Right; }
        else if (step < 7) { playerTileCoord += Vector2I.Right; }
        else if (step < 8) { playerTileCoord += Vector2I.Up + Vector2I.Right; }
        else { playerTileCoord += Vector2I.Up; }

        return GridManager.GetGridLocation(playerTileCoord);


    }

    #region pass 1


    private void WhenAvatarAssigned(object sender, PlayerAvatar e)
    {
        avatar = e;
        //ProcessMode = ProcessModeEnum.Inherit;
        if (avatar is not null)
        {
            avatar.TreeExiting += () =>
            {
                //ProcessMode = ProcessModeEnum.Disabled;
                avatar = null;
            };
        }
    }

    #endregion
}// EOF CLASS
