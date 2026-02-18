using Godot;

namespace Waves;
[GlobalClass]
public partial class WaveDefinition : Resource
{
    [Export] public int spawnsOnGameTick = -1;
    [Export(PropertyHint.ResourceType, "EnemySpawn")]
    public EnemySpawn[] spawns;

    public WaveDefinition()
    {
        
    }
}// EOF CLASS