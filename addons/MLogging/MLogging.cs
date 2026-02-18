using System;
using Godot;

namespace MLogging;

public static class MLog
{
    public static EventHandler<string[]> OnLogMessagePushed;

    /// <summary>
    /// Prints message to console<br>
    /// if toOutput is true it also prints to Godot editor Output
    /// </summary>
    /// <param name="message"></param>
    /// <param name="toOutput"></param>
    public static void Log(string message, bool toOutput = false)
    {
        message = "[color=green](L)[/color]" + message;
        EventHandler<string[]> logEvt = OnLogMessagePushed;
        logEvt?.Invoke(null, [message]);
        if (toOutput)
        {
            GD.PrintRich(message);
        }
    }
    /// <summary>
    /// Prints an error to console<br>
    /// Defaults to also put it in Godot editor Output
    /// </summary>
    /// <param name="message"></param>
    /// <param name="toOutput"></param>
    public static void LogError(string message, bool toOutput = true)
    {
        message = "[color=red](E)[/color]" + message;
        EventHandler<string[]> logEvt = OnLogMessagePushed;
        logEvt?.Invoke(null, [message]);
        if (toOutput)
        {
            GD.PrintRich(message);
        }
    }
    public static void LogInfo(string message, bool toOutput = true)
    {
        message = "[color=yellow](i)[/color]" + message;
        EventHandler<string[]> logEvt = OnLogMessagePushed;
        logEvt?.Invoke(null, [message]);
        if (toOutput)
        {
            GD.PrintRich(message);
        }
    }
    public static void LogWarning(string message, bool toOutput = true)
    {
        message = "[color=orange](W)[/color]" + message;
        EventHandler<string[]> logEvt = OnLogMessagePushed;
        logEvt?.Invoke(null, [message]);
        if (toOutput)
        {
            GD.PrintRich(message);
        }
    }
}// EOF CLASS