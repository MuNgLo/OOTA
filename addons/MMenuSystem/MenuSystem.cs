using System;
using System.Collections.Generic;
using Godot;
namespace MMenuSystem;

[GlobalClass]
public partial class MenuSystem : CanvasLayer
{
    private static MenuSystem Instance;

    /// <summary>
    /// Any time the menu is open this will also be active
    /// </summary>
    [Export] private CanvasLayer persistentOnTop;

    /// <summary>
    /// Raised as menu is opened or closed
    /// </summary>
    public static event EventHandler<bool> OnMenuVisibilityChanged;
    private bool keepCursor = true;
    private CanvasItem startMenu;

    private bool HasPreviousMenu { get => lastMenu != null; }
    private Dictionary<string, CanvasItem> menus = new Dictionary<string, CanvasItem>();
    private CanvasItem lastMenu = null, currentMenu = null;
    /// <summary>
    /// Returns true if menu is showing
    /// </summary>
    static public bool IsUp => Instance.currentMenu != null ? Instance.currentMenu.Visible : false;
    public override void _Ready()
    {
        Instance = this;
        if (GetChildCount() > 0)
        {
            foreach (Node item in GetChildren())
            {
                if (item is CanvasItem)
                {
                    if (menus.ContainsKey(item.Name))
                    {
                        GD.Print($"Menu system: Trying to register already registered menu {item.Name}");
                        return;
                    }
                    menus[item.Name] = item as CanvasItem;
                }
            }
            // If starmenu wasnt assigned pick the first child menu
            startMenu = GetChild(0) as CanvasItem;
        }
        HideAllMenus();
        lastMenu = null;
        currentMenu = startMenu;
        currentMenu.Show();
        if (persistentOnTop is not null) { persistentOnTop.Show(); }
    }
    public static void GoToMenu(string menuName)
    {
        if (!Instance.menus.ContainsKey(menuName))
        {
            return;
        }
        Instance.HideAllMenus();
        Instance.menus[menuName].Show();
        Instance.lastMenu = Instance.currentMenu;
        Instance.currentMenu = Instance.menus[menuName];
        if (Instance.persistentOnTop is not null) { Instance.persistentOnTop.Show(); }

    }
    public static void GoToPreviousMenu()
    {
        if (Instance.lastMenu == null)
        {
            return;
        }
        if (Instance.lastMenu == Instance.currentMenu)
        {
            GD.Print("GoToPreviousMenu:: Last Menu and Current are same!");
            return;
        }
        Instance.HideAllMenus();
        CanvasItem shuffle = Instance.currentMenu;
        Instance.currentMenu = Instance.lastMenu;
        Instance.lastMenu = shuffle;
        Instance.currentMenu.Show();
        if (Instance.persistentOnTop is not null) { Instance.persistentOnTop.Show(); }

    }

    public static void HideMenu(bool keepCursorVisible = false)
    {
        if (Instance is null) { return; }
        Instance.keepCursor = keepCursorVisible;
        SetCursorState();
        Instance.HideAllMenus();
        OnMenuVisibilityChanged?.Invoke(Instance, false);
    }

    public static void ShowMenu()
    {
        if (!Instance.currentMenu.Visible)
        {
            OnMenuVisibilityChanged?.Invoke(Instance, !Instance.currentMenu.Visible);
        }

        Instance.HideAllMenus();
        Instance.currentMenu.Show();
        if (Instance.persistentOnTop is not null) { Instance.persistentOnTop.Show(); }

        Input.MouseMode = Input.MouseModeEnum.Visible;
    }
    /// <summary>
    /// Returns the active state of current menu after it was toggled
    /// </summary>
    /// <returns></returns>
    public static bool ToggleMenu()
    {
        if (Instance is null)
        {
            if (Input.MouseMode == Input.MouseModeEnum.Captured)
            {
                Input.MouseMode = Input.MouseModeEnum.Visible;
            }
            else
            {
                Input.MouseMode = Input.MouseModeEnum.Captured;
            }
            return false;
        }
        if (Instance.currentMenu.Visible)
        {
            HideMenu();
        }
        else
        {
            ShowMenu();
            if (Instance.persistentOnTop is not null)
            {
                Instance.persistentOnTop.Show();
            }
        }
        return Instance.currentMenu.Visible;
    }
    /// <summary>
    /// Hides all registered menus
    /// </summary>
    private void HideAllMenus()
    {
        foreach (string key in menus.Keys)
        {
            menus[key].Hide();
        }
    }

    public static CanvasItem GetMenuGameNode(string name)
    {
        if (Instance.menus.ContainsKey(name))
        {
            return Instance.menus[name];
        }
        return null;
    }

    internal static void GoHUD()
    {
        Instance.HideAllMenus();
    }
    /// <summary>
    /// This sets the cursor based on the keepCursor field
    /// </summary>
    private static void SetCursorState()
    {
        if (Instance.keepCursor)
        {
            Input.MouseMode = Input.MouseModeEnum.Visible;
        }
        else
        {
            Input.MouseMode = Input.MouseModeEnum.Captured;
        }
    }
}// EOF CLASS