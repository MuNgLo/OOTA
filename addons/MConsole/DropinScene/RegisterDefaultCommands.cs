using System;
using System.Linq;
using Godot;

namespace MConsole;

/// <summary>
/// A node to register in default commands
/// Each command is optional by bool flag
/// </summary>
[GlobalClass]
public partial class RegisterDefaultCommands : Node
{
    [Export] private bool quitExitApp = true;
    [Export] private bool maxFPS = true;
    [Export] private bool showFPS = true;
    [Export] private bool vsync = true;
    [Export] private bool wireframe = true;
    [Export] private bool colliderDraw = true;
    [Export] private bool occluders = true;
    [Export] private bool clear = true;

    [ExportGroup("Debug build only")]
    [Export] private bool debugOrphans = true;


    public override void _Ready()
    {
        if (quitExitApp) { AddExitQuitCommand(); }
        if (maxFPS) { AddMaxFPSCommand(); }
        if (showFPS) { ShowFPSCommand(); }
        if (vsync) { VSyncCommand(); }
        if (wireframe) { WireframeCommand(); }
        if (colliderDraw) { DrawCollidersCommand(); }
        if (occluders) { OccludersCommand(); }
        if (clear) { ClearCommand(); }
#if TOOLS
        if (debugOrphans) { DebugOrphanCommand(); }
#endif
    }



    private void ClearCommand()
    {
        Command cmd = new Command("RegisterDefaultCommands")
        {
            Name = "clear",
            Tip = "Clear the console",
            act = (a) =>
            {
                GameConsole.ClearOutput();
                return string.Empty;
            }
        };
        ConsoleCommands.RegisterCommand(cmd);
    }

    private void DrawCollidersCommand()
    {
        Command cmd = new Command("RegisterDefaultCommands")
        {
            Name = "r_colliders",
            Tip = "toggle collider draw",
            act = (a) =>
            {
                if (!GetTree().DebugCollisionsHint)
                {
                    GetTree().DebugCollisionsHint = true;
                    return "Drawing colliders.";
                }
                else
                {
                    GetTree().DebugCollisionsHint = false;
                    return "Drawing colliders turned off.";
                }
            }
        };
        ConsoleCommands.RegisterCommand(cmd);
    }

    private void WireframeCommand()
    {
        Command cmd = new Command("RegisterDefaultCommands")
        {
            Name = "r_wireframe",
            Tip = "turn wireframe on/off",
            act = (a) =>
            {
                if (GetViewport().DebugDraw != Viewport.DebugDrawEnum.Wireframe)
                {
                    GetViewport().DebugDraw = Viewport.DebugDrawEnum.Wireframe;
                    return "Wireframe on.";
                }
                else
                {
                    GetViewport().DebugDraw = Viewport.DebugDrawEnum.Disabled;
                    return "Wireframe off.";
                }
            }
        };
        ConsoleCommands.RegisterCommand(cmd);
    }

    private void OccludersCommand()
    {
        Command cmd = new Command("RegisterDefaultCommands")
        {
            Name = "r_occluders",
            Tip = "turn occlusion debug view on/off",
            act = (a) =>
            {
                if (GetViewport().DebugDraw != Viewport.DebugDrawEnum.Occluders)
                {
                    GetViewport().DebugDraw = Viewport.DebugDrawEnum.Occluders;
                    return "Occlusion view on.";
                }
                else
                {
                    GetViewport().DebugDraw = Viewport.DebugDrawEnum.Disabled;
                    return "Occlusion view off.";
                }
            }
        };
        ConsoleCommands.RegisterCommand(cmd);
    }



    private void VSyncCommand()
    {
        Command cmd = new Command("RegisterDefaultCommands")
        {
            Name = "r_vsync",
            Tip = "turn vertical sync on/off",
            act = (a) =>
            {
                if (DisplayServer.WindowGetVsyncMode() == DisplayServer.VSyncMode.Disabled)
                {
                    DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Enabled);
                    return "VSync on.";
                }
                else
                {
                    DisplayServer.WindowSetVsyncMode(DisplayServer.VSyncMode.Disabled);
                    return "VSync off.";
                }

            } };
        ConsoleCommands.RegisterCommand(cmd);
    }

    private void ShowFPSCommand()
    {
        Command cmd = new Command("RegisterDefaultCommands") { Name = "showfps", Tip = "show/hide frames per second counter", act = (a) => { return ""; } };
        ConsoleCommands.RegisterCommand(cmd);
    }

    private void AddMaxFPSCommand()
    {
        Command cmd = new Command("RegisterDefaultCommands", -1)
        {
            Name = "maxfps",
            Tip = "Set maximum frames per second",
            Help = "Example: maxfps 60",
            act = (a) =>
            {
                if (a.Length == 1)
                {
                    return "Currently maxFPS is set to " + Engine.MaxFps.ToString();
                }

                SetMaxFPS(a[1]); return "";
            }
        };
        ConsoleCommands.RegisterCommand(cmd);
    }

    private void SetMaxFPS(string v)
    {
        if (int.TryParse(v, out int i)) { Engine.MaxFps = Math.Clamp(i, 0, 1000); }
    }

    private void AddExitQuitCommand()
    {
        Command cmd1 = new Command("RegisterDefaultCommands") { Name = "exit", Tip = "Closes application", act = (a) => { GetTree().Quit(); return ""; } };
        Command cmd2 = new Command("RegisterDefaultCommands") { Name = "quit", Tip = "Closes application", act = (a) => { GetTree().Quit(); return ""; } };
        ConsoleCommands.RegisterCommand(cmd1);
        ConsoleCommands.RegisterCommand(cmd2);
    }
#if TOOLS
    private void DebugOrphanCommand()
    {
        Command cmd = new Command("RegisterDefaultCommands")
        {
            Name = "debug_orphanReport",
            Tip = "outputs stats about orphaned nodes in current game instance",
            act = (a) =>
            {
                return DebugOrphanReport();
            }
        };
        ConsoleCommands.RegisterCommand(cmd);
    }

    private string DebugOrphanReport()
    {
        int[] ids = GetOrphanNodeIds().ToArray();
        Core.LogInfo($"[[[ ORPHAN REPORT ]]]  Total of [{ids.Length}] orphans.", true);
        int validCount = 0;
        for (int i = 0; i < ids.Length; i++)
        {
            if (IsInstanceIdValid((ulong)ids[i]))
            {
                validCount++;
            }
        }
        Core.LogInfo($"Of those [{validCount}] is valid instances.", true);

        for (int i = 0; i < ids.Length; i++)
        {
            GodotObject n = InstanceFromId((ulong)ids[i]);
            if(n is not null)
            {
                Core.LogInfo($"[{i}] Name[{n.GetType()}]", true);
            }
        }

        PrintOrphanNodes();


        return string.Empty;
    }
#endif
}// EOF CLASS

