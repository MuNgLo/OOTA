using Godot;
using System;

namespace OOTA.UI;

[GlobalClass]
public partial class UIMainMenuFlipper : GridContainer
{
    [Export] MENUSTATE allHiddenState = MENUSTATE.NONE;

    [Export] UIMainMenu menu;
    [ExportGroup("Group 1")]
    [Export] MENUSTATE groupOneState;
    [Export] Control[] groupOneElements;
    [ExportGroup("Group 2")]
    [Export] MENUSTATE groupTwoState;
    [Export] Control[] groupTwoElements;
    [ExportGroup("Group 3")]
    [Export] MENUSTATE groupThreeState;
    [Export] Control[] groupThreeElements;

    public override void _Ready()
    {
        menu.OnMenuStateChanged += whenMenuStateChange;
    }

    private void whenMenuStateChange(object sender, MENUSTATE newState)
    {
        if(newState == allHiddenState || allHiddenState == MENUSTATE.NONE && newState != groupOneState)
        {
            Hide(); 
            return;
        }else if(Visible == false)
        {
            Show();
        }
        if (newState == groupOneState)
        {
            ChangeElements(groupOneElements, true);
            ChangeElements(groupTwoElements, false);
            ChangeElements(groupThreeElements, false);
        }
        else if (newState == groupTwoState)
        {
            ChangeElements(groupOneElements, false);
            ChangeElements(groupTwoElements, true);
            ChangeElements(groupThreeElements, false);

        }
        else if (newState == groupThreeState)
        {
            ChangeElements(groupOneElements, false);
            ChangeElements(groupTwoElements, false);
            ChangeElements(groupThreeElements, true);

        }
        else
        {
            ChangeElements(groupOneElements, true);
            ChangeElements(groupTwoElements, false);
            ChangeElements(groupThreeElements, false);

        }
    }

    private void ChangeElements(Control[] elements, bool flagTo)
    {
        for (int i = 0; i < elements.Length; i++)
        {
            elements[i].Visible = flagTo;
        }
    }
}// EOF CLASS
