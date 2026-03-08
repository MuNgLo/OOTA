using System;
using Godot;

namespace OOTA;

public struct PlayerActionStruct
{
    public Vector2I Coord;
    public string ToolTip;
    public Action action;
    public Texture2D texture;
    public Color modulate;
    public int Cost;

    public static PlayerActionStruct Repair(Vector2I coord, int cost)
    {
        return new PlayerActionStruct()
        {
            Coord = coord,
            ToolTip = "Repair",
            Cost = cost,
            modulate = Colors.Green,
            texture = ResourceLoader.Load<Texture2D>("res://Images/Icons/Repair.png"),
            action = () => { Core.Rules.Repair(coord, cost); }
        };
    }

    public static PlayerActionStruct Sell(Vector2I coord, int cost)
    {
        return new PlayerActionStruct()
        {
            Coord = coord,
            ToolTip = "Sell",
            Cost = cost,
            modulate = Colors.Gold,
            texture = ResourceLoader.Load<Texture2D>("res://Images/Icons/Sell.png"),
            action = () => { Core.Rules.Sell(coord, cost); }
        };
    }
}// EOF STRUCT