using Godot;

namespace MMenuSystem;
[GlobalClass]
public partial class UIElementPager : Node {
    [Export] private Control[] pages;

    public override void _Ready()
    {
        GoToPage(0);
    }
    /// <summary>
    /// -1 hides all pages
    /// </summary>
    /// <param name="idx"></param>
    public void GoToPage(int idx)
    {
        if (idx < 0)
        {
            for (int i = 0; i < pages.Length; i++)
            {
                pages[i].Visible = false;
            }
            return;
        }
        if (idx >= 0 && idx < pages.Length)
        {
            for (int i = 0; i < pages.Length; i++)
            {
                pages[i].Visible = i == idx ? true : false;
            }
        }
    }
}// EOF CLASS