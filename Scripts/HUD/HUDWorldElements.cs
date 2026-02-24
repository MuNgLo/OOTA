using Godot;
using System;
using System.Collections.Generic;

namespace OOTA.HUD;

public partial class HUDWorldElements : Control
{
    private static HUDWorldElements ins;
    Camera3D Camera => Core.Camera;
    [Export] PackedScene prefabHealthBar;

    [Export] Color fullHealth;
    [Export] Color noHealth;

    [Export] float maxDistance = 20.0f;

    List<HUDWorldLabel> labels;

    public override void _EnterTree()
    {
        ins = this;
    }
    public override void _Ready()
    {
        ClearElements();
    }

    public static void ShowPlayerSign(ulong id, Vector3 worldPosition, string playerName, double normalizedHealth)
    {
        ins.UpdateSign(id, worldPosition, playerName, normalizedHealth);
    }
    private void UpdateSign(ulong id, Vector3 worldPosition, string text, double normalizedHealth)
    {
        HUDWorldLabel lbl = GetLabel(id);
        //Vector2 offset = Vector2.Left * lbl.element.Size.X * 0.2f + Vector2.Up * 25;
        Vector2 offset = Vector2.Up * 25;
        if (lbl.element.IsInsideTree())
        {
            lbl.element.Position = Camera.UnprojectPosition(worldPosition) + offset;
            lbl.Text = text;
            if (
            Camera.GlobalPosition.DistanceTo(worldPosition) > maxDistance
            ||
            Camera.GlobalTransform.Basis.Z.Dot(Camera.GlobalPosition.DirectionTo(worldPosition)) > -0.75f
            )
            {
                RemoveChild(lbl.element);
            }
        }
        else
        {
            if (
               Camera.GlobalPosition.DistanceTo(worldPosition) < maxDistance
               &&
               Camera.GlobalTransform.Basis.Z.Dot(Camera.GlobalPosition.DirectionTo(worldPosition)) < -0.75f
               )
            {
                AddChild(lbl.element);
                lbl.element.Position = Camera.UnprojectPosition(worldPosition) + offset;
                lbl.Text = text;
            }
        }

    }





    public static void ShowHealthBar(ulong id, Vector3 worldPosition, double normalizedHealth)
    {
        ins.UpdateBar(id, worldPosition, normalizedHealth);
    }
    private HUDWorldLabel GetBar(ulong id, double normalizedHealth)
    {
        if (!labels.Exists(p => p.id == id))
        {
            ProgressBar newBar = ins.prefabHealthBar.Instantiate<ProgressBar>();

            HUDWorldLabel lbl = new HUDWorldLabel(id, newBar, normalizedHealth);
            labels.Add(lbl);
        }
        return labels.Find(p => p.id == id);
    }


    private void UpdateBar(ulong id, Vector3 worldPosition, double normalizedHealth)
    {
        HUDWorldLabel lbl = GetBar(id, normalizedHealth);
        if (lbl.element.IsInsideTree())
        {
            lbl.element.Position = Camera.UnprojectPosition(worldPosition) + Vector2.Left * (lbl.element.Size.X * 0.5f);
            lbl.Value = normalizedHealth;
            lbl.Color = noHealth.Lerp(fullHealth, (float)normalizedHealth);

            if (
            Camera.GlobalPosition.DistanceTo(worldPosition) > maxDistance
            ||
            Camera.GlobalTransform.Basis.Z.Dot(Camera.GlobalPosition.DirectionTo(worldPosition)) > -0.75f
            )
            {
                RemoveChild(lbl.element);
            }
        }
        else
        {
            if (
               Camera.GlobalPosition.DistanceTo(worldPosition) < maxDistance
               &&
               Camera.GlobalTransform.Basis.Z.Dot(Camera.GlobalPosition.DirectionTo(worldPosition)) < -0.75f
               )
            {
                AddChild(lbl.element);
                lbl.element.Position = Camera.UnprojectPosition(worldPosition);
                lbl.Value = normalizedHealth;
                lbl.Color = noHealth.Lerp(fullHealth, (float)normalizedHealth);
            }
        }
    }

    private HUDWorldLabel GetLabel(ulong id)
    {
        if (!labels.Exists(p => p.id == id))
        {
            labels.Add(new(id));
        }
        return labels.Find(p => p.id == id);
    }


    #region Done
    private void ClearElements()
    {
        foreach (Node child in GetChildren())
        {
            child.QueueFree();
        }
        labels = new List<HUDWorldLabel>();
    }
    internal static void RemoveElement(ulong id)
    {
        HUDWorldLabel lbl = ins.labels.Find(p => p.id == id);
        if (lbl is not null)
        {
            lbl.element.QueueFree();
        }
        ins.labels.RemoveAll(p => p.id == id);
    }
    #endregion
}// EOF CLASS
