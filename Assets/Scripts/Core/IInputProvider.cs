using UnityEngine;

namespace HorrorTech.Core
{
    public interface IInputProvider
    {
        Vector2 GetLookDelta();
        Vector2 GetMoveAxis();
        bool GetJumpDown();
        bool GetSprintHeld();
        bool GetCrouchHeld();
        bool GetInteractDown();
        bool GetCaptureDown();
    }
}