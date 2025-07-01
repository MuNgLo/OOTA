using Godot;
using System.Security.Cryptography.X509Certificates;

namespace Munglo.DungeonGenerator
{
    [GlobalClass, Tool]
    public partial class ProfileResource : DungeonAddonResource
    {
        [Export] public bool showDebugLayer = false;
        [Export] public Vector3 globalOffset = Vector3.Zero;
        [Export] public GenerationSettingsResource settings;
        [Export] public BiomeResource biome;
        [Export] public bool useRandomSeed = true;
    }
}
