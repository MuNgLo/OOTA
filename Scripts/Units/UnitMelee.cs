using Godot;
using System;
using System.Threading.Tasks;

namespace OOTA.Units;

[GlobalClass]
public partial class UnitMelee : UnitBaseClass
{
    [Export] ulong attackDurationMS = 150;

    MeshInstance3D attackIndicatorMesh;
    ulong tsLastAttack;

    public override void _EnterTree()
    {
        if (Multiplayer.IsServer())
        {
            attackIndicatorMesh = GetNode<MeshInstance3D>("MeshInstance3D2");
        }
    }

    public override void _Process(double delta)
    {
        if (Multiplayer.IsServer())
        {
            if (attackIndicatorMesh.Visible && Time.GetTicksMsec() > tsLastAttack + attackDurationMS)
            {
                attackIndicatorMesh.Hide();
            }
        }
    }
    public override void AttackTarget()
    {
        tsLastAttack = Time.GetTicksMsec();
        attackIndicatorMesh.Show();
        base.AttackTarget();
    }
}// EOF CLASS
