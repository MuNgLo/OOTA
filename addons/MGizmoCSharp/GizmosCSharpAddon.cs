#if TOOLS
using Godot;

namespace MGizmosCSharp;
[Tool]
public partial class GizmosCSharpAddon : EditorPlugin
{
    public override void _EnterTree()
    {
        GD.Print("Loaded MGizmosCSharp Plugin : Gizmo nodes can now be added through the add child node menu. For pure runtime code, the addon don't have to be loaded.");
    }
    public override void _ExitTree()
    {
        GD.Print("Unloaded GizmosCSharp Plugin");
    }

    public override bool _HasMainScreen()
    {
        return false;
    }
    public override string _GetPluginName()
    {
        return "MGizmosCSharp";
    }
}// EOF CLASS
#endif