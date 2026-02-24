using Godot;

namespace OOTA.Resources;

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