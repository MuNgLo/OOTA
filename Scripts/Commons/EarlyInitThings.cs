using Godot;
using System.Threading.Tasks;
using System.Collections.Generic;
using MLogging;
using System;
using OOTA.GameLogic;

namespace OOTA;

[GlobalClass]
public partial class EarlyInitThings : Node
{
	[Export] bool debug;
	public override void _EnterTree()
	{
		// Initialize the game event system
		//Events.SetupEvents(GetParent().GetNode<NetworkEvents>("NetworkEvents"));
	}
	public override void _Ready()
	{
		ProcessCmdLineArgs(OS.GetCmdlineArgs());
	}

	private void ProcessCmdLineArgs(string[] args)
	{
		Dictionary<string, string> arguments = BreakArgsToDict(args);
		foreach (KeyValuePair<string, string> argument in arguments)
		{
			if (argument.Key == "SID")
			{
				if (int.TryParse(argument.Value, out int id))
				{
					if (id == 1) { continue; }
					//DelayedWhenOnSessionID(id - 1);
				}
			}
		}
		for (int i = 0; i < args.Length; i++)
		{
			if (args[i].Contains('=')) { continue; }
			args[i] = args[i].Replace("--", "");
			if (args[i].ToLower() == "autohost")
			{
				DelayedAutoHostLaunch(50);
			}
			if (args[i].ToLower() == "autojoin")
			{
				DelayedAutoJoinLaunch(250);
			}
			if (args[i].ToLower() == "ontop")
			{
				GetWindow().AlwaysOnTop = true;
			}
			if (args[i].ToLower() == "left")
			{
				SetLeftWindow();
			}
			if (args[i].ToLower() == "right")
			{
				SetRightWindow();
			}
		}
	}



	private Dictionary<string, string> BreakArgsToDict(string[] args)
	{
		Dictionary<string, string> result = new Dictionary<string, string>();
		foreach (string arg in args)
		{
			if (arg.Contains('='))
			{
				string[] split = arg.Split('=');
				result[split[0].Replace("--", "")] = split[1];
			}
		}
		return result;
	}

	private async void DelayedAutoHostLaunch(int delayMS = 500)
	{
		MLog.LogInfo($"EarlyInitThings::DelayedAutoHostLaunch() Launching host in {delayMS}ms");
		await Task.Delay(delayMS);
		//Events.Player.OnPlayerAdded += AutoHostAdded;
		UIAPICalls.HostStart();
	}
	private async void DelayedAutoJoinLaunch(int delayMS = 500)
	{
		MLog.LogInfo($"EarlyInitThings::DelayedAutoJoinLaunch() Joining host in {delayMS}ms");
		await Task.Delay(delayMS);
		//Events.Player.OnPlayerAdded += AutoHostAdded;
		if (System.Net.IPAddress.TryParse("127.0.0.1", out System.Net.IPAddress ip))
		{
			UIAPICalls.JoinHost(Core.AddressAndPortToString(ip, 27015));
		}
	}

	private void SetLeftWindow()
	{
		GetWindow().Size = new Vector2I(800, 400);
		GetWindow().Position = new Vector2I(1920 + 300, 0);
		GetWindow().Title = "Client";
	}
	private void SetRightWindow()
	{
		GetWindow().Size = new Vector2I(800, 400);
		GetWindow().Position = new Vector2I(1940 + 1520, 200);
		GetWindow().Title = "Host";
	}
}// EOF CLASS
