

namespace MMenuSystem;

public interface IHUDElement
{
    bool IsAnimated { get; }
    void MoveOffScreen();
    void MoveOnScreen();
    void Toggle();
}// EOF Interface