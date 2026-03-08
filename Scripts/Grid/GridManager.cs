using Godot;
using OOTA.Buildings;
using OOTA.Enums;
using System;
using System.Collections.Generic;

namespace OOTA.Grid;

[Tool]
public partial class GridManager : Node
{
    [ExportToolButton("DrawDebug")]
    Callable DebugDraw => Callable.From(DebugGrid);
    [Export] float tileSize = 1.0f;
    [Export] int gridHeight = 6;
    [Export] int gridWidth = 8;


    Vector2I coordOffset = Vector2I.Zero;
    /// <summary>
    /// row/column
    /// </summary>
    Dictionary<int, Dictionary<int, GridLocation>> grid;
    private Goal rightTeamGoal;
    private Goal leftTeamGoal;

    private StagingArea rightTeamStagingArea;
    private StagingArea leftTTeamStagingArea;

    public Goal RightTeamGoal => rightTeamGoal;
    public Goal LeftTeamGoal => leftTeamGoal;

    public StagingArea RightTeamStagingArea => rightTeamStagingArea;
    public StagingArea LeftTeamStagingArea => leftTTeamStagingArea;
    public event EventHandler<Vector2I> OnTileChanged;

    private void DebugGrid()
    {
        InitGrid();
        foreach (KeyValuePair<int, Dictionary<int, GridLocation>> item in grid)
        {
            foreach (KeyValuePair<int, GridLocation> tile in item.Value)
            {
                MGizmosCSharp.GizmoUtils.DrawShape(CoordToWorld(tile.Value.Coord), MGizmosCSharp.GSHAPES.STOP, 10.0f, tileSize, Colors.Yellow);
            }
        }
    }

    public Vector3 CoordToWorld(Vector2I coord)
    {
        coord += coordOffset;
        return new Vector3(coord.X, 0.0f, coord.Y) * tileSize + new Vector3(1, 0, 1) * tileSize * 0.5f;
    }


    public Vector2I WorldToCoord(Vector3 worldPoint)
    {
        Vector3 adjustedWorldPoint = worldPoint * 1 / tileSize - Vector3.One * tileSize * 0.5f;
        Vector2I coord = new Vector2I(Mathf.RoundToInt(adjustedWorldPoint.X), Mathf.RoundToInt(adjustedWorldPoint.Z));
        return coord - coordOffset;
    }

    public Vector3 SnapWorldToTileCenter(Vector3 worldPoint)
    {
        Vector3I clamped = (Vector3I)(worldPoint * 1 / tileSize);
        Vector2I worldPos = new Vector2I(Mathf.RoundToInt(clamped.X), Mathf.RoundToInt(clamped.Z));
        if (clamped.X < 0) { worldPos.X -= 1; }
        if (clamped.Y < 0) { worldPos.Y -= 1; }
        Vector2I coord = worldPos;//+ offset;
        return CoordToWorld(coord);
    }

