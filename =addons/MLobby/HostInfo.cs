using System;
using System.Text.Json.Serialization;

namespace MLobby;

public class HostInfo
{
    [JsonInclude]
    public readonly string name;
    [JsonInclude]
    public readonly string address;
    [JsonInclude]
    public readonly int port;
      [JsonInclude]
    public readonly int maxPlayers;
    [JsonInclude]
    public bool isFavourite = false;

    public HostInfo(string name, string address, int port, int maxPlayers)
    {
        this.name = name; this.address = address; this.port = port; this.maxPlayers = maxPlayers;
    }
}// EOF CLASS