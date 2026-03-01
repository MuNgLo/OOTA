
namespace OOTA.Enums;

public enum GAMESTATE { NONE, SETUP, PLAYING, POST }

public enum TOWERTYPE { NONE, FOUNDATION, ATTACK };

public enum TEAM { NONE, LEFT, RIGHT }


public enum PLAYERSTATE { NONE, LOBBY, SPECTATING, ALIVE, DEAD }
public enum PLAYERMODE { NONE, ATTACKING, BUILDING }


/// <summary>
/// None: Not doing anything<br/>
/// Traveling: Moving along a path<br/>
/// Hunting: Moving relative to target
/// </summary>
public enum MINDSTATE { NONE, TRAVELING, HUNTING }
public enum PATHSTATE { IDLE, PENDING, EXECUTING, FINISHED }