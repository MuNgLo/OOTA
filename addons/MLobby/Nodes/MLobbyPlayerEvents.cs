using System;

namespace MLobby;

public static class MLobbyPlayerEvents
{
    /// <summary>
    /// Local game instance player gold changed
    /// </summary>
    public static event EventHandler<int> OnGoldAmountChanged;
    /// <summary>
    /// When anything changes in the player collection this fires
    /// </summary>
    public static event EventHandler OnPlayersChanged;

    internal static void RaiseOnPlayersChanged()
    {
        EventHandler raiseEvent = OnPlayersChanged;
        if (raiseEvent != null)
        {
            raiseEvent(null, null);
        }
    }
    internal static void RaiseOnGoldChanged(int amount)
    {
        EventHandler<int> raiseEvent = OnGoldAmountChanged;
        if (raiseEvent != null)
        {
            raiseEvent(null, amount);
        }
    }

}// EOF CLASS