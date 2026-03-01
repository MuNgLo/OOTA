using System.Collections.Generic;
using Godot;
using OOTA.Enums;
using OOTA.Units;
namespace OOTA.Interfaces;

public interface ITargetable
{
    public TEAM Team { get; set; }
    public Vector3 GlobalPosition { get; }
    public Node3D Body { get; }

    public float Health { get; set; }
    public float MaxHealth { get; set; }
    public float CurrentSpeed { get; }

    public bool CanTakeDamage { get; set; }
    public bool IsSupported => HasSupporters();
    public bool CanBeSupported { get; }

    public void AddSupporter(ISupporter supporter);
    public void RemoveSupporter(ISupporter supporter);
    public List<ISupporter> Supporters { get; }


    public void Die();

    public bool HasSupporters() { return Supporters.Count > 0; }

    /// <summary>
    /// Sets Health to the specified amount, and checks for death.<br/>
    /// This is the base method for changing health, and should be used by all other methods that change health.<br/>
    /// This is to ensure that death is properly handled, and that any future changes to health handling only need to be made in one place.
    /// </summary>
    /// <param name="amount"></param>
    public virtual void SetHealth(float amount)
    {
        Health = Mathf.Clamp(amount, 0, MaxHealth);
        if (Health <= 0.0f) { Die(); }
    }

    public virtual void AddHealth(float amount)
    {
        if (amount < 1) { return; }
        SetHealth(Health + amount);
    }
    public virtual void TakeDamage(float amount)
    {
        if (amount <= 0.0f || !CanTakeDamage) { return; }
        SetHealth(Health - amount);
    }
}// EOF INTERFACE