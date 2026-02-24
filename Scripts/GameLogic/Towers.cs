using Godot;
using OOTA.Resources;
using System;

namespace OOTA.GameLogic;

public partial class Towers : Node
{
    [ExportGroup("Towers")]
    [Export(PropertyHint.ResourceType, "TowerResource")] TowerResource[] towers;
    public int MaxIndex => towers.Length - 1;

    public TowerResource GetTowerByIndex(int index)
    {
        return towers[index];
    }
}// EOF CLASS
