using Godot;
using OOTA.Buildings;
using OOTA.Units;
using System;

namespace OOTA.HUD;

public partial class HUDLabelMaker : Node3D
{
    [Export] UnitBaseClass unit;
    [Export] BuildingBaseClass building;
    [Export] PlayerAvatar avatar;

    Action workMethod;

    public override void _Ready()
    {
        if(unit is not null)
        {
            workMethod = UpdateUnit;
        }else if (building is not null)
        {
            workMethod = UpdateBuilding;
        }else if (avatar is not null)
        {
            workMethod = UpdateAvatar;
        }
        else
        {
            ProcessMode = ProcessModeEnum.Disabled;
        }
    }
    private void UpdateAvatar()
    {
        HUDWorldElements.ShowPlayerSign(GetInstanceId(), GetGlobalTransformInterpolated().Origin,  avatar.player.PlayerName, avatar.player.NormalizedHealth);
    }


    private void UpdateUnit()
    {
        if (unit.NormalizedHealth < 1.0f)
        {
            HUDWorldElements.ShowHealthBar(GetInstanceId(), GetGlobalTransformInterpolated().Origin, unit.NormalizedHealth);
        }
        else
        {
            HUDWorldElements.RemoveElement(GetInstanceId());
        }
    }

    private void UpdateBuilding()
    {
        if (building.NormalizedHealth < 1.0f)
        {
            HUDWorldElements.ShowHealthBar(GetInstanceId(), GetGlobalTransformInterpolated().Origin, building.NormalizedHealth);
        }
        else
        {
            HUDWorldElements.RemoveElement(GetInstanceId());
        }
    }

    public override void _Process(double delta)
    {
        workMethod.Invoke();
        /*
        if(unit is not null)
        {
            UpdateUnit();
        }else if (building is not null)
        {
            UpdateBuilding();
        }
        */
    }
    public override void _ExitTree()
    {
        HUDWorldElements.RemoveElement(GetInstanceId());
    }
}// EOF CLASS
