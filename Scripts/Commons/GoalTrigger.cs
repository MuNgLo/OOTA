using Godot;
using OOTA;
using OOTA.Buildings;
using OOTA.Enums;
using OOTA.Units;
using System;

public partial class GoalTrigger : Area3D
{
    [Export] protected TEAM team = TEAM.NONE;
    public TEAM Team { get => team; set => SetTeam(value); }

    private void SetTeam(TEAM value)
    {
        CollisionMask = value == TEAM.RIGHT ? Core.Rules.leftTeamCollision : Core.Rules.rightTeamCollision;
        team = value;
    }

    public override void _Ready()
    {
        if (Multiplayer.IsServer())
        {
            Team = GetParent<BuildingBaseClass>().Team;
            BodyEntered += WhenBodyEntered;
        }
    }

    private void WhenBodyEntered(Node3D body)
    {
        //GD.Print($"GoalTrigger: Body entered {body.Name} Trigger Team: {team}   Body Team: {(body is UnitBaseClass unit2 ? unit2.Team.ToString() : "N/A")}");
        if (body is UnitBaseClass unit && unit.Team != team)
        {
            Core.Rules.UnitReachedGoal(unit);
        }
    }
}// EOF CLASS
