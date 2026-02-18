using System;
using System.Text.Json.Serialization;

namespace MLobby;

public class HostInfo
{
    [JsonInclude]
    public readonly string name;
    [JsonInclude]
    public readonly string key;
    [JsonInclude]
    public readonly int maxPlayers;
    [JsonInclude]
    public bool isFavourite = false;

    public HostInfo(string name, string key, int maxPlayers)
    {
        this.name = name; this.key = key; this.maxPlayers = maxPlayers;
    }
}// EOF CLASS