
using System;

public interface ISupporter
{
    public ulong GetInstanceId();
    public event Action TreeExiting;
    /// <summary>
    /// Returns extra damage gained based on the base damage value
    /// </summary>
    /// <param name="originalDamage"></param>
    /// <returns></returns>
    public int BaseDamageBonus(int originalDamage);
    /// <summary>
    /// Returns how much the buffed should increase in scale
    /// </summary>
    /// <returns></returns>
    public float BaseScaleBonus();
    /// <summary>
    /// Returns how much extra health the unit will have while buffed<br/>
    /// Based on the unit's max health
    /// </summary>
    /// <param name="currentMaxHealth"></param>
    /// <returns></returns>
    public int ExtraHealth(int currentMaxHealth);
}// EOF INTERFACE