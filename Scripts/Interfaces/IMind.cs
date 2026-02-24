using Godot;
using OOTA.Enums;

namespace OOTA.Interfaces;

public interface IMind
{
    public TEAM Team {get; set;}
    public void BodyEnteredAggroRange(Node3D body);
    public void BodyExitedAggroRange(Node3D body);
}// EOF INTERFACE