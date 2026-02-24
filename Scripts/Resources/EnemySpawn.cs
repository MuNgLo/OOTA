using Godot;

namespace OOTA.Resources;

[GlobalClass]
public partial class EnemySpawn : Resource
{
    [Export] public PackedScene enemyPrefab;
    [Export] public int amount = 0;
}// EOF CLASS