using Godot;
using System;

namespace OOTA.Grid;

public partial class GridNavigation : NavigationRegion3D
{
    private static GridNavigation ins;
    private static NavigationMeshSourceGeometryData3D geoData;
    public static EventHandler OnNavMeshRebuilt;

    public static Rid WorldNavMapRid => ins.GetNavigationMap();
    public static bool IsRebuilding => NavigationServer3D.IsBakingNavigationMesh(ins.NavigationMesh);

    public override void _EnterTree()
    {
        ins = this;
        geoData = new NavigationMeshSourceGeometryData3D();
    }

    public static void Rebuild()
    {
        if(IsRebuilding)
        {
            //GD.Print("WorldNavigation::Rebuild() already rebuilding, skipping...");
            return;
        }
        //GD.Print("WorldNavigation::Rebuild() REBUILDING!!!");
        NavigationServer3D.ParseSourceGeometryData(ins.NavigationMesh, geoData, ins, Callable.From(RebuildPart2));
    }

    public static void RebuildPart2()
    {
        NavigationServer3D.BakeFromSourceGeometryDataAsync(ins.NavigationMesh, geoData, Callable.From(RebuildDone));
    }
    public static void RebuildDone()
    {
        EventHandler evt = OnNavMeshRebuilt;
        evt?.Invoke(null, null);
    }

}// EOF CLASS

