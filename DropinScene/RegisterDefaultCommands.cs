using System;
using Godot;

namespace MConsole;

/// <summary>
/// A node to register in default commands
/// Each command is optional by bool flag
/// </summary>
[GlobalClass]
public partial class RegisterDefaultCommands : Node
{
    [Export] private bool quitExitapp = true;
    [Export] private bool maxfps = true;
    [Export] private bool showfps = true;
    [Export] private bool vsync = true;
    [Export] private bool wireframe = true;
    [Export] private bool occluders = true;
    [Export] private bool clear = true;

    public override void _Ready()
    {
        if (quitExitapp) { AddExitQuitCommand(); }
        if (maxfps) { AddMaxFPSCommand(); }
        if (showfps) { ShowFPSCommand(); }
        if (vsync) { VSyncCommand(); }
        if (wireframe) { WireframeCommand(); }
        if (occluders) { OccludersCommand(); }
        if (clear) { ClearCommand(); }
    }

    private void ClearCommand()
    {
          Command cmd = new Command("RegisterDefaultCommands") { Name = "clear", Tip = "Clear the console",
            act = (a) =>
            {
                GameConsole.ClearOutput();
                return string.Empty;
            } };
        ConsoleCommands.RegisterCommand(cmd);
    }

    private void WireframeCommand()
    {
        Command cmd = new Command("RegisterDefaultCommands") { Name = "r_wireframe", Tip = "turn wireframe on/off",
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
            } };
        ConsoleCommands.RegisterCommand(cmd);
    }

  private void OccludersCommand()
    {
        Command cmd = new Command("RegisterDefaultCommands") { Name = "r_occluders", Tip = "turn occlusion debug view on/off",
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
            } };
        ConsoleCommands.RegisterCommand(cmd);
    }



    private void VSyncCommand()
    {
        Command cmd = new Command("RegisterDefaultCommands") { Name = "r_vsync", Tip = "turn vertical sync on/off",
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
}// EOF CLASS

