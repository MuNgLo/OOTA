using Godot;
using OOTA.Buildings;
using OOTA.Enums;
using System;
using System.Collections.Generic;

namespace OOTA.Grid;

[Tool]
public partial class GridManager : Node
{
    static GridManager ins;

    [ExportToolButton("DrawDebug")]
    Callable DebugDraw => Callable.From(DebugGrid);
    [Export] float tileSize = 1.0f;
    [Export] int gridHeight = 6;
    [Export] int gridWidth = 8;


    static Vector2I coordOffset = Vector2I.Zero;
    /// <summary>
    /// row/column
    /// </summary>
    Dictionary<int, Dictionary<int, GridLocation>> grid;
    private Goal rightTeamGoal;
    private Goal leftTeamGoal;

    public static Goal RightTeamGoal => ins.rightTeamGoal;
    public static Goal LeftTeamGoal => ins.leftTeamGoal;

    public static event EventHandler<Vector2I> OnTileChanged;

    public override void _EnterTree()
    {
        ins = this;
    }

    private void DebugGrid()
    {
        if (ins is null) { ins = this; }
        InitGrid();
        foreach (KeyValuePair<int, Dictionary<int, GridLocation>> item in grid)
        {
            foreach (KeyValuePair<int, GridLocation> tile in item.Value)
            {
                MGizmosCSharp.GizmoUtils.DrawShape(CoordToWorld(tile.Value.Coord), MGizmosCSharp.GSHAPES.STOP, 10.0f, tileSize, Colors.Yellow);
            }
        }
        ins = null;
    }

    public static Vector3 CoordToWorld(Vector2I coord)
    {
        coord += coordOffset;
        return new Vector3(coord.X, 0.0f, coord.Y) * ins.tileSize + new Vector3(1, 0, 1) * ins.tileSize * 0.5f;
    }


    public static Vector2I WorldToCoord(Vector3 worldPoint)
    {
        Vector3 adjustedWorldPoint = worldPoint * 1 / ins.tileSize - Vector3.One * ins.tileSize * 0.5f;
        Vector2I coord = new Vector2I(Mathf.RoundToInt(adjustedWorldPoint.X), Mathf.RoundToInt(adjustedWorldPoint.Z));
        return coord - coordOffset;
    }

    public static Vector3 SnapWorldToTileCenter(Vector3 worldPoint)
    {
        Vector3I clamped = (Vector3I)(worldPoint * 1 / ins.tileSize);
        Vector2I worldPos = new Vector2I(Mathf.RoundToInt(clamped.X), Mathf.RoundToInt(clamped.Z));
        if (clamped.X < 0) { worldPos.X -= 1; }
        if (clamped.Y < 0) { worldPos.Y -= 1; }
        Vector2I coord = worldPos;//+ offset;
        return CoordToWorld(coord);
    }

    internal static void InitGrid(int width, int depth)
    {
        ins.gridWidth = width;
        ins.gridHeight = depth;
        ins.InitGrid();
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

    internal static int Distance(Vector2I coord, Vector2I otherTileCoord)
    {
        Vector2I distance = coord - otherTileCoord;
        return Math.Min(Math.Abs(distance.X), Math.Abs(distance.Y));
    }

    internal static bool[] GetFriendlyFoundationFlag(TEAM team, Vector2I coord)
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

    private static List<GridLocation> GetNeighbors(Vector2I coord)
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

    public static GridLocation GetGridLocation(Vector3 worldPoint)
    {
        return GetGridLocation(WorldToCoord(worldPoint));
    }

    public static GridLocation GetGridLocation(Vector2I coord)
    {
        if (ins.grid.ContainsKey(coord.X))
        {
            if (ins.grid[coord.X].ContainsKey(coord.Y))
            {
                return ins.grid[coord.X][coord.Y];
            }
        }
        return null;
    }

    internal static bool TileIsFree(Vector3 placerPoint)
    {
        Vector2I coord = WorldToCoord(placerPoint);
        GridLocation tile = GetGridLocation(coord);
        //GD.Print($"GridManager::TileIsFree({placerPoint}) coord[{coord}] tile is NULL[{tile is null}]");
        return tile.IsFree;
    }

    internal static void PlaceStructure(BuildingBaseClass building, bool rebuildNavMesh = true)
    {
        GridLocation tile = GetGridLocation(WorldToCoord(building.GlobalPosition));
        tile.SetBuilding(building);
        if (rebuildNavMesh) { GridNavigation.Rebuild(); }

        if(building is Goal goal)
        {
            if(goal.Team == TEAM.RIGHT)
            {
                ins.rightTeamGoal = goal;
            }
            else
            {
                ins.leftTeamGoal = goal;
            }
        }
        OnTileChanged?.Invoke(null, tile.Coord);
    }

    internal static void RemoveStructure(BuildingBaseClass building)
    {
        GridLocation tile = GetGridLocation(WorldToCoord(building.GlobalPosition));
        if (building.towerType == TOWERTYPE.FOUNDATION)
        {
            tile.Foundation = null;
        }
        else
        {
            tile.Tower = null;
        }
        GridNavigation.Rebuild();
        OnTileChanged?.Invoke(null, tile.Coord);
    }

    internal static List<GridLocation> GetFirstColumn()
    {
        return GetColumn(0);
    }
    internal static List<GridLocation> GetLastColumn()
    {
        return GetColumn(ins.gridWidth - 1);
    }


    internal static List<GridLocation> GetColumn(int column)
    {
        List<GridLocation> result = new List<GridLocation>();
        if (ins.grid.ContainsKey(column))
        {
            for (int y = 0; y < ins.gridHeight; y++)
            {
                if(ins.grid[column].ContainsKey(y))
                {
                    result.Add(ins.grid[column][y]);
                }
            }
        }
        //GD.Print($"GridManager::GetColumn({column}) result count[{result.Count}] GridHeight[{ins.gridHeight}] gridWidth[{ins.gridWidth}]");
        return result;
    }

    internal static Vector2I LeftGoalCoord()
    {
        return (ins.grid[1][ins.gridHeight / 2] as GridLocation).Coord;
    }
    internal static Vector2I RightGoalCoord()
    {
        return (ins.grid[ins.gridWidth - 2][ins.gridHeight / 2] as GridLocation).Coord;
    }
}// EOF CLASS
