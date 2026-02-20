using Godot;
using System;

public partial class WorldNavigation : NavigationRegion3D
{
    private static WorldNavigation ins;

    public static Rid WorldNavMapRid => ins.GetNavigationMap();

    public override void _EnterTree()
    {
        ins = this;
    }

}// EOF CLASS

