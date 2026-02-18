using Godot;
using MLobby;
using MLogging;
using System;
using System.Diagnostics;

public enum TEAM { NONE, LEFT, RIGHT }
public enum PLAYERMODE { NONE, ATTACKING, BUILDING }

public partial class Core : Node
{
    private static Core ins;
    [Export] PackedScene goldPrefab;
    [Export] PackedScene healthPrefab;

    [Export] LobbyManager lobby;
    [Export] OOTAPlayerManager players;
    [Export] GameLogic rules;
    public static GameLogic Rules => ins.rules;
    public static LobbyManager Lobby => ins.lobby;

    [ExportGroup("Team related")]
    [Export] Material teamLeft;
    [Export] Material teamRight;

    [Export(PropertyHint.Layers3DPhysics)]
    public uint mouseCursorCollision;



    public static Material TeamLeft => ins.teamLeft;
    public static Material TeamRight => ins.teamRight;
    internal static OOTAPlayerManager Players => ins.players;


    public static Camera3D Camera { get; set; }

    public override void _EnterTree()
    {
        ins = this;
    }

    public static bool PlotMouseWorldPosition(out Vector3 worldPosition)
    {
        Vector2 mPos = ins.GetViewport().GetMousePosition();
        Vector3 origin = Camera.ProjectRayOrigin(mPos);
        Vector3 direction = Camera.ProjectRayNormal(mPos);
        Vector3 rayEnd = origin + direction * 500.0f;
        PhysicsDirectSpaceState3D spaceState = Camera.GetWorld3D().DirectSpaceState;
        PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(origin, rayEnd, ins.mouseCursorCollision);
        Godot.Collections.Dictionary result = spaceState.IntersectRay(query);
        if (result is not null && result.ContainsKey("position"))
        {
            worldPosition = result["position"].AsVector3();
            return true;
        }
        worldPosition = Vector3.Zero;
        return false;
    }

    private void SpawnHealth(Vector3 globalPosition)
    {
        RigidBody3D gold = healthPrefab.Instantiate<RigidBody3D>();
        gold.Position = globalPosition;
        GetTree().Root.AddChild(gold, true);
    }

    private void SpawnGold(Vector3 globalPosition)
    {
        RigidBody3D gold = goldPrefab.Instantiate<RigidBody3D>();
        gold.Position = globalPosition;
        GetTree().Root.AddChild(gold, true);
    }



    public static Material TeamMaterial(TEAM team)
    {
        if (team == TEAM.LEFT) { return TeamLeft; }
        if (team == TEAM.RIGHT) { return TeamRight; }
        return null;
    }

    internal static void BuildingDied(BuildingBaseClass building)
    {
        GD.Print($"Core::BuildingDied() path[{building.GetPath()}]");

        int roll = GD.RandRange(0, 100);
        if (roll < 40)
        {
            ins.SpawnGold(building.GlobalPosition);
        }
        else if (roll < 60)
        {
            ins.SpawnHealth(building.GlobalPosition);
        }
        building.QueueFree();
    }
    internal static void UnitDied(UnitBaseClass unit)
    {
        if (!ins.Multiplayer.IsServer()) { return; }
        int roll = GD.RandRange(0, 100);
        if (roll < 40)
        {
            PickupSpawner.SpawnThisPickup(new PickupSpawner.SpawnPickupArgument(TEAM.NONE, PickupSpawner.PICKUPTYPE.GOLD, unit.GlobalRotation, unit.GlobalPosition));
        }
        else if (roll < 60)
        {
            PickupSpawner.SpawnThisPickup(new PickupSpawner.SpawnPickupArgument(TEAM.NONE, PickupSpawner.PICKUPTYPE.HEALTH, unit.GlobalRotation, unit.GlobalPosition));
        }
        unit.QueueFree();
    }

    #region Connection things
    public static string AddressAndPortToString(System.Net.IPAddress anIPAddress, int port)
    {
        Debug.Assert(port <= UInt16.MaxValue && port >= 0);
        int portSize = sizeof(UInt16); // it's '2' but whatever
        byte[] ipBytes = anIPAddress.GetAddressBytes();
        byte[] addressAndPortBytes = new byte[portSize + ipBytes.Length];
        Array.Copy(ipBytes, 0, addressAndPortBytes, portSize, ipBytes.Length);
        ipBytes = BitConverter.GetBytes((UInt16)(port & UInt16.MaxValue));
        Array.Copy(ipBytes, addressAndPortBytes, portSize);
        string encoded = Convert.ToBase64String(addressAndPortBytes);
        return encoded.TrimEnd('=');
    }
    public static bool ValidateLobbyKey(string key)
    {
        return StringToAddressAndPort(key, out System.Net.IPAddress anIPAddress, out int port);
    }
    public static bool StringToAddressAndPort(string encoded, out System.Net.IPAddress anIPAddress, out int port)
    {
        anIPAddress = null;
        port = -1;
        byte[] addressAndPortBytes = new byte[1];
        try
        {
            addressAndPortBytes = Convert.FromBase64String(encoded);
        }
        catch (Exception exception)
        {
            MLog.LogError($"Core::StringToAddressAndPort() FAILED conversion [{exception}]");
        }
        if (addressAndPortBytes.Length < 2) { return false; }
        port = BitConverter.ToUInt16(addressAndPortBytes, 0);
        byte[] anotherArray = new byte[addressAndPortBytes.Length - sizeof(UInt16)];
        Array.Copy(addressAndPortBytes, sizeof(UInt16), anotherArray, 0, anotherArray.Length);
        try
        {
            anIPAddress = new System.Net.IPAddress(anotherArray);
        }
        catch (Exception)
        {
            anIPAddress = null;
        }
        return !(anIPAddress is null);
    }


    #endregion
}// EOF CLASS