    internal void InitGrid(int width, int depth)
    {
        gridWidth = width;
        gridHeight = depth;
        InitGrid();
    }
    private void InitGrid()
    {
        grid = new Dictionary<int, Dictionary<int, GridLocation>>();
        coordOffset = new Vector2I(-Mathf.FloorToInt(gridWidth * 0.5f), -Mathf.FloorToInt(gridHeight * 0.5f));

        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                if (!grid.ContainsKey(x)) { grid[x] = new Dictionary<int, GridLocation>(); }
                grid[x][y] = new GridLocation(new Vector2I(x, y));// - offset);
            }
        }
    }

    internal int Distance(Vector2I coord, Vector2I otherTileCoord)
    {
        Vector2I distance = coord - otherTileCoord;
        return Math.Max(Math.Abs(distance.X), Math.Abs(distance.Y));
    }

    internal bool[] GetFriendlyFoundationFlag(TEAM team, Vector2I coord)
    {
        bool[] result = new bool[4];
        List<GridLocation> neighbors = GetNeighbors(coord);
        for (int i = 0; i < 4; i++)
        {
            GridLocation location = neighbors[i];
            if (location is null || location.IsFree) { result[i] = false; continue; }
            if (location.Foundation is null) { result[i] = false; continue; }
            if (location.Team == team) { result[i] = true; }
        }
        return result;
    }

    public List<GridLocation> GetNeighbors(Vector2I coord)
    {
        List<GridLocation> result =
        [
            GetGridLocation(coord + Vector2I.Down),
            GetGridLocation(coord + Vector2I.Left),
            GetGridLocation(coord + Vector2I.Up),
            GetGridLocation(coord + Vector2I.Right),
        ];
        return result;
    }
    public List<GridLocation> GetFreeNeighbors(Vector2I coord)
    {
        List<GridLocation> result =
        [
            GetGridLocation(coord + Vector2I.Up),
            GetGridLocation(coord + Vector2I.Up + Vector2I.Right),
            GetGridLocation(coord + Vector2I.Right),
            GetGridLocation(coord + Vector2I.Right + Vector2I.Down),
            GetGridLocation(coord + Vector2I.Down),
            GetGridLocation(coord + Vector2I.Down + Vector2I.Left),
            GetGridLocation(coord + Vector2I.Left),
            GetGridLocation(coord + Vector2I.Left + Vector2I.Up),
        ];
        return result.FindAll(p => p.IsFree);
    }


    public GridLocation GetGridLocation(Vector3 worldPoint)
    {
        return GetGridLocation(WorldToCoord(worldPoint));
    }

    public GridLocation GetGridLocation(Vector2I coord)
    {
        if (grid.ContainsKey(coord.X))
        {
            if (grid[coord.X].ContainsKey(coord.Y))
            {
                return grid[coord.X][coord.Y];
            }
        }
        return null;
    }

    internal bool TileIsFree(Vector3 placerPoint)
    {
        Vector2I coord = WorldToCoord(placerPoint);
        GridLocation tile = GetGridLocation(coord);
        //GD.Print($"GridManager::TileIsFree({placerPoint}) coord[{coord}] tile is NULL[{tile is null}]");
        return tile.IsFree;
    }
    /// <summary>
    /// Ties the given building to the grid location instance<br/>
    /// Causes event OnTileChanged to be raised
    /// </summary>
    /// <param name="building"></param>
    /// <param name="rebuildNavMesh"></param>
    internal void PlaceStructure(BuildingBaseClass building, bool rebuildNavMesh = true)
    {
        GridLocation tile = GetGridLocation(WorldToCoord(building.GlobalPosition));
        tile.Building = building;
        if (rebuildNavMesh) { GridNavigation.Rebuild(); }

        if (building is Goal goal)
        {
            if (goal.Team == TEAM.RIGHT)
            {
                rightTeamGoal = goal;
            }
            else
            {
                leftTeamGoal = goal;
            }
        }

        if (building is StagingArea stagingArea)
        {
            if (stagingArea.Team == TEAM.RIGHT)
            {
                rightTeamStagingArea = stagingArea;
            }
            else
            {
                leftTTeamStagingArea = stagingArea;
            }
        }

        OnTileChanged?.Invoke(null, tile.Coord);
        Rpc(nameof(RPCPlaceStructure), building.GetPath());
    }
    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void RPCPlaceStructure(string nodePath)
    {
        BuildingBaseClass building = GetNode<BuildingBaseClass>(nodePath);

        GridLocation tile = GetGridLocation(WorldToCoord(building.GlobalPosition));
        tile.Building = building;

        if (building is Goal goal)
        {
            if (goal.Team == TEAM.RIGHT)
            {
                rightTeamGoal = goal;
            }
            else
            {
                leftTeamGoal = goal;
            }
        }
        if (building is StagingArea stagingArea)
        {
            if (stagingArea.Team == TEAM.RIGHT)
            {
                rightTeamStagingArea = stagingArea;
            }
            else
            {
                leftTTeamStagingArea = stagingArea;
            }
        }
        OnTileChanged?.Invoke(null, tile.Coord);
    }

    internal void RemoveStructure(BuildingBaseClass building)
    {
        GridLocation tile = GetGridLocation(WorldToCoord(building.GlobalPosition));
        if (building.towerType == TOWERTYPE.FOUNDATION)
        {
            tile.Foundation = null;
        }
        else
        {
            tile.Building = null;
        }
        GridNavigation.Rebuild();
        OnTileChanged?.Invoke(null, tile.Coord);
        Rpc(nameof(RPCRemoveStructure), building.GetPath());
    }

    [Rpc(MultiplayerApi.RpcMode.Authority, CallLocal = false, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    internal void RPCRemoveStructure(string nodePath)
    {
        BuildingBaseClass building = GetNode<BuildingBaseClass>(nodePath);
        GridLocation tile = GetGridLocation(WorldToCoord(building.GlobalPosition));

        if (building.towerType == TOWERTYPE.FOUNDATION)
        {
            tile.Foundation = null;
        }
        else
        {
            tile.Building = null;
        }
        OnTileChanged?.Invoke(null, tile.Coord);
    }



    internal List<GridLocation> GetFirstColumn()
    {
        return GetColumn(0);
    }
    internal List<GridLocation> GetLastColumn()
    {
        return GetColumn(gridWidth - 1);
    }

    /// <summary>
    /// To get column from the end, use negative index<br/>
    /// -1 is the last column
    /// </summary>
    /// <param name="columnIDX"></param>
    /// <returns></returns>
    internal List<GridLocation> GetColumn(int columnIDX)
    {
        List<GridLocation> result = new List<GridLocation>();

        if (columnIDX < 0) { columnIDX = gridWidth + columnIDX; }

        if (grid.ContainsKey(columnIDX))
        {
            for (int y = 0; y < gridHeight; y++)
            {
                if (grid[columnIDX].ContainsKey(y))
                {
                    result.Add(grid[columnIDX][y]);
                }
            }
        }
        //GD.Print($"GridManager::GetColumn({columnIDX}) result count[{result.Count}] GridHeight[{ins.gridHeight}] gridWidth[{ins.gridWidth}]");
        return result;
    }

    internal Vector2I LeftGoalCoord()
    {
        return (grid[1][(int)(gridHeight * 0.8f)] as GridLocation).Coord;
    }
    internal Vector2I RightGoalCoord()
    {
        return (grid[gridWidth - 2][(int)(gridHeight * 0.8f)] as GridLocation).Coord;
    }

}// EOF CLASS
