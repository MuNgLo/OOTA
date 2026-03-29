using System;

namespace MSettings;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class VolumeAttribute : Attribute
{
    public float Max;
    public float Min;

    public float Range;

    public VolumeAttribute(float max = 6.0f, float min = -80.0f)
    {
        Max = max; Min = min;
        Range = max - min;
        if(max > 0 && min < 0 && Range < 0){ Range = MathF.Abs(Range) + max + max; }
    }
}
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class IsCheckBox : Attribute{ }
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class IsKeyBind : Attribute{ }
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class GodotAction : Attribute{
    public string actionName;
    public GodotAction(string name){actionName = name;}
 }
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class RangeAttribute : Attribute
{
    public float Max;
    public float Min;
    /// <summary>
    /// Define the min and max value for the field
    /// </summary>
    /// <param name="min"></param>
    /// <param name="max"></param>
    public RangeAttribute(float min = 0.0f, float max = 1000.0f)
    {
        Max = max; Min = min;
    }
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class SecretAttribute : Attribute
{
    public SecretAttribute()
    {
    }
}
[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class Encrypt : Attribute
{
    public Encrypt()
    {
    }
}
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class Tooltip : Attribute
{
    public string Text;
    public Tooltip(string tip)
    {
        Text=tip;
    }
}
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
public class MenuLabel : Attribute
{
    public string Text;
    public MenuLabel(string text)
    {
        Text=text;
    }
}

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
public class EditableValue : Attribute
{
    public bool isEditable;
    public EditableValue(bool v)
    {
        isEditable=v;
    }
}