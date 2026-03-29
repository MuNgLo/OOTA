using Godot;
using System;
using System.Net;
using System.Text.Json.Serialization;
using MSettings;

[System.Serializable]
public class GameConfigSettings
{
	#region VOIP Settings
	[JsonInclude]
	public bool voipEnabled = true;
	[JsonInclude]
	public bool voipMicEnabled = true;
	/// <summary>
	/// true = PTT, false = voice activation
	/// </summary>
	[JsonInclude] public bool voipMode = true;
	[JsonInclude] public bool voipDeNoise = true;


	#endregion
	#region Sound Settings
	[JsonInclude, MenuLabel("Voice Volume"), Tooltip("Other players voice volume"), Volume, EditableValue(true)]
	public float volumeVoices = 0.0f;
	[JsonInclude, MenuLabel("Master"), Tooltip("Changes the volume for the whole game"), Volume, EditableValue(true)]
	public float volumeMaster = 0.0f;
	[JsonInclude, MenuLabel("Sound Effects"), Tooltip("Changes the volume for sound effects"), Volume, EditableValue(true)]
	public float volumeSoundEffects = 0.0f;
	[JsonInclude, MenuLabel("Music"), Tooltip("Changes the volume for music"), Volume, EditableValue(true)]
	public float volumeMusic = 0.0f;
	[JsonInclude, MenuLabel("UI"), Tooltip("Changes the volume for user interface"), Volume, EditableValue(true)]
	public float volumeUI = 0.0f;

	[JsonInclude, MenuLabel("Mute on focus lost"), Tooltip("Mutes the game when its not in focus")]
	public bool muteOnFocusLoss = true;
	#endregion

	#region  Player settings
	[JsonInclude, MenuLabel("Player Name"), Tooltip("The name you will show up as in a game")]
	public string playerName = "Bob";
	#endregion

	#region Host settings
	[JsonInclude, Range(25000, 60000), MenuLabel("Port"), Tooltip("Network port the host will accept connection on. Make sure this is forwarded. Check the network Info."), EditableValue(true)]
	public int port = 27015;
	[JsonInclude, Range(2, 10), MenuLabel("Max Players"), Tooltip("Maximum number of players the host will allow"), EditableValue(true)]
	public int maxPlayers = 10;
	private string hostIP = "127.0.0.1";
	[JsonIgnore]
	public IPAddress HostIP { set => hostIP = value.ToString(); get => IPAddress.Parse(hostIP); }
	[JsonIgnore]
	public string HostIPString { set => hostIP = value; get => hostIP; }
	#endregion




	#region KeyBinds Settings
	/*
	[JsonInclude, IsKeyBind, MenuLabel("Forward"), GodotAction("Forward")]
    public PlayerKeyBind forward = new PlayerKeyBind() { Device = INPUTDEVICE.KEYBOARD, Key = Key.W, AltDevice = INPUTDEVICE.KEYBOARD, KeyAlt = Key.None };
    [JsonInclude, IsKeyBind, MenuLabel("Backwards"), GodotAction("Backward")]
    public PlayerKeyBind backward = new PlayerKeyBind() { Device = INPUTDEVICE.KEYBOARD, Key = Key.S, AltDevice = INPUTDEVICE.KEYBOARD, KeyAlt = Key.None };
    [JsonInclude, IsKeyBind, MenuLabel("Left"), GodotAction("Left")]
    public PlayerKeyBind left = new PlayerKeyBind() { Device = INPUTDEVICE.KEYBOARD, Key = Key.A, AltDevice = INPUTDEVICE.KEYBOARD, KeyAlt = Key.None };
    [JsonInclude, IsKeyBind, MenuLabel("Right"), GodotAction("Right")]
    public PlayerKeyBind right = new PlayerKeyBind() { Device = INPUTDEVICE.KEYBOARD, Key = Key.D, AltDevice = INPUTDEVICE.KEYBOARD, KeyAlt = Key.None };
    [JsonInclude, IsKeyBind, MenuLabel("Jump"), GodotAction("Jump")]
    public PlayerKeyBind jump = new PlayerKeyBind() { Device = INPUTDEVICE.KEYBOARD, Key = Key.Space, AltDevice = INPUTDEVICE.KEYBOARD, KeyAlt = Key.None };
  	*/
	#endregion

}// EOF CLASS
