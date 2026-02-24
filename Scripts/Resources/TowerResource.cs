using Godot;
using OOTA.Enums;

namespace OOTA.Resources;

[GlobalClass]
public partial class TowerResource : Resource
{
    [Export] public TOWERTYPE towerType = TOWERTYPE.NONE;
    [Export] public PackedScene towerPrefab;
    [Export] public int cost = 0;
    [Export] public Mesh mesh;
}// EOF CLASS