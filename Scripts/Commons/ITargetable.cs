using Godot;

public interface ITargetable
{
    public TEAM Team {get; set;}
    public Vector3 GlobalPosition { get; }
    public Node3D Body { get; }


    public void AddSupporter(ISupporter supporter);
    public void RemoveSupporter(ISupporter supporter);

    public void TakeDamage(int amount);


}// EOF INTERFACE