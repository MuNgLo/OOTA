using Godot;
using MLobby;
using MLogging;
using OOTA.Enums;
using OOTA.GameLogic;
using System;
using System.Diagnostics;

namespace OOTA;

public partial class Core : Node
{
    private static Core ins;


    [Export] LobbyManager lobby;
    [Export] OOTAPlayerManager players;
    [Export] Rules rules;
    public static Rules Rules => ins.rules;
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

    



    public static Material TeamMaterial(TEAM team)
    {
        if (team == TEAM.LEFT) { return TeamLeft; }
        if (team == TEAM.RIGHT) { return TeamRight; }
        return null;
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
