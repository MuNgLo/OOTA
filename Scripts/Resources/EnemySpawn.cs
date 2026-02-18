using Godot;

namespace Waves;
[GlobalClass]
public partial class EnemySpawn : Resource
{
    [Export] public PackedScene enemyPrefab;
    [Export] public int amount = 0;
}// EOF CLASS